#!/usr/bin/env node
// Local developer dashboard server. Loopback only, read-mostly: it serves the
// dashboard page, a validated snapshot, server-sent events, the prototype files
// and Markdown as plain text. The only write is a visual review from the page.
import fs from 'node:fs';
import http from 'node:http';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { defaultRoot, resolveInside, readText, toolDir, isInside } from './lib/paths.mjs';
import { loadConfig } from './lib/config.mjs';
import { buildSnapshot, deriveFeatures, recordFiles } from './lib/snapshot.mjs';
import { previewFingerprint } from './lib/fingerprint.mjs';
import { appendJsonl, validateReview } from './lib/records.mjs';
import { gitDir } from './lib/git.mjs';

const PUBLIC = path.join(toolDir, 'public');
const PUBLIC_FILES = { '/': 'index.html', '/index.html': 'index.html', '/app.js': 'app.js', '/styles.css': 'styles.css' };
const TYPES = {
  '.html': 'text/html; charset=utf-8', '.js': 'text/javascript; charset=utf-8', '.css': 'text/css; charset=utf-8',
  '.json': 'application/json; charset=utf-8', '.svg': 'image/svg+xml', '.png': 'image/png', '.webp': 'image/webp',
  '.ico': 'image/x-icon', '.ttf': 'font/ttf', '.woff': 'font/woff', '.woff2': 'font/woff2', '.md': 'text/plain; charset=utf-8',
};
const DASHBOARD_CSP = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-src 'self'; object-src 'none'; base-uri 'none'; form-action 'none'; frame-ancestors 'none'";

function send(res, status, body, headers = {}) {
  res.writeHead(status, { 'X-Content-Type-Options': 'nosniff', 'Referrer-Policy': 'no-referrer', 'Cache-Control': 'no-store', ...headers });
  res.end(body);
}
const json = (res, status, value) => send(res, status, JSON.stringify(value), { 'Content-Type': 'application/json; charset=utf-8' });

function debounce(fn, ms) {
  let timer = null;
  return (...args) => { clearTimeout(timer); timer = setTimeout(() => fn(...args), ms); timer.unref?.(); };
}

function watch(dir, options, onChange) {
  try {
    fs.mkdirSync(dir, { recursive: true });
    const watcher = fs.watch(dir, options, (_event, name) => onChange(String(name ?? '').split(path.sep).join('/')));
    watcher.on('error', () => {});
    return watcher;
  } catch { return null; }
}

/** Resolve a /preview/ path to a servable prototype file, or null. */
export function previewFile(config, requestPath) {
  let relative;
  try { relative = decodeURIComponent(requestPath); } catch { return null; }
  const parts = relative.split('/');
  if (parts.some(p => p === '' || p === '..' || p.startsWith('.') || (config.preview.ignore ?? []).includes(p))) return null;
  const file = resolveInside(config.abs.root, relative);
  if (!file || !config.abs.previewRoots.some(r => isInside(r, file))) return null;
  if (!TYPES[path.extname(file).toLowerCase()] || path.extname(file).toLowerCase() === '.md') return null;
  try { return fs.statSync(file).isFile() ? file : null; } catch { return null; }
}

/** Resolve a /doc/ path to a Markdown file under the configured document roots, or null. */
export function documentFile(config, requestPath) {
  let relative;
  try { relative = decodeURIComponent(requestPath); } catch { return null; }
  if (relative.split('/').some(p => p === '' || p === '..' || p.startsWith('.')) || !relative.toLowerCase().endsWith('.md')) return null;
  const file = resolveInside(config.abs.root, relative);
  const roots = (config.documents ?? []).map(d => resolveInside(config.abs.root, d)).filter(Boolean);
  if (!file || !roots.some(r => isInside(r, file))) return null;
  try { return fs.statSync(file).isFile() ? file : null; } catch { return null; }
}

export async function startServer({ root = defaultRoot, config, port, host, quiet = true } = {}) {
  config ??= loadConfig(root);
  const bindHost = host ?? config.server?.host ?? '127.0.0.1';
  if (!['127.0.0.1', 'localhost', '::1'].includes(bindHost)) throw new Error('The dashboard only binds to a loopback address.');
  const token = crypto.randomBytes(24).toString('hex');
  const clients = new Set();
  let snapshot = null, snapshotJson = '{}', version = 0, digest = '', lastError = null;
  let fingerprint = previewFingerprint(config.abs.root, config.abs.previewRoots, config.preview.ignore ?? []);
  let allowedHosts = new Set();

  const broadcast = (event, data) => {
    const frame = `event: ${event}\ndata: ${JSON.stringify(data)}\n\n`;
    for (const res of clients) res.write(frame);
  };

  async function rebuild() {
    try {
      const next = await buildSnapshot(config);
      const { generatedAt, ...stable } = next;
      const nextDigest = crypto.createHash('sha256').update(JSON.stringify(stable)).digest('hex');
      lastError = null;
      if (nextDigest === digest) return;
      digest = nextDigest;
      version++;
      snapshot = { ...next, version };
      snapshotJson = JSON.stringify(snapshot);
      broadcast('snapshot', { version });
    } catch (error) {
      lastError = error.message;
      broadcast('problem', { message: `Snapshot rebuild failed: ${error.message}` });
    }
  }
  const scheduleRebuild = debounce(rebuild, 250);
  const previewChanged = debounce(changed => {
    const next = previewFingerprint(config.abs.root, config.abs.previewRoots, config.preview.ignore ?? []);
    const list = [...changed].slice(0, 20);
    changed.clear();
    if (next === fingerprint) return;
    fingerprint = next;
    broadcast('preview', { fingerprint, files: list });
    rebuild();
  }, 300);
  const pendingPreview = new Set();

  await rebuild();

  const watchers = [
    watch(config.abs.specs, { recursive: true }, name => { if (name.endsWith('.md')) scheduleRebuild(); }),
    watch(path.dirname(config.abs.requirements), { recursive: true }, () => scheduleRebuild()),
    watch(config.abs.records, {}, () => scheduleRebuild()),
    watch(config.abs.runtime, {}, () => scheduleRebuild()),
    ...config.abs.previewRoots.map(r => watch(r, { recursive: true }, name => {
      const parts = name.split('/');
      if (parts.some(p => p.startsWith('.') || (config.preview.ignore ?? []).includes(p))) return;
      pendingPreview.add(path.relative(config.abs.root, path.join(r, name)).split(path.sep).join('/'));
      previewChanged(pendingPreview);
    })),
  ];
  const gd = await gitDir(config.abs.root);
  if (gd) {
    const gitChanged = debounce(rebuild, 1000);
    watchers.push(watch(gd, {}, name => { if (['HEAD', 'index', 'ORIG_HEAD', 'FETCH_HEAD'].includes(name)) gitChanged(); }));
    watchers.push(watch(path.join(gd, 'refs'), { recursive: true }, () => gitChanged()));
  }

  const files = recordFiles(config);
  const server = http.createServer(async (req, res) => {
    try {
      if (!allowedHosts.has(String(req.headers.host ?? '').toLowerCase())) return send(res, 403, 'Forbidden host');
      const url = new URL(req.url, 'http://local');
      const pathname = url.pathname;

      if (req.method === 'POST' && pathname === '/api/reviews') {
        const origin = String(req.headers.origin ?? '');
        if (![...allowedHosts].some(h => origin === `http://${h}`)) return json(res, 403, { error: 'Cross-origin write refused.' });
        if (req.headers['x-dashboard-token'] !== token) return json(res, 403, { error: 'Missing or wrong dashboard token.' });
        if (!String(req.headers['content-type'] ?? '').startsWith('application/json')) return json(res, 415, { error: 'Send JSON.' });
        const body = await new Promise((resolve, reject) => {
          let data = '';
          req.setEncoding('utf8');
          req.on('data', chunk => { data += chunk; if (data.length > 16_384) { reject(new Error('Body too large')); req.destroy(); } });
          req.on('end', () => resolve(data));
          req.on('error', reject);
        });
        let input;
        try { input = JSON.parse(body); } catch { return json(res, 400, { error: 'Invalid JSON.' }); }
        const features = snapshot?.features ?? deriveFeatures(config);
        const feature = features.find(f => f.id === input.featureId);
        const screen = feature?.prototype ? [feature.prototype.view !== 'explore' && feature.prototype.view, feature.prototype.screen, feature.prototype.overlay].filter(Boolean).join('/') : undefined;
        let record;
        try { record = validateReview({ featureId: input.featureId, decision: input.decision, note: input.note, screen, fingerprint, reviewer: 'user' }, { features }); }
        catch (error) { return json(res, 400, { error: error.message }); }
        appendJsonl(files.reviews, record);
        await rebuild();
        return json(res, 201, { ok: true, review: record });
      }
      if (req.method !== 'GET' && req.method !== 'HEAD') return send(res, 405, 'Method not allowed', { Allow: 'GET, HEAD' });

      if (PUBLIC_FILES[pathname]) {
        const name = PUBLIC_FILES[pathname];
        let body = fs.readFileSync(path.join(PUBLIC, name), 'utf8');
        if (name === 'index.html') body = body.replace('content="__TOKEN__"', `content="${token}"`);
        return send(res, 200, body, { 'Content-Type': TYPES[path.extname(name)], 'Content-Security-Policy': DASHBOARD_CSP });
      }
      if (pathname === '/api/snapshot') {
        return send(res, snapshot ? 200 : 503, snapshot ? snapshotJson : JSON.stringify({ error: lastError ?? 'Not ready' }), { 'Content-Type': 'application/json; charset=utf-8' });
      }
      if (pathname === '/api/events') {
        res.writeHead(200, { 'Content-Type': 'text/event-stream; charset=utf-8', 'Cache-Control': 'no-store', Connection: 'keep-alive', 'X-Content-Type-Options': 'nosniff' });
        res.write(`retry: 2000\nevent: hello\ndata: ${JSON.stringify({ version, fingerprint })}\n\n`);
        clients.add(res);
        const ping = setInterval(() => res.write(': ping\n\n'), 25_000);
        ping.unref?.();
        req.on('close', () => { clearInterval(ping); clients.delete(res); });
        return;
      }
      if (pathname.startsWith('/preview/')) {
        const file = previewFile(config, pathname.slice('/preview/'.length));
        if (!file) return send(res, 404, 'Not found');
        return send(res, 200, fs.readFileSync(file), { 'Content-Type': TYPES[path.extname(file).toLowerCase()], 'Content-Security-Policy': "frame-ancestors 'self'", 'Cache-Control': 'no-cache' });
      }
      if (pathname.startsWith('/doc/')) {
        const file = documentFile(config, pathname.slice('/doc/'.length));
        if (!file) return send(res, 404, 'Not found');
        return send(res, 200, readText(file), { 'Content-Type': 'text/plain; charset=utf-8', 'Content-Security-Policy': "default-src 'none'; frame-ancestors 'self'" });
      }
      return send(res, 404, 'Not found');
    } catch (error) {
      if (!res.headersSent) send(res, 500, 'Internal error');
      if (!quiet) console.error(error);
    }
  });

  const wantedPort = port ?? config.server?.port ?? 4317;
  await new Promise((resolve, reject) => { server.once('error', reject); server.listen(wantedPort, bindHost, resolve); });
  const actualPort = server.address().port;
  allowedHosts = new Set([`127.0.0.1:${actualPort}`, `localhost:${actualPort}`, `[::1]:${actualPort}`]);
  const url = `http://127.0.0.1:${actualPort}/`;

  return {
    server, url, port: actualPort, token,
    rebuild,
    get version() { return version; },
    close: () => new Promise(resolve => {
      for (const w of watchers) w?.close();
      for (const c of clients) c.end();
      server.close(() => resolve());
      server.closeAllConnections?.();
    }),
  };
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const portArg = process.argv.indexOf('--port');
  startServer({ port: portArg > -1 ? Number(process.argv[portArg + 1]) : undefined, quiet: false })
    .then(s => console.log(`Developer dashboard: ${s.url}  (Ctrl+C to stop)`))
    .catch(error => { console.error(error.message); process.exitCode = 1; });
}
