const {test}=require('node:test');
const assert=require('node:assert/strict');
const C=require('../ph-catalog.js');
const E=require('../exploration-model.js');
const M=require('../../mobile-review/model.js');
function memory(){return{raw:null,fail:false,getItem(){return this.raw;},setItem(key,value){if(this.fail)throw Error('quota');this.raw=value;}};}
function create(storage=memory()){let sequence=0;return new C.Catalog(storage,()=>String(++sequence));}

test('all requested groups/53 predefined categories have stable IDs, distinct billers and relevant optional suggestions',()=>{
  const c=create();assert.equal(C.groups.length,10);assert.equal(C.categories.length,53);
  assert.equal(new Set(C.categories.map(x=>x.id)).size,53);
  for(const label of ['Real Property Tax (Amilyar)','Pag-IBIG MP2 Savings','Gadget Installments','Software / SaaS Subscriptions','Family Support / Allowance'])assert.ok(c.all().some(x=>x.name===label));
  assert.deepEqual(c.suggest('ph.utilities.electricity').map(p=>p.name),['Meralco','Local Electric Cooperative']);
  assert.equal(c.suggest('ph.credit.card').length,7);
  assert.equal(c.suggest('ph.utilities.internet').find(p=>p.name==='Globe').id,c.suggest('ph.utilities.mobile').find(p=>p.name==='Globe').id);
  assert.equal(c.category('ph.utilities.electricity').name,'Electricity');
  assert.equal(c.provider('provider.meralco').name,'Meralco');
});
test('custom creation validates normalized names and presentation choices; rename preserves identity across reload',()=>{
  const storage=memory(),c=create(storage);
  for(const name of ['', '   ', ' Electricity ','Ｅｌｅｃｔｒｉｃｉｔｙ'])assert.ok(c.saveCategory({name}).error);
  assert.ok(c.saveCategory({name:'Invalid',icon:'<script>'}).error);
  const value=c.saveCategory({name:'  Pet   care  ',icon:'health',color:'violet'}).value;
  assert.equal(value.name,'Pet care');assert.ok(value.id.startsWith('custom.category.'));
  assert.ok(c.saveCategory({name:'PET CARE'}).error);
  assert.equal(c.saveCategory({id:value.id,name:'Pet insurance',icon:'health',color:'blue'}).value.id,value.id);
  const restored=create(storage);assert.equal(restored.category(value.id).name,'Pet insurance');assert.equal(restored.category(value.id).color,'blue');
  assert.ok(restored.deleteCategory('ph.utilities.electricity').error);
});
test('catalog write/read failure retains last good state and never rewrites unreadable storage',()=>{
  const storage=memory(),c=create(storage),value=c.saveCategory({name:'Emergency fund'}).value;
  const before=storage.raw;storage.fail=true;
  assert.ok(c.saveCategory({id:value.id,name:'Changed'}).error);assert.equal(c.category(value.id).name,'Emergency fund');assert.equal(storage.raw,before);
  assert.ok(c.deleteCategory(value.id).error);assert.equal(c.category(value.id).id,value.id);
  storage.fail=false;storage.raw='{broken';assert.equal(c.load(),false);assert.equal(c.category(value.id).name,'Emergency fund');assert.ok(c.saveCategory({name:'Another'}).error);assert.equal(storage.raw,'{broken');
  storage.raw=before;assert.equal(c.load(),true);assert.ok(c.saveCategory({name:'Another'}).value);
});
test('provider IDs are separate, manual entries persist and normalized duplicates reuse the same provider',()=>{
  const storage=memory(),c=create(storage);
  assert.equal(c.resolveProvider('').value,null);
  assert.equal(c.resolveProvider(' meralco ').value.id,'provider.meralco');
  const p=c.resolveProvider('Neighborhood Cooperative').value;
  assert.ok(p.id.startsWith('custom.provider.'));assert.equal(c.resolveProvider('NEIGHBORHOOD COOPERATIVE').value.id,p.id);
  assert.ok(c.renameProvider(p.id,'BDO').error);assert.equal(c.renameProvider(p.id,'Our Cooperative').ok,true);
  assert.equal(create(storage).provider(p.id).name,'Our Cooperative');
  assert.equal(c.suggest('ph.housing.rent').length,0);assert.equal(c.provider(p.id).id,p.id);
});
test('recent/frequent/search survive category renames and deletion falls back to Other without mutating dues',()=>{
  const storage=memory(),c=create(storage),dues=C.localizeSeed(E.seed());
  assert.equal(c.search('amilyaR')[0].id,'ph.housing.amilyar');assert.ok(c.search('Utilities').length>=5);
  assert.ok(c.frequent(dues).some(x=>x.id==='ph.utilities.internet'));
  const custom=c.saveCategory({name:'Shared groceries'}).value;c.remember(custom.id);c.remember('ph.utilities.water');
  assert.deepEqual(create(storage).state.recent,['ph.utilities.water',custom.id]);
  const original=JSON.stringify(dues);c.deleteCategory(custom.id);assert.equal(c.category(custom.id).id,'ph.other.general');assert.equal(JSON.stringify(dues),original);
  assert.ok(!c.state.recent.includes(custom.id));
});
test('quarterly and custom repeat previews preserve month ends and safe bounds without duplicate series',()=>{
  assert.equal(E.nextOccurrence('2026-01-31','quarterly'),'2026-04-30');
  assert.equal(E.nextOccurrence('2026-11-04','custom',2,'weeks'),'2026-11-18');
  assert.equal(E.nextOccurrence('2026-12-31','custom',2,'months'),'2027-02-28');
  assert.equal(E.nextOccurrence('2028-02-29','custom',2,'years'),'2030-02-28');
  for(const count of [0,-1,1.5,366])assert.equal(E.nextOccurrence(M.TODAY,'custom',count,'days'),null);
  assert.equal(E.nextOccurrence('9999-12-31','custom',1,'days'),null);
});
