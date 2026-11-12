/* Developer dashboard bridge (tools/developer-dashboard).
   Inert unless this page is embedded by the local dashboard: an iframe whose URL
   has ?embed=dashboard. It only calls the prototype's own navigate(), loadHash()
   and phOpenPicker(), so draft guards and dialogs behave exactly as usual.
   Messages are accepted only from the parent window on the same origin, after a
   nonce handshake, and only for an allowlisted set of screens and overlays. */
'use strict';
(function () {
  if (new URLSearchParams(location.search).get('embed') !== 'dashboard' || window.parent === window) return;
  const CHANNEL = 'duewise-devdash', VERSION = 1;
  const SCREENS = ['dashboard', 'calendar', 'dues', 'settings', 'details', 'add', 'edit', 'reminders', 'catalog'];
  const VIEWS = ['explore', 'lab', 'decide', 'notify', 'compare'];
  const OVERLAYS = ['category-picker'];
  const LAB_SCREENS = ['dashboard', 'calendar', 'dues', 'settings'];
  let nonce = null, busy = false, last = '';

  const post = (type, payload, requestId) => window.parent.postMessage({ channel: CHANNEL, version: VERSION, nonce, type, requestId, payload }, location.origin);
  const dialogOpen = () => Boolean(document.querySelector('dialog[open]'));

  function current() {
    let dirty = false;
    try { dirty = ['add', 'edit'].includes(state.page) && Boolean(Lab.dirty()); } catch { dirty = false; }
    const view = Lab.enabled ? 'lab' : state.view;
    return {
      view, concept: state.concept,
      screen: view === 'explore' || view === 'lab' ? state.page : null,
      overlay: document.getElementById('category-picker')?.open ? 'category-picker' : null,
      dirty, modal: dialogOpen(), selected: state.selected ?? null,
    };
  }
  function publish() {
    if (!nonce) return;
    const value = current(), key = JSON.stringify(value);
    if (key === last) return;
    last = key;
    post('state', value);
  }
  let queued = false;
  const schedule = () => { if (queued) return; queued = true; queueMicrotask(() => { queued = false; publish(); }); };

  async function viaHash(hash) { history.replaceState(null, '', hash); await loadHash(); }
  const activeSample = id => state.data.find(d => d.id === id && !d.deleted);
  const outcome = ok => (ok ? { outcome: 'ok' } : { outcome: 'cancelled', reason: 'The prototype kept the current screen (for example, you chose to keep editing).' });

  async function go({ view = 'explore', screen = null, overlay = null, sample = null }) {
    if (!VIEWS.includes(view) || (screen && !SCREENS.includes(screen)) || (overlay && !OVERLAYS.includes(overlay))) return { outcome: 'invalid', reason: 'Unsupported preview target.' };
    if (dialogOpen()) return { outcome: 'busy', reason: 'A dialog is open in the prototype; close it first.' };
    if (view === 'notify') { await viaHash('#notifications'); return outcome(state.view === 'notify'); }
    if (view === 'decide') { await viaHash('#ux-decisions'); return outcome(state.view === 'decide'); }
    if (view === 'compare') { await viaHash('#compare'); return outcome(state.view === 'compare'); }
    if (view === 'lab') {
      const page = LAB_SCREENS.includes(screen) ? screen : 'dashboard';
      if (!Lab.enabled) await viaHash(`#interaction-lab/${page}`);
      else if (state.page !== page) await navigate(page);
      return outcome(Lab.enabled && state.page === page);
    }
    if (state.view !== 'explore' || Lab.enabled) {
      await viaHash(`#${state.concept}/dashboard`);
      if (state.view !== 'explore' || Lab.enabled) return outcome(false);
    }
    const page = screen ?? 'dashboard';
    if (page === 'details' || page === 'edit') {
      const due = activeSample(sample) || activeSample(state.selected) || state.data.find(d => !d.deleted);
      if (!due) return { outcome: 'unavailable', reason: 'The sample has no active due to open. Use "Reset sample data" in the prototype.' };
      if (state.page !== 'details' || state.selected !== due.id) {
        const previous = state.selected;
        state.selected = due.id;
        await navigate('details');
        if (state.page !== 'details') { state.selected = previous; return outcome(false); }
      }
      if (page === 'edit') { await navigate('edit'); if (state.page !== 'edit') return outcome(false); }
    } else if (page === 'catalog') {
      if (!['settings', 'catalog'].includes(state.page)) { await navigate('settings'); if (state.page !== 'settings') return outcome(false); }
      if (state.page !== 'catalog') { await navigate('catalog'); if (state.page !== 'catalog') return outcome(false); }
    } else if (state.page !== page) {
      await navigate(page);
      if (state.page !== page) return outcome(false);
    }
    if (overlay === 'category-picker') {
      if (typeof phOpenPicker !== 'function') return { outcome: 'unavailable', reason: 'The category picker is not available in this prototype.' };
      phOpenPicker('editor');
    }
    return { outcome: 'ok' };
  }

  window.addEventListener('message', async event => {
    if (event.source !== window.parent || event.origin !== location.origin) return;
    const m = event.data;
    if (!m || typeof m !== 'object' || m.channel !== CHANNEL || m.version !== VERSION) return;
    if (m.type === 'handshake') {
      if (typeof m.nonce !== 'string' || !/^[A-Za-z0-9-]{16,64}$/.test(m.nonce)) return;
      nonce = m.nonce; last = ''; publish();
      return;
    }
    if (!nonce || m.nonce !== nonce) return;
    if (m.type === 'get-state') { last = ''; publish(); return; }
    if (m.type !== 'navigate') return;
    const requestId = typeof m.requestId === 'string' && /^[A-Za-z0-9-]{1,64}$/.test(m.requestId) ? m.requestId : null;
    if (!requestId) return;
    const p = m.payload && typeof m.payload === 'object' ? m.payload : {};
    if (busy) { post('navigation-result', { outcome: 'busy', reason: 'Another preview request is still running.', state: current() }, requestId); return; }
    busy = true;
    let result;
    try {
      result = await go({
        view: typeof p.view === 'string' ? p.view : 'explore',
        screen: typeof p.screen === 'string' ? p.screen : null,
        overlay: typeof p.overlay === 'string' ? p.overlay : null,
        sample: typeof p.sample === 'string' ? p.sample : null,
      });
    } catch (error) {
      result = { outcome: 'error', reason: String(error?.message ?? error).slice(0, 200) };
    } finally { busy = false; }
    last = '';
    post('navigation-result', { ...result, state: current() }, requestId);
    publish();
  });

  const observer = new MutationObserver(schedule);
  document.querySelectorAll('dialog').forEach(d => observer.observe(d, { attributes: true, attributeFilter: ['open'] }));
  window.addEventListener('hashchange', () => setTimeout(schedule, 0));
  // Typing changes the draft without a render. These run after app.js's own
  // document listeners (registered earlier), so the draft is already updated.
  document.addEventListener('input', schedule);
  document.addEventListener('change', schedule);
  window.DashboardBridge = { publish: schedule };
  post('ready', {});
})();
