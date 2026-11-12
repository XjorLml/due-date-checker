// Loads dashboard.config.json and records/mappings.json and checks them against
// what the prototype actually supports. Problems are reported, not guessed around.
import fs from 'node:fs';
import path from 'node:path';
import { readText, resolveInside, toolDir } from './paths.mjs';

// Screen, view and overlay IDs taken from prototypes/design-exploration/app.js
// (navigate(), loadHash()) and ph-ui.js (phOpenPicker). Keep in sync with the bridge.
export const SCREENS = ['dashboard', 'calendar', 'dues', 'settings', 'details', 'add', 'edit', 'reminders', 'catalog'];
export const VIEWS = ['explore', 'lab', 'decide', 'notify', 'compare'];
export const OVERLAYS = ['category-picker'];
export const SCOPES = ['approved', 'optional', 'proposed', 'prototype', 'tooling', 'roadmap'];
export const STATUSES = ['backlog', 'in_progress', 'in_review', 'blocked', 'verified'];
export const UNAVAILABLE = ['no-ui', 'unimplemented', 'unmapped'];

export function loadConfig(root, configPath = path.join(toolDir, 'dashboard.config.json')) {
  const config = JSON.parse(readText(configPath));
  if (config.schemaVersion !== 1) throw new Error(`Unsupported config schemaVersion ${config.schemaVersion}`);
  const must = (relative, label) => {
    const file = resolveInside(root, relative);
    if (!file) throw new Error(`${label} path ${relative} is outside the repository`);
    return file;
  };
  return {
    ...config,
    abs: {
      root,
      requirements: must(config.requirements.file, 'requirements'),
      specs: must(config.specsDir, 'specsDir'),
      status: must(config.statusDocument.file, 'statusDocument'),
      records: must(config.records, 'records'),
      runtime: must(config.runtime, 'runtime'),
      mappings: must(config.mappings, 'mappings'),
      previewEntry: must(config.preview.entry, 'preview entry'),
      previewRoots: config.preview.roots.map(r => must(r, 'preview root')),
    },
  };
}

export function loadMappings(config) {
  try { return JSON.parse(readText(config.abs.mappings)); }
  catch (error) {
    if (error.code === 'ENOENT') return { schemaVersion: 1, phases: [], features: {} };
    throw new Error(`Cannot read ${config.mappings}: ${error.message}`);
  }
}

/** Check one mapping entry; returns a list of human-readable problems. */
export function mappingProblems(id, entry, { config, known }) {
  const problems = [];
  const say = message => problems.push(`${config.mappings}: ${id}: ${message}`);
  if (!known.has(id)) say('no such feature (not a PRD requirement or spec folder)');
  if (entry.scope && !SCOPES.includes(entry.scope)) say(`scope must be one of ${SCOPES.join(', ')}`);
  if (entry.prototype && entry.unavailable) say('has both prototype and unavailable; choose one');
  const p = entry.prototype;
  if (p) {
    if (p.view && !VIEWS.includes(p.view)) say(`unknown view ${p.view}`);
    if (p.screen && !SCREENS.includes(p.screen)) say(`unknown screen ${p.screen}`);
    if (p.overlay && !OVERLAYS.includes(p.overlay)) say(`unknown overlay ${p.overlay}`);
    if (p.overlay && !['add', 'edit'].includes(p.screen)) say('the category-picker overlay needs screen add or edit');
    if (p.file) {
      const file = resolveInside(config.abs.root, p.file);
      const inRoot = file && config.abs.previewRoots.some(r => file.startsWith(r + path.sep));
      if (!inRoot || !fs.existsSync(file)) say(`prototype file ${p.file} is missing or outside the preview roots`);
    }
  }
  if (entry.unavailable && !UNAVAILABLE.includes(entry.unavailable.kind)) say(`unavailable.kind must be one of ${UNAVAILABLE.join(', ')}`);
  if (entry.status) {
    if (!['backlog', 'in_progress', 'in_review'].includes(entry.status.value)) say('manual status may only be backlog, in_progress or in_review; verified and blocked come from records');
    if (!entry.status.source) say('manual status needs a source document');
  }
  return problems;
}
