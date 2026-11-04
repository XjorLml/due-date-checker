'use strict';
// One live editor and one shared navbar. This controller changes presentation only.
const Lab={
  enabled:false,action:'a',navigation:'b',status:'ready',saving:false,asking:false,
  baseline:null,keyboard:false,layer:null,parent:null,origin:'dashboard',stored:{action:null,navigation:null},
  actionNames:{a:'Sticky bottom actions',b:'Primary Save / secondary Cancel',c:'Header Cancel / bottom Save'},
  navNames:{a:'Persistent navigation',b:'Context-aware navigation',c:'Layered navigation'},
  key:'duewise.interaction-lab.v1',
  get focusSelected(){return state.concept==='a'&&preferences.refinement==='focus';},
  init(){
    try{const saved=JSON.parse(localStorage.getItem(this.key)||'{}');for(const key of ['action','navigation'])if(['a','b','c'].includes(saved[key]))this.stored[key]=saved[key];}catch{/* Preferences are optional; never prevent the preview. */}
    document.addEventListener('click',event=>{
      const button=event.target.closest('[data-lab]');if(!button)return;
      event.preventDefault();this.command(button.dataset.lab,button.dataset.value);
    });
    document.addEventListener('change',event=>{
      if(event.target.id==='lab-state'){if(this.saving)return;this.status=event.target.value;this.paintActions();}
      if(event.target.id==='lab-keyboard'){this.keyboard=event.target.checked;this.layout();}
      if(event.target.dataset.labSwitch){
        if(this.saving)return;
        const kind=event.target.dataset.labSwitch,value=event.target.value;
        if(kind==='mode'){state.modes[state.concept]=value;setPalette($('#phone'),concept(),value);}
        else{this[kind]=value;renderPhone();(this.layer||$('#lab-live-controls')).querySelector(`[data-lab-switch=${kind}]`)?.focus();}
      }
    });
    document.addEventListener('focusin',event=>{if(this.enabled&&event.target.closest('#due-form'))setTimeout(()=>{if(event.target.isConnected)event.target.scrollIntoView({block:'nearest'});},80);});
    window.addEventListener('resize',()=>this.layout());
    window.addEventListener('scroll',()=>this.positionLayer(),{passive:true});
    window.visualViewport?.addEventListener('resize',()=>this.layout());
    window.visualViewport?.addEventListener('scroll',()=>this.positionLayer());
    window.addEventListener('beforeunload',event=>{if(this.dirty()){event.preventDefault();event.returnValue='';}});
  },
  persist(){
    try{localStorage.setItem(this.key,JSON.stringify(this.stored));$('#lab-storage').textContent='Only your two design preferences are saved in this browser. Choosing does not apply a final design.';}
    catch{$('#lab-storage').textContent='Browser storage is unavailable. Export your choices before leaving.';}
  },
  controls(){
    $('#interaction-lab').hidden=!this.enabled;document.body.dataset.interactionLab=String(this.enabled);
    document.body.dataset.focusActions=String(this.enabled||this.focusSelected);
    $('#lab-live-controls').hidden=!this.enabled;
    if(!this.enabled)return;
    for(const kind of ['action','navigation'])$('#lab-live-controls').querySelector(`[data-lab-switch=${kind}]`).value=this[kind];
    const descriptions={a:'Both actions stay close to your thumb. A compact 48px row, with less room for long button labels.',b:'A wide, easy-to-reach Save with a quiet Cancel above. Clear priority, but a taller footer.',c:'The lightest footer and a full-width Save. Cancel stays discoverable in the header, farther from your thumb.'};
    $('#lab-action-options').innerHTML=Object.entries(this.actionNames).map(([id,name])=>`<article class="lab-option" data-preview="${this.action===id}"><span class="eyebrow">OPTION ${id.toUpperCase()}${this.action===id?' / PREVIEWING':''}</span><h4>${name}</h4><div class="lab-mini" aria-hidden="true"><div class="lab-mini-header">${id==='c'?'Cancel':''}<strong>Add due</strong></div><div class="lab-mini-field"></div><div class="lab-mini-field short"></div><div class="lab-mini-footer ${id==='b'?'stacked':''}">${id!=='c'?'<span>Cancel</span>':''}<span>Save</span></div></div><p>${descriptions[id]}</p><div class="lab-option-controls"><button class="studio-secondary" data-lab="preview-action" data-value="${id}" aria-pressed="${this.action===id}">Preview ${id.toUpperCase()}</button><button class="studio-primary" data-lab="choose-action" data-value="${id}" aria-pressed="${this.stored.action===id}">${this.stored.action===id?'Chosen':'Choose'} ${id.toUpperCase()}</button></div></article>`).join('');
    const navDescriptions={a:'Tabs stay available on roots and pushed pages. Form actions sit above them. More destinations, less form space.',b:'Tabs stay fixed on all four roots. Add/Edit and other focused pages use contextual actions instead. More room to focus.',c:'The task opens above its parent. Tabs remain visible behind a dimmed overlay and are unavailable until you finish.'};
    $('#lab-nav-options').innerHTML=Object.entries(this.navNames).map(([id,name])=>`<article class="lab-option" data-preview="${this.navigation===id}"><span class="eyebrow">OPTION ${id.toUpperCase()}${this.navigation===id?' / PREVIEWING':''}</span><h4>${name}</h4><div class="lab-nav-diagram" aria-hidden="true">${id==='a'?'Form → actions → <strong>Focus tabs</strong>':id==='b'?'Roots → Focus tabs<br>Form → <strong>task actions</strong>':'<strong>Modal task</strong><br>Parent tabs behind overlay'}</div><p>${navDescriptions[id]}</p><div class="lab-option-controls"><button class="studio-secondary" data-lab="preview-nav" data-value="${id}" aria-pressed="${this.navigation===id}">Preview ${id.toUpperCase()}</button><button class="studio-primary" data-lab="choose-nav" data-value="${id}" aria-pressed="${this.stored.navigation===id}">${this.stored.navigation===id?'Chosen':'Choose'} ${id.toUpperCase()}</button></div></article>`).join('');
    $('#lab-preferences').textContent=`Your action choice: ${this.stored.action?this.stored.action.toUpperCase()+' / '+this.actionNames[this.stored.action]:'Not chosen'}. Your navigation choice: ${this.stored.navigation?this.stored.navigation.toUpperCase()+' / '+this.navNames[this.stored.navigation]:'Not chosen'}.`;
    $('#lab-state').value=this.status;$('#lab-state').disabled=this.saving;
  },
  async command(command,value){
    if(this.saving||this.asking)return;
    if(command==='enter'){this.enabled=true;state.concept='a';preferences.refinement='focus';state.view='explore';render();this.persist();$('#interaction-lab').scrollIntoView({block:'start'});return;}
    if(command==='exit'){if(!await this.allowLeave('dashboard',true))return;if(['add','edit'].includes(state.page))state.form=null;this.enabled=false;this.status='ready';navigate('dashboard');return;}
    if(command==='preview-action'||command==='preview-nav'){
      if(!['a','b','c'].includes(value))return;
      if(command==='preview-action')this.action=value;else this.navigation=value;
      renderPhone();this.controls();this.focusCommand(command,value);return;
    }
    if(command==='choose-action'||command==='choose-nav'){
      if(!['a','b','c'].includes(value))return;
      this.stored[command==='choose-action'?'action':'navigation']=value;this.persist();this.controls();this.focusCommand(command,value);return;
    }
    if(command==='clear'){this.stored={action:null,navigation:null};this.persist();this.controls();return;}
    if(command==='add'){navigate('add');$('#phone-stage').scrollIntoView({block:'center'});return;}
    if(command==='edit'){if(!selected())state.selected=state.data.find(d=>!d.deleted)?.id;navigate('edit');$('#phone-stage').scrollIntoView({block:'center'});return;}
    if(command==='export'){
      const copy=['# Duewise interaction preferences','',`Action layout: ${this.stored.action?this.stored.action.toUpperCase()+' / '+this.actionNames[this.stored.action]:'Not chosen'}`,`Navigation behavior: ${this.stored.navigation?this.stored.navigation.toUpperCase()+' / '+this.navNames[this.stored.navigation]:'Not chosen'}`,'','Focus navbar visual style retained. These choices record prototype preferences only; final implementation requires separate approval.','Previewed defaults do not count as choices.'].join('\n');
      const url=URL.createObjectURL(new Blob([copy],{type:'text/markdown;charset=utf-8'})),link=document.createElement('a');link.href=url;link.download='duewise-interaction-choices.md';link.click();setTimeout(()=>URL.revokeObjectURL(url),1000);
    }
  },
  focusCommand(command,value){document.querySelector(`[data-lab="${command}"][data-value="${value}"]`)?.focus({preventScroll:true});},
  dirty(){
    if(['add','edit'].includes(state.page))return !!state.form&&JSON.stringify(state.form)!==this.baseline;
    if(!this.enabled)return false;
    const parentDirty=!!state.settingsDraft&&JSON.stringify(state.settingsDraft)!==JSON.stringify(state.prefs);
    if(state.page==='settings'||state.page==='catalog')return parentDirty;
    if(state.page==='reminders')return parentDirty||this.reminderDirty();
    return false;
  },
  reminderDirty(){return !!state.reminderDraft&&['before','time','quietStart','quietEnd'].some(key=>state.reminderDraft[key]!==state.reminder[key]);},
  async allowLeave(target,force=false){
    if(!this.enabled&&!['add','edit'].includes(state.page))return true;
    if(this.saving||this.asking)return false;
    // Settings' nested pages retain the parent draft, so opening them is safe.
    if(!this.dirty()||(!force&&state.page==='settings'&&['catalog','reminders'].includes(target)))return true;
    if(!force&&target==='settings'&&(state.page==='catalog'||state.page==='reminders'&&!this.reminderDirty()))return true;
    this.asking=true;const cancel=$('#confirmation button[value=cancel]');cancel.textContent='Keep editing';
    try{return await confirm('Discard unsaved changes?','Your draft has not been saved. Keep editing to return with everything intact, or discard it and leave.','Discard changes',true);}
    finally{this.asking=false;cancel.textContent='Cancel';}
  },
  didNavigate(previous,page){
    if(['add','edit'].includes(page))this.baseline=JSON.stringify(state.form);
    if(!this.enabled)return;
    if(roots.includes(previous))this.origin=previous;
    if(roots.includes(previous)&&page==='edit')state.parent=previous;
    if(['catalog','reminders'].includes(previous)&&page!=='settings')state.settingsDraft=null;
    this.status='ready';
  },
  header(page,original){
    if((!this.enabled&&!this.focusSelected)||!['add','edit'].includes(page)||page==='edit'&&!selected())return original;
    const layout=this.enabled?this.action:'a';
    return `${layout==='c'?`<button class="header-back" data-action="back">${icon('back')}Cancel</button>`:'<span class="lab-header-space"></span>'}<span class="subpage-title">${page==='edit'?'Edit due':'Add due'}</span>${page==='edit'?`<button class="icon-button danger" data-action="delete" aria-label="Delete due">${icon('trash')}</button>`:'<span></span>'}`;
  },
  actionHTML(layout=this.action){return `${layout!=='c'?'<button class="app-button secondary" type="button" data-action="back">Cancel</button>':''}<button class="app-button primary" type="submit" form="due-form">${icon('check')}<span>Save</span></button>`;},
  afterRender(){
    this.controls();const phone=$('#phone'),bar=$('#app-actions');
    if(!this.enabled){
      this.unlayer();bar.classList.remove('lab-actions');$('#lab-feedback')?.remove();$('#lab-keyboard-space')?.remove();
      phone.dataset.labPersistentActions=String(roots.includes(state.page));
      if(this.focusSelected&&$('#due-form')&&!bar.hidden){
        $('#app-header').innerHTML=this.header(state.page,$('#app-header').innerHTML);
        bar.classList.add('lab-actions');bar.dataset.layout='a';bar.innerHTML=this.actionHTML('a');
      }
      this.layout();return;
    }
    const pushed=!roots.includes(state.page),origin=this.origin;
    phone.dataset.labPersistentActions=String(!pushed||this.navigation==='a');
    $('#app-header').innerHTML=this.header(state.page,$('#app-header').innerHTML);
    $('#app-nav').innerHTML=nav(pushed?origin:state.page);
    $('#app-nav').hidden=pushed&&this.navigation==='b';
    bar.classList.toggle('lab-actions',['add','edit'].includes(state.page));bar.dataset.layout=this.action;
    if(['add','edit'].includes(state.page)){bar.innerHTML=this.actionHTML();bar.hidden=false;}
    if(pushed&&this.navigation==='c')this.showLayer(origin);else this.unlayer();
    this.paintActions();this.layout();
  },
  paintActions(){
    if(!this.enabled)return;
    $('#lab-state').value=this.status;$('#lab-state').disabled=this.saving;
    const editing=['add','edit'].includes(state.page),busy=this.saving||this.status==='loading',disabled=busy||this.status==='disabled';
    const bar=$('#app-actions');bar.setAttribute('aria-busy',String(busy));
    if(editing){
      bar.querySelectorAll('button').forEach(button=>{button.disabled=disabled;});
      const label=bar.querySelector('[type=submit] span');if(label)label.textContent=busy?'Saving…':'Save';
      $('#due-form')?.setAttribute('aria-busy',String(busy));
      $('#due-form')?.querySelectorAll('input,select,textarea,button').forEach(field=>{field.disabled=busy;});
      $('#app-header').querySelectorAll('button').forEach(button=>{button.disabled=busy;});
    }
    let note=$('#lab-feedback');
    if(!note){note=document.createElement('div');note.id='lab-feedback';note.className='lab-live-note';note.setAttribute('role','status');}
    bar.before(note);
    note.hidden=!editing;note.dataset.status=this.status;
    const messages={ready:`Preview ${this.action.toUpperCase()} actions / ${this.navigation.toUpperCase()} navigation. Save affects sample data only.`,disabled:'Disabled preview: Save is unavailable. Choose Ready to continue.',loading:'Loading preview: saving is simulated; duplicate submissions are blocked.',error:'Save error preview: nothing was written. Your draft is intact. Choose Ready to retry.',success:'Success preview: this message demonstrates feedback; it has not saved your draft.'};
    note.textContent=this.saving?'Saving sample due… Please wait.':messages[this.status];
    $('#app-nav').querySelectorAll('button').forEach(button=>{button.disabled=busy&&editing;});
    this.positionLayer();
  },
  async save(event,commit){
    event.preventDefault();
    if(this.saving||['loading','disabled'].includes(this.status))return;
    if(this.status==='error'){this.paintActions();return;}
    if(!['title','amount','date'].map(validate).every(Boolean)){focusEditorError(app('[aria-invalid=true]'));return;}
    this.saving=true;this.paintActions();
    // This delay exists solely to make the in-memory prototype's busy state observable.
    await new Promise(resolve=>setTimeout(resolve,450));
    this.saving=false;this.status='ready';this.paintActions();
    commit(event);this.controls();
  },
  showLayer(origin){
    if(!this.layer){
      this.parent=document.createElement('div');this.parent.className='lab-parent';this.parent.setAttribute('aria-hidden','true');this.parent.inert=true;
      this.layer=document.createElement('dialog');this.layer.id='lab-form-layer';this.layer.className='lab-form-layer';this.layer.setAttribute('aria-label','Focused task preview');
      this.layer.addEventListener('cancel',event=>{event.preventDefault();act('back');});
      $('#app-nav').before(this.parent,this.layer);
      const tools=document.createElement('div');tools.className='lab-layer-tools';
      tools.innerHTML='<label>Actions<select data-lab-switch="action" aria-label="Preview action layout"><option value="a">A / Sticky</option><option value="b">B / Primary</option><option value="c">C / Header</option></select></label><label>Navigation<select data-lab-switch="navigation" aria-label="Preview navigation behavior"><option value="a">A / Persistent</option><option value="b">B / Context</option><option value="c">C / Layered</option></select></label><label>Theme<select data-lab-switch="mode" aria-label="Preview layer appearance"><option value="light">Light</option><option value="dark">Dark</option></select></label>';
      this.layer.append(tools);
      this.layer.append($('#app-header'),$('#app-body'),...($('#lab-feedback')?[$('#lab-feedback')]:[]),$('#app-actions'));
    }
    this.parent.innerHTML=`<div class="app-header">${header(origin)}</div><div class="lab-parent-content">${content(origin)}</div>`;
    if(!this.layer.querySelector('[data-lab-switch=status]')){
      const label=document.createElement('label');label.innerHTML='State<select data-lab-switch="status" aria-label="Preview layer action state"><option value="ready">Ready</option><option value="disabled">Disabled</option><option value="loading">Loading</option><option value="error">Error</option><option value="success">Success</option></select>';this.layer.querySelector('.lab-layer-tools').append(label);
    }
    for(const kind of ['action','navigation','mode','status'])this.layer.querySelector(`[data-lab-switch=${kind}]`).value=kind==='mode'?state.modes[state.concept]:this[kind];
    if(!this.layer.open){$('#phone-stage').scrollIntoView({block:'center'});this.positionLayer();this.layer.showModal();}
    this.positionLayer();
  },
  unlayer(){
    if(!this.layer)return;
    $('#app-nav').before($('#app-header'),$('#app-body'),...($('#lab-feedback')?[$('#lab-feedback')]:[]),$('#app-actions'));
    this.layer.close();this.layer.remove();this.parent.remove();this.layer=null;this.parent=null;
  },
  layout(){
    if(!this.enabled&&!this.focusSelected)return;
    const viewport=window.visualViewport,available=Math.max(220,(viewport?.height||innerHeight)-30);
    document.documentElement.style.setProperty('--lab-viewport',available+'px');
    $('#phone').dataset.labCompact=String($('#phone').getBoundingClientRect().height<610);
    if(!this.enabled)return;
    let keyboard=$('#lab-keyboard-space');if(!keyboard){keyboard=document.createElement('div');keyboard.id='lab-keyboard-space';keyboard.className='lab-keyboard';keyboard.setAttribute('aria-hidden','true');keyboard.innerHTML='Q W E R T Y U I O P<br>A S D F G H J K L<br>KEYBOARD SPACE / DEMO';$('#app-nav').after(keyboard);}
    keyboard.hidden=!this.keyboard||(viewport&&innerHeight-viewport.height>120);
    this.positionLayer();
  },
  positionLayer(){
    if(!this.layer)return;
    const phone=$('#phone').getBoundingClientRect(),viewport=window.visualViewport;
    const offset=viewport?.offsetTop||0,bottom=offset+(viewport?.height||innerHeight)-8;
    const reserved=($('#app-nav').hidden?0:$('#app-nav').getBoundingClientRect().height+18)+($('#lab-keyboard-space')?.getBoundingClientRect().height||0);
    const top=Math.max(offset+8,Math.min(phone.top+38,bottom-240));
    const height=Math.max(140,Math.min(phone.bottom-reserved-8,bottom)-top);
    Object.assign(this.layer.style,{top:top+'px',left:Math.max(4,phone.left+8)+'px',width:Math.max(200,phone.width-16)+'px',height:height+'px'});
  }
};
