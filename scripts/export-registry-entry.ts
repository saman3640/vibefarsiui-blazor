// Build-time export of the Next.js registry metadata into the Blazor app.
// Run through scripts/export-registry.mjs (esbuild bundles the TS imports).
import { mkdirSync, readFileSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { EOL } from "node:os";
import path from "node:path";
import { buildCatalog } from "../lib/registry/catalog";
import { components } from "../lib/registry/components";
import { animations } from "../lib/registry/animations";
import { backgrounds } from "../lib/registry/backgrounds";
import { charts } from "../lib/registry/charts";
import { calendar } from "../lib/registry/calendar";
import { templates } from "../lib/registry/templates";
import { blocks } from "../lib/registry/blocks";
import { sites } from "../lib/registry/sites";
import { themes } from "../lib/registry/themes";
import { hostedSkills, skillTarget } from "../lib/registry/skills";
import { libs } from "../lib/registry/libs";
import { buildPrompt, buildThemePrompt } from "../lib/registry/prompt";

const outDir = "src/VibeFarsiUI.Web/wwwroot/registry";
mkdirSync(outDir, { recursive: true });

/* ---------- catalog ---------- */
const catalog = buildCatalog("https://vibefarsi.ir");
writeFileSync(`${outDir}/catalog.json`, JSON.stringify(catalog, null, 2) + "\n");

/* ---------- per-item documents, contract-compatible with /r/{type}/{slug}.json ---------- */
const toUserSource = (src: string) =>
  src
    .replace(/@\/registry\/ui\//g, "@/components/ui/")
    .replace(/@\/registry\/animations\//g, "@/components/animations/")
    .replace(/@\/registry\/backgrounds\//g, "@/components/backgrounds/")
    .replace(/@\/registry\/charts\//g, "@/components/charts/")
    .replace(/@\/registry\/calendar\//g, "@/components/calendar/")
    .replace(/@\/registry\/templates\//g, "@/components/templates/")
    .replace(/@\/registry\/blocks\//g, "@/components/blocks/")
    .replace(/@\/registry\/sites\//g, "@/components/sites/")
    .trimEnd();

const readSource = (file: string) => toUserSource(readFileSync(path.resolve(process.cwd(), file), "utf8"));

const userPath = (type: string, file: string) => {
  if (type === "themes") return "app/globals.css";
  if (type === "lib") return file;
  const name = file.split("/").pop()!;
  if (type === "components") return `components/ui/${name}`;
  if (type === "animations") return `components/animations/${name}`;
  if (type === "backgrounds") return `components/backgrounds/${name}`;
  if (type === "charts") return `components/charts/${name}`;
  if (type === "calendars") return `components/calendar/${name}`;
  if (type === "blocks") return `components/blocks/${name}`;
  return `components/templates/${name}`;
};

const lists: Record<string, any[]> = {
  components,
  charts,
  calendars: calendar,
  animations,
  backgrounds,
  templates,
  blocks,
  themes,
  lib: libs,
  skills: hostedSkills,
};

const kindOf: Record<string, string> = {
  components: "component",
  charts: "chart",
  calendars: "calendar",
  animations: "animation",
  backgrounds: "background",
  templates: "template",
  blocks: "block",
  lib: "lib",
  skills: "skill",
};

const documents: Record<string, unknown> = {};

for (const [type, list] of Object.entries(lists)) {
  for (const item of list) {
    const isTheme = type === "themes";
    const isLib = type === "lib";
    const isSkill = type === "skills";
    const kind = kindOf[type] ?? "component";
    const content = isTheme
      ? readFileSync(path.resolve(process.cwd(), item.file), "utf8")
      : readSource(item.file);
    const prompt = isTheme
      ? buildThemePrompt(item)
      : isSkill
        ? undefined
        : buildPrompt(item, kind as any);
    const fileKind = isTheme
      ? "registry:style"
      : isLib
        ? "registry:lib"
        : isSkill
          ? "registry:skill"
          : "registry:component";

    const target = isSkill && item.format ? skillTarget(item) : userPath(type, item.file);
    documents[`${type}/${item.slug}`] = {
      $schema: "https://vibefarsi.dev/schema/registry-item.json",
      name: item.slug,
      type: `registry:${isLib ? "lib" : type.replace(/s$/, "")}`,
      title: item.name,
      description: item.desc,
      dependencies: item.deps ?? [],
      registryDependencies: item.registryDeps ?? [],
      files: [{ path: target, content, type: fileKind }],
      ...(item.css ? { css: item.css } : {}),
      ...(prompt ? { prompt } : {}),
    };
  }
}

/* ---------- whole sites: every file in the folder, plus its photos ---------- */
for (const site of sites) {
  const root = path.resolve(process.cwd(), site.dir);
  const files: { path: string; content: string; type: string }[] = [];
  const walk = (dir: string) => {
    for (const entry of readdirSync(dir)) {
      const full = path.join(dir, entry);
      if (statSync(full).isDirectory()) walk(full);
      else {
        const rel = path.relative(root, full).split(path.sep).join("/");
        files.push({
          path: `components/sites/${site.slug}/${rel}`,
          content: toUserSource(readFileSync(full, "utf8")),
          type: "registry:component",
        });
      }
    }
  };
  if (statSync(root).isDirectory()) walk(root);
  for (const page of site.pages) {
    const target = page.path ? `app/${page.path}/page.tsx` : "app/page.tsx";
    files.push({
      path: target,
      content: `export { ${page.export} as default } from "@/components/sites/${site.slug}/${page.file.replace(/\.tsx$/, "")}";` + EOL,
      type: "registry:page",
    });
  }
  documents[`sites/${site.slug}`] = {
    $schema: "https://vibefarsi.dev/schema/registry-item.json",
    name: site.slug,
    type: "registry:site",
    title: site.name,
    description: site.desc,
    dependencies: site.deps ?? [],
    registryDependencies: site.registryDeps ?? [],
    files,
    prompt: buildPrompt(site as any, "component"),
  };
}

writeFileSync(`${outDir}/documents.json`, JSON.stringify(documents) + EOL);

const counts = new Map<string, number>();
for (const item of catalog.items) counts.set(item.type, (counts.get(item.type) ?? 0) + 1);
console.log(`Exported ${catalog.items.length} catalog items and ${Object.keys(documents).length} documents.`);
console.log([...counts.entries()].map(([type, count]) => `${type}: ${count}`).join(", "));
