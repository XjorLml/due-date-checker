(function(root){
  'use strict';
  const M=typeof module==='object'&&module.exports?require('../mobile-review/model.js'):root.DuewiseModel;
  // Rent shows the PRD TM rule (2026-11-16): twice a month on two user-picked days.
  const seed=()=>M.seed().map(d=>d.id==='rent'?{...d,repeat:'twicemonthly',repeatDay1:4,repeatDay2:19}:{...d,repeat:['internet','music'].includes(d.id)?'monthly':'none'});
  function addDays(date,count){const d=new Date(date+'T12:00:00Z');d.setUTCDate(d.getUTCDate()+count);return d.toISOString().slice(0,10);}
  const iso=(y,m,d)=>`${String(y).padStart(4,'0')}-${String(m).padStart(2,'0')}-${String(d).padStart(2,'0')}`;
  function lastDay(year,month){const end=new Date(0);end.setUTCHours(12,0,0,0);end.setUTCFullYear(year,month,0);return end.getUTCDate();}
  // Twice a month (PRD TM): two different whole days 1–31.
  const validTwiceDays=days=>Array.isArray(days)&&days.length===2&&days.every(n=>Number.isInteger(n)&&n>=1&&n<=31)&&days[0]!==days[1];
  // The earliest of the two days after the date; a missing day falls back to the month's last day,
  // so two days landing on the same date give that month one occurrence (29 & 30 in February).
  function twiceMonthlyNext(date,days){
    if(!validTwiceDays(days))return null;
    const [first,second]=[...days].sort((a,b)=>a-b),[year,month]=date.split('-').map(Number);
    const on=(y,m,d)=>iso(y,m,Math.min(d,lastDay(y,m)));
    const candidates=[on(year,month,first),on(year,month,second)];
    if(!(year===9999&&month===12))candidates.push(on(month===12?year+1:year,month===12?1:month+1,first));
    return candidates.find(c=>c>date)||null;
  }
  function nextOccurrence(date,repeat,every=1,unit='months',days=[15,30]){
    if(!M.validDate(date)||repeat==='none')return null;
    if(!['weekly','monthly','twicemonthly','quarterly','yearly','custom'].includes(repeat))return null;
    if(repeat==='twicemonthly')return twiceMonthlyNext(date,days);
    if(repeat==='custom'&&(!Number.isInteger(every)||every<1||every>365||!['days','weeks','months','years'].includes(unit)))return null;
    if(repeat==='weekly'||repeat==='custom'&&['days','weeks'].includes(unit)){
      const next=addDays(date,repeat==='weekly'?7:every*(unit==='weeks'?7:1));return M.validDate(next)?next:null;
    }
    const [year,month,day]=date.split('-').map(Number),step=repeat==='yearly'?12:repeat==='quarterly'?3:repeat==='custom'?every*(unit==='years'?12:1):1;
    const index=year*12+month-1+step,nextYear=Math.floor(index/12),nextMonth=index%12+1;
    if(nextYear>9999)return null;
    return iso(nextYear,nextMonth,Math.min(day,lastDay(nextYear,nextMonth)));
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
  const api={seed,sum,overview,addDays,nextOccurrence,validTwiceDays,weekDates};
  if(typeof module==='object'&&module.exports)module.exports=api;else root.DuewiseExplore=api;
})(globalThis);
