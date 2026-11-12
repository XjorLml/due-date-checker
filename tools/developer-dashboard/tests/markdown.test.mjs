import test from 'node:test';
import assert from 'node:assert/strict';
import { parseSpec, parseTasks, parseRequirements, slug, firstBoldIn, plain } from '../lib/markdown.mjs';

test('spec scenarios come from first table cells, skip references, duplicates and fences', () => {
  const spec = parseSpec([
    '# Alpha feature', 'Status: **Proposed**; awaiting approval.', '## Acceptance scenarios',
    '| ID | Scenario |', '|---|---|', '| AL01 / R02 | Save a [draft](x.md) |', '| **AL02** | Cancel |',
    '| R02 | reference |', '| TP01 | procedure reference |', '| AL01 | duplicate |',
    '```', '| AL99 | fenced |', '```',
  ].join('\n'));
  assert.equal(spec.title, 'Alpha feature');
  assert.equal(spec.status, 'Proposed; awaiting approval.');
  assert.deepEqual(spec.scenarios.map(s => s.id), ['AL01', 'AL02']);
  assert.equal(spec.scenarios[0].text, 'Save a draft');
  assert.equal(spec.scenarios[0].line, 6);
  assert.equal(spec.scenarios[0].anchor, 'acceptance-scenarios');
});

test('heading anchors match the documentation checker, including duplicates', () => {
  assert.equal(slug('Optional CI (R08)'), 'optional-ci-r08');
  assert.equal(slug('Review and proposed revision — 2026-11-09'), 'review-and-proposed-revision--2026-11-09');
  const spec = parseSpec('# T\n## Same\n| AA01 | one |\n## Same\n| AA02 | two |\n');
  assert.deepEqual(spec.scenarios.map(s => s.anchor), ['same', 'same-1']);
});

test('tasks are counted per section and fenced examples are ignored', () => {
  const tasks = parseTasks('# Tasks\n\n## Build\n- [x] One\n- [X] Two\n- [ ] Three `code`\n## Ship\n  * [ ] Nested four\n```\n- [ ] fenced\n```\n');
  assert.equal(tasks.checked, 2);
  assert.equal(tasks.open, 2);
  assert.deepEqual(tasks.sections.map(s => [s.section, s.checked, s.open]), [['Build', 2, 1], ['Ship', 0, 1]]);
  assert.deepEqual(tasks.openItems.map(t => t.text), ['Three code', 'Nested four']);
});

test('requirements are read only from the named section', () => {
  const rows = parseRequirements('## Approved requirements\n| ID | Text | Slice | Acceptance |\n|---|---|---|---|\n| R01 | Install | V1.0 | Launches |\n## Other\n| R09 | no | x | y |\n', 'Approved requirements');
  assert.deepEqual(rows.map(r => [r.id, r.text, r.acceptance]), [['R01', 'Install', 'Launches']]);
});

test('first bold phrase of a section is used as the focus', () => {
  assert.equal(firstBoldIn('## Current focus\n\n**Native presets, 2026-11-08:** text\n**Later:** x', 'Current focus'), 'Native presets, 2026-11-08');
  assert.equal(firstBoldIn('## Other\n**x**', 'Current focus'), null);
  assert.equal(plain('A **bold** [link](u) `code` <b>tag</b>'), 'A bold link code tag');
});
