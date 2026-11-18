"""Behavior tests for the reusable documentation checker; no app build required."""

import importlib.util
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest


SCRIPT = Path(__file__).resolve().parents[1] / 'check_docs.py'
spec = importlib.util.spec_from_file_location('check_docs', SCRIPT)
checker = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = checker
spec.loader.exec_module(checker)


class LinkChecks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def write(self, name, text):
        path = self.root / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding='utf-8')
        return path

    def test_links_anchors_encoding_and_references(self):
        self.write('README.md', '[guide](docs/My%20Guide.md#hello-world)\n[again][g]\n[g]: docs/My%20Guide.md#hello-world-1\n[HTML](page.html#panel)\n')
        self.write('docs/My Guide.md', '# Hello `World`\n# Hello World\n')
        self.write('page.html', '<section id="panel"></section>')
        result = checker.check(self.root)
        self.assertEqual(result.errors, [])
        self.assertEqual(result.links, 4)

    def test_broken_links_anchors_and_undefined_references_fail(self):
        self.write('README.md', '[file](missing.md)\n[anchor](#missing)\n[reference][undefined]\n')
        result = checker.check(self.root)
        self.assertEqual(len(result.errors), 3)
        self.assertTrue(any('README.md:2: missing anchor' in e for e in result.errors))

    def test_fences_comments_inline_code_and_external_links(self):
        self.write('README.md', '```md\n[example](missing.md)\n```\n~~~text\n[example](missing.md)\n~~~\n`[inline](missing.md)`\n<!-- [comment](missing.md) -->\n[external](https://example.invalid/no-network)\n')
        result = checker.check(self.root)
        self.assertEqual(result.errors, [])
        self.assertEqual(result.links, 0)
        self.assertEqual(result.external, 1)

    def test_duplicate_headings_setext_html_and_angle_paths(self):
        self.write('README.md', '[one](#same) [two](#same-1) [three](#same-1-1) [setext](#plain-title) [named](#legacy) [space](<a b.md>)\n# Same\n# Same\n# Same-1\nPlain title\n===========\n<a name="legacy"></a>\n')
        self.write('a b.md', '# Good')
        self.assertEqual(checker.check(self.root).errors, [])

    def test_draft_placeholders_are_allowed_until_ready(self):
        self.write('README.md', '# {{ProjectName}}\nThe notation {{...}} is explanatory.\n')
        self.assertEqual(checker.check(self.root).errors, [])
        self.assertEqual(len(checker.check(self.root, ready=True).errors), 1)

    def test_ready_skips_reusable_templates_but_not_their_links_or_copies(self):
        self.write('docs/adr/TEMPLATE.md', '# ADR {{NNN}}\n[broken](missing.md)\n')
        self.write('specs/feature-template/spec.md', '# {{FeatureName}}\n')
        self.write('specs/sign-in/spec.md', '# {{FeatureName}}\n')
        result = checker.check(self.root, ready=True)
        self.assertEqual(len(result.errors), 2, result.errors)
        self.assertTrue(any('sign-in' in e and 'unresolved placeholder' in e for e in result.errors))
        self.assertTrue(any('TEMPLATE.md:2: missing target' in e for e in result.errors))
        self.assertEqual(result.template_placeholders, 2)

    def test_explicit_paths_missing_root_and_escape_fail(self):
        self.write('README.md', '[outside](../outside.md)\n')
        self.assertTrue(checker.check(self.root).errors)
        self.assertTrue(checker.check(self.root, ['absent']).errors)
        self.assertTrue(checker.check(self.root / 'missing').errors)
        self.assertTrue(checker.check(self.root, ['../']).errors)

    def test_route_allowlist_does_not_hide_other_missing_fragments(self):
        self.write('README.md', '[route](page.html#view/home) [broken](page.html#typo)')
        self.write('page.html', '<main id="main"></main>')
        result = checker.check(self.root, routes={'page.html': ['view/home']})
        self.assertEqual(result.routes, 1)
        self.assertEqual(len(result.errors), 1)
        self.assertIn('#typo', result.errors[0])

    def test_cli_copy_defaults_to_destination_and_returns_actionable_failure(self):
        copied = self.root / 'tools/check_docs.py'
        copied.parent.mkdir()
        shutil.copy2(SCRIPT, copied)
        self.write('README.md', '[broken](missing.md)')
        result = subprocess.run([sys.executable, str(copied)], cwd=self.root.parent, capture_output=True, text=True)
        self.assertEqual(result.returncode, 1)
        self.assertIn('README.md:1:', result.stdout)
        self.write('README.md', '# Customized app')
        result = subprocess.run([sys.executable, str(copied), '--ready'], cwd=self.root.parent, capture_output=True, text=True)
        self.assertEqual(result.returncode, 0, result.stdout)

    def test_cli_rejects_bad_configuration(self):
        self.write('README.md', '# App')
        self.write('tools/docs-routes.json', '[]')
        result = subprocess.run([sys.executable, str(SCRIPT), '--root', str(self.root)], capture_output=True, text=True)
        self.assertEqual(result.returncode, 2)
        self.assertIn('expected an object', result.stderr)


if __name__ == '__main__':
    unittest.main()
