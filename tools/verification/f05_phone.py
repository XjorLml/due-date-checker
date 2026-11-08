"""Targeted phone check. State is saved locally; cleanup restores original settings."""
import argparse
from datetime import datetime,timedelta
import json,re,time
from pathlib import Path
from android_ui import Device,PACKAGE,require

STATE=Path('artifacts/f05-completion/phone-state.json')
p=None
state=json.loads(STATE.read_text())

def save():STATE.write_text(json.dumps(state),encoding='utf-8')
def entry(label):
    for attempt in range(10):
        found=[n for n in p.ui() if n.get('class')=='android.widget.EditText' and n.get('text','').startswith(label)]
        if len(found)==1:return found[0]
        p.scroll()
    raise RuntimeError('Controlled field unavailable')
def set_entry(label,value):
    p.tap(entry(label));p.shell('input','keycombination','KEYCODE_CTRL_LEFT','KEYCODE_A')
    p.shell('input','keyevent','KEYCODE_DEL');p.shell('input','text',value.replace(' ','%s'))
    p.ui();p.shell('input','keyevent','KEYCODE_BACK')
    require(entry(label).get('text','').endswith(value),'Controlled field input applied')
def time_dial(identifier,hour,minute):
    p.top();p.tap(p.seek(id=PACKAGE+':id/'+identifier))
    p.tap(p.find(id='android:id/hours'));p.tap(p.find(desc=str(hour)))
    p.tap(p.find(id='android:id/minutes'));p.tap(p.find(desc=str(minute)))
    p.tap(p.find(id='android:id/button1'))
    require(f'{hour:02}:{minute:02}' in p.seek(id=PACKAGE+':id/'+identifier).get('text',''),'Native dial committed requested time')
def reminders():
    p.top();p.button('Reminders');p.seek(id=PACKAGE+':id/ReminderEnabledState')

def prepare():
    p.launch();p.top()
    if not state['created']:
        p.button('Add due');p.top();set_entry('Due title, required',state['prefix'])
        set_entry('Amount in Philippine pesos, required','0');p.button('Save due');p.seek(text='Your dues')
        state['created']=True;save()
    reminders();p.top()
    day=p.seek(id=PACKAGE+':id/ReminderDayBefore')
    if day.get('checked')=='true':p.tap(day)
    epoch=int(p.shell('date','+%s'))
    # Clock dial exposes 5-minute minute targets. Leave at least three minutes for setup.
    future=datetime.fromtimestamp(epoch).replace(second=0,microsecond=0)+timedelta(minutes=3)
    future+=timedelta(minutes=(-future.minute)%5)
    time_dial('ReminderTime',future.hour,future.minute)
    p.button('Save and enable reminders')
    nodes=p.ui();allows=[n for n in nodes if n.get('resource-id','').endswith('permission_allow_button')]
    if len(allows)==1:p.tap(allows[0])
    p.top();require(p.seek(id=PACKAGE+':id/ReminderEnabledState').get('text')=='Reminders enabled in Duewise','Temporary natural-delivery schedule enabled')
    state['delivery_time']=future.isoformat();save()
    p.shell('cmd','connectivity','airplane-mode','enable');p.shell('svc','wifi','disable');p.shell('svc','data','disable')
    p.shell('input','keyevent','KEYCODE_BACK');p.shell('input','keyevent','KEYCODE_HOME');p.shell('input','keyevent','KEYCODE_SLEEP')
    print('READY: physical phone natural delivery '+future.strftime('%H:%M')+' Asia/Manila',flush=True)

def observe():
    keys=[k for k in p.notification_keys() if re.search(r'[0-9a-f]{32}/\d{4}-\d{2}-\d{2}/',k)]
    print(json.dumps({'due_notification':bool(keys),'phone_time':p.shell('date','+%H:%M:%S')}),flush=True)
    if keys:
        state['natural_notification']=True;state['notification_keys']=keys;save()
        require(True,'Natural notification appeared while phone backgrounded/offline; no forced worker or foreground refresh')

def cleanup():
    try:
        p.shell('am','force-stop',PACKAGE);p.launch();p.top()
        if state['created']:
            task=p.seek(text=state['prefix'])
            require(len([n for n in p.ui() if n.get('text')==state['prefix']])==1,'Exactly one controlled task row selected for cleanup')
            p.tap(task);require(any(n.get('text')==state['prefix'] for n in p.ui()),'Task identity verified before deletion')
            p.button('Edit due');p.button('Delete due');p.tap(p.find(id='android:id/button1'));p.button('Back to list')
            state['created']=False;save()
        p.top();reminders();p.button('Disable reminders')
        require(state['original_settings']['ReminderEnabledState']=='Reminders are off','Original app opt-in was off')
        for identifier in ['ReminderTime','ReminderQuietStart','ReminderQuietEnd']:
            hour,minute=map(int,re.findall(r'(\d{2}):(\d{2})',state['original_settings'][identifier])[-1])
            p.top();current=p.seek(id=PACKAGE+':id/'+identifier).get('text','')
            if f'{hour:02}:{minute:02}' not in current:time_dial(identifier,hour,minute)
        p.top();day=p.seek(id=PACKAGE+':id/ReminderDayBefore')
        if day.get('checked')!=state['original_settings']['ReminderDayBefore']:p.tap(day)
        p.button('Save schedule');require('saved' in p.seek(id=PACKAGE+':id/ReminderMessage').get('text',''),'Original reminder schedule restored')
        p.button('Android app / battery settings');p.tap(p.seek(text='Power'));p.tap(p.find(text=state['original_power']))
        require(p.find(text=state['original_power']).get('checked')=='true','Original vendor battery policy restored')
        state['power_changed']=False;save()
        p.shell('input','keyevent','KEYCODE_BACK');p.shell('input','keyevent','KEYCODE_BACK');p.top()
        require(p.seek(id=PACKAGE+':id/ReminderEnabledState').get('text')=='Reminders are off','Final opt-in remains off')
        p.shell('input','keyevent','KEYCODE_BACK');p.shell('am','force-stop',PACKAGE);p.launch();p.top()
        p.tap(p.find(desc='Dashboard'));p.top()
        import hashlib
        values=[n.get('text') for n in p.ui() if re.fullmatch(r'\d+ of \d+ saved dues paid|(?:Due today|Overdue|Upcoming): \d+',n.get('text',''))]
        require(hashlib.sha256('|'.join(values).encode()).hexdigest()==state['baseline_hash'],'Final private saved-data membership matches original baseline')
        p.tap(p.find(desc='Dues'))
        if not state['original_permission']:
            p.shell('pm','revoke',PACKAGE,'android.permission.POST_NOTIFICATIONS')
            p.shell('pm','clear-permission-flags',PACKAGE,'android.permission.POST_NOTIFICATIONS','user-set','user-fixed')
        require(not p.notification_keys(),'No app notifications remain')
        state['cleaned']=True;save()
    finally:
        p.shell('settings','put','system','font_scale',state['original_font']);p.shell('cmd','uimode','night',state['original_night'])
        p.shell('cmd','connectivity','airplane-mode','enable' if state['original_airplane']=='1' else 'disable')
        p.shell('svc','wifi','enable' if state['original_wifi']=='1' else 'disable')
        p.shell('svc','data','enable' if state['original_data']=='1' else 'disable')

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('action',choices=['prepare','observe','cleanup'])
    parser.add_argument('--serial',required=True,help='Explicitly selected authorized test phone')
    args=parser.parse_args();p=Device(args.serial)
    if args.action=='prepare':prepare()
    elif args.action=='cleanup':cleanup()
    else:observe()
