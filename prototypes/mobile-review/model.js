/* Pure helpers shared by the prototype and its checks. No app data is accessed. */
(function (root) {
  'use strict';
  const TODAY = '2026-11-04';
  const categories = ['Bill', 'Task', 'Subscription', 'Other'];
  const seed = () => [
    { id: 'internet', title: 'Home internet', category: 'Bill', amount: '169900', date: '2026-11-02', paid: false, notes: 'Monthly home connection.' },
    { id: 'rent', title: 'Apartment rent', category: 'Bill', amount: '1250000', date: TODAY, paid: false, notes: 'Keep the payment receipt for your records.' },
    { id: 'project', title: 'Submit project proposal', category: 'Task', amount: null, date: TODAY, paid: false, notes: '' },
    { id: 'music', title: 'Music subscription', category: 'Subscription', amount: '14900', date: '2026-11-06', paid: false, notes: '' },
    { id: 'electric', title: 'Electricity bill', category: 'Bill', amount: '245050', date: '2026-11-09', paid: false, notes: 'Check the meter reading before paying.' },
    { id: 'water', title: 'Water bill', category: 'Bill', amount: '38500', date: '2026-11-12', paid: true, notes: '' }
  ];
  function status(due, today = TODAY) {
    return due.paid ? 'Paid' : due.date < today ? 'Overdue' : due.date === today ? 'Due today' : 'Upcoming';
  }
  function sorted(dues) { return dues.filter(d => !d.deleted).slice().sort((a, b) => a.date.localeCompare(b.date) || (a.createdOrder || 0) - (b.createdOrder || 0)); }
  function filter(dues, query = '', selected = 'All', category = 'All categories') {
    const needle = query.trim().toLowerCase();
    return sorted(dues).filter(d => d.title.toLowerCase().includes(needle) && (selected === 'All' || status(d) === selected) && (category === 'All categories' || d.category === category));
  }
  function summary(dues) {
    const result = { total: 0, Paid: 0, Overdue: 0, 'Due today': 0, Upcoming: 0 };
    dues.filter(d => !d.deleted).forEach(d => { result.total++; result[status(d)]++; });
    return result;
  }
  // Decimal strings + BigInt preserve centavos, including the signed 64-bit limit.
  function parseAmount(input, category) {
    const raw = input.trim();
    if (!raw) return ['Task', 'Other'].includes(category) ? { value: null } : { error: 'Enter an amount for a Bill or Subscription.' };
    if (!/^\d+(\.\d+)?$/.test(raw)) return { error: 'Use digits and a decimal point, for example 2450.50.' };
    const [whole, fraction = ''] = raw.split('.');
    let cents = BigInt(whole) * 100n + BigInt((fraction + '00').slice(0, 2));
    if ((fraction[2] || '0') >= '5') cents++;
    return cents > 9223372036854775807n ? { error: 'This amount is too large. Enter a smaller amount.' } : { value: cents.toString() };
  }
  function money(cents) {
    if (cents === null) return 'No amount';
    const n = BigInt(cents);
    return '₱' + (n / 100n).toLocaleString('en-PH') + '.' + (n % 100n).toString().padStart(2, '0');
  }
  function inputAmount(cents) { return cents === null ? '' : (BigInt(cents) / 100n) + '.' + (BigInt(cents) % 100n).toString().padStart(2, '0'); }
  function validDate(value) {
    if (!/^\d{4}-\d{2}-\d{2}$/.test(value) || value < '0001-01-01' || value > '9999-12-31') return false;
    const d = new Date(value + 'T12:00:00Z');
    return !Number.isNaN(d.valueOf()) && d.toISOString().slice(0, 10) === value;
  }
  function calendar(year, month, weekStart = 'monday') {
    const first = new Date(0); first.setUTCHours(12, 0, 0, 0); first.setUTCFullYear(year, month, 1);
    const last = new Date(first); last.setUTCMonth(month + 1, 0);
    const offset = weekStart === 'monday' ? (first.getUTCDay() + 6) % 7 : first.getUTCDay();
    return Array.from({ length: 42 }, (_, i) => i < offset || i >= offset + last.getUTCDate() ? null : `${String(year).padStart(4, '0')}-${String(month + 1).padStart(2, '0')}-${String(i - offset + 1).padStart(2, '0')}`);
  }
  const api = { TODAY, categories, seed, status, sorted, filter, summary, parseAmount, money, inputAmount, validDate, calendar };
  if (typeof module === 'object' && module.exports) module.exports = api;
  else root.DuewiseModel = api;
})(typeof globalThis === 'object' ? globalThis : this);
