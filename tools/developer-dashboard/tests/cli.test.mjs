import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
import { makeFixture } from './fixture.mjs';

const CLI = path.resolve(import.meta.dirname, '../cli.mjs');

test('CLI against a fixture repository', async t => {
  const fx = makeFixture();
  t.after(() => fx.cleanup());
  const run = (args, input) => spawnSync(process.execPath, [CLI, ...args, '--root', fx.root, '--config', fx.configPath], { input, encoding: 'utf8' });
  fx.write('docs/evidence.md', '# Evidence\n');

  await t.test('status is a short deterministic summary', () => {
    const r = run(['status']);
    assert.equal(r.status, 0, r.stderr);
    assert.match(r.stdout, /Approved scope, 3 features/);
    assert.match(r.stdout, /Tasks checked: 1 of 2/);
    assert.match(r.stdout, /0 of 4 \(0%\).*no record 4/); // R01, R02 and alpha's two scenarios
    assert.ok(r.stdout.split('\n').length <= 20);
  });

  await t.test('check passes, then reports a bad mapping', () => {
    assert.equal(run(['check']).status, 0);
    const file = path.join(fx.root, 'tools/developer-dashboard/records/mappings.json');
    const original = fs.readFileSync(file, 'utf8');
    const broken = JSON.parse(original);
    broken.features.ghost = { phase: 'Core', prototype: { screen: 'nowhere' } };
    fs.writeFileSync(file, JSON.stringify(broken));
    const r = run(['check']);
    assert.equal(r.status, 1);
    assert.match(r.stdout, /ghost: no such feature/);
    assert.match(r.stdout, /unknown screen nowhere/);
    fs.writeFileSync(file, original);
  });

  await t.test('verify records evidence; all gates passing makes the feature Verified', () => {
    const base = ['--feature', 'alpha', '--result', 'pass', '--observer', 'agent', '--evidence', 'docs/evidence.md', '--env', 'fixture', '--commit', 'none'];
    assert.equal(run(['verify', '--gate', 'AL01', ...base]).status, 0);
    let r = run(['feature', 'alpha']);
    assert.match(r.stdout, /Status: In Progress/);
    assert.equal(run(['verify', '--gate', 'AL02', ...base]).status, 0);
    r = run(['feature', 'alpha']);
    assert.match(r.stdout, /Status: Verified/);
    const bad = run(['verify', '--gate', 'ZZ01', ...base]);
    assert.equal(bad.status, 2);
    assert.match(bad.stderr, /not a gate of alpha/);
  });

  await t.test('block and unblock change the status through records only', () => {
    assert.equal(run(['block', '--feature', 'R02', '--reason', 'Waiting for a device']).status, 0);
    assert.match(run(['feature', 'R02']).stdout, /Status: Blocked/);
    assert.equal(run(['unblock', '--feature', 'R02']).status, 0);
    assert.doesNotMatch(run(['feature', 'R02']).stdout, /Status: Blocked/);
  });

  await t.test('hooks are silent, keep only lifecycle facts and never fail', () => {
    const payload = JSON.stringify({ session_id: 'abc-123', hook_event_name: 'SessionStart', transcript_path: 'C:/secret/transcript.jsonl', prompt: 'private words' });
    const r = run(['hook', 'claude-code'], payload);
    assert.equal(r.status, 0);
    assert.equal(r.stdout, '');
    const sessions = fs.readFileSync(path.join(fx.root, '.developer-dashboard/sessions.jsonl'), 'utf8');
    assert.match(sessions, /"sessionId":"abc-123"/);
    assert.match(sessions, /"event":"start"/);
    assert.doesNotMatch(sessions, /transcript|private words/);
    const garbage = run(['hook', 'claude-code', '--unknown-flag'], 'not json');
    assert.equal(garbage.status, 0);
    assert.equal(garbage.stdout, '');
    const codex = spawnSync(process.execPath, [CLI, 'hook', 'codex', '--root', fx.root, '--config', fx.configPath, JSON.stringify({ type: 'agent-turn-complete', 'last-assistant-message': 'secret reply' })], { encoding: 'utf8' });
    assert.equal(codex.status, 0);
    assert.doesNotMatch(fs.readFileSync(path.join(fx.root, '.developer-dashboard/sessions.jsonl'), 'utf8'), /secret reply/);
  });

  await t.test('session checkpoints and test runs are recorded', () => {
    assert.equal(run(['session', 'checkpoint', '--agent', 'codex', '--task', 'Build alpha', '--done', 'Model', '--pending', 'Page']).status, 0);
    assert.match(run(['status']).stdout, /codex: last signal \d+ min ago .*not confirmed running — Build alpha/);
    const r = run(['test', 'ok']);
    assert.equal(r.status, 0, r.stdout + r.stderr);
    assert.match(r.stdout, /Recorded ok: pass/);
    assert.match(run(['status']).stdout, /Tests: ok pass/);
    assert.equal(run(['test', 'missing']).status, 2);
  });
});
