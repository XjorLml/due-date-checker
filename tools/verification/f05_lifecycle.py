"""F05 native verification on a fresh workspace-owned DuewiseF05Check AVD.

Requires F05Database.java compiled to /data/local/tmp/f05-database.dex and adb root.
Never use this tool against a phone or an AVD containing personal app data.
"""
import argparse
import base64
from datetime import datetime, timedelta, timezone
import json
from pathlib import Path
import time
import uuid
from android_ui import Device, PACKAGE, require

ROOT = Path(__file__).resolve().parents[2]
STATE = ROOT / 'artifacts/f05-completion/emulator-state.json'
MANILA = timezone(timedelta(hours=8))


class Verification:
    def __init__(self, wait_device=False):
        self.device = Device('emulator-5580')
        deadline = time.monotonic()+180
        while True:
            avd_name = self.device.shell('getprop', 'ro.boot.qemu.avd_name', check=False) or self.device.shell('getprop', 'ro.kernel.qemu.avd_name', check=False)
            if avd_name or not wait_device or time.monotonic() >= deadline: break
            time.sleep(2)
        require(avd_name == 'DuewiseF05Check', 'Workspace-owned isolated AVD selected')
        self.state = json.loads(STATE.read_text()) if STATE.exists() else {'cases': {}}

    def save(self):
        STATE.write_text(json.dumps(self.state, indent=2), encoding='utf-8')

    def artifact(self):
        path = self.device.shell('pm', 'path', PACKAGE).splitlines()[0].split(':', 1)[1]
        return self.device.shell('sha256sum', path).split()[0].upper()

    def db(self, sql, query=False):
        encoded = base64.b64encode(sql.encode()).decode()
        raw = self.device.shell('CLASSPATH=/data/local/tmp/f05-database.dex', 'app_process', '/system/bin',
                                'F05Database', 'query' if query else 'exec', encoded)
        if query:
            # app_process may prefix runtime startup diagnostics.
            return json.loads(next(line for line in raw.splitlines() if line.startswith('[')))
        require(raw.splitlines()[-1] == 'OK', 'Controlled emulator database operation committed')

    def wait_boot(self, timeout=180):
        deadline = time.monotonic()+timeout
        while time.monotonic() < deadline:
            boot_id = self.device.shell('cat', '/proc/sys/kernel/random/boot_id', check=False)
            if (boot_id and boot_id != self.state.get('previous_boot_id')
                    and self.device.shell('getprop', 'sys.boot_completed', check=False) == '1'):
                return
            time.sleep(2)
        raise RuntimeError('Emulator boot not complete; continue waiting with a later invocation')

    def setup(self):
        self.device.launch()
        self.device.seek(text='Your dues')
        require(self.db('PRAGMA user_version', True)[0]['user_version'] == '3', 'Native SQLite initialized at schema 3')
        require(not self.db("SELECT id FROM dues WHERE title NOT LIKE 'F05 native %'", True), 'Isolated database contains no unrelated dues')
        self.device.shell('pm', 'grant', PACKAGE, 'android.permission.POST_NOTIFICATIONS')
        self.device.shell('settings', 'put', 'global', 'auto_time', 0)
        self.state['setup'] = True
        self.save()

    def seed(self, case, ahead):
        require(self.state.get('setup'), 'Isolated test setup complete')
        self.device.shell('am', 'force-stop', PACKAGE)
        now = datetime.fromtimestamp(int(self.device.shell('date', '+%s')), MANILA)
        at = (now+timedelta(minutes=ahead)).replace(second=0, microsecond=0)
        identifier = str(uuid.uuid4())
        stamp = now.astimezone(timezone.utc).strftime('%Y-%m-%dT%H:%M:%S')+'.0000000+00:00'
        self.db("UPDATE reminder_settings SET enabled=1,day_before=0,minute_of_day="+
                str(at.hour*60+at.minute)+",quiet_start=0,quiet_end=0 WHERE id=1")
        self.db("DELETE FROM dues WHERE title LIKE 'F05 native %'")
        self.db("INSERT INTO dues(id,title,amount_centavos,due_date,is_paid,created_at,updated_at,category) VALUES("+
                f"'{identifier}','F05 native {case}',0,'{at:%Y-%m-%d}',0,'{stamp}','{stamp}',1)")
        self.device.launch()
        self.device.seek(text='Your dues')
        rows = self.db(f"SELECT * FROM reminder_occurrences WHERE due_id='{identifier}' AND delivered_at IS NULL", True)
        require(len(rows) == 1, 'Native startup queued one future occurrence')
        self.state['current'] = {'case': case, 'id': identifier, 'key': rows[0]['occurrence_key'],
                                 'at': at.isoformat(), 'receipt': None}
        self.save()
        self.device.shell('input', 'keyevent', 'KEYCODE_HOME')
        self.device.shell('input', 'keyevent', 'KEYCODE_SLEEP')
        print('READY: '+case+' at '+at.strftime('%H:%M')+' Asia/Manila', flush=True)

    def observe(self):
        current = self.state['current']
        rows = self.db("SELECT delivered_at FROM reminder_occurrences WHERE occurrence_key='"+current['key']+"'", True)
        delivered = bool(rows and rows[0]['delivered_at'])
        notified = any(current['key'] in key for key in self.device.notification_keys())
        print(json.dumps({'case': current['case'], 'delivered': delivered, 'notification': notified,
                          'device_time': self.device.shell('date', '+%Y-%m-%dT%H:%M:%S%z')}), flush=True)
        if delivered and notified:
            current['receipt'] = rows[0]['delivered_at']
            self.state['cases'][current['case']] = {'delivery': True, 'receipt': current['receipt'], 'id': current['id'], 'key': current['key'],
                                                   'observed_at': self.device.shell('date', '+%Y-%m-%dT%H:%M:%S%z'), 'artifact': self.artifact()}
            self.save()
            require(True, 'Actual queued notification delivered without a forced job or foreground reconciliation')
        return delivered and notified

    def delivered(self):
        current = self.state['current']
        rows = self.db("SELECT delivered_at FROM reminder_occurrences WHERE occurrence_key='"+current['key']+"'", True)
        return rows[0]['delivered_at'] if rows else None

    def clock(self, epoch):
        self.device.shell('cmd', 'alarm', 'set-time', int(epoch*1000))
        time.sleep(1)

    def restore_clock(self):
        self.clock(time.time())
        self.device.shell('cmd', 'alarm', 'set-timezone', 'Asia/Manila')

    def record(self, case):
        self.state['cases'][case] = {'passed': True, 'artifact': self.artifact()}
        self.save()
        require(True, case)

    def cleanup(self):
        p = self.device
        p.shell('cmd', 'deviceidle', 'unforce')
        p.shell('cmd', 'battery', 'reset')
        self.restore_clock()
        p.shell('settings', 'put', 'global', 'auto_time', 1)
        p.shell('am', 'force-stop', PACKAGE)
        require(not self.db("SELECT id FROM dues WHERE title NOT LIKE 'F05 native %'", True),
                'Cleanup database contains only isolated test fixtures')
        self.db('UPDATE reminder_settings SET enabled=0,day_before=1,minute_of_day=540,quiet_start=1320,quiet_end=480 WHERE id=1')
        self.db("DELETE FROM dues WHERE title LIKE 'F05 native %'")
        self.db('DELETE FROM reminder_occurrences')
        p.launch(); p.seek(text='Your dues'); p.top(); p.button('Reminders'); p.top()
        require(p.seek(id=PACKAGE+':id/ReminderEnabledState').get('text') == 'Reminders are off',
                'Isolated app returned to reminders-off defaults')
        require(not self.db('SELECT id FROM dues', True), 'Isolated task dues removed')
        require(not p.notification_keys(), 'Isolated app notifications cleared')
        p.shell('input', 'keyevent', 'KEYCODE_BACK')
        self.record('cleanup')

    def fast_lifecycle(self):
        require(self.state['cases'].get('natural', {}).get('delivery'), 'Natural delivery already observed before lifecycle mutations')
        receipt = self.delivered()
        self.device.shell('input', 'keyevent', 'KEYCODE_WAKEUP')
        self.device.shell('wm', 'dismiss-keyguard')
        self.device.launch()
        require(self.delivered() == receipt, 'Repeated foreground reconciliation preserves delivered receipt')
        self.record('restart-no-replay')

        self.seed('update', 30)
        old_key = self.state['current']['key']
        apk = ROOT / 'src/DueDateChecker.App/bin/Release/net10.0-android/com.lem.duedatechecker-Signed.apk'
        require('Success' in self.device.adb('install', '-r', apk, timeout=120), 'Same-artifact package replacement succeeds')
        time.sleep(2)
        rows = self.db("SELECT occurrence_key FROM reminder_occurrences WHERE delivered_at IS NULL", True)
        require(len(rows) == 1 and rows[0]['occurrence_key'] == old_key, 'Package-update receiver preserves the pending occurrence without launching UI')
        jobs = self.device.shell('cmd', 'jobscheduler', 'get-job-state', PACKAGE, 5206, check=False)
        require(bool(jobs) and 'unknown' not in jobs, 'Package update rebuilds recovery scheduling')
        self.record('package-update')

        self.device.shell('am', 'force-stop', PACKAGE)
        at = datetime.fromisoformat(self.state['current']['at']).timestamp()
        self.clock(at+60)
        require(not self.delivered() and not self.device.notification_keys(), 'Force-stopped package does not deliver after due instant')
        self.device.launch()
        self.device.seek(text='Your dues')
        require(bool(self.delivered()) and bool(self.device.notification_keys()), 'Reopening catches up an existing queued reminder before expiry')
        self.record('force-stop-reopen')
        self.restore_clock()

        self.seed('clock-zone', 20)
        key = self.state['current']['key']
        self.device.shell('cmd', 'alarm', 'set-timezone', 'Pacific/Honolulu')
        time.sleep(1)
        rows = self.db("SELECT occurrence_key FROM reminder_occurrences WHERE delivered_at IS NULL", True)
        require(len(rows) == 1 and rows[0]['occurrence_key'] == key, 'Device timezone change preserves Manila schedule identity')
        self.clock(datetime.fromisoformat(self.state['current']['at']).timestamp()+30)
        deadline = time.monotonic()+25
        while not self.delivered() and time.monotonic()<deadline:
            time.sleep(1)
        require(bool(self.delivered()), 'System clock-change receiver reconciles queued eligible work without foreground launch')
        self.record('clock-zone-recovery')
        self.restore_clock()

        self.seed('expiry', 20)
        local = datetime.fromisoformat(self.state['current']['at'])
        self.clock((local+timedelta(days=1)).replace(hour=0, minute=5).timestamp())
        time.sleep(2)
        rows = self.db("SELECT occurrence_key FROM reminder_occurrences WHERE due_id='"+self.state['current']['id']+"'", True)
        require(not rows and not self.device.notification_keys(), 'Clock-forward expiry removes pending work without notifying')
        self.record('native-expiry')
        self.restore_clock()

        self.seed('revocation', 20)
        self.device.shell('pm', 'revoke', PACKAGE, 'android.permission.POST_NOTIFICATIONS')
        self.clock(datetime.fromisoformat(self.state['current']['at']).timestamp()+30)
        time.sleep(2)
        require(not self.delivered() and not self.device.notification_keys(), 'Revoked notification permission prevents clock-event delivery')
        self.device.launch(); self.device.seek(text='Your dues'); self.device.top(); self.device.button('Reminders')
        text = self.device.seek(id=PACKAGE+':id/ReminderPermissionState').get('text', '')
        require('not allowed' in text, 'Native permission-revoked status visible')
        require('enabled' in self.device.seek(id=PACKAGE+':id/ReminderEnabledState').get('text', ''), 'Permission revocation preserves saved opt-in preference')
        self.device.shell('pm', 'grant', PACKAGE, 'android.permission.POST_NOTIFICATIONS')
        self.device.shell('input', 'keyevent', 'KEYCODE_HOME'); self.device.launch()
        require(not self.device.notification_keys(), 'Regrant does not newly catch up cleared elapsed occurrences')
        self.record('permission-revoke-regrant')
        self.restore_clock()


    def storage_ui(self):
        self.seed('channel-storage', 60)
        p = self.device
        p.launch(); p.seek(text='Your dues'); p.top(); p.button('Reminders')
        p.button('Android notification settings')
        switches = [n for n in p.ui() if 'Switch' in n.get('class', '')]
        require(bool(switches), 'Native notification-channel switch available')
        p.tap(switches[0]); p.shell('input', 'keyevent', 'KEYCODE_BACK'); p.top()
        require('channel is blocked' in p.seek(id=PACKAGE+':id/ReminderPermissionState').get('text', ''), 'Disabled channel is distinguished from allowed app permission')
        require(not self.db('SELECT occurrence_key FROM reminder_occurrences WHERE delivered_at IS NULL', True), 'Channel-blocked reconciliation clears pending work')
        p.button('Android notification settings')
        switches = [n for n in p.ui() if 'Switch' in n.get('class', '')]
        p.tap(switches[0]); p.shell('input', 'keyevent', 'KEYCODE_BACK'); p.top()
        require('are allowed' in p.seek(id=PACKAGE+':id/ReminderPermissionState').get('text', ''), 'Channel re-enable is reflected on return')
        self.record('channel-disable-enable')

        p.top(); day = p.seek(id=PACKAGE+':id/ReminderDayBefore'); p.tap(day)
        self.db('ALTER TABLE reminder_settings RENAME TO reminder_settings_fault')
        try:
            p.button('Save schedule')
            require("Couldn't save reminder settings" in p.seek(id=PACKAGE+':id/ReminderMessage').get('text', ''), 'Native settings write failure is visible and retryable')
            p.top(); require(p.seek(id=PACKAGE+':id/ReminderDayBefore').get('checked') == 'true', 'Native failed Save retains settings draft')
            require(self.db('SELECT day_before FROM reminder_settings_fault', True)[0]['day_before'] == '0', 'Failed native Save did not commit the draft')
        finally:
            self.db('ALTER TABLE reminder_settings_fault RENAME TO reminder_settings')
        p.button('Save schedule')
        require('saved' in p.seek(id=PACKAGE+':id/ReminderMessage').get('text', ''), 'Native Save retry succeeds after storage restoration')
        require(self.db('SELECT day_before FROM reminder_settings', True)[0]['day_before'] == '1', 'Native retry persists settings')
        self.db('ALTER TABLE reminder_settings RENAME TO reminder_settings_fault')
        try:
            p.button('Refresh / Retry')
            require("Couldn't load reminder settings" in p.seek(id=PACKAGE+':id/ReminderMessage').get('text', ''), 'Native settings read failure reports retry without reset')
        finally:
            self.db('ALTER TABLE reminder_settings_fault RENAME TO reminder_settings')
        p.button('Refresh / Retry')
        require(not p.seek(id=PACKAGE+':id/ReminderMessage').get('text'), 'Native read retry recovers after table restoration')
        require(len(self.db('SELECT id FROM dues WHERE deleted_at IS NULL', True)) == 1, 'Native storage recovery preserves the task due')
        self.record('native-storage-failure-retry')
        p.shell('input', 'keyevent', 'KEYCODE_BACK')
        p.tap(p.seek(text='F05 native channel-storage')); p.button('Edit due')
        paid = p.seek(cls='android.widget.Switch'); p.tap(paid); p.button('Save changes')
        require(self.db("SELECT is_paid FROM dues WHERE id='"+self.state['current']['id']+"'", True)[0]['is_paid'] == '1', 'Real editor paid Save committed')
        require(not self.db('SELECT occurrence_key FROM reminder_occurrences WHERE delivered_at IS NULL', True), 'Post-commit paid Save cancels the native reminder queue')
        p.button('Edit due'); p.tap(p.seek(cls='android.widget.Switch')); p.button('Save changes')
        require(len(self.db('SELECT occurrence_key FROM reminder_occurrences WHERE delivered_at IS NULL', True)) == 1, 'Unpaid Save requeues future work')
        p.button('Edit due'); p.button('Delete due'); p.tap(p.find(id='android:id/button2'))
        require(self.db("SELECT deleted_at FROM dues WHERE id='"+self.state['current']['id']+"'", True)[0]['deleted_at'] is None, 'Native Delete Cancel preserves the queued due')
        p.button('Delete due'); p.tap(p.find(id='android:id/button1')); p.button('Back to list')
        require(self.db("SELECT deleted_at FROM dues WHERE id='"+self.state['current']['id']+"'", True)[0]['deleted_at'] is not None, 'Native confirmed deletion retains a tombstone')
        require(not self.db('SELECT occurrence_key FROM reminder_occurrences WHERE delivered_at IS NULL', True), 'Post-commit deletion clears pending native reminders')
        self.record('native-paid-delete-cancellation')

    def reboot_prepare(self):
        self.seed('reboot', 5)
        self.state['previous_boot_id'] = self.device.shell('cat', '/proc/sys/kernel/random/boot_id')
        self.save()
        self.device.adb('reboot')
        print('READY: isolated emulator reboot requested; do not launch Duewise to observe recovery', flush=True)

    def reboot_check(self):
        self.wait_boot()
        self.device.adb('root')
        time.sleep(2)
        self.device.shell('wm', 'dismiss-keyguard', check=False)
        current = self.state['current']
        rows = self.db("SELECT occurrence_key,delivered_at FROM reminder_occurrences WHERE due_id='"+current['id']+"'", True)
        require(len(rows) == 1 and rows[0]['occurrence_key'] == current['key'], 'Real reboot preserves pending/delivered occurrence identity')
        deadline = time.monotonic()+45
        while True:
            alarms = self.device.shell('dumpsys', 'alarm')
            active = [line for line in alarms.splitlines() if line.strip().startswith('tag=') and PACKAGE+'.REMINDER_ALARM' in line]
            rows = self.db("SELECT occurrence_key,delivered_at FROM reminder_occurrences WHERE due_id='"+current['id']+"'", True)
            if active or rows[0]['delivered_at'] or time.monotonic() >= deadline: break
            time.sleep(2)
        require(bool(active) or bool(rows[0]['delivered_at']), 'Reboot/unlock rebuilds an alarm or delivers due work without app launch')
        foreground = self.device.shell('dumpsys', 'activity', 'activities')
        resumed = [line for line in foreground.splitlines() if 'topResumedActivity=' in line]
        require(not any(PACKAGE in line for line in resumed), 'Reboot verification did not foreground Duewise')
        self.record('real-reboot-unlock-recovery')

    def ui_checks(self):
        p = self.device
        p.launch(); p.seek(text='Your dues'); p.top(); p.button('Reminders'); p.top()
        reached = False
        for _ in range(20):
            p.shell('input', 'keyevent', 'KEYCODE_TAB')
            focused = [n for n in p.ui() if n.get('focused') == 'true']
            if any(n.get('text') == 'Save schedule' for n in focused):
                p.shell('input', 'keyevent', 'KEYCODE_ENTER')
                reached = True
                break
        require(reached, 'Keyboard Tab reaches Save schedule')
        require('saved' in p.seek(id=PACKAGE+':id/ReminderMessage').get('text', ''), 'Keyboard Enter activates Save')
        self.record('keyboard-tab-enter')
        p.shell('settings', 'put', 'system', 'accelerometer_rotation', 0)
        p.shell('settings', 'put', 'system', 'user_rotation', 1)
        try:
            p.ui(); p.top()
            require(bool(p.seek(id=PACKAGE+':id/ReminderBackgroundState').get('text')), 'Landscape settings status reachable at wider layout')
            p.button('Save schedule')
            require('saved' in p.seek(id=PACKAGE+':id/ReminderMessage').get('text', ''), 'Landscape settings actions scroll and Save')
            self.record('landscape-wide-layout')
        finally:
            p.shell('settings', 'put', 'system', 'user_rotation', 0)
            p.shell('settings', 'put', 'system', 'accelerometer_rotation', 1)
        p.shell('input', 'keyevent', 'KEYCODE_BACK'); p.top()
        current = self.state['current']
        title = self.db("SELECT title FROM dues WHERE id='"+current['id']+"'", True)[0]['title']
        p.tap(p.seek(text=title)); p.button('Edit due'); p.top()
        picker = None
        for _ in range(12):
            picker = next((n for n in p.ui() if n.get('class') == 'android.widget.EditText' and n.get('text', '').startswith('Due date, required')), None)
            if picker is not None: break
            p.scroll()
        require(picker is not None, 'Native due-date picker reachable')
        p.tap(picker)
        tomorrow = datetime.fromtimestamp(int(p.shell('date', '+%s')), MANILA).date()+timedelta(days=1)
        nodes = p.ui()
        descriptions = [f'{tomorrow.day:02} {tomorrow:%B %Y}', f'{tomorrow.day} {tomorrow:%B %Y}']
        option = next((n for n in nodes if n.get('content-desc') in descriptions), None)
        require(option is not None, 'Next calendar date exposed by native picker')
        p.tap(option); p.tap(p.find(id='android:id/button1')); p.button('Save changes')
        row = self.db("SELECT due_date FROM dues WHERE id='"+current['id']+"'", True)[0]
        require(row['due_date'] == str(tomorrow), 'Real editor saves the changed due date')
        pending = self.db("SELECT occurrence_key FROM reminder_occurrences WHERE due_id='"+current['id']+"' AND delivered_at IS NULL", True)
        require(len(pending) == 1 and pending[0]['occurrence_key'] != current['key'], 'Date Save replaces the native pending occurrence identity')
        require(not p.notification_keys(), 'Date Save cancels the old displayed notification')
        p.button('Back to list')
        component = p.shell('cmd', 'package', 'resolve-activity', '--brief', PACKAGE).splitlines()[-1]
        p.shell('am', 'start', '-W', '-n', component, '-a', 'com.lem.duedatechecker.OPEN_REMINDER', '--es', 'due_id', current['id'], '--es', 'occurrence_key', current['key'], '-f', '0x24000000')
        require(any(n.get('text') == 'Your dues' for n in p.ui()), 'Obsolete delivered-date intent is ignored safely')
        self.record('date-edit-cancel-stale-tap')



if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['setup', 'seed', 'observe', 'boot', 'fast', 'storage-ui', 'reboot-prepare', 'reboot-check', 'ui-checks', 'cleanup'])
    parser.add_argument('--case', default='natural')
    parser.add_argument('--ahead', type=int, default=3)
    args = parser.parse_args()
    verification = Verification(wait_device=args.action in ['boot', 'reboot-check'])
    if args.action == 'setup': verification.setup()
    elif args.action == 'seed': verification.seed(args.case, args.ahead)
    elif args.action == 'boot': verification.wait_boot()
    elif args.action == 'fast': verification.fast_lifecycle()
    elif args.action == 'storage-ui': verification.storage_ui()
    elif args.action == 'reboot-prepare': verification.reboot_prepare()
    elif args.action == 'reboot-check': verification.reboot_check()
    elif args.action == 'ui-checks': verification.ui_checks()
    elif args.action == 'cleanup': verification.cleanup()
    else: verification.observe()
