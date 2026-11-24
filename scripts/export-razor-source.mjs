// Exports the Razor sources of the native components so the Blazor code viewer
// can show real implementation code at runtime (published builds included).
import { readFileSync, writeFileSync, readdirSync, statSync } from "node:fs";
import { join, basename, extname } from "node:path";

const root = "src/VibeFarsiUI.Web/Components/UI";
const output = "src/VibeFarsiUI.Web/wwwroot/registry/razor-source.json";
const sources = {};

function walk(dir) {
  for (const entry of readdirSync(dir)) {
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) walk(full);
    else if (extname(full) === ".razor") {
      sources[basename(full, ".razor")] = readFileSync(full, "utf8");
    }
  }
}

walk(root);
writeFileSync(output, JSON.stringify(sources, null, 2));
console.log(`Exported ${Object.keys(sources).length} Razor sources to ${output}`);
