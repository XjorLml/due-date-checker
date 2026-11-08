import base64
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch
import zipfile

SPEC = importlib.util.spec_from_file_location('duewise_release', Path(__file__).resolve().parents[1] / 'release.py')
release = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(release)
SHA = 'a'*40
ENV = {'RELEASE_VERSION': '1.0.0', 'VERSION_CODE': '2', 'GITHUB_SHA': SHA, 'GH_REPO': 'owner/repo'}


class InputTests(unittest.TestCase):
    def test_valid_inputs_define_safe_asset_and_tag_names(self):
        self.assertEqual(release.inputs('1.20.300', '2'),
                         {'version': '1.20.300', 'code': '2', 'tag': 'v1.20.300', 'apk': 'Duewise-v1.20.300.apk'})
        self.assertEqual(release.inputs('0.0.0', '2100000000')['code'], '2100000000')

    def test_rejects_invalid_versions_and_injection(self):
        for version in ['v1.0.0', '../1.0.0', '1.0', '01.0.0', '1.0.0-beta', '1.0.0\nother=x', '1.0.0; echo bad', '$(whoami)', '1/2/3']:
            with self.subTest(version=version), self.assertRaises(ValueError): release.inputs(version, '2')

    def test_rejects_invalid_android_codes(self):
        for code in ['0', '-1', '02', '2.0', '2100000001', '9999999999999', '2\nother=x', '2;echo bad']:
            with self.subTest(code=code), self.assertRaises(ValueError): release.inputs('1.0.0', code)

    def test_rejects_invalid_source_sha(self):
        with patch.dict(os.environ, ENV | {'GITHUB_SHA': 'develop'}), self.assertRaises(ValueError): release.context()


class SigningTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(); self.root = Path(self.temp.name)
        self.env = {'ANDROID_KEYSTORE_BASE64': base64.b64encode(b'fixture-key-bytes').decode(),
                    'ANDROID_KEY_ALIAS': 'upload', 'ANDROID_KEYSTORE_PASSWORD': 'store-fixture-secret',
                    'ANDROID_KEY_PASSWORD': 'key-fixture-secret'}

    def tearDown(self): self.temp.cleanup()

    def test_decodes_only_into_temp_and_accepts_base64_whitespace(self):
        env = self.env | {'ANDROID_KEYSTORE_BASE64': '\n'+self.env['ANDROID_KEYSTORE_BASE64']+'\n'}
        path = release.decode_keystore(self.root, env)
        self.assertEqual(path, self.root / release.SIGNING_FILE)
        self.assertEqual(path.read_bytes(), b'fixture-key-bytes')
        if os.name == 'posix': self.assertEqual(path.stat().st_mode & 0o777, 0o600)

    def test_missing_secret_names_are_reported_without_values(self):
        for key in self.env:
            env = self.env | {key: ''}
            with self.subTest(key=key), self.assertRaisesRegex(ValueError, key) as error:
                release.decode_keystore(self.root, env)
            for value in self.env.values(): self.assertNotIn(value, str(error.exception))

    def test_malformed_and_empty_base64_create_no_file(self):
        for value in ['not-base64-sensitive!', ' ']:
            with self.subTest(value=value), self.assertRaises(ValueError):
                release.decode_keystore(self.root, self.env | {'ANDROID_KEYSTORE_BASE64': value})
            self.assertFalse((self.root / release.SIGNING_FILE).exists())

    def test_never_overwrites_an_existing_temp_key(self):
        path = self.root / release.SIGNING_FILE; path.write_bytes(b'keep')
        with self.assertRaises(FileExistsError): release.decode_keystore(self.root, self.env)
        self.assertEqual(path.read_bytes(), b'keep')

    def test_failed_tool_does_not_echo_command_or_secret_output(self):
        completed = subprocess.CompletedProcess([], 1, 'store-fixture-secret', 'key-fixture-secret')
        with patch.object(release.subprocess, 'run', return_value=completed), self.assertRaises(RuntimeError) as error:
            release.run(['keytool', 'store-fixture-secret'])
        self.assertNotIn('store-fixture-secret', str(error.exception))
        self.assertNotIn('key-fixture-secret', str(error.exception))

    def test_cli_failure_never_prints_secret_value(self):
        env = os.environ | self.env | {'RUNNER_TEMP': str(self.root), 'ANDROID_KEYSTORE_BASE64': 'sensitive-invalid-value!'}
        result = subprocess.run([os.sys.executable, str(Path(release.__file__)), 'decode'], env=env,
                                capture_output=True, text=True)
        self.assertNotEqual(result.returncode, 0)
        self.assertNotIn('sensitive-invalid-value', result.stdout+result.stderr)
        self.assertFalse((self.root / release.SIGNING_FILE).exists())

    def test_cleanup_is_idempotent_without_inputs_and_preserves_other_files(self):
        for name in (release.SIGNING_FILE, release.CERT_FILE, 'unrelated-file'):
            (self.root / name).write_bytes(b'fixture')
        env = os.environ | {'RUNNER_TEMP': str(self.root)}
        for _ in range(2):
            result = subprocess.run([os.sys.executable, str(Path(release.__file__)), 'clean'], env=env,
                                    capture_output=True, text=True)
            self.assertEqual(result.returncode, 0)
        self.assertFalse((self.root / release.SIGNING_FILE).exists())
        self.assertFalse((self.root / release.CERT_FILE).exists())
        self.assertEqual((self.root / 'unrelated-file').read_bytes(), b'fixture')


class MetadataTests(unittest.TestCase):
    BADGING = "package: name='com.lem.duedatechecker' versionCode='2' versionName='1.0.0'\n"
    FINGERPRINT = 'b'*64
    SIGNING = 'Signer #1 certificate SHA-256 digest: '+FINGERPRINT+'\n'

    def test_accepts_verified_matching_identity(self):
        release.verify_metadata(self.BADGING, self.SIGNING, self.FINGERPRINT, '1.0.0', '2')

    def test_rejects_wrong_package_version_code_debug_flag_and_signer(self):
        badgings = [self.BADGING.replace('com.lem.duedatechecker','other.app'),
                    self.BADGING.replace("versionCode='2'", "versionCode='1'"),
                    self.BADGING.replace('1.0.0','0.9.0'), self.BADGING+'application-debuggable\n']
        for badging in badgings:
            with self.subTest(badging=badging), self.assertRaises(ValueError):
                release.verify_metadata(badging, self.SIGNING, self.FINGERPRINT, '1.0.0', '2')
        for signing in ['', self.SIGNING.replace('b'*64,'c'*64), self.SIGNING+self.SIGNING]:
            with self.subTest(signing=signing), self.assertRaises(ValueError):
                release.verify_metadata(self.BADGING, signing, self.FINGERPRINT, '1.0.0', '2')

    def test_package_exports_only_signed_apk_checksum_and_traceable_notes(self):
        with tempfile.TemporaryDirectory() as temp, patch.dict(os.environ, ENV | {'ANDROID_KEY_ALIAS':'upload'}):
            root = Path(temp); publish = root/'publish'; output = root/'public'; publish.mkdir()
            apk = publish/'app-Signed.apk'
            with zipfile.ZipFile(apk,'w') as z: z.writestr('AndroidManifest.xml', b'fixture')
            (publish/'do-not-upload.props').write_text('fixture secret')
            cert = b'public certificate fixture'; fingerprint = hashlib.sha256(cert).hexdigest()
            def fake_run(args):
                if Path(str(args[0])).name == 'apksigner': return 'Signer #1 certificate SHA-256 digest: '+fingerprint
                if args[0] == 'keytool':
                    self.assertIn('-storepass:env', args); (root/release.CERT_FILE).write_bytes(cert); return ''
                return self.BADGING
            with patch.object(release,'run',side_effect=fake_run): release.package(publish,output,root,root)
            self.assertEqual({p.name for p in output.iterdir()}, {'Duewise-v1.0.0.apk','Duewise-v1.0.0.apk.sha256','RELEASE-NOTES.md'})
            target = output/'Duewise-v1.0.0.apk'
            self.assertEqual(target.read_bytes(),apk.read_bytes())
            self.assertEqual(target.with_suffix('.apk.sha256').read_text(),hashlib.sha256(apk.read_bytes()).hexdigest()+'  '+target.name+'\n')
            self.assertIn('<!-- duewise-source:'+SHA+' -->',(output/'RELEASE-NOTES.md').read_text())

    def test_missing_or_ambiguous_signed_output_fails(self):
        with tempfile.TemporaryDirectory() as temp, patch.dict(os.environ,ENV):
            root=Path(temp)
            with self.assertRaises(ValueError): release.package(root,root/'out',root,root)
            (root/'a-Signed.apk').touch();(root/'b-Signed.apk').touch()
            with self.assertRaises(ValueError): release.package(root,root/'out',root,root)


class DraftTests(unittest.TestCase):
    def test_guard_accepts_only_new_or_same_commit_draft(self):
        release.guard_release(None,None,SHA)
        release.guard_release({'isDraft':True,'description':'<!-- duewise-source:'+SHA+' -->'},SHA,SHA)
        for current, tag in [({'isDraft':False},SHA), (None,'b'*40),
                             ({'isDraft':True,'description':'manual draft'},None),
                             ({'isDraft':True,'description':'<!-- duewise-source:'+'b'*40+' -->'},None)]:
            with self.subTest(current=current), self.assertRaises(ValueError): release.guard_release(current,tag,SHA)

    def test_graphql_errors_fail_closed(self):
        for response in [{'errors':[{'message':'not allowed'}]}, {'data':{'repository':None}}]:
            with patch.object(release,'run',return_value=json.dumps(response)),self.assertRaises(RuntimeError):
                release.release_state('owner/repo','v1.0.0')

    def test_annotated_tags_are_peeled_to_commit(self):
        state={'data':{'repository':{'release':None,'ref':{'target':{'oid':'b'*40,'__typename':'Tag'}}}}}
        nested={'object':{'type':'tag','sha':'c'*40}}
        commit={'object':{'type':'commit','sha':SHA}}
        with patch.object(release,'run',side_effect=[json.dumps(x) for x in [state,nested,commit]]):
            self.assertEqual(release.release_state('owner/repo','v1.0.0'),(None,SHA))

    def fixture(self, root):
        apk=root/'Duewise-v1.0.0.apk';apk.write_bytes(b'fixture APK')
        apk.with_suffix('.apk.sha256').write_text(hashlib.sha256(apk.read_bytes()).hexdigest()+'  '+apk.name+'\n')
        (root/'RELEASE-NOTES.md').write_text('<!-- duewise-source:'+SHA+' -->')
        return apk

    def test_new_release_is_always_draft_at_exact_built_sha(self):
        with tempfile.TemporaryDirectory() as temp,patch.dict(os.environ,ENV):
            root=Path(temp);self.fixture(root)
            with patch.object(release,'release_state',return_value=(None,None)),patch.object(release,'run') as run:
                release.draft(root)
                args=run.call_args.args[0]
                self.assertEqual(args[:4],['gh','release','create','v1.0.0'])
                self.assertIn('--draft',args);self.assertEqual(args[args.index('--target')+1],SHA)

    def test_same_source_retry_replaces_only_draft_assets(self):
        current={'isDraft':True,'description':'<!-- duewise-source:'+SHA+' -->'}
        with tempfile.TemporaryDirectory() as temp,patch.dict(os.environ,ENV):
            root=Path(temp);self.fixture(root)
            with patch.object(release,'release_state',return_value=(current,SHA)),patch.object(release,'run') as run:
                release.draft(root)
                self.assertEqual(run.call_count,2)
                self.assertEqual(run.call_args_list[0].args[0][2],'upload')
                self.assertIn('--draft=true',run.call_args_list[1].args[0])

    def test_published_release_cannot_trigger_any_write(self):
        with patch.dict(os.environ,ENV),patch.object(release,'release_state',return_value=({'isDraft':False},SHA)),patch.object(release,'run') as run:
            with self.assertRaises(ValueError): release.draft(Path('unused'))
            run.assert_not_called()

    def test_downloaded_tampered_apk_or_wrong_notes_cannot_be_uploaded(self):
        for tamper in ['apk','notes']:
            with tempfile.TemporaryDirectory() as temp,patch.dict(os.environ,ENV):
                root=Path(temp);apk=self.fixture(root)
                if tamper=='apk': apk.write_bytes(b'tampered')
                else: (root/'RELEASE-NOTES.md').write_text('<!-- duewise-source:'+'b'*40+' -->')
                with patch.object(release,'release_state',return_value=(None,None)),patch.object(release,'run') as run:
                    with self.assertRaises(ValueError): release.draft(root)
                    run.assert_not_called()


if __name__ == '__main__': unittest.main()
