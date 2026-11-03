/* Buildless prototype. All dues and app preferences are temporary sample state. */
'use strict';
const M = DuewiseModel;
const $ = selector => document.querySelector(selector);
const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&':'&amp;', '<':'&lt;', '>':'&gt;', '"':'&quot;', "'":'&#39;' }[c]));
const icons = {
  dues:'<rect x="5" y="3" width="14" height="18" rx="3"/><path d="M9 8h6M9 12h6M9 16h4"/>',
  dashboard:'<rect x="3" y="3" width="7" height="7" rx="2"/><rect x="14" y="3" width="7" height="11" rx="2"/><rect x="3" y="14" width="7" height="7" rx="2"/><rect x="14" y="18" width="7" height="3" rx="1"/>',
  calendar:'<rect x="3" y="5" width="18" height="16" rx="3"/><path d="M7 3v4M17 3v4M3 10h18M8 14h2M14 14h2M8 17h2"/>',
  detail:'<path d="M8 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-3M8 8h8M8 12h5M8 16h3M16 3h5v5M21 3l-6 6"/>',
  add:'<path d="M12 5v14M5 12h14"/>',
  edit:'<path d="m15 5 4 4M4 20l5-1L21 7a2.8 2.8 0 0 0-4-4L5 15z"/>',
  settings:'<path d="m10 3-1 3-3 1-3 3 2 2-1 3 3 3 3-1 2 2 3-1 1-3 3-1 2-3-2-2 1-3-3-2-3 1-1-2z"/><circle cx="12" cy="12" r="3"/>',
  reminders:'<path d="M6 9a6 6 0 0 1 12 0c0 7 3 7 3 9H3c0-2 3-2 3-9M10 21h4"/>',
  onboarding:'<path d="m12 3 2.5 6.5L21 12l-6.5 2.5L12 21l-2.5-6.5L3 12l6.5-2.5z"/>',
  Bill:'<path d="M6 3h12v18l-3-2-3 2-3-2-3 2zM9 7h6M9 11h6M9 15h3"/>',
  Task:'<rect x="4" y="4" width="16" height="16" rx="4"/><path d="m8 12 3 3 5-6"/>',
  Subscription:'<path d="M20 7v5h-5M4 17v-5h5M6 7a7 7 0 0 1 12-2l2 3M4 16l2 3a7 7 0 0 0 12-2"/>',
  Other:'<rect x="4" y="4" width="16" height="16" rx="5"/><path d="M8 12h8M12 8v8"/>',
  back:'<path d="m14 6-6 6 6 6"/>', next:'<path d="m9 6 6 6-6 6"/>',
  search:'<circle cx="10.5" cy="10.5" r="6.5"/><path d="m16 16 5 5"/>',
  shield:'<path d="m12 3 8 3v6c0 4-5 8-8 9-3-1-8-5-8-9V6zM8 12l3 3 5-6"/>',
  check:'<path d="m5 12 4 4L19 6"/>', refresh:'<path d="M20 11a8 8 0 1 0-2 7M20 4v7h-7"/>',
  trash:'<path d="M3 6h18M9 6V3h6v3M5 6l1 15h12l1-15M10 10v7M14 10v7"/>',
  bellOff:'<path d="m3 3 18 18M6 9c0 6-3 7-3 9h14M10 21h4M18 13V9a6 6 0 0 0-9-5"/>',
  moon:'<path d="M20 14A8 8 0 0 1 10 4a8 8 0 1 0 10 10z"/>'
};
const icon = name => `<svg viewBox="0 0 24 24" aria-hidden="true">${icons[name] || icons.dues}</svg>`;
const pages = [
  { id:'dues', label:'Your dues', title:'Make the list the main event.', summary:'The everyday starting point. Put saved dues within reach and give each amount a clear, predictable place.', finding:'The current list header stacks Settings, Reminders, Add and Refresh before the first due. On a phone, this spends valuable space on occasional actions.', choices:[['cards','Calm cards','More separation and a dedicated amount line. Easier to scan at larger text sizes.'],['ledger','Ledger rows','A quieter, denser list for people who review many dues.']], recommendation:'Start with calm cards. Move Settings and Reminders to labeled header actions, use the header + icon for Add due, and make Refresh a quiet icon action.', guards:['Keep ascending date order, including paid dues.','Use Paid / Unpaid here; date classifications belong to Dashboard and Calendar.','No swipe-to-pay or hidden delete gestures.'] },
  { id:'dashboard', label:'Dashboard', title:'Clarity before charts.', summary:'Give people a short answer to “What needs my attention?” without implying a bank balance or spending report.', finding:'The current vertical progress ring, individual counts, Add button and explanatory text push search and results down the screen.', choices:[['compact','Compact progress','A horizontal summary with three labeled counts. Search sits closer to results.'],['ring','Progress ring','A larger visual checkpoint that gives paid progress more emphasis.']], recommendation:'Choose compact progress. Keep the global paid count, show the three unpaid date groups, and preserve space for finding a due.', guards:['Summary counts stay global when filters change.','Search + one status + one category combine together.','No peso totals or payment-history claims.'] },
  { id:'calendar', label:'Calendar', title:'Dates first. Details next.', summary:'Make it easy to choose a day and see exactly what belongs to it, with a readable agenda underneath.', finding:'Seven columns constrain mobile touch targets. Adding amount text or category colors inside dates would make the grid harder to use.', choices:[['month','Month + agenda','The complete month stays visible above the selected day’s list.'],['agenda','Agenda focus','Collapse the month behind a labeled date chooser to free room for dues.']], recommendation:'Use month + agenda by default. Show a small textual item count per date, a distinct Today outline, and a solid selected date.', guards:['Six rows; saved Monday / Sunday week start.','Paid dues still count on their due dates.','Add due defaults to sample Today, not the selected calendar day.'] },
  { id:'detail', label:'Due details', title:'A receipt for your memory.', summary:'A clear saved record: what it is, how much, when it is due and whether it is paid.', finding:'The existing content has the right fields. Stronger grouping can separate the obligation itself from notes and technical timestamps.', choices:[['amount','Amount first','A prominent peso amount, followed by the title and a compact facts panel.'],['title','Title first','Lead with the obligation title. Works especially well for tasks with no amount.']], recommendation:'Use amount first for finance-heavy use. Keep “No amount” literal for tasks, and never mistake an absent amount for ₱0.00.', guards:['Edit is the only mutation entry point.','Created / updated timestamps explicitly say UTC.','Back returns to the original list or agenda.'] },
  { id:'add', label:'Add due', title:'A form that earns your trust.', summary:'Clear labels and a visible amount preview reduce uncertainty before a financial record is saved.', finding:'The existing form already validates on blur and Save. The revision focuses on shorter guidance, visible category requirements and a clearer reading sequence.', choices:[['simple','One flowing form','Title, category, amount, date and notes in a single sequence.'],['grouped','Grouped form','Separate “What is it?” from “Amount & date” for a more guided entry.']], recommendation:'Choose one flowing form for a short mobile task. Keep a compact Save in the persistent bottom action bar, with the exact rounded amount preview beside the field.', guards:['Bill / Subscription require an amount; Task / Other may omit it.','Zero is valid; no amount is distinct.','Cancel discards; recurrence and per-due reminders stay deferred.'] },
  { id:'edit', label:'Edit due', title:'Make every change deliberate.', summary:'Reuse the add form, then separate the paid draft and destructive action so both have an unmistakable meaning.', finding:'Payment must change only after Save. Delete should remain visually separate from routine edits, with the saved title in confirmation.', choices:[['simple','One flowing form','The same field order as Add, with a saved-on-Save paid checkbox.'],['grouped','Grouped form','Content, amount/date and payment are shown as distinct sections.']], recommendation:'Use the same form direction as Add for consistency. Keep “Changes apply when you save” by the payment control, and use a separate trash icon beside Save with explicit confirmation.', guards:['A paid checkbox never writes immediately.','Delete confirms against the saved record.','No Undo or restore is implied.'] },
  { id:'settings', label:'Settings', title:'Fewer settings. Clearer choices.', summary:'Appearance and week start are enough for this slice. Keep their effect and save timing visible.', finding:'The current screen uses native pickers and explicit Save. A comparison of visible choices versus dropdowns can improve discoverability without changing persistence.', choices:[['segments','Visible choices','Show System / Light / Dark and Monday / Sunday directly.'],['pickers','Labeled dropdowns','Stay closer to native MAUI pickers, with a smaller visual footprint.']], recommendation:'Prefer visible choices if they remain readable at large text sizes. Apply both only after Save; include a short explanation of System appearance.', guards:['Opening Reminders preserves the Settings draft.','Back discards an unsaved draft.','PHP and Asia/Manila are context, not editable selectors.'] },
  { id:'reminders', label:'Reminders', title:'Permission with a purpose.', summary:'Explain the schedule first. Invite opt-in with a clear action, then show delivery readiness separately.', finding:'The current page presents several save, enable, disable, test, Android-settings and retry buttons together. Grouping secondary tools reduces competing calls to action.', choices:[['progressive','Progressive disclosure','Schedule and one main action up front. Android tools live in an expandable section.'],['expanded','All controls visible','Expose delivery diagnostics and secondary actions immediately for troubleshooting.']], recommendation:'Use progressive disclosure. Default to off, name the main action “Save and enable”, and put permission and battery checks together below the schedule.', guards:['This HTML demo simulates opt-in; it never requests OS permission.','Real reminders use generic notification text.','Android delivery is inexact and never guaranteed.'] },
  { id:'onboarding', label:'Onboarding', title:'A short, honest introduction.', future:true, summary:'Four DESIGN.md onboarding concepts, adapted to the features already in scope. This is a future concept, not a production flow.', finding:'The original onboarding promises broader reminder and completion features. Keep the language tied to local dues, date-only reminders and explicit paid state.', choices:[['minimal','Quiet introduction','One icon and a short message. Less decoration, faster reading.'],['illustrated','Illustrated introduction','A layered calendar motif gives the same message a little more warmth.']], recommendation:'Choose the quiet introduction if onboarding is later approved. Let people skip it, and ask about reminders only when they explicitly enable them.', guards:['The production app still starts at Dues.','No account, cloud sync or bank connection is implied.','All four steps can be reviewed here; no seen-state is saved.'] }
];
const storageKey = 'duewise.mobile-review.2026-11-04.v1';
let persistent = true, stored = {};
try { stored = JSON.parse(localStorage.getItem(storageKey) || '{}') || {}; } catch { persistent = false; }
const choices = {}, notes = {};
for (const p of pages) {
  choices[p.id] = p.choices.some(c => c[0] === stored.choices?.[p.id]) ? stored.choices[p.id] : p.choices[0][0];
  notes[p.id] = typeof stored.notes?.[p.id] === 'string' ? stored.notes[p.id] : '';
}
const state = { page:'dues', dues:M.seed(), selected:'rent', parent:'dues', mode:'normal', theme:'light', query:'', status:'All', category:'All categories', calendarYear:2026, calendarMonth:10, selectedDate:M.TODAY, preferences:{ appearance:'system', week:'monday' }, settingsDraft:null, settingsReturn:'dues', reminderReturn:'dues', reminders:{ enabled:false, before:true, time:'09:00', quietStart:'22:00', quietEnd:'08:00' }, reminderDraft:null, editorDraft:null, editReturn:'detail', onboardingStep:0 };
const roots = ['dues','dashboard','calendar'];
const dateText = value => new Intl.DateTimeFormat('en-GB',{ day:'2-digit', month:'short', year:'numeric', timeZone:'UTC' }).format(new Date(value+'T12:00:00Z'));
function persist() {
  try { localStorage.setItem(storageKey,JSON.stringify({ choices,notes })); persistent = true; } catch { persistent = false; }
  $('#save-caption').textContent = persistent ? 'Your design choices are saved in this browser.' : 'Browser storage is unavailable. Export your choices before leaving.';
}
function showToast(message) { const el=$('#toast'); el.textContent=message; el.classList.add('visible'); clearTimeout(showToast.timer); showToast.timer=setTimeout(()=>el.classList.remove('visible'),4200); }
function selectedDue() { return state.dues.find(d=>d.id===state.selected && !d.deleted); }
function newDraft(editing) {
  const d=editing ? selectedDue() : null;
  return { title:d?.title||'', category:d?.category||'Bill', amount:d?M.inputAmount(d.amount):'', date:d?.date||M.TODAY, notes:d?.notes||'', paid:d?.paid||false };
}
function navigate(id, options={}) {
  clearTimeout(showToast.timer); $('#toast').classList.remove('visible');
  if (!pages.some(p=>p.id===id)) id='dues';
  const previous=state.page;
  if (roots.includes(previous) && ['detail','add'].includes(id)) state.parent=previous;
  if (id==='settings' && previous!=='reminders') { state.settingsReturn=roots.includes(previous)?previous:'dues'; state.settingsDraft={...state.preferences}; }
  if (id==='reminders' && previous!=='reminders') { state.reminderReturn=previous==='settings'?'settings':'dues'; state.reminderDraft={...state.reminders}; }
  if (previous==='settings' && !['settings','reminders'].includes(id)) state.settingsDraft=null;
  if (id==='add' || id==='edit') {
    state.editorDraft=newDraft(id==='edit');
    state.editReturn=id==='edit'?'detail':state.parent;
  } else if (previous==='add'||previous==='edit') state.editorDraft=null;
  state.page=id;
  if (!['dues','dashboard','calendar','detail'].includes(id)) state.mode='normal';
  if (options.fromRail && roots.includes(id)) state.parent=id;
  const nextHash='#'+id;
  if (location.hash!==nextHash) history.replaceState(null,'',nextHash);
  render();
  if (options.focus) $('#preview').focus({preventScroll:true});
}
function render() {
  const page=pages.find(p=>p.id===state.page), index=pages.indexOf(page);
  $('#screen-nav').innerHTML=pages.map((p,i)=>`<button class="screen-link" data-page="${p.id}" ${p.id===page.id?'aria-current="page"':''}>${icon(p.id)}<span>${p.label}</span>${p.future?'<span class="concept-label">Concept</span>':''}<span class="screen-number">${String(i+1).padStart(2,'0')}</span></button>`).join('');
  $('#screen-count').textContent=`${String(index+1).padStart(2,'0')} / 09`;
  $('#preview-caption').textContent=`${String(index+1).padStart(2,'0')} · ${page.label}`;
  $('#review-title').textContent=page.title;
  $('#review-summary').textContent=page.summary;
  $('#review-finding').textContent=page.finding;
  $('#recommendation-copy').textContent=page.recommendation;
  $('#scope-badge').textContent=page.future?'Future concept':'Current feature · revised UI';
  $('#scope-badge').classList.toggle('future',Boolean(page.future));
  $('#guardrails').innerHTML=page.guards.map(g=>`<li>${escapeHtml(g)}</li>`).join('');
  $('#choice-options').innerHTML=page.choices.map((c,i)=>`<label class="choice-option"><input type="radio" name="layout-choice" value="${c[0]}" ${choices[page.id]===c[0]?'checked':''}><span><strong>${escapeHtml(c[1])}${i===0?'<span class="recommended-label">RECOMMENDED STARTING POINT</span>':''}</strong><small>${escapeHtml(c[2])}</small></span></label>`).join('');
  $('#decision-note').value=notes[page.id];
  $('#save-caption').textContent=persistent?'Your design choices are saved in this browser.':'Browser storage is unavailable. Export your choices before leaving.';
  $('#next-page').innerHTML=index===pages.length-1?'Back to first screen <span aria-hidden="true">↗</span>':'Next screen <span aria-hidden="true">→</span>';
  $('#preview-state').value=state.mode;
  $('#preview-state').disabled=!['dues','dashboard','calendar','detail'].includes(page.id);
  $('#app-tabs').hidden=!roots.includes(page.id);
  $('#app-tabs').innerHTML=roots.map(id=>`<button data-page="${id}" ${state.page===id?'aria-current="page"':''}>${icon(id)}<span>${id==='dues'?'Dues':id==='dashboard'?'Dashboard':'Calendar'}</span></button>`).join('');
  renderHeader(); renderApp();
}
function renderHeader() {
  const page=pages.find(p=>p.id===state.page);
  if (roots.includes(state.page)) $('#app-header').innerHTML=`<div class="app-logo"><span class="brand-mark" aria-hidden="true">✓</span>Duewise</div><div class="header-actions">${state.page==='dues'?`<button class="icon-button quiet-icon" data-action="open-reminders" aria-label="Reminders" title="Reminders">${icon('reminders')}</button><button class="icon-button quiet-icon" data-page="settings" aria-label="Settings" title="Settings">${icon('settings')}</button>`:''}<button class="icon-button add-icon" data-page="add" aria-label="Add due" title="Add due">${icon('add')}</button></div>`;
  else $('#app-header').innerHTML=`<button class="back-button" data-action="back">${icon('back')}${state.page==='detail'?'Back to list':state.page==='add'||state.page==='edit'?'Cancel':state.page==='reminders'&&state.reminderReturn==='settings'?'Settings':'Back to dues'}</button><span class="header-title">${page.label}</span>${state.page==='detail'&&selectedDue()&&state.mode!=='empty'?`<button class="icon-button quiet-icon" data-page="edit" aria-label="Edit due" title="Edit due">${icon('edit')}</button>`:''}`;
}
function renderActionBar() {
  const bar=$('#app-actions');
  const editing=state.page==='edit';
  bar.innerHTML='';
  if (!['normal','stale'].includes(state.mode)) { bar.hidden=true; return; }
  if (state.page==='add'||editing&&selectedDue()) bar.innerHTML=`<button class="primary" type="submit" form="due-form" aria-label="${editing?'Save changes':'Save due'}">${icon('check')}Save</button>${editing?`<button class="icon-button delete-icon" data-action="delete-due" aria-label="Delete due" title="Delete due">${icon('trash')}</button>`:''}`;
  if (state.page==='settings') bar.innerHTML=`<button class="primary" data-action="save-preferences" aria-label="Save preferences">${icon('check')}Save</button>`;
  if (state.page==='reminders') bar.innerHTML=`<button class="primary" type="submit" form="reminder-form" data-enable="${!state.reminders.enabled}" aria-label="${state.reminders.enabled?'Save schedule':'Save and enable reminders'}">${icon(state.reminders.enabled?'check':'reminders')}${state.reminders.enabled?'Save schedule':'Save & enable'}</button>${state.reminders.enabled?`<button class="icon-button delete-icon" data-action="disable-reminders" aria-label="Disable reminders" title="Disable reminders">${icon('bellOff')}</button>`:'<button class="secondary" type="submit" form="reminder-form" data-enable="false" aria-label="Save schedule without enabling">Save only</button>'}`;
  bar.hidden=!bar.innerHTML;
}
function heading(title,subtitle,name) { return `<div class="page-heading"><div><h2>${title}</h2><p>${subtitle}</p></div>${name?`<span class="heading-icon">${icon(name)}</span>`:''}</div>`; }
function badge(text) { const css={'Overdue':'overdue','Due today':'today','Paid':'paid','Upcoming':'upcoming','Unpaid':'unpaid'}[text]; return `<span class="badge ${css}">${text==='Paid'?'✓':text==='Overdue'?'!':'○'} ${text}</span>`; }
function card(d,classified=true) {
  return `<button class="due-card" data-due="${escapeHtml(d.id)}" aria-label="${escapeHtml(`${d.title}, ${d.category}, ${M.money(d.amount)}, due ${dateText(d.date)}, ${classified?M.status(d):d.paid?'Paid':'Unpaid'}`)}"><span class="card-top"><span class="category-icon">${icon(d.category)}</span><span><span class="card-title">${escapeHtml(d.title)}</span><span class="card-category">${d.category}</span></span>${icon('next').replace('<svg ','<svg class="card-arrow" ')}</span><span class="card-bottom"><span><span class="card-amount ${d.amount===null?'absent':''}">${M.money(d.amount)}</span><span class="card-date">Due ${dateText(d.date)}</span></span>${badge(classified?M.status(d):d.paid?'Paid':'Unpaid')}</span></button>`;
}
function empty(title,copy,action='add',label='Add due') { return `<div class="empty-state"><div class="empty-art">${icon(action==='retry'?'refresh':'dues')}</div><h3>${title}</h3><p>${copy}</p><button class="primary" data-action="${action}">${label}</button></div>`; }
function sampleData() { return state.mode==='empty'?[]:state.dues.filter(d=>!d.deleted); }
function renderApp() {
  const el=$('#preview'); const priorScroll=el.scrollTop;
  renderActionBar();
  if (state.mode==='loading') { el.innerHTML=`<div role="status">${heading('A moment of clarity…','Loading your saved dues.','')}<div class="skeleton"><span></span><span></span><span></span></div><div class="skeleton"><span></span><span></span><span></span></div></div><button class="secondary" data-action="retry">Show sample data</button>`; return; }
  if (state.mode==='error') { el.innerHTML=empty('Couldn’t load your dues.','Your saved information has not been changed. Try loading it again.','retry','Retry loading')+(state.page==='dues'?'<button class="secondary" data-page="settings">Open Settings</button>':''); return; }
  const views={dues:duesPage,dashboard:dashboardPage,calendar:calendarPage,detail:detailPage,add:editorPage,edit:editorPage,settings:settingsPage,reminders:reminderPage,onboarding:onboardingPage};
  el.innerHTML=(state.mode==='stale'?'<div class="inline-notice warning" role="status">Couldn’t refresh. Showing the last loaded sample data.<button class="text-button" data-action="retry">Retry refresh</button></div>':'')+views[state.page]();
  el.scrollTop=priorScroll;
}
function duesPage() {
  const data=M.sorted(sampleData());
  return heading('Your dues','One less thing to keep in your head.','dues')+`<div class="subtle-action"><p>Soonest due first · PHP</p><button class="icon-button quiet-icon" data-action="refresh" aria-label="Refresh dues" title="Refresh dues">${icon('refresh')}</button></div>`+(data.length?`<div class="section-label"><h3>All saved dues</h3><span>${data.length} items</span></div><div class="list ${choices.dues==='ledger'?'ledger':''}">${data.map(d=>card(d,false)).join('')}</div><div class="helper-line">${icon('shield')}Stored on this device · Works offline</div>`:empty('A little clarity starts here.','Add a bill, task or subscription. Your dues will appear here in date order.'));
}
function dashboardPage() {
  const data=sampleData(), summary=M.summary(data), percent=summary.total?Math.round(summary.Paid/summary.total*100):0;
  const results=M.filter(data,state.query,state.status,state.category);
  return heading('A little more in control.','Wednesday, 04 Nov 2026 · Asia/Manila','')+`<section class="summary-card ${choices.dashboard==='ring'?'ring-layout':''}" aria-label="Global summary of all sample dues"><div class="summary-top"><div><div class="summary-title">Payment progress · all saved dues</div><div class="progress-number">${summary.Paid}<small> / ${summary.total} paid</small></div></div><div class="ring" style="--progress:${percent}%" role="img" aria-label="${summary.Paid} of ${summary.total} dues paid, ${percent} percent"><span>${percent}%</span></div></div><div class="progress-track" aria-hidden="true"><div class="progress-fill" style="width:${percent}%"></div></div><div class="metrics"><div class="metric overdue"><strong>${summary.Overdue}</strong><span>Overdue</span></div><div class="metric"><strong>${summary['Due today']}</strong><span>Due today</span></div><div class="metric"><strong>${summary.Upcoming}</strong><span>Upcoming</span></div></div><p class="summary-foot">Counts include all saved dues, across dates and filters.</p></section><label class="search-label" for="search">Find a due · search titles</label><div class="search-field">${icon('search')}<input id="search" class="field-input" placeholder="Search your dues" type="search" value="${escapeHtml(state.query)}"></div><div class="filters" aria-label="Status filter">${['All','Overdue','Due today','Upcoming','Paid'].map(s=>`<button class="filter-chip" data-filter="${s}" aria-pressed="${state.status===s}">${s}</button>`).join('')}</div><label class="category-filter" for="category-filter">Category<select id="category-filter">${['All categories',...M.categories].map(c=>`<option ${state.category===c?'selected':''}>${c}</option>`).join('')}</select></label><div class="section-label"><h3 id="result-count" aria-live="polite">${results.length} matching dues</h3><button class="text-button" data-action="clear-filters">Clear filters</button></div><div id="dashboard-results" class="list">${dashboardResults(results,data)}</div>`;
}
function dashboardResults(results,data) { return results.length?results.map(d=>card(d)).join(''):data.length?empty('No matching dues.','Try a different title, status or category.','clear-filters','Clear filters'):empty('Start with one due.','Your payment progress will appear when you add a due.'); }
function updateSearchResults() { const data=sampleData(),results=M.filter(data,state.query,state.status,state.category); $('#result-count').textContent=`${results.length} matching dues`; $('#dashboard-results').innerHTML=dashboardResults(results,data); }
function calendarGrid() {
  const days=M.calendar(state.calendarYear,state.calendarMonth,state.preferences.week);
  const headers=state.preferences.week==='monday'?['M','T','W','T','F','S','S']:['S','M','T','W','T','F','S'];
  const full=state.preferences.week==='monday'?['Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday']:['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];
  const data=sampleData(), month=new Intl.DateTimeFormat('en-GB',{month:'long',year:'numeric',timeZone:'UTC'}).format(new Date(`${String(state.calendarYear).padStart(4,'0')}-${String(state.calendarMonth+1).padStart(2,'0')}-01T12:00:00Z`));
  return `<div class="month-bar"><h3>${month}</h3><div class="month-actions"><button class="icon-button" data-month="-1" aria-label="Previous month" ${state.calendarYear===1&&state.calendarMonth===0?'disabled':''}>${icon('back')}</button><button class="icon-button" data-month="1" aria-label="Next month" ${state.calendarYear===9999&&state.calendarMonth===11?'disabled':''}>${icon('next')}</button></div></div><div class="calendar-dates"><div class="weekdays">${headers.map((d,i)=>`<span aria-label="${full[i]}">${d}</span>`).join('')}</div><div class="calendar-grid" aria-label="${month}">${days.map(date=>{if(!date)return '<span class="calendar-blank" aria-hidden="true"></span>';const count=data.filter(d=>d.date===date).length;return `<button class="calendar-day ${date===M.TODAY?'today':''}" data-date="${date}" aria-label="${dateText(date)}, ${count} ${count===1?'due':'dues'}${date===M.TODAY?', today':''}" aria-pressed="${date===state.selectedDate}">${Number(date.slice(-2))}<span class="day-count">${count?count+' due'+(count===1?'':'s'):''}</span></button>`;}).join('')}</div></div>`;
}
function calendarPage() {
  const agenda=sampleData().filter(d=>d.date===state.selectedDate);
  const grid=choices.calendar==='agenda'?`<details class="calendar-picker"><summary>Choose a date ${icon('calendar')}</summary>${calendarGrid()}</details>`:calendarGrid();
  return heading('Your month, made clear.','Calendar dates use Asia/Manila.','')+`<div class="subtle-action"><p>${dateText(state.selectedDate)}</p><button class="text-button" data-action="today">Go to Today</button></div>${grid}<p class="calendar-caption">${state.preferences.week==='monday'?'Monday':'Sunday'} first · Counts include paid dues.</p><div class="section-label"><h3 class="agenda-title">${dateText(state.selectedDate)}</h3><span>${agenda.length} ${agenda.length===1?'due':'dues'}</span></div><div class="list">${agenda.length?agenda.map(d=>card(d)).join(''):'<div class="note-card"><h3>A little breathing room.</h3><p>No dues on this date.</p></div>'}</div><p class="field-hint">New dues default to Today, even when viewing another date.</p>`;
}
function detailPage() {
  const d=state.mode==='empty'?null:selectedDue();
  if(!d)return empty('This due is unavailable.','It may have been deleted. Return to your list to choose another due.','return-list','Back to list');
  const amount=`<div class="detail-amount">${M.money(d.amount)}</div>`,title=`<h2>${escapeHtml(d.title)}</h2>`;
  return `<div class="detail-hero ${choices.detail==='title'?'title-first':''}"><div class="detail-category">${icon(d.category)}${d.category}</div>${choices.detail==='title'?title+amount:amount+title}<div style="margin-top:16px">${badge(d.paid?'Paid':'Unpaid')}</div></div><dl class="information"><div class="info-row"><dt>Due date</dt><dd>${dateText(d.date)}</dd></div><div class="info-row"><dt>Timezone</dt><dd>Asia/Manila</dd></div><div class="info-row"><dt>Currency</dt><dd>PHP</dd></div></dl><section class="note-card"><h3>Notes</h3><p>${d.notes?escapeHtml(d.notes).replace(/\n/g,'<br>'):'No notes added.'}</p></section><div class="timestamps"><p>Created · ${d.created||'01 Nov 2026, 01:41 UTC'}<br>Last updated · ${d.updated||'01 Nov 2026, 01:41 UTC'}</p></div><div class="helper-line">${icon('shield')}Saved information · No payment is made in this app</div>`;
}
function editorPage() {
  const editing=state.page==='edit';
  if(editing&&!selectedDue())return empty('This due is unavailable.','Return to your list to choose another due.','return-list','Back to list');
  const d=state.editorDraft||(state.editorDraft=newDraft(editing));
  const titleField=`<div class="field"><label for="due-title">Title <span aria-hidden="true">*</span></label><input class="field-input" id="due-title" name="title" value="${escapeHtml(d.title)}" placeholder="e.g. Electricity bill" required aria-describedby="title-error"><span class="field-error" id="title-error"></span></div>`;
  const categoryField=`<fieldset class="field"><legend>Category</legend><div class="category-options">${M.categories.map(c=>`<label class="category-option"><input type="radio" name="category" value="${c}" ${d.category===c?'checked':''}>${icon(c)}${c}</label>`).join('')}</div></fieldset>`;
  const amountField=`<div class="field"><label for="due-amount">Amount (PHP)</label><input class="field-input" id="due-amount" name="amount" inputmode="decimal" value="${escapeHtml(d.amount)}" placeholder="2450.50" aria-describedby="amount-hint amount-error amount-preview"><p class="field-hint" id="amount-hint">${amountHint(d.category)}</p><span class="field-error" id="amount-error"></span><p class="field-hint" id="amount-preview" aria-live="polite">${amountPreview(d)}</p></div>`;
  const dateField=`<div class="field"><label for="due-date">Due date</label><input class="field-input" type="date" id="due-date" name="date" min="0001-01-01" max="9999-12-31" value="${escapeHtml(d.date)}" required aria-describedby="date-error"><p class="field-hint">Asia/Manila · Past dates are allowed.</p><span class="field-error" id="date-error"></span></div>`;
  const grouped=choices[state.page]==='grouped';
  return heading(editing?'Edit with confidence.':'Make room for peace of mind.',editing?'Changes apply only when you save.':'A few details. One less thing to remember.','')+`<form id="due-form" novalidate>${grouped?`<section class="form-group"><h3 class="form-group-heading">01 / What is it?</h3>${titleField+categoryField}</section><section class="form-group"><h3 class="form-group-heading">02 / Amount & date</h3>${amountField+dateField}</section>`:titleField+categoryField+amountField+dateField}<div class="field"><label for="due-notes">Notes (optional)</label><textarea class="field-input" id="due-notes" name="notes" rows="3" placeholder="Anything you’d like to remember">${escapeHtml(d.notes)}</textarea></div>${editing?`<div class="field"><label class="payment-toggle" for="due-paid"><span>Mark as paid</span><input id="due-paid" type="checkbox" name="paid" ${d.paid?'checked':''}></label><p class="field-hint">Changes apply when you save.</p></div>`:'<p class="field-hint" style="margin-bottom:18px">New dues start unpaid. Required fields are checked before saving.</p>'}</form>`;
}
function amountHint(category) { return `${['Bill','Subscription'].includes(category)?'Required for '+category+'.':'Optional for '+category+'. Blank means no amount.'} Digits and decimal point only; e.g. 2450.50.`; }
function amountPreview(d) { const parsed=M.parseAmount(d.amount,d.category);return parsed.error?'Amounts are rounded to centavos when saved.':`Will save as ${M.money(parsed.value)}.`; }
function segments(name,values,selected) { return `<div class="segmented" aria-label="${name}">${values.map(([value,label])=>`<button type="button" data-pref="${name}" data-value="${value}" aria-pressed="${value===selected}">${label}</button>`).join('')}</div>`; }
function preferenceControl(name,values,selected) { return choices.settings==='segments'?segments(name,values,selected):`<select class="field-input" data-pref-select="${name}" aria-label="${name==='appearance'?'Appearance':'Start of week'}">${values.map(([value,label])=>`<option value="${value}" ${selected===value?'selected':''}>${label}</option>`).join('')}</select>`; }
function settingsPage() {
  const d=state.settingsDraft||(state.settingsDraft={...state.preferences});
  return heading('Make yourself at home.','A few preferences for your daily routine.','')+`<section class="settings-section"><h3>Appearance</h3><p>System follows your device. Light and Dark stay as you choose.</p>${preferenceControl('appearance',[['system','System'],['light','Light'],['dark','Dark']],d.appearance)}</section><section class="settings-section"><h3>Start of week</h3><p>Changes the calendar layout, without changing your due dates.</p>${preferenceControl('week',[['monday','Monday'],['sunday','Sunday']],d.week)}</section><div class="inline-notice">Changes apply only when you save. Back discards unsaved choices.</div><button class="settings-link" data-action="open-reminders">${icon('reminders')}<span><strong>Reminders</strong><small>Schedule and notification readiness</small></span>${icon('next')}</button><div class="helper-line">${icon('shield')}PHP · Asia/Manila · Local to this device</div>`;
}
function reminderPage() {
  const d=state.reminderDraft||(state.reminderDraft={...state.reminders});
  const diagnostics=`<p>Notification permission: ${state.reminders.enabled?'allowed in simulation':'not requested in simulation'}.</p><p>Battery Saver, background restriction and battery optimization: unavailable in this browser prototype. Check each separately on Android.</p><div class="diagnostic-actions"><button class="secondary" data-action="test-notification">${icon('reminders')}Test notification</button><button class="secondary" data-action="android-notifications">${icon('settings')}Notification settings</button><button class="secondary" data-action="android-battery">${icon('shield')}App / battery settings</button><button class="secondary" data-action="refresh-readiness">${icon('refresh')}Refresh status</button></div>`;
  return heading('A gentle nudge.','Your schedule. Your choice.','')+`<div class="reminder-status"><span class="category-icon">${icon('reminders')}</span><div><strong>Reminders are ${state.reminders.enabled?'on':'off'}</strong><p>Applies to all active, unpaid dues.</p></div><span class="badge">${state.reminders.enabled?'On':'Off'}</span></div><div class="inline-notice">Prototype simulation only. No OS permission is requested and no notification is scheduled.</div><form id="reminder-form"><section class="settings-section"><h3>When to remind you</h3><p>On the due date, with an optional reminder the day before.</p><label class="payment-toggle" for="reminder-before"><span>Also one day before</span><input id="reminder-before" name="before" type="checkbox" ${d.before?'checked':''}></label><div class="field"><label for="reminder-time">Reminder time · Asia/Manila</label><input class="field-input" type="time" id="reminder-time" name="time" value="${d.time}" required></div></section><section class="settings-section"><h3>Quiet hours</h3><p>Reminders move to the next allowed time. Anything delayed past the due date is skipped. Equal times turn quiet hours off.</p><div class="field"><label for="quiet-start">Start</label><input class="field-input" type="time" id="quiet-start" name="quietStart" value="${d.quietStart}" required></div><div class="field"><label for="quiet-end">End</label><input class="field-input" type="time" id="quiet-end" name="quietEnd" value="${d.quietEnd}" required></div></section></form>${choices.reminders==='progressive'?`<details class="system-details"><summary>Delivery readiness & Android tools</summary>${diagnostics}</details>`:`<section class="system-details"><h3>Delivery readiness</h3>${diagnostics}</section>`}<p class="field-hint">Real notifications hide titles, amounts and notes. Android can delay or prevent delivery; reminders are never guaranteed.</p>`;
}
const onboardingSteps=[['Keep what’s due in view.','Bills, tasks, subscriptions and deadlines. A little clarity, all in one place.','calendar','Your dues, together'],['Add it. Clear your head.','Give it a title, category and date. Add a PHP amount when it matters.','dues','One less thing to remember'],['A nudge, when you choose.','Enable optional reminders for unpaid dues. Set a global time and quiet hours. Android delivery can vary.','reminders','Reminders start off'],['Know what comes next.','See your dates on the calendar and your paid progress at a glance. Your dues stay on this device.','check','A little more in control']];
function onboardingPage() {
  const step=state.onboardingStep,[title,body,name,note]=onboardingSteps[step];
  return `<section class="onboarding ${choices.onboarding==='minimal'?'onboarding-minimal':''}"><div class="onboarding-top"><p>Future concept · ${step+1} of 4</p><button class="text-button" data-action="skip-onboarding">Skip</button></div><div class="onboarding-art">${icon(name)}<span class="floating-note">${note}</span><span class="floating-check">✓</span></div><h2>${title}</h2><p>${body}</p><div class="step-dots" aria-label="Step ${step+1} of 4">${onboardingSteps.map((_,i)=>`<span class="${i===step?'current':''}" aria-hidden="true"></span>`).join('')}</div><button class="primary" data-action="onboarding-next">${step===3?'Get started':'Next'}${icon('next')}</button>${step>0?'<button class="text-button" style="margin:8px auto" data-action="onboarding-back">Previous step</button>':''}</section>`;
}
function applyTheme(theme) { state.theme=theme;$('#phone').dataset.theme=theme;document.querySelectorAll('[data-theme]').forEach(el=>{if(el.tagName==='BUTTON')el.setAttribute('aria-pressed',String(el.dataset.theme===theme));}); }
function validateField(name) {
  const d=state.editorDraft;
  let error='';
  if(name==='title'&&!d.title.trim())error='Enter a title for this due.';
  if(name==='amount')error=M.parseAmount(d.amount,d.category).error||'';
  if(name==='date'&&!M.validDate(d.date))error='Choose a valid due date.';
  const input=$(`#due-${name}`),message=$(`#${name}-error`);
  if(input&&message){ input.setAttribute('aria-invalid',String(Boolean(error)));message.textContent=error; }
  return !error;
}
function submitDue(event) {
  event.preventDefault();
  const valid=['title','amount','date'].map(validateField).every(Boolean);
  if(!valid){$('#due-form [aria-invalid="true"]').focus();return;}
  const d=state.editorDraft,editing=state.page==='edit',saved=editing?selectedDue():null;
  if(editing&&!saved)return;
  const amount=M.parseAmount(d.amount,d.category).value;
  if(editing)Object.assign(saved,{title:d.title.trim(),category:d.category,amount,date:d.date,notes:d.notes.trim()?d.notes:'',paid:d.paid,updated:'04 Nov 2026, 01:41 UTC'});
  else state.dues.push({id:'sample-'+Date.now(),title:d.title.trim(),category:d.category,amount,date:d.date,notes:d.notes.trim()?d.notes:'',paid:false,createdOrder:state.dues.length,created:'04 Nov 2026, 01:41 UTC'});
  state.mode='normal';navigate(editing?'detail':state.parent);showToast('Sample due saved. Demo changes reset when this page reloads.');
}
async function confirmAction(title,copy,label='Confirm',destructive=false) {
  const dialog=$('#action-dialog');$('#dialog-title').textContent=title;$('#dialog-copy').textContent=copy;$('#dialog-confirm').textContent=label;$('#dialog-confirm').className=destructive?'destructive':'primary';
  dialog.returnValue='';dialog.showModal();
  return new Promise(resolve=>dialog.addEventListener('close',()=>resolve(dialog.returnValue==='confirm'),{once:true}));
}
async function action(name) {
  if(name==='add'){navigate('add');return;}
  if(name==='back') {
    const target=state.page==='detail'?state.parent:state.page==='add'||state.page==='edit'?state.editReturn:state.page==='reminders'?state.reminderReturn:state.page==='settings'?state.settingsReturn:'dues';
    navigate(target);return;
  }
  if(name==='return-list'){state.mode='normal';navigate(state.parent);return;}
  if(name==='refresh'||name==='retry'){state.mode='normal';renderApp();$('#preview-state').value='normal';showToast('Sample data refreshed.');return;}
  if(name==='clear-filters'){state.query='';state.status='All';state.category='All categories';renderApp();return;}
  if(name==='today'){state.calendarYear=2026;state.calendarMonth=10;state.selectedDate=M.TODAY;renderApp();return;}
  if(name==='open-reminders'){navigate('reminders');return;}
  if(name==='save-preferences') {
    state.preferences={...state.settingsDraft};
    const theme=state.preferences.appearance==='system'?(matchMedia('(prefers-color-scheme: dark)').matches?'dark':'light'):state.preferences.appearance;
    applyTheme(theme);showToast('Sample preferences saved for this session.');return;
  }
  if(name==='delete-due') {
    const d=selectedDue();if(!d)return;
    if(await confirmAction(`Delete “${d.title}”?`,'This sample due will leave your active list. There is no Undo or restore. Unsaved form edits will not be saved.','Delete due',true)){d.deleted=true;state.mode='normal';navigate('detail');showToast('Sample due deleted.');}return;
  }
  if(name==='disable-reminders'){state.reminders.enabled=false;renderApp();showToast('Reminders disabled in the simulation.');return;}
  if(name==='test-notification'){await confirmAction('Test notification preview','Duewise\nYou have a due-date reminder.\n\nThis generic preview contains no title, amount or notes. No OS notification is sent.','Close preview');return;}
  if(name==='android-notifications'||name==='android-battery'){await confirmAction('Android action preview',name==='android-battery'?'In the Android app, this opens its app / battery settings. This browser prototype cannot read or change Android power settings.':'In the Android app, this opens its notification settings. This prototype cannot read or change OS notification permission.','Got it');return;}
  if(name==='refresh-readiness'){showToast('Simulation only. Android permission and power states are unavailable here.');return;}
  if(name==='skip-onboarding'){navigate('dues');return;}
  if(name==='onboarding-next'){if(state.onboardingStep<3){state.onboardingStep++;renderApp();}else navigate('dues');return;}
  if(name==='onboarding-back'){state.onboardingStep=Math.max(0,state.onboardingStep-1);renderApp();}
}
document.addEventListener('click',event=>{
  const target=event.target.closest('button');if(!target)return;
  if(target.dataset.page){navigate(target.dataset.page,{fromRail:Boolean(target.closest('#screen-nav')),focus:!target.closest('#screen-nav')});$('#preview').scrollTop=0;}
  else if(target.dataset.due){state.selected=target.dataset.due;navigate('detail',{focus:true});$('#preview').scrollTop=0;}
  else if(target.dataset.action){action(target.dataset.action);}
  else if(target.dataset.filter){state.status=target.dataset.filter;renderApp();}
  else if(target.dataset.date){state.selectedDate=target.dataset.date;renderApp();}
  else if(target.dataset.month){const next=state.calendarYear*12+state.calendarMonth+Number(target.dataset.month);state.calendarYear=Math.floor(next/12);state.calendarMonth=next%12;state.selectedDate=`${String(state.calendarYear).padStart(4,'0')}-${String(state.calendarMonth+1).padStart(2,'0')}-01`;renderApp();}
  else if(target.dataset.pref){state.settingsDraft[target.dataset.pref]=target.dataset.value;renderApp();}
  else if(target.dataset.theme){applyTheme(target.dataset.theme);}
});
document.addEventListener('input',event=>{
  const target=event.target;
  if(target.id==='decision-note'){notes[state.page]=target.value;persist();}
  if(target.id==='search'){state.query=target.value;updateSearchResults();}
  if(target.closest('#due-form')&&target.name){state.editorDraft[target.name]=target.type==='checkbox'?target.checked:target.value;if(target.name==='amount')$('#amount-preview').textContent=amountPreview(state.editorDraft);if(target.name==='date')$('#date-preview').textContent=(M.validDate(target.value)?dateText(target.value):'Choose a date')+' ? Asia/Manila. Past dates are allowed.';}
  if(target.closest('#reminder-form')&&target.name)state.reminderDraft[target.name]=target.type==='checkbox'?target.checked:target.value;
});
document.addEventListener('change',event=>{
  const target=event.target;
  if(target.name==='layout-choice'){choices[state.page]=target.value;persist();renderApp();}
  if(target.name==='category'&&target.closest('#due-form')){state.editorDraft.category=target.value;$('#amount-hint').textContent=amountHint(target.value);$('#amount-preview').textContent=amountPreview(state.editorDraft);validateField('amount');}
  if(target.id==='category-filter'){state.category=target.value;updateSearchResults();}
  if(target.id==='preview-width')$('#phone').style.width=target.value+'px';
  if(target.id==='preview-state'){state.mode=target.value;renderApp();}
  if(target.dataset.prefSelect)state.settingsDraft[target.dataset.prefSelect]=target.value;
});
document.addEventListener('focusout',event=>{if(event.target.closest('#due-form')&&['title','amount','date'].includes(event.target.name))validateField(event.target.name);});
document.addEventListener('submit',async event=>{
  if(event.target.id==='due-form')submitDue(event);
  if(event.target.id==='reminder-form'){
    event.preventDefault();const enable=event.submitter?.dataset.enable==='true';
    if(enable&&!await confirmAction('Enable sample reminders?','This simulates the Android opt-in step. No real permission is requested. The draft schedule is saved in temporary demo state only.','Enable simulation'))return;
    state.reminders={...state.reminderDraft,enabled:enable||state.reminders.enabled};renderApp();showToast(enable?'Reminders enabled in the simulation.':'Sample reminder schedule saved.');
  }
});
$('#next-page').addEventListener('click',()=>{navigate(pages[(pages.findIndex(p=>p.id===state.page)+1)%pages.length].id,{fromRail:true});$('#preview').scrollTop=0;});
$('#reset-demo').addEventListener('click',async()=>{if(await confirmAction('Reset the sample data?','This resets sample dues, filters, calendar, preferences and reminders. Your design choices and notes are kept.','Reset demo')){state.dues=M.seed();state.selected='rent';state.parent='dues';state.mode='normal';state.query='';state.status='All';state.category='All categories';state.calendarYear=2026;state.calendarMonth=10;state.selectedDate=M.TODAY;state.preferences={appearance:'system',week:'monday'};state.settingsDraft=null;state.reminders={enabled:false,before:true,time:'09:00',quietStart:'22:00',quietEnd:'08:00'};state.reminderDraft=null;state.onboardingStep=0;applyTheme('light');navigate('dues');showToast('Demo reset. Your design choices are kept.');}});
$('#export-choices').addEventListener('click',()=>{
  const content=['# Duewise mobile UI choices','',`Exported: ${new Intl.DateTimeFormat('en-GB',{dateStyle:'long',timeZone:'Asia/Taipei'}).format(new Date())}`,'','Design preferences only; not approval to change the Android app.','',...pages.flatMap(p=>[`## ${p.label}${p.future?' (future concept)':''}`,'',`Selected: ${p.choices.find(c=>c[0]===choices[p.id])[1]}`,`Recommendation: ${p.recommendation}`,'',`Notes: ${notes[p.id]||'No notes.'}`,''])].join('\n');
  const url=URL.createObjectURL(new Blob([content],{type:'text/markdown;charset=utf-8'})),a=document.createElement('a');a.href=url;a.download='duewise-ui-choices.md';document.body.appendChild(a);a.click();a.remove();setTimeout(()=>URL.revokeObjectURL(url),1000);showToast('Design choices exported.');
});
matchMedia('(prefers-color-scheme: dark)').addEventListener('change',event=>{if(state.preferences.appearance==='system')applyTheme(event.matches?'dark':'light');});
window.addEventListener('hashchange',()=>navigate(location.hash.slice(1),{fromRail:true}));
navigate(pages.some(p=>p.id===location.hash.slice(1))?location.hash.slice(1):'dues',{fromRail:true});
