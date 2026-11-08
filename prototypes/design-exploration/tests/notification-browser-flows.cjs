// Browser checks for the notification lab (specs/notification-lab, NL05–NL10).
const assert=require('node:assert/strict');
module.exports=async({url,send,evaluate,waitFor,reloadAndWait,click,dims,shot,delay})=>{
  const overflow=()=>evaluate("document.documentElement.scrollWidth>innerWidth+1");
  const set=async(name,value)=>{await click(`[data-nl-set="${name}:${value}"]`);await waitFor(`document.querySelector('[data-nl-set="${name}:${value}"]').getAttribute('aria-pressed')==='true'`);};
  const due=async id=>{await click(`[data-nl-due="${id}"]`);await waitFor(`document.querySelector('[data-nl-due="${id}"]').getAttribute('aria-current')==='true'`);};
  const titles=()=>evaluate("[...document.querySelectorAll('.nl-phone .nl-title')].map(e=>e.textContent)");
  await dims(1440);
  await send('Page.navigate',{url:url+'#notifications'});
  await waitFor("document.readyState==='complete'&&window.NotificationLab&&!document.querySelector('#notification-view').hidden");
  await evaluate("localStorage.removeItem('duewise.notification-lab.v1')");
  await reloadAndWait("window.NotificationLab&&document.querySelector('.nl-phone')");

  // NL05: the tab replaces the other views and is pressed.
  assert.deepEqual(await evaluate("[document.querySelector('#compare-view').hidden,document.querySelector('#explore-view').hidden,document.querySelector('#decision-view').hidden,document.querySelector('.view-toggle [data-view=notify]').getAttribute('aria-pressed')]"),[true,true,true,'true']);

  // NL06: every due and the two-dues case render in each mode without page overflow.
  for(const id of ['rent','internet','passport','spotify','water','tuition','group']){
    await due(id);
    for(const privacy of ['generic','unlocked','everywhere'])for(const surface of ['shade','lock']){
      await set('privacy',privacy);await set('surface',surface);
      assert.ok((await evaluate("document.querySelectorAll('.nl-phone .nl-card,.nl-phone .nl-group').length"))>0,`${id} ${privacy} ${surface}`);
      assert.equal(await overflow(),false,`overflow ${id} ${privacy} ${surface}`);
    }
  }
  await set('privacy','unlocked');await set('surface','shade');await set('when','before');await due('rent');
  assert.deepEqual(await titles(),['Rent is due tomorrow']);
  assert.equal(await evaluate("document.querySelector('.nl-shade-head span').textContent"),'Wed, 4 Nov','the phone shows the day the reminder arrives');
  await shot('notification-rent-before');
  await set('when','day');await due('group');
  assert.deepEqual(await titles(),['Water bill is due today','Tuition is due today']);
  await shot('notification-group');

  // NL07: Mark as paid confirms in place, the list shows Paid, and Undo restores.
  await due('rent');
  await click('[data-nl-pay="rent"]');
  await waitFor("document.querySelector('[data-nl-undo=\"rent\"]')");
  assert.deepEqual(await titles(),['Rent marked paid']);
  assert.match(await evaluate("document.querySelector('.nl-phone .nl-text').textContent"),/Next one added for 05 Dec 2026/);
  assert.match(await evaluate("document.querySelector('[data-nl-due=\"rent\"]').textContent"),/Paid/);
  await shot('notification-rent-paid');
  await click('[data-nl-undo="rent"]');
  assert.deepEqual(await titles(),['Rent is due today']);

  // NL08: on a locked phone the button asks to unlock first; the generic mode has no button at all.
  await set('privacy','everywhere');await set('surface','lock');
  await click('[data-nl-pay="rent"]');
  await waitFor("document.querySelector('.nl-overlay')");
  await shot('notification-unlock');
  await click('[data-nl-cmd="unlock"]');
  assert.deepEqual(await titles(),['Rent marked paid']);
  await click('[data-nl-undo="rent"]');
  await set('privacy','unlocked');
  assert.equal(await evaluate("document.querySelectorAll('[data-nl-pay]').length"),0,'no button on the private lock screen');
  assert.deepEqual(await titles(),['Duewise']);
  await set('privacy','generic');await set('surface','shade');
  assert.equal(await evaluate("document.querySelectorAll('[data-nl-pay]').length"),0,'no button on the generic text');
  assert.deepEqual(await titles(),['Duewise reminder']);
  await shot('notification-today');

  // NL09: choices and notes persist; choosing N1 sets the preview; the risky pair warns; export lists both.
  await click('[data-nl-pick="N1:A"]');
  assert.equal(await evaluate("document.querySelector('[data-nl-set=\"privacy:unlocked\"]').getAttribute('aria-pressed')"),'true');
  await click('[data-nl-pick="N2:A"]');
  await evaluate(`(()=>{const t=document.querySelector('[data-nl-note="N2"]');t.value='Undo is enough';t.dispatchEvent(new Event('input',{bubbles:true}));})()`);
  await reloadAndWait("window.NotificationLab&&document.querySelector('.nl-phone')");
  const md=await evaluate('window.NotificationLab.markdown()');
  assert.match(md,/\| N1 \| What a reminder says \| A: Details when unlocked \|/);
  assert.match(md,/\| N2 \| Paying from a reminder \| A: Mark as paid button \| Undo is enough \|/);
  await click('[data-nl-pick="N1:B"]');
  assert.match(await evaluate("document.querySelector('.nl-warn')?.textContent||''"),/could pay the wrong one/);
  await click('[data-nl-cmd="clear"]');
  assert.match(await evaluate('window.NotificationLab.markdown()'),/\| N1 \| What a reminder says \| Not chosen \|/);

  // NL10: phone width keeps the page inside the viewport; the other views still work.
  await dims(390);
  for(const id of ['rent','passport','group']){await due(id);assert.equal(await overflow(),false,'mobile overflow '+id);}
  await shot('notification-mobile');
  await dims(1440);
  await click('[data-view=compare]');
  await waitFor("!document.querySelector('#compare-view').hidden&&document.querySelector('#notification-view').hidden");
  await evaluate("location.hash='#ux-decisions'");
  await waitFor("!document.querySelector('#decision-view').hidden&&document.querySelector('#notification-view').hidden");
  await delay(100);
  console.log('PASS: notification lab; six dues and the two-on-one-day case in three privacy modes on shade and lock screen without overflow; Mark as paid, Undo and the simulated unlock; generic mode has no button; choices, notes, warning and export; 390 px; other views still work.');
};
