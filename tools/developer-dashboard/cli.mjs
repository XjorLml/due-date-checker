#!/usr/bin/env node
// Developer dashboard CLI. Reads and records progress deterministically; it never
// calls an AI model and only runs the fixed test commands named in the config.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { spawn } from 'node:child_process';
import { parseArgs } from 'node:util';
import { defaultRoot, resolveInside, rel } from './lib/paths.mjs';
import { loadConfig } from './lib/config.mjs';
import { buildSnapshot, deriveFeatures, recordFiles } from './lib/snapshot.mjs';
import { brief, featureText } from './lib/format.mjs';
import { appendJsonl, readJsonl, validateVerification, validateReview, validateBlocker, validateSession, newId, now } from './lib/records.mjs';
import { changedFiles, headCommit } from './lib/git.mjs';
import { previewFingerprint } from './lib/fingerprint.mjs';
import { summarizeSessions } from './lib/progress.mjs';

const HELP = `Usage: node tools/developer-dashboard/cli.mjs <command> [options]

Viewing (no writes):
  status [--json]            Short deterministic progress summary (default)
  feature <id>               One feature: status, gates, open tasks, review
  check                      Validate config, mappings and records (exit 1 on problems)
  serve [--port N]           Start the local dashboard (http://127.0.0.1:4317)

Recording (append-only; never edits STATUS or specs):
  verify --feature ID --gate ID --result pass|fail|blocked|not-run
         --observer agent|user|historical --evidence PATH#anchor|https-URL
         --env "where it ran" [--date YYYY-MM-DD] [--files a,b] [--note TEXT]
         [--commit SHA|none]  (default: HEAD; none for historical records)
  review --feature ID --decision approved|changes-requested [--note TEXT]
  block --feature ID --reason TEXT        unblock --feature ID
  session start|checkpoint|end --agent NAME [--session ID] [--task TEXT]
         [--done TEXT]... [--pending TEXT]... [--blocker TEXT] [--verification TEXT]
  test <suite> | test --list   Run one configured suite and record its result
  hook <agent> [json]          Used by agent hooks; silent, always exits 0

Global: --root DIR (repository), --config FILE (dashboard config).`;

const options = {
  root: { type: 'string' }, config: { type: 'string' }, json: { type: 'boolean' }, brief: { type: 'boolean' },
  port: { type: 'string' }, host: { type: 'string' }, list: { type: 'boolean' }, help: { type: 'boolean', short: 'h' },
  feature: { type: 'string' }, gate: { type: 'string' }, result: { type: 'string' }, observer: { type: 'string' },
  evidence: { type: 'string' }, env: { type: 'string' }, date: { type: 'string' }, files: { type: 'string' }, note: { type: 'string' }, commit: { type: 'string' },
  decision: { type: 'string' }, reason: { type: 'string' }, agent: { type: 'string' }, session: { type: 'string' },
  task: { type: 'string' }, done: { type: 'string', multiple: true }, pending: { type: 'string', multiple: true },
  blocker: { type: 'string' }, verification: { type: 'string' },
};

function today() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function context(values) {
  const root = path.resolve(values.root ?? defaultRoot);
  const config = loadConfig(root, values.config ? path.resolve(values.config) : undefined);
  return { root, config, files: recordFiles(config) };
}

const exists = root => relative => { const f = resolveInside(root, relative); return Boolean(f && fs.existsSync(f)); };

async function sessionIdFor(ctx, agent, requested) {
  if (requested) return requested;
  const { items } = readJsonl(ctx.files.sessions);
  const open = summarizeSessions(items.filter(e => e.agent === agent), Date.now(), ctx.config.sessions?.staleAfterMinutes ?? 30).find(s => s.state === 'recent');
  return open?.sessionId ?? `${agent}-${today()}-${newId().slice(0, 8)}`;
}

async function runTest(ctx, suiteId) {
  const suite = ctx.config.testSuites?.[suiteId];
  if (!suite) throw Object.assign(new Error(`Unknown suite ${suiteId}. Suites: ${Object.keys(ctx.config.testSuites ?? {}).join(', ')}`), { validation: true });
  const [command, ...args] = suite.command;
  const executable = command === 'node' ? process.execPath : command;
  const startedAt = now();
  const started = Date.now();
  const tail = [];
  const keep = chunk => { process.stdout.write(chunk); for (const line of String(chunk).split(/\r?\n/)) if (line.trim()) { tail.push(line); if (tail.length > 40) tail.shift(); } };
  const exitCode = await new Promise(resolve => {
    const child = spawn(executable, args, { cwd: ctx.root, windowsHide: true, shell: false });
    child.stdout.on('data', keep);
    child.stderr.on('data', keep);
    child.on('error', error => { keep(`Could not start ${command}: ${error.message}\n`); resolve(null); });
    child.on('close', code => resolve(code));
  });
  const changed = await changedFiles(ctx.root);
  const record = {
    kind: 'test-run', id: newId(), suite: suiteId, label: suite.label, command: suite.command.join(' '),
    startedAt, finishedAt: now(), durationMs: Date.now() - started, exitCode,
    result: exitCode === 0 ? 'pass' : exitCode === null ? 'error' : 'fail',
    commit: await headCommit(ctx.root), dirtyFiles: changed?.length ?? null,
    environment: { node: process.version, platform: `${os.type()} ${os.release()}`, host: os.hostname() },
    outputTail: tail.join('\n').slice(-4000),
  };
  appendJsonl(ctx.files.tests, record);
  console.log(`\nRecorded ${suiteId}: ${record.result} (exit ${exitCode}) in ${Math.round(record.durationMs / 1000)} s.`);
  return exitCode === 0 ? 0 : 1;
}

/** Agent hook: record a lifecycle signal. Never prints, never fails the agent. */
async function hook(ctx, agent, argJson) {
  try {
    let input = {};
    const raw = argJson ?? (process.stdin.isTTY ? '' : await new Promise(resolve => {
      let data = '';
      process.stdin.setEncoding('utf8');
      process.stdin.on('data', c => { data += c; if (data.length > 1_000_000) process.stdin.destroy(); });
      process.stdin.on('end', () => resolve(data));
      process.stdin.on('error', () => resolve(data));
      setTimeout(() => resolve(data), 2000).unref();
    }));
    try { input = JSON.parse(raw || '{}'); } catch { input = {}; }
    // Only lifecycle facts are kept. Prompts, messages and transcript paths are ignored.
    const hookEvent = String(input.hook_event_name ?? input.type ?? 'signal').slice(0, 40);
    const event = /SessionStart/i.test(hookEvent) ? 'start' : /SessionEnd/i.test(hookEvent) ? 'end' : 'signal';
    const id = String(input.session_id ?? input['thread-id'] ?? input.thread_id ?? `${agent}-${today()}`).replace(/[^A-Za-z0-9._-]/g, '').slice(0, 80) || `${agent}-${today()}`;
    const changed = await changedFiles(ctx.root);
    appendJsonl(ctx.files.sessions, validateSession({ agent, sessionId: id, event, source: 'hook', hookEvent, changedFiles: changed?.map(c => c.path) }));
  } catch { /* a dashboard problem must never interrupt the agent */ }
  return 0;
}

async function main(argv) {
  const { values, positionals } = parseArgs({ args: argv, options, allowPositionals: true, strict: true });
  const [command = 'status', sub, extra] = positionals;
  if (values.help || command === 'help') { console.log(HELP); return 0; }
  if (command === 'hook') {
    try { return await hook(context(values), (sub ?? 'agent').toLowerCase().replace(/[^a-z0-9-]/g, '') || 'agent', extra); } catch { return 0; }
  }
  const ctx = context(values);

  if (command === 'serve') {
    const { startServer } = await import('./server.mjs');
    const server = await startServer({ root: ctx.root, config: ctx.config, port: values.port ? Number(values.port) : undefined, host: values.host });
    console.log(`Developer dashboard: ${server.url}  (Ctrl+C to stop)`);
    return new Promise(() => {});
  }
  if (command === 'status') {
    const snapshot = await buildSnapshot(ctx.config);
    console.log(values.json ? JSON.stringify(snapshot.overview, null, 2) : brief(snapshot));
    return 0;
  }
  if (command === 'feature') {
    const snapshot = await buildSnapshot(ctx.config);
    const f = snapshot.features.find(x => x.id === sub);
    if (!f) { console.error(`Unknown feature ${sub}. Try one of: ${snapshot.features.map(x => x.id).join(', ')}`); return 1; }
    console.log(values.json ? JSON.stringify(f, null, 2) : featureText(f));
    return 0;
  }
  if (command === 'check') {
    const snapshot = await buildSnapshot(ctx.config);
    for (const p of snapshot.problems) console.log(`PROBLEM ${p}`);
    const unmapped = snapshot.features.filter(f => f.phase === 'Unassigned').map(f => f.id);
    if (unmapped.length) console.log(`NOTE features without a mapping (shown as "Unassigned"): ${unmapped.join(', ')}`);
    console.log(`${snapshot.problems.length ? 'FAIL' : 'PASS'}: ${snapshot.features.length} features, ${snapshot.features.reduce((n, f) => n + f.gates.length, 0)} gates, ${snapshot.problems.length} problems.`);
    return snapshot.problems.length ? 1 : 0;
  }
  if (command === 'verify') {
    const features = deriveFeatures(ctx.config);
    const record = validateVerification({
      featureId: values.feature, gate: values.gate, result: values.result, observer: values.observer,
      evidence: values.evidence, environment: values.env, observedAt: values.date ?? today(), note: values.note,
      files: values.files ? values.files.split(',').map(s => s.trim()).filter(Boolean) : undefined,
      // A historical record describes an older artifact, so it gets no current commit unless given.
      commit: values.commit === 'none' ? undefined : values.commit ?? (values.observer === 'historical' ? undefined : await headCommit(ctx.root)),
    }, { features, exists: exists(ctx.root) });
    appendJsonl(ctx.files.verifications, record);
    console.log(`Recorded ${record.featureId} ${record.gate}: ${record.result} (${rel(ctx.root, ctx.files.verifications)}).`);
    return 0;
  }
  if (command === 'review') {
    const features = deriveFeatures(ctx.config);
    const record = validateReview({
      featureId: values.feature, decision: values.decision, note: values.note, reviewer: 'user',
      fingerprint: previewFingerprint(ctx.root, ctx.config.abs.previewRoots, ctx.config.preview.ignore ?? []),
    }, { features });
    appendJsonl(ctx.files.reviews, record);
    console.log(`Recorded visual review for ${record.featureId}: ${record.decision}. This does not verify behavior.`);
    return 0;
  }
  if (command === 'block' || command === 'unblock') {
    const record = validateBlocker({ featureId: values.feature, action: command, reason: values.reason }, { features: deriveFeatures(ctx.config) });
    appendJsonl(ctx.files.blockers, record);
    console.log(`Recorded ${command} for ${record.featureId}.`);
    return 0;
  }
  if (command === 'session') {
    if (!['start', 'checkpoint', 'end'].includes(sub)) { console.error('Use: session start|checkpoint|end --agent NAME'); return 1; }
    const agent = values.agent ?? 'agent';
    const changed = await changedFiles(ctx.root);
    const record = validateSession({
      agent, sessionId: await sessionIdFor(ctx, agent, values.session), event: sub, source: 'cli',
      task: values.task, done: values.done, pending: values.pending, blocker: values.blocker, verification: values.verification,
      changedFiles: changed?.map(c => c.path),
    });
    appendJsonl(ctx.files.sessions, record);
    console.log(`Recorded session ${sub} for ${agent} (${record.sessionId}).`);
    return 0;
  }
  if (command === 'test') {
    if (values.list || !sub) { for (const [id, s] of Object.entries(ctx.config.testSuites ?? {})) console.log(`${id.padEnd(20)} ${s.label}  [${s.command.join(' ')}]`); return 0; }
    return runTest(ctx, sub);
  }
  console.error(`Unknown command ${command}.\n\n${HELP}`);
  return 1;
}

main(process.argv.slice(2)).then(code => { if (typeof code === 'number') process.exitCode = code; }, error => {
  if (process.argv[2] === 'hook') { process.exitCode = 0; return; }
  console.error(error.validation ? `Not recorded: ${error.message}` : (error.stack ?? String(error)));
  process.exitCode = error.validation ? 2 : 1;
});
