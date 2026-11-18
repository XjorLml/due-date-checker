// Read-only Git metadata through fixed commands. Nothing here writes to the
// repository, and no caller-supplied text becomes a command.
import { execFile } from 'node:child_process';

function git(root, args, { allowFail = false } = {}) {
  return new Promise((resolve, reject) => {
    execFile('git', args, { cwd: root, timeout: 8000, maxBuffer: 8 * 1024 * 1024, windowsHide: true }, (error, stdout) => {
      if (error) return allowFail ? resolve(null) : reject(error);
      resolve(stdout);
    });
  });
}

/** Paths from `git status --porcelain=v1 -z` (renames report the new path). */
export function parsePorcelain(output) {
  const entries = output.split('\0').filter(Boolean);
  const files = [];
  for (let i = 0; i < entries.length; i++) {
    const entry = entries[i];
    const code = entry.slice(0, 2);
    files.push({ code: code.trim() || '?', path: entry.slice(3) });
    if (code[0] === 'R' || code[0] === 'C') i++; // skip the original path
  }
  return files;
}

export async function changedFiles(root) {
  const out = await git(root, ['status', '--porcelain=v1', '-z', '--untracked-files=all'], { allowFail: true });
  return out === null ? null : parsePorcelain(out);
}

export async function gitInfo(root) {
  const [branch, head, status, counts, recent] = await Promise.all([
    git(root, ['rev-parse', '--abbrev-ref', 'HEAD'], { allowFail: true }),
    git(root, ['log', '-1', '--format=%H%x1f%h%x1f%s%x1f%cI'], { allowFail: true }),
    changedFiles(root),
    git(root, ['rev-list', '--left-right', '--count', '@{upstream}...HEAD'], { allowFail: true }),
    git(root, ['log', '-8', '--format=%h%x1f%s%x1f%cI%x1f%an'], { allowFail: true }),
  ]);
  if (branch === null) return { available: false };
  const [sha, short, subject, date] = (head ?? '').trim().split('\x1f');
  const [behind, ahead] = counts ? counts.trim().split(/\s+/).map(Number) : [null, null];
  return {
    available: true,
    branch: branch.trim(),
    head: sha ? { sha, short, subject, date } : null,
    upstream: counts ? { ahead, behind } : null,
    changed: status ?? [],
    recent: (recent ?? '').trim().split('\n').filter(Boolean).map(line => {
      const [shortSha, subjectLine, when, author] = line.split('\x1f');
      return { short: shortSha, subject: subjectLine, date: when, author };
    }),
  };
}

/** Latest commit date per path under the given pathspecs (one git call). */
export async function lastCommitDates(root, pathspecs) {
  const out = await git(root, ['log', '-n', '600', '--format=%x1e%cI', '--name-only', '--', ...pathspecs], { allowFail: true });
  const dates = new Map();
  for (const block of (out ?? '').split('\x1e')) {
    const [date, ...files] = block.trim().split('\n');
    for (const file of files) if (file && !dates.has(file)) dates.set(file, date);
  }
  return dates;
}

/** Files that differ between `commit` and the working tree (committed or not). */
export async function filesChangedSince(root, commit) {
  if (!/^[0-9a-f]{7,40}$/.test(commit)) return null;
  const out = await git(root, ['diff', '--name-only', commit, '--'], { allowFail: true });
  return out === null ? null : new Set(out.split('\n').map(s => s.trim()).filter(Boolean));
}

export async function gitDir(root) {
  const out = await git(root, ['rev-parse', '--absolute-git-dir'], { allowFail: true });
  return out ? out.trim() : null;
}

export async function headCommit(root) {
  const out = await git(root, ['rev-parse', 'HEAD'], { allowFail: true });
  return out ? out.trim() : null;
}
