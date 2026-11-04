/* Local Edge/Chromium CDP checks. EDGE_PATH may override the installed browser.
   No packages are installed. The owned loopback server/browser stop on completion. */
const fs=require('node:fs'),path=require('node:path'),http=require('node:http'),assert=require('node:assert/strict');
const {spawn}=require('node:child_process');
const {pathToFileURL}=require('node:url');
const root=path.resolve(__dirname,'../../..'),folder=path.resolve(__dirname,'..'),qa=path.join(folder,'.qa');
const delay=ms=>new Promise(r=>setTimeout(r,ms));
let server,browser,ws;
async function run(){
  const executable=process.env.EDGE_PATH||['C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe','C:/Program Files/Google/Chrome/Application/chrome.exe'].find(p=>fs.existsSync(p));
  assert.ok(executable,'Set EDGE_PATH to an installed Chromium browser.');fs.mkdirSync(qa,{recursive:true});
  const mime={'.html':'text/html','.js':'text/javascript','.css':'text/css','.ttf':'font/ttf','.md':'text/plain'};
  server=http.createServer((req,res)=>{let file;try{file=path.resolve(root,'.'+decodeURIComponent(new URL(req.url,'http://localhost').pathname));}catch{res.writeHead(400).end();return;}if(!file.startsWith(root+path.sep)){res.writeHead(403).end();return;}fs.readFile(file,(err,data)=>{if(err){res.writeHead(404).end();return;}res.writeHead(200,{'Content-Type':mime[path.extname(file)]||'application/octet-stream'}).end(data);});});
  await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
  const url=`http://127.0.0.1:${server.address().port}/prototypes/design-exploration/index.html`;
  assert.equal((await fetch(url)).status,200);
  const profile=path.join(qa,'profile-'+Date.now());
  browser=spawn(executable,['--headless=new','--disable-gpu','--no-first-run','--no-default-browser-check','--disable-extensions','--disable-background-networking','--disable-sync','--disable-ipc-flooding-protection','--remote-debugging-port=0','--user-data-dir='+profile,'about:blank'],{windowsHide:true,stdio:'ignore'});
  const portFile=path.join(profile,'DevToolsActivePort');for(let i=0;i<100&&!fs.existsSync(portFile);i++)await delay(150);assert.ok(fs.existsSync(portFile),'Browser debugging startup timed out.');
  const port=fs.readFileSync(portFile,'utf8').split('\n')[0],target=await(await fetch(`http://127.0.0.1:${port}/json/new?about:blank`,{method:'PUT'})).json();
  ws=new WebSocket(target.webSocketDebuggerUrl);await new Promise((resolve,reject)=>{ws.onopen=resolve;ws.onerror=reject;});
  let id=0;const pending=new Map(),errors=[];
  ws.onmessage=({data})=>{const m=JSON.parse(data);if(m.id){const p=pending.get(m.id);if(p){pending.delete(m.id);m.error?p.reject(new Error(m.error.message)):p.resolve(m.result);}}else if(m.method==='Runtime.exceptionThrown')errors.push(m.params.exceptionDetails.exception?.description||m.params.exceptionDetails.text);};
  const send=(method,params={})=>new Promise((resolve,reject)=>{const n=++id;pending.set(n,{resolve,reject});ws.send(JSON.stringify({id:n,method,params}));setTimeout(()=>{if(pending.has(n)){pending.delete(n);reject(new Error('CDP timeout: '+method));}},30000).unref();});
  const evaluate=async expression=>{const r=await send('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});if(r.exceptionDetails)throw new Error(r.exceptionDetails.exception?.description||'Evaluation failed');return r.result.value;};
  const waitFor=async expression=>{for(let i=0;i<80;i++){if(await evaluate(expression))return;await delay(100);}throw new Error('Condition timed out: '+expression);};
  const reloadAndWait=async condition=>{const origin=await evaluate('performance.timeOrigin');await send('Page.reload');await waitFor(`performance.timeOrigin!==${origin}&&document.readyState==='complete'&&(${condition})`);};
  const reveal=selector=>evaluate(`(()=>{const e=document.querySelector(${JSON.stringify(selector)});if(!e)throw Error('Missing control: '+${JSON.stringify(selector)});for(let p=e.parentElement;p;p=p.parentElement)if(p.tagName==='DETAILS'&&!p.firstElementChild?.contains(e))p.open=true;})()`);
  const click=async selector=>{await reveal(selector);return evaluate(`document.querySelector(${JSON.stringify(selector)}).click()`);};
  const input=async(selector,value)=>{await reveal(selector);return evaluate(`(()=>{const e=document.querySelector(${JSON.stringify(selector)});e.value=${JSON.stringify(value)};e.dispatchEvent(new Event('input',{bubbles:true}));e.dispatchEvent(new Event('change',{bubbles:true}));})()`);};
  const page=name=>input('#screen-selector',name);
  const dims=width=>send('Emulation.setDeviceMetricsOverride',{width,height:1050,deviceScaleFactor:1,mobile:width<600});
  const shot=async name=>{await evaluate('document.fonts.ready');const image=await send('Page.captureScreenshot',{format:'png',captureBeyondViewport:true});fs.writeFileSync(path.join(qa,name+'.png'),Buffer.from(image.data,'base64'));};
  const noOverflow=async label=>{const result=await evaluate("({body:document.documentElement.scrollWidth>innerWidth+1,app:!document.querySelector('#explore-view').hidden&&document.querySelector('#app-body').scrollWidth>document.querySelector('#app-body').clientWidth+1})");assert.deepEqual(result,{body:false,app:false},'Overflow '+label+' '+JSON.stringify(result));};
  await send('Runtime.enable');await send('Page.enable');await send('Page.navigate',{url});
  if(!process.argv.includes('--essential')){
    await require('./ph-browser-flows.cjs')({fs,path,folder,qa,url,send,evaluate,waitFor,reloadAndWait,click,input,page,dims,shot,noOverflow,delay,pathToFileURL});
    await require('./interaction-browser-flows.cjs')({url,send,evaluate,waitFor,reloadAndWait,click,input,page,dims,shot,noOverflow,delay});
  }
  await require('./essential-browser-flows.cjs')({send,evaluate,waitFor,click,input,page,dims,shot,noOverflow,delay});
  assert.deepEqual(errors,[],'Browser runtime errors');

}
run().catch(error=>{console.error(error.stack);process.exitCode=1;}).finally(()=>{ws?.close();browser?.kill();server?.close();});
