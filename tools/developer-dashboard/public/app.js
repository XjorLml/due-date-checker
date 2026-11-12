// Developer dashboard page. Renders the server snapshot with plain DOM calls
// (text nodes only, never HTML strings from data) and talks to the embedded
// prototype through a validated postMessage protocol.

const CHANNEL = 'duewise-devdash', VERSION = 1;
const STATUS = { backlog: 'Backlog', in_progress: 'In Progress', in_review: 'In Review', blocked: 'Blocked', verified: 'Verified' };
const GATE = { pass: 'Pass', fail: 'Fail', blocked: 'Blocked', 'not-run': 'Not run', stale: 'Stale', none: 'No record' };
const VIEW = { explore: 'Explore', lab: 'Interaction lab', decide: 'UX decision lab', notify: 'Notification lab', compare: 'Compare all four' };
const SCREEN = { dashboard: 'Dashboard', calendar: 'Calendar', dues: 'Dues', settings: 'Settings', details: 'Details', add: 'Add due', edit: 'Edit due', reminders: 'Reminders', catalog: 'Catalog' };
const UNAVAILABLE = { 'no-ui': 'Not a screen', unimplemented: 'Not built in the prototype', unmapped: 'Not mapped yet' };
const token = document.querySelector('meta[name="dashboard-token"]').content;
const $ = id => document.getElementById(id);

const ui = {
  snapshot: null, selected: null, search: '', status: '', scope: '',
  frame: { file: null, ready: false, nonce: null, state: null, pending: new Map(), generation: 0, deferredReload: false },
};

// ---------- small helpers ----------
function h(tag, attrs = {}, ...children) {
  const el = document.createElement(tag);
  for (const [k, v] of Object.entries(attrs ?? {})) {
    if (v === undefined || v === null || v === false) continue;
    if (k === 'class') el.className = v;
    else if (k.startsWith('on')) el.addEventListener(k.slice(2), v);
    else el.setAttribute(k, v === true ? '' : String(v));
  }
  for (const c of children.flat()) if (c !== null && c !== undefined && c !== false) el.append(c instanceof Node ? c : String(c));
  return el;
}
const store = {
  get(key, fallback) { try { return localStorage.getItem(key) ?? fallback; } catch { return fallback; } },
  set(key, value) { try { localStorage.setItem(key, value); } catch { /* per-viewer convenience only */ } },
};
const pct = n => (n === null || n === undefined ? 'n/a' : `${n}%`);
const two = n => String(n).padStart(2, '0');
function when(iso) {
  if (!iso) return 'unknown';
  if (/^\d{4}-\d{2}-\d{2}$/.test(iso)) return iso;
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? iso : `${d.getFullYear()}-${two(d.getMonth() + 1)}-${two(d.getDate())} ${two(d.getHours())}:${two(d.getMinutes())}`;
}
function ago(iso) {
  const m = Math.max(0, Math.round((Date.now() - Date.parse(iso)) / 60000));
  return m < 1 ? 'just now' : m < 60 ? `${m} min ago` : m < 1440 ? `${Math.round(m / 60)} h ago` : `${Math.round(m / 1440)} d ago`;
}
const badge = (value, label) => h('span', { class: `badge s-${value}` }, label ?? STATUS[value]);
const bar = (value, cls = '') => { const s = h('span'); s.style.width = `${Math.max(0, Math.min(100, value ?? 0))}%`; return h('div', { class: `bar ${cls}`, role: 'presentation' }, s); };
const vscode = (path, line = 1) => `vscode://file/${ui.snapshot.root}/${path}:${line}`;
const docLinks = (path, line, label) => h('span', {}, h('a', { href: vscode(path, line), title: `Open ${path} in VS Code` }, label ?? path), ' ', h('a', { href: `/doc/${path}`, target: '_blank', rel: 'noopener', class: 'muted', title: 'View as text' }, '(text)'));
const screenLabel = p => [p.view && p.view !== 'explore' ? VIEW[p.view] : null, p.screen ? SCREEN[p.screen] : null, p.overlay ? 'category picker' : null].filter(Boolean).join(' · ') || 'Prototype';

// ---------- data ----------
async function loadSnapshot() {
  const res = await fetch('/api/snapshot', { cache: 'no-store' });
  if (!res.ok) throw new Error(`Snapshot unavailable (${res.status})`);
  ui.snapshot = await res.json();
  if (!ui.selected || !ui.snapshot.features.some(f => f.id === ui.selected)) {
    const fromHash = decodeURIComponent(location.hash.replace(/^#feature=/, ''));
    ui.selected = ui.snapshot.features.some(f => f.id === fromHash) ? fromHash : ui.snapshot.features[0]?.id ?? null;
  }
  renderAll();
}

function connect() {
  const conn = $('connection');
  const events = new EventSource('/api/events');
  events.addEventListener('hello', e => {
    conn.textContent = 'Live'; conn.className = 'connection live';
    const { version } = JSON.parse(e.data);
    if (!ui.snapshot || ui.snapshot.version !== version) loadSnapshot().catch(showError);
  });
  events.addEventListener('snapshot', () => loadSnapshot().catch(showError));
  events.addEventListener('preview', e => onPreviewChanged(JSON.parse(e.data)));
  events.addEventListener('problem', e => showError(new Error(JSON.parse(e.data).message)));
  events.onerror = () => { conn.textContent = 'Reconnecting…'; conn.className = 'connection down'; };
}
function showError(error) { $('overview-note').textContent = error.message; }

// ---------- rendering ----------
function renderAll() {
  const s = ui.snapshot;
  $('project').textContent = s.project;
  document.title = `${s.project} · Developer dashboard`;
  renderGitChips(s);
  renderCards(s);
  renderScopeOptions(s);
  renderList();
  renderInspector();
  renderActivity(s);
  markOnScreen();
}

function renderGitChips(s) {
  const g = s.git;
  const chips = g.available ? [
    h('span', { class: 'chip mono' }, `${g.branch} @ ${g.head?.short ?? '?'}`),
    h('span', { class: 'chip', title: g.head?.subject ?? '' }, (g.head?.subject ?? '').slice(0, 60)),
    h('span', { class: 'chip' }, `${g.changed.length} changed files`),
    g.upstream ? h('span', { class: 'chip' }, `ahead ${g.upstream.ahead} · behind ${g.upstream.behind}`) : null,
  ] : [h('span', { class: 'chip' }, 'Git unavailable')];
  $('git-chips').replaceChildren(...chips.filter(Boolean), h('span', { class: 'chip muted' }, `snapshot ${when(s.generatedAt)}`));
}

function renderCards(s) {
  const a = s.overview.approved;
  const card = (title, ...body) => h('div', { class: 'card' }, h('h2', {}, title), ...body);
  const statusButtons = h('div', { class: 'status-chips' }, Object.entries(STATUS).map(([k, label]) =>
    h('button', { type: 'button', class: `s-${k}`, 'aria-pressed': String(ui.status === k), onclick: () => { ui.status = ui.status === k ? '' : k; $('status-filter').value = ui.status; renderList(); renderCards(ui.snapshot); } }, `${label} ${a.byStatus[k]}`)));
  const tests = s.tests.length
    ? h('div', {}, s.tests.map(t => h('div', {}, h('span', { class: `badge g-${t.result === 'pass' ? 'pass' : 'fail'}` }, t.result), ' ', t.suite, h('span', { class: 'muted' }, ` · ${ago(t.finishedAt)}`))))
    : h('p', { class: 'note' }, 'No recorded runs. Run: cli.mjs test <suite>');
  const others = Object.entries(s.overview.byScope).filter(([k]) => k !== 'approved').map(([k, v]) => `${k} ${v.features}`).join(' · ');
  $('cards').replaceChildren(
    card('Verified (approved scope)',
      h('div', { class: 'big' }, `${a.gates.pass} / ${a.gates.total}`, h('span', { class: 'muted' }, ' gates')),
      bar(a.percentVerified, 'verified'),
      h('div', { class: 'note' }, `${pct(a.percentVerified)} · failing ${a.gates.fail} · blocked ${a.gates.blocked} · stale ${a.gates.stale} · no record ${a.gates.none}`)),
    card('Tasks checked',
      h('div', { class: 'big' }, `${a.tasks.checked} / ${a.tasks.total}`),
      bar(a.percentTasks),
      h('div', { class: 'note' }, `${pct(a.percentTasks)} · workflow progress, not verification`)),
    card(`Features · ${a.features} approved`, statusButtons, h('div', { class: 'note' }, others ? `Not counted: ${others}` : '')),
    card('Blockers', s.overview.blockers.length
      ? h('ul', {}, s.overview.blockers.map(b => h('li', {}, h('a', { href: `#feature=${b.featureId}`, onclick: e => { e.preventDefault(); select(b.featureId); } }, b.featureId), ` ${b.reason}`)))
      : h('p', { class: 'note' }, 'None recorded.')),
    card('Focus (from STATUS)', s.focus ? h('div', {}, s.focus.text, h('div', { class: 'note' }, docLinks(s.focus.source, 1))) : h('p', { class: 'note' }, 'Not found.')),
    card('Latest tests', tests),
  );
  $('overview-note').textContent = a.gates.pass === 0 && a.gates.total
    ? 'No approved-scope gate has a verification record yet. Earlier evidence lives in STATUS; add records with "cli.mjs verify" as checks are observed.'
    : s.overview.recordsSince ? `Verification records since ${when(s.overview.recordsSince)}. Percentages count gates with a current passing record; nothing is estimated.` : '';
}

function renderScopeOptions(s) {
  const select = $('scope-filter');
  const scopes = [...new Set(s.features.map(f => f.scope))];
  if (select.options.length - 1 === scopes.length) return;
  select.replaceChildren(h('option', { value: '' }, 'All scopes'), ...scopes.map(sc => h('option', { value: sc }, sc)));
  select.value = ui.scope;
}

function visibleFeatures() {
  const q = ui.search.trim().toLowerCase();
  return ui.snapshot.features.filter(f =>
    (!ui.status || f.status.value === ui.status) && (!ui.scope || f.scope === ui.scope) &&
    (!q || [f.id, f.name, f.phase, ...f.gates.map(g => `${g.id} ${g.text}`), ...(f.tasks?.openItems ?? []).map(t => t.text)].join(' ').toLowerCase().includes(q)));
}

function renderList() {
  const list = $('feature-list');
  const features = visibleFeatures();
  $('list-count').textContent = `${features.length} of ${ui.snapshot.features.length} features`;
  const groups = [];
  for (const phase of ui.snapshot.phases) {
    const items = features.filter(f => f.phase === phase);
    if (!items.length) continue;
    groups.push(h('h3', { class: 'phase' }, phase));
    for (const f of items) {
      const pass = f.gates.filter(g => g.state === 'pass').length;
      groups.push(h('button', {
        type: 'button', class: 'feature', 'data-id': f.id, 'aria-current': String(f.id === ui.selected),
        onclick: () => select(f.id),
      },
      h('span', { class: 'row' }, h('span', { class: 'mono' }, f.id), badge(f.status.value, f.status.label)),
      h('span', { class: 'name', title: f.name }, f.name),
      h('span', { class: 'meta' },
        f.tasks ? h('span', {}, `tasks ${f.tasks.checked}/${f.tasks.total}`) : null,
        h('span', {}, `gates ${pass}/${f.gates.length}`),
        h('span', {}, f.prototype ? '▶ preview' : 'no preview'),
        f.review ? h('span', {}, f.review.decision === 'approved' ? (f.review.stale ? 'visual ok (stale)' : 'visual ok') : 'changes requested') : null,
        f.uncommitted ? h('span', {}, 'uncommitted') : null)));
    }
  }
  if (!groups.length) groups.push(h('p', { class: 'note' }, 'No features match the filters.'));
  list.replaceChildren(...groups);
  markOnScreen();
}

function selected() { return ui.snapshot?.features.find(f => f.id === ui.selected) ?? null; }

function select(id, { scroll = true } = {}) {
  ui.selected = id;
  history.replaceState(null, '', `#feature=${encodeURIComponent(id)}`);
  for (const el of document.querySelectorAll('.feature')) el.setAttribute('aria-current', String(el.dataset.id === id));
  if (scroll) document.querySelector(`.feature[data-id="${CSS.escape(id)}"]`)?.scrollIntoView({ block: 'nearest' });
  renderInspector();
}

function renderInspector() {
  const f = selected();
  const box = $('inspector');
  if (!f) { box.replaceChildren(h('p', { class: 'note' }, 'Select a feature.')); return; }
  const parts = [];
  parts.push(h('div', { class: 'head' }, h('span', { class: 'mono' }, f.id), badge(f.status.value, f.status.label), h('span', { class: 'chip' }, f.scope), h('span', { class: 'chip' }, f.phase)));
  parts.push(h('h2', {}, f.name));
  parts.push(h('p', { class: 'note' }, `${f.status.label}: ${f.status.reason}`, f.status.source && f.status.source !== 'tasks' ? ` Source: ${f.status.source}${f.status.seeded ? `, seeded ${f.status.seeded}` : ''}.` : ''));
  if (f.blocker) parts.push(h('div', { class: 'callout bad' }, `Blocked since ${when(f.blocker.since)}: ${f.blocker.reason}`));

  const actions = h('div', { class: 'actions' });
  if (f.prototype) actions.append(h('button', { type: 'button', class: 'primary', onclick: () => previewFeature(f) }, `Preview feature · ${f.prototype.file === ui.snapshot.preview.entry ? screenLabel(f.prototype) : f.prototype.file.split('/').slice(-2).join('/')}`));
  else actions.append(h('button', { type: 'button', disabled: true, title: f.unavailable.note }, `Preview unavailable · ${UNAVAILABLE[f.unavailable.kind]}`));
  for (const [kind, d] of Object.entries(f.docs)) actions.append(h('a', { class: 'button', href: vscode(d.path, d.line), title: `Open ${d.path} in VS Code` }, `Open ${kind}`));
  parts.push(actions);
  if (!f.prototype) parts.push(h('div', { class: 'callout' }, `${UNAVAILABLE[f.unavailable.kind]}: ${f.unavailable.note}`));
  if (f.note) parts.push(h('div', { class: 'callout' }, f.note));

  parts.push(h('dl', { class: 'kv' },
    f.specStatus ? [h('dt', {}, 'Spec says'), h('dd', {}, f.specStatus)] : null,
    f.kind === 'requirement' ? [h('dt', {}, 'Requirement'), h('dd', {}, f.description)] : null,
    f.tasks ? [h('dt', {}, 'Tasks'), h('dd', {}, `${f.tasks.checked} of ${f.tasks.total} checked`)] : null,
    [h('dt', {}, 'Last committed'), h('dd', {}, f.lastCommitted ? when(f.lastCommitted) : 'never', f.uncommitted ? ' · has uncommitted changes' : '')],
    f.relatedFiles.length ? [h('dt', {}, 'Related files'), h('dd', {}, f.relatedFiles.map((p, i) => [i ? ', ' : '', h('a', { href: vscode(p) }, p)]))] : null,
  ));

  parts.push(h('h3', {}, `Acceptance criteria and verification (${f.gates.length})`));
  parts.push(f.gates.length ? h('ul', { class: 'gates' }, f.gates.map(g => h('li', {},
    h('a', { class: 'mono', href: vscode(g.doc, g.line), title: 'Open in VS Code' }, g.id),
    h('span', { class: `badge g-${g.state}` }, GATE[g.state] ?? g.state),
    h('span', { class: 'gate-text' }, g.text || '(no description)',
      g.record ? h('div', { class: 'note' }, `${g.record.result} · ${g.record.observedAt} · ${g.record.observer} · `, g.record.evidence.startsWith('https://') ? h('a', { href: g.record.evidence, target: '_blank', rel: 'noopener' }, 'evidence') : docLinks(g.record.evidence.split('#')[0], 1, 'evidence'), g.state === 'stale' ? ' · files changed since this record' : '') : null))))
    : h('p', { class: 'note' }, 'No scenario IDs found in this spec\'s tables.'));

  if (f.tasks?.openItems.length) {
    parts.push(h('h3', {}, `Open tasks (${f.tasks.openItems.length})`));
    parts.push(h('ul', { class: 'tasks' }, f.tasks.openItems.map(t => h('li', {}, h('span', {}, t.text, t.section ? h('span', { class: 'muted' }, ` · ${t.section}`) : null)))));
  }

  parts.push(h('h3', {}, 'Visual review'));
  if (f.review) {
    const cls = f.review.stale ? 'warn' : f.review.decision === 'approved' ? 'good' : 'bad';
    parts.push(h('div', { class: `callout ${cls}` }, `${f.review.decision === 'approved' ? 'Approved' : 'Changes requested'} ${when(f.review.at)}${f.review.screen ? ` on ${f.review.screen}` : ''}.`, f.review.stale ? ' The prototype changed since this review.' : '', f.review.note ? h('div', {}, f.review.note) : null));
  }
  if (f.prototype) {
    const note = h('textarea', { id: 'review-note', 'aria-label': 'Review note', placeholder: 'Optional note: what looks right or what to change' });
    const status = h('p', { class: 'note', role: 'status' }, 'A visual review records how the prototype looks. It does not verify behavior, tests or release acceptance.');
    const send = decision => async () => {
      status.textContent = 'Saving…';
      try {
        const res = await fetch('/api/reviews', { method: 'POST', headers: { 'Content-Type': 'application/json', 'X-Dashboard-Token': token }, body: JSON.stringify({ featureId: f.id, decision, note: note.value }) });
        const body = await res.json();
        if (!res.ok) throw new Error(body.error ?? res.statusText);
        status.textContent = 'Saved. It is stored in tools/developer-dashboard/records/reviews.jsonl.';
      } catch (error) { status.textContent = `Not saved: ${error.message}`; }
    };
    parts.push(note, h('div', { class: 'actions' },
      h('button', { type: 'button', onclick: send('approved') }, 'Approve visual'),
      h('button', { type: 'button', onclick: send('changes-requested') }, 'Request changes')), status);
  } else parts.push(h('p', { class: 'note' }, 'Visual review needs a prototype screen.'));

  $('inspector').replaceChildren(...parts);
}

function renderActivity(s) {
  const staleMin = s.sessionStaleMinutes;
  const sessions = s.sessions.map(x => {
    const minutes = Math.round((Date.now() - Date.parse(x.lastAt)) / 60000);
    const state = x.ended ? `Ended ${when(x.lastAt)}` : minutes <= staleMin ? `Last signal ${ago(x.lastAt)} · not confirmed running` : `Stale · no signal for ${ago(x.lastAt).replace(' ago', '')}`;
    return h('div', { class: 'session' },
      h('div', {}, h('strong', {}, x.agent), ' ', h('span', { class: 'muted' }, state)),
      h('div', { class: 'note' }, `Started ${when(x.startedAt)} · last ${x.lastHookEvent ?? x.lastEvent} · via ${x.sources.join(', ')}`),
      x.task ? h('div', {}, `Task: ${x.task}`) : null,
      x.done.length ? h('details', {}, h('summary', {}, `Completed checkpoints (${x.done.length})`), h('ul', {}, x.done.map(d => h('li', {}, d)))) : null,
      x.pending.length ? h('details', { open: true }, h('summary', {}, `Pending checkpoints (${x.pending.length})`), h('ul', {}, x.pending.map(d => h('li', {}, d)))) : null,
      x.blockers.length ? h('div', { class: 'callout bad' }, `Blockers: ${x.blockers.join('; ')}`) : null,
      x.verification.length ? h('details', {}, h('summary', {}, `Verification notes (${x.verification.length})`), h('ul', {}, x.verification.map(d => h('li', {}, d)))) : null,
      x.changedFiles.length ? h('details', {}, h('summary', {}, `Changed files at last signal (${x.changedFiles.length})`), h('ul', { class: 'mono' }, x.changedFiles.slice(0, 30).map(p => h('li', {}, p)))) : null);
  });
  $('sessions').replaceChildren(h('h2', {}, 'AI sessions'),
    h('p', { class: 'note' }, 'From agent hooks and CLI checkpoints. A recent signal shows activity, not that an agent is running now.'),
    ...(sessions.length ? sessions : [h('p', { class: 'note' }, 'No session records yet.')]));

  const g = s.git;
  $('git-card').replaceChildren(h('h2', {}, 'Git'), g.available ? h('div', {},
    h('div', { class: 'mono' }, `${g.branch} @ ${g.head?.short}`), h('div', {}, g.head?.subject ?? ''),
    h('div', { class: 'note' }, `Committed ${when(g.head?.date)}${g.upstream ? ` · ahead ${g.upstream.ahead}, behind ${g.upstream.behind} (local tracking refs)` : ' · no upstream'}`),
    g.changed.length ? h('details', {}, h('summary', {}, `${g.changed.length} uncommitted changes`), h('ul', { class: 'mono' }, g.changed.slice(0, 40).map(c => h('li', {}, `${c.code} ${c.path}`)))) : h('p', { class: 'note' }, 'Working tree clean.'),
    h('h3', {}, 'Recent commits'), h('ul', {}, g.recent.map(c => h('li', {}, h('span', { class: 'mono' }, c.short), ` ${c.subject} `, h('span', { class: 'muted' }, when(c.date))))))
    : h('p', { class: 'note' }, 'Git is not available.'));

  const latest = new Map(s.tests.map(t => [t.suite, t]));
  $('tests-card').replaceChildren(h('h2', {}, 'Tests'), h('p', { class: 'note' }, 'Recorded runs only. Viewing never runs tests; use "cli.mjs test <suite>".'),
    h('ul', {}, s.suites.map(su => {
      const t = latest.get(su.id);
      return h('li', {}, h('strong', {}, su.id), ' ', t
        ? [h('span', { class: `badge g-${t.result === 'pass' ? 'pass' : 'fail'}` }, t.result), h('span', { class: 'muted' }, ` ${when(t.finishedAt)} @ ${t.commit?.slice(0, 7) ?? '?'}${t.dirtyFiles ? ` with ${t.dirtyFiles} uncommitted files` : ''} · ${Math.round(t.durationMs / 1000)} s`),
          t.result !== 'pass' ? h('div', { class: 'note' }, t.lastPass ? `Last pass: ${when(t.lastPass.finishedAt)} @ ${t.lastPass.commit?.slice(0, 7) ?? '?'} (an older run; it does not cover this failure)` : 'No passing run recorded.') : null,
          t.result !== 'pass' ? h('details', {}, h('summary', {}, 'Output tail'), h('pre', { class: 'mono' }, t.outputTail)) : null]
        : h('span', { class: 'muted' }, 'not run'), h('div', { class: 'note' }, su.label));
    })));

  $('problems-card').replaceChildren(h('h2', {}, 'Data problems'), s.problems.length
    ? h('ul', {}, s.problems.map(p => h('li', {}, p)))
    : h('p', { class: 'note' }, 'None. Config, mappings and records are consistent.'),
    h('p', { class: 'note' }, `Node ${s.environment.node} · ${s.environment.platform}`));
}

// ---------- preview and bridge ----------
const frame = () => $('preview');

function loadFrame(file) {
  const f = ui.frame;
  f.file = file; f.ready = false; f.nonce = null; f.state = null; f.lastScreenKey = null; f.generation++;
  for (const p of f.pending.values()) p.resolve({ outcome: 'cancelled', reason: 'The preview reloaded.' });
  f.pending.clear();
  const bridged = file === ui.snapshot.preview.entry;
  frame().src = `/preview/${file}${bridged ? '?embed=dashboard' : ''}`;
  updateOpenTab();
  $('active-screen').textContent = bridged ? 'Connecting…' : file;
  $('screen-flags').textContent = bridged ? '' : '(no dashboard bridge in this file)';
}

function updateOpenTab() {
  let hash = '';
  try { hash = frame().contentWindow?.location.hash ?? ''; } catch { hash = ''; }
  $('open-tab').href = `/preview/${ui.frame.file}${hash}`;
}

function postToFrame(type, payload, requestId) {
  frame().contentWindow?.postMessage({ channel: CHANNEL, version: VERSION, nonce: ui.frame.nonce, type, requestId, payload }, location.origin);
}

window.addEventListener('message', event => {
  if (event.source !== frame().contentWindow || event.origin !== location.origin) return;
  const m = event.data;
  if (!m || typeof m !== 'object' || m.channel !== CHANNEL || m.version !== VERSION || typeof m.type !== 'string') return;
  const f = ui.frame;
  if (m.type === 'ready') {
    f.nonce = crypto.randomUUID();
    f.ready = true;
    postToFrame('handshake', {});
    return;
  }
  if (!f.nonce || m.nonce !== f.nonce) return;
  if (m.type === 'state' && m.payload && typeof m.payload === 'object') onFrameState(m.payload);
  else if (m.type === 'navigation-result' && typeof m.requestId === 'string') {
    const p = f.pending.get(m.requestId);
    if (!p) return; // stale or unknown reply
    f.pending.delete(m.requestId);
    if (m.payload?.state) onFrameState(m.payload.state);
    p.resolve(m.payload ?? { outcome: 'error' });
  }
});

frame().addEventListener('load', () => {
  updateOpenTab();
  if (ui.frame.file === ui.snapshot?.preview.entry) {
    setTimeout(() => { if (!ui.frame.ready) $('preview-status').textContent = 'The prototype loaded, but its dashboard bridge did not answer.'; }, 3000);
  }
});

function featuresOnScreen(s) {
  if (!ui.snapshot || !s) return [];
  return ui.snapshot.features.filter(f => {
    const p = f.prototype;
    if (!p || p.file !== ui.snapshot.preview.entry || p.view !== s.view) return false;
    if (p.screen && p.screen !== s.screen) return false;
    return (p.overlay ?? null) === (s.overlay ?? null);
  });
}

function onFrameState(s) {
  const f = ui.frame;
  f.state = s;
  $('active-screen').textContent = screenLabel(s);
  $('screen-flags').textContent = [s.dirty ? 'unsaved sample draft' : null, s.modal ? 'dialog open' : null].filter(Boolean).join(' · ');
  updateOpenTab();
  if (f.deferredReload && !s.dirty && !s.modal) $('preview-banner').querySelector('.hint')?.replaceChildren('You can reload now.');
  if (f.pending.size) { $('preview-status').textContent = s.modal ? 'Waiting for your answer in the prototype\'s dialog…' : $('preview-status').textContent; return; }
  const candidates = featuresOnScreen(s);
  const related = $('preview-related');
  if (candidates.length > 1) {
    related.hidden = false;
    related.replaceChildren(h('span', { class: 'muted' }, 'This screen relates to:'), ...candidates.map(c => h('button', { type: 'button', 'aria-pressed': String(c.id === ui.selected), onclick: () => select(c.id) }, c.id)));
  } else if (!candidates.length) {
    related.hidden = false;
    related.replaceChildren(h('span', { class: 'muted' }, 'No tracked feature is mapped to this screen.'));
  } else related.hidden = true;
  // Follow the prototype only when its screen changed and the match is unambiguous;
  // never guess between features that share a screen.
  const screenKey = `${s.view}|${s.screen}|${s.overlay}`;
  const moved = screenKey !== f.lastScreenKey;
  f.lastScreenKey = screenKey;
  if (moved && candidates.length === 1 && candidates[0].id !== ui.selected) select(candidates[0].id);
  markOnScreen();
}

function markOnScreen() {
  const ids = new Set(featuresOnScreen(ui.frame.state).map(f => f.id));
  for (const el of document.querySelectorAll('.feature')) el.classList.toggle('on-screen', ids.has(el.dataset.id));
}

async function waitForBridge(ms = 6000) {
  const start = Date.now();
  while (!(ui.frame.ready && ui.frame.state)) {
    if (Date.now() - start > ms) return false;
    await new Promise(r => setTimeout(r, 100));
  }
  return true;
}

async function previewFeature(feature) {
  const status = $('preview-status');
  select(feature.id, { scroll: false });
  const p = feature.prototype;
  if (!p) { status.textContent = `Preview unavailable: ${feature.unavailable.note}`; return; }
  if (p.file !== ui.snapshot.preview.entry) {
    if (ui.frame.file !== p.file) loadFrame(p.file);
    status.textContent = `Showing ${p.file}. This file has no bridge, so screens cannot be selected from here.`;
    return;
  }
  if (ui.frame.file !== p.file) loadFrame(p.file);
  if (!(await waitForBridge())) { status.textContent = 'The prototype did not connect. Try Reload.'; return; }
  const requestId = crypto.randomUUID();
  status.textContent = `Opening ${screenLabel(p)}…`;
  const result = await new Promise(resolve => {
    const generation = ui.frame.generation;
    ui.frame.pending.set(requestId, { resolve });
    postToFrame('navigate', { view: p.view, screen: p.screen ?? null, overlay: p.overlay ?? null, sample: p.sample ?? null }, requestId);
    setTimeout(() => { if (ui.frame.pending.has(requestId) && ui.frame.generation === generation) { ui.frame.pending.delete(requestId); resolve({ outcome: 'timeout', reason: 'The prototype did not answer in time.' }); } }, 120_000);
  });
  if (result.outcome === 'ok') { status.textContent = `Showing ${feature.id} on ${screenLabel(p)}.`; select(feature.id, { scroll: false }); }
  else status.textContent = `${feature.id} not shown (${result.outcome}). ${result.reason ?? ''}`;
  if (ui.frame.state) onFrameState(ui.frame.state);
}

function reloadFrame() {
  ui.frame.deferredReload = false;
  $('preview-banner').hidden = true;
  if (ui.frame.file) loadFrame(ui.frame.file);
}

function onPreviewChanged({ files }) {
  const s = ui.frame.state;
  const list = (files ?? []).slice(0, 4).join(', ');
  if (s && (s.dirty || s.modal)) {
    ui.frame.deferredReload = true;
    const banner = $('preview-banner');
    banner.hidden = false;
    banner.replaceChildren(h('span', {}, `Prototype files changed (${list}). Reloading would discard the open draft or dialog. `), h('span', { class: 'hint muted' }, ''), h('button', { type: 'button', onclick: reloadFrame }, 'Reload now'));
    return;
  }
  $('preview-status').textContent = `Prototype files changed (${list}); reloaded. Sample data resets on reload.`;
  reloadFrame();
}

// ---------- controls ----------
function setupControls() {
  const theme = $('theme');
  theme.value = store.get('devdash.theme', 'system');
  const applyTheme = () => { if (theme.value === 'system') delete document.documentElement.dataset.theme; else document.documentElement.dataset.theme = theme.value; store.set('devdash.theme', theme.value); };
  theme.addEventListener('change', applyTheme); applyTheme();

  $('search').addEventListener('input', e => { ui.search = e.target.value; renderList(); });
  $('status-filter').addEventListener('change', e => { ui.status = e.target.value; renderList(); renderCards(ui.snapshot); });
  $('scope-filter').addEventListener('change', e => { ui.scope = e.target.value; renderList(); });
  $('feature-list').addEventListener('keydown', e => {
    if (!['ArrowDown', 'ArrowUp'].includes(e.key)) return;
    const buttons = [...document.querySelectorAll('.feature')];
    const i = buttons.indexOf(document.activeElement);
    const next = buttons[Math.max(0, Math.min(buttons.length - 1, i + (e.key === 'ArrowDown' ? 1 : -1)))];
    if (next) { e.preventDefault(); next.focus(); select(next.dataset.id, { scroll: false }); }
  });

  const viewport = $('viewport');
  const applyViewport = () => {
    const wrap = $('frame-wrap');
    const fixed = viewport.value !== 'fill';
    wrap.classList.toggle('fixed', fixed);
    frame().style.width = fixed ? `${viewport.value}px` : '100%';
    store.set('devdash.viewport', viewport.value);
  };
  viewport.value = store.get('devdash.viewport', 'fill');
  viewport.addEventListener('change', applyViewport); applyViewport();
  $('reload-preview').addEventListener('click', reloadFrame);
  $('fullscreen').addEventListener('click', () => {
    const blocked = error => { $('preview-status').textContent = `Full screen is not available here (${error.message}). Use "Open in new tab" instead.`; };
    if (document.fullscreenElement) document.exitFullscreen().catch(blocked);
    else if ($('preview-pane').requestFullscreen) $('preview-pane').requestFullscreen().catch(blocked);
    else blocked(new Error('not supported'));
  });

  const workspace = $('workspace');
  const setWidth = w => { const clamped = Math.max(320, Math.min(window.innerWidth - 620, w)); workspace.style.setProperty('--preview-width', `${clamped}px`); store.set('devdash.previewWidth', String(clamped)); };
  const saved = Number(store.get('devdash.previewWidth', '0'));
  if (saved) workspace.style.setProperty('--preview-width', `${saved}px`);
  const resizer = $('resizer');
  resizer.addEventListener('pointerdown', e => {
    resizer.setPointerCapture(e.pointerId);
    const right = workspace.getBoundingClientRect().right;
    const move = ev => setWidth(right - ev.clientX);
    const up = () => { resizer.removeEventListener('pointermove', move); resizer.removeEventListener('pointerup', up); };
    resizer.addEventListener('pointermove', move); resizer.addEventListener('pointerup', up);
  });
  resizer.addEventListener('keydown', e => {
    if (!['ArrowLeft', 'ArrowRight'].includes(e.key)) return;
    e.preventDefault();
    const current = parseInt(getComputedStyle(workspace).getPropertyValue('--preview-width'), 10) || 500;
    setWidth(current + (e.key === 'ArrowLeft' ? 32 : -32));
  });

  // Relative times ("5 min ago") refresh locally; this does not contact the server.
  setInterval(() => { if (ui.snapshot) renderActivity(ui.snapshot); }, 60_000);
}

setupControls();
loadSnapshot().then(() => { loadFrame(ui.snapshot.preview.entry); connect(); }).catch(error => { showError(error); connect(); });
