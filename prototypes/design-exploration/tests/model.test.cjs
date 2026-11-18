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
  assert.equal(o.categories.reduce((n,c)=>n+c.count,0),7); // includes the payment-history credit card (2026-11-19)
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
  const data=E.seed();E.nextOccurrence(data[0].date,data[0].repeat);assert.equal(data.length,7);
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
test('twice a month (PRD TM) follows two picked days, falls back at month end and has one Feb occurrence when both collide',()=>{
  const chain=(date,days,n)=>{const out=[date];for(let i=0;i<n;i++)out.push(E.nextOccurrence(out.at(-1),'twicemonthly',1,'months',days));return out;};
  assert.deepEqual(chain('2026-11-15',[15,30],2),['2026-11-15','2026-11-30','2026-12-15']);
  assert.deepEqual(chain('2027-01-30',[15,30],4),['2027-01-30','2027-02-15','2027-02-28','2027-03-15','2027-03-30']);
  assert.deepEqual(chain('2026-04-15',[15,31],3),['2026-04-15','2026-04-30','2026-05-15','2026-05-31']);
  assert.deepEqual(chain('2027-01-30',[29,30],2),['2027-01-30','2027-02-28','2027-03-29']);
  assert.deepEqual(chain('2028-01-30',[29,30],2),['2028-01-30','2028-02-29','2028-03-29']);
  assert.equal(E.nextOccurrence('2026-11-20','twicemonthly',1,'months',[15,30]),'2026-11-30','an off-schedule first date moves to the next picked day');
  assert.equal(E.nextOccurrence('2026-12-31','twicemonthly',1,'months',[30,15]),'2027-01-15','day order does not matter');
  assert.equal(E.nextOccurrence('9999-12-30','twicemonthly',1,'months',[15,30]),null);
  for(const days of [[15,15],[0,30],[15,32],[15.5,30],[15]])assert.equal(E.nextOccurrence('2026-11-01','twicemonthly',1,'months',days),null);
  const rent=E.seed().find(d=>d.id==='rent');assert.equal(rent.repeat,'twicemonthly');assert.deepEqual([rent.repeatDay1,rent.repeatDay2],[4,19]);
});
test('payment history (proposed 2026-11-19): kinds, full balance rule, series history and summary',()=>{
  assert.equal(E.paymentKind('1248000','150000','1248000'),'Full balance');
  assert.equal(E.paymentKind('1300000','150000','1248000'),'Full balance');
  assert.equal(E.paymentKind('150000','150000','1248000'),'Minimum');
  assert.equal(E.paymentKind('500000','150000','1248000'),'Above minimum');
  assert.equal(E.paymentKind('100000','150000','1248000'),'Less than due');
  assert.equal(E.paymentKind('100000','150000',null),'Less than due');
  assert.equal(E.paymentKind('150000','150000',null),null);
  assert.equal(E.paymentKind(null,'150000','1248000'),null);
  assert.ok(E.validFullBalance('150000','1248000'));assert.ok(E.validFullBalance('150000',null));assert.ok(E.validFullBalance(null,'1'));
  assert.ok(!E.validFullBalance('150000','149999'));
  const data=E.seed(),card=data.find(d=>d.id==='card'),rows=[...E.seedHistory(),...data];
  const history=E.seriesHistory(rows,card);
  assert.deepEqual(history.map(h=>h.date),['2026-11-03','2026-10-03','2026-09-03','2026-08-03','2026-07-03']);
  assert.ok(E.seriesHistory(rows,data.find(d=>d.id==='water')).every(h=>h.id==='water'),'series do not mix');
  assert.deepEqual(E.seriesHistory([...rows,{...card,id:'gone',paid:true,deleted:true,paidAmount:'1'}],card).length,5,'deleted rows are left out');
  assert.deepEqual(E.historySummary(history),{count:5,recorded:5,total:'3402000',average:'680400'});
  assert.deepEqual(E.historySummary([{paidAmount:null},{paidAmount:'9223372036854775807'},{paidAmount:'9223372036854775807'}]),
    {count:3,recorded:2,total:'18446744073709551614',average:'9223372036854775807'});
  assert.deepEqual(E.historySummary([]),{count:0,recorded:0,total:'0',average:null});
});
test('payment history: paying records one payment and a repeating due adds its next occurrence once',()=>{
  const card=E.seed().find(d=>d.id==='card');
  const r=E.payOccurrence(card,'1248000','2026-11-04');
  assert.deepEqual(r.patch,{paid:true,paidAmount:'1248000',paidOn:'2026-11-04',nextAdded:true});
  assert.equal(r.next.date,'2027-01-03');assert.equal(r.next.seriesId,'card');assert.equal(r.next.amount,'150000');
  assert.equal(r.next.fullBalance,null,'a statement balance is not carried to the next month');
  assert.deepEqual([r.next.paid,r.next.paidAmount,r.next.paidOn],[false,null,null]);
  assert.equal(E.payOccurrence({...card,...r.patch,...E.unpay()},'1','2026-11-05').next,null,'paid, unpaid and paid again adds nothing more');
  const task=E.seed().find(d=>d.id==='project');
  assert.deepEqual(E.payOccurrence(task,null,'2026-11-04'),{patch:{paid:true,paidAmount:null,paidOn:'2026-11-04'},next:null});
  const rent=E.seed().find(d=>d.id==='rent');assert.equal(E.payOccurrence(rent,'1250000','2026-11-04').next.date,'2026-11-19');
  assert.deepEqual(E.unpay(),{paid:false,paidAmount:null,paidOn:null});
});
