const assert=require('node:assert/strict');
module.exports=async({send,evaluate,waitFor,click,input,page,dims,shot,noOverflow,delay})=>{
  await waitFor("typeof Lab!=='undefined'&&document.querySelector('#app-nav .floating-add')!==null");
  await page('dues');
  const leave=async discard=>{
    await click('#app-actions [data-action=back]');
    if(await evaluate("document.querySelector('#confirmation').open")){
      await click(discard?'#confirmation-yes':'#confirmation button[value=cancel]');await waitFor('!Lab.asking');
    }
  };
  // Default, collapsed form: measure the actual order and fold, not just source strings.
  for(const [width,height] of [[320,568],[360,800],[390,844],[430,932]]){
    await send('Emulation.setDeviceMetricsOverride',{width,height,deviceScaleFactor:1,mobile:true});await delay(80);
    for(const mode of ['light','dark']){
      await click(`[data-mode=${mode}]`);await click('#app-nav .floating-add');
      assert.equal(await evaluate('Lab.enabled'),false);
      assert.equal(await evaluate("document.querySelectorAll('#due-form details[open]').length"),0);
      assert.deepEqual(await evaluate("[...document.querySelectorAll('#due-form label[for]')].filter(e=>!e.closest('details:not([open])')).slice(0,3).map(e=>e.htmlFor)"),['due-title','due-amount','due-date']);
      const geo=await evaluate("(()=>{const b=document.querySelector('#app-body').getBoundingClientRect(),d=document.querySelector('#due-date').getBoundingClientRect();return {dateOffset:d.top-b.top,dateBottom:d.bottom,bodyBottom:b.bottom,scrollTop:document.querySelector('#app-body').scrollTop,scrollHeight:document.querySelector('#app-body').scrollHeight,bodyHeight:document.querySelector('#app-body').clientHeight,footer:document.querySelector('#app-actions').getBoundingClientRect().top-document.querySelector('#phone').getBoundingClientRect().top};})()");
      assert.equal(geo.scrollTop,0,'each new editor starts at its essentials');
      assert.ok(geo.dateOffset<420,'date is substantially earlier than the prior 662px label offset: '+JSON.stringify(geo));
      if(height>=800)assert.ok(geo.dateBottom<=geo.bodyBottom+1,'all three essentials fit before scrolling');
      assert.equal(await evaluate("document.querySelector('#due-amount').required"),true);
      assert.equal(await evaluate("document.querySelector('#due-amount').getAttribute('aria-required')"),'true');
      assert.ok(await evaluate("document.querySelector('[data-editor-value=reminder-options]').textContent.includes('Off')"));
      assert.equal(await evaluate("[...document.querySelectorAll('#app-actions button')].every(e=>e.getBoundingClientRect().height>=48)"),true);
      await noOverflow('essential '+width+'/'+height+'/'+mode);
      if(width===390){await evaluate("document.querySelector('#phone').scrollIntoView({block:'center'})");await delay(220);await shot('essential-'+mode+'-mobile');console.log('ESSENTIAL GEOMETRY '+mode+' '+JSON.stringify(geo));}
      await evaluate("document.querySelector('#app-body').scrollTop=9999");assert.equal(await evaluate("document.querySelector('#app-actions').getBoundingClientRect().top-document.querySelector('#phone').getBoundingClientRect().top"),geo.footer);
      await leave(false);assert.equal(await evaluate('state.page'),'dues');assert.equal(await evaluate("document.querySelector('#confirmation').open"),false,'clean Cancel exits immediately');
    }
  }
  await dims(390);await click('[data-mode=light]');await click('#app-nav .floating-add');
  await input('#due-title','Essential bill');await input('#due-amount','PHP 1,250.005');await input('#due-notes','Preserve all of this');await input('#due-provider','Meralco');
  await input('#due-repeat','custom');await input('#repeat-every','2');await input('#repeat-unit','weeks');await input('#reminder-extra','2, 14');
  await evaluate("for(const e of document.querySelectorAll('#due-form details'))e.open=false");
  await click('#category-picker-trigger');await input('#category-search','Electricity');await click('#category-picker [data-category-id="ph.utilities.electricity"]');
  assert.equal(await evaluate("document.querySelectorAll('#due-form details[open]').length"),0,'collapsed state survives category render');
  assert.deepEqual(await evaluate("({notes:state.form.notes,provider:state.form.providerLabel,every:state.form.repeatEvery,unit:state.form.repeatUnit,extra:state.form.reminderExtra,type:state.form.category})"),{notes:'Preserve all of this',provider:'Meralco',every:'2',unit:'weeks',extra:'2, 14',type:'Bill'});
  await click('#extra-details>summary');await click('[data-mode=dark]');assert.equal(await evaluate("document.querySelector('#extra-details').open"),true);
  await leave(false);assert.equal(await evaluate('state.page'),'add');assert.equal(await evaluate("document.querySelector('#due-notes').value"),'Preserve all of this');
  // Leaving via the studio route selector is protected too.
  await page('calendar');await waitFor("document.querySelector('#confirmation').open");await click('#confirmation button[value=cancel]');await waitFor('!Lab.asking');assert.equal(await evaluate('state.page'),'add');
  // Invalid controls in closed disclosures are revealed and focused, with associated errors.
  await input('#repeat-every','0');await evaluate("document.querySelector('#repeat-options').open=false");await click('#app-actions [type=submit]');
  assert.equal(await evaluate("document.querySelector('#repeat-options').open&&document.activeElement.id==='repeat-every'&&document.querySelector('#repeat-every').getAttribute('aria-describedby').includes('repeat-error')"),true);
  await input('#repeat-every','2');await input('#reminder-extra','1, -2');await evaluate("document.querySelector('#reminder-options').open=false;document.querySelector('#custom-reminder-options').open=false");await click('#app-actions [type=submit]');
  assert.equal(await evaluate("document.querySelector('#reminder-options').open&&document.querySelector('#custom-reminder-options').open&&document.activeElement.id==='reminder-extra'"),true);
  await input('#reminder-extra','2, 14');
  // Controlled catalog write failure reveals the optional biller field without losing input.
  await input('#due-provider','Essential custom biller');await evaluate("document.querySelector('#extra-details').open=false;window.essentialWrite=catalog.storage.setItem;catalog.storage.setItem=()=>{throw Error('controlled write failure')}");await click('#app-actions [type=submit]');
  assert.equal(await evaluate("document.querySelector('#extra-details').open&&document.activeElement.id==='due-provider'&&document.querySelector('#due-provider').getAttribute('aria-describedby').includes('provider-error')"),true);
  await evaluate('catalog.storage.setItem=window.essentialWrite');
  await click('#app-actions [type=submit]');assert.equal(await evaluate('state.page'),'dues');
  const saved=await evaluate("state.data.find(d=>d.title==='Essential bill')");assert.equal(saved.amount,'125001');assert.equal(saved.repeatEvery,2);assert.deepEqual(saved.reminderDays,[0,1,2,14]);assert.equal(saved.categoryId,'ph.utilities.electricity');assert.equal(saved.notes,'Preserve all of this');
  // Ordinary Edit preserves its parent chain and blank/zero/required amount contracts.
  await click(`#phone [data-due="${saved.id}"]`);await click('#app-header [data-page=edit]');await leave(false);assert.equal(await evaluate('state.page'),'details','clean Edit returns immediately');
  await click('#app-header [data-page=edit]');await input('#due-category','Task');await input('#due-amount','');
  assert.equal(await evaluate("document.querySelector('#due-amount').required"),false);assert.equal(await evaluate("document.querySelector('#amount-requirement').textContent"),'optional');
  await click('#app-actions [type=submit]');assert.equal(await evaluate('state.page'),'details');assert.equal(await evaluate('selected().amount'),null);
  await click('#app-header [data-page=edit]');await input('#due-category','Other');await input('#due-amount','0');await click('#app-actions [type=submit]');assert.equal(await evaluate('selected().amount'),'0');
  await click('#app-header [data-page=edit]');await input('#due-category','Subscription');await input('#due-amount','');await click('#app-actions [type=submit]');assert.equal(await evaluate('state.page'),'edit');assert.ok(await evaluate("document.querySelector('#amount-error').textContent.length>0"));await leave(true);assert.equal(await evaluate('selected().category'),'Other');
  await evaluate("act('back')");assert.equal(await evaluate('state.page'),'dues');
  // Enabled reminder summaries remain explicitly simulated; presets stay independent of global state.
  await evaluate('state.reminder.enabled=true');await click('#app-nav .floating-add');assert.ok(await evaluate("document.querySelector('[data-editor-value=reminder-options]').textContent.includes('09:00 Manila')"));
  await input('#due-title','Discard this draft');await leave(true);assert.equal(await evaluate("state.data.some(d=>d.title==='Discard this draft')"),false);assert.equal(await evaluate("document.querySelector('#app-nav').hidden"),false);
  // Twice a month (PRD TM): default 15 & 30, equal days refused, both days saved and shown in Details.
  await page('dues');await click('#app-nav .floating-add');await input('#due-title','Twice bill');await input('#due-amount','500');await input('#due-date','2026-11-20');await input('#due-repeat','twicemonthly');
  assert.equal(await evaluate("document.querySelector('#twice-repeat-fields').hidden||document.querySelector('#custom-repeat-fields').hidden===false"),false);
  assert.equal(await evaluate("document.querySelector('#repeat-day-1').value+'/'+document.querySelector('#repeat-day-2').value"),'15/30');
  assert.equal(await evaluate("document.querySelector('#repeat-preview').textContent===`Next preview: ${dateText('2026-11-30')}, then ${dateText('2026-12-15')}. No future dues are generated.`"),true);
  await input('#repeat-day-2','15');await click('#app-actions [type=submit]');
  assert.equal(await evaluate("state.page==='add'&&document.activeElement.id==='repeat-day-2'&&document.querySelector('#repeat-error').textContent.includes('two different days')"),true);
  await input('#repeat-day-1','30');await click('#app-actions [type=submit]');assert.equal(await evaluate('state.page'),'dues');
  const twice=await evaluate("state.data.find(d=>d.title==='Twice bill')");assert.equal(twice.repeat,'twicemonthly');assert.deepEqual([twice.repeatDay1,twice.repeatDay2],[15,30]);
  assert.ok(await evaluate(`document.querySelector('#phone [data-due="${twice.id}"]').textContent.includes('Twice a month')`));
  await click(`#phone [data-due="${twice.id}"]`);assert.ok(await evaluate("document.querySelector('#phone .detail-facts').textContent.includes('Twice a month on days 15 and 30')"));
  // Every root still receives its direct Add return, including a successful quick Save.
  for(const origin of ['dashboard','calendar','dues','settings']){
    await page(origin);await click('#app-nav .floating-add');await input('#due-title','Quick '+origin);await input('#due-amount','1');await click('#app-actions [type=submit]');assert.equal(await evaluate('state.page'),origin);assert.equal(await evaluate("document.querySelector('#app-nav [aria-current=page]').dataset.page"),origin);
  }
  // Payment history preview (proposed 2026-11-19, specs/payment-history): amount paid, chips, history, edit, unpay.
  const dialogOpen=id=>`document.querySelector('#${id}').open`;
  await page('dues');await click('#phone [data-due="card"]');
  assert.ok(await evaluate("document.querySelector('#phone .payment-history .history-summary').textContent.startsWith('5 payments')"));
  assert.equal(await evaluate("document.querySelectorAll('#phone .history-row').length"),5);
  assert.ok(await evaluate("document.querySelector('#phone .detail-sub').textContent.includes('Full balance')"));
  await click('#phone [data-action=quick-paid]');await waitFor(dialogOpen('payment-dialog'));
  assert.equal(await evaluate("document.querySelector('#payment-amount').value"),'1500.00','prefilled with the minimum due');
  assert.equal(await evaluate("[...document.querySelectorAll('#payment-chips .chip')].map(c=>c.textContent).join('|')"),'Minimum ₱1,500.00|Full balance ₱12,480.00|Other amount');
  await click('#payment-chips .chip:nth-of-type(2)');
  assert.equal(await evaluate("document.querySelector('#payment-amount').value"),'12480.00');
  await shot('payment-dialog');
  await input('#payment-amount','12,48x');await click('#payment-save');
  assert.ok(await evaluate(dialogOpen('payment-dialog')+"&&document.querySelector('#payment-amount').getAttribute('aria-invalid')==='true'&&document.querySelector('#payment-error').textContent.length>0"),'an invalid amount is refused');
  await input('#payment-amount','5,000');await click('#payment-save');await waitFor("state.data.find(d=>d.id==='card').paid");
  const paidCard=await evaluate("state.data.find(d=>d.id==='card')");
  assert.deepEqual([paidCard.paid,paidCard.paidAmount,paidCard.paidOn],[true,'500000','2026-11-04']);
  assert.ok(await evaluate("state.data.some(d=>d.id==='card-2027-01-03'&&!d.paid&&d.fullBalance===null)"),'next month added without a statement balance');
  assert.ok(await evaluate("document.querySelector('#phone .paid-line').textContent.includes('Paid ₱5,000.00')"));
  assert.equal(await evaluate("document.querySelectorAll('#phone .history-row').length"),6);
  await evaluate("document.querySelector('#phone .payment-history').scrollIntoView({block:'start'})");await shot('payment-history');
  assert.ok(await evaluate("document.querySelector('#app-toast').textContent.startsWith('Paid ₱5,000.00. Next one added')"));
  await click('#phone [data-action=edit-payment]');await waitFor(dialogOpen('payment-dialog'));
  assert.equal(await evaluate("document.querySelector('#payment-amount').value"),'5000.00');
  await input('#payment-amount','6000');await click('#payment-save');await waitFor("state.data.find(d=>d.id==='card').paidAmount==='600000'");
  assert.equal(await evaluate("state.data.find(d=>d.id==='card').paidAmount"),'600000');
  await click('#phone [data-action=edit-payment]');await waitFor(dialogOpen('payment-dialog'));
  await input('#payment-amount','1');await click('#payment-dialog [data-pay=cancel]');await waitFor('!'+dialogOpen('payment-dialog'));
  assert.equal(await evaluate("state.data.find(d=>d.id==='card').paidAmount"),'600000','cancel keeps the recorded amount');
  await click('#phone [data-action=quick-paid]');await waitFor(dialogOpen('confirmation'));await click('#confirmation-yes');await waitFor("!state.data.find(d=>d.id==='card').paid");
  assert.deepEqual(await evaluate("(({paid,paidAmount,paidOn})=>[paid,paidAmount,paidOn])(state.data.find(d=>d.id==='card'))"),[false,null,null]);
  assert.equal(await evaluate("state.data.filter(d=>d.seriesId==='card').length"),2,'unpaying keeps the next month already added');
  await page('dues');await click('#phone [data-due="project"]');await click('#phone [data-action=quick-paid]');await waitFor('selected().paid');
  assert.equal(await evaluate(dialogOpen('payment-dialog')),false,'a due with no amount is marked paid without the dialog');
  assert.deepEqual(await evaluate("(({paid,paidAmount})=>[paid,paidAmount])(selected())"),[true,null]);
  await page('dues');await click('#phone [data-due="card"]');await click('#app-header [data-page=edit]');
  await input('#due-full-balance','1000');await click('#app-actions [type=submit]');
  assert.ok(await evaluate("state.page==='edit'&&document.querySelector('#full-balance-error').textContent.includes('less than the amount due')"),'full balance below the minimum is refused');
  await leave(true);
  console.log('PASS: essential-first default Add/Edit, earlier core fields, collapsed controls, draft/disclosure retention, normal dirty exits, revealed errors, PHP paste, amount policies, schedules, twice-a-month days, payment amounts and history, and origin returns. 30-second human acceptance remains unmeasured.');
};
