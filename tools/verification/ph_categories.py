"""Native preset-category checks. Only the previously task-owned emulator is allowed.
Requires the existing F05Database.java helper at /data/local/tmp/f05-database.dex and adb root.
"""
import base64
from datetime import datetime, timezone
import json
from pathlib import Path
import sys
import time
import uuid
from android_ui import Device, PACKAGE, require

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'artifacts/ph-categories-check'
STATE = OUT / 'state.json'


class Check:
    def __init__(self):
        self.device = Device('emulator-5580')
        require(self.device.shell('getprop', 'ro.boot.qemu.avd_name') == 'DuewiseNotificationsCheck', 'Task-owned emulator selected')

    def db(self, sql, query=False):
        encoded = base64.b64encode(sql.encode()).decode()
        raw = self.device.shell('CLASSPATH=/data/local/tmp/f05-database.dex', 'app_process', '/system/bin',
                                'F05Database', 'query' if query else 'exec', encoded)
        if query: return json.loads(next(line for line in raw.splitlines() if line.startswith('[')))
        require(raw.splitlines()[-1] == 'OK', 'Controlled task database operation committed')

    def tap_id(self, ident): self.device.tap(self.device.seek(id=PACKAGE+':id/'+ident))

    def migration(self):
        require(not STATE.exists(), 'No prior category checkpoint overwritten')
        require(not self.db('SELECT id FROM dues',True), 'Emulator database contains no unrelated dues')
        self.device.shell('am','force-stop',PACKAGE)
        ids=[str(uuid.uuid4()),str(uuid.uuid4())]
        stamp='2026-11-01T00:00:00.0000000+00:00'
        self.db('ALTER TABLE dues DROP COLUMN category_id')
        self.db('PRAGMA user_version=5')
        for i,ident in enumerate(ids):
            self.db(f"INSERT INTO dues(id,title,amount_centavos,due_date,notes,is_paid,created_at,updated_at,deleted_at,category,repeat_rule,repeat_day) VALUES('{ident}','NPH legacy {i}',{'0' if i==0 else 'NULL'},'2026-11-30','NPH original note',{i},'{stamp}','{stamp}',{'NULL' if i==0 else repr(stamp)},{1 if i==0 else 2},{2 if i==0 else 0},{30 if i==0 else 'NULL'})")
        columns='rowid,id,title,amount_centavos,currency,due_date,notes,is_paid,timezone,created_at,updated_at,deleted_at,category,repeat_rule,repeat_day,repeat_source_id'
        before=self.db('SELECT '+columns+' FROM dues ORDER BY id',True)
        STATE.write_text(json.dumps({'ids':ids,'before':before},indent=2))
        self.device.launch()
        require(self.db('PRAGMA user_version',True)[0]['user_version']=='6','Actual app upgraded schema 5 to 6')
        require(self.db('SELECT '+columns+' FROM dues ORDER BY id',True)==before,'Migration preserved legacy stored values/timestamps/tombstone')
        require(all(row['category_id']=='ph.other.general' for row in self.db('SELECT category_id FROM dues',True)),'Legacy detailed categories default to Other without guessing')

    def search(self, query):
        self.tap_id('PHCategorySearch')
        self.device.shell('input','keyevent','KEYCODE_MOVE_END')
        self.device.shell('input','keyevent','--longpress','KEYCODE_DEL')
        # Fresh searches are opened for each case; test queries are plain controlled ASCII.
        self.device.shell('input','text',query)

    def picker(self):
        self.device.tap(self.device.find(desc='Add due'))
        self.tap_id('ChoosePHCategory')
        require(any(n.get('text')=='53 categories' for n in self.device.ui()),'Picker shows 53 grouped presets')
        self.search('KURYENTE')
        self.device.tap(self.device.find(desc='Electricity, Utilities & Telecommunications'))
        require(self.device.find(id=PACKAGE+':id/DetailedCategoryValue').get('text')=='Electricity','Tagalog search selects Electricity')
        require(self.device.find(id=PACKAGE+':id/DueTypeRow').get('text')=='Type: Bill','Detailed category does not change due type')
        self.tap_id('EditorCancel')
        require(any(n.get('text')=='Discard changes?' for n in self.device.ui()),'Category-only change triggers dirty-exit protection')
        self.device.tap(self.device.find(text='Keep editing'))
        self.tap_id('ChoosePHCategory'); self.search('SSS'); self.tap_id('PHCategoryCancel')
        require(self.device.find(id=PACKAGE+':id/DetailedCategoryValue').get('text')=='Electricity','Cancelling picker keeps prior draft selection')
        # Fill the actual title and amount controls; category remains selected across keyboard changes.
        self.device.top()
        self.device.tap(self.device.find(text='Due title, required',cls='android.widget.EditText'))
        self.device.shell('input','text','NPH%scategory%ssample'); self.device.shell('input','keyevent','KEYCODE_BACK')
        self.tap_id('DueAmount'); self.device.shell('input','text','42.50'); self.device.shell('input','keyevent','KEYCODE_BACK')
        self.tap_id('EditorSave')
        rows=self.db("SELECT id,title,amount_centavos,category,category_id FROM dues WHERE title='NPH category sample'",True)
        require(len(rows)==1 and rows[0]['category_id']=='ph.utilities.electricity' and rows[0]['category']=='1' and rows[0]['amount_centavos']=='4250','Actual editor Save persists category, type and exact amount')
        self.device.shell('am','force-stop',PACKAGE);self.device.launch()
        require(self.db("SELECT category_id FROM dues WHERE title='NPH category sample'",True)[0]['category_id']=='ph.utilities.electricity','Category survives process restart')

    def cleanup(self):
        require(not self.db("SELECT id FROM dues WHERE title NOT LIKE 'NPH %'",True),'Cleanup contains only category task fixtures')
        self.db("DELETE FROM dues WHERE title LIKE 'NPH %'")
        self.db('DELETE FROM reminder_occurrences')
        self.device.shell('settings','put','system','font_scale','1.0')
        self.device.shell('wm','size','reset');self.device.shell('cmd','uimode','night','auto')
        self.device.launch()
        require(not self.db('SELECT id FROM dues',True),'Category task rows removed')

    def policies(self):
        require(not self.db("SELECT id FROM dues WHERE title NOT LIKE 'NPH %'",True),'Amount-policy checks contain only task fixtures')
        d=self.device
        d.shell('am','force-stop',PACKAGE); d.launch(); d.ui()
        d.tap(d.find(desc='Add due')); self.tap_id('ChoosePHCategory'); self.search('KURYENTE')
        d.tap(d.find(desc='Electricity, Utilities & Telecommunications'))
        d.top(); d.tap(d.find(text='Due title, required',cls='android.widget.EditText'))
        d.shell('input','text','NPH%spolicy%sTask'); d.shell('input','keyevent','KEYCODE_BACK')
        for type_name in ('Bill','Subscription'):
            if type_name!='Bill':
                self.tap_id('DueCategory'); d.tap(d.find(text=type_name))
            self.tap_id('EditorSave')
            require(any(n.get('text','').startswith('Enter an amount for this '+type_name) for n in d.ui()),type_name+' still requires an amount with Electricity selected')
            d.shell('input','keyevent','KEYCODE_BACK')
        self.tap_id('DueCategory'); d.tap(d.find(text='Task')); self.tap_id('EditorSave'); d.ui()
        rows=self.db("SELECT category,category_id,amount_centavos FROM dues WHERE title='NPH policy Task'",True)
        require(len(rows)==1 and rows[0]['category']=='2' and rows[0]['category_id']=='ph.utilities.electricity' and rows[0]['amount_centavos'] is None,'Task saves Electricity with absent amount')
        d.tap(d.find(desc='Add due')); self.tap_id('DueCategory'); d.tap(d.find(text='Other'))
        self.tap_id('ChoosePHCategory'); self.search('KURYENTE'); d.tap(d.find(desc='Electricity, Utilities & Telecommunications'))
        d.top(); d.tap(d.find(text='Due title, required',cls='android.widget.EditText'))
        d.shell('input','text','NPH%spolicy%sOther'); d.shell('input','keyevent','KEYCODE_BACK')
        self.tap_id('EditorSave'); d.ui()
        rows=self.db("SELECT category,category_id,amount_centavos FROM dues WHERE title='NPH policy Other'",True)
        require(len(rows)==1 and rows[0]['category']=='0' and rows[0]['category_id']=='ph.utilities.electricity' and rows[0]['amount_centavos'] is None,'Other saves Electricity with absent amount')


if __name__=='__main__':
    if len(sys.argv)!=2 or sys.argv[1] not in ('migration','picker','policies','cleanup'): raise SystemExit('Expected migration, picker, policies or cleanup')
    OUT.mkdir(parents=True,exist_ok=True)
    getattr(Check(),sys.argv[1])()
