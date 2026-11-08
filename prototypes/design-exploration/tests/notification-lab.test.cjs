// Data checks for the notification lab (specs/notification-lab, NL01–NL04).
const {test}=require('node:test'),assert=require('node:assert/strict');
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');

function loadLab(){
  const store=new Map();
  const sandbox={
    window:{},
    document:{addEventListener(){},querySelector(){return null;}},
    localStorage:{getItem:k=>store.get(k)??null,setItem:(k,v)=>store.set(k,String(v))},
    history:{replaceState(){}},Intl,Blob:class{},URL,setTimeout,navigator:{}
  };
  vm.runInNewContext(fs.readFileSync(path.join(__dirname,'..','notification-lab.js'),'utf8'),sandbox);
  return sandbox.window.NotificationLab;
}

test('NL01: today\'s app says the same generic text for every due and offers no button',()=>{
  const lab=loadLab();
  for(const due of lab.dues)for(const when of ['before','day']){
    const shade=lab.content(due,when,'generic','shade'),lock=lab.content(due,when,'generic','lock');
    assert.equal(shade.title,'Duewise reminder');
    assert.equal(shade.text,'Open Duewise to review an unpaid due date.');
    assert.equal(lock.text,'Contents hidden');
    assert.equal(shade.actions.length,0);
    for(const c of [shade,lock]){assert.ok(!c.title.includes(due.title)&&!c.text.includes(due.title),due.id);if(due.amount)assert.ok(!c.text.includes(due.amount),due.id);}
  }
});

test('NL02: details mode names the due, amount, date and repeat, and the lock screen stays private',()=>{
  const lab=loadLab(),rent=lab.dues.find(d=>d.id==='rent'),passport=lab.dues.find(d=>d.id==='passport');
  assert.deepEqual({...lab.content(rent,'before','unlocked','shade'),actions:[...lab.content(rent,'before','unlocked','shade').actions]},
    {title:'Rent is due tomorrow',text:'₱12,000.00 · Thu 05 Nov · repeats monthly',actions:['pay'],note:''});
  assert.equal(lab.content(passport,'day','unlocked','shade').text,'Task, no amount · Sat 07 Nov');
  for(const due of lab.dues){
    const lock=lab.content(due,'day','unlocked','lock');
    assert.equal(lock.text,'You have a due today');assert.equal(lock.actions.length,0);
    assert.ok(!lock.text.includes(due.title));
    const everywhere=lab.content(due,'day','everywhere','lock');
    assert.match(everywhere.title,new RegExp(`^${due.title} is due today$`));assert.deepEqual([...everywhere.actions],['pay']);
    assert.match(everywhere.note,/PIN or fingerprint/);
  }
});

test('NL03: after Mark as paid the notification confirms and offers Undo; repeating dues name their next date',()=>{
  const lab=loadLab(),rent=lab.dues.find(d=>d.id==='rent'),water=lab.dues.find(d=>d.id==='water');
  const paidRent=lab.content(rent,'day','unlocked','shade',true),paidWater=lab.content(water,'day','unlocked','shade',true);
  assert.equal(paidRent.title,'Rent marked paid');assert.equal(paidRent.text,'Next one added for 05 Dec 2026.');assert.deepEqual([...paidRent.actions],['undo']);
  assert.equal(paidWater.text,'Saved in Duewise.');
  assert.equal(lab.content(rent,'day','unlocked','lock',true).text,'You have a due today','the lock screen never shows details in this mode');
});

test('NL04: two decisions with one recommended option each, and the export lists both',()=>{
  const lab=loadLab();
  assert.deepEqual([...lab.decisions.map(d=>d.id)],['N1','N2']);
  for(const d of lab.decisions){assert.equal(d.options.filter(o=>o.rec).length,1,d.id);assert.equal(new Set(d.options.map(o=>o.key)).size,d.options.length);}
  assert.deepEqual([...lab.decisions[0].options.map(o=>o.mode)],['unlocked','generic','everywhere']);
  const md=lab.markdown();
  assert.match(md,/\| N1 \| What a reminder says \| Not chosen \|/);assert.match(md,/\| N2 \| Paying from a reminder \| Not chosen \|/);
  assert.match(md,/preferences until confirmed/);
  assert.equal(lab.group.members.every(id=>lab.dues.find(d=>d.id===id).date===lab.group.date),true);
});
