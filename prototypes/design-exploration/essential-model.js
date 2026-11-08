(function(root){
  'use strict';
  // UI normalization only. The shared exact-centavo parser remains authoritative.
  function normalizeAmount(value){
    const raw=String(value??'').trim();
    const candidate=raw.replace(/^(?:PHP\s+|₱\s*)/i,'');
    if(/^\d+(?:\.\d+)?$/.test(candidate))return candidate;
    if(/^\d{1,3}(?:,\d{3})+(?:\.\d+)?$/.test(candidate))return candidate.replace(/,/g,'');
    return raw; // Preserve malformed input so validation can explain it.
  }
  function reminderSummary(form,global){
    if(!global.enabled)return 'Off · Simulation';
    const count=new Set([...(form.reminderDays||[]),...String(form.reminderExtra||'').split(',').filter(s=>/^\s*\d+\s*$/.test(s)).map(Number)]).size;
    return `${count?count+' offset'+(count===1?'':'s'):'No offsets'} · ${global.time} Manila · Simulation`;
  }
  const api={normalizeAmount,reminderSummary};
  if(typeof module==='object'&&module.exports)module.exports=api;else root.DuewiseEssential=api;
})(globalThis);
