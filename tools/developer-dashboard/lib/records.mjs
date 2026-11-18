// Append-only JSON Lines records. One writer at a time (lock file), one JSON
// object per line. A torn or malformed line is reported and skipped; earlier
// valid lines are never rewritten or deleted.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

export const RESULTS = ['pass', 'fail', 'blocked', 'not-run'];
export const OBSERVERS = ['agent', 'user', 'historical'];
export const DECISIONS = ['approved', 'changes-requested'];
const DATE = /^\d{4}-\d{2}-\d{2}(T[\d:.]+(Z|[+-]\d{2}:\d{2})?)?$/;
const SAFE_ID = /^[A-Za-z0-9._-]{1,80}$/;

export function readJsonl(file) {
  const items = [], problems = [];
  let text;
  try { text = fs.readFileSync(file, 'utf8'); } catch (error) {
    if (error.code !== 'ENOENT') problems.push({ file, line: 0, error: error.message });
    return { items, problems };
  }
  const lines = text.split('\n');
  lines.forEach((raw, index) => {
    const line = raw.trim();
    if (!line) return;
    try {
      const value = JSON.parse(line);
      if (value && typeof value === 'object' && !Array.isArray(value)) items.push(value);
      else problems.push({ file, line: index + 1, error: 'not a JSON object' });
    } catch {
      const torn = index === lines.length - 1 && !text.endsWith('\n');
      problems.push({ file, line: index + 1, error: torn ? 'incomplete last line (interrupted write)' : 'invalid JSON' });
    }
  });
  return { items, problems };
}

function sleep(ms) { Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, ms); }

/** Run `fn` while holding `<file>.lock`. Locks older than 10 s are treated as abandoned. */
export function withLock(file, fn) {
  const lock = file + '.lock';
  fs.mkdirSync(path.dirname(file), { recursive: true });
  for (let attempt = 0; ; attempt++) {
    try {
      const fd = fs.openSync(lock, 'wx');
      fs.writeSync(fd, String(process.pid));
      fs.closeSync(fd);
      break;
    } catch (error) {
      if (error.code !== 'EEXIST') throw error;
      try {
        if (Date.now() - fs.statSync(lock).mtimeMs > 10_000) { fs.rmSync(lock, { force: true }); continue; }
      } catch { continue; }
      if (attempt > 250) throw new Error(`Timed out waiting for ${path.basename(lock)}`);
      sleep(20);
    }
  }
  try { return fn(); } finally { fs.rmSync(lock, { force: true }); }
}

/** Append one validated object. Starts a new line if the file ends mid-line. */
export function appendJsonl(file, record) {
  const line = JSON.stringify(record);
  withLock(file, () => {
    let prefix = '';
    try {
      const size = fs.statSync(file).size;
      if (size > 0) {
        const fd = fs.openSync(file, 'r');
        const last = Buffer.alloc(1);
        fs.readSync(fd, last, 0, 1, size - 1);
        fs.closeSync(fd);
        if (last[0] !== 0x0a) prefix = '\n';
      }
    } catch (error) { if (error.code !== 'ENOENT') throw error; }
    fs.appendFileSync(file, prefix + line + '\n');
  });
  return record;
}

/** Write a whole file atomically: temp file in the same folder, then rename. */
export function writeFileAtomic(file, text) {
  const temp = `${file}.${process.pid}.${crypto.randomBytes(4).toString('hex')}.tmp`;
  fs.writeFileSync(temp, text);
  fs.renameSync(temp, file);
}

export function newId() { return crypto.randomUUID(); }
export function now() { return new Date().toISOString(); }

function fail(message) { const e = new Error(message); e.validation = true; throw e; }
function text(value, name, max, required = true) {
  if (value === undefined || value === null || value === '') { if (required) fail(`${name} is required`); return undefined; }
  if (typeof value !== 'string') fail(`${name} must be text`);
  const clean = value.replace(/[\u0000-\u0008\u000b-\u001f]/g, '').trim();
  if (clean.length > max) fail(`${name} is longer than ${max} characters`);
  return clean;
}
function oneOf(value, name, allowed) { if (!allowed.includes(value)) fail(`${name} must be one of: ${allowed.join(', ')}`); return value; }

/**
 * Validate a verification record against the current feature list.
 * `exists(path)` checks repository evidence paths.
 */
export function validateVerification(input, { features, exists }) {
  const feature = features.find(f => f.id === input.featureId) ?? fail(`unknown feature ${input.featureId}`);
  const gate = feature.gates.find(g => g.id === input.gate) ?? fail(`${input.gate} is not a gate of ${feature.id} (gates: ${feature.gates.map(g => g.id).join(', ') || 'none'})`);
  const evidence = text(input.evidence, 'evidence', 500);
  if (!/^https:\/\/\S+$/.test(evidence) && !exists(evidence.split('#')[0])) fail(`evidence ${evidence} is not an existing repository path or https URL`);
  const observedAt = text(input.observedAt, 'observedAt', 40);
  if (!DATE.test(observedAt)) fail('observedAt must be a date like 2026-11-10');
  const files = input.files === undefined ? undefined : (Array.isArray(input.files) ? input.files : fail('files must be a list'));
  return {
    kind: 'verification', id: input.id ?? newId(), featureId: feature.id, gate: gate.id,
    result: oneOf(input.result, 'result', RESULTS),
    observer: oneOf(input.observer, 'observer', OBSERVERS),
    observedAt, evidence,
    environment: text(input.environment, 'environment', 200),
    commit: input.commit && /^[0-9a-f]{7,40}$/.test(input.commit) ? input.commit : undefined,
    files: files?.map(f => text(f, 'file', 300)),
    note: text(input.note, 'note', 500, false),
    recordedAt: input.recordedAt ?? now(),
  };
}

export function validateReview(input, { features }) {
  const feature = features.find(f => f.id === input.featureId) ?? fail(`unknown feature ${input.featureId}`);
  return {
    kind: 'review', id: input.id ?? newId(), featureId: feature.id,
    decision: oneOf(input.decision, 'decision', DECISIONS),
    note: text(input.note, 'note', 1000, false),
    screen: text(input.screen, 'screen', 120, false),
    fingerprint: text(input.fingerprint, 'fingerprint', 80),
    reviewer: oneOf(input.reviewer ?? 'user', 'reviewer', ['user', 'agent']),
    recordedAt: input.recordedAt ?? now(),
  };
}

export function validateBlocker(input, { features }) {
  const feature = features.find(f => f.id === input.featureId) ?? fail(`unknown feature ${input.featureId}`);
  const action = oneOf(input.action, 'action', ['block', 'unblock']);
  return {
    kind: 'blocker', id: input.id ?? newId(), featureId: feature.id, action,
    reason: text(input.reason, 'reason', 300, action === 'block'),
    recordedAt: input.recordedAt ?? now(),
  };
}

export function validateSession(input) {
  const agent = text(input.agent, 'agent', 32);
  if (!/^[a-z0-9-]+$/.test(agent)) fail('agent must be lowercase letters, digits or hyphens');
  const sessionId = text(input.sessionId, 'sessionId', 80);
  if (!SAFE_ID.test(sessionId)) fail('sessionId has unsupported characters');
  const list = (value, name) => value === undefined ? undefined : (Array.isArray(value) ? value : [value]).map(v => text(v, name, 300));
  return {
    kind: 'session', id: input.id ?? newId(), agent, sessionId,
    event: oneOf(input.event, 'event', ['start', 'checkpoint', 'signal', 'end']),
    source: oneOf(input.source ?? 'cli', 'source', ['cli', 'hook']),
    hookEvent: text(input.hookEvent, 'hookEvent', 40, false),
    task: text(input.task, 'task', 300, false),
    done: list(input.done, 'done'),
    pending: list(input.pending, 'pending'),
    blocker: text(input.blocker, 'blocker', 300, false),
    verification: text(input.verification, 'verification', 300, false),
    changedFiles: Array.isArray(input.changedFiles) ? input.changedFiles.slice(0, 50) : undefined,
    at: input.at ?? now(),
  };
}
