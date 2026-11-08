const assert=require('node:assert/strict');
module.exports=async({url,send,evaluate,waitFor,reloadAndWait,click,input,page,dims,shot,noOverflow,delay})=>{
  await send('Page.navigate',{url:url+'#interaction-lab'});
  await waitFor("typeof Lab!=='undefined'&&Lab.enabled&&!document.querySelector('#interaction-lab').hidden");
  assert.deepEqual(await evaluate('Lab.stored'),{action:null,navigation:null});
  const preview=(kind,value)=>evaluate(`Lab.command(${JSON.stringify('preview-'+kind)},${JSON.stringify(value)})`);
  const discard=async()=>{await evaluate("act('back')");if(await evaluate("document.querySelector('#confirmation').open")){await click('#confirmation-yes');await waitFor("!document.querySelector('#confirmation').open");}await waitFor("!['add','edit'].includes(state.page)");};
  // All nine combinations, both palettes, four roots and four common phone widths.
  for(const width of [320,360,390,430]){
    await dims(width);
    for(const mode of ['light','dark']){
      await click(`[data-mode=${mode}]`);
      for(const action of ['a','b','c'])for(const navigation of ['a','b','c']){
        await preview('action',action);await preview('nav',navigation);
        const roots=await evaluate(`(async()=>{const out=[];for(const page of ['dashboard','calendar','dues','settings']){await navigate(page);const nav=document.querySelector('#app-nav'),r=nav.getBoundingClientRect();out.push({page,current:nav.querySelector('[aria-current=page]').dataset.page,hidden:nav.hidden,y:r.y,h:r.height,buttons:[...nav.querySelectorAll('button')].map(b=>({h:b.getBoundingClientRect().height,w:b.getBoundingClientRect().width}))});}return out;})()`);
        for(const root of roots){assert.equal(root.current,root.page);assert.equal(root.hidden,false);assert.ok(Math.abs(root.y-roots[0].y)<1,JSON.stringify(roots));assert.equal(root.h,roots[0].h);assert.ok(root.buttons.every(b=>b.h>=44&&b.w>=44),'44px nav targets');}
        await click('#app-nav .floating-add');
        await input('#due-title','Draft retained');
        const geometry=await evaluate(`(()=>{const body=document.querySelector('#app-body'),bar=document.querySelector('#app-actions'),p=document.querySelector('#phone').getBoundingClientRect(),r=bar.getBoundingClientRect(),n=document.querySelector('#app-nav').getBoundingClientRect();return {scroll:body.scrollHeight>body.clientHeight,bodyHeight:body.clientHeight,bar:{top:r.top,bottom:r.bottom,left:r.left,right:r.right},phone:{top:p.top,bottom:p.bottom,left:p.left,right:p.right},navHidden:document.querySelector('#app-nav').hidden,navTop:n.top,layer:!!document.querySelector('#lab-form-layer[open]'),targets:[...bar.querySelectorAll('button')].map(b=>b.getBoundingClientRect().height),ids:[...document.querySelectorAll('#phone [id]')].map(e=>e.id)};})()`);
        assert.ok(geometry.scroll&&geometry.bodyHeight>30,'long scrollable form '+JSON.stringify({width,mode,action,navigation,geometry}));
        assert.equal(geometry.navHidden,navigation==='b');assert.equal(geometry.layer,navigation==='c');
        assert.ok(geometry.bar.left>=geometry.phone.left-1&&geometry.bar.right<=geometry.phone.right+1);
        assert.ok(geometry.bar.bottom<=geometry.phone.bottom+1);assert.ok(geometry.targets.every(h=>h>=48),'48px actions');
        if(action!=='a')assert.ok(await evaluate("document.querySelector('#app-actions [type=submit]').getBoundingClientRect().width>=document.querySelector('#app-actions').clientWidth-34"),'full-width primary Save');
        if(navigation!=='b')assert.ok(geometry.bar.bottom<=geometry.navTop+1,'actions above tabs');
        if(navigation==='a')assert.ok(await evaluate("document.querySelector('#app-actions [type=submit]').getBoundingClientRect().bottom<=document.querySelector('#app-nav .fab-disc').getBoundingClientRect().top-6"),'Save clears the raised Add button');
        assert.equal(new Set(geometry.ids).size,geometry.ids.length,'one live form / unique IDs');
        await evaluate("document.querySelector('#app-body').scrollTop=9999");
        assert.ok(Math.abs(await evaluate("document.querySelector('#app-actions').getBoundingClientRect().top")-geometry.bar.top)<1,'footer does not scroll');
        await noOverflow(`${width}/${mode}/${action}/${navigation}`);
        await discard();
      }
    }
  }
  // A draft survives live layout switches, native nested sheets and a rejected exit.
  await dims(390);await page('dues');await preview('nav','a');await click('#app-nav .floating-add');
  await input('#due-title','QA action lab');await input('#due-amount','123.45');await input('#due-notes','Keep this draft');
  for(const action of ['b','c','a']){await preview('action',action);assert.equal(await evaluate("document.querySelector('#due-notes').value"),'Keep this draft');}
  await preview('nav','c');assert.equal(await evaluate("document.querySelector('#lab-form-layer').open"),true);
  await input('#lab-form-layer [data-lab-switch=action]','c');assert.ok(await evaluate("document.querySelector('#app-header').textContent.includes('Cancel')"));
  await input('#lab-form-layer [data-lab-switch=status]','error');assert.ok(await evaluate("document.querySelector('#lab-feedback').textContent.includes('nothing was written')"));
  await input('#lab-form-layer [data-lab-switch=status]','loading');assert.equal(await evaluate("document.querySelector('#app-actions [type=submit]').disabled"),true);
  await input('#lab-form-layer [data-lab-switch=status]','ready');assert.equal(await evaluate("document.querySelector('#app-actions [type=submit]').disabled"),false);
  await click('#category-picker-trigger');assert.equal(await evaluate("document.querySelector('#category-picker').open"),true);
  await click('#category-picker [data-ph=close-picker]');
  await send('Input.dispatchKeyEvent',{type:'keyDown',key:'Tab',code:'Tab',windowsVirtualKeyCode:9});await send('Input.dispatchKeyEvent',{type:'keyUp',key:'Tab',code:'Tab',windowsVirtualKeyCode:9});
  assert.equal(await evaluate("!!document.activeElement.closest('#lab-form-layer')"),true,'native modal contains Tab focus');
  await send('Input.dispatchKeyEvent',{type:'keyDown',key:'Escape',code:'Escape',windowsVirtualKeyCode:27});await send('Input.dispatchKeyEvent',{type:'keyUp',key:'Escape',code:'Escape',windowsVirtualKeyCode:27});
  await waitFor("document.querySelector('#confirmation').open");assert.equal(await evaluate("document.querySelector('#confirmation button[value=cancel]').textContent"),'Keep editing');
  await click('#confirmation button[value=cancel]');await waitFor('!Lab.asking');assert.equal(await evaluate("document.querySelector('#due-notes').value"),'Keep this draft');
  await input('#lab-form-layer [data-lab-switch=navigation]','a');assert.equal(await evaluate("document.querySelector('#lab-form-layer')===null"),true);
  await click('#app-nav [data-page=calendar]');await waitFor("document.querySelector('#confirmation').open");await click('#confirmation button[value=cancel]');await waitFor('!Lab.asking');
  assert.equal(await evaluate('state.page'),'add');
  // Explicit disabled/loading/error/success previews must not write data.
  const count=await evaluate('state.data.length');
  for(const status of ['disabled','loading','error','success']){
    await input('#lab-state',status);assert.ok(await evaluate("document.querySelector('#lab-feedback').textContent.length>15"));
    if(['disabled','loading'].includes(status))assert.equal(await evaluate("document.querySelector('#app-actions [type=submit]').disabled"),true);
    if(status==='error')await click('#app-actions [type=submit]');
    assert.equal(await evaluate('state.data.length'),count);
  }
  await input('#lab-state','ready');
  await evaluate("document.querySelector('#due-form').dispatchEvent(new Event('submit',{bubbles:true,cancelable:true}));document.querySelector('#due-form').dispatchEvent(new Event('submit',{bubbles:true,cancelable:true}));");
  assert.equal(await evaluate('Lab.saving'),true);assert.equal(await evaluate("document.querySelector('#app-actions').getAttribute('aria-busy')"),'true');
  await waitFor("!Lab.saving&&state.page==='dues'");assert.equal(await evaluate('state.data.length'),count+1);assert.ok(await evaluate("document.querySelector('#app-toast').textContent.includes('saved')"));
  // Edit uses the same guarded Save path and returns to the saved record.
  await evaluate("state.selected=state.data.find(d=>d.title==='QA action lab').id");await evaluate("Lab.command('edit')");await input('#due-title','QA action lab edited');await click('#app-actions [type=submit]');await waitFor("!Lab.saving&&state.page==='details'");assert.equal(await evaluate('selected().title'),'QA action lab edited');assert.equal(await evaluate('state.data.length'),count+1);
  await evaluate("act('back')");
  // Simulated keyboard reservation and reduced visual space: all nine options.
  await send('Emulation.setDeviceMetricsOverride',{width:390,height:720,deviceScaleFactor:1,mobile:true});
  await click('#lab-keyboard');
  for(const action of ['a','b','c'])for(const navigation of ['a','b','c']){
    await preview('action',action);await preview('nav',navigation);await click('#app-nav .floating-add');
    await evaluate("document.querySelector('#app-body').scrollTop=9999");
    assert.equal(await evaluate("(()=>{const a=document.querySelector('#app-actions').getBoundingClientRect(),k=document.querySelector('#lab-keyboard-space').getBoundingClientRect();return a.bottom<=k.top+1&&document.querySelector('#app-body').clientHeight>20;})()"),true,'keyboard reservation '+action+navigation);
    if(action==='b'&&navigation==='a')await shot('lab-primary-keyboard');
    if(action==='a'&&navigation==='c')await shot('lab-layered-keyboard');
    await discard();
  }
  for(const [width,height] of [[320,568],[390,640]]){
    await send('Emulation.setDeviceMetricsOverride',{width,height,deviceScaleFactor:1,mobile:true});
    for(const action of ['a','b','c'])for(const navigation of ['a','b','c']){
      await preview('action',action);await preview('nav',navigation);await click('#app-nav .floating-add');
      assert.ok(await evaluate("document.querySelector('#app-body').clientHeight>20"),'short viewport retains scroll area '+width+'/'+height+'/'+action+'/'+navigation);
      await noOverflow('short keyboard '+width+'/'+action+'/'+navigation);await discard();
    }
  }
  await click('#lab-keyboard');await dims(390);await preview('action','a');await preview('nav','b');await click('#app-nav .floating-add');await shot('lab-sticky-context-mobile');await discard();
  // Parent preference drafts remain protected when leaving a nested management page.
  await preview('nav','a');await page('settings');await click('#phone [data-preference=week][data-value=sunday]');
  await click('#phone [data-ph=manage]');assert.equal(await evaluate('state.page'),'catalog');
  await click('#app-nav [data-page=dashboard]');await waitFor("document.querySelector('#confirmation').open");await click('#confirmation button[value=cancel]');await waitFor('!Lab.asking');assert.equal(await evaluate('state.settingsDraft.week'),'sunday');
  await evaluate("act('back')");await waitFor("state.page==='settings'");await click('#phone [data-action=save-settings]');await page('dashboard');assert.equal(await evaluate("document.querySelector('#confirmation').open"),false);
  // Independent choices persist without changing the current preview or adopting a final layout.
  await preview('nav','b');await click('[data-lab=choose-action][data-value=c]');assert.deepEqual(await evaluate('Lab.stored'),{action:'c',navigation:null});assert.equal(await evaluate('Lab.action'),'a');
  await click('[data-lab=choose-nav][data-value=a]');assert.deepEqual(await evaluate('Lab.stored'),{action:'c',navigation:'a'});assert.equal(await evaluate('Lab.navigation'),'b');
  await reloadAndWait('Lab.enabled');assert.deepEqual(await evaluate('Lab.stored'),{action:'c',navigation:'a'});assert.equal(await evaluate('Lab.action'),'a');
  await evaluate("window.labExportCopy='';window.labOriginalCreateURL=URL.createObjectURL;URL.createObjectURL=blob=>{blob.text().then(copy=>window.labExportCopy=copy);return 'blob:review-test';};window.labOriginalAnchorClick=HTMLAnchorElement.prototype.click;HTMLAnchorElement.prototype.click=function(){};");
  await click('[data-lab=export]');await waitFor("window.labExportCopy.includes('Action layout: C')&&window.labExportCopy.includes('Navigation behavior: A')");
  await evaluate("URL.createObjectURL=window.labOriginalCreateURL;HTMLAnchorElement.prototype.click=window.labOriginalAnchorClick;");
  await evaluate("window.labSetItem=Storage.prototype.setItem;Storage.prototype.setItem=function(){throw new Error('Controlled storage failure');};Lab.persist();Storage.prototype.setItem=window.labSetItem;");
  assert.ok(await evaluate("document.querySelector('#lab-storage').textContent.includes('unavailable')"));assert.deepEqual(await evaluate('Lab.stored'),{action:'c',navigation:'a'});
  await dims(1600);await shot('lab-comparison-desktop');await noOverflow('lab desktop');
  await click('[data-lab=clear]');await preview('action','c');await preview('nav','c');await click('[data-lab=exit]');assert.equal(await evaluate('Lab.enabled'),false);assert.equal(await evaluate("document.querySelector('#interaction-lab').hidden"),true);assert.equal(await evaluate("document.querySelector('#phone').dataset.refinement"),'focus');
  await click('#app-nav .floating-add');assert.equal(await evaluate("document.querySelector('#app-actions').dataset.layout"),'a');assert.equal(await evaluate("document.querySelector('#app-nav').hidden&&document.querySelector('#lab-form-layer')===null&&document.querySelector('#app-actions [data-action=back]').textContent==='Cancel'"),true,'normal mockup restores selected sticky/context-aware combination');await click('#app-actions [data-action=back]');
  console.log('PASS: 3 independent action layouts x 3 navigation policies; 320/360/390/430 light/dark roots and long forms; fixed geometry/48px actions; dirty Cancel/Escape/tab guards; preserved drafts/nested modal focus; disabled/loading/error/success previews; guarded Add/Edit with one write; reduced-height keyboard reservation; independent persistent preferences and normal Focus return.');
};
