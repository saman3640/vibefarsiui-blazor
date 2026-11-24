#!/usr/bin/env node
/**
 * Regenerates src/VibeFarsiUI.Web/wwwroot/registry/catalog.json from the
 * TypeScript metadata in lib/registry. The Blazor app never runs Next.js:
 * this is a one-time (or on-change) build step.
 *
 * Requires network on first run so npx can fetch esbuild.
 */
import { execSync } from "node:child_process";
import { mkdirSync } from "node:fs";

mkdirSync(".tmp", { recursive: true });
execSync(
  "npx --yes esbuild scripts/export-registry-entry.ts --bundle --platform=node --format=esm --outfile=.tmp/export-registry.mjs",
  { stdio: "inherit" },
);
execSync(`${JSON.stringify(process.execPath)} .tmp/export-registry.mjs`, { stdio: "inherit" });
