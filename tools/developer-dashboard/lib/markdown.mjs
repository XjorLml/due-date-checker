// Deterministic Markdown readers for specs, tasks and the PRD requirement table.
// They read structure only (headings, table rows, checkboxes); they never decide
// whether something was verified.

const FENCE = /^ {0,3}(`{3,}|~{3,})/;
const HEADING = /^ {0,3}(#{1,6})\s+(.+?)\s*#*\s*$/;
const TASK = /^\s*[-*+]\s+\[( |x|X)\]\s+(.+)$/;
// First-cell IDs such as DD01, KA01, FT01, UX12. Bold is allowed.
const ROW_ID = /^\*{0,2}([A-Z]{1,5}\d{2,3})\b/;
// IDs that tables cite as references rather than define as this spec's scenarios.
const REFERENCE_IDS = /^(R|TP|BR)\d{2}$/;

/** Plain text from inline Markdown: links, code, emphasis and HTML removed. */
export function plain(text) {
  return String(text)
    .replace(/!?\[([^\]]*)\]\([^)]*\)/g, '$1')
    .replace(/<[^>]+>/g, '')
    .replace(/`([^`]*)`/g, '$1')
    .replace(/\*\*|__/g, '')
    .replace(/\s+/g, ' ')
    .trim();
}

/** GitHub-style heading slug, matching tools/check_docs.py. */
export function slug(text) {
  const visible = String(text).replace(/!?\[([^\]]+)\]\([^)]*\)/g, '$1').replace(/<[^>]+>/g, '');
  return [...visible.trim().toLowerCase()].filter(c => /[\p{L}\p{N}_\- ]/u.test(c)).join('').replace(/ /g, '-');
}

/** Lines outside fenced code blocks, with 1-based line numbers. */
function* proseLines(text) {
  let fence = null;
  const lines = text.split(/\r?\n/);
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    const marker = FENCE.exec(line);
    if (marker) {
      if (!fence) fence = marker[1][0];
      else if (marker[1][0] === fence) fence = null;
      continue;
    }
    if (!fence) yield { line, number: i + 1 };
  }
}

function splitRow(line) {
  const body = line.trim().replace(/^\|/, '').replace(/\|$/, '');
  return body.split(/(?<!\\)\|/).map(cell => cell.trim());
}

// Same duplicate-heading rule as tools/check_docs.py: "x", "x-1", "x-2", ...
function headingTracker() {
  const used = new Set();
  return title => {
    const base = slug(title);
    let candidate = base;
    for (let n = 1; used.has(candidate); n++) candidate = `${base}-${n}`;
    used.add(candidate);
    return candidate;
  };
}

/**
 * Read a spec: title, the first "Status:" line, and scenario IDs defined in the
 * first column of its tables (deduplicated, in document order).
 */
export function parseSpec(text) {
  const nextAnchor = headingTracker();
  let title = null, status = null, anchor = '';
  const scenarios = [];
  const seen = new Set();
  for (const { line, number } of proseLines(text)) {
    const h = HEADING.exec(line);
    if (h) {
      anchor = nextAnchor(h[2]);
      if (!title && h[1] === '#') title = plain(h[2]);
      continue;
    }
    if (!status) {
      const s = /^\s*(?:\*\*)?status(?:\*\*)?\s*:\s*(?:\*\*)?\s*(.+)$/i.exec(line);
      if (s) status = plain(s[1]).slice(0, 240);
    }
    if (!line.trimStart().startsWith('|') || /^\s*\|[\s:|-]+\|\s*$/.test(line)) continue;
    const cells = splitRow(line);
    const m = ROW_ID.exec(cells[0] ?? '');
    if (!m || REFERENCE_IDS.test(m[1]) || seen.has(m[1])) continue;
    seen.add(m[1]);
    scenarios.push({ id: m[1], text: plain(cells[1] ?? '').slice(0, 280), line: number, anchor });
  }
  return { title, status, scenarios };
}

/** Count checkbox tasks per section. Open items keep their text and line. */
export function parseTasks(text) {
  let section = '';
  let status = null;
  const sections = [];
  const bySection = new Map();
  let checked = 0, open = 0;
  const openItems = [];
  for (const { line, number } of proseLines(text)) {
    const h = HEADING.exec(line);
    if (h) { section = plain(h[2]); continue; }
    if (!status) {
      const s = /^\s*(?:\*\*)?status(?:\*\*)?\s*:\s*(?:\*\*)?\s*(.+)$/i.exec(line);
      if (s) status = plain(s[1]).slice(0, 240);
    }
    const t = TASK.exec(line);
    if (!t) continue;
    let entry = bySection.get(section);
    if (!entry) { entry = { section, checked: 0, open: 0 }; bySection.set(section, entry); sections.push(entry); }
    if (t[1] === ' ') {
      entry.open++; open++;
      openItems.push({ section, text: plain(t[2]).slice(0, 240), line: number });
    } else { entry.checked++; checked++; }
  }
  return { checked, open, total: checked + open, sections, openItems, status };
}

/** Rows of the table under `## <sectionTitle>` whose first cell is R01-style. */
export function parseRequirements(text, sectionTitle) {
  const rows = [];
  let inside = false;
  for (const { line, number } of proseLines(text)) {
    const h = HEADING.exec(line);
    if (h && h[1] === '##') { inside = plain(h[2]).toLowerCase() === sectionTitle.toLowerCase(); continue; }
    if (!inside || !line.trimStart().startsWith('|')) continue;
    const cells = splitRow(line);
    const id = /^R\d{2}$/.exec(plain(cells[0] ?? ''));
    if (!id) continue;
    rows.push({
      id: id[0],
      text: plain(cells[1] ?? '').slice(0, 400),
      slice: plain(cells[2] ?? ''),
      acceptance: plain(cells[3] ?? '').slice(0, 280),
      line: number,
    });
  }
  return rows;
}

/** First bold phrase under `## <sectionTitle>`, e.g. STATUS "Current focus". */
export function firstBoldIn(text, sectionTitle) {
  let inside = false;
  for (const { line } of proseLines(text)) {
    const h = HEADING.exec(line);
    if (h && h[1] === '##') { inside = plain(h[2]).toLowerCase() === sectionTitle.toLowerCase(); continue; }
    if (!inside) continue;
    const b = /\*\*(.+?)\*\*/.exec(line);
    if (b) return plain(b[1]).replace(/[:.]\s*$/, '').slice(0, 200);
  }
  return null;
}
