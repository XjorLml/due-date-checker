"""F04 checks on an explicitly selected phone with an existing local checkpoint.

Never edits dues. UI XML remains in memory; only Settings screenshots are saved.
Create/review artifacts/f04-preferences/phone-state.json before upgrading the APK.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import struct
import zlib
from android_ui import ADB, Device, PACKAGE, require

ROOT = Path(__file__).resolve().parents[2]
DIRECTORY = ROOT / 'artifacts/f04-preferences'


def canvas_pixel(png):
    """Read one canvas pixel from adb's non-interlaced RGB/RGBA PNG, without dependencies."""
    require(png[:8] == b'\x89PNG\r\n\x1a\n', 'Screenshot is a PNG')
    offset = 8
    compressed = bytearray()
    while offset < len(png):
        length = struct.unpack('>I', png[offset:offset+4])[0]
        kind, data = png[offset+4:offset+8], png[offset+8:offset+8+length]
        if kind == b'IHDR': width, height, depth, color, _, _, interlaced = struct.unpack('>IIBBBBB', data)
        if kind == b'IDAT': compressed.extend(data)
        offset += 12+length
    require(depth == 8 and color in [2, 6] and interlaced == 0, 'Native screenshot format supported')
    bpp = 3 if color == 2 else 4
    stride = width*bpp
    raw = zlib.decompress(compressed)
    previous = bytearray(stride)
    for y in range(height//2+1):
        at = y*(stride+1)
        method, row = raw[at], bytearray(raw[at+1:at+1+stride])
        for x in range(stride):
            left, above = row[x-bpp] if x >= bpp else 0, previous[x]
            corner = previous[x-bpp] if x >= bpp else 0
            if method == 0: continue
            if method == 1: predictor = left
            elif method == 2: predictor = above
            elif method == 3: predictor = (left+above)//2
            elif method == 4:
                p = left+above-corner
                distances = [abs(p-left), abs(p-above), abs(p-corner)]
                predictor = [left, above, corner][distances.index(min(distances))]
            else: raise ValueError('Unknown PNG filter')
            row[x] = (row[x]+predictor)&255
        previous = row
    return tuple(row[8*bpp:8*bpp+3])


class Verification:
    def __init__(self, serial):
        self.p = Device(serial)
        self.file = DIRECTORY / 'phone-state.json'
        self.state = json.loads(self.file.read_text(encoding='utf-8'))
        path = self.p.shell('pm', 'path', PACKAGE).splitlines()[0].split(':', 1)[1]
        self.artifact = self.p.shell('sha256sum', path).split()[0].upper()

    def record(self, case):
        self.state.setdefault('cases', {})[case] = {'passed': True, 'artifact': self.artifact}
        self.file.write_text(json.dumps(self.state, indent=2), encoding='utf-8')
        require(True, case)

    def settings(self):
        self.p.top(); self.p.button('Settings'); self.p.top()
        self.p.seek(id=PACKAGE+':id/SettingsTheme')

    def choice(self, identifier, value):
        self.p.top(); self.p.tap(self.p.seek(id=PACKAGE+':id/'+identifier))
        self.p.tap(self.p.find(text=value))
        require(value in self.p.seek(id=PACKAGE+':id/'+identifier).get('text', ''), 'Native preference selection committed to draft')

    def save(self):
        self.p.button('Save preferences')
        require(self.p.seek(id=PACKAGE+':id/SettingsMessage').get('text') == 'Preferences saved.', 'Native preference Save succeeds')

    def snapshot(self, name, dark):
        self.p.top()
        require(bool(self.p.seek(id=PACKAGE+':id/SettingsTheme').get('text')), 'Settings-only screenshot boundary verified')
        result = subprocess.run([str(ADB), '-s', self.p.serial, 'exec-out', 'screencap', '-p'], capture_output=True, timeout=30, check=True)
        expected = (11, 18, 32) if dark else (248, 250, 252)
        require(canvas_pixel(result.stdout) == expected, 'App canvas follows the expected appearance')
        (DIRECTORY / (name+'.png')).write_bytes(result.stdout)

    def baseline(self):
        p = self.p
        p.tap(p.find(desc='Dashboard')); p.top()
        values = [n.get('text') for n in p.ui() if re.fullmatch(r'\d+ of \d+ saved dues paid|(?:Due today|Overdue|Upcoming): \d+', n.get('text', ''))]
        require(bool(values) and hashlib.sha256('|'.join(values).encode()).hexdigest() == self.state['baseline_hash'],
                'Private saved-data summary matches the original baseline')
        p.tap(p.find(desc='Dues'))

    def initial(self):
        self.p.launch(); self.p.top(); self.baseline(); self.settings()
        require('System' in self.p.seek(id=PACKAGE+':id/SettingsTheme').get('text', ''), 'Upgrade defaults to System')
        require('Monday' in self.p.seek(id=PACKAGE+':id/SettingsWeekStart').get('text', ''), 'Upgrade defaults to Monday')
        require(not any(n.get('content-desc') in ['Dues', 'Dashboard'] for n in self.p.ui()), 'Settings hides root tabs')
        self.record('upgrade-defaults-data')
        self.choice('SettingsTheme', 'Light'); self.choice('SettingsWeekStart', 'Sunday')
        self.p.shell('input', 'keyevent', 'KEYCODE_BACK'); self.settings()
        require('System' in self.p.seek(id=PACKAGE+':id/SettingsTheme').get('text', ''), 'Back discards unsaved appearance')
        require('Monday' in self.p.seek(id=PACKAGE+':id/SettingsWeekStart').get('text', ''), 'Back discards unsaved week start')
        self.record('draft-back')
        self.choice('SettingsTheme', 'Dark'); self.choice('SettingsWeekStart', 'Sunday')
        self.p.button('Reminders'); self.p.top()
        require(self.p.seek(id=PACKAGE+':id/ReminderEnabledState').get('text') == self.state['original_reminders']['ReminderEnabledState'], 'Settings-to-Reminders does not opt in')
        self.p.shell('input', 'keyevent', 'KEYCODE_BACK'); self.p.top()
        require('Dark' in self.p.seek(id=PACKAGE+':id/SettingsTheme').get('text', ''), 'Returning from Reminders preserves appearance draft')
        self.p.shell('input', 'keyevent', 'KEYCODE_HOME'); self.p.launch(); self.p.top()
        require('Sunday' in self.p.seek(id=PACKAGE+':id/SettingsWeekStart').get('text', ''), 'Resume preserves week-start draft')
        self.save(); self.snapshot('dark-saved', True)
        self.record('save-reminders-resume')

    def calendar_restart(self):
        p = self.p
        p.shell('input', 'keyevent', 'KEYCODE_BACK'); p.top()
        p.shell('cmd', 'connectivity', 'airplane-mode', 'enable'); p.shell('svc', 'wifi', 'disable'); p.shell('svc', 'data', 'disable')
        p.shell('am', 'force-stop', PACKAGE); p.launch(); self.settings()
        require('Dark' in p.seek(id=PACKAGE+':id/SettingsTheme').get('text', ''), 'Saved Dark survives offline process restart')
        require('Sunday' in p.seek(id=PACKAGE+':id/SettingsWeekStart').get('text', ''), 'Saved Sunday survives offline process restart')
        self.snapshot('dark-offline-restart', True)
        p.shell('input', 'keyevent', 'KEYCODE_BACK'); p.tap(p.find(desc='Calendar')); p.top()
        require(p.seek(id=PACKAGE+':id/CalendarWeekStart').get('text', '').startswith('Sunday first'), 'Calendar uses saved Sunday caption')
        p.button('Next')
        # Choose the first real day exposed in the viewed month, without reading dues.
        p.seek(id=PACKAGE+':id/CalendarMonth')
        dates = [n for n in p.ui() if re.fullmatch(re.escape(PACKAGE)+r':id/CalendarDay-\d{4}-\d{2}-\d{2}', n.get('resource-id', ''))]
        require(bool(dates), 'Native calendar date buttons available')
        first = min(dates, key=lambda n:n.get('resource-id'))
        selected_id = first.get('resource-id')
        p.tap(first)
        month = p.seek(id=PACKAGE+':id/CalendarMonth').get('text')
        sunday_bounds = p.seek(id=selected_id).get('bounds')
        p.shell('input', 'keyevent', 'KEYCODE_BACK'); self.settings()
        self.choice('SettingsWeekStart', 'Monday'); self.save()
        p.shell('input', 'keyevent', 'KEYCODE_BACK'); p.tap(p.find(desc='Calendar')); p.top()
        require(p.seek(id=PACKAGE+':id/CalendarMonth').get('text') == month, 'Week-start Save retains the viewed month')
        require(p.seek(id=PACKAGE+':id/CalendarWeekStart').get('text', '').startswith('Monday first'), 'Calendar caption updates with the saved Monday choice')
        moved = p.seek(id=selected_id)
        require(moved.get('bounds') != sunday_bounds, 'Calendar repositions the same date for the changed week start')
        require('selected' in moved.get('content-desc', ''), 'Week-start Save retains selected date')
        p.shell('input', 'keyevent', 'KEYCODE_BACK'); self.baseline()
        self.record('offline-restart-cached-calendar')

    def themes(self):
        p = self.p
        self.settings(); self.choice('SettingsTheme', 'System'); self.save()
        for mode, dark in [('no', False), ('yes', True)]:
            p.shell('cmd', 'uimode', 'night', mode); p.ui()
            self.snapshot('system-'+mode, dark)
        self.choice('SettingsTheme', 'Light'); self.save()
        self.snapshot('override-light-os-dark', False)
        self.record('system-and-override')
        p.shell('input', 'keyevent', 'KEYCODE_BACK')
        for font in ['1.0', '2.0']:
            p.shell('settings', 'put', 'system', 'font_scale', font)
            p.shell('am', 'force-stop', PACKAGE); p.launch(); self.settings()
            for theme in ['Light', 'Dark']:
                self.choice('SettingsTheme', theme); self.save()
                self.snapshot('settings-'+theme.lower()+'-'+font, theme == 'Dark')
                p.button('Reminders'); p.top()
                require(p.seek(id=PACKAGE+':id/ReminderEnabledState').get('text') == 'Reminders are off', 'Large-text reminder action remains reachable without opt-in')
                p.shell('input', 'keyevent', 'KEYCODE_BACK')
                self.record('theme-text-'+theme.lower()+'-'+font)
            p.shell('input', 'keyevent', 'KEYCODE_BACK')

    def cleanup(self):
        p = self.p
        # The checkpoint precedes the first F04 upgrade: defaults were System/Monday.
        try:
            p.shell('am', 'force-stop', PACKAGE); p.launch(); self.settings()
            self.choice('SettingsTheme', 'System'); self.choice('SettingsWeekStart', 'Monday'); self.save()
            p.shell('input', 'keyevent', 'KEYCODE_BACK'); self.baseline()
            p.top(); p.button('Reminders'); p.top()
            for identifier, original in self.state['original_reminders'].items():
                require(p.seek(id=PACKAGE+':id/'+identifier).get('text') == original, 'Original reminder setting unchanged')
            p.shell('input', 'keyevent', 'KEYCODE_BACK')
        finally:
            p.shell('settings', 'put', 'system', 'font_scale', self.state['original_font'])
            mode = 'yes' if 'yes' in self.state['original_night'] else 'no' if 'no' in self.state['original_night'] else 'auto'
            p.shell('cmd', 'uimode', 'night', mode)
            for command, key in [('wifi','original_wifi'), ('data','original_data')]:
                p.shell('svc', command, 'enable' if self.state[key] == '1' else 'disable')
            p.shell('cmd', 'connectivity', 'airplane-mode', 'enable' if self.state['original_airplane'] == '1' else 'disable')
        require(not p.notification_keys(), 'No test notification remains')
        self.record('cleanup-restoration')

    def final_calendar(self):
        self.p.shell('settings', 'put', 'system', 'font_scale', '1.0')
        self.p.shell('am', 'force-stop', PACKAGE); self.p.launch(); self.settings()
        self.choice('SettingsTheme', 'Dark'); self.choice('SettingsWeekStart', 'Sunday'); self.save()
        self.calendar_restart()
        self.record('final-calendar-regression')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['initial', 'calendar-restart', 'themes', 'cleanup', 'final-calendar'])
    parser.add_argument('--serial', required=True)
    args = parser.parse_args()
    verify = Verification(args.serial)
    {'initial': verify.initial, 'calendar-restart': verify.calendar_restart, 'themes': verify.themes,
     'cleanup': verify.cleanup, 'final-calendar': verify.final_calendar}[args.action]()
