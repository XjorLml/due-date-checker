"""Repository entry point for the reusable starter-kit documentation checker."""

from pathlib import Path
import runpy
import sys


if __name__ == '__main__':
    root = Path(__file__).resolve().parents[1]
    sys.argv[1:1] = ['--root', str(root)]
    runpy.run_path(str(root / 'docs/templates/maui-starter/project/tools/check_docs.py'), run_name='__main__')
