"""GitHub draft-release helper. Secrets stay in environment variables/runner temp.

Only the `draft` command writes to GitHub. All releases remain unpublished.
"""
import argparse
import base64
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import zipfile

PACKAGE = 'com.lem.duedatechecker'
SIGNING_FILE = 'duewise-upload.jks'
CERT_FILE = 'duewise-signing-cert.der'


class ReleaseError(ValueError):
    """A controlled, credential-free diagnostic safe for runner logs."""


def inputs(version, code):
    if not re.fullmatch(r'(0|[1-9][0-9]{0,3})\.(0|[1-9][0-9]{0,3})\.(0|[1-9][0-9]{0,3})', version):
        raise ReleaseError('Version must be three numeric parts, for example 1.0.0 (no v prefix).')
    if not re.fullmatch(r'[1-9][0-9]{0,9}', code) or int(code) > 2_100_000_000:
        raise ReleaseError('Android version code must be an integer from 1 to 2100000000.')
    return {'version': version, 'code': code, 'tag': 'v'+version, 'apk': f'Duewise-v{version}.apk'}


def context():
    values = inputs(os.environ['RELEASE_VERSION'], os.environ['VERSION_CODE'])
    sha = os.environ['GITHUB_SHA']
    if not re.fullmatch(r'[0-9a-f]{40}', sha):
        raise ReleaseError('Expected the full selected GitHub commit SHA.')
    return values, sha


def run(args):
    result = subprocess.run([str(arg) for arg in args], capture_output=True, text=True, errors='replace')
    if result.returncode:
        # Tool output can contain aliases, paths or configuration. Do not print it on failure.
        raise RuntimeError(f'{Path(str(args[0])).name} failed; check the tool installation or signing/repository configuration.')
    return result.stdout


def decode_keystore(temp, env):
    for name in ('ANDROID_KEYSTORE_BASE64', 'ANDROID_KEY_ALIAS', 'ANDROID_KEYSTORE_PASSWORD', 'ANDROID_KEY_PASSWORD'):
        if not env.get(name):
            raise ReleaseError(f'Missing required repository secret: {name}')
    try:
        data = base64.b64decode(''.join(env['ANDROID_KEYSTORE_BASE64'].split()), validate=True)
    except ValueError:
        raise ReleaseError('ANDROID_KEYSTORE_BASE64 is not valid base64.') from None
    if not data:
        raise ReleaseError('ANDROID_KEYSTORE_BASE64 decoded to an empty file.')
    path = temp / SIGNING_FILE
    # Never overwrite another file; the runner temp directory is unique to this job.
    with path.open('xb') as stream:
        path.chmod(0o600)
        stream.write(data)
    return path


def verify_metadata(badging, signing, cert, version, code):
    package = next((line for line in badging.splitlines() if line.startswith('package:')), '')
    fields = dict(re.findall(r"([A-Za-z]+)='([^']*)'", package))
    if fields.get('name') != PACKAGE or fields.get('versionName') != version or fields.get('versionCode') != code:
        raise ReleaseError('APK package/version does not match the requested release.')
    if 'application-debuggable' in badging.splitlines():
        raise ReleaseError('Release APK must not be debuggable.')
    fingerprints = re.findall(r'Signer #\d+ certificate SHA-256 digest: ([a-fA-F0-9]{64})', signing)
    if len(fingerprints) != 1 or fingerprints[0].lower() != cert.lower():
        raise ReleaseError('APK signing certificate does not match the supplied keystore.')


def package(publish_dir, output_dir, sdk, temp):
    values, sha = context()
    candidates = list(publish_dir.glob('*-Signed.apk'))
    if len(candidates) != 1:
        raise ReleaseError('Expected exactly one signed APK in the publish directory.')
    apk = candidates[0]
    with zipfile.ZipFile(apk) as archive:
        if archive.testzip() is not None:
            raise ReleaseError('APK archive integrity check failed.')
    tools = sdk / 'build-tools' / '36.0.0'
    signing = run([tools / 'apksigner', 'verify', '--verbose', '--print-certs', apk])
    cert_path = temp / CERT_FILE
    run(['keytool', '-exportcert', '-noprompt', '-keystore', temp / SIGNING_FILE,
         '-alias', os.environ['ANDROID_KEY_ALIAS'], '-storepass:env', 'ANDROID_KEYSTORE_PASSWORD', '-file', cert_path])
    fingerprint = hashlib.sha256(cert_path.read_bytes()).hexdigest()
    badging = run([tools / 'aapt', 'dump', 'badging', apk])
    verify_metadata(badging, signing, fingerprint, values['version'], values['code'])
    output_dir.mkdir(parents=True, exist_ok=True)
    target = output_dir / values['apk']
    shutil.copyfile(apk, target)
    checksum = hashlib.sha256(target.read_bytes()).hexdigest()
    target.with_suffix('.apk.sha256').write_text(f'{checksum}  {target.name}\n', encoding='utf-8')
    notes = (f'<!-- duewise-source:{sha} -->\n\n'
             f'Duewise {values["version"]} for Android (build {values["code"]}).\n\n'
             f'- Source commit: `{sha}`\n'
             f'- Core, App and release-tool automated tests passed in this workflow.\n'
             f'- Release APK signature verified against the supplied keystore.\n'
             f'- Signing certificate SHA-256: `{fingerprint}`\n'
             f'- APK SHA-256: `{checksum}`\n\n'
             'Draft for review. Automated checks do not establish phone, accessibility or notification-delivery acceptance.\n')
    (output_dir / 'RELEASE-NOTES.md').write_text(notes, encoding='utf-8')
    if os.environ.get('GITHUB_STEP_SUMMARY'):
        with Path(os.environ['GITHUB_STEP_SUMMARY']).open('a', encoding='utf-8') as stream:
            stream.write(notes)
    print(f'Verified {target.name}; SHA-256 {checksum}')


def release_state(repo, tag):
    if not re.fullmatch(r'[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+', repo):
        raise ReleaseError('Invalid GitHub repository.')
    owner, name = repo.split('/')
    query = '''query($owner:String!,$name:String!,$tag:String!,$ref:String!){
      repository(owner:$owner,name:$name){
        release(tagName:$tag){isDraft description url}
        ref(qualifiedName:$ref){target{oid __typename}}
      }
    }'''
    result = json.loads(run(['gh', 'api', 'graphql', '-f', 'query='+query, '-f', 'owner='+owner,
                             '-f', 'name='+name, '-f', 'tag='+tag, '-f', 'ref=refs/tags/'+tag]))
    if result.get('errors') or not result.get('data', {}).get('repository'):
        raise RuntimeError('Could not verify existing GitHub release/tag state.')
    state = result['data']['repository']
    target = state['ref']['target'] if state.get('ref') else None
    # Annotated tags may point at another tag; peel them without relying on a moving branch.
    for _ in range(8):
        if target is None or target['__typename'] == 'Commit':
            return state['release'], None if target is None else target['oid']
        if target['__typename'] != 'Tag':
            raise ReleaseError('Release tag does not resolve to a commit.')
        obj = json.loads(run(['gh', 'api', f'repos/{repo}/git/tags/{target["oid"]}']))['object']
        target = {'oid': obj['sha'], '__typename': {'commit': 'Commit', 'tag': 'Tag'}.get(obj['type'], 'Other')}
    raise ReleaseError('Release tag nesting is too deep.')


def guard_release(release, tag_sha, sha):
    if tag_sha is not None and tag_sha != sha:
        raise ReleaseError('This version tag points to another commit. Choose a new version.')
    if release is not None:
        if not release['isDraft']:
            raise ReleaseError('This release is already published; it will not be modified.')
        if f'<!-- duewise-source:{sha} -->' not in (release.get('description') or ''):
            raise ReleaseError('Existing draft belongs to another/unknown source. Choose a new version.')


def draft(output_dir, check_only=False):
    values, sha = context()
    repo = os.environ['GH_REPO']
    release, tag_sha = release_state(repo, values['tag'])
    guard_release(release, tag_sha, sha)
    if check_only:
        print('Release/tag preflight passed.')
        return
    apk = output_dir / values['apk']
    checksum_path = apk.with_suffix('.apk.sha256')
    notes_path = output_dir / 'RELEASE-NOTES.md'
    expected = hashlib.sha256(apk.read_bytes()).hexdigest()+'  '+apk.name+'\n'
    if checksum_path.read_text(encoding='utf-8') != expected:
        raise ReleaseError('Downloaded APK checksum mismatch.')
    if f'<!-- duewise-source:{sha} -->' not in notes_path.read_text(encoding='utf-8'):
        raise ReleaseError('Downloaded release notes do not match the built source.')
    assets = [str(apk), str(checksum_path)]
    if release is None:
        run(['gh', 'release', 'create', values['tag'], *assets, '--repo', repo, '--target', sha,
             '--title', 'Duewise '+values['version'], '--notes-file', notes_path, '--draft'])
    else:
        # Only a same-source unpublished draft may have its generated assets replaced.
        run(['gh', 'release', 'upload', values['tag'], *assets, '--repo', repo, '--clobber'])
        run(['gh', 'release', 'edit', values['tag'], '--repo', repo, '--notes-file', notes_path, '--draft=true'])
    print('Draft release ready: '+values['tag']+' (not published).')
    if os.environ.get('GITHUB_STEP_SUMMARY'):
        with Path(os.environ['GITHUB_STEP_SUMMARY']).open('a', encoding='utf-8') as stream:
            stream.write(f'\nDraft release prepared for `{values["tag"]}` from `{sha}`. Review it under Releases before publishing.\n')


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('command', choices=['inputs', 'decode', 'package', 'check', 'draft', 'clean'])
    parser.add_argument('--publish-dir', type=Path, default=Path('artifacts/release-build'))
    parser.add_argument('--output-dir', type=Path, default=Path('artifacts/github-release'))
    args = parser.parse_args()
    if args.command == 'inputs':
        values, _ = context()
        with Path(os.environ['GITHUB_OUTPUT']).open('a', encoding='utf-8') as stream:
            for key, value in values.items(): stream.write(f'{key}={value}\n')
    elif args.command in ('check', 'draft'):
        draft(args.output_dir, check_only=args.command == 'check')
    else:
        temp = Path(os.environ['RUNNER_TEMP'])
        if args.command == 'decode': decode_keystore(temp, os.environ)
        elif args.command == 'package': package(args.publish_dir, args.output_dir, Path(os.environ['ANDROID_HOME']), temp)
        elif args.command == 'clean':
            for name in (SIGNING_FILE, CERT_FILE): (temp / name).unlink(missing_ok=True)


if __name__ == '__main__':
    try: main()
    except (ReleaseError, RuntimeError) as error:
        print('Release step failed: '+str(error))
        raise SystemExit(1)
    except (ValueError, OSError, KeyError, zipfile.BadZipFile):
        # Errors are actionable in tests and code; runner logs never include secret-derived values.
        print('Release step failed. Check inputs, required secrets, signing tools, or conflicting release/tag state.')
        raise SystemExit(1)
