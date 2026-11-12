// Builds the dashboard snapshot: derived features (PRD + specs), hand-kept
// mappings, append-only records, Git metadata and the prototype fingerprint.
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import { readText, rel } from './paths.mjs';
import { parseRequirements, parseSpec, parseTasks, firstBoldIn } from './markdown.mjs';
import { loadMappings, mappingProblems } from './config.mjs';
import { readJsonl } from './records.mjs';
import { gitInfo, lastCommitDates, filesChangedSince } from './git.mjs';
import { previewFingerprint } from './fingerprint.mjs';
import { STATUS_LABEL, latestBy, gateState, deriveStatus, summarize, summarizeSessions, latestTests } from './progress.mjs';

const parseCache = new Map(); // file -> { mtimeMs, size, value }

function cached(file, parse) {
  let stat;
  try { stat = fs.statSync(file); } catch { return null; }
  const hit = parseCache.get(file);
  if (hit && hit.mtimeMs === stat.mtimeMs && hit.size === stat.size) return hit.value;
  const value = parse(readText(file));
  parseCache.set(file, { mtimeMs: stat.mtimeMs, size: stat.size, value });
  return value;
}

export function recordFiles(config) {
  const r = config.abs.records, t = config.abs.runtime;
  return {
    verifications: path.join(r, 'verifications.jsonl'),
    reviews: path.join(r, 'reviews.jsonl'),
    blockers: path.join(r, 'blockers.jsonl'),
    sessions: path.join(t, 'sessions.jsonl'),
    tests: path.join(t, 'test-runs.jsonl'),
  };
}

/** Features derived from documents only (no records). Used by the CLI to validate input. */
export function deriveFeatures(config) {
  const root = config.abs.root;
  const prdRel = rel(root, config.abs.requirements);
  const features = [];
  for (const r of cached(config.abs.requirements, t => parseRequirements(t, config.requirements.section)) ?? []) {
    features.push({
      id: r.id, kind: 'requirement', name: r.text.length > 90 ? r.text.slice(0, 87) + '…' : r.text,
      description: r.text, slice: r.slice,
      docs: { requirement: { path: prdRel, line: r.line } },
      gates: [{ id: r.id, text: r.acceptance || r.text, doc: prdRel, line: r.line }],
      tasks: null, specStatus: null, folder: null,
    });
  }
  let dirs = [];
  try { dirs = fs.readdirSync(config.abs.specs, { withFileTypes: true }).filter(d => d.isDirectory()).map(d => d.name).sort(); } catch { /* no specs */ }
  for (const dir of dirs) {
    const folder = path.join(config.abs.specs, dir);
    const specFile = path.join(folder, 'spec.md');
    const spec = cached(specFile, parseSpec);
    if (!spec) continue;
    const tasksFile = path.join(folder, 'tasks.md');
    const tasks = cached(tasksFile, parseTasks);
    const planFile = path.join(folder, 'plan.md');
    const specRel = rel(root, specFile);
    features.push({
      id: dir, kind: 'spec', name: spec.title ?? dir, description: spec.status ?? tasks?.status ?? null,
      docs: {
        spec: { path: specRel, line: 1 },
        ...(fs.existsSync(planFile) ? { plan: { path: rel(root, planFile), line: 1 } } : {}),
        ...(tasks ? { tasks: { path: rel(root, tasksFile), line: 1 } } : {}),
      },
      gates: spec.scenarios.map(s => ({ id: s.id, text: s.text, doc: specRel, line: s.line, anchor: s.anchor })),
      tasks, specStatus: spec.status ?? tasks?.status ?? null, folder: rel(root, folder),
    });
  }
  return features;
}

export async function buildSnapshot(config, { nowMs = Date.now() } = {}) {
  const root = config.abs.root;
  const problems = [];
  const features = deriveFeatures(config);
  const known = new Set(features.map(f => f.id));

  let mappings;
  try { mappings = loadMappings(config); } catch (error) { problems.push(error.message); mappings = { phases: [], features: {} }; }
  for (const [id, entry] of Object.entries(mappings.features ?? {})) problems.push(...mappingProblems(id, entry, { config, known }));

  const files = recordFiles(config);
  const read = name => { const { items, problems: p } = readJsonl(files[name]); for (const x of p) problems.push(`${rel(root, x.file)}:${x.line}: ${x.error}`); return items; };
  const verifications = read('verifications'), reviews = read('reviews'), blockers = read('blockers');
  const sessionEvents = read('sessions'), testRuns = read('tests');

  const [git, dates] = await Promise.all([
    gitInfo(root),
    lastCommitDates(root, [rel(root, config.abs.specs), rel(root, config.abs.requirements)]),
  ]);
  const commits = [...new Set(verifications.map(v => v.commit).filter(Boolean))];
  const changedSince = new Map(await Promise.all(commits.map(async c => [c, await filesChangedSince(root, c)])));
  const changedPaths = new Set((git.changed ?? []).map(c => c.path));
  const fingerprint = previewFingerprint(root, config.abs.previewRoots, config.preview.ignore ?? []);

  const latestVerification = latestBy(verifications, v => `${v.featureId}|${v.gate}`);
  const latestReview = latestBy(reviews, r => r.featureId);
  const latestBlocker = latestBy(blockers, b => b.featureId);
  for (const v of verifications) {
    const f = features.find(x => x.id === v.featureId);
    if (!f) problems.push(`verification ${v.id}: unknown feature ${v.featureId}`);
    else if (!f.gates.some(g => g.id === v.gate)) problems.push(`verification ${v.id}: ${v.gate} is no longer a gate of ${v.featureId}`);
  }

  const phases = mappings.phases ?? [];
  for (const f of features) {
    const m = mappings.features?.[f.id] ?? {};
    f.phase = m.phase ?? 'Unassigned';
    f.scope = m.scope ?? 'proposed';
    f.relatedFiles = m.relatedFiles ?? [];
    f.prototype = m.prototype ? { file: config.preview.entry, view: 'explore', overlay: null, sample: null, ...m.prototype } : null;
    f.unavailable = f.prototype ? null : (m.unavailable ?? { kind: 'unmapped', note: 'No prototype screen is mapped to this feature yet.' });
    f.note = m.note ?? null;
    for (const g of f.gates) {
      const record = latestVerification.get(`${f.id}|${g.id}`);
      Object.assign(g, gateState(record, { changedSince, relatedFiles: f.relatedFiles }));
      g.record = record ? { result: record.result, observedAt: record.observedAt, observer: record.observer, evidence: record.evidence, environment: record.environment, commit: record.commit?.slice(0, 7) ?? null, note: record.note ?? null } : null;
    }
    const block = latestBlocker.get(f.id);
    f.blocker = block?.action === 'block' ? { reason: block.reason, since: block.recordedAt } : null;
    f.status = deriveStatus({ gates: f.gates, tasks: f.tasks, manual: m.status, blocker: f.blocker });
    f.status.label = STATUS_LABEL[f.status.value];
    const review = latestReview.get(f.id);
    f.review = review ? { decision: review.decision, note: review.note ?? null, screen: review.screen ?? null, at: review.recordedAt, stale: review.fingerprint !== fingerprint } : null;
    const owned = f.folder ? [...dates].filter(([p]) => p.startsWith(f.folder + '/')).map(([, d]) => d) : [dates.get(rel(root, config.abs.requirements))].filter(Boolean);
    f.lastCommitted = owned.sort().at(-1) ?? null;
    f.uncommitted = f.folder ? [...changedPaths].some(p => p.startsWith(f.folder + '/')) : changedPaths.has(rel(root, config.abs.requirements));
  }
  const order = name => { const i = phases.indexOf(name); return i === -1 ? phases.length : i; };
  features.sort((a, b) => order(a.phase) - order(b.phase) || (a.kind === b.kind ? 0 : a.kind === 'requirement' ? -1 : 1) || a.id.localeCompare(b.id));

  const byScope = {};
  for (const scope of [...new Set(features.map(f => f.scope))]) byScope[scope] = summarize(features.filter(f => f.scope === scope));
  const staleMinutes = config.sessions?.staleAfterMinutes ?? 30;
  let focus = null;
  try { focus = firstBoldIn(readText(config.abs.status), config.statusDocument.focusSection); } catch { /* optional */ }

  return {
    schemaVersion: 1,
    project: config.project,
    generatedAt: new Date(nowMs).toISOString(),
    root: root.split(path.sep).join('/'),
    focus: focus ? { text: focus, source: rel(root, config.abs.status) } : null,
    git,
    overview: {
      approved: byScope.approved ?? summarize([]),
      all: summarize(features),
      byScope,
      blockers: features.filter(f => f.blocker).map(f => ({ featureId: f.id, name: f.name, ...f.blocker })),
      recordsSince: verifications.map(v => v.recordedAt).sort()[0] ?? null,
    },
    phases: [...new Set([...phases, ...features.map(f => f.phase)])].filter(p => features.some(f => f.phase === p)),
    features,
    sessions: summarizeSessions(sessionEvents, nowMs, staleMinutes).slice(0, 12),
    sessionStaleMinutes: staleMinutes,
    tests: latestTests(testRuns),
    suites: Object.entries(config.testSuites ?? {}).map(([id, s]) => ({ id, label: s.label })),
    preview: { entry: config.preview.entry, fingerprint },
    problems,
    environment: { node: process.version, platform: `${os.type()} ${os.release()}` },
  };
}
