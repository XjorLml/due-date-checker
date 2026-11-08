/* Notification lab (specs/notification-lab/spec.md): what each due's reminder could say, and a
   "Mark as paid" action on the notification itself. A proposal drawn as Android notifications;
   nothing here changes the app. Today's app sends one generic text (PRD F05 privacy rule). */
(function(){
'use strict';
const KEY='duewise.notification-lab.v1';
const esc=s=>String(s).replace(/[&<>"]/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]));

/* Sample dues: the emulator's data around Saturday 07 Nov 2026, reminders at 09:00 Manila. */
const DUES=[
  {id:'rent',title:'Rent',type:'Bill',amount:'₱12,000.00',date:'2026-11-05',repeat:'Monthly',next:'05 Dec 2026'},
  {id:'internet',title:'Internet',type:'Bill',amount:'₱1,699.00',date:'2026-11-09',repeat:'Monthly',next:'09 Dec 2026'},
  {id:'passport',title:'Renew passport',type:'Task',amount:null,date:'2026-11-07',repeat:null},
  {id:'spotify',title:'Spotify',type:'Subscription',amount:'₱149.00',date:'2026-11-12',repeat:'Monthly',next:'12 Dec 2026'},
  {id:'water',title:'Water bill',type:'Bill',amount:'₱850.00',date:'2026-11-15',repeat:null},
  {id:'tuition',title:'Tuition',type:'Bill',amount:'₱8,500.00',date:'2026-11-15',repeat:null},
];
const GROUP={id:'group',title:'Two on one day',members:['water','tuition'],date:'2026-11-15'};
const DAYS=['Sun','Mon','Tue','Wed','Thu','Fri','Sat'],MONTHS=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
const parse=iso=>{const[y,m,d]=iso.split('-').map(Number);return new Date(Date.UTC(y,m-1,d));};
const shortDate=iso=>{const t=parse(iso);return `${DAYS[t.getUTCDay()]} ${String(t.getUTCDate()).padStart(2,'0')} ${MONTHS[t.getUTCMonth()]}`;};
const byId=id=>DUES.find(d=>d.id===id);
const LONG_DAYS=['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'],LONG_MONTHS=['January','February','March','April','May','June','July','August','September','October','November','December'];
/* The day the reminder arrives: the due date, or the day before it. */
const reminderDay=(iso,when)=>{const t=parse(iso);if(when==='before')t.setUTCDate(t.getUTCDate()-1);return t;};

/* Privacy modes, matching decision N1's options. */
const MODES={generic:"Today's app",unlocked:'Details when unlocked',everywhere:'Details everywhere'};
const WHEN={before:'tomorrow',day:'today'};

/* What one reminder shows. surface: 'shade' (unlocked, pulled down) or 'lock' (lock screen).
   Returns the title, text, actions ('pay' | 'undo') and an optional note. Pure, so tests can check it. */
function content(due,when,privacy,surface,paid=false){
  if(privacy==='generic'){
    return surface==='lock'
      ? {title:'Duewise',text:'Contents hidden',actions:[],hidden:true,note:'Android hides the text of a private notification on the lock screen.'}
      : {title:'Duewise reminder',text:'Open Duewise to review an unpaid due date.',actions:[],note:'Tap opens this due in Duewise. No button: the text does not say which due it is.'};
  }
  if(surface==='lock'&&privacy==='unlocked')
    return {title:'Duewise',text:`You have a due ${WHEN[when]}`,actions:[],hidden:true,note:'Lock-screen version: no title or amount until the phone is unlocked.'};
  if(paid) return {title:`${due.title} marked paid`,text:due.repeat?`Next one added for ${due.next}.`:'Saved in Duewise.',actions:['undo'],note:'Closes by itself after 10 seconds.'};
  const parts=[due.amount??`${due.type}, no amount`,shortDate(due.date)];
  if(due.repeat)parts.push(`repeats ${due.repeat.toLowerCase()}`);
  return {title:`${due.title} is due ${WHEN[when]}`,text:parts.join(' · '),actions:['pay'],
    note:surface==='lock'?'Mark as paid asks for the PIN or fingerprint first.':''};
}

/* Decisions the user can make here (exported for the chat, like the UX decision lab). */
const DECISIONS=[
  {id:'N1',title:'What a reminder says',rule:'PRD F05: "Notification text is generic and contains no due title, amount or notes."',options:[
    {key:'A',name:'Details when unlocked',rec:true,mode:'unlocked',what:'The notification names the due, its amount and date. On the lock screen it only says "You have a due today" until the phone is unlocked.',
      pros:['You know which bill it is at a glance','Nothing private shows on a locked phone','Makes a Mark as paid button safe'],cons:['Someone using your unlocked phone can read it','Changes the PRD privacy rule']},
    {key:'B',name:"Keep today's generic text",mode:'generic',what:'"Duewise reminder · Open Duewise to review an unpaid due date." for every due.',
      pros:['Most private','No rule change'],cons:['You must open the app to know which due','Two reminders at 09:00 look identical','No safe Mark as paid button']},
    {key:'C',name:'Details everywhere',mode:'everywhere',what:'Title, amount and date also show on the lock screen.',
      pros:['Quickest to read'],cons:['Amounts visible to anyone who picks up the phone']},
  ]},
  {id:'N2',title:'Paying from a reminder',rule:'PRD F05: "A reminder tap … opens the corresponding active unpaid due through F01 Details."',options:[
    {key:'A',name:'Mark as paid button',rec:true,what:'One button on the notification saves the due as paid without opening Duewise. The notification then says "Rent marked paid" with Undo for 10 seconds; a repeating due adds its next one. Needs the phone unlocked.',
      pros:['Pay and forget in one tap','Undo covers a slip','Same rules as Details: next occurrence, reminders cancelled'],cons:['A tap from the notification shade can be accidental (Undo helps)','Needs N1 A or C so you know which due it is']},
    {key:'B',name:'Button opens Details',what:'The button opens Duewise on that due with "Mark as paid" ready to tap.',
      pros:['You see the due before paying'],cons:['Two taps and an app launch']},
    {key:'C',name:'No button (today)',what:'Tapping the notification opens the due in Duewise; paying happens there.',
      pros:['No change'],cons:['Always opens the app']},
  ]},
];

let state={due:'rent',when:'day',privacy:'unlocked',surface:'shade',paid:{},unlocking:null,opened:null,choices:{},notes:{}};
function load(){try{const s=JSON.parse(localStorage.getItem(KEY)||'null');if(s&&typeof s==='object'){state.choices=s.choices||{};state.notes=s.notes||{};}}catch{/* storage unavailable: work in memory */}}
function save(){try{localStorage.setItem(KEY,JSON.stringify({choices:state.choices,notes:state.notes}));return true;}catch{return false;}}
const $=s=>document.querySelector(s);
/* N2 drives what the preview's button does: A pays, B opens Details, C hides the button. */
const action=()=>state.choices.N2||'A';

function cardHtml(due,c,{grouped=false}={}){
  const act=state.privacy==='generic'||action()==='C'?[]:c.actions;
  const buttons=act.map(a=>a==='pay'
    ? `<button type="button" class="nl-action" data-nl-pay="${due.id}" aria-label="Mark ${esc(due.title)} as paid">Mark as paid</button>`
    : `<button type="button" class="nl-action" data-nl-undo="${due.id}" aria-label="Undo: mark ${esc(due.title)} as unpaid">Undo</button>`).join('');
  return `<article class="nl-card${grouped?' nl-child':''}${c.hidden?' nl-hidden':''}" aria-label="Notification: ${esc(c.title)}. ${esc(c.text)}">
    ${grouped?'':`<div class="nl-card-top"><span class="nl-app" aria-hidden="true">✓</span><small>Duewise · 9:00</small><span class="nl-chev" aria-hidden="true">⌄</span></div>`}
    <b class="nl-title">${esc(c.title)}</b><p class="nl-text">${esc(c.text)}</p>${buttons?`<div class="nl-actions">${buttons}</div>`:''}</article>`;
}

function phoneHtml(){
  const lock=state.surface==='lock',sel=state.due;
  const day=reminderDay(sel==='group'?GROUP.date:byId(sel).date,state.when);
  const shortDay=`${DAYS[day.getUTCDay()]}, ${day.getUTCDate()} ${MONTHS[day.getUTCMonth()]}`,longDay=`${LONG_DAYS[day.getUTCDay()]}, ${day.getUTCDate()} ${LONG_MONTHS[day.getUTCMonth()]}`;
  let cards='';
  if(sel==='group'){
    const members=GROUP.members.map(byId);
    if(state.privacy==='generic'){cards=members.map(d=>cardHtml(d,content(d,state.when,'generic',state.surface))).join('');}
    else if(lock&&state.privacy==='unlocked'){cards=cardHtml(members[0],{title:'Duewise',text:`You have 2 dues ${WHEN[state.when]}`,actions:[],hidden:true});}
    else{
      cards=`<div class="nl-group" role="group" aria-label="2 Duewise reminders"><div class="nl-card-top"><span class="nl-app" aria-hidden="true">✓</span><small>Duewise · 2 dues ${WHEN[state.when]}</small><span class="nl-chev" aria-hidden="true">⌃</span></div>`+
        members.map(d=>cardHtml(d,content(d,state.when,state.privacy,state.surface,!!state.paid[d.id]),{grouped:true})).join('')+'</div>';
    }
  }else{
    const d=byId(sel);cards=cardHtml(d,content(d,state.when,state.privacy,state.surface,!!state.paid[d.id]));
  }
  const top=lock
    ? `<div class="nl-clock" aria-hidden="true">9:00</div><div class="nl-date" aria-hidden="true">${longDay}</div>`
    : `<div class="nl-shade-head" aria-hidden="true"><b>9:00</b><span>${shortDay}</span></div><div class="nl-tiles" aria-hidden="true"><i>Wi-Fi</i><i>Mobile data</i><i>Silent</i><i>Battery saver</i></div>`;
  const unlock=state.unlocking?`<div class="nl-overlay" role="dialog" aria-label="Unlock to continue"><div class="nl-unlock"><span aria-hidden="true">🔒</span><b>Unlock to mark as paid</b><small>Android asks for your PIN or fingerprint first.</small><div class="nl-actions"><button type="button" class="nl-action" data-nl-cmd="cancel-unlock">Cancel</button><button type="button" class="nl-action nl-strong" data-nl-cmd="unlock">Unlock (simulated)</button></div></div></div>`:'';
  const opened=state.opened?`<div class="nl-toast" role="status">Opens Duewise at ${esc(byId(state.opened).title)}, with Mark as paid ready.</div>`:'';
  return `<div class="nl-phone ${lock?'nl-lock':'nl-shade'}" role="region" aria-label="${lock?'Lock screen':'Notification shade'} preview"><div class="nl-status" aria-hidden="true"><span>9:00</span><span>▾ ◢ ▮</span></div>${top}<div class="nl-cards">${cards}</div>${lock?'<div class="nl-lockhint" aria-hidden="true">Swipe up to unlock</div>':'<div class="nl-manage" aria-hidden="true"><span>Manage</span><span>Clear all</span></div>'}${unlock}${opened}</div>`;
}

function explainHtml(){
  const d=state.due==='group'?null:byId(state.due),name=d?d.title:'each due';
  const items=[];
  items.push(`<li><b>Tap the notification:</b> opens ${esc(name)} in Duewise, as today.</li>`);
  if(state.privacy==='generic')items.push('<li><b>No button:</b> the text does not say which due it is, so a button could pay the wrong one.</li>');
  else if(action()==='A')items.push(`<li><b>Mark as paid:</b> saves ${esc(name)} as paid without opening Duewise. It is the same save as on Details: a repeating due adds its next one and its other reminders are cancelled. The notification then shows Undo for 10 seconds.</li>`,
    '<li><b>If something changed:</b> a due already paid, edited or deleted since the reminder is not touched; the notification says "Open Duewise to check this due."</li>',
    '<li><b>Locked phone:</b> Android asks for the PIN or fingerprint before the button runs (Android 12 and later).</li>');
  else if(action()==='B')items.push(`<li><b>Button:</b> opens ${esc(name)} in Duewise with Mark as paid ready; nothing is saved until you tap it there.</li>`);
  else items.push('<li><b>No button:</b> paying happens in Duewise after the tap.</li>');
  items.push('<li><b>When:</b> at your reminder time (09:00 here) on the due date, and the day before if that setting is on. Quiet hours move it; nothing is sent once the date has passed.</li>');
  items.push('<li><b>Not push:</b> Duewise schedules these on the phone itself; no server or internet is involved.</li>');
  return `<h3>What happens</h3><ul>${items.join('')}</ul>`;
}

const seg=(name,label,values,current)=>`<div class="nl-seg" role="group" aria-label="${label}"><span>${label}</span>${Object.entries(values).map(([k,v])=>`<button type="button" data-nl-set="${name}:${k}" aria-pressed="${current===k}">${esc(v)}${name==='privacy'&&k==='unlocked'?' <em>recommended</em>':''}</button>`).join('')}</div>`;

function tableHtml(){
  const mode=state.privacy==='generic'?'unlocked':state.privacy;
  const rows=DUES.map(d=>{const before=content(d,'before',mode,'shade'),day=content(d,'day',mode,'shade'),lock=content(d,'day',mode,'lock');
    return `<tr><th scope="row">${esc(d.title)}<small>${d.type} · ${d.amount??'no amount'}${d.repeat?` · ${d.repeat}`:''}</small></th><td><b>${esc(before.title)}</b><br>${esc(before.text)}</td><td><b>${esc(day.title)}</b><br>${esc(day.text)}</td><td>${esc(lock.title)} · ${esc(lock.text)}</td><td>${day.actions.includes('pay')?'Mark as paid':'—'}</td></tr>`;}).join('');
  return `<h3>Every due, both reminders <small>(${esc(MODES[mode])}${state.privacy==='generic'?'; today’s app shows the generic text for all of them':''})</small></h3>
  <div class="nl-table-scroll"><table><thead><tr><th scope="col">Due</th><th scope="col">Day before, 09:00</th><th scope="col">On the day, 09:00</th><th scope="col">Lock screen</th><th scope="col">Button</th></tr></thead><tbody>${rows}</tbody></table></div>
  <p class="nl-small">After Mark as paid: “Rent marked paid · Next one added for 05 Dec 2026.” or, for a due that does not repeat, “Water bill marked paid · Saved in Duewise.”, each with Undo.</p>`;
}

function decisionsHtml(){
  const warn=state.choices.N1==='B'&&state.choices.N2==='A'?'<p class="nl-warn" role="status">N1 B with N2 A: a button on a reminder that does not name the due could pay the wrong one.</p>':'';
  return `<h3>Choose</h3><p class="nl-small">Choices stay in this browser until you copy or export them. Nothing changes in the app until a choice is confirmed and written into the PRD and DESIGN.</p>${warn}
  <div class="nl-decisions">${DECISIONS.map(dec=>`<section class="nl-decision" aria-labelledby="nl-${dec.id}"><p class="eyebrow">${dec.id}</p><h4 id="nl-${dec.id}">${esc(dec.title)}</h4><p class="nl-small">Rule affected: ${esc(dec.rule)}</p>
    ${dec.options.map(o=>{const chosen=state.choices[dec.id]===o.key;return `<article class="nl-option${chosen?' nl-picked':''}${o.rec?' nl-rec':''}"><header><span class="nl-key">${o.key}</span><b>${esc(o.name)}</b>${o.rec?'<span class="nl-badge">Recommended</span>':''}</header><p>${esc(o.what)}</p><ul>${o.pros.map(p=>`<li class="pro">${esc(p)}</li>`).join('')}${o.cons.map(c=>`<li class="con">${esc(c)}</li>`).join('')}</ul><button type="button" class="${chosen?'studio-primary':'studio-secondary'}" data-nl-pick="${dec.id}:${o.key}" aria-pressed="${chosen}">${chosen?`Chosen: ${o.key}`:`Choose ${o.key}`}</button></article>`;}).join('')}
    <label class="nl-note">Your note for ${dec.id} (optional)<textarea rows="2" data-nl-note="${dec.id}">${esc(state.notes[dec.id]||'')}</textarea></label></section>`).join('')}</div>`;
}

function render(id){
  if(id&&(id==='group'||byId(id)))state.due=id;
  const root=$('#nl-root');if(!root)return;
  const chosen=Object.keys(state.choices).length;
  root.innerHTML=`<div class="nl-heading"><div><p class="eyebrow">NOTIFICATIONS / A PROPOSAL, NOT IN THE APP YET</p><h2 id="notification-heading">What each reminder could say.</h2>
    <p>Pick a due to see its reminders the day before and on the day, on the notification shade and on the lock screen. <strong>Today's app</strong> shows one generic text for every due; the other two modes are proposals, with a <strong>Mark as paid</strong> button you can try.</p>
    <p>Sources: <a href="../../specs/notification-lab/spec.md">notification lab spec</a>, <a href="../../docs/PRD.md#f05-reminder-rules">PRD F05 reminder rules</a>.</p></div>
    <div class="nl-review"><p id="nl-progress" role="status">${chosen} of ${DECISIONS.length} chosen.</p><button type="button" class="studio-primary" data-nl-cmd="copy">Copy for Claude</button><button type="button" class="studio-secondary" data-nl-cmd="export">Export .md</button><button type="button" class="text-action" data-nl-cmd="clear">Clear choices</button><textarea id="nl-export-text" hidden aria-label="Exported decisions" readonly></textarea></div></div>
  <div class="nl-layout"><nav class="nl-dues" aria-label="Sample dues">${DUES.map(d=>`<button type="button" data-nl-due="${d.id}" aria-current="${state.due===d.id}"><b>${esc(d.title)}</b><small>${d.type} · ${d.amount??'no amount'} · ${shortDate(d.date)}${d.repeat?' · '+d.repeat:''}</small>${state.paid[d.id]?'<span class="nl-paid">✓ Paid</span>':''}</button>`).join('')}
    <button type="button" data-nl-due="group" aria-current="${state.due==='group'}"><b>${GROUP.title}</b><small>Water bill and Tuition · ${shortDate(GROUP.date)}</small></button></nav>
    <div class="nl-main"><div class="nl-controls">${seg('privacy','What it says',MODES,state.privacy)}${seg('surface','Screen',{shade:'Notification shade',lock:'Lock screen'},state.surface)}${seg('when','Moment',{before:'Day before',day:'On the day'},state.when)}</div>
      <div class="nl-stage">${phoneHtml()}<aside class="nl-explain">${explainHtml()}</aside></div></div></div>
  <section class="nl-table">${tableHtml()}</section>
  <section class="nl-choose">${decisionsHtml()}</section>`;
  root.querySelectorAll('[data-nl-note]').forEach(t=>t.addEventListener('input',e=>{state.notes[e.target.dataset.nlNote]=e.target.value;save();}));
}

function markdown(){
  const date=new Intl.DateTimeFormat('en-PH',{dateStyle:'long',timeZone:'Asia/Manila'}).format(new Date());
  const lines=['# Duewise notification decisions','',`Exported ${date} from the notification lab (prototypes/design-exploration/index.html#notifications).`,'Source: specs/notification-lab/spec.md. These are preferences until confirmed and recorded in the PRD/DESIGN.','','| ID | Decision | Choice | Note |','|---|---|---|---|'];
  for(const d of DECISIONS){const k=state.choices[d.id],o=d.options.find(x=>x.key===k);lines.push(`| ${d.id} | ${d.title} | ${o?`${k}: ${o.name}`:'Not chosen'} | ${(state.notes[d.id]||'').replace(/\|/g,'/').replace(/\n/g,' ')} |`);}
  return lines.join('\n');
}
function announce(text){const s=$('#nl-progress');if(s)s.textContent=text;}
async function command(name){
  if(name==='export'){const url=URL.createObjectURL(new Blob([markdown()],{type:'text/markdown;charset=utf-8'})),a=document.createElement('a');a.href=url;a.download='duewise-notification-decisions.md';document.body.appendChild(a);a.click();a.remove();setTimeout(()=>URL.revokeObjectURL(url),1000);announce('Your decisions file is downloading.');}
  if(name==='copy'){try{await navigator.clipboard.writeText(markdown());announce('Copied. Paste it into the chat with Claude.');}catch{const t=$('#nl-export-text');t.hidden=false;t.value=markdown();t.select();announce('Copy the selected text below.');}}
  if(name==='clear'){state.choices={};state.notes={};save();render();announce('Choices cleared.');}
  if(name==='unlock'){const id=state.unlocking;state.unlocking=null;pay(id);}
  if(name==='cancel-unlock'){state.unlocking=null;render();}
}
function pay(id){
  if(action()==='B'){state.opened=id;render();setTimeout(()=>{state.opened=null;render();},2500);return;}
  state.paid[id]=true;render();
  setTimeout(()=>{const b=document.querySelector(`[data-nl-undo="${id}"]`);b?.focus?.();},0);
}
function init(){
  load();
  document.addEventListener('click',e=>{
    const b=e.target.closest('button');if(!b||!b.closest('#notification-view'))return;
    const ds=b.dataset;
    if(ds.nlDue){state.due=ds.nlDue;state.unlocking=null;render();history.replaceState(null,'',`#notifications/${state.due}`);}
    else if(ds.nlSet){const[k,v]=ds.nlSet.split(':');state[k]=v;state.unlocking=null;render();}
    else if(ds.nlPay){if(state.surface==='lock'){state.unlocking=ds.nlPay;render();}else pay(ds.nlPay);}
    else if(ds.nlUndo){delete state.paid[ds.nlUndo];render();}
    else if(ds.nlPick){const[id,key]=ds.nlPick.split(':');state.choices[id]===key?delete state.choices[id]:state.choices[id]=key;
      if(id==='N1'&&state.choices.N1)state.privacy=DECISIONS[0].options.find(o=>o.key===key).mode;
      const ok=save();render();announce(ok?(state.choices[id]?`${id}: option ${key} chosen.`:`${id}: choice removed.`):'Chosen for this visit only: browser storage is unavailable.');}
    else if(ds.nlCmd){command(ds.nlCmd);}
  });
}
window.NotificationLab={init,render,hash:()=>`#notifications/${state.due}`,content,dues:DUES,group:GROUP,decisions:DECISIONS,markdown,state:()=>state};
init();
})();
