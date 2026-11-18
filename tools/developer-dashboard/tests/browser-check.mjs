// End-to-end check in a real headless Edge/Chrome through the DevTools protocol.
// No packages. Uses the real prototype and specs, but writes reviews and sessions
// to temporary copies of the records so committed records stay untouched.
// Usage: node tools/developer-dashboard/tests/browser-check.mjs   (EDGE_PATH overrides the browser)
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { defaultRoot } from '../lib/paths.mjs';
import { loadConfig } from '../lib/config.mjs';
import { startServer } from '../server.mjs';
import { appendJsonl } from '../lib/records.mjs';

const delay = ms => new Promise(r => setTimeout(r, ms));
const work = fs.mkdtempSync(path.join(os.tmpdir(), 'devdash-browser-'));
let server, browser, ws;
const step = name => console.log(`ok - ${name}`);

async function main() {
  const executable = process.env.EDGE_PATH || ['C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', 'C:/Program Files/Google/Chrome/Application/chrome.exe', '/usr/bin/chromium', '/usr/bin/google-chrome'].find(p => fs.existsSync(p));
  assert.ok(executable, 'Set EDGE_PATH to an installed Chromium-based browser.');

  const config = loadConfig(defaultRoot);
  const records = path.join(work, 'records');
  fs.mkdirSync(records);
  for (const name of fs.readdirSync(config.abs.records)) fs.copyFileSync(path.join(config.abs.records, name), path.join(records, name));
  config.abs.records = records;
  config.abs.mappings = path.join(records, 'mappings.json');
  config.abs.runtime = path.join(work, 'runtime');
  server = await startServer({ root: defaultRoot, config, port: 0 });

  const profile = path.join(work, 'profile');
  browser = spawn(executable, ['--headless=new', '--disable-gpu', '--no-first-run', '--no-default-browser-check', '--disable-extensions', '--disable-background-networking', '--disable-sync', '--remote-debugging-port=0', `--user-data-dir=${profile}`, 'about:blank'], { windowsHide: true, stdio: 'ignore' });
  const portFile = path.join(profile, 'DevToolsActivePort');
  for (let i = 0; i < 100 && !fs.existsSync(portFile); i++) await delay(150);
  assert.ok(fs.existsSync(portFile), 'Browser debugging startup timed out.');
  const debugPort = fs.readFileSync(portFile, 'utf8').split('\n')[0];
  const target = await (await fetch(`http://127.0.0.1:${debugPort}/json/new?about:blank`, { method: 'PUT' })).json();
  ws = new WebSocket(target.webSocketDebuggerUrl);
  await new Promise((resolve, reject) => { ws.onopen = resolve; ws.onerror = reject; });

  let id = 0;
  const pending = new Map(), errors = [];
  ws.onmessage = ({ data }) => {
    const m = JSON.parse(data);
    if (m.id) { const p = pending.get(m.id); if (p) { pending.delete(m.id); m.error ? p.reject(new Error(m.error.message)) : p.resolve(m.result); } }
    else if (m.method === 'Runtime.exceptionThrown') errors.push(m.params.exceptionDetails.exception?.description ?? m.params.exceptionDetails.text);
    else if (m.method === 'Log.entryAdded' && m.params.entry.level === 'error' && !/favicon/.test(m.params.entry.url ?? '')) errors.push(`${m.params.entry.source}: ${m.params.entry.text}`);
  };
  const send = (method, params = {}) => new Promise((resolve, reject) => {
    const n = ++id; pending.set(n, { resolve, reject }); ws.send(JSON.stringify({ id: n, method, params }));
    setTimeout(() => { if (pending.has(n)) { pending.delete(n); reject(new Error(`CDP timeout: ${method}`)); } }, 30000).unref();
  });
  const evaluate = async expression => {
    const r = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
    if (r.exceptionDetails) throw new Error(r.exceptionDetails.exception?.description ?? 'Evaluation failed: ' + expression);
    return r.result.value;
  };
  const waitFor = async (expression, label = expression) => {
    for (let i = 0; i < 120; i++) { if (await evaluate(expression)) return; await delay(100); }
    throw new Error(`Timed out: ${label}`);
  };
  const inFrame = expression => evaluate(`document.getElementById('preview').contentWindow.eval(${JSON.stringify(expression)})`);
  const text = id => evaluate(`document.getElementById(${JSON.stringify(id)}).textContent`);
  const pick = async featureId => {
    await evaluate(`document.querySelector('.feature[data-id="${featureId}"]').click()`);
    await waitFor(`document.querySelector('#inspector .head .mono')?.textContent === ${JSON.stringify(featureId)}`, `inspector shows ${featureId}`);
  };
  const preview = async featureId => { await pick(featureId); await evaluate(`document.querySelector('#inspector button.primary').click()`); };
  const shot = async name => {
    const image = await send('Page.captureScreenshot', { format: 'png' });
    fs.writeFileSync(path.join(work, `${name}.png`), Buffer.from(image.data, 'base64'));
  };

  await send('Runtime.enable'); await send('Page.enable'); await send('Log.enable');
  await send('Emulation.setDeviceMetricsOverride', { width: 1600, height: 1000, deviceScaleFactor: 1, mobile: false });
  await send('Page.navigate', { url: server.url });

  // DD01: overview and features load without AI or network services.
  await waitFor(`document.querySelectorAll('.feature').length > 30`, 'feature list rendered');
  await waitFor(`document.getElementById('connection').textContent === 'Live'`, 'event stream connected');
  assert.match(await text('cards'), /Verified \(approved scope\)/);
  step('overview, feature list and live connection load');

  // DD04: the embedded prototype connects through the bridge on its default screen.
  await waitFor(`document.getElementById('active-screen').textContent === 'Dashboard'`, 'bridge reports the Dashboard screen');
  const origin = await inFrame('performance.timeOrigin');
  step('prototype embedded; bridge handshake completed');

  // DD06: Preview Feature opens the exact mapped screen and overlay through the prototype's own code.
  await preview('ph-categories-native');
  await waitFor(`document.getElementById('preview').contentWindow.eval("state.page === 'add' && document.getElementById('category-picker').open")`, 'Add screen with the category picker open');
  assert.equal(await text('active-screen'), 'Add due · category picker');
  assert.equal(await evaluate(`document.querySelector('.feature[data-id="ph-categories-native"]').getAttribute('aria-current')`), 'true');
  await waitFor(`getComputedStyle(document.getElementById('preview-related')).display === 'none'`, 'single-feature screen hides the candidate list');
  assert.equal(await evaluate(`getComputedStyle(document.getElementById('preview-banner')).display`), 'none', 'empty reload banner stays hidden');
  await shot('preview-category-picker');
  await inFrame(`document.getElementById('category-picker').close()`);
  step('Preview Feature opened Add with the category picker');

  await preview('R04');
  await waitFor(`document.getElementById('preview').contentWindow.eval("state.page === 'edit' && state.parent !== undefined")`, 'Edit screen through Details');
  await preview('ux-improvements');
  await waitFor(`document.getElementById('preview').contentWindow.eval("state.view === 'decide'")`, 'UX decision lab view');
  step('Edit (through Details) and the UX decision lab open from the dashboard');

  // DD07: navigating inside the prototype selects the matching feature; shared screens list candidates.
  await inFrame(`navigate('calendar')`);
  await waitFor(`document.getElementById('active-screen').textContent === 'Calendar'`, 'Calendar reported');
  await waitFor(`document.querySelector('.feature[data-id="f03-calendar"]').getAttribute('aria-current') === 'true'`, 'f03-calendar selected from the prototype');
  await inFrame(`navigate('dues')`);
  await waitFor(`!document.getElementById('preview-related').hidden && document.querySelectorAll('#preview-related button').length >= 3`, 'shared-screen candidates listed');
  assert.equal(await evaluate(`document.querySelector('.feature[aria-current="true"]').dataset.id`), 'f03-calendar', 'a shared screen must not guess a feature');
  step('prototype navigation followed; shared screen shows candidates without guessing');

  // DD06 guard: an unsaved draft makes the prototype ask; "Keep editing" cancels the dashboard request.
  await preview('R02');
  await waitFor(`document.getElementById('preview').contentWindow.eval("state.page === 'add'")`, 'Add opened');
  await inFrame(`(() => { const t = document.getElementById('due-title'); t.value = 'Unsaved draft'; t.dispatchEvent(new Event('input', { bubbles: true })); })()`);
  await waitFor(`document.getElementById('screen-flags').textContent.includes('unsaved sample draft')`, 'dirty draft reported');
  await preview('f02-dashboard');
  await waitFor(`document.getElementById('preview').contentWindow.eval("document.getElementById('confirmation').open")`, 'prototype asks before discarding');
  await waitFor(`document.getElementById('preview-status').textContent.includes('Waiting for your answer')`, 'dashboard waits for the dialog');
  await inFrame(`document.querySelector('#confirmation button[value=cancel]').click()`);
  await waitFor(`document.getElementById('preview-status').textContent.includes('not shown (cancelled)')`, 'cancellation reported');
  assert.equal(await inFrame(`state.page`), 'add');
  assert.equal(await inFrame(`document.getElementById('due-title').value`), 'Unsaved draft', 'the draft survives');
  step('unsaved-draft guard respected; cancellation reported, draft kept');

  // DD08: forged or wrong-nonce messages are ignored on both sides.
  const before = await text('active-screen');
  await evaluate(`window.postMessage({ channel: 'duewise-devdash', version: 1, type: 'state', nonce: 'x', payload: { view: 'explore', screen: 'settings' } }, '*')`);
  await evaluate(`document.getElementById('preview').contentWindow.postMessage({ channel: 'duewise-devdash', version: 1, type: 'navigate', nonce: 'wrong-nonce-0000000000', requestId: 'r1', payload: { screen: 'settings' } }, location.origin)`);
  await delay(400);
  assert.equal(await text('active-screen'), before);
  assert.equal(await inFrame(`state.page`), 'add');
  step('forged and wrong-nonce messages ignored');

  // DD09: a visual review is stored with the prototype fingerprint and shown.
  await pick('f03-calendar');
  await evaluate(`document.getElementById('review-note').value = 'Browser check review'`);
  await evaluate(`[...document.querySelectorAll('#inspector button')].find(b => b.textContent === 'Approve visual').click()`);
  await waitFor(`document.getElementById('inspector').textContent.includes('Approved') && document.getElementById('inspector').textContent.includes('Browser check review')`, 'review shown after save');
  const reviews = fs.readFileSync(path.join(records, 'reviews.jsonl'), 'utf8').trim().split('\n').map(l => JSON.parse(l));
  assert.equal(reviews.at(-1).featureId, 'f03-calendar');
  assert.match(reviews.at(-1).fingerprint, /^sha256:/);
  step('visual review saved to the temporary records and displayed');

  // DD05/DD11: a dashboard data change re-renders without reloading the prototype.
  appendJsonl(path.join(config.abs.runtime, 'sessions.jsonl'), { kind: 'session', agent: 'codex', sessionId: 'browser-check', event: 'checkpoint', source: 'cli', task: 'Browser check task', at: new Date().toISOString() });
  await waitFor(`document.getElementById('sessions').textContent.includes('Browser check task')`, 'session shown live');
  assert.match(await text('sessions'), /not confirmed running/);
  assert.equal(await inFrame('performance.timeOrigin'), origin, 'the prototype iframe was not reloaded');
  step('live session update rendered; prototype not reloaded');

  // DD14: search and filters narrow the list; DD04: new-tab link opens the standalone prototype.
  // Search covers IDs, names, gate text and open tasks; PC08 is a gate ID found only in ph-categories-native.
  await evaluate(`(() => { const s = document.getElementById('search'); s.value = 'pc08'; s.dispatchEvent(new Event('input')); })()`);
  await waitFor(`JSON.stringify([...document.querySelectorAll('.feature')].map(b => b.dataset.id)) === '["ph-categories-native"]'`, 'search by gate ID finds exactly one feature');
  await evaluate(`(() => { const s = document.getElementById('search'); s.value = ''; s.dispatchEvent(new Event('input')); const f = document.getElementById('status-filter'); f.value = 'verified'; f.dispatchEvent(new Event('change')); })()`);
  await waitFor(`[...document.querySelectorAll('.feature')].length > 0 && [...document.querySelectorAll('.feature .badge')].every(b => b.textContent === 'Verified')`, 'status filter shows only Verified');
  await evaluate(`(() => { const f = document.getElementById('status-filter'); f.value = ''; f.dispatchEvent(new Event('change')); })()`);
  const tabHref = await evaluate(`document.getElementById('open-tab').getAttribute('href')`);
  assert.match(tabHref, /^\/preview\/prototypes\/design-exploration\/index\.html(#.*)?$/, 'new-tab link is the standalone prototype without the embed flag');
  assert.equal((await fetch(new URL(tabHref, server.url))).status, 200);
  step('search by gate ID, status filter and standalone new-tab link');
  // Headless browsers may refuse real full screen; report that instead of claiming a pass.
  await send('Runtime.evaluate', { expression: `document.getElementById('fullscreen').click()`, userGesture: true, awaitPromise: true });
  await delay(500);
  if (await evaluate(`document.fullscreenElement?.id === 'preview-pane'`)) { await evaluate(`document.exitFullscreen()`); step('full-screen preview'); }
  else console.log('note - full screen not entered in this headless browser; check the Full screen button manually');

  // DD04/DD14: viewport presets, keyboard list navigation and a narrow window.
  await evaluate(`(() => { const v = document.getElementById('viewport'); v.value = '390'; v.dispatchEvent(new Event('change')); })()`);
  assert.equal(await evaluate(`document.getElementById('preview').getBoundingClientRect().width`), 390);
  await evaluate(`document.querySelector('.feature[data-id="R01"]').focus()`);
  await send('Input.dispatchKeyEvent', { type: 'keyDown', key: 'ArrowDown', code: 'ArrowDown', windowsVirtualKeyCode: 40 });
  await waitFor(`document.querySelector('.feature[aria-current="true"]').dataset.id === 'R02'`, 'ArrowDown moves the selection');
  await send('Emulation.setDeviceMetricsOverride', { width: 390, height: 900, deviceScaleFactor: 1, mobile: true });
  await delay(300);
  assert.equal(await evaluate(`document.documentElement.scrollWidth <= innerWidth + 1`), true, 'no horizontal page scroll at 390 px');
  await shot('narrow-390');
  await evaluate(`(() => { const v = document.getElementById('viewport'); v.value = 'fill'; v.dispatchEvent(new Event('change')); })()`);
  step('viewport preset, keyboard navigation and 390 px layout');

  assert.deepEqual(errors, [], 'Browser errors (including CSP violations)');
  step('no runtime or console errors');
  console.log(`Screenshots: ${work}`);
}

main().catch(error => { console.error(error.stack ?? error); process.exitCode = 1; }).finally(async () => {
  ws?.close(); browser?.kill(); await server?.close();
});
