// Builds a small throwaway repository so tests never touch the real records.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';

export function makeFixture() {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), 'devdash-'));
  const write = (relative, text) => {
    const file = path.join(root, relative);
    fs.mkdirSync(path.dirname(file), { recursive: true });
    fs.writeFileSync(file, text);
    return file;
  };
  write('docs/PRD.md', [
    '# Product', '', '## Approved requirements', '',
    '| ID | Requirement | Slice | Acceptance |', '|---|---|---|---|',
    '| R01 | Install the app. | V1.0 | Launches from the launcher. |',
    '| R02 | Create a [due](x.md). | V1.1 | The due is listed. |', '',
    '## Business rules', '', '| R09 | Not a requirement row in this section | x | y |', '',
  ].join('\n'));
  write('docs/delivery/STATUS.md', '# Status\n\n## Current focus\n\n**Alpha slice, 2026-11-10:** in progress.\n');
  write('specs/alpha/spec.md', [
    '# Alpha feature', '', 'Status: proposed for testing.', '',
    '## Acceptance scenarios', '',
    '| ID / requirement | Scenario | Boundary |', '|---|---|---|',
    '| AL01 / R02 | Save a draft | Unit |',
    '| **AL02** | Cancel keeps data | Native |',
    '| R02 | A reference row, not a scenario | - |',
    '| AL01 | Duplicate ID is ignored | - |', '',
    '```text', '| AL99 | inside a code fence |', '```', '',
  ].join('\n'));
  write('specs/alpha/tasks.md', '# Alpha tasks\n\n## Build\n\n- [x] Write the model\n- [ ] Write the page\n\n```\n- [ ] fenced, ignored\n```\n');
  write('specs/beta/spec.md', '# Beta tooling\n\n| ID | Scenario |\n|---|---|\n| BE01 | Runs locally |\n');
  write('specs/beta/tasks.md', '- [x] Done\n');
  write('proto/app/index.html', '<!doctype html><title>Proto</title><script src="app.js"></script>');
  write('proto/app/app.js', 'console.log("v1");\n');
  write('proto/app/tests/hidden.js', 'secret test\n');
  write('proto/app/.qa/shot.txt', 'qa output\n');
  write('secret.txt', 'top secret\n');
  write('tools/developer-dashboard/records/mappings.json', JSON.stringify({
    schemaVersion: 1,
    phases: ['Core', 'Tooling'],
    features: {
      R01: { phase: 'Core', scope: 'approved', unavailable: { kind: 'no-ui', note: 'Device only.' }, status: { value: 'in_review', source: 'docs/delivery/STATUS.md', seeded: '2026-11-10', note: 'Seeded.' } },
      R02: { phase: 'Core', scope: 'approved', prototype: { screen: 'add' } },
      alpha: { phase: 'Core', scope: 'approved', prototype: { screen: 'add', overlay: 'category-picker' } },
      beta: { phase: 'Tooling', scope: 'tooling', unavailable: { kind: 'no-ui', note: 'Tooling.' } },
    },
  }, null, 2));
  const configPath = write('dashboard.config.json', JSON.stringify({
    schemaVersion: 1, project: 'Fixture',
    requirements: { file: 'docs/PRD.md', section: 'Approved requirements' },
    specsDir: 'specs',
    statusDocument: { file: 'docs/delivery/STATUS.md', focusSection: 'Current focus' },
    preview: { entry: 'proto/app/index.html', roots: ['proto/app'], ignore: ['tests'] },
    documents: ['docs', 'specs'],
    records: 'tools/developer-dashboard/records',
    mappings: 'tools/developer-dashboard/records/mappings.json',
    runtime: '.developer-dashboard',
    server: { host: '127.0.0.1', port: 0 },
    sessions: { staleAfterMinutes: 30 },
    testSuites: { ok: { label: 'Always passes', command: ['node', '-e', 'console.log("fine")'] } },
  }, null, 2));
  return { root, configPath, write, cleanup: () => fs.rmSync(root, { recursive: true, force: true }) };
}
