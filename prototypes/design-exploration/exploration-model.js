(function(root){
  'use strict';
  const M=typeof module==='object'&&module.exports?require('../mobile-review/model.js'):root.DuewiseModel;
  const seed=()=>M.seed().map(d=>({...d,repeat:['internet','rent','music'].includes(d.id)?'monthly':'none'}));
  function addDays(date,count){const d=new Date(date+'T12:00:00Z');d.setUTCDate(d.getUTCDate()+count);return d.toISOString().slice(0,10);}
  function nextOccurrence(date,repeat,every=1,unit='months'){
    if(!M.validDate(date)||repeat==='none')return null;
    if(!['weekly','monthly','quarterly','yearly','custom'].includes(repeat))return null;
    if(repeat==='custom'&&(!Number.isInteger(every)||every<1||every>365||!['days','weeks','months','years'].includes(unit)))return null;
    if(repeat==='weekly'||repeat==='custom'&&['days','weeks'].includes(unit)){
      const next=addDays(date,repeat==='weekly'?7:every*(unit==='weeks'?7:1));return M.validDate(next)?next:null;
    }
    const [year,month,day]=date.split('-').map(Number),step=repeat==='yearly'?12:repeat==='quarterly'?3:repeat==='custom'?every*(unit==='years'?12:1):1;
    const index=year*12+month-1+step,nextYear=Math.floor(index/12),nextMonth=index%12+1;
    if(nextYear>9999)return null;
    const end=new Date(0);end.setUTCHours(12,0,0,0);end.setUTCFullYear(nextYear,nextMonth,0);
    return `${String(nextYear).padStart(4,'0')}-${String(nextMonth).padStart(2,'0')}-${String(Math.min(day,end.getUTCDate())).padStart(2,'0')}`;
  }
  const sum=dues=>dues.reduce((n,d)=>n+(d.amount===null?0n:BigInt(d.amount)),0n).toString();
  function overview(dues){
    const active=M.sorted(dues),month=M.TODAY.slice(0,7),ahead=active.filter(d=>!d.paid&&d.date>=M.TODAY&&d.date.startsWith(month)),overdue=active.filter(d=>M.status(d)==='Overdue');
    const open=active.filter(d=>!d.paid),counts=M.summary(active);
    return {active,ahead,overdue,open,counts,aheadAmount:sum(ahead),overdueAmount:sum(overdue),withoutAmount:ahead.filter(d=>d.amount===null).length,next:active.find(d=>!d.paid&&d.date>=M.TODAY)||null,recurring:active.filter(d=>d.repeat!=='none'),monthOpen:open.filter(d=>d.date.startsWith(month)),categories:M.categories.map(category=>({category,count:active.filter(d=>d.category===category).length})),weeks:[0,1,2,3,4].map(i=>{const entries=open.filter(d=>d.date.startsWith(month)&&Math.floor((Number(d.date.slice(-2))-1)/7)===i);return {label:`${i*7+1}–${Math.min(i*7+7,30)}`,amount:sum(entries),count:entries.length};})};
  }
  function weekDates(date,weekStart='monday'){
    const day=new Date(date+'T12:00:00Z').getUTCDay(),offset=weekStart==='monday'?(day+6)%7:day;
    const safe=date<'0001-01-07'||date>'9999-12-25';
    return Array.from({length:7},(_,i)=>{const result=addDays(date,i-offset);return safe&&!M.validDate(result)?null:result;});
  }
  const api={seed,sum,overview,addDays,nextOccurrence,weekDates};
  if(typeof module==='object'&&module.exports)module.exports=api;else root.DuewiseExplore=api;
})(globalThis);
