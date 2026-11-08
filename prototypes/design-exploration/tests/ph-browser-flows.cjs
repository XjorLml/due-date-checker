const assert=require('node:assert/strict');
module.exports=async({fs,path,folder,qa,send,evaluate,waitFor,reloadAndWait,click,input,page,dims,shot,noOverflow,delay,pathToFileURL})=>{
  const back=async()=>{await click('#phone [data-action=back]');if(await evaluate("document.querySelector('#confirmation').open")){await click('#confirmation-yes');await waitFor('!Lab.asking');}};
  await waitFor("document.querySelector('#phone #app-nav .floating-add')!==null");await dims(390);await evaluate('document.fonts.ready');
  assert.equal(await evaluate("document.querySelector('#phone').dataset.refinement"),'focus');
  assert.equal(await evaluate("getComputedStyle(document.querySelector('#concept-switcher')).display"),'none');
  assert.equal(await evaluate("document.querySelector('#minimal-refinements').hidden"),true);
  assert.equal(await evaluate('Lab.enabled'),false);
  assert.equal(await evaluate("document.body.dataset.focusActions"),'true');
  assert.equal(await evaluate('C.groups.length'),10);assert.equal(await evaluate('C.categories.length'),53);
  await shot('ph-focus-dashboard');
  const rootNavPositions=[];
  for(const origin of ['dashboard','calendar','dues','settings']){
    await page(origin);assert.deepEqual(await evaluate("Array.from(document.querySelectorAll('#app-nav button'),b=>b.dataset.page)"),['dashboard','calendar','add','dues','settings']);
    assert.equal(await evaluate("document.querySelector('#app-header [data-page=add]')===null"),true);
    rootNavPositions.push(await evaluate("document.querySelector('#app-nav').getBoundingClientRect().bottom"));
    await click('#app-nav .floating-add');assert.equal(await evaluate("document.querySelector('#app-nav').hidden"),true);await back();assert.equal(await evaluate("document.querySelector('#app-nav [aria-current=page]').dataset.page"),origin);
  }
  assert.ok(rootNavPositions.every(y=>Math.abs(y-rootNavPositions[0])<1),'default root navigation stays anchored');
  // Searchable grouped sheet and separate editable suggestions.
  await page('dues');await click('#app-nav .floating-add');await click('#category-picker-trigger');
  assert.equal(await evaluate("document.querySelector('#category-picker').open"),true);assert.equal(await evaluate("document.querySelectorAll('#category-picker .category-group').length"),10);
  assert.ok(await evaluate("document.querySelector('#category-picker-body').textContent.includes('Frequently used')"));
  await input('#category-search','Amilyar');assert.equal(await evaluate("document.querySelectorAll('#category-picker .category-choice').length"),1);assert.ok(await evaluate("document.querySelector('#category-picker .category-choice').textContent.includes('Real Property Tax')"));
  await input('#category-search','Electricity');await click('#category-picker [data-category-id="ph.utilities.electricity"]');
  assert.ok(await evaluate("document.querySelector('#category-picker-trigger').textContent.includes('Electricity')"));assert.ok(await evaluate("document.querySelector('#provider-suggestions').textContent.includes('Meralco')"));
  await click('#provider-suggestions [data-provider="provider.meralco"]');assert.equal(await evaluate("document.querySelector('#due-provider').value"),'Meralco');
  await click('#category-picker-trigger');await shot('ph-category-picker-light');
  // Empty and duplicate validation, optional presentation and immediate catalog persistence.
  await click('#category-picker [data-ph=create-category]');await click('#catalog-editor .sheet-footer [type=submit]');assert.ok(await evaluate("document.querySelector('#catalog-name-error').textContent.includes('Enter')"));
  await input('#catalog-name',' electricity ');await click('#catalog-editor .sheet-footer [type=submit]');assert.ok(await evaluate("document.querySelector('#catalog-name-error').textContent.includes('already exists')"));
  await input('#catalog-name','Pet Care');await input('#catalog-icon','health');await click('#catalog-color [value=blue]');await click('#catalog-editor .sheet-footer [type=submit]');
  assert.equal(await evaluate("document.querySelector('#category-picker').open"),false);assert.ok(await evaluate("document.querySelector('#category-picker-trigger').textContent.includes('Pet Care')"));
  const catId=await evaluate("catalog.state.categories.find(c=>c.name==='Pet Care').id");assert.ok(catId.startsWith('custom.category.'));
  await back();await page('settings');await click('#phone [data-ph=manage]');assert.ok(await evaluate("document.querySelector('#phone').textContent.includes('My Categories')"));
  await click('#phone [data-edit-category="'+catId+'"]');await input('#catalog-name','Pet Wellness');await click('#catalog-editor .sheet-footer [type=submit]');assert.equal(await evaluate(`catalog.category(${JSON.stringify(catId)}).name`),'Pet Wellness');
  await reloadAndWait("document.querySelector('#phone .floating-add')!==null");await page('settings');await click('#phone [data-ph=manage]');assert.ok(await evaluate("document.querySelector('#phone').textContent.includes('Pet Wellness')"));
  // Create one due: manual variable charge, custom biller, quarterly/custom recurrence and offsets.
  await page('dues');await click('#app-nav .floating-add');await input('#due-title','QA Philippine due');await input('#due-amount','2500.005');await click('#category-picker-trigger');await input('#category-search','Pet Wellness');await click('#category-picker [data-category-id="'+catId+'"]');
  await input('#due-provider','Barangay Cooperative');await input('#due-repeat','quarterly');assert.ok(await evaluate("document.querySelector('#repeat-preview').textContent.includes('2027')"));
  await input('#due-repeat','custom');await input('#repeat-every','2');await input('#repeat-unit','weeks');assert.ok(await evaluate("document.querySelector('#repeat-preview').textContent.includes('18')"));
  await click('#phone .reminder-offset[value="3"]');await input('#reminder-extra','2, 14');await input('#due-date','2026-11-01');await click('#app-actions [type=submit]');
  assert.equal(await evaluate("document.querySelectorAll('#phone .due-row').length"),7);
  const saved=await evaluate("state.data.find(d=>d.title==='QA Philippine due')");assert.equal(saved.categoryId,catId);assert.equal(saved.amount,'250001');assert.ok(saved.providerId.startsWith('custom.provider.'));assert.deepEqual(saved.reminderDays,[0,1,2,3,14]);assert.equal(saved.repeatEvery,2);assert.equal(saved.repeatUnit,'weeks');
  const dueId=saved.id,providerId=saved.providerId;
  await click('#phone [data-due="'+dueId+'"]');assert.ok(await evaluate("document.querySelector('#phone .detail-facts').textContent.includes('Barangay Cooperative')"));assert.ok(await evaluate("document.querySelector('#phone .detail-facts').textContent.includes('14 days before')"));
  await click('#phone [data-page=edit]');assert.equal(await evaluate("document.querySelector('#app-header [data-action=delete]')!==null&&document.querySelector('#app-actions [data-action=back]').textContent==='Cancel'"),true);await input('#due-amount','3333');await back();assert.equal(await evaluate(`state.data.find(d=>d.id===${JSON.stringify(dueId)}).amount`),'250001');
  // Catalog identity and labels update consistently across all surfaces.
  await page('settings');await click('#phone [data-ph=manage]');await click('#phone [data-edit-category="'+catId+'"]');await input('#catalog-name','Household Care');await click('#catalog-editor .sheet-footer [type=submit]');
  await click('#phone [data-edit-provider="'+providerId+'"]');await input('#catalog-name','Neighborhood Cooperative');await click('#catalog-editor .sheet-footer [type=submit]');
  for(const name of ['dashboard','calendar','dues']){await page(name);if(name==='calendar')await click('#phone [data-date="2026-11-01"]');assert.ok(await evaluate("document.querySelector('#phone').textContent.includes('Household Care')"),name+' category label');assert.ok(await evaluate("document.querySelector('#phone').textContent.includes('Neighborhood Cooperative')"),name+' biller label');}
  await page('dues');await click('#category-filter');await input('#category-search','Household Care');await click('#category-picker [data-category-id="'+catId+'"]');assert.equal(await evaluate("document.querySelectorAll('#phone #dues-results .due-row').length"),1);
  await click('#phone [data-due="'+dueId+'"]');assert.ok(await evaluate("document.querySelector('#phone .detail-facts').textContent.includes('Household Care')"));
  // Cancel category deletion, then retain the due under Other; biller deletion retains its name.
  await page('settings');await click('#phone [data-ph=manage]');await click('#phone [data-edit-category="'+catId+'"]');await click('#catalog-delete');await click('#confirmation button[value=cancel]');await delay(20);assert.equal(await evaluate(`catalog.category(${JSON.stringify(catId)}).id`),catId);
  await click('#catalog-delete');await click('#confirmation-yes');await delay(20);
  const preserved=await evaluate(`state.data.find(d=>d.id===${JSON.stringify(dueId)})`);assert.equal(preserved.categoryId,'ph.other.general');assert.equal(preserved.amount,saved.amount);assert.equal(preserved.date,saved.date);assert.equal(preserved.notes,saved.notes);assert.equal(preserved.paid,saved.paid);assert.equal(await evaluate('state.data.length'),7);
  await click('#phone [data-edit-provider="'+providerId+'"]');await click('#catalog-delete');await click('#confirmation-yes');await delay(20);assert.equal(await evaluate(`phProviderName(state.data.find(d=>d.id===${JSON.stringify(dueId)}))`),'Neighborhood Cooperative');
  // A failed browser storage write must retain the form and existing catalog.
  await evaluate("globalThis.qaWrite=catalog.storage.setItem;catalog.storage.setItem=()=>{throw Error('controlled quota')}");await click('#phone [data-ph=create-category]');await input('#catalog-name','Unsaved category');await click('#catalog-editor .sheet-footer [type=submit]');assert.ok(await evaluate("document.querySelector('#catalog-name-error').textContent.includes('Couldn’t save')"));assert.equal(await evaluate("catalog.state.categories.some(c=>c.name==='Unsaved category')"),false);await evaluate('catalog.storage.setItem=globalThis.qaWrite');await click('#catalog-editor [data-ph=close-editor]');
  // Literal custom names cannot become markup; provider omission and zero remain valid.
  await click('#phone [data-ph=create-category]');await input('#catalog-name','Text <img src=x onerror=alert(1)>');await click('#catalog-editor .sheet-footer [type=submit]');assert.equal(await evaluate("document.querySelector('#phone img')===null"),true);
  await page('dues');await click('#phone [data-action=clear-filters]');await click('#app-nav .floating-add');await input('#due-title','QA optional task');await input('#due-category','Task');await click('#app-actions [type=submit]');assert.equal(await evaluate("state.data.find(d=>d.title==='QA optional task').amount"),null);
  await evaluate("Array.from(document.querySelectorAll('#phone .due-row')).find(b=>b.textContent.includes('QA optional task')).click()");await click('#phone [data-page=edit]');await input('#due-amount','0');await click('#app-actions [type=submit]');assert.equal(await evaluate("document.querySelector('#phone .detail-amount').textContent"),'₱0.00');
  // Picker, editor and selected navigation stay usable at phone/desktop widths in both modes.
  for(const width of [1600,768,430,390,360,320]){
    await dims(width);
    for(const mode of ['light','dark']){
      await click('[data-mode='+mode+']');
      for(const name of ['dashboard','calendar','dues','settings','reminders']){await page(name);await noOverflow([width,mode,name].join('/'));}
      await page('dues');await click('#app-nav .floating-add');await noOverflow('editor '+width+mode);
      const footer=await evaluate("(()=>{const b=document.querySelector('#app-actions'),r=b.getBoundingClientRect();return {top:r.top,bottom:r.bottom,phoneBottom:document.querySelector('#phone').getBoundingClientRect().bottom,buttons:[...b.querySelectorAll('button')].map(e=>({text:e.textContent.trim(),height:e.getBoundingClientRect().height,width:e.getBoundingClientRect().width})),headerCancel:!!document.querySelector('#app-header [data-action=back]'),navHidden:document.querySelector('#app-nav').hidden};})()");
      assert.deepEqual(footer.buttons.map(b=>b.text),['Cancel','Save']);assert.ok(footer.buttons.every(b=>b.height>=48&&b.width>=44));assert.equal(footer.headerCancel,false);assert.equal(footer.navHidden,true);assert.ok(footer.bottom<=footer.phoneBottom+1);
      await evaluate("document.querySelector('#app-body').scrollTop=9999");assert.ok(Math.abs(await evaluate("document.querySelector('#app-actions').getBoundingClientRect().top")-footer.top)<1,'default sticky actions remain visible');
      if(width===390&&mode==='light')await shot('focus-sticky-actions-selected');
      await click('#category-picker-trigger');
      assert.equal(await evaluate("(()=>{const r=document.querySelector('#category-picker').getBoundingClientRect();return r.left>=0&&r.right<=innerWidth+1&&r.height<=innerHeight})()"),true);
      assert.equal(await evaluate("document.querySelector('#category-picker').scrollWidth<=document.querySelector('#category-picker').clientWidth+1"),true);
      if(width===390&&mode==='dark'){await shot('ph-category-picker-dark');await click('#category-picker [data-ph=create-category]');await shot('ph-custom-category-dark');await click('#catalog-editor [data-ph=close-editor]');}
      await click('#category-picker [data-ph=close-picker]');await back();
    }
  }
  await dims(390);await click('[data-mode=light]');await page('dues');await click('#app-nav .floating-add');await click('#category-picker-trigger');await input('#category-search','Credit Card');await click('#category-picker [data-category-id="ph.credit.card"]');assert.ok(await evaluate("document.querySelector('#provider-suggestions').textContent.includes('BDO')"));await shot('ph-provider-form');
  await input('#due-title','Retain draft');await input('#due-provider','Manual sample biller');await click('[data-mode=dark]');assert.equal(await evaluate("document.querySelector('#due-provider').value"),'Manual sample biller');await back();
  // Existing preference drafts and opt-in survive the new management route.
  await page('settings');await click('#phone [data-preference=week][data-value=sunday]');await click('#phone [data-action=reminders]');await back();
  assert.equal(await evaluate("document.querySelector('#phone [data-preference=week][data-value=sunday]').getAttribute('aria-pressed')"),'true');
  await click('#phone [data-ph=manage]');await back();assert.equal(await evaluate("document.querySelector('#phone [data-preference=week][data-value=sunday]').getAttribute('aria-pressed')"),'true');
  await click('#app-actions [data-action=save-settings]');await page('calendar');assert.ok(await evaluate("document.querySelector('#phone').textContent.includes('Sunday first')"));
  await page('reminders');await click('#app-actions [data-enable=false]');assert.equal(await evaluate('state.reminder.enabled'),false);await click('#app-actions [data-enable=true]');await click('#confirmation button[value=cancel]');await delay(20);assert.equal(await evaluate('state.reminder.enabled'),false);
  await click('#app-actions [data-enable=true]');await click('#confirmation-yes');await delay(20);assert.equal(await evaluate('state.reminder.enabled'),true);
  await page('dues');
  // Multiple presets and custom offsets validate without writes, then round-trip.
  await click('#app-nav .floating-add');await input('#due-title','QA invalid schedule');await input('#due-amount','1');await input('#due-repeat','custom');await input('#repeat-every','0');await click('#app-actions [type=submit]');
  const scheduleState=await evaluate("({repeat:state.form?.repeat,every:state.form?.repeatEvery,error:document.querySelector('#repeat-error')?.textContent,amountError:document.querySelector('#amount-error')?.textContent,titleError:document.querySelector('#title-error')?.textContent,dateError:document.querySelector('#date-error')?.textContent,inside:!!document.querySelector('#repeat-every')?.closest('#due-form')})");
  assert.ok(scheduleState.error.includes('1 to 365'),JSON.stringify(scheduleState));
  await input('#repeat-every','2');await input('#reminder-extra','-1');await click('#app-actions [type=submit]');assert.ok(await evaluate("document.querySelector('#offset-error').textContent.length>0"));await back();
  // Catalog persists, while reset/reload only clears sample dues. Defaults remain Focus.
  await click('#reset-sample');await click('#confirmation-yes');await delay(20);assert.equal(await evaluate('state.data.length'),6);assert.equal(await evaluate('catalog.state.categories.length'),1);
  await reloadAndWait("document.querySelector('#phone .floating-add')!==null");assert.equal(await evaluate('catalog.state.categories.length'),1);assert.equal(await evaluate('state.data.length'),6);assert.equal(await evaluate("document.querySelector('#phone').dataset.refinement"),'focus');
  await page('settings');await click('#phone [data-ph=manage]');assert.ok(await evaluate("document.querySelector('#phone').textContent.includes('Text <img')"));
  await dims(390);await evaluate("document.documentElement.style.fontSize='32px'");await noOverflow('200 percent catalog');await evaluate("document.documentElement.style.fontSize='16px'");
  assert.equal(await evaluate("(()=>{const ids=Array.from(document.querySelectorAll('[id]'),e=>e.id);return ids.length===new Set(ids).size})()"),true);
  await send('Page.navigate',{url:pathToFileURL(path.join(folder,'index.html')).href});await waitFor("location.protocol==='file:'&&document.readyState==='complete'&&document.querySelector('#phone .floating-add')!==null");await evaluate('document.fonts.ready');assert.equal(await evaluate("document.fonts.check('400 16px Jakarta')"),true);await dims(1600);await shot('ph-focus-desktop');
  console.log('PASS: selected Focus navigation; 53 grouped/searchable Philippine categories; optional suggestions and stable IDs; persistent custom category/biller CRUD, duplicates, safe deletion and write-failure retention; consistent surfaces/filters; custom/quarterly recurrence and multiple offsets; responsive light/dark, drafts/Save/Cancel/zero/absence and direct-file startup.');
};
