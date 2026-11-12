import test from 'node:test';
import assert from 'node:assert/strict';
import { deriveStatus, gateState, summarize, summarizeSessions, latestBy } from '../lib/progress.mjs';

const tasks = (checked, open) => ({ checked, open, total: checked + open });

test('status precedence: blocker, then verified gates, then manual seed, then tasks', () => {
  const passing = [{ state: 'pass' }, { state: 'pass' }];
  assert.equal(deriveStatus({ gates: passing, tasks: tasks(1, 1), blocker: { reason: 'x' } }).value, 'blocked');
  assert.equal(deriveStatus({ gates: passing, tasks: tasks(1, 1), manual: { value: 'in_review', source: 's' } }).value, 'verified');
  assert.equal(deriveStatus({ gates: [{ state: 'pass' }, { state: 'none' }], tasks: tasks(0, 0), manual: { value: 'in_review', source: 's' } }).value, 'in_review');
  assert.equal(deriveStatus({ gates: [], tasks: tasks(0, 3) }).value, 'backlog');
  assert.equal(deriveStatus({ gates: [], tasks: tasks(2, 3) }).value, 'in_progress');
  assert.equal(deriveStatus({ gates: [{ state: 'none' }], tasks: tasks(4, 0) }).value, 'in_review');
  assert.equal(deriveStatus({ gates: [], tasks: null }).value, 'backlog');
});

test('a feature with no gates is never verified, even with all tasks checked', () => {
  assert.equal(deriveStatus({ gates: [], tasks: tasks(5, 0) }).value, 'in_review');
});

test('gate state comes from the latest record and turns stale when covered files change', () => {
  const changedSince = new Map([['abc1234', new Set(['src/a.cs', 'tools/x/y.js'])]]);
  assert.equal(gateState(undefined, { changedSince }).state, 'none');
  assert.equal(gateState({ result: 'fail' }, { changedSince }).state, 'fail');
  assert.deepEqual(gateState({ result: 'pass' }, { changedSince }), { state: 'pass', freshness: 'untracked' });
  assert.equal(gateState({ result: 'pass', commit: 'abc1234', files: ['src/a.cs'] }, { changedSince }).state, 'stale');
  assert.equal(gateState({ result: 'pass', commit: 'abc1234' }, { changedSince, relatedFiles: ['tools/x/'] }).state, 'stale');
  assert.equal(gateState({ result: 'pass', commit: 'abc1234', files: ['src/b.cs'] }, { changedSince }).state, 'pass');
  assert.equal(gateState({ result: 'pass', commit: 'fff0000', files: ['src/b.cs'] }, { changedSince }).freshness, 'unknown');
});

test('latest record wins by observed date, then recorded time', () => {
  const latest = latestBy([
    { k: 'a', observedAt: '2026-11-01', recordedAt: '2026-11-09T01:00:00Z', v: 1 },
    { k: 'a', observedAt: '2026-11-05', recordedAt: '2026-11-02T01:00:00Z', v: 2 },
    { k: 'a', observedAt: '2026-11-05', recordedAt: '2026-11-03T01:00:00Z', v: 3 },
  ], r => r.k);
  assert.equal(latest.get('a').v, 3);
});

test('summaries count gates and tasks without estimating; empty scope is n/a', () => {
  const f = (status, gates, t) => ({ status: { value: status }, gates: gates.map(state => ({ state })), tasks: t });
  const s = summarize([f('verified', ['pass', 'pass'], tasks(2, 0)), f('in_progress', ['none', 'fail'], tasks(1, 3))]);
  assert.equal(s.gates.total, 4);
  assert.equal(s.gates.pass, 2);
  assert.equal(s.percentVerified, 50);
  assert.equal(s.percentTasks, 50);
  assert.equal(s.byStatus.verified, 1);
  assert.equal(summarize([]).percentVerified, null);
});

test('a failing latest test run keeps the last pass visible beside it, not instead of it', async () => {
  const { latestTests } = await import('../lib/progress.mjs');
  const runs = [
    { suite: 'core', result: 'pass', finishedAt: '2026-11-09T01:00:00Z', commit: 'aaa1111' },
    { suite: 'core', result: 'fail', finishedAt: '2026-11-10T01:00:00Z', commit: 'bbb2222' },
    { suite: 'docs', result: 'pass', finishedAt: '2026-11-10T02:00:00Z', commit: 'bbb2222' },
  ];
  const [core, docs] = latestTests(runs);
  assert.equal(core.result, 'fail');
  assert.equal(core.lastPass.commit, 'aaa1111');
  assert.equal(docs.lastPass, undefined);
});

test('sessions show ended, recent or stale, and never claim an agent is running', () => {
  const now = Date.parse('2026-11-10T10:00:00Z');
  const events = [
    { agent: 'claude-code', sessionId: 'a', event: 'start', source: 'hook', at: '2026-11-10T09:58:00Z' },
    { agent: 'claude-code', sessionId: 'a', event: 'checkpoint', source: 'cli', at: '2026-11-10T09:59:00Z', task: 'Build', pending: ['UI', 'Tests'] },
    { agent: 'claude-code', sessionId: 'a', event: 'checkpoint', source: 'cli', at: '2026-11-10T09:59:30Z', done: ['UI'] },
    { agent: 'codex', sessionId: 'b', event: 'signal', source: 'hook', at: '2026-11-10T08:00:00Z' },
    { agent: 'codex', sessionId: 'c', event: 'end', source: 'cli', at: '2026-11-10T07:00:00Z' },
  ];
  const sessions = summarizeSessions(events, now, 30);
  const byId = Object.fromEntries(sessions.map(s => [s.sessionId, s]));
  assert.equal(byId.a.state, 'recent');
  assert.deepEqual(byId.a.done, ['UI']);
  assert.deepEqual(byId.a.pending, ['Tests']);
  assert.equal(byId.a.task, 'Build');
  assert.equal(byId.b.state, 'stale');
  assert.equal(byId.c.state, 'ended');
  assert.ok(sessions.every(s => s.state !== 'running'));
  assert.equal(sessions[0].sessionId, 'a');
});
