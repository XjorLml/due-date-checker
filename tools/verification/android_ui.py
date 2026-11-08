"""Small ADB/UI helper. Always select a device; never print live UI dumps."""
import os
from pathlib import Path
import re
import subprocess
import time
import xml.etree.ElementTree as ET

ADB = Path(os.environ['LOCALAPPDATA']) / 'Android/Sdk/platform-tools/adb.exe'
PACKAGE = 'com.lem.duedatechecker'


class Device:
    def __init__(self, serial):
        self.serial = serial

    def adb(self, *args, check=True, timeout=60):
        result = subprocess.run([str(ADB), '-s', self.serial, *map(str, args)],
                                capture_output=True, timeout=timeout)
        if check and result.returncode:
            raise RuntimeError('ADB operation failed: ' + ' '.join(map(str, args[:2])))
        return result.stdout.decode('utf-8', errors='replace').strip()

    def shell(self, *args, **kwargs):
        return self.adb('shell', *args, **kwargs)

    def ui(self):
        for attempt in range(4):
            raw = self.adb('exec-out', 'uiautomator', 'dump', '/dev/tty')
            start, end = raw.find('<?xml'), raw.find('</hierarchy>')
            if start >= 0 and end >= 0:
                return list(ET.fromstring(raw[start:end + len('</hierarchy>')]).iter('node'))
            time.sleep(0.5)
        raise RuntimeError('UI hierarchy unavailable')

    @staticmethod
    def matches(node, text=None, id=None, desc=None, cls=None):
        return ((text is None or node.get('text') == text)
                and (id is None or node.get('resource-id') == id)
                and (desc is None or node.get('content-desc') == desc)
                and (cls is None or node.get('class') == cls))

    def find(self, **selector):
        matches = [n for n in self.ui() if self.matches(n, **selector)]
        if len(matches) != 1:
            raise RuntimeError('Expected one verification control: ' + str(selector))
        return matches[0]

    def tap(self, node):
        x1, y1, x2, y2 = map(int, re.findall(r'\d+', node.get('bounds', '')))
        if x2 <= x1 or y2 <= y1:
            raise RuntimeError('Verification control has no visible bounds')
        self.shell('input', 'tap', (x1+x2)//2, (y1+y2)//2)

    def size(self):
        display = re.search(r'\bcur=(\d+)x(\d+)', self.shell('dumpsys', 'window', 'displays'))
        if display:
            return tuple(map(int, display.groups()))
        values = re.findall(r'(\d+)x(\d+)', self.shell('wm', 'size'))
        return tuple(map(int, values[-1]))

    def scroll(self, down=True):
        width, height = self.size()
        # Slow, overlapping gestures avoid flinging past small targets at 200% text.
        a, b = (0.8, 0.55) if down else (0.3, 0.8)
        self.shell('input', 'swipe', width//2, int(height*a), width//2, int(height*b), 600)

    def seek(self, **selector):
        for _ in range(20):
            matches = [n for n in self.ui() if self.matches(n, **selector)]
            for node in matches:
                values = list(map(int, re.findall(r'\d+', node.get('bounds', ''))))
                if len(values) == 4 and values[1] >= 80 and values[3]-values[1] >= 25:
                    return node
            self.scroll()
        raise RuntimeError('Verification control not reachable: ' + str(selector))

    def button(self, text):
        self.tap(self.seek(text=text, cls='android.widget.Button'))

    def top(self):
        for _ in range(5):
            self.scroll(False)

    def launch(self):
        self.shell('input', 'keyevent', 'KEYCODE_WAKEUP')
        self.shell('wm', 'dismiss-keyguard', check=False)
        component = self.shell('cmd', 'package', 'resolve-activity', '--brief', PACKAGE).splitlines()[-1]
        if not component.startswith(PACKAGE+'/'):
            raise RuntimeError('Duewise activity unavailable')
        self.shell('am', 'start', '-W', '-n', component)

    def notification_keys(self):
        return [key for key in self.shell('cmd', 'notification', 'list').splitlines() if PACKAGE in key]


def require(value, label):
    if not value:
        raise AssertionError(label)
    print('PASS: '+label, flush=True)
