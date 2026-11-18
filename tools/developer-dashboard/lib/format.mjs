// Plain-text views of a snapshot for terminals and coding agents.
import { STATUS_LABEL } from './progress.mjs';

const pct = n => (n === null ? 'n/a' : `${n}%`);
const two = n => String(n).padStart(2, '0');
// Local time; date-only values (2026-11-06) are shown as written.
const day = iso => {
  if (!iso) return 'unknown';
  if (/^\d{4}-\d{2}-\d{2}$/.test(iso)) return iso;
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? iso : `${d.getFullYear()}-${two(d.getMonth() + 1)}-${two(d.getDate())} ${two(d.getHours())}:${two(d.getMinutes())}`;
};
const short = (text, n) => (text && text.length > n ? text.slice(0, n - 1) + '…' : text ?? '');

function counts(summary) {
  return Object.entries(STATUS_LABEL).map(([k, label]) => `${label} ${summary.byStatus[k]}`).join(' · ');
}

function sessionLine(s) {
  if (s.state === 'ended') return `${s.agent}: ended ${day(s.lastAt)}${s.task ? ` — ${short(s.task, 60)}` : ''}`;
  if (s.state === 'stale') return `${s.agent}: stale, no signal for ${s.minutesSinceLast} min${s.task ? ` — ${short(s.task, 60)}` : ''}`;
  return `${s.agent}: last signal ${s.minutesSinceLast} min ago (${s.lastHookEvent ?? s.lastEvent}), not confirmed running${s.task ? ` — ${short(s.task, 60)}` : ''}`;
}

export function brief(snapshot) {
  const { git, overview, features } = snapshot;
  const a = overview.approved;
  const lines = [];
  lines.push(`${snapshot.project} — developer status (deterministic, no AI) · ${day(snapshot.generatedAt)}`);
  if (git.available) {
    const up = git.upstream ? ` · ahead ${git.upstream.ahead} / behind ${git.upstream.behind}` : '';
    lines.push(`Git: ${git.branch} @ ${git.head?.short ?? '?'} "${short(git.head?.subject, 60)}" · ${git.changed.length} changed files${up}`);
  }
  if (snapshot.focus) lines.push(`Focus (from STATUS): ${short(snapshot.focus.text, 110)}`);
  lines.push(`Approved scope, ${a.features} features: ${counts(a)}`);
  lines.push(`  Gates with a current passing record: ${a.gates.pass} of ${a.gates.total} (${pct(a.percentVerified)}) · failing ${a.gates.fail} · blocked ${a.gates.blocked} · stale ${a.gates.stale} · no record ${a.gates.none}`);
  lines.push(`  Tasks checked: ${a.tasks.checked} of ${a.tasks.total} (${pct(a.percentTasks)}), workflow progress, not verification`);
  const others = Object.entries(overview.byScope).filter(([s]) => s !== 'approved').map(([s, v]) => `${s} ${v.features}`).join(' · ');
  if (others) lines.push(`Other scopes (not in the numbers above): ${others}`);
  lines.push(overview.blockers.length ? `Blockers: ${overview.blockers.map(b => `${b.featureId} (${short(b.reason, 60)})`).join('; ')}` : 'Blockers: none recorded');
  lines.push(`Sessions: ${snapshot.sessions.length ? snapshot.sessions.slice(0, 2).map(sessionLine).join(' | ') : 'no session records'}`);
  const tests = snapshot.tests.map(t => `${t.suite} ${t.result} ${day(t.finishedAt)} @ ${t.commit?.slice(0, 7) ?? '?'}${t.dirtyFiles ? ' (uncommitted changes)' : ''}`);
  lines.push(`Tests: ${tests.length ? tests.join(' · ') : 'no recorded runs'}`);
  const open = features.filter(f => f.scope === 'approved' && f.tasks?.open).sort((x, y) => y.tasks.open - x.tasks.open).slice(0, 5);
  if (open.length) lines.push(`Most open tasks (approved): ${open.map(f => `${f.id} ${f.tasks.open}`).join(', ')}`);
  const inProgress = features.filter(f => f.status.value === 'in_progress' && f.scope === 'approved').map(f => f.id);
  if (inProgress.length) lines.push(`In progress: ${inProgress.join(', ')}`);
  lines.push(`Data problems: ${snapshot.problems.length}${snapshot.problems.length ? ' (run "check")' : ''}`);
  lines.push('More: "feature <id>" for one feature; "serve" for the dashboard.');
  return lines.join('\n');
}

export function featureText(f) {
  const lines = [];
  lines.push(`${f.id} — ${f.name}`);
  lines.push(`Status: ${f.status.label}. ${f.status.reason}${f.status.source && f.status.source !== 'tasks' ? ` (source: ${f.status.source})` : ''}`);
  lines.push(`Phase: ${f.phase} · scope: ${f.scope}${f.lastCommitted ? ` · last committed ${day(f.lastCommitted)}` : ''}${f.uncommitted ? ' · has uncommitted changes' : ''}`);
  lines.push(`Docs: ${Object.values(f.docs).map(d => d.path + (d.line > 1 ? `:${d.line}` : '')).join(', ')}`);
  if (f.specStatus) lines.push(`Spec status line: ${short(f.specStatus, 200)}`);
  if (f.tasks) lines.push(`Tasks: ${f.tasks.checked} of ${f.tasks.total} checked`);
  lines.push(`Gates (${f.gates.length}):`);
  for (const g of f.gates) {
    const r = g.record ? ` — ${g.record.result} ${g.record.observedAt} by ${g.record.observer}, ${g.record.evidence}` : '';
    lines.push(`  ${g.id} [${g.state}] ${short(g.text, 90)}${r}`);
  }
  if (f.tasks?.openItems.length) {
    lines.push('Open tasks:');
    for (const t of f.tasks.openItems) lines.push(`  - ${short(t.text, 120)}${t.section ? ` (${t.section})` : ''}`);
  }
  if (f.blocker) lines.push(`Blocked: ${f.blocker.reason} (since ${day(f.blocker.since)})`);
  if (f.review) lines.push(`Visual review: ${f.review.decision}${f.review.stale ? ' (stale: prototype changed since)' : ''}${f.review.note ? ` — ${short(f.review.note, 120)}` : ''}`);
  if (f.prototype) lines.push(`Prototype: ${[f.prototype.view !== 'explore' && f.prototype.view, f.prototype.screen, f.prototype.overlay].filter(Boolean).join(' / ') || f.prototype.file}`);
  else lines.push(`Prototype: unavailable (${f.unavailable.kind}) — ${f.unavailable.note}`);
  if (f.note) lines.push(`Note: ${f.note}`);
  return lines.join('\n');
}
