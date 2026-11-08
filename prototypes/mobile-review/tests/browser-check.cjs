/* Dependency-free Edge/Chromium QA through the Chrome DevTools Protocol.
   Run: node prototypes/mobile-review/tests/browser-check.cjs
   Optionally set EDGE_PATH. Screenshots and the disposable profile stay in .qa/. */
const fs = require('node:fs');
const path = require('node:path');
const http = require('node:http');
const { spawn } = require('node:child_process');
const assert = require('node:assert/strict');
const root = path.resolve(__dirname, '../../..');
const folder = path.resolve(__dirname, '..');
const qa = path.join(folder, '.qa');
const delay = ms => new Promise(r=>setTimeout(r,ms));
const mime = {'.html':'text/html','.js':'text/javascript','.css':'text/css','.ttf':'font/ttf','.md':'text/plain'};
let browser,server,ws;
async function run() {
  const executable = process.env.EDGE_PATH || ['C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe','C:/Program Files/Google/Chrome/Application/chrome.exe'].find(p=>fs.existsSync(p));
  assert.ok(executable,'Set EDGE_PATH to an installed Chromium browser.');
  fs.mkdirSync(qa,{recursive:true});
  server=http.createServer((req,res)=>{
    let file;
    try { file=path.resolve(root,'.'+decodeURIComponent(new URL(req.url,'http://localhost').pathname)); } catch {res.writeHead(400).end();return;}
    if(!file.startsWith(root+path.sep)){res.writeHead(403).end();return;}
    fs.readFile(file,(error,data)=>{if(error){res.writeHead(404).end();return;}res.writeHead(200,{'Content-Type':mime[path.extname(file)]||'application/octet-stream'}).end(data);});
  });
  await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
  const url=`http://127.0.0.1:${server.address().port}/prototypes/mobile-review/index.html`;
  assert.equal((await fetch(url)).status,200);
  const profile=path.join(qa,'profile-'+Date.now());
  browser=spawn(executable,['--headless=new','--disable-gpu','--no-first-run','--no-default-browser-check','--disable-extensions','--disable-background-networking','--disable-sync','--remote-debugging-port=0','--user-data-dir='+profile,'about:blank'],{windowsHide:true,stdio:'ignore'});
  const portFile=path.join(profile,'DevToolsActivePort');
  for(let i=0;i<100&&!fs.existsSync(portFile);i++)await delay(150);
  assert.ok(fs.existsSync(portFile),'Browser debugging endpoint did not start.');
  const port=fs.readFileSync(portFile,'utf8').split('\n')[0];
  const target=await (await fetch(`http://127.0.0.1:${port}/json/new?about:blank`,{method:'PUT'})).json();
  ws=new WebSocket(target.webSocketDebuggerUrl);
  await new Promise((resolve,reject)=>{ws.onopen=resolve;ws.onerror=reject;});
  let id=0;const pending=new Map(),errors=[];
  ws.onmessage=({data})=>{const m=JSON.parse(data);if(m.id){const p=pending.get(m.id);if(p){pending.delete(m.id);m.error?p.reject(new Error(m.error.message)):p.resolve(m.result);}}else if(m.method==='Runtime.exceptionThrown')errors.push(m.params.exceptionDetails.text+': '+(m.params.exceptionDetails.exception?.description||''));};
  const send=(method,params={})=>new Promise((resolve,reject)=>{const n=++id;pending.set(n,{resolve,reject});ws.send(JSON.stringify({id:n,method,params}));setTimeout(()=>{if(pending.has(n)){pending.delete(n);reject(new Error('CDP timeout: '+method));}},30000).unref();});
  const evaluate=async expression=>{const r=await send('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});if(r.exceptionDetails)throw new Error(r.exceptionDetails.exception?.description||'Evaluation failed');return r.result.value;};
  await send('Runtime.enable');await send('Page.enable');await send('Page.navigate',{url});
  for(let i=0;i<70;i++){if(await evaluate("document.querySelector('.due-card') !== null"))break;await delay(100);}
  await evaluate('document.fonts.ready');
  assert.equal(await evaluate("document.querySelectorAll('#screen-nav button').length"),9);
  const screenshot=async name=>{const r=await send('Page.captureScreenshot',{format:'png',captureBeyondViewport:true});fs.writeFileSync(path.join(qa,name+'.png'),Buffer.from(r.data,'base64'));};
  await send('Emulation.setDeviceMetricsOverride',{width:1440,height:1120,deviceScaleFactor:1,mobile:false});
  await screenshot('desktop-light');
  const issues=[];
  // Exercise every screen and every design alternative at desktop and mobile widths.
  for(const width of [1440,390,360,320]) {
    await send('Emulation.setDeviceMetricsOverride',{width,height:1000,deviceScaleFactor:1,mobile:width<500});
    for(const page of ['dues','dashboard','calendar','detail','add','edit','settings','reminders','onboarding']) {
      await evaluate(`document.querySelector('#screen-nav [data-page="${page}"]').click()`);
      for(const option of [0,1]) {
        await evaluate(`document.querySelectorAll('[name="layout-choice"]')[${option}].click()`);
        const overflow=await evaluate(`({body:document.documentElement.scrollWidth>innerWidth+1,app:document.querySelector('#preview').scrollWidth>document.querySelector('#preview').clientWidth+1})`);
        if(overflow.body||overflow.app)issues.push({width,page,option,...overflow});
        const unnamedIcons=await evaluate("Array.from(document.querySelectorAll('#phone .icon-button')).filter(b=>!b.getAttribute('aria-label')).length");
        assert.equal(unnamedIcons,0,`Unnamed icon on ${page}`);
        if(['add','edit','settings','reminders'].includes(page)) {
          for(const scroll of ['0','document.querySelector("#preview").scrollHeight']) {
            await evaluate(`document.querySelector('#preview').scrollTop=${scroll}`);
            assert.equal(await evaluate("(()=>{const b=document.querySelector('#app-actions .primary').getBoundingClientRect(),p=document.querySelector('#phone').getBoundingClientRect();return b.height>=44&&b.top>=p.top&&b.bottom<=p.bottom;})()"),true,`Save offscreen on ${page}, ${width}`);
          }
        }
      }
    }
  }
  assert.deepEqual(issues,[],'Horizontal overflow: '+JSON.stringify(issues));
  await send('Emulation.setDeviceMetricsOverride',{width:390,height:1000,deviceScaleFactor:1,mobile:true});
  await evaluate(`document.querySelector('#screen-nav [data-page="dues"]').click();document.querySelector('[name=layout-choice][value=cards]').click();document.querySelector('#preview').scrollTop=0;`);
  const cardOffset=await evaluate("document.querySelector('.due-card').getBoundingClientRect().top-document.querySelector('#preview').getBoundingClientRect().top");
  assert.ok(cardOffset<210,`First due is too far down: ${cardOffset}px`);
  assert.equal(await evaluate("document.querySelector('#preview [data-page=add]')===null"),true);
  await screenshot('mobile-compact-dues');
  // Search input is retained through navigation and global counts do not change.
  await evaluate(`document.querySelector('#screen-nav [data-page="dashboard"]').click();document.querySelector('#search').value='rent';document.querySelector('#search').dispatchEvent(new Event('input',{bubbles:true}));`);
  assert.equal(await evaluate("document.querySelectorAll('#dashboard-results .due-card').length"),1);
  assert.ok(await evaluate("document.querySelector('.progress-number').textContent.includes('/ 6 paid')"));
  await evaluate(`document.querySelector('[data-filter=Paid]').click();`);
  assert.ok(await evaluate("document.querySelector('#dashboard-results').textContent.includes('No matching dues')"));
  await evaluate(`document.querySelector('#screen-nav [data-page="dues"]').click();document.querySelector('#screen-nav [data-page="dashboard"]').click();`);
  assert.equal(await evaluate("document.querySelector('#search').value"),'rent');
  await evaluate(`document.querySelector('#preview [data-action=clear-filters]').click();`);
  assert.equal(await evaluate("document.querySelectorAll('#dashboard-results .due-card').length"),6);
  // Day counts and agenda membership agree, and the selected date survives tabs.
  await evaluate(`document.querySelector('#screen-nav [data-page="calendar"]').click();document.querySelector('[name=layout-choice][value=month]').click();document.querySelector('[data-date="2026-11-04"]').click();`);
  assert.equal(await evaluate("document.querySelectorAll('#preview .due-card').length"),2);
  await evaluate(`document.querySelector('[data-date="2026-11-06"]').click();document.querySelector('#screen-nav [data-page="dues"]').click();document.querySelector('#screen-nav [data-page="calendar"]').click();`);
  assert.equal(await evaluate("document.querySelectorAll('#preview .due-card').length"),1);
  assert.ok(await evaluate("document.querySelector('#preview .due-card').textContent.includes('Music subscription')"));
  await evaluate(`document.querySelector('#app-header [data-page=add]').click();`);
  assert.equal(await evaluate("document.querySelector('#due-date').value"),'2026-11-04');
  await evaluate(`document.querySelector('#app-header [data-action=back]').click();`);
  assert.equal(await evaluate("document.querySelector('[data-date=\"2026-11-06\"]').getAttribute('aria-pressed')"),'true');
  // A real form submission and round-trip through details; Cancel never saves paid draft.
  await evaluate(`document.querySelector('#screen-nav [data-page="dues"]').click(); document.querySelector('#app-header [data-page="add"]').click();document.querySelector('#app-actions [type=submit]').click();`);
  assert.equal(await evaluate("document.activeElement.id"),'due-title');
  await evaluate(`document.querySelector('#due-title').value='QA sample due'; document.querySelector('#due-title').dispatchEvent(new Event('input',{bubbles:true})); document.querySelector('#due-amount').value='10.005'; document.querySelector('#due-amount').dispatchEvent(new Event('input',{bubbles:true})); document.querySelector('#app-actions [type=submit]').click();`);
  assert.ok(await evaluate("document.querySelector('#preview').textContent.includes('QA sample due')"));
  await evaluate(`Array.from(document.querySelectorAll('.due-card')).find(b=>b.textContent.includes('QA sample due')).click();`);
  assert.ok(await evaluate("document.querySelector('.detail-amount').textContent.includes('10.01')"));
  await evaluate(`document.querySelector('#app-header [data-page="edit"]').click(); document.querySelector('#due-paid').click(); document.querySelector('#app-header [data-action="back"]').click();`);
  assert.ok(await evaluate("document.querySelector('.detail-hero').textContent.includes('Unpaid')"));
  await evaluate(`document.querySelector('#app-header [data-page="edit"]').click(); document.querySelector('#due-paid').click(); document.querySelector('#due-form').requestSubmit();`);
  assert.ok(await evaluate("document.querySelector('.detail-hero').textContent.includes('Paid')"));
  // Title is user input: HTML-like text must render literally, never as markup.
  await evaluate(`document.querySelector('#app-header [data-page="edit"]').click(); document.querySelector('#due-title').value='<img src=x onerror=alert(1)>'; document.querySelector('#due-title').dispatchEvent(new Event('input',{bubbles:true})); document.querySelector('#due-form').requestSubmit();`);
  assert.equal(await evaluate("document.querySelector('#preview img')===null"),true);
  // Draft Settings survives Reminders; week start changes only after Save.
  await evaluate(`document.querySelector('#screen-nav [data-page="settings"]').click();document.querySelectorAll('[name="layout-choice"]')[0].click(); document.querySelector('[data-pref="week"][data-value="sunday"]').click(); document.querySelector('#preview [data-action="open-reminders"]').click(); document.querySelector('#app-header [data-action="back"]').click();`);
  assert.equal(await evaluate("document.querySelector('[data-pref=week][data-value=sunday]').getAttribute('aria-pressed')"),'true');
  await evaluate(`document.querySelector('#app-actions [data-action="save-preferences"]').click(); document.querySelector('#screen-nav [data-page="calendar"]').click();`);
  assert.ok(await evaluate("document.querySelector('#preview').textContent.includes('Sunday first')"));
  // No enable until explicit dialog confirmation.
  await evaluate(`document.querySelector('#screen-nav [data-page="reminders"]').click();document.querySelector('#reminder-form').requestSubmit(document.querySelector('[data-enable=true]'));`);
  assert.equal(await evaluate("document.querySelector('#action-dialog').open"),true);
  await evaluate("document.querySelector('#action-dialog button[value=cancel]').click()");await delay(50);
  assert.ok(await evaluate("document.querySelector('.reminder-status').textContent.includes('off')"));
  await evaluate(`document.querySelector('#reminder-form').requestSubmit(document.querySelector('[data-enable=true]')); document.querySelector('#dialog-confirm').click();`);await delay(50);
  assert.ok(await evaluate("document.querySelector('.reminder-status').textContent.includes('on')"));
  // Destructive confirmation Cancel, then delete and unavailable Details.
  await evaluate(`document.querySelector('#screen-nav [data-page="edit"]').click();document.querySelector('[data-action=delete-due]').click();document.querySelector('#action-dialog button[value=cancel]').click();`);await delay(50);
  assert.equal(await evaluate("document.querySelector('#due-form')!==null"),true);
  await evaluate(`document.querySelector('[data-action=delete-due]').click();document.querySelector('#dialog-confirm').click();`);await delay(50);
  assert.ok(await evaluate("document.querySelector('#preview').textContent.includes('unavailable')"));
  // Recovery state previews and persistence of review choices/notes (not app data).
  for(const mode of ['empty','loading','error','stale','normal']) {
    await evaluate(`document.querySelector('#screen-nav [data-page="dues"]').click();document.querySelector('#preview-state').value='${mode}';document.querySelector('#preview-state').dispatchEvent(new Event('change',{bubbles:true}));`);
    assert.ok(await evaluate("document.querySelector('#preview').textContent.length>30"));
  }
  await send('Emulation.setDeviceMetricsOverride',{width:390,height:1000,deviceScaleFactor:1,mobile:true});
  await evaluate(`document.querySelector('[data-theme=dark]').click();document.querySelector('#screen-nav [data-page="dashboard"]').click();document.querySelectorAll('[name="layout-choice"]')[0].click();`);
  await screenshot('mobile-dark');
  assert.equal(await evaluate("document.querySelector('#phone').dataset.theme"),'dark');
  assert.ok(await evaluate("JSON.parse(localStorage.getItem('duewise.mobile-review.2026-11-04.v1')).choices.dashboard==='compact'"));
  for(const page of ['calendar','add','detail','settings','reminders','onboarding']) {
    await evaluate(`document.querySelector('#screen-nav [data-page="${page}"]').click();`);
    for(const option of [0,1]) {
      await evaluate(`document.querySelectorAll('[name="layout-choice"]')[${option}].click();`);
      assert.equal(await evaluate("document.querySelector('#preview').scrollWidth<=document.querySelector('#preview').clientWidth+1"),true,`Dark overflow: ${page}`);
    }
  }
  await evaluate(`document.querySelector('#screen-nav [data-page="onboarding"]').click();`);
  for(let step=1;step<=4;step++) {
    assert.ok(await evaluate(`document.querySelector('.onboarding-top').textContent.includes('${step} of 4')`));
    await evaluate("document.querySelector('[data-action=onboarding-next]').click()");
  }
  assert.ok(await evaluate("document.querySelector('#preview').textContent.includes('Your dues')"));
  await evaluate(`document.querySelector('#screen-nav [data-page="calendar"]').click();document.querySelector('#preview [data-action=today]').click();document.querySelectorAll('[name="layout-choice"]')[0].click();`);
  await screenshot('mobile-calendar');
  await evaluate(`document.querySelector('#screen-nav [data-page="reminders"]').click();document.querySelectorAll('[name="layout-choice"]')[0].click();`);
  await screenshot('mobile-reminders');
  await evaluate(`document.querySelector('#screen-nav [data-page="add"]').click();document.querySelectorAll('[name="layout-choice"]')[0].click();`);
  await screenshot('mobile-editor');
  await evaluate(`document.querySelector('#screen-nav [data-page="dues"]').click();document.querySelectorAll('[name="layout-choice"]')[1].click();document.querySelector('#decision-note').value='Prefer larger dates.';document.querySelector('#decision-note').dispatchEvent(new Event('input',{bubbles:true}));`);
  await send('Browser.setDownloadBehavior',{behavior:'allow',downloadPath:qa});
  const download=path.join(qa,'duewise-ui-choices.md');
  if(fs.existsSync(download))fs.unlinkSync(download);
  await evaluate("document.querySelector('#export-choices').click()");
  for(let i=0;i<50&&!fs.existsSync(download);i++)await delay(100);
  assert.ok(fs.existsSync(download),'Markdown export did not download.');
  const exported=fs.readFileSync(download,'utf8');
  assert.ok(exported.includes('Selected: Ledger rows')&&exported.includes('Prefer larger dates.')&&exported.includes('Onboarding (future concept)'));
  await send('Page.reload');
  for(let i=0;i<70;i++){if(await evaluate("document.querySelector('[name=layout-choice]:checked')!==null"))break;await delay(100);}
  assert.equal(await evaluate("document.querySelector('[name=layout-choice]:checked').value"),'ledger');
  assert.equal(await evaluate("document.querySelector('#decision-note').value"),'Prefer larger dates.');
  assert.equal(await evaluate("document.querySelectorAll('.due-card').length"),6);
  // Direct-file startup is the intended user handoff, independently of HTTP preview.
  const {pathToFileURL}=require('node:url');
  await send('Page.navigate',{url:pathToFileURL(path.join(folder,'index.html')).href});
  for(let i=0;i<70;i++){if(await evaluate("document.querySelectorAll('.due-card').length===6"))break;await delay(100);}
  assert.equal(await evaluate("document.querySelectorAll('.due-card').length"),6);
  await evaluate('document.fonts.ready');
  assert.equal(await evaluate("document.fonts.check('400 16px Jakarta')"),true);
  await send('Emulation.setDeviceMetricsOverride',{width:1440,height:1120,deviceScaleFactor:1,mobile:false});
  await evaluate(`document.querySelector('[data-theme=light]').click();document.querySelector('#screen-nav [data-page="dues"]').click();document.querySelectorAll('[name="layout-choice"]')[0].click();`);
  await screenshot('desktop-final');
  const ax=await send('Accessibility.getFullAXTree');
  assert.ok(ax.nodes.some(n=>n.role?.value==='navigation'));
  assert.deepEqual(errors,[],'Browser runtime errors');
  console.log(`PASS: compact header actions, named icons, persistent 44px Save actions at both scroll ends; first due at ${Math.round(cardOffset)}px inside the scroll area. All 9 screens × 2 alternatives at 1440/390/360/320 px; existing save/validation/payment/delete, preferences/reminders, export/reload and direct-file checks passed.`);
  console.log('Screenshots: prototypes/mobile-review/.qa/{desktop-light,mobile-dark,mobile-calendar,mobile-reminders,mobile-editor,desktop-final}.png');
}
run().catch(error=>{console.error(error.stack);process.exitCode=1;}).finally(()=>{if(ws)ws.close();if(browser)browser.kill();if(server)server.close();});
