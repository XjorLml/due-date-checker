/* UX decision lab: draws the native app as built ("Today") next to each proposed option so the user
   can choose visually. Round 2 (open) follows specs/ux-improvements-2/spec.md; round 1 (decided and
   shipped) follows specs/ux-improvements/spec.md. Drawings are illustrations of the Android app, not
   the HTML Focus prototype, and choices are preferences until confirmed. */
(function(){
'use strict';
const esc=s=>String(s).replace(/[&<>"]/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]));

/* ---- Drawing helpers (decorative, aria-hidden; each option also has a text description) ---- */
const STATUS={overdue:['!','Overdue'],today:['●','Due today'],upcoming:['○','Upcoming'],paid:['✓','Paid']};
const badge=(s,label='')=>`<span class="m-badge m-${s}">${STATUS[s][0]} ${label||STATUS[s][1]}</span>`;
const row=(title,meta,amount,status,cls='',label='')=>`<div class="m-row ${cls}"><div class="m-row-main"><b>${esc(title)}</b><small>${esc(meta)}</small></div><div class="m-row-end"><span class="m-amt${amount==='No amount'?' m-absent':''}">${esc(amount)}</span>${badge(status,label)}</div></div>`;
const tap=n=>`<i class="m-tap">${n}</i>`;
const btn=(text,kind='primary',extra='')=>`<span class="m-btn m-${kind}">${text}${extra}</span>`;
const field=(label,value,{hint='',placeholder=false,focus=false,error=''}={})=>`<div class="m-field"><span class="m-label">${label}</span><span class="m-input${focus?' m-focus':''}${error?' m-error-box':''}${placeholder?' m-placeholder':''}">${value}</span>${[].concat(hint).filter(Boolean).map(h=>`<small>${h}</small>`).join('')}${error?`<small class="m-error">${error}</small>`:''}</div>`;
const heading=(h,caption)=>`<div class="m-heading"><b>${h}</b>${caption?`<small>${caption}</small>`:''}</div>`;
const brand=`<div class="m-brand"><span class="m-logo">✓</span>Duewise</div>`;
const NAV=[['dashboard','Dashboard','▦'],['calendar','Calendar','▤'],['add','Add',''],['dues','Dues','☰'],['settings','Settings','⚙']];
function nav(active,{labels=null,big=false}={}){
  return `<div class="m-nav${big?' m-nav-big':''}">${NAV.map(([id,name,icon],i)=>{
    if(id==='add')return `<span class="m-nav-add"><span class="m-disc">+</span><small>Add</small></span>`;
    const label=labels?labels[i]:name;
    return `<span class="m-nav-item${id===active?' m-active':''}"><span class="m-nav-icon">${icon}</span>${label===null?'':`<small>${label}</small>`}</span>`;
  }).join('')}</div>`;
}
function phone(body,{active='dues',push=null,action='',footer='',overlay='',navOpts={},cls='',keyboard=''}={}){
  const top=push?`<div class="m-appbar"><span>←</span><b>${push}</b>${action?`<em class="m-appbar-action">${action}</em>`:''}</div>`:'';
  const bar=push?'':nav(active,navOpts);
  return `<div class="m-phone ${cls}" aria-hidden="true"><div class="m-status"></div>${top}<div class="m-body">${body}</div>${footer}${keyboard}${bar}${overlay}</div>`;
}
const sticky=(a,b)=>`<div class="m-footer">${btn(a,'secondary')}${btn(b,'primary')}</div>`;
const caption=t=>`<p class="m-caption">${t}</p>`;
const pair=(a,b,la,lb)=>`<div class="m-pair"><figure>${a}<figcaption>${la}</figcaption></figure><figure>${b}<figcaption>${lb}</figcaption></figure></div>`;

/* Sample dues used by every drawing (the emulator's seed plus a little history). */
const HISTORY=[['Tuition','15 Sep · Bill','₱8,500.00'],['Electricity','02 Oct · Bill','₱2,140.00'],['Water bill','03 Oct · Bill','₱790.00'],['Phone load','03 Oct · Bill','₱299.00'],['Rent','05 Oct · Bill','₱12,000.00'],['Internet','07 Oct · Bill','₱1,699.00'],['Spotify','12 Oct · Subscription','₱149.00'],['Phone load','03 Nov · Bill','₱299.00']];
const historyRows=n=>HISTORY.slice(0,n).map(([t,m,a])=>row(t,m,a,'paid')).join('');
const currentRows=row('Water bill','03 Nov · Bill','₱850.00','overdue')+row('Rent','05 Nov · Bill','₱12,000.00','overdue')+row('Renew passport','06 Nov · Task','No amount','today')+row('Internet','07 Nov · Bill','₱1,699.00','upcoming');
const duesHead=brand+heading('Your dues.','Date order · all active saved dues · amounts in PHP')+`<span class="m-chip">Reminders</span>`;
const detailsBody=(extra='',amount='₱1,699.00',st='upcoming')=>`<small class="m-eyebrow">Bill</small><b class="m-title">Internet</b><b class="m-hero">${amount}</b>${badge(st)}<div class="m-facts"><span><small>Due date</small>07 Nov 2026</span><span><small>Type</small>Bill</span><span class="m-notes"><small>Notes</small>No notes.</span></div>${extra}`;
const editorTop=(fields)=>`<p class="m-caption">Title, amount and date first. Everything else is optional.</p>${fields}`;
const keypad=(keys)=>`<div class="m-keypad">${keys.map(k=>`<span${k===''?' class="m-blank"':''}>${k}</span>`).join('')}</div>`;

/* ---- Round 1 decisions (ranked by impact; see specs/ux-improvements/plan.md). Drawings show the
   app before Waves 1–5 and are kept unchanged as the record the user chose from on 2026-11-06. ---- */
const D1=[
{id:'UX01',rank:1,wave:1,title:'Mark as paid',score:[3,3,2],effort:'S',
 finding:'Marking a due paid, the most frequent action, takes about five taps and a scroll: row → Details → Edit due → scroll → Payment switch → Save changes.',
 rule:'PRD F01 ("Details offers Edit and Back to dues"); DESIGN item detail',
 today:{what:'Details offers only Edit due and Back to list. Paid is a switch at the bottom of the editor and applies on Save.',
  draw:()=>pair(phone(detailsBody(`${btn('Edit due'+tap(2),'primary')}${btn('Back to list','secondary')}`),{push:'Due details'}),
   phone(editorTop(field('Notes (optional)','Add a reference or anything useful',{placeholder:true}))+`<div class="m-card"><span class="m-label">Payment status</span><span class="m-switch">${tap(4)}</span> Unpaid<small>Changes apply when you save.</small></div>`,{push:'Edit due',footer:sticky('Cancel','Save changes'+tap(5))}),'Details (tap 1 opened it)','Editor, after scrolling (3)')},
 options:[
 {key:'A',name:'Mark as paid on Details',rec:true,what:'A primary "Mark as paid" button on Details saves at once; "Edit due" becomes secondary. On a paid due it reads "Mark as unpaid".',
  pros:['Two taps from the list, one from a reminder','Hard to tap by accident; one clear primary action','Reuses the existing guarded save'],cons:['Still opens Details first'],
  draw:()=>pair(phone(detailsBody(`${btn('Mark as paid'+tap(2),'primary')}${btn('Edit due','secondary')}${btn('Back to list','quiet')}`),{push:'Due details'}),
   phone(detailsBody(`${btn('Mark as unpaid','secondary')}${btn('Edit due','secondary')}`,'₱1,699.00','paid'),{push:'Due details',overlay:'<span class="m-toast">Marked paid</span>'}),'Tap 2: Mark as paid','Result: badge shows Paid')},
 {key:'B',name:'Swipe a row',what:'Swiping a row in Dues reveals a green "Paid" action.',
  pros:['One gesture from the list'],cons:['Hidden: nothing shows the gesture','Needs a separate TalkBack action','Swipes are easy to trigger while scrolling'],
  draw:()=>phone(duesHead+row('Water bill','03 Nov · Bill','₱850.00','overdue')+`<div class="m-swipe">${row('Rent','05 Nov · Bill','₱12,000.00','overdue','m-shifted')}<span class="m-rail">✓ Paid</span></div>`+row('Internet','07 Nov · Bill','₱1,699.00','upcoming'))},
 {key:'C',name:'Checkbox on every row',what:'Each Dues row gets a round checkbox that marks it paid.',
  pros:['One tap from the list','Visible'],cons:['Easy to mark the wrong row by accident','Crowds rows that already hold amount and badge','No room left at 200% text'],
  draw:()=>phone(duesHead+['Water bill|03 Nov · Bill|₱850.00|overdue','Rent|05 Nov · Bill|₱12,000.00|overdue','Internet|07 Nov · Bill|₱1,699.00|upcoming'].map(s=>{const[t,m,a,st]=s.split('|');return `<div class="m-check-row"><span class="m-check"></span>${row(t,m,a,st)}</div>`;}).join(''))}
 ]},
{id:'UX02',rank:2,wave:1,title:'Dues shows what is next',score:[3,3,2],effort:'M',
 finding:'Dues, the start page, lists every active due in ascending date order including paid ones, so after a few months the first screen is paid history and what is next sinks below it.',
 rule:'PRD R03 presentation; DESIGN Dues list (as built)',
 today:{what:'After two months of use, the first screen shows only paid history.',draw:()=>phone(duesHead+historyRows(6)+caption('… 2 more paid, then Water bill, Rent, Renew passport, Internet'))},
 options:[
 {key:'A',name:'Collapse earlier paid dues',rec:true,what:'Paid dues dated before today fold into one row at the top. Tapping it expands them in place. Order stays by date; unpaid overdue dues always show.',
  pros:['What is next is on the first screen','Keeps the date order R03 promises','Smallest change; history is one tap away'],cons:['One extra row at the top'],
  draw:()=>phone(duesHead+`<div class="m-collapsed">Show 8 earlier paid dues <span>›</span></div>`+currentRows)},
 {key:'B',name:'Sections: Overdue, Upcoming, Paid',what:'The list is grouped under headings, with Paid collapsed at the bottom.',
  pros:['Very clear grouping','Paid history out of the way'],cons:['A new list structure to build and test','Date order is broken across sections'],
  draw:()=>phone(duesHead+`<div class="m-section">Overdue · 2</div>`+row('Water bill','03 Nov · Bill','₱850.00','overdue')+row('Rent','05 Nov · Bill','₱12,000.00','overdue')+`<div class="m-section">Today and upcoming · 2</div>`+row('Renew passport','06 Nov · Task','No amount','today')+row('Internet','07 Nov · Bill','₱1,699.00','upcoming')+`<div class="m-section">Paid · 8 <span>›</span></div>`)},
 {key:'C',name:'Open scrolled to today',what:'Nothing is hidden; the list opens already scrolled to the first due on or after today, with a "Today" marker.',
  pros:['No hidden items'],cons:['A jump that is easy to miss','Screen readers start mid-list','Overdue items end up above the fold'],
  draw:()=>phone(`<div class="m-scrolled">${row('Rent','05 Oct · Bill','₱12,000.00','paid')}${row('Internet','07 Oct · Bill','₱1,699.00','paid')}${row('Spotify','12 Oct · Subscription','₱149.00','paid')}${row('Phone load','03 Nov · Bill','₱299.00','paid')}${row('Water bill','03 Nov · Bill','₱850.00','overdue')}${row('Rent','05 Nov · Bill','₱12,000.00','overdue')}<div class="m-today-line">Today · 06 Nov</div>${row('Renew passport','06 Nov · Task','No amount','today')}${row('Internet','07 Nov · Bill','₱1,699.00','upcoming')}</div>`)}
 ]},
{id:'UX03',rank:3,wave:2,title:'Recurring dues',score:[2,3,3],effort:'L',
 finding:'There is no Repeat. Rent, utilities and subscriptions must be re-entered every month, and forgetting to re-enter one defeats the app\'s purpose.',
 rule:'PRD exclusions (recurrence deferred); roadmap F07 recurrence; SDD schema 5',
 today:{what:'The editor has no Repeat. After paying November\'s rent you add December\'s yourself.',draw:()=>phone(editorTop(field('What is due?','Rent')+field('Amount (PHP)','12000','',{hint:'Required for Bill.'})+field('Due date','2026-11-05')+field('Type','Bill')),{push:'Add due',footer:sticky('Cancel','Save due')})},
 options:[
 {key:'A',name:'Next one appears when you mark paid',rec:true,what:'Choose Repeat (Never, Weekly, Monthly, Yearly). When you mark this one paid, the next one is created on the next date with the same title, amount, type and notes. A 31st falls back to the last day of shorter months.',
  pros:['The list only shows what is due now','Paying is what moves you to the next one','Works with Mark as paid (UX01)'],cons:['You do not see next month\'s bill until this one is paid'],
  draw:()=>pair(phone(editorTop(field('What is due?','Rent')+field('Amount (PHP)','12000','',{hint:'Required for Bill.'})+field('Due date','05 Nov 2026')+field('Repeat','Monthly ▾',{focus:true})),{push:'Add due',footer:sticky('Cancel','Save due')}),
   phone(detailsBody(`${btn('Mark as unpaid','secondary')}`,'₱12,000.00','paid').replace('Internet','Rent').replace('07 Nov 2026','05 Nov 2026'),{push:'Due details',overlay:'<span class="m-toast">Paid. Next one added for 05 Dec 2026</span>'}),'Repeat in the editor','After Mark as paid')},
 {key:'B',name:'Next one always visible ahead',what:'The next occurrence exists as soon as you save, so December\'s rent already shows in the list and calendar.',
  pros:['You can see future bills on the Calendar'],cons:['The list doubles every repeating bill','Editing one occurrence vs all needs extra choices','More reminder rows to manage'],
  draw:()=>phone(duesHead+row('Rent','05 Nov · Bill','₱12,000.00','overdue')+row('Internet','07 Nov · Bill','₱1,699.00','upcoming')+row('Rent','05 Dec · Bill','₱12,000.00','upcoming')+row('Internet','07 Dec · Bill','₱1,699.00','upcoming'))}
 ]},
{id:'UX04',rank:4,wave:2,title:'Reminder reliability on Xiaomi-type phones',score:[2,3,2],effort:'S',
 finding:'MIUI/HyperOS and similar systems can stop background work, so reminders arrive late. The app shows what Android reports and an app-settings button, but cannot see Xiaomi\'s Autostart setting.',
 rule:'F05 spec; DESIGN notification settings',
 today:{what:'Reminders shows Android\'s status and one settings button.',draw:()=>phone(`<b class="m-title">Reminders</b>`+`<div class="m-card"><b>Background delivery</b><small>Battery optimization is active. Review Android app battery settings if reminders arrive late.</small></div>`+btn('Open Android app settings','secondary'),{push:'Reminders'})},
 options:[
 {key:'A',name:'Steps for your phone\'s maker',rec:true,what:'On Xiaomi, Redmi and POCO phones, the page adds two plain steps next to the existing button. Other makers follow later.',
  pros:['Works on every MIUI/HyperOS version','No risk of a crash or a dead button'],cons:['You still navigate Android settings yourself'],
  draw:()=>phone(`<b class="m-title">Reminders</b><div class="m-card"><b>Background delivery</b><small>Battery optimization is active.</small></div><div class="m-card m-hint"><b>On Xiaomi, Redmi and POCO</b><small>1. Settings › Apps › Duewise › Autostart: On</small><small>2. Battery saver: No restrictions</small></div>`+btn('Open Android app settings','secondary'),{push:'Reminders'})},
 {key:'B',name:'Buttons that open Xiaomi\'s screens',what:'Two buttons jump straight into Xiaomi\'s Autostart and battery screens.',
  pros:['One tap closer'],cons:['Those screens move between versions; a button can open nothing or crash','Needs testing on each version'],
  draw:()=>phone(`<b class="m-title">Reminders</b><div class="m-card"><b>Background delivery</b><small>Battery optimization is active.</small></div>`+btn('Open Autostart settings','secondary')+btn('Open Battery saver settings','secondary')+caption('May not open on some versions.'),{push:'Reminders'})}
 ]},
{id:'UX05',rank:5,wave:3,title:'Export and import your data',score:[3,1,3],effort:'L',
 finding:'There is no export or backup, and Android backup is deliberately off. Losing or replacing the phone loses every due; the 2026-11-06 near-loss showed it.',
 rule:'PRD exclusions (export) and F04; SDD data-loss limits; ADR needed',
 today:{what:'Settings says data stays on this phone. There is no way to copy it out.',draw:()=>phone(heading('Make it feel like you.','')+`<div class="m-card"><b>Appearance</b><span class="m-seg"><i class="on">System</i><i>Light</i><i>Dark</i></span></div><div class="m-card"><small>Your dues are stored only on this phone. There is no backup.</small></div>`+btn('Save preferences','primary'),{active:'settings'})},
 options:[
 {key:'A',name:'Export to a file you choose',rec:true,what:'"Export dues" saves a file where you pick (Downloads, Drive, a USB stick). "Import" restores it into an empty app on a new phone.',
  pros:['Nothing leaves the phone unless you move the file','Keeps today\'s privacy choice','Works without a Google account'],cons:['You have to remember to export'],
  draw:()=>pair(phone(heading('Make it feel like you.','')+`<div class="m-card"><b>Your data</b><small>Save a copy of all dues to a file you choose.</small></div>`+btn('Export dues','primary')+btn('Import from a file','secondary')+caption('Import works on an empty app only.'),{active:'settings'}),
   phone(`<b class="m-title">Save to</b><div class="m-picker"><span>📁 Downloads</span><span>☁ Google Drive</span><span>📁 Documents</span></div><div class="m-field"><span class="m-label">File name</span><span class="m-input">duewise-dues-2026-11-06.json</span></div>`+btn('Save','primary'),{push:'Android file picker'}),'Settings','Android picks the place')},
 {key:'B',name:'Android backup to your Google account',what:'Turn Android Auto Backup back on so a new phone restores dues automatically.',
  pros:['Automatic','Nothing to remember'],cons:['Copies financial data to Google backup','Reverses the deliberate allowBackup=false','Restore timing is controlled by Android'],
  draw:()=>phone(heading('Make it feel like you.','')+`<div class="m-card"><b>Backup</b><small>Duewise is included in your Google account backup.</small><span class="m-switch on"></span></div>`,{active:'settings'})}
 ]},
{id:'UX06',rank:6,wave:4,title:'Confirmation after actions',score:[3,3,1],effort:'M',
 finding:'After Save, a payment change or Delete, the only feedback is the changed list.',
 rule:'DESIGN toasts (future); PRD keeps no Undo',
 today:{what:'You return to the list; nothing says the save worked.',draw:()=>phone(duesHead+currentRows)},
 options:[
 {key:'A',name:'Short message, no Undo',rec:true,what:'"Saved", "Marked paid" or "Deleted" shows above the bottom bar for 3 seconds and is read aloud by TalkBack.',
  pros:['Clear feedback','No PRD change'],cons:['Cannot undo a delete'],draw:()=>phone(duesHead+currentRows,{overlay:'<span class="m-toast m-toast-nav">Saved</span>'})},
 {key:'B',name:'Message with Undo for delete',what:'After Delete, the message offers Undo for 5 seconds.',
  pros:['Recovers from a mistaken delete'],cons:['Needs a PRD change (v1 has no Undo) and restore rules','More states to test'],draw:()=>phone(duesHead+currentRows,{overlay:'<span class="m-toast m-toast-nav">Due deleted <u>Undo</u></span>'})}
 ]},
{id:'UX07',rank:7,wave:4,title:'Editor date format',score:[3,2,1],effort:'S',
 finding:'The editor shows the date as 2026-11-06 while every other page shows 06 Nov 2026.',rule:'PRD business rule ("the editor uses yyyy-MM-dd")',
 today:{what:'ISO date in the editor.',draw:()=>phone(editorTop(field('What is due?','Internet')+field('Amount (PHP)','1699')+field('Due date','2026-11-07')),{push:'Edit due',footer:sticky('Cancel','Save changes')})},
 options:[
 {key:'A',name:'06 Nov 2026',rec:true,what:'The date field reads like the rest of the app.',pros:['Consistent with lists and Details','Month name avoids day/month mix-ups'],cons:['Slightly longer'],draw:()=>phone(editorTop(field('What is due?','Internet')+field('Amount (PHP)','1699')+field('Due date','07 Nov 2026',{focus:true})),{push:'Edit due',footer:sticky('Cancel','Save changes')})},
 {key:'B',name:'Keep 2026-11-06',what:'The date field keeps the year-month-day form, unlike the rest of the app.',pros:['Unambiguous for technical users'],cons:['Inconsistent with every other page'],same:true,draw:()=>phone(editorTop(field('Due date','2026-11-07')),{push:'Edit due',footer:sticky('Cancel','Save changes')})}
 ]},
{id:'UX08',rank:8,wave:4,title:'Amount keyboard',score:[3,2,1],effort:'S',
 finding:'The number keyboard offers "," and "-", which the amount check then rejects.',rule:'None',
 today:{what:'Number keyboard with comma and minus.',draw:()=>phone(editorTop(field('Amount (PHP)','1,000',{focus:true,error:'Enter a valid PHP amount, for example 1250.50.'})),{push:'Add due',keyboard:keypad(['1','2','3','-','4','5','6',',','7','8','9','⌫','.','0','_','✓'])})},
 options:[{key:'A',name:'Digits and decimal point only',rec:true,what:'The keyboard shows only digits and ".". Validation stays as the real guard. Needs a check on the phone\'s own keyboard.',pros:['Fewer invalid entries'],cons:['Some keyboards ignore the hint'],draw:()=>phone(editorTop(field('Amount (PHP)','1000',{focus:true})),{push:'Add due',keyboard:keypad(['1','2','3','','4','5','6','','7','8','9','⌫','.','0','','✓'])})}]},
{id:'UX09',rank:9,wave:4,title:'Dues and Dashboard roles',score:[3,2,1],effort:'M',
 finding:'Dues and Dashboard both list the same dues; only Dashboard can search. Decide after UX02 has been used.',rule:'PRD F02/R03 page roles (for B or C)',
 today:{what:'Dues: full list, no search. Dashboard: summary, search, filters.',draw:()=>pair(phone(duesHead+currentRows),phone(heading('A clear plan for today.','Friday, 06 Nov 2026')+`<b class="m-hero">₱1,699.00</b><small class="m-meta">2 unpaid · today to 30 Nov</small>`+field('Find a due','Search a bill or task',{placeholder:true}),{active:'dashboard'}),'Dues','Dashboard')},
 options:[
 {key:'A',name:'Keep as is',rec:true,what:'Dues stays the full list; search stays on Dashboard.',pros:['No change'],cons:['Two lists remain'],same:true,draw:()=>phone(duesHead+currentRows)},
 {key:'B',name:'Search on Dues too',what:'A search field at the top of Dues.',pros:['Find a due from the start page'],cons:['Duplicates Dashboard search'],draw:()=>phone(duesHead+field('Search','Search a bill or task',{placeholder:true})+currentRows)},
 {key:'C',name:'Open on Dashboard; rename Dues "All dues"',what:'The app starts on Dashboard; Dues becomes the archive of everything.',pros:['Next date first on launch'],cons:['Changes the start page users know'],draw:()=>phone(heading('A clear plan for today.','Friday, 06 Nov 2026')+`<b class="m-hero">₱1,699.00</b><small class="m-meta">Next date · 06 Nov · Renew passport</small>`,{active:'dashboard',navOpts:{labels:['Dashboard','Calendar','Add','All dues','Settings']}})}
 ]},
{id:'UX10',rank:10,wave:4,title:'Counters as real buttons',score:[1,3,2],effort:'S',
 finding:'The Overdue / Today / Paid counters are tappable boxes, not native buttons, so screen readers do not announce them as buttons.',rule:'DESIGN summary counters',
 today:{what:'Looks like buttons; TalkBack reads "2 overdue. Shows overdue dues" with no button role.',draw:()=>phone(`<div class="m-counters"><span><b class="m-red">2</b>Overdue</span><span><b class="m-amber">1</b>Today</span><span><b class="m-green">1/5</b>Paid</span></div>`+caption('TalkBack: "2 overdue. Shows overdue dues"'),{active:'dashboard'})},
 options:[{key:'A',name:'Native buttons, same look',rec:true,what:'Same appearance; TalkBack adds "button" and they work with switch access.',pros:['Correct role for assistive technology'],cons:['None visible'],draw:()=>phone(`<div class="m-counters"><span><b class="m-red">2</b>Overdue</span><span><b class="m-amber">1</b>Today</span><span><b class="m-green">1/5</b>Paid</span></div>`+caption('TalkBack: "2 overdue. Shows overdue dues, button"'),{active:'dashboard'})}]},
{id:'UX11',rank:11,wave:4,title:'Type before Amount',score:[2,2,1],effort:'S',
 finding:'Type comes after Amount, so someone adding a Task first reads "Required for Bill".',rule:'DESIGN and Focus adoption editor order',
 today:{what:'Title → Amount → Date → Type.',draw:()=>phone(editorTop(field('What is due?','Renew passport')+field('Amount (PHP)','1250.50',{placeholder:true,hint:'Required for Bill.'})+field('Due date','06 Nov 2026')+field('Type','Bill')),{push:'Add due',footer:sticky('Cancel','Save due')})},
 options:[
 {key:'A',name:'Title → Type → Amount → Date',rec:true,what:'Choose Type right after the title, so the amount hint is already right.',pros:['No misleading "Required" for Tasks'],cons:['Amount moves one field down'],draw:()=>phone(editorTop(field('What is due?','Renew passport')+field('Type','Task',{focus:true})+field('Amount (PHP)','Optional',{placeholder:true,hint:'Optional for Task.'})+field('Due date','06 Nov 2026')),{push:'Add due',footer:sticky('Cancel','Save due')})},
 {key:'B',name:'Keep current order',what:'Title, Amount, Date, then Type, as today; the amount hint follows Type once it is changed.',pros:['Amount stays near the top'],cons:['Hint is wrong until Type is changed'],same:true,draw:()=>phone(editorTop(field('What is due?','Renew passport')+field('Amount (PHP)','1250.50',{placeholder:true,hint:'Required for Bill.'})),{push:'Add due',footer:sticky('Cancel','Save due')})}
 ]},
{id:'UX12',rank:12,wave:4,title:'Times on Details',score:[3,1,1],effort:'S',
 finding:'Details shows Created and Last updated in UTC, eight hours behind Manila.',rule:'PRD F01 ("explicit UTC labels")',
 today:{what:'"Created 06 Nov 2026, 03:11 UTC" for something added at 11:11.',draw:()=>phone(detailsBody(`<small class="m-meta">Created 06 Nov 2026, 03:11 UTC</small><small class="m-meta">Last updated 06 Nov 2026, 03:11 UTC</small>`),{push:'Due details'})},
 options:[
 {key:'A',name:'Manila time',rec:true,what:'"Created 06 Nov 2026, 11:11 Manila".',pros:['Matches the clock on the wall'],cons:['None for local users'],draw:()=>phone(detailsBody(`<small class="m-meta">Created 06 Nov 2026, 11:11 Manila</small><small class="m-meta">Last updated 06 Nov 2026, 11:11 Manila</small>`),{push:'Due details'})},
 {key:'B',name:'Keep UTC',what:'Created and Last updated stay in UTC with the UTC label, eight hours behind Manila.',pros:['Matches stored values'],cons:['Confusing for everyone in Manila'],same:true,draw:()=>phone(detailsBody(`<small class="m-meta">Created 06 Nov 2026, 03:11 UTC</small>`),{push:'Due details'})}
 ]},
{id:'UX13',rank:13,wave:5,title:'Bottom bar labels at large text',score:[1,3,1],effort:'M',
 finding:'At 200% text or on 320 dp screens the labels cut off ("Dash…", "Settin…").',rule:'DESIGN bottom bar',
 today:{what:'200% text: labels truncate.',draw:()=>phone(duesHead+currentRows,{cls:'m-big',navOpts:{big:true,labels:['Dash…','Calen…','Add','Dues','Settin…']}})},
 options:[
 {key:'A',name:'Cap label size in the bar',rec:true,what:'Labels stop growing past a set size; icons still identify tabs and TalkBack reads full names.',pros:['Always readable','No layout change'],cons:['Labels do not follow very large text'],draw:()=>phone(duesHead+currentRows,{cls:'m-big'})},
 {key:'B',name:'Two-line labels',what:'Long labels wrap onto two lines.',pros:['Follows text size'],cons:['Taller bar, less list space'],draw:()=>phone(duesHead+currentRows,{cls:'m-big',navOpts:{big:true,labels:['Dash-<br>board','Calen-<br>dar','Add','Dues','Set-<br>tings']}})},
 {key:'C',name:'Label only on the selected tab',what:'Unselected tabs show icons only.',pros:['Room for large text'],cons:['Icons alone are harder to learn'],draw:()=>phone(duesHead+currentRows,{cls:'m-big',navOpts:{big:true,labels:[null,null,'Add','Dues',null]}})}
 ]},
{id:'UX14',rank:14,wave:5,title:'"No amount" on Details',score:[2,1,1],effort:'S',
 finding:'"No amount" uses the same large style as a peso amount.',rule:'DESIGN F07 optional-amount adaptation',
 today:{what:'Large hero text.',draw:()=>phone(detailsBody('', 'No amount','today').replace('Internet','Renew passport').replace('<small class="m-eyebrow">Bill','<small class="m-eyebrow">Task'),{push:'Due details'})},
 options:[
 {key:'A',name:'Quieter body text',rec:true,what:'"No amount" in secondary colour at body size.',pros:['The title stays the focus','Clearly not a number'],cons:['Less symmetrical with paid bills'],draw:()=>phone(detailsBody('', '<span class="m-quiet">No amount</span>','today').replace('Internet','Renew passport').replace('<small class="m-eyebrow">Bill','<small class="m-eyebrow">Task'),{push:'Due details'})},
 {key:'B',name:'Keep large',what:'"No amount" keeps the large amount style used for peso values.',pros:['Same layout as other dues'],cons:['Shouts an absence'],same:true,draw:()=>phone(detailsBody('', 'No amount','today').replace('Internet','Renew passport'),{push:'Due details'})}
 ]},
{id:'UX15',rank:15,wave:5,title:'Calendar on very narrow phones',score:[1,2,1],effort:'S',
 finding:'At 320 dp the seven 44 dp day columns do not fit, so the grid scrolls sideways and Sunday needs a swipe.',rule:'DESIGN calendar grid / 44 dp targets',
 today:{what:'Six columns visible, swipe for Sunday.',draw:()=>phone(heading('A date for everything.','')+`<div class="m-cal m-cal-6">${['M','Tu','W','Th','F','Sa'].map(d=>`<i>${d}</i>`).join('')}${[2,3,4,5,6,7,9,10,11,12,13,14].map(n=>`<span${n===6?' class="on"':''}>${n}</span>`).join('')}</div>`+caption('Swipe sideways for Sunday →'),{active:'calendar',cls:'m-narrow'})},
 options:[
 {key:'A',name:'Keep 44 dp cells and scroll',rec:true,what:'No change: target size wins on rare 320 dp phones.',pros:['Every day stays easy to tap'],cons:['Sunday needs a swipe there'],same:true,draw:()=>phone(`<div class="m-cal m-cal-6">${['M','Tu','W','Th','F','Sa'].map(d=>`<i>${d}</i>`).join('')}${[2,3,4,5,6,7].map(n=>`<span${n===6?' class="on"':''}>${n}</span>`).join('')}</div>`,{active:'calendar',cls:'m-narrow'})},
 {key:'B',name:'Shrink cells to fit',what:'Below 340 dp, cells shrink so all seven columns fit.',pros:['Whole week visible'],cons:['Cells drop below the 44 dp touch target'],draw:()=>phone(`<div class="m-cal m-cal-7">${['M','Tu','W','Th','F','Sa','Su'].map(d=>`<i>${d}</i>`).join('')}${[2,3,4,5,6,7,8].map(n=>`<span${n===6?' class="on"':''}>${n}</span>`).join('')}</div>`,{active:'calendar',cls:'m-narrow'})}
 ]},
{id:'UX16',rank:16,wave:5,title:'Landscape headings',score:[1,1,1],effort:'S',
 finding:'In landscape the big page headings leave room for about one row.',rule:'DESIGN layout',
 today:{what:'One row visible.',draw:()=>phone(duesHead+row('Water bill','03 Nov · Bill','₱850.00','overdue'),{cls:'m-land'})},
 options:[
 {key:'A',name:'Smaller headings in landscape',rec:true,what:'Root pages use a compact heading when the screen is wider than tall.',pros:['Three or four rows visible'],cons:['Less brand presence sideways'],draw:()=>phone(`<div class="m-heading m-compact"><b>Your dues.</b></div>`+row('Water bill','03 Nov · Bill','₱850.00','overdue')+row('Rent','05 Nov · Bill','₱12,000.00','overdue')+row('Internet','07 Nov · Bill','₱1,699.00','upcoming'),{cls:'m-land'})},
 {key:'B',name:'Leave as is',what:'Landscape is rare for this app.',pros:['No work'],cons:['Cramped sideways'],same:true,draw:()=>phone(duesHead+row('Water bill','03 Nov · Bill','₱850.00','overdue'),{cls:'m-land'})}
 ]}
];
const WAVES1={1:'Wave 1 · daily friction',2:'Wave 2 · never miss a payment',3:'Wave 3 · protect the data',4:'Wave 4 · polish',5:'Wave 5 · low impact'};
// Round 1 choices as recorded in specs/ux-improvements/spec.md#decision-log (decided 2026-11-06, shipped in Waves 1–5).
const DECIDED1={UX01:'A',UX02:'A',UX03:'A',UX04:'A',UX05:'A',UX06:'B',UX07:'A',UX08:'A',UX09:'A',UX10:'A',UX11:'A',UX12:'A',UX13:'A',UX14:'A',UX15:'B',UX16:'A'};

/* ---- Round 2 drawings (docs/delivery/UX-REVIEW-2026-11-07.md): the app as built after Waves 1–5,
   texts from the working-tree pages on 2026-11-07. Sample date Saturday 07 Nov 2026: ₱12,850.00 is
   overdue (Water bill, Rent) and the upcoming total to 30 Nov is ₱1,699.00 (Internet). ---- */
const DUES2=[['Water bill','03 Nov','Bill','₱850.00','overdue',4],['Rent','05 Nov','Bill · Monthly','₱12,000.00','overdue',2],['Renew passport','07 Nov','Task','No amount','today'],['Internet','09 Nov','Bill · Monthly','₱1,699.00','upcoming'],['Spotify','02 Dec','Subscription · Monthly','₱149.00','upcoming'],['Electricity','04 Dec','Bill','₱2,140.00','upcoming'],['Tuition','15 Dec','Bill','₱8,500.00','upcoming']];
const late=n=>`${n} day${n===1?'':'s'} late`;
const rows2=({lateMeta=false,lateBadge=false}={})=>DUES2.map(([t,d,ty,a,s,days])=>row(t,`${d} · ${ty}${lateMeta&&days?` · ${late(days)}`:''}`,a,s,'',lateBadge&&days?late(days):'')).join('');
const rowC=(t,meta,a,s)=>`<div class="m-row m-row-c"><div class="m-row-main"><b>${esc(t)}</b><small>${badge(s)}<span>${esc(meta)}</span></small></div><div class="m-row-end"><span class="m-amt${a==='No amount'?' m-absent':''}">${esc(a)}</span></div></div>`;
const rowsC=()=>DUES2.map(([t,d,ty,a,s])=>rowC(t,`${d} · ${ty}`,a,s)).join('');
const fold2='<div class="m-collapsed">Show 8 earlier paid dues <span>›</span></div>';
const chip2=(s='')=>s==='off'?'<span class="m-chip m-chip-off">🔕 Reminders off</span>':s==='on'?'<span class="m-chip m-chip-on">🔔 Reminders on · 09:00</span>':'<span class="m-chip">Reminders</span>';
const dues2=(c=chip2(),body=fold2+rows2())=>brand+heading('Your dues.','Date order · all active saved dues · amounts in PHP')+c+body;
const compactDues=c=>`<div class="m-brandrow">${brand}${c}</div>`+heading('Your dues.','')+fold2+rowsC();
const DASH_CAPTION='Counts cover all saved dues. The total covers known unpaid amounts from today to month end.';
const counters2='<div class="m-counters"><span><b class="m-red">2</b>Overdue</span><span><b class="m-amber">1</b>Today</span><span><b class="m-green">8/15</b>Paid</span></div>';
const summary2=({label='Upcoming · November',total='₱1,699.00',meta='2 unpaid · today to 30 Nov',extra=''}={})=>`<div class="m-summary"><small class="m-label">${label}</small><b class="m-hero">${total}</b><small class="m-meta">${meta}</small>${extra}<span class="m-next"><small>Next date · 07 Nov</small><u>Renew passport</u></span></div>`;
const dash2=(opts={},{cap=DASH_CAPTION,top=''}={})=>heading('A clear plan for today.','Saturday, 07 Nov 2026')+top+summary2(opts)+counters2+(cap?caption(cap):'')+field('Find a due','Search a bill or task',{placeholder:true});
const overdueLine='<span class="m-overdue-line">! ₱12,850.00 overdue · 2 dues <span>›</span></span>';
const details2=({cap=true,late:lateText='',label='',actions=null}={})=>`${cap?caption('Saved information · Amounts in PHP · Dates use Asia/Manila'):''}<small class="m-eyebrow">Bill</small><b class="m-title">Water bill</b><b class="m-hero">₱850.00</b><span class="m-status-line">${badge('overdue',label)}${lateText?`<small>${lateText}</small>`:''}</span><div class="m-facts"><span><small>Due date</small>03 Nov 2026</span><span><small>Type</small>Bill</span><span class="m-notes"><small>Notes</small>No notes.</span></div>${actions??btn('Mark as paid','primary')+btn('Edit due','secondary')+btn('Back to list','secondary')}`;
const AMOUNT_RULE='Use a decimal point, without commas or a peso symbol. Zero is allowed.';
const TYPE_HINT='Bill and Subscription need an amount. Task and Other can leave it blank.';
const editor2=({cap=true,hints=['Required for Bill.',AMOUNT_RULE,'₱1,699.00'],typeHint=TYPE_HINT}={})=>(cap?caption('Title, type, amount and date first. Everything else is optional.'):'')+field('What is due?','Internet')+field('Type','Bill ▾',{hint:typeHint})+field('Amount (PHP)','1699',{hint:hints})+field('Due date','09 Nov 2026');
const seg=(items,on)=>`<span class="m-seg">${items.map(i=>`<i${i===on?' class="on"':''}>${i}</i>`).join('')}</span>`;
const settings2=({appearance='System',hint='System follows your device. Save to apply.',data='',save=true}={})=>heading('Make it feel like you.','A few preferences. A clearer routine. Saved on this device.')+`<div class="m-card"><b>Appearance</b><small>${hint}</small>${seg(['System','Light','Dark'],appearance)}</div><div class="m-card"><b>Start of week</b>${seg(['Monday','Sunday'],'Monday')}</div><div class="m-card m-linkrow"><b>◷ Reminders</b><small>Off ›</small></div><div class="m-card"><b>Your data</b><small>Export saves every due, deleted ones included, to a file you choose.</small>${data}${btn('Export dues','secondary')}${btn('Import dues','secondary')}<small>The file isn't encrypted: keep it somewhere private.</small></div>`+(save?caption('Save applies both preferences. Back discards unsaved choices.')+btn('Save preferences','primary'):'');
const toggleRow=(text,on)=>`<span class="m-toggle-row">${text}<span class="m-switch${on?' on':''}"></span></span>`;
const remSchedule=(intro=true)=>`<b class="m-sub">Schedule</b>${intro?'<small class="m-meta">A reminder is scheduled on each unpaid due date. Include the day before for an additional reminder.</small>':''}${toggleRow('Also one day before',true)}${field('Reminder time (Asia/Manila)','09:00')}<b class="m-sub">Quiet hours</b>${intro?'<small class="m-meta">Reminders inside quiet hours move to the next permitted time. Equal start and end times turn quiet hours off.</small>':''}<div class="m-two">${field('Start','22:00')}${field('End','08:00')}</div>`;
const remStatusNow='<p class="m-caption">Local reminders for unpaid dues. All times below use Asia/Manila.</p><div class="m-card"><b>Reminders are off</b><small>Android notification permission and Due reminders channel are allowed.</small><small>Battery optimization is active. Review Android app battery settings if reminders arrive late.</small></div><small class="m-meta">No future reminder is currently scheduled.</small>';
const remLate=`<b class="m-sub">If reminders arrive late</b><small class="m-meta">Android may delay reminders to save battery.</small>${btn('Android app / battery settings','secondary')}`;
const dialog=(title,body,actions)=>`<div class="m-dim"><div class="m-dialog"><b>${title}</b>${body?`<small>${body}</small>`:''}<span class="m-dialog-actions">${actions.map((a,i)=>`<i${i===actions.length-1?' class="on"':''}>${a}</i>`).join('')}</span></div></div>`;
const CAL_COUNTS={3:2,5:1,7:1,9:1};
function calendar2(mode,{cap="Find your day. See what's on it. Dates use Asia/Manila."}={}){
  const cells=['','','','','','',...Array.from({length:15},(_,i)=>i+1)].map(n=>{
    if(n==='')return '<span class="m-blank"></span>';
    const c=CAL_COUNTS[n]||0,cls=n===7?' class="on"':'';
    const mark=!c?'':mode==='badge'?`<em class="m-cnt">${c}</em>`:mode==='dots'?`<small class="m-dots">${'•'.repeat(Math.min(c,3))}</small>`:`<small>${c}</small>`;
    return `<span${cls}>${n}${mark}</span>`;
  }).join('');
  return heading('A date for everything.',cap)+`<div class="m-cal-bar"><b>November 2026</b><span>‹ › Today</span></div><div class="m-cal2">${['M','Tu','W','Th','F','Sa','Su'].map(d=>`<i>${d}</i>`).join('')}${cells}</div><small class="m-meta">Saturday, 07 Nov 2026 · 1 saved due</small>`+row('Renew passport','07 Nov · Task','No amount','today');
}

const D2=[
{id:'UX17',rank:1,wave:6,title:'Reminder state on Dues',score:[2,2,3],effort:'S',
 finding:'Reminders are off on every installation, and nothing on Dues, Dashboard or Details says so. The Dues chip reads "Reminders" either way; only Settings shows "Off". A user who never opens Reminders gets no warning before a due date.',
 rule:'DESIGN Dues list (as built); PRD F05 ("Reminders start off… no permission request on launch") for B or C',
 today:{what:'The chip says "Reminders" whether reminders are on or off.',draw:()=>phone(dues2())},
 options:[
 {key:'A',name:'State on the chip',rec:true,what:'The chip reads "Reminders off" with a bell-off icon and an amber outline, or "Reminders on · 09:00". Tapping it still opens Reminders. Nothing pops up.',
  pros:['Visible every time the app opens','No prompt and no permission request','Smallest change'],cons:['Easy to ignore'],
  draw:()=>pair(phone(dues2(chip2('off'))),phone(dues2(chip2('on'))),'Reminders off','Reminders on')},
 {key:'B',name:'Chip state plus a one-time card',what:'As A, and after the first unpaid due is saved, Dues shows one card: "Get a reminder before dues are due?" with Set up reminders and Not now. Not now hides it for good.',
  pros:['Asks at the moment reminders become useful','Explains why they matter'],cons:['A card above the dues once','Needs a stored dismissal and a PRD review'],
  draw:()=>phone(brand+heading('Your dues.','Date order · all active saved dues · amounts in PHP')+chip2('off')+`<div class="m-card m-hint"><b>Get a reminder before dues are due?</b><small>Reminders are off. Android asks for permission only when you turn them on.</small><span class="m-actions">${btn('Set up reminders','primary')}${btn('Not now','quiet')}</span></div>`+rows2())},
 {key:'C',name:'Turn on with the first due',what:'Saving the first due turns reminders on and Android asks for notification permission right then.',
  pros:['Reminders work without a visit to Settings'],cons:['Reverses "Reminders start off" in the PRD','A permission prompt the user did not ask for'],
  draw:()=>phone(dues2(chip2('on')),{overlay:dialog('Allow Duewise to send you notifications?','',['Don\'t allow','Allow'])})}
 ]},
{id:'UX18',rank:2,wave:6,title:'Overdue amount on Dashboard',score:[2,3,2],effort:'S',
 finding:'The Dashboard\'s large peso figure covers known unpaid amounts from today to month end only. With ₱12,850.00 overdue it shows "₱1,699.00", which reads as what is owed. The caption under the counters explains it in small text.',
 rule:'PRD F02 summary rules; DESIGN dashboard (as built)',
 today:{what:'The big figure leaves out the ₱12,850.00 already overdue.',draw:()=>phone(dash2(),{active:'dashboard'})},
 options:[
 {key:'A',name:'Separate overdue line',rec:true,what:'The upcoming figure stays. Directly under it, "! ₱12,850.00 overdue · 2 dues" appears whenever overdue dues have amounts; tapping it shows the overdue dues.',
  pros:['Nothing owed is hidden','The upcoming total keeps its meaning','One tap to the overdue dues'],cons:['Two amounts to read'],
  draw:()=>phone(dash2({extra:overdueLine}),{active:'dashboard'})},
 {key:'B',name:'One figure, overdue included',what:'The big figure becomes everything unpaid through 30 Nov, overdue included, with "₱12,850.00 of it overdue" below.',
  pros:['The big number is what you owe now'],cons:['Changes the F02 total rule','The month figure mixes old and new bills'],
  draw:()=>phone(dash2({label:'Unpaid · to 30 Nov',total:'₱14,549.00',meta:'4 unpaid · overdue included',extra:'<small class="m-red">₱12,850.00 of it overdue</small>'}),{active:'dashboard'})},
 {key:'C',name:'Clearer label only',what:'The figure stays; its label reads "Due today to 30 Nov · overdue not included".',
  pros:['Smallest change'],cons:['Still reads as what you owe at a glance'],
  draw:()=>phone(dash2({label:'Due today to 30 Nov · overdue not included'}),{active:'dashboard'})}
 ]},
{id:'UX19',rank:3,wave:6,title:'Reminders page by state',score:[3,1,3],effort:'M',
 finding:'Reminders is a long form ending in seven stacked buttons. Two are primary ("Save schedule", "Save and enable reminders"), turning reminders on is near the bottom, and Disable, Test and Refresh show even while reminders are off. Status lines read technically.',
 rule:'DESIGN F05 reminder adaptation and notification settings; principle 2',
 today:{what:'The schedule comes first; "Save and enable reminders" is the second of seven buttons below it.',
  draw:()=>pair(phone(remStatusNow+remSchedule(),{push:'Reminders'}),phone(caption('Save applies this schedule. Save and enable asks Android for permission. Notification text hides titles, amounts and notes.')+btn('Save schedule','primary')+btn('Save and enable reminders','primary')+btn('Disable reminders','secondary')+btn('Test notification','secondary')+btn('Android notification settings','secondary')+btn('Android app / battery settings','secondary')+btn('Refresh / Retry','secondary'),{push:'Reminders'}),'Top of the page','After scrolling')},
 options:[
 {key:'A',name:'Layout by state',rec:true,what:'Off: one card says reminders are off, sums up the schedule and offers "Turn on reminders". On: the card shows the next reminder, the schedule has "Save schedule", then quiet Test and Turn off. Android settings move under "If reminders arrive late".',
  pros:['One primary action, at the top','Only actions that work right now','Plain status wording'],cons:['Two layouts to build and test'],
  draw:()=>pair(phone(`<div class="m-card m-state"><b>Reminders are off</b><small>On each due date at 09:00 · also the day before</small>${btn('Turn on reminders','primary')}</div><b class="m-sub">Change schedule</b>${toggleRow('Also one day before',true)}${field('Reminder time (Asia/Manila)','09:00')}<div class="m-two">${field('Quiet from','22:00')}${field('Until','08:00')}</div>`+remLate,{push:'Reminders'}),
   phone(`<div class="m-card m-state m-on"><b>Reminders on</b><small>Next reminder: 09 Nov, 09:00</small></div>${toggleRow('Also one day before',true)}${field('Reminder time (Asia/Manila)','09:00')}<div class="m-two">${field('Quiet from','22:00')}${field('Until','08:00')}</div>${btn('Save schedule','primary m-disabled')}${btn('Test notification','secondary')}${btn('Turn off reminders','quiet')}`+remLate,{push:'Reminders'}),'While off','While on')},
 {key:'B',name:'Two pages',what:'Reminders keeps the status, an on/off switch and Test; the schedule moves to its own page.',
  pros:['Each page is short'],cons:['One more step to change the time','The switch must still explain the permission request'],
  draw:()=>pair(phone(`<div class="m-card"><b>Reminders</b>${toggleRow('Reminders are off',false)}</div><div class="m-card m-linkrow"><b>Schedule</b><small>09:00 · day before · quiet 22:00–08:00 ›</small></div>${btn('Test notification','secondary')}`+remLate,{push:'Reminders'}),phone(remSchedule(false)+btn('Save schedule','primary'),{push:'Schedule'}),'Reminders','Schedule page')},
 {key:'C',name:'Same order, fewer buttons',what:'The page keeps today\'s order. "Save and enable reminders" becomes the only primary button, and Disable, Test and Refresh hide while reminders are off.',
  pros:['Smallest change'],cons:['Turning on is still at the bottom'],
  draw:()=>phone(remSchedule(false)+btn('Save schedule','secondary')+btn('Save and enable reminders','primary')+btn('Android notification settings','secondary')+btn('Android app / battery settings','secondary'),{push:'Reminders'})}
 ]},
{id:'UX20',rank:4,wave:7,title:'More dues per screen',score:[3,3,1],effort:'M',
 finding:'The start page shows few dues per screen. Brand, heading, caption and chip take about 28% of the screen at 100% text and about half at 200%; each row is about 88 dp tall because the badge sits under the amount.',
 rule:'DESIGN Dues list (as built) and ledger row',
 today:{what:'The header takes about 28% of the screen at 100% text and about half at 200%, where two and a half rows fit.',draw:()=>pair(phone(dues2()),phone(dues2(),{cls:'m-big'}),'100% text','200% text')},
 options:[
 {key:'A',name:'Compact header and rows',rec:true,what:'The chip moves onto the brand row and the caption goes (UX26). The badge moves to the start of the date line, so the amount stands alone and each row is two short lines.',
  pros:['More dues on the first screen','Status stays as icon and word'],cons:['The date line gets longer and can wrap at 200%'],
  draw:()=>pair(phone(compactDues(chip2())),phone(compactDues(chip2()),{cls:'m-big'}),'100% text','200% text')},
 {key:'B',name:'Compact rows only',what:'Rows as in A; the header stays as it is.',
  pros:['Header unchanged'],cons:['Fewer extra rows than A'],
  draw:()=>pair(phone(dues2(chip2(),fold2+rowsC())),phone(dues2(chip2(),fold2+rowsC()),{cls:'m-big'}),'100% text','200% text')},
 {key:'C',name:'Keep',what:'The roomy Focus rows and header stay.',pros:['No change'],cons:['Few dues per screen, worst at 200% text'],same:true,draw:()=>phone(dues2())}
 ]},
{id:'UX21',rank:5,wave:8,title:'Remember to export',score:[3,1,3],effort:'M',
 finding:'Export is manual and silent: there is no "last exported" date and no suggestion after many changes. A user who never exports is still one lost phone from losing every due.',
 rule:'PRD UX05 (for A and C); SDD preferences; DESIGN Settings and dashboard',
 today:{what:'Your data offers Export and Import; nothing says when a copy was last saved.',draw:()=>phone(settings2(),{active:'settings'})},
 options:[
 {key:'A',name:'Date line plus a monthly suggestion',rec:true,what:'Your data shows "Last exported 05 Oct 2026". When dues changed and the last copy is over 30 days old, Dashboard shows one line, "Save a copy of your dues · Export", which can be closed for 30 days.',
  pros:['Helps people who never think of exporting','At most one quiet line a month'],cons:['One more line on Dashboard now and then','Needs a PRD UX05 change'],
  draw:()=>pair(phone(settings2({data:'<small class="m-strong">Last exported 05 Oct 2026</small>'}),{active:'settings'}),phone(dash2({},{top:'<div class="m-suggest"><span>Save a copy of your dues · <u>Export</u></span><i>✕</i></div>'}),{active:'dashboard'}),'Settings','Dashboard, 33 days later')},
 {key:'B',name:'Date line only',what:'Your data shows the last export date; nothing else changes.',
  pros:['No nudges','No PRD change'],cons:['Only seen by people who open Settings'],
  draw:()=>phone(settings2({data:'<small class="m-strong">Last exported 05 Oct 2026</small>'}),{active:'settings'})},
 {key:'C',name:'Automatic weekly copy',what:'Pick a folder once (Drive, for example); Duewise writes a fresh copy there every week while it runs.',
  pros:['Nothing to remember'],cons:['Copies pile up or overwrite each other','A folder on the same phone is lost with it','Background file work to build and test'],
  draw:()=>phone(settings2({data:`${toggleRow('Weekly copy to a folder',true)}<small>Drive › Duewise · last copy 04 Nov 2026</small>`}),{active:'settings'})}
 ]},
{id:'UX22',rank:6,wave:8,title:'Ask before discarding',score:[3,1,2],effort:'M',
 finding:'Back or Cancel in the editor discards a typed due without asking.',
 rule:'PRD workflow "Back/Cancel" ("An unsaved-changes prompt is not part of approved v1 behavior"); DESIGN dialogs',
 today:{what:'Cancel or Back leaves at once; the typed due is gone.',draw:()=>pair(phone(editor2(),{push:'Add due',footer:sticky('Cancel'+tap(1),'Save due')}),phone(dues2()),'Typed a due, then Cancel','Back on Dues: nothing saved, nothing asked')},
 options:[
 {key:'A',name:'Ask only when something changed',rec:true,what:'With changes, Back, the arrow and Cancel ask "Discard changes?" with Keep editing (default) and Discard. Without changes, nothing is asked.',
  pros:['No lost typing','Matches the approved Essential-first HTML editor'],cons:['One more tap when you do mean to discard'],
  draw:()=>phone(editor2(),{push:'Add due',footer:sticky('Cancel','Save due'),overlay:dialog('Discard changes?','The due you typed will not be saved.',['Discard','Keep editing'])})},
 {key:'B',name:'Keep a draft',what:'Leaving keeps what you typed; the next Add offers "Continue your draft?".',
  pros:['Nothing lost, no question asked'],cons:['A stored draft to manage','Surprising if you meant to throw it away'],
  draw:()=>phone(`<div class="m-card m-hint"><b>Continue your draft?</b><small>Internet · ₱1,699.00 · 09 Nov 2026</small><span class="m-actions">${btn('Continue','primary')}${btn('Start over','quiet')}</span></div>`+caption('Title, type, amount and date first. Everything else is optional.')+field('What is due?','For example, electricity',{placeholder:true})+field('Type','Bill ▾'),{push:'Add due',footer:sticky('Cancel','Save due')})},
 {key:'C',name:'Keep as is',what:'Back and Cancel discard without asking.',pros:['No change'],cons:['A stray Back loses the typing'],same:true,draw:()=>phone(editor2(),{push:'Add due',footer:sticky('Cancel','Save due')})}
 ]},
{id:'UX23',rank:7,wave:7,title:'How late an overdue due is',score:[2,2,1],effort:'S',
 finding:'An overdue due says "Overdue" but not how late it is, in the list, on Details and on Dashboard.',
 rule:'DESIGN status badges and ledger row; PRD F02 status text',
 today:{what:'"Overdue" for 2 days and for 4 days looks the same.',draw:()=>pair(phone(dues2()),phone(details2(),{push:'Due details'}),'Dues','Details')},
 options:[
 {key:'A',name:'Days late beside the badge',rec:true,what:'Rows add it to the date line ("03 Nov · Bill · 4 days late"); Details shows "4 days late" next to the badge. The badge keeps the word Overdue.',
  pros:['The badge stays the same everywhere','TalkBack reads it after the status'],cons:['Longer date line'],
  draw:()=>pair(phone(dues2(chip2(),fold2+rows2({lateMeta:true}))),phone(details2({late:'4 days late'}),{push:'Due details'}),'Dues','Details')},
 {key:'B',name:'The badge says it',what:'The Overdue badge reads "! 4 days late".',
  pros:['Short rows'],cons:['The word Overdue leaves the rows while the filter still says Overdue','Badges change width'],
  draw:()=>pair(phone(dues2(chip2(),fold2+rows2({lateBadge:true}))),phone(details2({label:'4 days late'}),{push:'Due details'}),'Dues','Details')},
 {key:'C',name:'Keep',what:'"Overdue" only.',pros:['No change'],cons:['Late fees are easier to miss'],same:true,draw:()=>phone(dues2())}
 ]},
{id:'UX24',rank:8,wave:9,title:'Calendar counts',score:[2,2,1],effort:'S',
 finding:'Calendar day counts are bare digits under the day number, which read like part of the date; the explanation is a caption above the grid.',
 rule:'DESIGN calendar grid (day cell)',
 today:{what:'Day 3 shows a small "2" under the "3", which reads like part of the date.',draw:()=>phone(calendar2('digits'),{active:'calendar'})},
 options:[
 {key:'A',name:'Corner count badge',rec:true,what:'The count sits in a small lime badge in the cell\'s top-right corner, visible on the selected day too. TalkBack still says "2 saved dues".',
  pros:['Count kept, clearly not part of the date','A familiar pattern'],cons:['A little busier'],draw:()=>phone(calendar2('badge'),{active:'calendar'})},
 {key:'B',name:'Dots',what:'Up to three dots under the day number.',pros:['Calm and familiar'],cons:['Four or more look the same as three'],draw:()=>phone(calendar2('dots'),{active:'calendar'})},
 {key:'C',name:'Keep digits, clearer caption',what:'Digits stay; the caption explains them.',pros:['Smallest change'],cons:['Captions are easy to skip'],draw:()=>phone(calendar2('digits',{cap:'Small numbers under a day count its dues.'}),{active:'calendar'})}
 ]},
{id:'UX25',rank:9,wave:8,title:'Settings apply on tap',score:[2,1,2],effort:'M',
 finding:'Appearance and Start of week change nothing until "Save preferences" further down the page; tapping "Dark" gives no visible result.',
 rule:'PRD F04 ("Changes are drafts until explicit Save… Back discards unsaved choices"); DESIGN Settings (as built)',
 today:{what:'Dark is selected, but the app stays light until Save, which is below the fold.',draw:()=>phone(settings2({appearance:'Dark'}),{active:'settings'})},
 options:[
 {key:'A',name:'Apply on tap',rec:true,what:'Tapping Dark switches the app at once and saves it; the Save button goes. If saving fails, the choice returns and an error shows. Reminders keeps its own Save.',
  pros:['What Android settings do','Instant, visible result'],cons:['Changes the PRD F04 draft rule'],
  draw:()=>phone(settings2({appearance:'Dark',hint:'System follows your device.',save:false}),{active:'settings',cls:'m-dark'})},
 {key:'B',name:'Preview, then Save',what:'The theme previews at once; "Not saved yet" and Save stay pinned at the bottom until you save.',
  pros:['Keeps the explicit Save'],cons:['Two steps for one choice','Back still discards'],
  draw:()=>phone(settings2({appearance:'Dark',hint:'Preview. Save to keep it.',save:false}),{active:'settings',cls:'m-dark',footer:`<div class="m-footer m-unsaved"><small>Not saved yet</small>${btn('Save preferences','primary')}</div>`})},
 {key:'C',name:'Keep',what:'Choices wait for Save.',pros:['No change'],cons:['Tapping Dark seems to do nothing'],same:true,draw:()=>phone(settings2({appearance:'Dark'}),{active:'settings'})}
 ]},
{id:'UX26',rank:10,wave:7,title:'Plainer, fewer captions',score:[3,1,1],effort:'S',
 finding:'Captions that read like developer notes repeat across pages and push content down. The amount field has three helper lines plus a preview, and Details repeats "Back to list" under the app bar\'s back arrow.',
 rule:'DESIGN voice and page anatomy; PRD F01 ("Edit and Back to dues")',
 today:{what:'Details opens with a facts caption and ends with Back to list; the amount field has three helper lines.',draw:()=>pair(phone(details2(),{push:'Due details'}),phone(editor2(),{push:'Edit due',footer:sticky('Cancel','Save changes')}),'Details','Editor')},
 options:[
 {key:'A',name:'Plainer, fewer captions',rec:true,what:'PHP and Asia/Manila are stated once, in Settings. Dues and Details lose their fact captions, the amount keeps one hint plus the preview, and Details drops Back to list.',
  pros:['Content moves up','Each caption helps the next step'],cons:['Some wording to agree (see the plan\'s rewrite table)'],
  draw:()=>pair(phone(details2({cap:false,actions:btn('Mark as paid','primary')+btn('Edit due','secondary')}),{push:'Due details'}),phone(editor2({hints:['Required for Bill.','₱1,699.00'],typeHint:''}),{push:'Edit due',footer:sticky('Cancel','Save changes')}),'Details','Editor')},
 {key:'B',name:'No captions at all',what:'Every caption and helper line goes except errors; the amount preview stays.',
  pros:['The cleanest pages'],cons:['Loses "Optional for Task" and the total\'s scope','Errors carry all the guidance'],
  draw:()=>pair(phone(details2({cap:false,actions:btn('Mark as paid','primary')+btn('Edit due','secondary')}),{push:'Due details'}),phone(editor2({cap:false,hints:['₱1,699.00'],typeHint:''}),{push:'Edit due',footer:sticky('Cancel','Save changes')}),'Details','Editor')},
 {key:'C',name:'Keep',what:'Captions stay as they are.',pros:['No change'],cons:['Less room for dues'],same:true,draw:()=>phone(details2(),{push:'Due details'})}
 ]},
{id:'UX27',rank:11,wave:9,title:'Where Delete lives',score:[2,1,1],effort:'—',
 finding:'Delete is reachable only through Details → Edit due → the "Delete" text in the editor\'s app bar.',
 rule:'None for A; V1.3 soft-delete spec and PRD F01 for B or C',
 today:{what:'Details → Edit due → Delete in the app bar; Undo follows for 5 seconds.',draw:()=>pair(phone(details2({actions:btn('Mark as paid','primary')+btn('Edit due'+tap(2),'secondary')+btn('Back to list','secondary')}),{push:'Due details'}),phone(editor2(),{push:'Edit due',action:'Delete'+tap(3),footer:sticky('Cancel','Save changes')}),'Details (tap 1 opened it)','Editor')},
 options:[
 {key:'A',name:'Keep as is',rec:true,what:'Delete stays in the editor; Undo covers mistakes and Details keeps one primary action.',
  pros:['No new destructive button','No work'],cons:['Three taps to delete'],same:true,draw:()=>phone(editor2(),{push:'Edit due',action:'Delete',footer:sticky('Cancel','Save changes')})},
 {key:'B',name:'Delete on Details',what:'A quiet red "Delete due" at the bottom of Details, with the same confirmation and Undo.',
  pros:['Two taps to delete','Easy to find'],cons:['A destructive control near Mark as paid'],
  draw:()=>phone(details2({actions:btn('Mark as paid','primary')+btn('Edit due','secondary')+btn('Back to list','secondary')+'<span class="m-danger-link">Delete due</span>'}),{push:'Due details'})},
 {key:'C',name:'Long-press a row',what:'Holding a row in a list opens a menu with Delete due.',
  pros:['Fast for people who know it'],cons:['Hidden: nothing shows the gesture','Needs a separate TalkBack action'],
  draw:()=>phone(dues2(),{overlay:'<div class="m-dim"><div class="m-sheet"><b>Rent</b><i class="m-danger">Delete due</i><i>Cancel</i></div></div>'})}
 ]}
];
const WAVES2={6:'Wave 6 · never miss a payment',7:'Wave 7 · more dues, plainer words',8:'Wave 8 · protect data and typing',9:'Wave 9 · small'};
const impact=d=>d.score[0]*d.score[1]*d.score[2];

/* ---- Rounds. Round 2 is open and shown first; round 1 is kept as the decided record. ---- */
const ROUNDS=[
 {id:'r2',key:'duewise.ux-decisions.r2.v1',label:'Round 2 · 7 Nov',status:'open',decisions:D2,waves:WAVES2,groupWaves:false,
  eyebrow:'UX DECISIONS / FROM THE 7 NOV 2026 REVIEW',heading:'Eleven choices. Highest impact first.',
  intro:'Each decision shows <strong>today\'s Android app</strong>, as built after Waves 1–5, next to the proposed options, drawn with the app\'s real colours and layout. Choose one option per decision; the recommended one is marked.',
  review:'../../docs/delivery/UX-REVIEW-2026-11-07.md',spec:'../../specs/ux-improvements-2/spec.md',plan:'../../specs/ux-improvements-2/plan.md',
  title:'# Duewise UX decisions, round 2',source:'Source: docs/delivery/UX-REVIEW-2026-11-07.md and specs/ux-improvements-2/spec.md. These are preferences until confirmed and recorded in the PRD/DESIGN.'},
 {id:'r1',key:'duewise.ux-decisions.v1',label:'Round 1 · 6 Nov',status:'decided',decisions:D1,waves:WAVES1,groupWaves:true,decided:DECIDED1,
  eyebrow:'UX DECISIONS / FROM THE 6 NOV 2026 REVIEW',heading:'Sixteen choices. Decided and shipped.',
  intro:'Decided on 6 Nov 2026 and shipped in Waves 1–5. Kept as a record: <strong>"Today" shows the app before those changes</strong>, and the option marked Decided is what the app does now.',
  note:'Decided 6 Nov 2026 and shipped. "Today" shows the app before Waves 1–5.',
  review:'../../docs/delivery/UX-REVIEW-2026-11-06.md',spec:'../../specs/ux-improvements/spec.md',plan:'../../specs/ux-improvements/plan.md',
  title:'# Duewise UX decisions',source:'Source: docs/delivery/UX-REVIEW-2026-11-06.md and specs/ux-improvements/spec.md. These are preferences until confirmed and recorded in the PRD/DESIGN.'}
];
const roundOf=id=>ROUNDS.find(r=>r.decisions.some(d=>d.id===id));

/* ---- State: one browser-local preference store per round (round 1 keeps its original key). ---- */
let active='r2';
const states=Object.fromEntries(ROUNDS.map(r=>[r.id,{current:r.decisions[0].id,choices:{},notes:{}}]));
const R=()=>ROUNDS.find(r=>r.id===active);
const S=()=>states[active];
function load(){for(const r of ROUNDS){try{const s=JSON.parse(localStorage.getItem(r.key)||'null');if(s&&typeof s==='object')states[r.id]={current:r.decisions.some(d=>d.id===s.current)?s.current:r.decisions[0].id,choices:s.choices||{},notes:s.notes||{}};}catch{/* storage unavailable: work in memory */}}}
function save(){try{localStorage.setItem(R().key,JSON.stringify(S()));return true;}catch{return false;}}
const $=s=>document.querySelector(s);
const byId=id=>R().decisions.find(d=>d.id===id)||R().decisions[0];

function renderHead(){
  const r=R();
  $('#dl-eyebrow').textContent=r.eyebrow;$('#decision-heading').textContent=r.heading;$('#dl-intro').innerHTML=r.intro;
  $('#dl-sources').innerHTML=`<a href="${r.review}">UX review</a>, <a href="${r.spec}">options</a>, <a href="${r.plan}">plan</a>`;
  $('#dl-rounds').innerHTML=ROUNDS.map(x=>`<button type="button" data-dl-round="${x.id}" aria-pressed="${x.id===active}">${x.label}<span>${x.status==='open'?`${x.decisions.length} open`:'decided'}</span></button>`).join('');
}
function renderList(){
  const r=R(),st=S();let html='',wave=0;
  for(const d of r.decisions){
    if(r.groupWaves&&d.wave!==wave){wave=d.wave;html+=`<p class="dl-wave">${r.waves[wave]}</p>`;}
    const choice=st.choices[d.id];
    html+=`<button type="button" class="dl-item" data-dl-go="${d.id}" aria-current="${d.id===st.current?'true':'false'}"><span class="dl-rank">${d.rank}</span><span class="dl-item-text"><b>${esc(d.title)}</b><small>${d.id} · impact ${impact(d)} · effort ${d.effort}${r.groupWaves?'':` · wave ${d.wave}`}</small></span><span class="dl-chosen">${choice?`✓ ${choice}`:''}</span></button>`;
  }
  $('#dl-list').innerHTML=html;
  // Keep the current item visible when the list is a horizontal strip on narrow screens.
  // Only the strip scrolls sideways; the page itself is not moved.
  const list=$('#dl-list'),current=list?.querySelector?.('.dl-item[aria-current=true]');
  if(current&&list.scrollWidth>list.clientWidth)list.scrollLeft=current.offsetLeft-list.offsetLeft-8;
}
function optionCard(d,o){
  const chosen=S().choices[d.id]===o.key,decided=R().decided?.[d.id]===o.key;
  return `<article class="dl-card${chosen?' dl-picked':''}${o.rec?' dl-rec':''}"><header><span class="dl-key">${o.key}</span><h4>${esc(o.name)}</h4>${decided?'<span class="dl-badge dl-decided">Decided</span>':''}${o.rec?'<span class="dl-badge">Recommended</span>':''}${o.same?'<span class="dl-badge dl-same">Same as today</span>':''}</header>
  <div class="dl-figure">${o.draw()}</div><p class="dl-what">${esc(o.what)}</p>
  <ul class="dl-pc">${(o.pros||[]).map(p=>`<li class="pro">${esc(p)}</li>`).join('')}${(o.cons||[]).map(c=>`<li class="con">${esc(c)}</li>`).join('')}</ul>
  <button type="button" class="${chosen?'studio-primary':'studio-secondary'} dl-pick" data-dl-pick="${d.id}:${o.key}" aria-pressed="${chosen}">${chosen?`Chosen: ${o.key}`:`Choose ${o.key}`}</button></article>`;
}
function renderMain(){
  const r=R(),D=r.decisions,d=byId(S().current),i=D.indexOf(d),[rr,f,s]=d.score;
  $('#dl-main').innerHTML=`<div class="dl-decision-head"><p class="eyebrow">${d.id} · RANK ${d.rank} OF ${D.length} · ${r.waves[d.wave].toUpperCase()}</p><h3 id="dl-title" tabindex="-1">${esc(d.title)}</h3>
  <p class="dl-finding">${esc(d.finding)}</p>
  <p class="dl-meta"><span>Impact <b>${impact(d)}</b> = reach ${rr} × frequency ${f} × severity ${s}</span><span>Effort <b>${d.effort}</b></span><span>Rule affected: ${esc(d.rule)}</span></p>${r.note?`<p class="dl-round-note">${esc(r.note)}</p>`:''}</div>
  <div class="dl-cards"><article class="dl-card dl-today"><header><span class="dl-key">Now</span><h4>Today in the app</h4></header><div class="dl-figure">${d.today.draw()}</div><p class="dl-what">${esc(d.today.what)}</p></article>${d.options.map(o=>optionCard(d,o)).join('')}</div>
  <label class="dl-note">Your note for ${d.id} (optional)<textarea id="dl-note" rows="2" placeholder="Anything to adjust in the option you chose">${esc(S().notes[d.id]||'')}</textarea></label>
  <div class="dl-step"><button type="button" class="studio-secondary" data-dl-go="${D[i-1]?.id||''}" ${i===0?'disabled':''}>← Previous</button><button type="button" class="studio-secondary" data-dl-go="${D[i+1]?.id||''}" ${i===D.length-1?'disabled':''}>Next →</button></div>`;
  $('#dl-note').addEventListener('input',e=>{S().notes[d.id]=e.target.value;save();});
}
function renderProgress(){
  const D=R().decisions,n=Object.keys(S().choices).filter(k=>D.some(d=>d.id===k)).length;
  $('#dl-progress').textContent=`${n} of ${D.length} chosen in ${R().label.split(' · ')[0].toLowerCase()}. Choices stay in this browser until you export or copy them.`;
}
function render(id){
  const r=id&&roundOf(id);
  if(r){active=r.id;states[r.id].current=id;}
  renderHead();renderList();renderMain();renderProgress();
}
function markdown(roundId=active){
  const r=ROUNDS.find(x=>x.id===roundId)||R(),st=states[r.id];
  const date=new Intl.DateTimeFormat('en-PH',{dateStyle:'long',timeZone:'Asia/Manila'}).format(new Date());
  const lines=[r.title,'',`Exported ${date} from the HTML decision lab (prototypes/design-exploration/index.html#ux-decisions).`,r.source,'','| Rank | ID | Decision | Choice | Note |','|---|---|---|---|---|'];
  for(const d of r.decisions){const k=st.choices[d.id],o=d.options.find(x=>x.key===k);lines.push(`| ${d.rank} | ${d.id} | ${d.title} | ${o?`${k}: ${o.name}`:'Not chosen'} | ${(st.notes[d.id]||'').replace(/\|/g,'/').replace(/\n/g,' ')} |`);}
  return lines.join('\n');
}
function announce(text){const s=$('#dl-progress');s.textContent=text;setTimeout(renderProgress,2500);}
async function command(name){
  if(name==='export'){const url=URL.createObjectURL(new Blob([markdown()],{type:'text/markdown;charset=utf-8'})),a=document.createElement('a');a.href=url;a.download=active==='r1'?'duewise-ux-decisions.md':'duewise-ux-decisions-round2.md';document.body.appendChild(a);a.click();a.remove();setTimeout(()=>URL.revokeObjectURL(url),1000);announce('Your decisions file is downloading.');}
  if(name==='copy'){try{await navigator.clipboard.writeText(markdown());announce('Copied. Paste it into the chat with Claude.');}catch{const t=$('#dl-export-text');t.hidden=false;t.value=markdown();t.select();announce('Copy the selected text below.');}}
  if(name==='clear'){S().choices={};S().notes={};save();render();announce('Choices cleared for this round.');}
}
const go=id=>{history.replaceState(null,'',`#ux-decisions/${id}`);};
function init(){
  load();
  document.addEventListener('click',e=>{
    const b=e.target.closest('button');if(!b||!b.closest('#decision-view'))return;
    if(b.dataset.dlGo){S().current=b.dataset.dlGo;save();render();go(S().current);$('#dl-title')?.focus?.();document.getElementById('dl-main').scrollIntoView({block:'start',behavior:'smooth'});}
    else if(b.dataset.dlRound){active=b.dataset.dlRound;render();go(S().current);$('#decision-heading')?.focus?.();}
    else if(b.dataset.dlPick){const[id,key]=b.dataset.dlPick.split(':'),st=S();st.choices[id]===key?delete st.choices[id]:st.choices[id]=key;const ok=save();render();announce(ok?(st.choices[id]?`${id}: option ${key} chosen.`:`${id}: choice removed.`):'Chosen for this visit only: browser storage is unavailable.');}
    else if(b.dataset.dlCmd){command(b.dataset.dlCmd);}
  });
}
window.DecisionLab={init,render,hash:()=>`#ux-decisions/${S().current}`,rounds:ROUNDS,get decisions(){return R().decisions;},get round(){return active;},impact,markdown};
init();
})();
