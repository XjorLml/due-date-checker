'use strict';
const M=DuewiseModel,E=DuewiseExplore,D=DuewiseDesign,C=DuewisePH;
const catalog=new C.Catalog({getItem:key=>localStorage.getItem(key),setItem:(key,value)=>localStorage.setItem(key,value)});
const $=s=>document.querySelector(s),app=s=>$('#phone '+s);
const esc=value=>String(value??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const paths={
  dashboard:'<rect x="3" y="3" width="7" height="7" rx="2"/><rect x="14" y="3" width="7" height="11" rx="2"/><rect x="3" y="14" width="7" height="7" rx="2"/><path d="M14 18h7v3h-7z"/>',
  calendar:'<rect x="3" y="5" width="18" height="16" rx="3"/><path d="M7 3v4M17 3v4M3 10h18M8 14h2M14 14h2M8 17h2"/>',
  dues:'<rect x="5" y="3" width="14" height="18" rx="3"/><path d="M9 8h6M9 12h6M9 16h3"/>',
  settings:'<path d="m10 3-1 3-3 1-3 3 2 2-1 3 3 3 3-1 2 2 3-1 1-3 3-1 2-3-2-2 1-3-3-2-3 1-1-2z"/><circle cx="12" cy="12" r="3"/>',
  bell:'<path d="M6 9a6 6 0 0 1 12 0c0 7 3 7 3 9H3c0-2 3-2 3-9M10 21h4"/>',
  add:'<path d="M12 5v14M5 12h14"/>',back:'<path d="m14 6-6 6 6 6"/>',next:'<path d="m9 6 6 6-6 6"/>',arrow:'<path d="M5 12h14M14 7l5 5-5 5"/>',
  search:'<circle cx="10.5" cy="10.5" r="6.5"/><path d="m16 16 5 5"/>',check:'<path d="m5 12 4 4L19 6"/>',edit:'<path d="m15 5 4 4M4 20l5-1L21 7a2.8 2.8 0 0 0-4-4L5 15z"/>',
  trash:'<path d="M3 6h18M9 6V3h6v3M5 6l1 15h12l1-15M10 10v7M14 10v7"/>',repeat:'<path d="m17 2 4 4-4 4M3 11V8a2 2 0 0 1 2-2h16M7 22l-4-4 4-4M21 13v3a2 2 0 0 1-2 2H3"/>',
  Bill:'<path d="M6 3h12v18l-3-2-3 2-3-2-3 2zM9 7h6M9 11h6M9 15h3"/>',Task:'<rect x="4" y="4" width="16" height="16" rx="4"/><path d="m8 12 3 3 5-6"/>',Subscription:'<path d="m12 3 8 5v8l-8 5-8-5V8zM4 8l8 5 8-5M12 13v8"/>',Other:'<rect x="4" y="4" width="16" height="16" rx="5"/><path d="M8 12h8M12 8v8"/>',
  shield:'<path d="m12 3 8 3v6c0 4-5 8-8 9-3-1-8-5-8-9V6zM8 12l3 3 5-6"/>',refresh:'<path d="M20 11a8 8 0 1 0-2 7M20 4v7h-7"/>',info:'<circle cx="12" cy="12" r="9"/><path d="M12 11v6M12 7h.01"/>',star:'<path d="m12 3 2.8 5.8 6.4.9-4.6 4.5 1.1 6.3-5.7-3-5.7 3 1.1-6.3-4.6-4.5 6.4-.9z"/>'
};
Object.assign(paths,{home:'<path d="m3 10 9-7 9 7M5 9v12h14V9M9 21v-7h6v7"/>',bolt:'<path d="m13 2-9 12h7l-1 8 10-13h-7z"/>',credit:'<rect x="2" y="5" width="20" height="14" rx="3"/><path d="M2 10h20M6 15h3"/>',government:'<path d="m3 8 9-5 9 5zM5 10v8M10 10v8M14 10v8M19 10v8M3 21h18"/>',health:'<path d="M9 3h6v6h6v6h-6v6H9v-6H3V9h6z"/>',school:'<path d="m2 8 10-5 10 5-10 5zM6 11v6c4 3 8 3 12 0v-6M22 8v8"/>',family:'<circle cx="8" cy="8" r="3"/><circle cx="17" cy="9" r="2"/><path d="M2 21v-3a6 6 0 0 1 12 0v3M15 14a5 5 0 0 1 7 5v2"/>',business:'<rect x="3" y="7" width="18" height="14" rx="3"/><path d="M8 7V3h8v4M3 12h18M10 12v3h4v-3"/>'});
Object.assign(paths,{water:'<path d="M12 2C9 7 4 11 4 16a8 8 0 0 0 16 0c0-5-5-9-8-14zM8 16a4 4 0 0 0 4 4"/>',wifi:'<path d="M2 8a16 16 0 0 1 20 0M5 12a11 11 0 0 1 14 0M8 16a6 6 0 0 1 8 0M12 20h.01"/>',mobile:'<rect x="6" y="2" width="12" height="20" rx="3"/><path d="M10 5h4M11 19h2"/>',tv:'<rect x="2" y="6" width="20" height="15" rx="3"/><path d="m7 2 5 4 5-4"/>'});
const icon=name=>`<svg viewBox="0 0 24 24" aria-hidden="true">${paths[name]||paths.dues}</svg>`;
const roots=['dashboard','calendar','dues','settings'];
const rootNames={dashboard:'Dashboard',calendar:'Calendar',dues:'Dues',settings:'Settings'};
const repeats={none:'Does not repeat',weekly:'Every week',monthly:'Every month',quarterly:'Every quarter',yearly:'Annually',custom:'Custom interval'};
const storageKey='duewise.design-exploration.v1';
let storageAvailable=true,saved={};
try{saved=JSON.parse(localStorage.getItem(storageKey)||'{}')||{};}catch{storageAvailable=false;}
const preferences={chosen:'a',refinement:'focus',notes:Object.fromEntries(D.concepts.map(c=>[c.id,typeof saved.notes?.[c.id]==='string'?saved.notes[c.id]:'']))};
const state={view:'compare',concept:'a',modes:Object.fromEntries(D.concepts.map(c=>[c.id,c.defaultMode])),comparePage:'dashboard',compareMode:'signature',page:'dashboard',data:C.localizeSeed(E.seed()),selected:'rent',parent:'dashboard',form:null,formReturn:'dashboard',query:'',filter:'All',category:'All categories',calendarView:'month',year:2026,month:10,date:M.TODAY,sampleMode:'normal',scrolls:{},prefs:{appearance:'system',week:'monday'},settingsDraft:null,reminder:{enabled:false,before:true,time:'09:00',quietStart:'22:00',quietEnd:'08:00'},reminderDraft:null,reminderReturn:'settings'};
const concept=()=>D.concepts.find(c=>c.id===state.concept);
const dateText=value=>new Intl.DateTimeFormat('en-PH',{day:'2-digit',month:'short',year:'numeric',timeZone:'Asia/Manila'}).format(new Date(value+'T12:00:00Z'));
const shortDate=value=>new Intl.DateTimeFormat('en-PH',{day:'2-digit',month:'short',timeZone:'Asia/Manila'}).format(new Date(value+'T12:00:00Z'));
const statusClass=value=>({'Paid':'paid','Overdue':'overdue','Due today':'today','Upcoming':'upcoming'}[value]);
const selected=()=>state.data.find(d=>d.id===state.selected&&!d.deleted);
const liveData=()=>state.sampleMode==='empty'?[]:M.sorted(state.data);
function paletteStyle(c,mode){return Object.entries(c[mode]).map(([k,v])=>`--c-${k.replace(/[A-Z]/g,m=>'-'+m.toLowerCase())}:${v}`).join(';')+`;--radius:${D.tokens.radii[c.layout]}px`;}
function setPalette(el,c,mode){el.dataset.concept=c.id;el.dataset.mode=mode;el.dataset.refinement=c.id==='a'?preferences.refinement:'none';el.style.cssText=paletteStyle(c,mode);}
function persist(){try{localStorage.setItem(storageKey,JSON.stringify(preferences));storageAvailable=true;}catch{storageAvailable=false;}$('#storage-status').textContent=storageAvailable?'Your choice and notes stay in this browser.':'Storage is unavailable. Export your brief before leaving.';}
function toast(copy){const el=$('#app-toast');el.textContent=copy;el.classList.add('visible');clearTimeout(toast.timer);toast.timer=setTimeout(()=>el.classList.remove('visible'),4000);}
function clearToast(){clearTimeout(toast.timer);$('#app-toast').classList.remove('visible');}
function screenTitle(title,caption=''){return `<h1 class="page-title">${title}</h1>${caption?`<p class="day-caption">${caption}</p>`:''}`;}
function section(title,action='',label='View all'){return `<div class="section-line"><h2 class="section-title">${title}</h2>${action?`<button class="section-link" data-action="${action}">${label}${icon('next')}</button>`:''}</div>`;}
function row(d){
  const status=M.status(d),category=phCategoryName(d),provider=phProviderName(d);
  return `<button class="due-row" data-due="${esc(d.id)}" aria-label="${esc(`${d.title}, ${category}, ${provider||'No biller'}, ${M.money(d.amount)}, due ${dateText(d.date)}, ${status}`)}">${phCategoryIcon(d)}<span class="row-main"><span class="row-title">${esc(d.title)}</span><span class="row-meta">${shortDate(d.date)} \u00b7 ${esc(category)}</span>${provider?`<span class="row-provider">${esc(provider)}</span>`:''}${d.repeat!=='none'?`<span class="row-meta">${icon('repeat')}${esc(repeats[d.repeat])}</span>`:''}</span><span class="row-end"><span class="row-amount ${d.amount===null?'absent':''}">${M.money(d.amount)}</span><span class="row-status ${statusClass(status)}">${status==='Paid'?'\u2713':status==='Overdue'?'!':'\u25cb'} ${status}</span></span></button>`;
}
function filteredData(data){return M.filter(data,state.query,state.filter,'All categories').filter(d=>(state.category==='All categories'||d.categoryId===state.category)&&(!state.onlyRepeats||d.repeat!=='none'));}

function stats(summary){return `<div class="stats-strip"><button class="stat overdue" data-status-link="Overdue"><span>Overdue</span><strong>${summary.Overdue}</strong><span>needs attention</span></button><button class="stat urgent" data-status-link="Due today"><span>Due today</span><strong>${summary['Due today']}</strong><span>on your radar</span></button><button class="stat paid" data-status-link="Paid"><span>Paid</span><strong>${summary.Paid}<small> / ${summary.total}</small></strong><span>recorded by you</span></button></div>`;}
function hero(o){return `<div class="hero-card">${state.concept==='c'?'<div class="hero-greeting">A little more in control.</div>':''}<div class="hero-label">${icon('calendar')}Upcoming · November</div><div class="hero-amount">${M.money(o.aheadAmount)}</div><div class="hero-meta">${o.ahead.length} unpaid dues · today to 30 Nov${o.withoutAmount?`<br>${o.withoutAmount} without an amount, excluded from total`:''}</div>${o.next?`<button class="next-up" data-due="${esc(o.next.id)}"><span><strong>${esc(o.next.title)}</strong>Next due · ${dateText(o.next.date)}</span>${icon('arrow')}</button>`:'<p class="hero-footnote">No unpaid dues coming up this month.</p>'}</div>`;}
function weeklyChart(o){const max=o.weeks.reduce((n,w)=>BigInt(w.amount)>n?BigInt(w.amount):n,1n);return `<section class="chart-panel"><h3>How the month stacks up <span class="concept-tag">Concept</span></h3><p>Unpaid known amounts by due-date week</p><div class="week-chart" role="img" aria-label="${esc(o.weeks.map(w=>`${w.label} November: ${M.money(w.amount)}, ${w.count} dues`).join('; '))}">${o.weeks.map(w=>`<div><span class="chart-bar" style="height:${Math.max(3,Number(BigInt(w.amount)*90n/max))}%" aria-hidden="true"></span><small>${w.label}</small></div>`).join('')}</div><p>November · Due dates, not payment history</p></section>`;}
function dashboard(mini=false){
  const o=E.overview(liveData());
  if(!o.active.length)return screenTitle('A fresh start.','Your next chapter of clarity.')+empty('Your first due starts here.','Add a bill, subscription or task. We’ll help you see what comes next.','add','Add a due');
  if(state.concept==='a'&&preferences.refinement==='focus')return focusDashboard(o,mini);
  const urgent=o.overdue.length?`<div class="notice alert">${icon('info')}<button data-status-link="Overdue"><strong>${o.overdue.length} overdue ${o.overdue.length===1?'due':'dues'} · ${M.money(o.overdueAmount)}</strong><br>Take a look when you’re ready. ${mini?'':'→'}</button></div>`:'';
  const list=section(state.concept==='d'?'Your due-date timeline':'Next in line','all-dues')+`<div class="rows">${o.ahead.slice(0,mini?2:3).map(row).join('')}</div>`;
  const recur=section('Repeats on your radar','recurring-dues','See repeats')+`<div class="recurring-strip">${o.recurring.slice(0,2).map(d=>`<button class="recurring-tile" data-due="${esc(d.id)}">${icon('repeat')}<span><strong>${esc(d.title)}</strong><small>${repeats[d.repeat]} · concept preference</small></span>${icon('next')}</button>`).join('')}</div>`;
  const title=screenTitle(state.concept==='c'?'Hello, a clearer day.':state.concept==='b'?'Everything, in focus.':state.concept==='d'?'Your month in perspective.':'Your overview.','Wednesday, 04 November');
  const payment=`<div class="progress-label"><span>Paid progress · all saved dues</span><strong>${o.counts.Paid} of ${o.counts.total}</strong></div><div class="progress-line" role="progressbar" aria-label="Paid dues" aria-valuenow="${o.counts.Paid}" aria-valuemin="0" aria-valuemax="${o.counts.total}"><span style="width:${o.counts.total?o.counts.Paid/o.counts.total*100:0}%"></span></div>`;
  const category=section('Dues by category')+`<div class="chart-panel"><p>All active sample dues · counts, not spending</p><div class="category-breakdown">${o.categories.filter(c=>c.count).map(c=>`<div class="breakdown-item"><span>${c.category}</span><div class="breakdown-track" aria-hidden="true"><span style="width:${c.count/o.counts.total*100}%"></span></div><strong>${c.count}</strong></div>`).join('')}</div></div>`;
  if(state.concept==='d')return title+hero(o)+stats(o.counts)+weeklyChart(o)+list+category+recur;
  if(state.concept==='c'){
    const task=o.open.find(d=>d.category==='Task');
    return title+hero(o)+urgent+(task?section('A small next step')+`<button class="today-task" data-due="${esc(task.id)}"><span class="eyebrow">${icon('Task')}TODAY’S TASK</span><strong>${esc(task.title)}</strong><p>No amount to track. Just a date to remember. →</p></button>`:'')+section('Look how far you’ve come')+payment+list+recur;
  }
  return title+hero(o)+(state.concept==='a'?stats(o.counts):urgent+stats(o.counts))+list+section('Your progress')+payment+recur+`<p class="tiny-copy">${icon('shield')}Local-first clarity. No bank connection.</p>`;
}
function focusDashboard(o,mini=false){
  const attention=o.open.filter(d=>d.date<=M.TODAY),later=o.ahead.filter(d=>d.date>M.TODAY);
  const next=o.next?`<button class="focus-next" data-due="${esc(o.next.id)}"><span>Next date · ${shortDate(o.next.date)}</span><strong>${esc(o.next.title)}</strong>${icon('arrow')}</button>`:'';
  return screenTitle('A clear plan for today.','Wednesday, 04 November')+`<section class="hero-card focus-summary"><div class="hero-label">Upcoming · November</div><div class="hero-amount">${M.money(o.aheadAmount)}</div><div class="hero-meta">${o.ahead.length} unpaid · today to 30 Nov${o.withoutAmount?` · ${o.withoutAmount} without an amount, excluded`:''}</div>${next}</section><div class="focus-status"><button data-status-link="Overdue"><span class="focus-status-count overdue">${o.counts.Overdue}</span>Overdue</button><button data-status-link="Due today"><span class="focus-status-count today">${o.counts['Due today']}</span>Today</button><button data-status-link="Paid"><span class="focus-status-count paid">${o.counts.Paid}/${o.counts.total}</span>Paid</button></div>${section('Needs attention','all-dues','All dues')}<div class="rows attention-rows">${attention.length?attention.slice(0,mini?2:3).map(row).join(''):'<p class="tiny-copy">Nothing overdue or due today.</p>'}</div>${section('A little further ahead')}<div class="rows">${later.length?later.slice(0,3).map(row).join(''):'<p class="tiny-copy">No later dues this month.</p>'}</div><p class="tiny-copy">Counts are global. Amounts cover known unpaid dues from today to month end.</p>`;
}
function renderRefinements(){$('#minimal-refinements').hidden=true;}

function empty(title,copy,action,label){return `<div class="empty-state">${icon('dues')}<h2>${title}</h2><p>${copy}</p><button class="app-button primary" data-action="${action}">${label}</button></div>`;}
function dues(mini=false){
  const data=liveData(),result=filteredData(data);
  if(mini)return screenTitle('Your dues.','All your dates, in one place.')+`<div class="rows">${M.sorted(data).slice(0,5).map(row).join('')}</div>`;
  return screenTitle('Your dues.','Date order · all active saved dues')+`<label for="dues-search" class="search-label">Find by title</label><div class="search-wrap">${icon('search')}<input id="dues-search" class="search-input" type="search" value="${esc(state.query)}" placeholder="Search a bill or task"></div><div class="filter-wrap" aria-label="Status">${['All','Overdue','Due today','Upcoming','Paid'].map(s=>`<button class="chip" data-filter="${s}" aria-pressed="${state.filter===s}">${s}</button>`).join('')}</div><div class="category-filter"><span>Category</span><button type="button" class="picker-trigger" id="category-filter" data-ph="filter-category" aria-haspopup="dialog">${esc(state.category==='All categories'?'All categories':catalog.category(state.category).name)}${icon('next')}</button></div>${state.onlyRepeats?'<div class="notice">Repeat preferences only · concept<button data-action="clear-filters">Clear</button></div>':''}<div class="section-line"><h2 class="section-title" id="match-count" aria-live="polite">${result.length} matching dues</h2><button class="section-link" data-action="clear-filters">Reset filters</button></div><div class="rows" id="dues-results">${duesResults(result,data)}</div>`;
}
function duesResults(result,data){return result.length?result.map(row).join(''):data.length?empty('Nothing matches just yet.','Try another title, category or status.','clear-filters','Reset filters'):empty('Room for your first due.','Add a bill, subscription or task to begin.','add','Add a due');}
function updateResults(){const data=liveData(),result=filteredData(data);app('#match-count').textContent=`${result.length} matching dues`;app('#dues-results').innerHTML=duesResults(result,data);}
function calendarCells(dates){const data=liveData();return dates.map(date=>{if(!date)return '<span aria-hidden="true"></span>';const count=data.filter(d=>d.date===date).length;return `<button class="day ${date===M.TODAY?'today':''}" data-date="${date}" aria-pressed="${date===state.date}" aria-label="${dateText(date)}, ${count} dues${date===M.TODAY?', today':''}">${Number(date.slice(-2))}<span class="date-count">${count?count+' due'+(count===1?'':'s'):''}</span></button>`;}).join('');}
function calendar(){
  const monday=state.prefs.week==='monday',days=monday?['M','T','W','T','F','S','S']:['S','M','T','W','T','F','S'],labels=monday?['Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday']:['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];
  const month=new Intl.DateTimeFormat('en-PH',{month:'long',year:'numeric',timeZone:'Asia/Manila'}).format(new Date(`${String(state.year).padStart(4,'0')}-${String(state.month+1).padStart(2,'0')}-01T12:00:00Z`));
  const dates=state.calendarView==='week'?E.weekDates(state.date,state.prefs.week):M.calendar(state.year,state.month,state.prefs.week),agenda=liveData().filter(d=>d.date===state.date);
  const prevDisabled=state.calendarView==='week'?state.date<'0001-01-08':state.year===1&&state.month===0,nextDisabled=state.calendarView==='week'?state.date>'9999-12-24':state.year===9999&&state.month===11;
  return screenTitle('A date for everything.','Find your day. See what’s on it.')+`<div class="calendar-view-toggle" aria-label="Calendar view"><button data-calendar-view="month" aria-pressed="${state.calendarView==='month'}">Month</button><button data-calendar-view="week" aria-pressed="${state.calendarView==='week'}">Week <span class="concept-tag">Concept</span></button></div><div class="calendar-controls"><h2>${month}</h2><div class="calendar-arrows"><button class="icon-button" data-calendar-step="-1" aria-label="Previous ${state.calendarView}" ${prevDisabled?'disabled':''}>${icon('back')}</button><button class="icon-button" data-calendar-step="1" aria-label="Next ${state.calendarView}" ${nextDisabled?'disabled':''}>${icon('next')}</button></div></div><div class="calendar-scroll"><div class="week-labels">${days.map((d,i)=>`<span aria-label="${labels[i]}">${d}</span>`).join('')}</div><div class="calendar-grid">${calendarCells(dates)}</div></div><div class="section-line"><span class="tiny-copy">${monday?'Monday':'Sunday'} first · includes paid</span><button class="section-link" data-action="today">Today${icon('calendar')}</button></div>${section(dateText(state.date))}<div class="rows">${agenda.length?agenda.map(row).join(''):'<div class="notice">Nothing due. A little breathing room.</div>'}</div><p class="tiny-copy">Add defaults to Today, independently of this selected date.</p>`;
}
function detail(){
  const d=state.sampleMode==='empty'?null:selected();
  if(!d)return empty('This due is unavailable.','It may have been deleted. Your other dues are still in the list.','back-to-parent','Back to list');
  return `${phCategoryIcon(d)}<h1 class="detail-title">${esc(d.title)}</h1><div class="detail-amount">${M.money(d.amount)}</div><span class="badge ${statusClass(M.status(d))}">${M.status(d)}</span><dl class="detail-facts"><div><dt>Due date</dt><dd>${dateText(d.date)}</dd></div><div><dt>Category</dt><dd>${esc(phCategoryName(d))}</dd></div><div><dt>Provider / biller</dt><dd>${esc(phProviderName(d)||'Not set')}</dd></div><div><dt>Due type</dt><dd>${d.category}</dd></div><div><dt>Repeat <span class="concept-tag">Concept</span></dt><dd>${esc(d.repeat==='custom'?'Every '+d.repeatEvery+' '+d.repeatUnit:repeats[d.repeat])}</dd></div><div><dt>Reminder offsets</dt><dd>${(d.reminderDays||[]).map(n=>n===0?'On due date':n+(n===1?' day before':' days before')).join(', ')||'None'}<small class="detail-simulation">Simulation only</small></dd></div><div><dt>Currency / timezone</dt><dd>PHP / Asia/Manila</dd></div></dl><div class="detail-quick"><button class="app-button primary" data-action="quick-paid">${icon('check')}${d.paid?'Mark unpaid':'Mark paid'}</button><button class="app-button" data-action="reschedule">${icon('calendar')}Reschedule</button></div><p class="tiny-copy">Quick status changes are a confirmed prototype shortcut. No payment is made.</p><section class="detail-note"><h2 class="section-title">Notes</h2><p>${esc(d.notes||'No notes added.')}</p></section><p class="tiny-copy">Created · ${d.created||'01 Nov 2026, 01:41 UTC'}<br>Updated · ${d.updated||'01 Nov 2026, 01:41 UTC'}</p>`;
}
function draft(editing){const d=editing?selected():null,days=d?.reminderDays||[0,1];return{title:d?.title||'',category:d?.category||'Bill',categoryId:d?.categoryId||'ph.other.general',providerLabel:d?phProviderName(d):'',amount:d?M.inputAmount(d.amount):'',date:d?.date||M.TODAY,notes:d?.notes||'',paid:d?.paid||false,repeat:d?.repeat||'none',repeatEvery:d?.repeatEvery||1,repeatUnit:d?.repeatUnit||'months',reminderDays:[...days],reminderExtra:days.filter(n=>![0,1,3,7].includes(n)).join(', ')};}

function amountPreview(){if(!state.form.amount.trim())return '';const r=M.parseAmount(editorAmount(),state.form.category);return r.error?'Use a valid PHP amount.':`Will save as ${M.money(r.value)}.`;}
function repeatPreview(){const d=state.form,next=E.nextOccurrence(d.date,d.repeat,Number(d.repeatEvery),d.repeatUnit);return d.repeat==='none'?'One date, no repeat preference.':next?`Next preview: ${dateText(next)}. No future dues are generated.`:'No next date within the supported calendar.';}
function editorSection(id,label,value,body,extra=''){
  return `<details id="${id}" class="editor-disclosure ${extra}" data-editor-section="${id}" ${state.editorOpen?.[id]?'open':''}><summary><span class="summary-label">${label}</span><span class="summary-value" data-editor-value="${id}">${esc(value)}</span></summary><div class="editor-disclosure-content">${body}</div></details>`;
}
function rememberEditorSections(){
  const form=app('#due-form');if(!form||Number(form.dataset.editorToken)!==state.editorToken)return;
  state.editorOpen=Object.fromEntries([...form.querySelectorAll('details[data-editor-section]')].map(el=>[el.id,el.open]));
}
function focusEditorError(input){
  if(!input)return;
  for(let el=input.parentElement;el;el=el.parentElement)if(el.tagName==='DETAILS'){el.open=true;(state.editorOpen||={})[el.id]=true;}
  input.focus();input.scrollIntoView({block:'nearest'});
}
function editorAmount(){return DuewiseEssential.normalizeAmount(state.form.amount);}
function refreshEditorSummaries(){
  if(!state.form||!app('#due-form'))return;
  const f=state.form,values={'type-options':`Type: ${f.category}`,'repeat-options':f.repeat==='none'?"Doesn't repeat":f.repeat==='custom'?`Every ${f.repeatEvery} ${f.repeatUnit}`:repeats[f.repeat],'reminder-options':DuewiseEssential.reminderSummary(f,state.reminder),'extra-details':[f.providerLabel.trim(),f.notes.trim()?'Notes added':''].filter(Boolean).join(' · ')||'Optional','custom-reminder-options':f.reminderExtra.trim()||'Optional'};
  for(const [id,value] of Object.entries(values)){const el=app(`[data-editor-value="${id}"]`);if(el)el.textContent=value;}
}
function editor(){
  if(state.page==='edit'&&!selected())return empty('This due is unavailable.','It may have been deleted. Your other dues are safe.','back-to-parent','Back');
  const f=state.form||(state.form=draft(state.page==='edit')),required=['Bill','Subscription'].includes(f.category);
  const repeatBody=`<div class="field"><label for="due-repeat">Repeat <span class="optional-label">optional</span></label><select id="due-repeat" name="repeat">${Object.entries(repeats).map(([k,v])=>`<option value="${k}" ${k===f.repeat?'selected':''}>${v}</option>`).join('')}</select>${phScheduleFields(f)}<p id="repeat-preview" class="hint" aria-live="polite">${repeatPreview()}</p></div>`;
  const reminderBody=`<p class="reminder-state">${state.reminder.enabled?`App reminders are on at ${esc(state.reminder.time)} Manila; quiet hours ${esc(state.reminder.quietStart)}–${esc(state.reminder.quietEnd)}.`:'App reminders are off. Selected offsets are saved as preferences only.'} This HTML schedules no notifications.</p>${phReminderFields(f)}`;
  const extraBody=phProviderFields(f)+`<div class="field"><label for="due-notes">Notes <span class="optional-label">optional</span></label><textarea id="due-notes" name="notes" rows="3" placeholder="A little context for later">${esc(f.notes)}</textarea></div>`;
  return `<p class="editor-instructions">Required fields are marked.</p><form id="due-form" class="essential-form" data-editor-token="${state.editorToken||0}" novalidate>
    <div class="field"><label for="due-title">What is due? <span class="required-label">required</span></label><input id="due-title" name="title" value="${esc(f.title)}" placeholder="e.g. Electricity bill" required aria-describedby="title-error"><span id="title-error" class="error" role="status"></span></div>
    <div class="field"><label for="due-amount">Amount (PHP) <span class="required-label" id="amount-requirement">${required?'required':'optional'}</span></label><input id="due-amount" name="amount" inputmode="decimal" value="${esc(f.amount)}" placeholder="e.g. 1250.00" ${required?'required':''} aria-required="${required}" aria-describedby="amount-rule amount-preview amount-error"><p id="amount-rule" class="hint">${required?`Required for ${esc(f.category)}.`:'Blank means no amount; zero is a recorded amount.'}</p><p id="amount-preview" class="hint" aria-live="polite">${f.amount.trim()?amountPreview():''}</p><span id="amount-error" class="error" role="status"></span></div>
    <details id="type-options" class="editor-disclosure editor-type" data-editor-section="type-options" ${state.editorOpen?.['type-options']?'open':''}><summary><span data-editor-value="type-options">Type: ${esc(f.category)}</span><span class="type-change">Change</span></summary><div class="editor-disclosure-content"><div class="field"><label for="due-category">Due type</label><select id="due-category" name="category">${M.categories.map(c=>`<option ${c===f.category?'selected':''}>${c}</option>`).join('')}</select><p class="hint">Bill and Subscription require an amount. Task and Other can have no amount. Category stays separate.</p></div></div></details>
    <div class="field"><label for="due-date">Due date <span class="required-label">required</span></label><input id="due-date" name="date" type="date" value="${esc(f.date)}" min="0001-01-01" max="9999-12-31" required aria-describedby="date-error date-hint"><p id="date-hint" class="hint">${M.validDate(f.date)?dateText(f.date):'Choose a date'} · Manila. Past dates are allowed.</p><span id="date-error" class="error" role="status"></span></div>
    ${phCategoryFields(f)}
    ${editorSection('repeat-options','Repeat',f.repeat==='none'?"Doesn't repeat":f.repeat==='custom'?`Every ${f.repeatEvery} ${f.repeatUnit}`:repeats[f.repeat],repeatBody)}
    ${editorSection('reminder-options','Reminders',DuewiseEssential.reminderSummary(f,state.reminder),reminderBody)}
    ${editorSection('extra-details','Biller & notes',[f.providerLabel.trim(),f.notes.trim()?'Notes added':''].filter(Boolean).join(' · ')||'Optional',extraBody)}
    ${state.page==='edit'?`<label class="check-row" for="due-paid"><span>Paid · applies on Save</span><input id="due-paid" name="paid" type="checkbox" ${f.paid?'checked':''}></label>`:'<p class="tiny-copy">New dues start unpaid.</p>'}
  </form>`;
}
function settings(mini=false){
  const p=mini?state.prefs:state.settingsDraft||state.prefs;
  const segment=(name,values,value)=>`<div class="preference-segments" aria-label="${name}">${values.map(([key,label])=>`<button data-preference="${name}" data-value="${key}" aria-pressed="${value===key}">${label}</button>`).join('')}</div>`;
  return screenTitle('Make it feel like you.','A few preferences. A clearer routine.')+`<section class="settings-group"><h2>Appearance</h2><p>System follows your device. Save to apply.</p>${segment('appearance',[['system','System'],['light','Light'],['dark','Dark']],p.appearance)}</section><section class="settings-group"><h2>Start of week</h2><p>Your dates stay in Asia/Manila.</p>${segment('week',[['monday','Monday'],['sunday','Sunday']],p.week)}</section><button class="settings-row" data-action="reminders">${icon('bell')}<span><strong>Reminders</strong><small>${state.reminder.enabled?'On':'Off'} · ${state.reminder.time} Manila · simulation</small></span>${icon('next')}</button>${phManagementEntry()}<div class="notice">${icon('shield')}Local sample data. No account, bank connection or currency switch.</div><p class="tiny-copy">Save applies both preferences. Leaving this page discards a draft; a Reminders visit keeps it.</p>`;
}
function reminders(){
  const r=state.reminderDraft||state.reminder;
  return screenTitle('A gentle nudge.','Only when you ask for one.')+`<div class="notice ${state.reminder.enabled?'success':''}">${icon('bell')}<span><strong>Reminders ${state.reminder.enabled?'on':'off'}</strong><br>All active, unpaid dues · simulation only</span></div><form id="reminder-form"><section class="settings-group"><h2>Your schedule</h2><p>On each due date, with an optional day-before nudge.</p><label class="check-row" for="reminder-before"><span>Also one day before</span><input id="reminder-before" name="before" type="checkbox" ${r.before?'checked':''}></label><div class="field"><label for="reminder-time">Time · Asia/Manila</label><input id="reminder-time" name="time" type="time" value="${r.time}" required></div></section><section class="settings-group"><h2>Quiet hours</h2><div class="field"><label for="quiet-start">From</label><input id="quiet-start" name="quietStart" type="time" value="${r.quietStart}" required></div><div class="field"><label for="quiet-end">Until</label><input id="quiet-end" name="quietEnd" type="time" value="${r.quietEnd}" required></div><p>Reminders move to the next allowed time, skipping dates that have passed. Equal times turn quiet hours off.</p></section></form><details class="diagnostics"><summary>Notification readiness & privacy</summary><p>No OS permission is requested. Android permission and power states are unavailable in this browser. Real notifications omit titles, amounts and notes; Android delivery is inexact and may be delayed.</p><button class="diagnostic-row" data-action="test-notification">${icon('bell')}Preview a test notification</button><button class="diagnostic-row" data-action="android-settings">${icon('settings')}Android settings preview</button></details>`;
}
function content(page,mini=false){return({dashboard:()=>dashboard(mini),dues:()=>dues(mini),calendar,settings:()=>settings(mini),details:detail,add:editor,edit:editor,reminders,catalog:phManagement})[page]();}
function header(page){return roots.includes(page)?`<div class="app-brand"><span class="app-mark" aria-hidden="true">✓</span>duewise</div><div class="app-header-actions"><button class="icon-button" data-action="reminders" aria-label="Reminders" title="Reminders">${icon('bell')}</button>${state.concept==='a'?'':`<button class="icon-button accent" data-page="add" aria-label="Add due" title="Add due">${icon('add')}</button>`}</div>`:`<button class="header-back" data-action="back">${icon('back')}${['add','edit'].includes(page)?'Cancel':'Back'}</button><span class="subpage-title">${page==='details'?'Due details':page==='reminders'?'Reminders':page==='catalog'?'My Categories':page==='edit'?'Edit due':'Add due'}</span>${page==='details'&&selected()&&state.sampleMode!=='empty'?`<button class="icon-button" data-page="edit" aria-label="Edit due" title="Edit due">${icon('edit')}</button>`:'<span></span>'}`;}
function nav(page){return roots.map((id,i)=>`${state.concept==='a'&&i===2?`<button class="floating-add" data-page="add" aria-label="Add due" title="Add due"><span class="fab-disc">${icon('add')}</span><span class="fab-label">Add</span></button>`:''}<button data-page="${id}" ${id===page?'aria-current="page"':''}>${icon(id)}<span>${rootNames[id]}</span></button>`).join('');}
function renderGallery(){
  const old=state.concept;
  $('#concept-gallery').innerHTML=D.concepts.map(c=>{
    state.concept=c.id;const mode=state.compareMode==='signature'?c.defaultMode:state.compareMode;
    return `<article class="concept-card ${preferences.chosen===c.id?'selected':''}"><div class="concept-card-heading"><div class="concept-card-topline"><span class="concept-letter">${c.letter}</span><span class="${preferences.chosen===c.id?'selected-flag':'concept-signature'}">${preferences.chosen===c.id?'✓ Your preference':mode+' signature'}</span></div><h3>${c.name}</h3><p>${c.short}</p></div><div class="mock-frame" data-concept="${c.id}" data-mode="${mode}" data-refinement="${c.id==='a'?preferences.refinement:'none'}" style="${paletteStyle(c,mode)}" inert aria-hidden="true"><header class="app-header">${header(state.comparePage)}</header><div class="app-body">${content(state.comparePage,true)}</div><nav class="app-nav">${nav(state.comparePage)}</nav></div><div class="concept-card-footer"><button class="explore-button" data-explore="${c.id}" aria-label="Explore option ${c.letter}, ${c.name}">Explore ${c.letter}${icon('arrow')}</button><div class="swatches" aria-label="${c.name} palette">${['bg','text','accent','primary'].map(k=>`<span style="background:${c[mode][k]}" title="${k}: ${c[mode][k]}"></span>`).join('')}</div></div></article>`;
  }).join('');
  state.concept=old;
  const hierarchy=['Next amount, then the open ledger','Hero amount and separated cards','Gentle guidance and a task-first moment','Weekly amounts and dated timeline'];
  $('#comparison-rows').innerHTML=D.concepts.map((c,i)=>`<tr><td>${c.letter} / ${c.name}</td><td>${hierarchy[i]}</td><td>${c.traits.slice(0,2).join(' · ')}</td><td>${c.evaluate}</td></tr>`).join('');
}
function renderInspector(){
  const c=concept(),p=c[state.modes[c.id]];
  $('#concept-description').innerHTML=`<p class="eyebrow">${c.id==='a'?'FOCUS / SELECTED':'ARCHIVED DIRECTION '+c.letter}</p><span class="large-letter">${c.letter}</span><h2>${c.name}</h2><p>${c.description}</p><p class="reference">${c.reference}</p><div class="concept-traits">${c.traits.map(t=>`<span>${t}</span>`).join('')}</div><div class="evaluation"><h3>Something to notice</h3><p>${c.evaluate}</p></div><div class="token-samples" aria-label="Active palette">${['bg','surface','text','primary','accent'].map(k=>`<span style="background:${p[k]}" title="${k}: ${p[k]}"></span>`).join('')}</div><p class="token-description">Shared components. Different rhythm.<br>44 px actions · locally bundled type</p>`;
  $('#concept-switcher').innerHTML=D.concepts.map(x=>`<button data-concept="${x.id}" aria-pressed="${c.id===x.id}" aria-label="Preview option ${x.letter}, ${x.name}"><b>${x.letter}</b><span>${['Minimal','Premium','Friendly','Data'][D.concepts.indexOf(x)]}</span></button>`).join('');
  document.querySelectorAll('[data-mode]').forEach(el=>{if(el.tagName==='BUTTON')el.setAttribute('aria-pressed',String(el.dataset.mode===state.modes[c.id]));});
  $('#stage-label').textContent=`${c.letter} / ${c.name}`;
  $('#phone-stage').dataset.concept=c.id;
  $('#concept-notes').value=preferences.notes[c.id];
  const chosen=preferences.chosen===c.id;
  $('#select-concept').classList.toggle('chosen',chosen);
  $('#select-concept').innerHTML=chosen?'Your preferred direction <span>✓</span>':'Choose this direction <span>↗</span>';
  $('#selection-status').textContent=preferences.chosen?`Preference saved: ${D.concepts.find(x=>x.id===preferences.chosen).letter} / ${D.concepts.find(x=>x.id===preferences.chosen).name}. You can keep exploring.`:'No preferred direction selected.';
  $('#clear-selection').hidden=!preferences.chosen;
  $('#storage-status').textContent=storageAvailable?'Your choice and notes stay in this browser.':'Storage is unavailable. Export your brief before leaving.';
  renderRefinements();
}
function renderActions(){
  const page=state.page,bar=$('#app-actions');bar.innerHTML='';
  if(!['normal','stale'].includes(state.sampleMode)){bar.hidden=true;return;}
  if(page==='add'||page==='edit'&&selected())bar.innerHTML=`${page==='edit'?`<button class="icon-button danger" data-action="delete" aria-label="Delete due" title="Delete due">${icon('trash')}</button>`:''}<button class="app-button primary" type="submit" form="due-form">${icon('check')}Save</button>`;
  if(page==='settings')bar.innerHTML=`<button class="app-button primary" data-action="save-settings">${icon('check')}Save</button>`;
  if(page==='reminders')bar.innerHTML=`${state.reminder.enabled?'<button class="app-button secondary" data-action="disable-reminders">Disable</button>':'<button class="app-button secondary" type="submit" form="reminder-form" data-enable="false">Save only</button>'}<button class="app-button primary" type="submit" form="reminder-form" data-enable="${!state.reminder.enabled}">${icon(state.reminder.enabled?'check':'bell')}${state.reminder.enabled?'Save schedule':'Save & enable'}</button>`;
  bar.hidden=!bar.innerHTML;
}
function renderPhone(preserveScroll=true){
  rememberEditorSections();
  const el=$('#app-body'),scroll=preserveScroll?el.scrollTop:state.scrolls[state.page]||0;
  setPalette($('#phone'),concept(),state.modes[state.concept]);
  $('#app-header').innerHTML=header(state.page);$('#app-nav').innerHTML=nav(state.page);$('#app-nav').hidden=!roots.includes(state.page);renderActions();
  if(state.sampleMode==='loading')el.innerHTML=`<div role="status">${screenTitle('A moment of clarity…','Loading sample dues.')}<div class="skeleton"><span></span><span></span></div><div class="skeleton"><span></span><span></span></div></div><button class="app-button" data-action="retry">Show sample data</button>`;
  else if(state.sampleMode==='error')el.innerHTML=empty('Couldn’t load your dues.','Your information has not been changed. Try loading again.','retry','Retry');
  else el.innerHTML=(state.sampleMode==='stale'?'<div class="notice warning">Showing the last sample snapshot.<button data-action="retry">Retry refresh</button></div>':'')+content(state.page);
  el.scrollTop=scroll;
  $('#screen-selector').value=roots.includes(state.page)||state.page==='reminders'?state.page:'dues';
  $('#sample-state').value=state.sampleMode;$('#sample-state').disabled=['add','edit','settings','reminders','catalog'].includes(state.page);
  Lab.afterRender();
}
function render(){
  document.body.dataset.view=state.view;$('#compare-view').hidden=state.view!=='compare';$('#explore-view').hidden=state.view!=='explore';$('#decision-view').hidden=state.view!=='decide';$('#notification-view').hidden=state.view!=='notify';
  document.querySelectorAll('[data-view]').forEach(el=>{if(el.closest('.view-toggle'))el.setAttribute('aria-pressed',String(el.dataset.view===state.view));});
  if(state.view==='compare')renderGallery();
  else if(state.view==='decide')window.DecisionLab.render();
  else if(state.view==='notify')window.NotificationLab.render();
  else{renderInspector();renderPhone();}
  const hash=Lab.enabled?`#interaction-lab/${state.page}`:state.view==='compare'?'#compare':state.view==='decide'?window.DecisionLab.hash():state.view==='notify'?window.NotificationLab.hash():`#${state.concept}/${state.page}`;
  if(location.hash!==hash)history.replaceState(null,'',hash);
}
async function navigate(page){
  if(![...roots,'details','add','edit','reminders','catalog'].includes(page))return;
  if(Lab.enabled||['add','edit'].includes(state.page)){
    if(page===state.page||Lab.saving||Lab.asking)return;
    if(Lab.dirty()&&!await Lab.allowLeave(page)){ $('#screen-selector').value=roots.includes(state.page)||state.page==='reminders'?state.page:'dues';return; }
  }
  clearToast();state.scrolls[state.page]=$('#app-body').scrollTop;
  const previous=state.page;
  if(roots.includes(previous)&&['details','add'].includes(page))state.parent=previous;
  if(page==='add'||page==='edit'){state.form=draft(page==='edit');state.formReturn=page==='edit'?'details':roots.includes(previous)?previous:state.parent;state.editorToken=(state.editorToken||0)+1;state.editorOpen={};state.scrolls[page]=0;}
  else if(['add','edit'].includes(previous))state.form=null;
  if(page==='settings'&&!['reminders','catalog'].includes(previous))state.settingsDraft={...state.prefs};
  if(previous==='settings'&&!['settings','reminders','catalog'].includes(page))state.settingsDraft=null;
  if(page==='reminders'){state.reminderReturn=previous==='settings'?'settings':roots.includes(previous)?previous:state.parent;state.reminderDraft={...state.reminder};}
  if(['add','edit','settings','reminders','catalog'].includes(page))state.sampleMode='normal';
  state.page=page;state.view='explore';Lab.didNavigate(previous,page);render();$('#app-body').scrollTop=state.scrolls[page]||0;
}
function openConcept(id){if(!D.concepts.some(c=>c.id===id))return;state.concept=id;state.view='explore';render();}
async function confirm(title,copy,label='Confirm',danger=false){const el=$('#confirmation');phThemeDialog(el);$('#confirmation-title').textContent=title;$('#confirmation-copy').textContent=copy;$('#confirmation-yes').textContent=label;$('#confirmation-yes').classList.toggle('danger',danger);el.returnValue='';el.showModal();return new Promise(resolve=>el.addEventListener('close',()=>resolve(el.returnValue==='confirm'),{once:true}));}
function validate(name){const f=state.form;let error='';if(name==='title'&&!f.title.trim())error='Enter a title.';if(name==='amount')error=M.parseAmount(DuewiseEssential.normalizeAmount(f.amount),f.category).error||'';if(name==='date'&&!M.validDate(f.date))error='Choose a valid date.';const input=app('#due-'+name),label=app('#'+name+'-error');if(input&&label){input.setAttribute('aria-invalid',String(Boolean(error)));label.textContent=error;}return !error;}
function saveDue(event){if(Lab.enabled)return Lab.save(event,saveDueNow);return saveDueNow(event);}
function saveDueNow(event){
  event.preventDefault();if(!['title','amount','date'].map(validate).every(Boolean)){focusEditorError(app('[aria-invalid=true]'));return;}
  const f=state.form;
  if(!M.categories.includes(f.category)||!Object.keys(repeats).includes(f.repeat)||!catalog.all().some(c=>c.id===f.categoryId)){toast('Choose a valid category and due type.');return;}
  app('#repeat-error').textContent='';app('#repeat-every').setAttribute('aria-invalid','false');app('#reminder-extra').setAttribute('aria-invalid','false');app('#due-provider').setAttribute('aria-invalid','false');
  if(f.repeat==='custom'&&(!Number.isInteger(Number(f.repeatEvery))||Number(f.repeatEvery)<1||Number(f.repeatEvery)>365||!['days','weeks','months','years'].includes(f.repeatUnit))){app('#repeat-error').textContent='Choose 1 to 365 whole units for a custom interval.';app('#repeat-every').setAttribute('aria-invalid','true');focusEditorError(app('#repeat-every'));return;}
  const offsets=phParseReminderDays(Array.from(document.querySelectorAll('#phone .reminder-offset:checked'),el=>Number(el.value)),f.reminderExtra);
  app('#offset-error').textContent=offsets.error||'';
  if(offsets.error){app('#reminder-extra').setAttribute('aria-invalid','true');focusEditorError(app('#reminder-extra'));return;}
  const provider=catalog.resolveProvider(f.providerLabel);app('#provider-error').textContent=provider.error||'';
  if(provider.error){app('#due-provider').setAttribute('aria-invalid','true');focusEditorError(app('#due-provider'));return;}
  const editing=state.page==='edit',d=editing?selected():null;if(editing&&!d)return;
  const patch={title:f.title.trim(),category:f.category,categoryId:f.categoryId,providerId:provider.value?.id||null,providerText:provider.value?.name||'',amount:M.parseAmount(DuewiseEssential.normalizeAmount(f.amount),f.category).value,date:f.date,notes:f.notes.trim()?f.notes:'',repeat:f.repeat,repeatEvery:Number(f.repeatEvery),repeatUnit:f.repeatUnit,reminderDays:offsets.value,paid:editing?f.paid:false,updated:'04 Nov 2026, 01:41 UTC'};
  if(editing)Object.assign(d,patch);else state.data.push({id:'sample-'+Date.now(),...patch,createdOrder:state.data.length,created:'04 Nov 2026, 01:41 UTC'});
  const target=state.formReturn;state.form=null;state.sampleMode='normal';navigate(target);toast('Sample due saved. Your custom categories and billers stay in this browser.');
}

async function act(name){
  if(name==='add'){navigate('add');return;}
  if(name==='back'||name==='back-to-parent'){navigate(['add','edit'].includes(state.page)?state.formReturn:state.page==='details'?state.parent:state.page==='reminders'?state.reminderReturn:state.page==='catalog'?'settings':'dashboard');return;}
  if(name==='all-dues'){state.filter='All';state.onlyRepeats=false;navigate('dues');return;}
  if(name==='recurring-dues'){state.filter='All';state.onlyRepeats=true;navigate('dues');return;}
  if(name==='clear-filters'){state.query='';state.filter='All';state.category='All categories';state.onlyRepeats=false;renderPhone();return;}
  if(name==='retry'){state.sampleMode='normal';renderPhone();$('#sample-state').value='normal';toast('Sample snapshot loaded.');return;}
  if(name==='today'){state.year=2026;state.month=10;state.date=M.TODAY;renderPhone();return;}
  if(name==='reminders'){navigate('reminders');return;}
  if(name==='save-settings'){state.prefs={...state.settingsDraft};state.modes[state.concept]=state.prefs.appearance==='system'?(matchMedia('(prefers-color-scheme:dark)').matches?'dark':'light'):state.prefs.appearance;renderInspector();renderPhone();toast('Sample preferences saved for this session.');return;}
  if(name==='reschedule'){navigate('edit');app('#due-date').focus();return;}
  if(name==='quick-paid'){const d=selected();if(d&&await confirm(d.paid?'Mark unpaid?':'Mark paid?',`Update the sample status of “${d.title}”? This records a status only and makes no payment.`,d.paid?'Mark unpaid':'Mark paid')){d.paid=!d.paid;d.updated='04 Nov 2026, 01:41 UTC';renderPhone();toast('Sample status updated.');}return;}
  if(name==='delete'){const d=selected();if(d&&await confirm(`Delete “${d.title}”?`,'Remove this saved sample from the active list. There is no Undo or restore. Unsaved edits will be discarded.','Delete due',true)){d.deleted=true;state.form=null;navigate('details');toast('Sample due deleted.');}return;}
  if(name==='disable-reminders'){state.reminder.enabled=false;renderPhone();toast('Reminders disabled in the simulation.');return;}
  if(name==='test-notification'){await confirm('Test notification preview','Duewise\nYou have a due-date reminder.\n\nNo title, amount or notes are included. No OS notification is sent.','Close preview');return;}
  if(name==='android-settings'){await confirm('Android settings preview','The native app would open Android notification or battery settings. This browser cannot read or change those settings.','Got it');}
}
document.addEventListener('click',event=>{
  const b=event.target.closest('button');if(!b||b.closest('[inert]'))return;
  if(b.dataset.view){state.view=b.dataset.view;render();}
  else if(b.dataset.explore){openConcept(b.dataset.explore);$('#explore-view').scrollIntoView({block:'start'});}
  else if(b.dataset.concept){openConcept(b.dataset.concept);}
  else if(b.dataset.refinement){preferences.refinement=b.dataset.refinement;persist();renderInspector();renderPhone();$(`.refinement-preview[data-refinement="${preferences.refinement}"]`).focus({preventScroll:true});}
  else if(b.dataset.mode){state.modes[state.concept]=b.dataset.mode;renderInspector();setPalette($('#phone'),concept(),b.dataset.mode);}
  else if(b.dataset.page){navigate(b.dataset.page);}
  else if(b.dataset.due){state.selected=b.dataset.due;navigate('details');}
  else if(b.dataset.statusLink){state.filter=b.dataset.statusLink;state.query='';state.onlyRepeats=false;navigate('dues');}
  else if(b.dataset.filter){state.filter=b.dataset.filter;renderPhone();}
  else if(b.dataset.date){state.date=b.dataset.date;renderPhone();}
  else if(b.dataset.calendarView){state.calendarView=b.dataset.calendarView;renderPhone();}
  else if(b.dataset.calendarStep){const delta=Number(b.dataset.calendarStep);if(state.calendarView==='week'){state.date=E.addDays(state.date,delta*7);state.year=Number(state.date.slice(0,4));state.month=Number(state.date.slice(5,7))-1;}else{const n=state.year*12+state.month+delta;state.year=Math.floor(n/12);state.month=n%12;state.date=`${String(state.year).padStart(4,'0')}-${String(state.month+1).padStart(2,'0')}-01`;}renderPhone();}
  else if(b.dataset.preference){state.settingsDraft[b.dataset.preference]=b.dataset.value;renderPhone();}
  else if(b.dataset.action){act(b.dataset.action);}
  refreshEditorSummaries();
});
document.addEventListener('input',event=>{
  const el=event.target;
  if(el.id==='concept-notes'){preferences.notes[state.concept]=el.value;persist();}
  if(el.id==='dues-search'){state.query=el.value;updateResults();}
  if(el.closest('#phone #due-form')&&el.name){state.form[el.name]=el.type==='checkbox'?el.checked:el.value;if(el.name==='amount')app('#amount-preview').textContent=amountPreview();if(el.name==='date'){app('#date-hint').textContent=(M.validDate(el.value)?dateText(el.value):'Choose a date')+' · Asia/Manila. Past dates are allowed.';app('#repeat-preview').textContent=repeatPreview();}}
  refreshEditorSummaries();
  if(el.closest('#phone #reminder-form')&&el.name)state.reminderDraft[el.name]=el.type==='checkbox'?el.checked:el.value;
});
document.addEventListener('change',event=>{
  const el=event.target;
  if(el.id==='compare-page'){state.comparePage=el.value;renderGallery();}
  if(el.id==='compare-mode'){state.compareMode=el.value;renderGallery();}
  if(el.id==='screen-selector'){navigate(el.value);}
  if(el.id==='sample-state'){state.sampleMode=el.value;renderPhone();}
  if(el.id==='category-filter'){state.category=el.value;updateResults();}
  if(el.id==='due-category'){state.form.category=el.value;const required=['Bill','Subscription'].includes(el.value);app('#due-amount').required=required;app('#due-amount').setAttribute('aria-required',String(required));app('#amount-requirement').textContent=required?'required':'optional';app('#amount-rule').textContent=required?'Required for '+el.value+'.':'Blank means no amount; zero is a recorded amount.';app('#amount-preview').textContent=state.form.amount.trim()?amountPreview():'';validate('amount');}
  if(el.id==='due-repeat'){state.form.repeat=el.value;app('#custom-repeat-fields').hidden=el.value!=='custom';app('#repeat-preview').textContent=repeatPreview();}
  if(['repeat-every','repeat-unit'].includes(el.id))app('#repeat-preview').textContent=repeatPreview();
  if(el.classList.contains('reminder-offset')&&state.form)state.form.reminderDays=Array.from(document.querySelectorAll('#phone .reminder-offset:checked'),e=>Number(e.value));
  refreshEditorSummaries();
});
document.addEventListener('focusout',event=>{if(event.target.closest('#phone #due-form')&&['title','amount','date'].includes(event.target.name))validate(event.target.name);});
document.addEventListener('submit',async event=>{
  if(event.target.id==='due-form')saveDue(event);
  if(event.target.id==='reminder-form'){
    event.preventDefault();const enable=event.submitter?.dataset.enable==='true';
    if(enable&&!await confirm('Enable sample reminders?','This is a simulated opt-in. No OS permission is requested and nothing is scheduled. Save the draft schedule and enable this simulation?','Save & enable'))return;
    state.reminder={...state.reminderDraft,enabled:enable||state.reminder.enabled};renderPhone();toast(enable?'Enabled in the simulation.':'Sample schedule saved.');
  }
});
$('#select-concept').addEventListener('click',async()=>{const c=concept();if(preferences.chosen===c.id)return;if(await confirm(`Choose ${c.letter} / ${c.name}?`,'Save this as your preferred design direction. You can keep exploring or change your mind. This does not change the production app.','Choose direction')){preferences.chosen=c.id;persist();renderInspector();}});
$('#clear-selection').addEventListener('click',()=>{preferences.chosen=null;persist();renderInspector();});
$('#export-selection').addEventListener('click',()=>{
  const chosen=D.concepts.find(c=>c.id===preferences.chosen);
  const text=['# Duewise — personal design brief','',`Exported: ${new Intl.DateTimeFormat('en-PH',{dateStyle:'long',timeZone:'Asia/Taipei'}).format(new Date())}`,'',`Preferred direction: ${chosen?chosen.letter+' / '+chosen.name:'Not selected'}`,`Minimal Fintech refinement: ${D.minimalRefinements.find(r=>r.id===preferences.refinement).name}`,'','This records a prototype preference, not production implementation authorization.','',...D.concepts.flatMap(c=>[`## ${c.letter} / ${c.name}`,'',preferences.notes[c.id]||'No refinement notes.','']),'Next step: review the chosen direction and refinement notes before planning production design adoption.'].join('\n');
  const url=URL.createObjectURL(new Blob([text],{type:'text/markdown;charset=utf-8'})),a=document.createElement('a');a.href=url;a.download='duewise-design-brief.md';document.body.appendChild(a);a.click();a.remove();setTimeout(()=>URL.revokeObjectURL(url),1000);toast('Your design brief is ready.');
});
$('#reset-sample').addEventListener('click',async()=>{if(await confirm('Reset the sample?','Reset sample dues, filters, dates, app preferences and reminder simulation. Keep your preferred direction and refinement notes.','Reset sample')){state.data=C.localizeSeed(E.seed());state.selected='rent';state.query='';state.filter='All';state.category='All categories';state.onlyRepeats=false;state.calendarView='month';state.year=2026;state.month=10;state.date=M.TODAY;state.prefs={appearance:'system',week:'monday'};state.settingsDraft=null;state.reminder={enabled:false,before:true,time:'09:00',quietStart:'22:00',quietEnd:'08:00'};state.reminderDraft=null;state.form=null;state.sampleMode='normal';state.scrolls={};navigate('dashboard');toast('Sample reset. Your design preference is kept.');}});
matchMedia('(prefers-color-scheme:dark)').addEventListener('change',event=>{if(state.prefs.appearance==='system'){state.modes[state.concept]=event.matches?'dark':'light';if(state.view==='explore'){renderInspector();setPalette($('#phone'),concept(),state.modes[state.concept]);}}});
async function loadHash(){
  const parts=location.hash.slice(1).split('/');
  if(Lab.dirty()&&!await Lab.allowLeave(parts[1]||'dashboard',true)){render();return;}
  state.form=null;
  Lab.enabled=parts[0]==='interaction-lab';
  if(parts[0]==='compare'){state.view='compare';}
  else if(parts[0]==='ux-decisions'){state.view='decide';window.DecisionLab.render(parts[1]);}
  else if(parts[0]==='notifications'){state.view='notify';window.NotificationLab.render(parts[1]);}
  else{state.concept=D.concepts.some(c=>c.id===parts[0])?parts[0]:'a';state.view='explore';state.page=roots.includes(parts[1])?parts[1]:'dashboard';if(state.page==='settings')state.settingsDraft={...state.prefs};}
  render();
}

window.addEventListener('hashchange',loadHash);
Lab.init();
loadHash();
