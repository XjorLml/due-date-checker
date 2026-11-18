// A content hash of the prototype's source files. Visual reviews store it, so a
// later prototype edit marks the review stale instead of silently keeping it.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

const cache = new Map(); // file -> { mtimeMs, size, hash }

function walk(dir, ignore, out) {
  let entries;
  try { entries = fs.readdirSync(dir, { withFileTypes: true }); } catch { return; }
  for (const entry of entries) {
    if (entry.name.startsWith('.') || ignore.includes(entry.name)) continue;
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) walk(full, ignore, out);
    else if (entry.isFile()) out.push(full);
  }
}

export function previewFiles(roots, ignore) {
  const files = [];
  for (const root of roots) walk(root, ignore, files);
  return files.sort();
}

export function previewFingerprint(root, roots, ignore) {
  const total = crypto.createHash('sha256');
  for (const file of previewFiles(roots, ignore)) {
    const stat = fs.statSync(file);
    let entry = cache.get(file);
    if (!entry || entry.mtimeMs !== stat.mtimeMs || entry.size !== stat.size) {
      entry = { mtimeMs: stat.mtimeMs, size: stat.size, hash: crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex') };
      cache.set(file, entry);
    }
    total.update(path.relative(root, file).split(path.sep).join('/')).update('\0').update(entry.hash).update('\n');
  }
  return 'sha256:' + total.digest('hex').slice(0, 16);
}
