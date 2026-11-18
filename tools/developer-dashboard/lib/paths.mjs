// Repository path helpers. Every file the dashboard reads or serves goes through
// these checks, so a crafted path cannot escape the repository.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

export const toolDir = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
export const defaultRoot = path.resolve(toolDir, '..', '..');

/** Repository-relative path with forward slashes, for display and records. */
export function rel(root, file) {
  return path.relative(root, file).split(path.sep).join('/');
}

/** True when `file` is `dir` itself or lies inside it (after resolving). */
export function isInside(dir, file) {
  const r = path.relative(path.resolve(dir), path.resolve(file));
  return r === '' || (!r.startsWith('..') && !path.isAbsolute(r));
}

/**
 * Resolve a repository-relative path. Returns null for absolute input, escapes
 * from the root, or symlinks whose real target is outside the root.
 */
export function resolveInside(root, relative) {
  if (typeof relative !== 'string' || relative === '' || relative.includes('\0')) return null;
  if (path.isAbsolute(relative) || /^[a-zA-Z]:/.test(relative)) return null;
  const file = path.resolve(root, relative);
  if (!isInside(root, file)) return null;
  try {
    const real = fs.realpathSync(file);
    if (!isInside(fs.realpathSync(root), real)) return null;
  } catch {
    // A missing file is fine here; callers decide whether it must exist.
  }
  return file;
}

export function readText(file) {
  return fs.readFileSync(file, 'utf8').replace(/^﻿/, '');
}
