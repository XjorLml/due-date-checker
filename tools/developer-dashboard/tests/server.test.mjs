import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import http from 'node:http';
import path from 'node:path';
import { makeFixture } from './fixture.mjs';
import { loadConfig } from '../lib/config.mjs';
import { startServer } from '../server.mjs';

// Raw request so the path is sent exactly as written (no client-side normalization).
function raw(port, { method = 'GET', path: p, headers = {}, body } = {}) {
  return new Promise((resolve, reject) => {
    const req = http.request({ host: '127.0.0.1', port, method, path: p, headers: { Host: `127.0.0.1:${port}`, ...headers } }, res => {
      let data = '';
      res.setEncoding('utf8');
      res.on('data', c => { data += c; });
      res.on('end', () => resolve({ status: res.statusCode, headers: res.headers, body: data }));
    });
    req.on('error', reject);
    if (body) req.write(body);
    req.end();
  });
}

function sse(port) {
  const events = [];
  const waiters = [];
  const req = http.get({ host: '127.0.0.1', port, path: '/api/events', headers: { Host: `127.0.0.1:${port}` } }, res => {
    let buffer = '';
    res.setEncoding('utf8');
    res.on('data', chunk => {
      buffer += chunk;
      let i;
      while ((i = buffer.indexOf('\n\n')) !== -1) {
        const frame = buffer.slice(0, i); buffer = buffer.slice(i + 2);
        const event = /^event: (.+)$/m.exec(frame)?.[1];
        const data = /^data: (.+)$/m.exec(frame)?.[1];
        if (!event) continue;
        const e = { event, data: data ? JSON.parse(data) : null };
        events.push(e);
        for (const w of [...waiters]) if (w.match(e)) { waiters.splice(waiters.indexOf(w), 1); w.resolve(e); }
      }
    });
  });
  return {
    events,
    next: (name, ms = 5000) => new Promise((resolve, reject) => {
      const hit = events.find(e => e.event === name && !e.seen);
      if (hit) { hit.seen = true; return resolve(hit); }
      const timer = setTimeout(() => reject(new Error(`no ${name} event within ${ms} ms`)), ms);
      waiters.push({ match: e => e.event === name, resolve: e => { clearTimeout(timer); e.seen = true; resolve(e); } });
    }),
    close: () => req.destroy(),
  };
}

test('dashboard server: snapshot, security boundaries, reviews and live events', async t => {
  const fx = makeFixture();
  const config = loadConfig(fx.root, fx.configPath);
  const server = await startServer({ root: fx.root, config, port: 0 });
  const { port } = server;
  t.after(async () => { await server.close(); fx.cleanup(); });

  await t.test('snapshot derives features from the PRD and spec folders', async () => {
    const res = await raw(port, { path: '/api/snapshot' });
    assert.equal(res.status, 200);
    const s = JSON.parse(res.body);
    assert.deepEqual(s.features.map(f => f.id).sort(), ['R01', 'R02', 'alpha', 'beta']);
    const alpha = s.features.find(f => f.id === 'alpha');
    assert.deepEqual(alpha.gates.map(g => g.id), ['AL01', 'AL02']);
    assert.equal(alpha.status.value, 'in_progress');
    assert.equal(s.features.find(f => f.id === 'R01').status.value, 'in_review');
    assert.equal(s.focus.text, 'Alpha slice, 2026-11-10');
    assert.equal(s.overview.approved.features, 3);
    assert.deepEqual(s.problems, []);
  });

  await t.test('the page carries a per-process token and a strict content policy', async () => {
    const res = await raw(port, { path: '/' });
    assert.equal(res.status, 200);
    assert.match(res.body, new RegExp(`content="${server.token}"`));
    assert.match(res.headers['content-security-policy'], /script-src 'self'/);
    assert.match(res.headers['content-security-policy'], /frame-ancestors 'none'/);
  });

  await t.test('only allowlisted files are served', async () => {
    const status = async p => (await raw(port, { path: p })).status;
    assert.equal(await status('/preview/proto/app/index.html'), 200);
    assert.equal(await status('/preview/proto/app/../../secret.txt'), 404);
    assert.equal(await status('/preview/proto/app/%2e%2e/%2e%2e/secret.txt'), 404);
    assert.equal(await status('/preview/proto/app/tests/hidden.js'), 404);
    assert.equal(await status('/preview/proto/app/.qa/shot.txt'), 404);
    assert.equal(await status('/preview/docs/PRD.md'), 404);
    assert.equal(await status('/doc/specs/alpha/spec.md'), 200);
    assert.equal(await status('/doc/secret.txt'), 404);
    assert.equal(await status('/doc/../secret.txt'), 404);
    assert.equal(await status('/doc/tools/developer-dashboard/records/mappings.json'), 404);
    assert.equal(await status('/nothing'), 404);
  });

  await t.test('a foreign Host header is refused (DNS rebinding)', async () => {
    assert.equal((await raw(port, { path: '/api/snapshot', headers: { Host: 'attacker.example' } })).status, 403);
  });

  await t.test('reviews need same origin, the token and valid input', async () => {
    const origin = `http://127.0.0.1:${port}`;
    const post = (headers, body) => raw(port, { method: 'POST', path: '/api/reviews', headers: { 'Content-Type': 'application/json', ...headers }, body: JSON.stringify(body) });
    const valid = { featureId: 'alpha', decision: 'approved', note: 'Looks right' };
    assert.equal((await post({ Origin: origin }, valid)).status, 403);
    assert.equal((await post({ Origin: 'http://evil.example', 'X-Dashboard-Token': server.token }, valid)).status, 403);
    assert.equal((await post({ Origin: origin, 'X-Dashboard-Token': server.token }, { ...valid, decision: 'ship-it' })).status, 400);
    const ok = await post({ Origin: origin, 'X-Dashboard-Token': server.token }, valid);
    assert.equal(ok.status, 201);
    const lines = fs.readFileSync(path.join(fx.root, 'tools/developer-dashboard/records/reviews.jsonl'), 'utf8').trim().split('\n');
    assert.equal(lines.length, 1);
    const review = JSON.parse(lines[0]);
    assert.equal(review.screen, 'add/category-picker');
    const s = JSON.parse((await raw(port, { path: '/api/snapshot' })).body);
    assert.equal(s.features.find(f => f.id === 'alpha').review.stale, false);
  });

  await t.test('document and prototype edits arrive as events; reviews go stale', async () => {
    const events = sse(port);
    t.after(() => events.close());
    await events.next('hello');
    fx.write('specs/alpha/tasks.md', '- [x] Write the model\n- [x] Write the page\n');
    await events.next('snapshot');
    let s = JSON.parse((await raw(port, { path: '/api/snapshot' })).body);
    assert.equal(s.features.find(f => f.id === 'alpha').status.value, 'in_review');
    fx.write('proto/app/app.js', 'console.log("v2");\n');
    const preview = await events.next('preview');
    assert.ok(preview.data.files.some(f => f.endsWith('app.js')));
    await events.next('snapshot');
    s = JSON.parse((await raw(port, { path: '/api/snapshot' })).body);
    assert.equal(s.features.find(f => f.id === 'alpha').review.stale, true);
  });
});

test('the server refuses to bind to a non-loopback address', async () => {
  const fx = makeFixture();
  try {
    await assert.rejects(startServer({ root: fx.root, config: loadConfig(fx.root, fx.configPath), port: 0, host: '0.0.0.0' }), /loopback/);
  } finally { fx.cleanup(); }
});
