// Browser checks for the UX decision lab: round 1 scenarios DL04–DL08 (specs/ux-improvements) and
// round 2 scenarios DL11–DL13 (specs/ux-improvements-2).
const assert=require('node:assert/strict');
module.exports=async({url,send,evaluate,waitFor,reloadAndWait,click,dims,shot,delay})=>{
  const open=async id=>{
    await evaluate(`location.hash='#ux-decisions/${id}'`);
    await waitFor(`!document.querySelector('#decision-view').hidden&&document.querySelector('.dl-item[aria-current=true]')?.dataset.dlGo==='${id}'`);
  };
  const overflow=()=>evaluate("document.documentElement.scrollWidth>innerWidth+1");
  await dims(1440);
  await send('Page.navigate',{url:url+'#ux-decisions'});
  await waitFor("document.readyState==='complete'&&window.DecisionLab&&!document.querySelector('#decision-view').hidden");
  await evaluate("localStorage.removeItem('duewise.ux-decisions.v1');localStorage.removeItem('duewise.ux-decisions.r2.v1')");
  await reloadAndWait("window.DecisionLab&&document.querySelectorAll('.dl-card').length>0");

  // DL04: the view replaces the other two and the header toggle reflects it.
  assert.deepEqual(await evaluate("[document.querySelector('#compare-view').hidden,document.querySelector('#explore-view').hidden,document.querySelector('.view-toggle [data-view=decide]').getAttribute('aria-pressed')]"),[true,true,'true'],
    await evaluate("document.querySelector('.view-toggle').outerHTML"));

  // DL11: a bare #ux-decisions opens round 2; the switch shows both rounds.
  assert.equal(await evaluate('window.DecisionLab.round'),'r2');
  assert.equal(await evaluate("document.querySelector('.dl-item[aria-current=true]').dataset.dlGo"),'UX17');
  assert.deepEqual(await evaluate("[...document.querySelectorAll('[data-dl-round]')].map(b=>[b.dataset.dlRound,b.getAttribute('aria-pressed')])"),[['r2','true'],['r1','false']]);
  assert.match(await evaluate("document.querySelector('#dl-sources').innerHTML"),/UX-REVIEW-2026-11-07/);

  // DL05 and DL13: every decision of both rounds renders today plus its options without page overflow.
  for(const roundId of ['r2','r1']){
    const ids=await evaluate(`window.DecisionLab.rounds.find(r=>r.id==='${roundId}').decisions.map(d=>[d.id,d.options.length,d.title])`);
    for(const [id,count,title] of ids){
      await open(id);
      assert.equal(await evaluate('window.DecisionLab.round'),roundId,id);
      assert.equal(await evaluate("document.querySelector('#dl-title').textContent"),title);
      assert.equal(await evaluate("document.querySelectorAll('#dl-main .dl-card').length"),count+1,id);
      assert.equal(await overflow(),false,'page overflow '+id);
      await shot('decision-'+id);
    }
  }

  // DL11: round 1 is the decided record: each decided option is marked and the note explains the drawings.
  // Switching back returns to the round 2 item viewed last (UX27, from the loop above).
  await open('UX06');
  assert.equal(await evaluate("[...document.querySelectorAll('#dl-main .dl-card')].filter(c=>c.querySelector('.dl-decided')).map(c=>c.querySelector('.dl-key').textContent).join()"),'B');
  assert.match(await evaluate("document.querySelector('.dl-round-note').textContent"),/before Waves 1–5/);
  assert.match(await evaluate("document.querySelector('#dl-sources').innerHTML"),/UX-REVIEW-2026-11-06/);
  await click('[data-dl-round="r2"]');
  await waitFor("window.DecisionLab.round==='r2'&&location.hash==='#ux-decisions/UX27'&&!document.querySelector('.dl-round-note')");

  // DL06 (round 1) and DL12: choosing, toggling off, the list marker, notes, persistence and per-round export.
  await open('UX01');
  await click('[data-dl-pick="UX01:A"]');
  assert.equal(await evaluate(`document.querySelector('[data-dl-pick="UX01:A"]').getAttribute('aria-pressed')`),'true');
  assert.equal(await evaluate(`document.querySelector('.dl-item[data-dl-go="UX01"] .dl-chosen').textContent`),'✓ A');
  assert.match(await evaluate("document.querySelector('#dl-progress').textContent"),/UX01: option A chosen/);
  await open('UX02');await click('[data-dl-pick="UX02:B"]');
  await evaluate(`(()=>{const t=document.querySelector('#dl-note');t.value='Try sections on the phone first';t.dispatchEvent(new Event('input',{bubbles:true}));})()`);
  await open('UX18');await click('[data-dl-pick="UX18:A"]');
  await evaluate(`(()=>{const t=document.querySelector('#dl-note');t.value='Red line is fine';t.dispatchEvent(new Event('input',{bubbles:true}));})()`);
  await reloadAndWait("window.DecisionLab&&document.querySelectorAll('.dl-card').length>0");
  assert.equal(await evaluate('window.DecisionLab.round'),'r2','reload keeps the round named in the hash');
  const md1=await evaluate("window.DecisionLab.markdown('r1')"),md2=await evaluate("window.DecisionLab.markdown('r2')");
  assert.match(md1,/\| 1 \| UX01 \| Mark as paid \| A: Mark as paid on Details \|/);
  assert.match(md1,/\| 2 \| UX02 \| Dues shows what is next \| B: Sections: Overdue, Upcoming, Paid \| Try sections on the phone first \|/);
  assert.equal((md1.match(/Not chosen/g)||[]).length,14);
  assert.match(md2,/\| 2 \| UX18 \| Overdue amount on Dashboard \| A: Separate overdue line \| Red line is fine \|/);
  assert.equal((md2.match(/Not chosen/g)||[]).length,10);
  assert.doesNotMatch(md2,/UX01|UX02/);
  assert.match(await evaluate("document.querySelector('#dl-progress').textContent"),/1 of 11 chosen in round 2/);
  await open('UX02');await click('[data-dl-pick="UX02:B"]');
  assert.equal(await evaluate(`document.querySelector('.dl-item[data-dl-go="UX02"] .dl-chosen').textContent`),'');
  await click('[data-dl-cmd="clear"]');
  assert.match(await evaluate("window.DecisionLab.markdown('r2')"),/A: Separate overdue line/,'clearing round 1 leaves round 2 alone');
  await open('UX18');await click('[data-dl-pick="UX18:A"]');

  // DL07 and DL13: phone width keeps the page within the viewport for every round 2 decision.
  await dims(390);
  for(const id of await evaluate("window.DecisionLab.rounds.find(r=>r.id==='r2').decisions.map(d=>d.id)")){
    await open(id);
    assert.equal(await overflow(),false,'mobile overflow '+id);
  }
  await open('UX19');await shot('decision-mobile-UX19');
  await open('UX02');
  assert.equal(await overflow(),false,'mobile overflow UX02');
  await shot('decision-mobile-UX02');
  await dims(1440);

  // DL08: the existing views still work after visiting the lab.
  await click('[data-view=compare]');
  await waitFor("!document.querySelector('#compare-view').hidden&&document.querySelector('#decision-view').hidden");
  await evaluate("location.hash='#a/dashboard'");
  await waitFor("!document.querySelector('#explore-view').hidden&&document.querySelector('#decision-view').hidden");
  await delay(100);
  console.log('PASS: UX decision lab; round 2 (11) opens by default and round 1 (16, decided) is marked; every decision renders today plus every option without overflow at 1440 px (and round 2 at 390 px); choices and notes persist per round across reload; per-round export; compare and explore views still work.');
};
