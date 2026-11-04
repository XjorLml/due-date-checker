const {test}=require('node:test');
const assert=require('node:assert/strict');
const M=require('../../mobile-review/model.js');
const E=require('../exploration-model.js');
const D=require('../design-system.js');

test('upcoming amount includes today, excludes overdue/paid, and distinguishes absent/zero',()=>{
  const data=E.seed(),o=E.overview(data);
  assert.equal(o.aheadAmount,'1509950');assert.equal(o.overdueAmount,'169900');assert.equal(E.sum(o.monthOpen),'1679850');
  assert.equal(o.withoutAmount,1);assert.equal(o.ahead.length,4);assert.equal(o.next.id,'rent');assert.equal(o.counts.Paid,1);
  data.push({id:'zero',title:'Zero amount',category:'Other',date:M.TODAY,paid:false,amount:'0',repeat:'none'});
  const next=E.overview(data);assert.equal(next.aheadAmount,o.aheadAmount);assert.equal(next.ahead.length,5);assert.equal(next.withoutAmount,1);
  data[1].paid=true;assert.equal(E.overview(data).aheadAmount,'259950');
  data[0].deleted=true;assert.equal(E.overview(data).overdueAmount,'0');
});
test('date/count charts reconcile and use exact integer aggregation beyond per-record money limits',()=>{
  const o=E.overview(E.seed());
  assert.equal(o.categories.reduce((n,c)=>n+c.count,0),6);
  assert.equal(E.sum(o.weeks.map(w=>({amount:w.amount}))),E.sum(o.monthOpen));
  assert.equal(E.sum([{amount:'9223372036854775807'},{amount:'9223372036854775807'},{amount:null}]),'18446744073709551614');
  const empty=E.overview([]);assert.equal(empty.next,null);assert.equal(empty.aheadAmount,'0');assert.equal(empty.withoutAmount,0);
});
test('repeat preview clamps month ends, handles leap/year bounds, and creates no series',()=>{
  assert.equal(E.nextOccurrence('2026-01-31','monthly'),'2026-02-28');
  assert.equal(E.nextOccurrence('2028-01-31','monthly'),'2028-02-29');
  assert.equal(E.nextOccurrence('2028-02-29','yearly'),'2029-02-28');
  assert.equal(E.nextOccurrence('2026-12-29','weekly'),'2027-01-05');
  assert.equal(E.nextOccurrence('9999-12-31','monthly'),null);
  assert.equal(E.nextOccurrence('9999-12-25','weekly'),null);
  assert.equal(E.nextOccurrence('2026-02-30','monthly'),null);
  assert.equal(E.nextOccurrence(M.TODAY,'none'),null);
  const data=E.seed();E.nextOccurrence(data[0].date,data[0].repeat);assert.equal(data.length,6);
});
test('weekly dates retain exact agenda membership for either week start and safe bounds',()=>{
  assert.deepEqual(E.weekDates('2026-11-04','monday'),['2026-11-02','2026-11-03','2026-11-04','2026-11-05','2026-11-06','2026-11-07','2026-11-08']);
  assert.deepEqual(E.weekDates('2026-11-04','sunday'),['2026-11-01','2026-11-02','2026-11-03','2026-11-04','2026-11-05','2026-11-06','2026-11-07']);
  assert.ok(E.weekDates('0001-01-01').filter(Boolean).every(M.validDate));
  assert.ok(E.weekDates('9999-12-31').filter(Boolean).every(M.validDate));
});
function luminance(hex){const a=hex.slice(1).match(/../g).map(h=>parseInt(h,16)/255).map(n=>n<=.04045?n/12.92:((n+.055)/1.055)**2.4);return a[0]*.2126+a[1]*.7152+a[2]*.0722;}
function contrast(a,b){const x=luminance(a),y=luminance(b);return(Math.max(x,y)+.05)/(Math.min(x,y)+.05);}
test('all concept/mode tokens meet normal-text contrast on their intended flat surfaces',()=>{
  for(const c of D.concepts)for(const mode of ['light','dark']){
    const t=c[mode];
    for(const [a,b] of [['text','bg'],['secondary','bg'],['text','surface'],['secondary','surface'],['primary','surface'],['onPrimary','primary'],['onAccent','accent'],['heroText','hero'],['danger','dangerBg'],['warning','warningBg'],['success','successBg']])
      assert.ok(contrast(t[a],t[b])>=4.5,`${c.id}/${mode} ${a}/${b}: ${contrast(t[a],t[b])}`);
  }
});
