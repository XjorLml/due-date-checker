"""Check local Markdown links without dependencies or network access."""

from __future__ import annotations

import argparse
from dataclasses import dataclass, field
import html
import json
from pathlib import Path
import re
import sys
import unicodedata
from urllib.parse import unquote, urlsplit


DEFAULT_PATHS = ("README.md", "AGENTS.md", "docs", "specs", "tools", "prototypes", ".vibe-wise")
EXCLUDED = {".git", ".venv", "node_modules", "bin", "obj", ".qa", "artifacts", "__pycache__"}
# Reusable templates keep their placeholders on purpose, so --ready skips them.
# Copies get new names (NNN-title.md, specs/<feature>/), so copies are enforced.
TEMPLATE_FILES = {"template.md"}
TEMPLATE_DIRS = {"feature-template"}
# Inline destinations support escaped punctuation, angle brackets and balanced
# parentheses one level deep. This is a local documentation checker, not a full
# CommonMark renderer; see tools/README.md for supported constructs and limits.
INLINE = re.compile(r'!?\[(?:\\.|[^\]\\\n])*\]\(\s*(<[^>\n]+>|(?:\\.|[^\s()\\]|\([^()\n]*\))+)(?:\s+[\"\'][^\n]*?[\"\'])?\s*\)')
DEFINITION = re.compile(r'^ {0,3}\[([^\]]+)\]:\s*(<[^>]+>|\S+)', re.M)
REFERENCE = re.compile(r'!?\[([^\]\n]+)\]\[([^\]\n]*)\]')
SHORTCUT = re.compile(r'(?<!!)\[([^\]\n]+)\](?![\[(])')


@dataclass
class Report:
    files: int = 0
    links: int = 0
    external: int = 0
    routes: int = 0
    placeholders: int = 0
    template_placeholders: int = 0
    errors: list[str] = field(default_factory=list)


def reusable_template(relative: Path) -> bool:
    folders = {part.lower() for part in relative.parts[:-1]}
    return relative.name.lower() in TEMPLATE_FILES or bool(TEMPLATE_DIRS & folders)


def prose(text: str) -> str:
    """Mask fenced code/comments while preserving line numbers."""
    text = re.sub(r'<!--.*?-->', lambda m: re.sub(r'[^\n]', ' ', m[0]), text, flags=re.S)
    lines = []
    fence = None
    for line in text.splitlines(keepends=True):
        marker = re.match(r'^ {0,3}(`{3,}|~{3,})(.*)$', line.rstrip('\r\n'))
        masked = fence is not None or marker is not None
        if marker:
            run, rest = marker.groups()
            if fence is None:
                fence = run
            elif run[0] == fence[0] and len(run) >= len(fence) and not rest.strip():
                fence = None
        lines.append(re.sub(r'[^\n]', ' ', line) if masked else line)
    return ''.join(lines)


def slug(text: str) -> str:
    text = re.sub(r'!?\[([^\]]+)\]\([^)]*\)', r'\1', text)
    text = re.sub(r'<[^>]+>', '', text)
    text = html.unescape(text).strip().lower()
    text = ''.join(c for c in text if c in '_- ' or unicodedata.category(c)[0] in 'LN')
    return text.replace(' ', '-')


def anchors(text: str, markdown: bool) -> set[str]:
    visible = prose(text) if markdown else text
    result = set()
    if markdown:
        used = set()
        lines = visible.splitlines()
        for index, line in enumerate(lines):
            heading = re.match(r'^ {0,3}#{1,6}\s+(.+?)\s*#*\s*$', line)
            title = heading[1] if heading else None
            if not title and index + 1 < len(lines) and line.strip() and re.fullmatch(r' {0,3}(?:=+|-+)\s*', lines[index + 1]):
                title = line.strip()
            if title:
                base = slug(title)
                candidate, n = base, 0
                while candidate in used:
                    n += 1
                    candidate = f'{base}-{n}'
                used.add(candidate)
        result.update(used)
    result.update(html.unescape(m[2]) for m in re.finditer(r'\b(?:id|name)\s*=\s*([\"\'])(.*?)\1', visible, re.I))
    return result


def destinations(text: str):
    visible = prose(text)
    visible = re.sub(r'(`+)(.*?)\1', lambda m: ' ' * len(m[0]), visible)
    definitions = {m[1].casefold(): m[2] for m in DEFINITION.finditer(visible)}
    # Validate definitions even when unused, but do not mistake definitions for shortcuts.
    for match in DEFINITION.finditer(visible):
        yield match.start(), match[2]
    visible = DEFINITION.sub(lambda m: ' ' * len(m[0]), visible)
    for match in INLINE.finditer(visible):
        yield match.start(), match[1]
    visible = INLINE.sub(lambda m: ' ' * len(m[0]), visible)
    for match in REFERENCE.finditer(visible):
        key = (match[2] or match[1]).casefold()
        yield match.start(), definitions.get(key)
    visible = REFERENCE.sub(lambda m: ' ' * len(m[0]), visible)
    for match in SHORTCUT.finditer(visible):
        if match[1].casefold() in definitions:
            yield match.start(), definitions[match[1].casefold()]


def check(root: Path, paths: list[str] | None = None, ready: bool = False,
          routes: dict[str, list[str]] | None = None) -> Report:
    root = root.resolve()
    report = Report()
    if not root.is_dir():
        report.errors.append(f'{root}: root directory does not exist; pass --root PATH.')
        return report
    chosen = paths if paths else [p for p in DEFAULT_PATHS if (root / p).exists()]
    files = set()
    for item in chosen:
        path = (root / item).resolve()
        if not path.is_relative_to(root) or not path.exists():
            report.errors.append(f'{item}: path missing or outside --root; check its spelling.')
            continue
        candidates = path.rglob('*.md') if path.is_dir() else [path]
        files.update(p for p in candidates if p.suffix.lower() == '.md' and not EXCLUDED.intersection(p.relative_to(root).parts))
    if not files:
        report.errors.append('No Markdown files found; pass --root PATH or documentation paths.')
    cache = {}
    for path in sorted(files):
        report.files += 1
        try:
            text = path.read_text(encoding='utf-8-sig')
        except (OSError, UnicodeError) as error:
            report.errors.append(f'{path.relative_to(root)}: cannot read UTF-8 Markdown ({type(error).__name__}).')
            continue
        def problem(offset, message):
            report.errors.append(f'{path.relative_to(root)}:{text.count(chr(10), 0, offset) + 1}: {message}')
        placeholders = list(re.finditer(r'\{\{(?=[^{}\n]*[A-Za-z])[^{}\n]+\}\}', text))
        report.placeholders += len(placeholders)
        if ready and reusable_template(path.relative_to(root)):
            report.template_placeholders += len(placeholders)
        elif ready:
            for match in placeholders:
                problem(match.start(), f'unresolved placeholder {match[0]}; customize or remove the inapplicable example.')
        for offset, raw in destinations(text):
            if raw is None:
                problem(offset, 'undefined reference link; add its [label]: destination definition.')
                continue
            target = re.sub(r'\\([\s()\[\]#])', r'\1', raw.strip('<>'))
            try:
                parts = urlsplit(target)
            except ValueError:
                problem(offset, f'invalid link destination: {target}')
                continue
            if parts.scheme in {'http', 'https', 'mailto', 'tel', 'data'} or target.startswith('//'):
                report.external += 1
                continue
            report.links += 1
            if parts.scheme:
                problem(offset, f'nonportable link scheme: {target}; use a repository-relative path.')
                continue
            destination = (path.parent / unquote(parts.path)).resolve() if parts.path else path
            if not destination.is_relative_to(root) or not destination.exists():
                problem(offset, f'missing target or path outside --root: {target}')
                continue
            if not parts.fragment:
                continue
            fragment = unquote(parts.fragment)
            route = destination.relative_to(root).as_posix()
            if fragment in (routes or {}).get(route, []):
                report.routes += 1
                continue
            if destination.suffix.lower() not in {'.md', '.html', '.htm'}:
                problem(offset, f'unsupported fragment target: {target}; verify it manually.')
                continue
            if destination not in cache:
                try:
                    cache[destination] = anchors(destination.read_text(encoding='utf-8-sig'), destination.suffix.lower() == '.md')
                except (OSError, UnicodeError):
                    cache[destination] = set()
            if fragment not in cache[destination]:
                problem(offset, f'missing anchor: {target}; update the fragment or restore its heading/id.')
    return report


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, epilog='Exit 0: checks passed; 1: documentation errors; 2: invalid arguments/configuration. No network, build or device access.')
    parser.add_argument('paths', nargs='*', help='Files/directories relative to --root; default: existing documentation roots.')
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[1], help='Repository root; defaults to the copied project, independent of working directory.')
    parser.add_argument('--ready', action='store_true', help='Also fail on unresolved {{placeholders}}. Use after customizing a copied kit.')
    args = parser.parse_args(argv)
    manifest = args.root / 'tools/docs-routes.json'
    routes = {}
    if manifest.exists():
        try:
            routes = json.loads(manifest.read_text(encoding='utf-8-sig'))
            if not isinstance(routes, dict) or not all(isinstance(k, str) and isinstance(v, list) and all(isinstance(x, str) for x in v) for k, v in routes.items()):
                raise ValueError('expected an object mapping relative HTML paths to fragment lists')
        except (OSError, ValueError) as error:
            parser.error(f'Cannot read {manifest}: {error}')
    report = check(args.root, args.paths, args.ready, routes)
    for error in report.errors:
        print(f'ERROR {error}')
    print(f'{"FAIL" if report.errors else "PASS"}: {report.files} Markdown files; {report.links} local links; {len(report.errors)} errors.')
    mode = f'readiness enforced; {report.template_placeholders} kept in reusable templates' if args.ready else 'drafts allowed'
    print(f'Not executed: {report.external} external URLs; {report.routes} declared application routes. Placeholders: {report.placeholders} ({mode}).')
    return int(bool(report.errors))


if __name__ == '__main__':
    sys.exit(main())
