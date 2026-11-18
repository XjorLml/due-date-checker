import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { spawn } from 'node:child_process';
import { pathToFileURL } from 'node:url';
import { appendJsonl, readJsonl, validateVerification, validateReview, validateSession, validateBlocker } from '../lib/records.mjs';

const temp = () => fs.mkdtempSync(path.join(os.tmpdir(), 'devdash-rec-'));
const features = [{ id: 'alpha', gates: [{ id: 'AL01' }, { id: 'AL02' }] }];
const good = { featureId: 'alpha', gate: 'AL01', result: 'pass', observer: 'agent', evidence: 'docs/e.md#x', environment: 'Windows', observedAt: '2026-11-10' };

test('append and read round-trip; a torn last line is reported, not lost or merged', () => {
  const dir = temp();
  const file = path.join(dir, 'r.jsonl');
  appendJsonl(file, { a: 1 });
  fs.appendFileSync(file, '{"a":2'); // simulated interrupted write
  let read = readJsonl(file);
  assert.deepEqual(read.items, [{ a: 1 }]);
  assert.match(read.problems[0].error, /incomplete last line/);
  appendJsonl(file, { a: 3 });
  read = readJsonl(file);
  assert.deepEqual(read.items, [{ a: 1 }, { a: 3 }]);
  assert.equal(read.problems.length, 1, 'the torn line stays reported as invalid');
  assert.deepEqual(readJsonl(path.join(dir, 'missing.jsonl')), { items: [], problems: [] });
  fs.rmSync(dir, { recursive: true, force: true });
});

test('parallel writers from separate processes each get one intact line', async () => {
  const dir = temp();
  const file = path.join(dir, 'p.jsonl');
  const lib = pathToFileURL(path.resolve(import.meta.dirname, '../lib/records.mjs')).href;
  const writers = Array.from({ length: 8 }, (_, i) => new Promise((resolve, reject) => {
    const child = spawn(process.execPath, ['--input-type=module', '-e', `import { appendJsonl } from ${JSON.stringify(lib)}; for (let j = 0; j < 5; j++) appendJsonl(${JSON.stringify(file)}, { writer: ${i}, j, pad: 'x'.repeat(2000) });`], { stdio: 'inherit' });
    child.on('exit', code => (code === 0 ? resolve() : reject(new Error(`writer ${i} exit ${code}`))));
  }));
  await Promise.all(writers);
  const { items, problems } = readJsonl(file);
  assert.equal(problems.length, 0);
  assert.equal(items.length, 40);
  assert.equal(fs.existsSync(file + '.lock'), false);
  fs.rmSync(dir, { recursive: true, force: true });
});

test('verification records require a real gate, attribution and existing evidence', () => {
  const exists = p => p === 'docs/e.md';
  assert.equal(validateVerification(good, { features, exists }).gate, 'AL01');
  assert.equal(validateVerification({ ...good, evidence: 'https://github.com/x/actions/runs/1' }, { features, exists }).result, 'pass');
  const bad = (patch, pattern) => assert.throws(() => validateVerification({ ...good, ...patch }, { features, exists }), pattern);
  bad({ featureId: 'nope' }, /unknown feature/);
  bad({ gate: 'AL09' }, /not a gate of alpha/);
  bad({ result: 'passed' }, /result must be one of/);
  bad({ observer: 'claude' }, /observer must be one of/);
  bad({ evidence: 'docs/missing.md' }, /not an existing repository path/);
  bad({ evidence: 'http://insecure.example' }, /not an existing repository path/);
  bad({ observedAt: 'yesterday' }, /observedAt/);
  bad({ environment: '' }, /environment is required/);
});

test('reviews, blockers and sessions validate their own fields', () => {
  assert.equal(validateReview({ featureId: 'alpha', decision: 'approved', fingerprint: 'sha256:1' }, { features }).reviewer, 'user');
  assert.throws(() => validateReview({ featureId: 'alpha', decision: 'ok', fingerprint: 'x' }, { features }), /decision/);
  assert.throws(() => validateBlocker({ featureId: 'alpha', action: 'block' }, { features }), /reason is required/);
  assert.equal(validateBlocker({ featureId: 'alpha', action: 'unblock' }, { features }).action, 'unblock');
  assert.throws(() => validateSession({ agent: 'Claude Code', sessionId: 's', event: 'start' }), /agent must be/);
  assert.throws(() => validateSession({ agent: 'codex', sessionId: '../x', event: 'start' }), /sessionId/);
  assert.equal(validateSession({ agent: 'codex', sessionId: 's1', event: 'signal', source: 'hook' }).source, 'hook');
});
