const { test } = require('node:test');
const assert = require('node:assert/strict');
const M = require('../model.js');

test('optional, zero and rounded amounts preserve separate meanings without float loss', () => {
  assert.deepEqual(M.parseAmount('', 'Task'), { value: null });
  assert.deepEqual(M.parseAmount('  ', 'Other'), { value: null });
  assert.ok(M.parseAmount('', 'Bill').error);
  assert.ok(M.parseAmount('', 'Subscription').error);
  assert.equal(M.parseAmount('0', 'Bill').value, '0');
  assert.equal(M.parseAmount('2450.005', 'Bill').value, '245001');
  assert.equal(M.parseAmount('0.015', 'Bill').value, '2');
  assert.equal(M.parseAmount('92233720368547758.07', 'Bill').value, '9223372036854775807');
  assert.ok(M.parseAmount('92233720368547758.075', 'Bill').error);
  for (const invalid of ['-1', '1,000', '₱12', '1e3', '.5', '1.']) assert.ok(M.parseAmount(invalid, 'Task').error);
  assert.equal(M.money(null), 'No amount');
  assert.equal(M.money('0'), '₱0.00');
  assert.equal(M.money('9223372036854775807'), '₱92,233,720,368,547,758.07');
});

test('paid wins over date status and deleted rows leave both summaries and results', () => {
  const dues = M.seed();
  dues[0].paid = true;
  dues[1].deleted = true;
  assert.equal(M.status(dues[0]), 'Paid');
  assert.deepEqual(M.summary(dues), { total:5, Paid:2, Overdue:0, 'Due today':1, Upcoming:2 });
  assert.equal(M.sorted(dues).length, 5);
  assert.deepEqual(M.summary([]), { total:0, Paid:0, Overdue:0, 'Due today':0, Upcoming:0 });
});

test('search, date status and category combine while summary stays global', () => {
  const dues = M.seed();
  assert.deepEqual(M.filter(dues, '  RENT  ', 'Due today', 'Bill').map(d => d.id), ['rent']);
  assert.equal(M.filter(dues, 'rent', 'Paid', 'Bill').length, 0);
  assert.equal(M.filter(dues, '', 'Due today', 'Task')[0].amount, null);
  assert.equal(M.summary(dues).total, 6);
  assert.ok(M.filter(dues, '', 'Upcoming').every(d => d.date > M.TODAY));
  assert.deepEqual(M.sorted(dues).map(d => d.id), ['internet','rent','project','music','electric','water']);
});

test('six-week grids retain exact dates across week starts, leap years and year bounds', () => {
  for (const [year, month, count] of [[2026,10,30],[2028,1,29],[2027,1,28],[1,0,31],[9999,11,31]]) {
    const monday=M.calendar(year,month,'monday'), sunday=M.calendar(year,month,'sunday');
    assert.equal(monday.length,42); assert.equal(sunday.length,42);
    assert.equal(monday.filter(Boolean).length,count);
    assert.deepEqual(monday.filter(Boolean),sunday.filter(Boolean));
    assert.ok(monday.filter(Boolean).every(M.validDate));
  }
  assert.equal(M.calendar(2026,10,'monday')[6], '2026-11-01');
  assert.equal(M.calendar(2026,10,'sunday')[0], '2026-11-01');
  assert.equal(M.validDate('2026-02-29'),false);
  assert.equal(M.validDate('2028-02-29'),true);
  assert.equal(M.validDate('0000-01-01'),false);
});
