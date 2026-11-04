/* Philippine prototype taxonomy and browser-local catalog. Names are suggestions,
   never payment integrations. This module has no network/platform dependency. */
(function(root){
  'use strict';
  const definitions=[
    ['housing','Housing & Property','home',[
      ['rent','House Rent / Apartment Rent'],['mortgage','Mortgage / Housing Loan'],['condo','Condominium Dues'],['hoa','HOA / Subdivision Dues'],['amilyar','Real Property Tax (Amilyar)']]],
    ['utilities','Utilities & Telecommunications','bolt',[
      ['electricity','Electricity'],['water','Water'],['internet','Internet'],['mobile','Mobile Postpaid'],['cable','Cable TV']]],
    ['credit','Loans & Credit','credit',[
      ['card','Credit Card Bills'],['personal','Personal Loans'],['salary','Salary Loans'],['vehicle','Motorcycle / Car Loans'],['housing','Housing Loans'],['gadget','Gadget Installments'],['bnpl','Buy Now, Pay Later'],['digital','Digital Lending Payments']]],
    ['government','Government Contributions & Taxes','government',[
      ['sss','SSS Contributions'],['philhealth','PhilHealth Contributions'],['pagibig','Pag-IBIG Contributions'],['mp2','Pag-IBIG MP2 Savings'],['bir','BIR Tax Payments'],['permit','Business Permit Renewals'],['lto','Vehicle Registration (LTO)']]],
    ['insurance','Insurance & Healthcare','health',[
      ['life','Life Insurance'],['health','Health Insurance / HMO'],['vehicle','Car / Motorcycle Insurance'],['property','Property Insurance'],['medical','Medical Bills'],['membership','Healthcare Memberships']]],
    ['education','Education','school',[
      ['tuition','Tuition Fees'],['misc','School Miscellaneous Fees'],['review','Review Center Payments'],['loan','Educational Loans']]],
    ['subscriptions','Subscriptions & Memberships','Subscription',[
      ['streaming','Streaming Services'],['software','Software Subscriptions'],['cloud','Cloud Storage'],['gym','Gym Memberships'],['professional','Professional Memberships'],['other','Other Recurring Subscriptions']]],
    ['family','Family & Personal Obligations','family',[
      ['support','Family Support / Allowance'],['childcare','Childcare / Daycare Fees'],['household','Household Services'],['association','Association Contributions'],['personal','Other Personal Commitments']]],
    ['business','Business & Professional','business',[
      ['rent','Business Rent'],['supplier','Supplier Payments'],['loan','Business Loans'],['saas','Software / SaaS Subscriptions'],['license','Professional License Renewals'],['tax','Business Taxes and Government Fees']]],
    ['other','Other','Other',[['general','Other']]]
  ];
  const groups=definitions.map(([id,name,icon])=>({id,name,icon}));
  const utilityIcons={electricity:'bolt',water:'water',internet:'wifi',mobile:'mobile',cable:'tv'};
  const categories=definitions.flatMap(([groupId,,icon,items])=>items.map(([slug,name])=>({id:`ph.${groupId}.${slug}`,groupId,name,icon:groupId==='utilities'?utilityIcons[slug]:icon,color:'green',custom:false})));
  const providerNames={meralco:'Meralco',cooperative:'Local Electric Cooperative',maynilad:'Maynilad',manilawater:'Manila Water',waterdistrict:'Local Water District',pldt:'PLDT',converge:'Converge',globe:'Globe',sky:'Sky',smart:'Smart',dito:'DITO',bdo:'BDO',bpi:'BPI',metrobank:'Metrobank',rcbc:'RCBC',unionbank:'UnionBank',securitybank:'Security Bank',eastwest:'EastWest',sss:'SSS',philhealth:'PhilHealth',pagibig:'Pag-IBIG',bir:'BIR',lto:'LTO',billease:'BillEase',homecredit:'Home Credit',atome:'Atome',spaylater:'SPayLater',netflix:'Netflix',disney:'Disney+',spotify:'Spotify',youtube:'YouTube Premium'};
  const providers=Object.entries(providerNames).map(([id,name])=>({id:'provider.'+id,name,custom:false}));
  const suggestions={
    'ph.utilities.electricity':['meralco','cooperative'], 'ph.utilities.water':['maynilad','manilawater','waterdistrict'], 'ph.utilities.internet':['pldt','converge','globe','sky'], 'ph.utilities.mobile':['globe','smart','dito'], 'ph.utilities.cable':['sky'],
    'ph.credit.card':['bdo','bpi','metrobank','rcbc','unionbank','securitybank','eastwest'], 'ph.credit.bnpl':['billease','homecredit','atome','spaylater'], 'ph.government.sss':['sss'], 'ph.government.philhealth':['philhealth'], 'ph.government.pagibig':['pagibig'], 'ph.government.mp2':['pagibig'], 'ph.government.bir':['bir'], 'ph.government.lto':['lto'], 'ph.subscriptions.streaming':['netflix','disney','spotify','youtube']
  };
  const icons=['Bill','home','bolt','water','wifi','mobile','tv','credit','government','health','school','Subscription','family','business','Task','Other'];
  const colors=['green','blue','violet','amber','rose','slate'];
  const normalize=value=>String(value||'').normalize('NFKC').trim().replace(/\s+/g,' ').toLocaleLowerCase('en-PH');
  const key='duewise.ph-catalog.v1';
  const cleanName=name=>String(name||'').trim().replace(/\s+/g,' ');
  class Catalog {
    constructor(storage,makeId=()=>globalThis.crypto.randomUUID()){
      this.storage=storage;this.makeId=makeId;this.state={version:1,categories:[],providers:[],recent:[]};this.error='';this.load();
    }
    load(){
      try{
        const raw=this.storage.getItem(key);if(!raw){this.error='';return true;}
        const data=JSON.parse(raw);
        if(data.version!==1||!Array.isArray(data.categories)||!Array.isArray(data.providers)||!Array.isArray(data.recent))throw Error('format');
        const ids=new Set(),names=new Set(categories.map(c=>normalize(c.name))),providerSet=new Set(providers.map(p=>normalize(p.name)));
        for(const c of data.categories){if(!c||typeof c.id!=='string'||!c.id.startsWith('custom.category.')||ids.has(c.id)||typeof c.name!=='string'||!cleanName(c.name)||names.has(normalize(c.name))||!icons.includes(c.icon)||!colors.includes(c.color))throw Error('category');ids.add(c.id);names.add(normalize(c.name));}
        for(const p of data.providers){if(!p||typeof p.id!=='string'||!p.id.startsWith('custom.provider.')||ids.has(p.id)||typeof p.name!=='string'||!cleanName(p.name)||providerSet.has(normalize(p.name)))throw Error('provider');ids.add(p.id);providerSet.add(normalize(p.name));}
        if(data.recent.some(id=>typeof id!=='string'))throw Error('recent');
        this.state={version:1,categories:data.categories.map(c=>({...c,groupId:'mine',custom:true})),providers:data.providers.map(p=>({...p,custom:true})),recent:[...new Set(data.recent)].filter(id=>categories.some(c=>c.id===id)||data.categories.some(c=>c.id===id)).slice(0,6)};
        this.error='';return true;
      }catch{this.error='Couldn’t read your saved categories and billers. Stored data was kept. Retry before changing them.';return false;}
    }
    commit(next){
      if(this.error)return {error:this.error};
      try{this.storage.setItem(key,JSON.stringify(next));this.state=next;return {ok:true};}
      catch{return {error:'Couldn’t save this catalog change. Your existing categories, billers and dues were kept.'};}
    }
    all(){return [...categories,...this.state.categories];}
    category(id){return this.all().find(c=>c.id===id)||categories.find(c=>c.id==='ph.other.general');}
    allProviders(){return [...providers,...this.state.providers];}
    provider(id){return this.allProviders().find(p=>p.id===id)||null;}
    nameError(name,except=null){const n=cleanName(name);if(!n)return 'Enter a category name.';if(this.all().some(c=>c.id!==except&&normalize(c.name)===normalize(n)))return 'A category with this name already exists.';return '';}
    saveCategory({id=null,name,icon='Other',color='green'}){
      if(id&&!this.state.categories.some(c=>c.id===id))return {error:'Only custom categories can be changed.'};
      const error=this.nameError(name,id);if(error)return {error};
      if(!icons.includes(icon)||!colors.includes(color))return {error:'Choose one of the available icons and colors.'};
      const value={id:id||'custom.category.'+this.makeId(),name:cleanName(name),icon,color,groupId:'mine',custom:true};
      const next={...this.state,categories:id?this.state.categories.map(c=>c.id===id?value:c):[...this.state.categories,value]};
      const result=this.commit(next);return result.error?result:{value};
    }
    deleteCategory(id){
      if(!this.state.categories.some(c=>c.id===id))return {error:'Only custom categories can be deleted.'};
      return this.commit({...this.state,categories:this.state.categories.filter(c=>c.id!==id),recent:this.state.recent.filter(x=>x!==id)});
    }
    remember(id){if(!this.all().some(c=>c.id===id))return {error:'Choose a category from the list.'};return this.commit({...this.state,recent:[id,...this.state.recent.filter(x=>x!==id)].slice(0,6)});}
    resolveProvider(name){
      const label=cleanName(name);if(!label)return {value:null};
      const existing=this.allProviders().find(p=>normalize(p.name)===normalize(label));if(existing)return {value:existing};
      const value={id:'custom.provider.'+this.makeId(),name:label,custom:true};
      const result=this.commit({...this.state,providers:[...this.state.providers,value]});return result.error?result:{value};
    }
    renameProvider(id,name){
      const current=this.state.providers.find(p=>p.id===id);if(!current)return {error:'Only custom billers can be renamed.'};
      const label=cleanName(name);if(!label)return {error:'Enter a biller name.'};
      if(this.allProviders().some(p=>p.id!==id&&normalize(p.name)===normalize(label)))return {error:'A biller with this name already exists.'};
      return this.commit({...this.state,providers:this.state.providers.map(p=>p.id===id?{...p,name:label}:p)});
    }
    deleteProvider(id){if(!this.state.providers.some(p=>p.id===id))return {error:'Only custom billers can be deleted.'};return this.commit({...this.state,providers:this.state.providers.filter(p=>p.id!==id)});}
    suggest(id){return (suggestions[id]||[]).map(slug=>this.provider('provider.'+slug)).filter(Boolean);}
    search(query){const n=normalize(query);return this.all().filter(c=>normalize(c.name+' '+(groups.find(g=>g.id===c.groupId)?.name||'My Categories')).includes(n));}
    frequent(dues){const counts=new Map();dues.filter(d=>!d.deleted).forEach(d=>counts.set(d.categoryId,(counts.get(d.categoryId)||0)+1));return this.all().filter(c=>counts.has(c.id)).sort((a,b)=>counts.get(b.id)-counts.get(a.id)||a.name.localeCompare(b.name,'en-PH')).slice(0,4);}
  }
  function localizeSeed(dues){
    const ids={internet:['ph.utilities.internet','provider.pldt'],rent:['ph.housing.rent',null],project:['ph.other.general',null],music:['ph.subscriptions.streaming','provider.spotify'],electric:['ph.utilities.electricity','provider.meralco'],water:['ph.utilities.water','provider.maynilad']};
    return dues.map(d=>({...d,categoryId:ids[d.id]?.[0]||'ph.other.general',providerId:ids[d.id]?.[1]||null,providerText:'',repeatEvery:1,repeatUnit:'months',reminderDays:[1,0]}));
  }
  const api={groups,categories,providers,icons,colors,normalize,key,Catalog,localizeSeed};
  if(typeof module==='object'&&module.exports)module.exports=api;else root.DuewisePH=api;
})(globalThis);
