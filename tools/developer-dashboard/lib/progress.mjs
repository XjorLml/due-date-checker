// Pure, deterministic progress rules. Same inputs always give the same output;
// nothing here estimates, averages unlike things, or calls a model.

export const STATUS_LABEL = { backlog: 'Backlog', in_progress: 'In Progress', in_review: 'In Review', blocked: 'Blocked', verified: 'Verified' };
export const GATE_STATES = ['pass', 'fail', 'blocked', 'not-run', 'stale', 'none'];

const newer = (a, b) => `${a.observedAt ?? ''}|${a.recordedAt ?? ''}` > `${b.observedAt ?? ''}|${b.recordedAt ?? ''}`;

/** Latest record per key, by observedAt then recordedAt (or `at`). */
export function latestBy(records, key) {
  const map = new Map();
  for (const record of records) {
    const k = key(record);
    const current = map.get(k);
    const r = { ...record, observedAt: record.observedAt ?? record.recordedAt ?? record.at };
    if (!current || newer(r, current)) map.set(k, r);
  }
  return map;
}

function touches(changed, file) {
  const prefix = file.endsWith('/') ? file : file + '/';
  if (changed.has(file)) return true;
  for (const c of changed) if (c.startsWith(prefix)) return true;
  return false;
}

/**
 * Gate state from its latest verification record.
 * A pass becomes stale when files it covers changed after the recorded commit.
 */
export function gateState(record, { changedSince, relatedFiles = [] }) {
  if (!record) return { state: 'none', freshness: null };
  if (record.result !== 'pass') return { state: record.result, freshness: null };
  const files = [...(record.files ?? []), ...relatedFiles];
  if (!record.commit || files.length === 0) return { state: 'pass', freshness: 'untracked' };
  const changed = changedSince.get(record.commit);
  if (!changed) return { state: 'pass', freshness: 'unknown' };
  return files.some(f => touches(changed, f))
    ? { state: 'stale', freshness: 'changed' }
    : { state: 'pass', freshness: 'current' };
}

/** Workflow status. Precedence: blocker record, verified gates, manual seed, tasks. */
export function deriveStatus({ gates, tasks, manual, blocker }) {
  if (blocker) return { value: 'blocked', reason: blocker.reason, source: 'blocker record' };
  const pass = gates.filter(g => g.state === 'pass').length;
  if (gates.length > 0 && pass === gates.length) {
    const reason = gates.length === 1 ? 'Its gate has a current passing record.' : `All ${gates.length} gates have a current passing record.`;
    return { value: 'verified', reason, source: 'verification records' };
  }
  if (manual) return { value: manual.value, reason: manual.note ?? 'Set manually.', source: manual.source, seeded: manual.seeded };
  if (!tasks || tasks.total === 0) return { value: 'backlog', reason: 'No tasks recorded.', source: 'tasks' };
  if (tasks.checked === 0) return { value: 'backlog', reason: `0 of ${tasks.total} tasks checked.`, source: 'tasks' };
  if (tasks.open > 0) return { value: 'in_progress', reason: `${tasks.checked} of ${tasks.total} tasks checked.`, source: 'tasks' };
  return { value: 'in_review', reason: `All ${tasks.total} tasks checked; ${pass} of ${gates.length} gates have a passing record.`, source: 'tasks' };
}

const round1 = n => Math.round(n * 10) / 10;

export function summarize(features) {
  const byStatus = Object.fromEntries(Object.keys(STATUS_LABEL).map(k => [k, 0]));
  const gates = Object.fromEntries(GATE_STATES.map(k => [k, 0]));
  let gateTotal = 0, checked = 0, taskTotal = 0;
  for (const f of features) {
    byStatus[f.status.value]++;
    for (const g of f.gates) { gates[g.state]++; gateTotal++; }
    checked += f.tasks?.checked ?? 0;
    taskTotal += f.tasks?.total ?? 0;
  }
  return {
    features: features.length,
    byStatus,
    gates: { total: gateTotal, ...gates },
    tasks: { checked, total: taskTotal },
    percentVerified: gateTotal ? round1((gates.pass / gateTotal) * 100) : null,
    percentTasks: taskTotal ? round1((checked / taskTotal) * 100) : null,
  };
}

/**
 * Group session events into sessions. A recent signal is reported as
 * "last signal", never as proof that an agent is running.
 */
export function summarizeSessions(events, nowMs, staleMinutes) {
  const sessions = new Map();
  const sorted = [...events].sort((a, b) => String(a.at).localeCompare(String(b.at)));
  for (const e of sorted) {
    const key = `${e.agent}|${e.sessionId}`;
    let s = sessions.get(key);
    if (!s) {
      s = { agent: e.agent, sessionId: e.sessionId, startedAt: e.at, lastAt: e.at, ended: false, task: null, done: [], pending: [], blockers: [], verification: [], changedFiles: [], sources: [], signals: 0 };
      sessions.set(key, s);
    }
    s.lastAt = e.at;
    s.lastEvent = e.event;
    s.lastHookEvent = e.hookEvent ?? s.lastHookEvent;
    if (!s.sources.includes(e.source)) s.sources.push(e.source);
    if (e.event === 'start') s.ended = false;
    if (e.event === 'end') s.ended = true;
    if (e.event === 'signal') s.signals++;
    if (e.task) s.task = e.task;
    for (const d of e.done ?? []) if (!s.done.includes(d)) s.done.push(d);
    if (e.pending) s.pending = e.pending.filter(p => !s.done.includes(p));
    else s.pending = s.pending.filter(p => !s.done.includes(p));
    if (e.blocker) s.blockers.push(e.blocker);
    if (e.verification) s.verification.push(e.verification);
    if (e.changedFiles) s.changedFiles = e.changedFiles;
  }
  return [...sessions.values()].map(s => {
    const minutes = Math.max(0, Math.round((nowMs - Date.parse(s.lastAt)) / 60000));
    const state = s.ended ? 'ended' : minutes <= staleMinutes ? 'recent' : 'stale';
    return { ...s, minutesSinceLast: minutes, state };
  }).sort((a, b) => String(b.lastAt).localeCompare(String(a.lastAt)));
}

/** Latest run per suite; when it did not pass, the last passing run is kept beside it. */
export function latestTests(runs) {
  const stamped = runs.map(r => ({ ...r, recordedAt: r.finishedAt }));
  const latest = latestBy(stamped, r => r.suite);
  const lastPass = latestBy(stamped.filter(r => r.result === 'pass'), r => r.suite);
  return [...latest.values()].map(r => {
    const pass = lastPass.get(r.suite);
    return r.result === 'pass' || !pass ? r : { ...r, lastPass: { finishedAt: pass.finishedAt, commit: pass.commit } };
  }).sort((a, b) => a.suite.localeCompare(b.suite));
}
