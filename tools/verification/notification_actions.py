"""N1/N2 checks on the task-owned API-36 emulator only; never targets a phone.

Compile/push F05Database.java as documented in README.md first. Release build,
rooted emulator and an initialized empty Duewise database are prerequisites.
"""
import argparse
import base64
from datetime import datetime, timedelta, timezone
import json
import re
from pathlib import Path
import time
import uuid
from android_ui import Device, PACKAGE, require

ROOT = Path(__file__).resolve().parents[2]
STATE = ROOT / 'artifacts/notification-check/state.json'
MANILA = timezone(timedelta(hours=8))


class Check:
    def __init__(self):
        self.device = Device('emulator-5580')
        name = self.device.shell('getprop', 'ro.boot.qemu.avd_name')
        require(name == 'DuewiseNotificationsCheck', 'Isolated notification-check emulator selected')

    def db(self, sql, query=False):
        encoded = base64.b64encode(sql.encode()).decode()
        raw = self.device.shell('CLASSPATH=/data/local/tmp/f05-database.dex', 'app_process', '/system/bin',
                                'F05Database', 'query' if query else 'exec', encoded)
        if query:
            return json.loads(next(line for line in raw.splitlines() if line.startswith('[')))
        require(raw.splitlines()[-1] == 'OK', 'Sample database operation committed')

    @staticmethod
    def top(node):
        return int(re.findall(r'\d+', node.get('bounds'))[1])

    def expand_sample(self, prefix):
        for _ in range(3):
            nodes = self.device.ui()
            if self.payment_target(nodes) is not None: return nodes
            title = next((n for n in nodes if n.get('text', '').startswith(prefix)), None)
            expands = [n for n in nodes if n.get('content-desc') == 'Expand']
            if not expands: return nodes
            target = min(expands, key=lambda n: abs(self.top(n)-self.top(title))) if title is not None else expands[0]
            self.device.tap(target)
            nodes = self.device.ui()
            if any(n.get('text', '').lower() == 'mark as paid' for n in nodes): return nodes
        return nodes

    def payment_target(self, nodes):
        title = next((n for n in nodes if n.get('text','').startswith('N12 Sample rent')), None)
        if title is None: return None
        buttons = [n for n in nodes if n.get('text','').lower() == 'mark as paid' and self.top(n) > self.top(title)]
        for button in sorted(buttons,key=self.top):
            if not any(n.get('text','').startswith('N12 Sample task') and self.top(title) < self.top(n) < self.top(button) for n in nodes):
                return button
        return None

    def seed(self):
        require(not STATE.exists(), 'No prior task fixtures will be overwritten')
        require(not self.db('SELECT id FROM dues', True), 'Fresh database contains no unrelated dues')
        now = datetime.fromtimestamp(int(self.device.shell('date', '+%s')), timezone.utc)
        local = now.astimezone(MANILA)
        at = now.replace(second=0, microsecond=0)
        stamp = at.strftime('%Y-%m-%dT%H:%M:%S') + '.0000000+00:00'
        date = local.date().isoformat()
        ids = [str(uuid.uuid4()), str(uuid.uuid4())]
        self.db(f'UPDATE reminder_settings SET enabled=1, day_before=0, minute_of_day={local.hour*60+local.minute}, quiet_start=0, quiet_end=0 WHERE id=1')
        for i, ident in enumerate(ids):
            title = 'N12 Sample rent' if i == 0 else 'N12 Sample task'
            amount, category, repeat, day = ('123450', 1, 2, local.day) if i == 0 else ('NULL', 2, 0, 'NULL')
            self.db(f"INSERT INTO dues(id,title,amount_centavos,due_date,notes,created_at,updated_at,category,repeat_rule,repeat_day) VALUES('{ident}','{title}',{amount},'{date}','N12 NEVER DISPLAY',{stamp!r},{stamp!r},{category},{repeat},{day})")
            key = f'{ident.replace("-", "")}/{date}/0/{int(at.timestamp()*1000)}'
            self.db(f"INSERT INTO reminder_occurrences(occurrence_key,due_id,due_date,scheduled_at) VALUES('{key}','{ident}','{date}','{stamp}')")
        STATE.parent.mkdir(parents=True, exist_ok=True)
        STATE.write_text(json.dumps({'ids': ids, 'date': date}, indent=2))
        self.device.launch()

    def observe(self):
        state = json.loads(STATE.read_text())
        rows = self.db('SELECT delivered_at FROM reminder_occurrences', True)
        require(sum(row['delivered_at'] is not None for row in rows) == 2, 'Two real delivered receipts')
        # All data in this fresh emulator belongs to this task. Keep raw dump local/in memory.
        dump = self.device.shell('dumpsys', 'notification', '--noredact')
        require('N12 NEVER DISPLAY' not in dump, 'Notes absent from native notification payloads')
        require('N12 Sample rent is due today' in dump and 'Task, no amount' in dump, 'Native shade content populated')
        require('You have 2 dues today' in dump, 'Generic grouped summary counts two dues')
        self.device.shell('cmd', 'statusbar', 'expand-notifications')
        nodes = self.expand_sample('N12 Sample rent')
        require(any('N12 Sample rent' in n.get('text', '') for n in nodes), 'Real notification shade shows sample due')
        require(any(n.get('text', '').lower() == 'mark as paid' for n in nodes), 'Native payment action visible')

    def pay_undo(self):
        state = json.loads(STATE.read_text()); ident = state['ids'][0]
        self.device.shell('cmd', 'statusbar', 'expand-notifications')
        nodes = self.expand_sample('N12 Sample rent')
        # Expand the rent child if the group is collapsed. UI shape varies across Android skins.
        target = self.payment_target(nodes)
        require(target is not None, 'Payment action available on an expanded child')
        self.device.tap(target)
        # Android may collapse the shade after executing an authenticated action.
        self.device.shell('cmd', 'statusbar', 'expand-notifications')
        # Locate Undo at once, without a slow database query eating its 10-second window.
        undo = None
        for _ in range(2):
            nodes = self.device.ui()
            undo = next((n for n in nodes if n.get('text', '').lower() == 'undo'), None)
            if undo is not None: break
            feedback = next((n for n in nodes if 'marked paid' in n.get('text','')), None)
            expands = [n for n in nodes if n.get('content-desc') == 'Expand']
            if expands:
                expand = min(expands,key=lambda n: abs(self.top(n)-self.top(feedback))) if feedback is not None else expands[0]
                self.device.tap(expand)
        require(undo is not None, 'Payment confirmation offers Undo')
        self.device.tap(undo)
        time.sleep(1)
        rows = self.db(f"SELECT id,is_paid,repeat_source_id FROM dues WHERE id='{ident}' OR repeat_source_id='{ident}'", True)
        require(len(rows) == 2 and all(r['is_paid'] == '0' for r in rows), 'Undo restores unpaid and retains exactly one next occurrence')

    def cleanup(self):
        state = json.loads(STATE.read_text()); ids = state['ids']
        quoted = ','.join("'"+str(uuid.UUID(i))+"'" for i in ids)
        require(not self.db(f'SELECT id FROM dues WHERE id NOT IN ({quoted}) AND (repeat_source_id IS NULL OR repeat_source_id NOT IN ({quoted}))', True), 'Cleanup contains only task-created data')
        self.db('UPDATE reminder_settings SET enabled=0,day_before=1,minute_of_day=540,quiet_start=1320,quiet_end=480 WHERE id=1')
        self.db(f'DELETE FROM dues WHERE id IN ({quoted}) OR repeat_source_id IN ({quoted})')
        self.db('DELETE FROM reminder_occurrences')
        self.device.launch()
        require(not self.db('SELECT id FROM dues', True), 'Task sample dues removed')
        for _ in range(10):
            if not self.device.notification_keys(): break
            time.sleep(1)
        require(not self.device.notification_keys(), 'Task notifications removed after disabling reminders')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('case', choices=['seed', 'observe', 'pay-undo', 'cleanup'])
    args = parser.parse_args()
    check = Check()
    getattr(check, args.case.replace('-', '_'))()
