const {test}=require('node:test'),assert=require('node:assert/strict');
const E=require('../essential-model.js'),M=require('../../mobile-review/model.js');
test('well-formed PHP paste normalization preserves exact centavos and rounding',()=>{
  for(const input of ['1,250.005','PHP 1,250.005','₱1,250.005',' 1250.005 '])assert.equal(M.parseAmount(E.normalizeAmount(input),'Bill').value,'125001');
  assert.equal(E.normalizeAmount('PHP 0'),'0');assert.equal(M.parseAmount(E.normalizeAmount(''),'Task').value,null);
  assert.equal(M.parseAmount(E.normalizeAmount('0'),'Task').value,'0');
});
test('ambiguous/malformed money stays invalid rather than being silently reinterpreted',()=>{
  for(const input of ['1,25','1,25.00','1,250,00','1.250,00','-1','PHP','1e3','₱1,2','PHP 1 250','₱1,250 junk'])assert.ok(M.parseAmount(E.normalizeAmount(input),'Bill').error,input);
  assert.ok(M.parseAmount(E.normalizeAmount('9,223,372,036,854,775,808'),'Bill').error);
});
test('reminder summary reflects global off/on, custom offset count and simulation',()=>{
  const f={reminderDays:[0,1],reminderExtra:'1, 2, 14'},r={enabled:false,time:'09:00'};
  assert.equal(E.reminderSummary(f,r),'Off · Simulation');
  assert.equal(E.reminderSummary(f,{...r,enabled:true}),'4 offsets · 09:00 Manila · Simulation');
  assert.equal(E.reminderSummary({reminderDays:[],reminderExtra:''},{...r,enabled:true}),'No offsets · 09:00 Manila · Simulation');
});
