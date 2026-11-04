/* UI extension for the HTML prototype. App state/helpers are supplied by app.js. */
const phContext={target:'editor',query:'',editing:null,mode:'category'};
function phCategoryName(d){return catalog.category(d.categoryId).name;}
function phProviderName(d){return catalog.provider(d.providerId)?.name||d.providerText||'';}
function phCategoryIcon(d){const c=catalog.category(d.categoryId);return `<span class="category-icon category-color-${c.color}">${icon(c.icon)}</span>`;}
function phCategoryFields(f){
  const c=catalog.category(f.categoryId);
  return `<div class="field category-field"><label for="category-picker-trigger">Category <span class="optional-label">optional</span></label><button id="category-picker-trigger" type="button" class="picker-trigger" data-ph="open-picker" aria-haspopup="dialog">${phCategoryIcon(f)}<span>${esc(c.name)}</span>${icon('next')}</button></div>`;
}
function phProviderFields(f){
  return `<div class="field"><label for="due-provider">Provider / biller <span class="optional-label">optional</span></label><input id="due-provider" name="providerLabel" value="${esc(f.providerLabel)}" placeholder="Choose a suggestion or enter a name" autocomplete="off" aria-describedby="provider-hint provider-error"><div id="provider-suggestions" class="provider-suggestions" aria-label="Optional suggested billers">${phProviderSuggestions(f.categoryId)}</div><p class="hint" id="provider-hint">Editable suggestions, not payment integrations. New billers are saved with the due.</p><span class="error" id="provider-error" role="status"></span></div>`;
}
function phProviderSuggestions(id){return catalog.suggest(id).map(p=>`<button type="button" data-provider="${esc(p.id)}" class="chip">${esc(p.name)}</button>`).join('');}
function phScheduleFields(f){return `<div id="custom-repeat-fields" ${f.repeat==='custom'?'':'hidden'} class="custom-repeat-fields"><div class="field"><label for="repeat-every">Repeat every</label><input id="repeat-every" name="repeatEvery" aria-describedby="repeat-hint repeat-error" inputmode="numeric" type="number" min="1" max="365" value="${f.repeatEvery}"><p id="repeat-hint" class="hint">1–365 units. Preview only; no future dues are created.</p></div><div class="field"><label for="repeat-unit">Unit</label><select id="repeat-unit" name="repeatUnit" aria-describedby="repeat-error">${[['days','Days'],['weeks','Weeks'],['months','Months'],['years','Years']].map(([v,l])=>`<option value="${v}" ${f.repeatUnit===v?'selected':''}>${l}</option>`).join('')}</select></div></div><span id="repeat-error" class="error" role="status"></span>`;}
function phReminderFields(f){
  const custom=`<div class="field"><label for="reminder-extra">Other days before <span class="optional-label">optional</span></label><input id="reminder-extra" name="reminderExtra" value="${esc(f.reminderExtra)}" inputmode="text" placeholder="e.g. 2, 14" aria-describedby="offset-hint offset-error"><p id="offset-hint" class="hint">Comma-separated whole days, 0–365. Nothing is scheduled by this HTML.</p><span id="offset-error" class="error" role="status"></span></div>`;
  return `<fieldset class="due-reminders"><legend>Reminder offsets <span class="optional-label">optional</span></legend><div class="reminder-options">${[[0,'On the due date'],[1,'1 day before'],[3,'3 days before'],[7,'1 week before']].map(([n,l])=>`<label><input type="checkbox" class="reminder-offset" value="${n}" ${f.reminderDays.includes(n)?'checked':''}><span>${l}</span></label>`).join('')}</div>${editorSection('custom-reminder-options','Custom days before',f.reminderExtra.trim()||'Optional',custom)}</fieldset>`;
}
function phSelectedId(){return phContext.target==='filter'?state.category:state.form?.categoryId;}
function phPickerRow(c){return `<div class="category-choice-row"><button type="button" class="category-choice" data-category-id="${esc(c.id)}" aria-pressed="${phSelectedId()===c.id}"><span class="category-icon category-color-${c.color}">${icon(c.icon)}</span><span>${esc(c.name)}</span>${phSelectedId()===c.id?icon('check'):''}</button>${c.custom?`<button class="icon-button" type="button" data-edit-category="${esc(c.id)}" aria-label="Edit ${esc(c.name)}">${icon('edit')}</button>`:''}</div>`;}
function phPickerSection(label,items){return items.length?`<section class="picker-section"><h3>${label}</h3>${items.map(phPickerRow).join('')}</section>`:'';}
function phRenderPicker(){
  const query=phContext.query.trim(),result=catalog.search(query),all=catalog.all();
  const frequent=catalog.frequent(state.data),recent=catalog.state.recent.map(id=>catalog.category(id)).filter(c=>all.some(v=>v.id===c.id));
  $('#category-picker-body').innerHTML=(catalog.error?`<div class="notice warning">${esc(catalog.error)}<button type="button" data-ph="retry-catalog">Retry</button></div>`:'')+(phContext.target==='filter'?`<button class="category-choice all-category-option" type="button" data-category-id="All categories" aria-pressed="${state.category==='All categories'}">${icon('dues')}All categories</button>`:'')+(query?`<p class="picker-result-count" role="status">${result.length} matching categories</p>${result.map(phPickerRow).join('')}${result.length?'':'<p class="hint">No matches. Try another category name or create your own.</p>'}`:phPickerSection('Frequently used',frequent)+phPickerSection('Recently selected',recent)+phPickerSection('My Categories',catalog.state.categories)+C.groups.map(g=>{const items=result.filter(c=>c.groupId===g.id);return `<details class="category-group"><summary>${icon(g.icon)}<span>${g.name}</span><small>${items.length}</small></summary>${items.map(phPickerRow).join('')}</details>`;}).join(''));
}
function phOpenPicker(target='editor'){
  phContext.target=target;phContext.query='';$('#category-search').value='';
  phThemeDialog($('#category-picker'));phRenderPicker();$('#category-picker').showModal();$('#category-search').focus();
}
function phThemeDialog(el){setPalette(el,concept(),state.modes[state.concept]);}
function phPick(id){
  if(id!=='All categories'&&!catalog.all().some(c=>c.id===id))return;
  if(phContext.target==='filter'){state.category=id;if(id!=='All categories')catalog.remember(id);$('#category-picker').close();renderPhone();return;}
  if(!state.form)return;
  state.form.categoryId=id;const result=catalog.remember(id);$('#category-picker').close();renderPhone();if(result.error)toast('Category selected for this draft. Recent choices could not be saved.');
  app('#category-picker-trigger')?.focus({preventScroll:true});
}
function phEditor(mode,id=null){
  phContext.mode=mode;phContext.editing=id;
  const value=mode==='category'?catalog.state.categories.find(c=>c.id===id):catalog.state.providers.find(p=>p.id===id);
  $('#catalog-editor-title').textContent=(id?'Edit ':'Create ')+(mode==='category'?'custom category':'custom biller');
  $('#catalog-name-label').textContent=mode==='category'?'Category name':'Biller name';
  $('#catalog-name').value=value?.name||'';$('#catalog-name-error').textContent='';$('#catalog-name').setAttribute('aria-invalid','false');
  $('#catalog-icon-fields').hidden=mode!=='category';
  $('#catalog-icon').innerHTML=C.icons.map(i=>`<option value="${i}" ${i===(value?.icon||'Other')?'selected':''}>${i}</option>`).join('');
  $('#catalog-color').innerHTML=C.colors.map(color=>`<label class="color-option"><input type="radio" name="catalog-color" value="${color}" ${color===(value?.color||'green')?'checked':''}><span class="color-swatch category-color-${color}"></span><span>${color}</span></label>`).join('');
  $('#catalog-delete').hidden=!id;phThemeDialog($('#catalog-editor'));$('#catalog-editor').showModal();$('#catalog-name').focus();
}
function phRefresh(){renderPhone();if($('#category-picker').open)phRenderPicker();}
function phSaveCatalog(event){
  event.preventDefault();const name=$('#catalog-name').value;
  let result;
  if(phContext.mode==='category')result=catalog.saveCategory({id:phContext.editing,name,icon:$('#catalog-icon').value,color:$('#catalog-color input:checked')?.value||'green'});
  else if(phContext.editing)result=catalog.renameProvider(phContext.editing,name);
  else result=catalog.allProviders().some(p=>C.normalize(p.name)===C.normalize(name))?{error:'A biller with this name already exists.'}:!name.trim()?{error:'Enter a biller name.'}:catalog.resolveProvider(name);
  if(result.error){$('#catalog-name-error').textContent=result.error;$('#catalog-name').setAttribute('aria-invalid','true');$('#catalog-name').focus();return;}
  const created=!phContext.editing&&phContext.mode==='category',id=result.value?.id;
  $('#catalog-editor').close();
  if(created&&$('#category-picker').open&&phContext.target==='editor'){phPick(id);return;}
  phRefresh();toast('Catalog saved in this browser.');
}
async function phDeleteCatalog(){
  const id=phContext.editing,category=phContext.mode==='category';
  const item=category?catalog.state.categories.find(c=>c.id===id):catalog.state.providers.find(p=>p.id===id);if(!item)return;
  const affected=state.data.filter(d=>(category?d.categoryId:d.providerId)===id),count=affected.length;
  const copy=category?`${count} sample ${count===1?'due will':'dues will'} move to Other. Amounts, dates, notes and payment status are kept. No due is deleted.`:`${count} sample ${count===1?'due keeps':'dues keep'} the biller name as text. No due is deleted.`;
  if(!await confirm(`Delete “${item.name}”?`,copy,'Delete '+(category?'category':'biller'),true))return;
  const result=category?catalog.deleteCategory(id):catalog.deleteProvider(id);
  if(result.error){$('#catalog-name-error').textContent=result.error;return;}
  affected.forEach(d=>{if(category)d.categoryId='ph.other.general';else{d.providerText=item.name;d.providerId=null;}});
  if(category&&state.form?.categoryId===id)state.form.categoryId='ph.other.general';
  if(state.category===id)state.category='All categories';$('#catalog-editor').close();phRefresh();toast('Catalog item deleted. Your sample dues were preserved.');
}
function phManagement(){return screenTitle('Your categories & billers.','Personalize the list. Keep your dues intact.')+`<p class="tiny-copy">Custom categories and billers persist in this browser. Sample dues reset on reload.</p>${catalog.error?`<div class="notice warning">${esc(catalog.error)}<button data-ph="retry-catalog">Retry catalog</button></div>`:''}${section('My Categories')}<button class="app-button" data-ph="create-category">${icon('add')}Create Custom Category</button><div class="my-catalog-list">${catalog.state.categories.length?catalog.state.categories.map(c=>`<button class="settings-row" data-edit-category="${esc(c.id)}"><span class="category-icon category-color-${c.color}">${icon(c.icon)}</span><span><strong>${esc(c.name)}</strong><small>Edit icon, color or name</small></span>${icon('edit')}</button>`).join(''):'<p class="tiny-copy">No custom categories yet.</p>'}</div>${section('My Billers')}<button class="app-button" data-ph="create-provider">${icon('add')}Create Custom Biller</button><div class="my-catalog-list">${catalog.state.providers.length?catalog.state.providers.map(p=>`<button class="settings-row" data-edit-provider="${esc(p.id)}">${icon('business')}<span><strong>${esc(p.name)}</strong><small>Manual entry · no payment integration</small></span>${icon('edit')}</button>`).join(''):'<p class="tiny-copy">New manual billers are saved here when you save a due.</p>'}</div>`;}
function phManagementEntry(){return `<button class="settings-row" data-ph="manage">${icon('dues')}<span><strong>My Categories & Billers</strong><small>${catalog.state.categories.length} custom categories · ${catalog.state.providers.length} custom billers</small></span>${icon('next')}</button>`;}
function phParseReminderDays(presets,extra){
  const text=String(extra||'').trim();if(text&&!/^\d+(\s*,\s*\d+)*$/.test(text))return {error:'Use comma-separated whole days, for example 2, 14.'};
  const days=[...presets,...(text?text.split(',').map(Number):[])];
  if(days.some(n=>!Number.isInteger(n)||n<0||n>365))return {error:'Reminder offsets must be whole days from 0 to 365.'};
  return {value:[...new Set(days)].sort((a,b)=>a-b)};
}
document.addEventListener('click',event=>{
  const b=event.target.closest('button');if(!b||b.closest('[inert]'))return;
  if(b.dataset.ph==='open-picker')phOpenPicker();
  if(b.dataset.ph==='filter-category')phOpenPicker('filter');
  if(b.dataset.ph==='create-category')phEditor('category');
  if(b.dataset.ph==='create-provider')phEditor('provider');
  if(b.dataset.ph==='manage')navigate('catalog');
  if(b.dataset.ph==='retry-catalog'){catalog.load();phRefresh();}
  if(b.dataset.ph==='close-picker')$('#category-picker').close();
  if(b.dataset.ph==='close-editor')$('#catalog-editor').close();
  if(b.dataset.categoryId)phPick(b.dataset.categoryId);
  if(b.dataset.editCategory)phEditor('category',b.dataset.editCategory);
  if(b.dataset.editProvider)phEditor('provider',b.dataset.editProvider);
  if(b.dataset.provider&&state.form){const p=catalog.provider(b.dataset.provider);if(p){state.form.providerLabel=p.name;app('#due-provider').value=p.name;}}
  if(b.id==='catalog-delete')phDeleteCatalog();
});
document.addEventListener('input',event=>{if(event.target.id==='category-search'){phContext.query=event.target.value;phRenderPicker();}});
document.addEventListener('submit',event=>{if(event.target.id==='catalog-editor-form')phSaveCatalog(event);});
