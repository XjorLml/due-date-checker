// Data checks for the UX decision lab: round 1 (specs/ux-improvements, DL01–DL03) and
// round 2 (specs/ux-improvements-2, DL09, DL10 and the export part of DL12).
const {test}=require('node:test'),assert=require('node:assert/strict');
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');

function loadLab(){
  const store=new Map();
  const sandbox={
    window:{},
    document:{addEventListener(){},querySelector(){return null;},getElementById(){return null;}},
    localStorage:{getItem:k=>store.get(k)??null,setItem:(k,v)=>store.set(k,String(v))},
    history:{replaceState(){}},
    Intl,Blob:class{},URL,setTimeout,navigator:{}
  };
  vm.runInNewContext(fs.readFileSync(path.join(__dirname,'..','decision-lab.js'),'utf8'),sandbox);
  return sandbox.window.DecisionLab;
}
const round=(lab,id)=>lab.rounds.find(r=>r.id===id);

// The ranking tables in specs/ux-improvements/plan.md and specs/ux-improvements-2/plan.md
// (rank, id, reach, frequency, severity).
const PLAN1=[[1,'UX01',3,3,2],[2,'UX02',3,3,2],[3,'UX03',2,3,3],[4,'UX04',2,3,2],[5,'UX05',3,1,3],[6,'UX06',3,3,1],[7,'UX07',3,2,1],[8,'UX08',3,2,1],
  [9,'UX09',3,2,1],[10,'UX10',1,3,2],[11,'UX11',2,2,1],[12,'UX12',3,1,1],[13,'UX13',1,3,1],[14,'UX14',2,1,1],[15,'UX15',1,2,1],[16,'UX16',1,1,1]];
const PLAN2=[[1,'UX17',2,2,3],[2,'UX18',2,3,2],[3,'UX19',3,1,3],[4,'UX20',3,3,1],[5,'UX21',3,1,3],[6,'UX22',3,1,2],[7,'UX23',2,2,1],[8,'UX24',2,2,1],
  [9,'UX25',2,1,2],[10,'UX26',3,1,1],[11,'UX27',2,1,1]];
// The round 1 decision log in specs/ux-improvements/spec.md.
const DECIDED1={UX01:'A',UX02:'A',UX03:'A',UX04:'A',UX05:'A',UX06:'B',UX07:'A',UX08:'A',UX09:'A',UX10:'A',UX11:'A',UX12:'A',UX13:'A',UX14:'A',UX15:'B',UX16:'A'};

for(const [id,plan] of [['r1',PLAN1],['r2',PLAN2]]){
  test(`${id}: decisions match the plan ranking and are ordered by impact`,()=>{
    const lab=loadLab(),decisions=round(lab,id).decisions;
    assert.equal(decisions.length,plan.length);
    decisions.forEach((d,i)=>{
      const [rank,ux,r,f,s]=plan[i];
      assert.equal(d.rank,rank,ux);assert.equal(d.id,ux);assert.deepEqual([...d.score],[r,f,s],ux);
      if(i>0)assert.ok(lab.impact(decisions[i-1])>=lab.impact(d),`${ux} must not outrank the previous item`);
    });
  });

  test(`${id}: every decision has a today drawing and exactly one recommended option with unique keys`,()=>{
    const lab=loadLab();
    for(const d of round(lab,id).decisions){
      assert.equal(d.options.filter(o=>o.rec).length,1,d.id);
      const keys=d.options.map(o=>o.key);assert.equal(new Set(keys).size,keys.length,d.id);
      for(const drawing of [d.today,...d.options]){
        const html=drawing.draw();
        assert.match(html,/class="m-phone/,d.id);
        assert.match(html,/aria-hidden="true"/,`${d.id} drawings are decorative`);
        assert.ok(drawing.what&&drawing.what.length>10,`${d.id} has a text description`);
      }
    }
  });
}

test('round 2 opens by default, IDs are unique across rounds and each round has its own storage key',()=>{
  const lab=loadLab();
  assert.equal(lab.round,'r2');
  assert.deepEqual([...lab.decisions.map(d=>d.id)],PLAN2.map(p=>p[1]));
  const ids=lab.rounds.flatMap(r=>r.decisions.map(d=>d.id));assert.equal(new Set(ids).size,ids.length);
  assert.equal(round(lab,'r1').key,'duewise.ux-decisions.v1','round 1 keeps the key the user already chose under');
  assert.notEqual(round(lab,'r2').key,round(lab,'r1').key);
});

test('round 1 marks exactly the options recorded in the decision log',()=>{
  const lab=loadLab(),r1=round(lab,'r1');
  assert.deepEqual({...r1.decided},DECIDED1);
  for(const d of r1.decisions)assert.ok(d.options.some(o=>o.key===DECIDED1[d.id]),`${d.id} decided option exists`);
  assert.equal(round(lab,'r2').decided,undefined,'round 2 is not decided yet');
});

test('exported markdown lists every decision of the chosen round and marks unchosen ones',()=>{
  const lab=loadLab();
  const one=lab.markdown('r1'),two=lab.markdown('r2');
  for(const d of round(lab,'r1').decisions)assert.match(one,new RegExp(`\\| ${d.rank} \\| ${d.id} \\| `),d.id);
  assert.equal((one.match(/Not chosen/g)||[]).length,16);
  assert.match(one,/UX-REVIEW-2026-11-06/);
  for(const d of round(lab,'r2').decisions)assert.match(two,new RegExp(`\\| ${d.rank} \\| ${d.id} \\| `),d.id);
  assert.equal((two.match(/Not chosen/g)||[]).length,11);
  assert.doesNotMatch(two,/\| UX0\d \|/,'round 2 export holds only UX17–UX27');
  assert.match(two,/UX-REVIEW-2026-11-07/);
  for(const text of [one,two])assert.match(text,/preferences until confirmed/);
});
