#!/usr/bin/env python3
"""Contracts for the Blazor stack: dataset, search routing and design-system profile."""

import csv
import re
import subprocess
import sys
import unittest
from pathlib import Path
from urllib.parse import urlsplit

SCRIPTS_DIR = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SCRIPTS_DIR))

from core import (  # noqa: E402
    AVAILABLE_STACKS,
    DATA_DIR,
    STACK_CONFIG,
    STACK_CURRENT_APPLICABILITY,
    search_stack,
)
from design_system import generate_design_system  # noqa: E402
from validate_data import (  # noqa: E402
    STACK_OFFICIAL_HOSTS,
    _check_stack_freshness_contract,
)

SEARCH = SCRIPTS_DIR / "search.py"
REQUIRED_COLUMNS = {
    "No", "Category", "Guideline", "Description", "Do", "Don't",
    "Code Good", "Code Bad", "Severity", "Docs URL", "Applies To",
    "Status", "Verified At",
}
FORBIDDEN_CODE_TOKENS = ("usestate", "classname=", "v-if", "v-for", "ngfor", "ngif")


def read_blazor_rows():
    path = DATA_DIR / STACK_CONFIG["blazor"]["file"]
    with path.open(encoding="utf-8", newline="") as handle:
        return list(csv.DictReader(handle))


class TestBlazorDataset(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows = read_blazor_rows()
        cls.corpus = " ".join(
            " ".join(row.values()) for row in cls.rows
        ).casefold()

    def test_blazor_is_a_registered_stack(self):
        self.assertIn("blazor", AVAILABLE_STACKS)
        self.assertEqual(STACK_CONFIG["blazor"]["file"], "stacks/blazor.csv")
        self.assertEqual(STACK_CURRENT_APPLICABILITY["blazor"], "blazor 8+")

    def test_dataset_has_the_uniform_stack_schema(self):
        path = DATA_DIR / STACK_CONFIG["blazor"]["file"]
        with path.open(encoding="utf-8", newline="") as handle:
            header = next(csv.reader(handle))
        self.assertEqual(set(header), REQUIRED_COLUMNS)
        numbers = [row["No"] for row in self.rows]
        self.assertEqual(numbers, [str(index) for index in range(1, len(numbers) + 1)])
        for row in self.rows:
            with self.subTest(row=row["No"]):
                self.assertTrue(row["Guideline"].strip())
                self.assertTrue(row["Description"].strip())
                self.assertTrue(row["Do"].strip())
                self.assertTrue(row["Don't"].strip())
                self.assertIn(row["Status"], {"active", "deprecated"})

    def test_applicability_rows_target_one_generation(self):
        for row in self.rows:
            with self.subTest(row=row["No"]):
                applies_to = row["Applies To"].casefold()
                self.assertTrue(applies_to.startswith("blazor"))
                if row["Status"] == "active":
                    self.assertTrue(applies_to.startswith("blazor 8+"))
                    self.assertNotIn("legacy", applies_to)
                else:
                    self.assertIn("legacy", applies_to)

    def test_high_impact_rows_cite_official_documentation(self):
        hosts = STACK_OFFICIAL_HOSTS["blazor"]
        for row in self.rows:
            if row["Severity"] not in {"Critical", "High"}:
                continue
            with self.subTest(row=row["No"]):
                parsed = urlsplit(row["Docs URL"])
                self.assertEqual(parsed.scheme, "https")
                self.assertIn(parsed.hostname, hosts)

    def test_curated_freshness_contract_passes(self):
        problems = []
        _check_stack_freshness_contract("blazor", self.rows, problems)
        self.assertEqual([], problems)

    def test_required_profiles_and_topics_are_covered(self):
        required_phrases = (
            # app models and rendering
            "static ssr", "interactive server", "interactivewebassembly",
            "interactive auto", "prerender", "render mode", "@rendermode",
            "signalr", "reconnect", "circuit",
            # components and syntax
            "razor component", "code-behind", ".razor.css", "@typeparam",
            "@key", "@ref", "eventcallback", "cascadingvalue",
            "idisposable", "onparametersset", "onafterrender", "statehaschanged",
            "shouldrender",
            # styling and design system
            "css isolation", "css custom propert", "design token", "dark",
            "flexbox", "grid", "container quer", "prefers-reduced-motion",
            # forms
            "editform", "inputtext", "inputnumber", "inputselect",
            "inputcheckbox", "inputdate", "dataannotationsvalidator",
            "validationmessage", "validationsummary", "navigationlock",
            # navigation and structure
            "router", "navlink", "navigationmanager", "authorizeview",
            "notauthorized", "notfound", "mainlayout", "breadcrumb",
            # enterprise and responsive
            "enterprise", "dashboard", "data grid", "bulk", "audit",
            "role", "empty", "loading", "375", "1440", "44px", "safe area",
            # accessibility
            "aria", "keyboard", "focus", "wcag", "high-contrast",
            # interop, performance, maui, rtl
            "ijsruntime", "ijsobjectreference", "interop", "virtualize",
            "webassembly", "download size", "maui", "android", "ios",
            "windows", "macos", "rtl", "dir=\"rtl\"", "logical propert",
            "persian", "arabic", "hebrew",
            # component libraries
            "mudblazor", "radzen", "fluent ui", "syncfusion", "telerik",
            "devexpress", "blazorise", "mudtheme", "radzentheme",
        )
        for phrase in required_phrases:
            with self.subTest(phrase=phrase):
                self.assertIn(phrase, self.corpus)

    def test_render_mode_and_maui_guidance_stay_separate(self):
        maui_rows = [row for row in self.rows if "maui" in row["Applies To"]]
        self.assertTrue(maui_rows)
        self.assertIn("render modes, circuits", self.corpus)
        for row in maui_rows:
            if row["Status"] != "active":
                continue
            with self.subTest(row=row["No"]):
                self.assertIn("maui", row["Applies To"])


class TestBlazorSearch(unittest.TestCase):
    QUERIES = (
        "professional enterprise dashboard responsive layout",
        "Blazor component architecture reusable components",
        "Blazor CSS isolation styles",
        "Blazor form validation accessible errors",
        "Blazor accessible dialog focus trap",
        "Blazor navigation routing authorizeview",
        "MudBlazor theme palette dark mode",
        "Radzen data grid server paging filter",
        "MAUI Blazor Hybrid safe area touch",
        "Blazor RTL Persian logical properties",
        "Blazor performance virtualization",
    )

    def test_blazor_queries_return_blazor_guidance(self):
        for query in self.QUERIES:
            with self.subTest(query=query):
                result = search_stack(query, "blazor", max_results=3)
                self.assertGreater(result["count"], 0, query)
                self.assertEqual(result["file"], "stacks/blazor.csv")
                for row in result["results"]:
                    self.assertTrue(row["Applies To"].startswith("blazor"))

    def test_results_never_contain_other_framework_syntax(self):
        for query in self.QUERIES:
            with self.subTest(query=query):
                result = search_stack(query, "blazor", max_results=5)
                for row in result["results"]:
                    code = f"{row['Code Good']} {row['Code Bad']}".casefold()
                    for token in FORBIDDEN_CODE_TOKENS:
                        self.assertNotIn(token, code)

    def test_razor_alias_resolves_to_blazor_guidance(self):
        result = search_stack("razor component parameters eventcallback", "blazor")
        self.assertGreater(result["count"], 0)
        self.assertTrue(
            any("eventcallback" in row["Guideline"].casefold()
                or "eventcallback" in row["Description"].casefold()
                for row in result["results"])
        )

    def test_render_mode_queries_are_actionable(self):
        result = search_stack("Blazor render mode interactive server prerender", "blazor")
        self.assertGreater(result["count"], 0)
        self.assertTrue(any("render mode" in row["Guideline"].casefold()
                            for row in result["results"]))

    def test_dotnet_versions_stay_on_current_guidance(self):
        for query in ("Blazor 8 render mode", "Blazor 9 prerender", "Blazor 10 form validation"):
            with self.subTest(query=query):
                result = search_stack(query, "blazor")
                self.assertGreater(result["count"], 0, query)
                self.assertEqual({row["Status"] for row in result["results"]}, {"active"})

    def test_legacy_generation_returns_only_deprecated_rows(self):
        result = search_stack("Blazor 7 legacy server Host.cshtml circuit", "blazor")
        self.assertGreater(result["count"], 0)
        self.assertEqual({row["Status"] for row in result["results"]}, {"deprecated"})
        self.assertTrue(all("legacy" in row["Applies To"] for row in result["results"]))

    def test_unrelated_query_abstains(self):
        result = search_stack("sourdough starter crumb fermentation", "blazor")
        self.assertEqual(result["count"], 0)
        self.assertEqual(result["results"], [])

    def test_existing_stacks_still_resolve(self):
        for stack, query in (
            ("react", "useState component local state"),
            ("nextjs", "App Router server component"),
            ("avalonia", "Avalonia XAML namespace declaration"),
            ("laravel", "Laravel Blade UI components"),
        ):
            with self.subTest(stack=stack):
                self.assertGreater(search_stack(query, stack, max_results=1)["count"], 0)


class TestBlazorDesignSystem(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.result = generate_design_system(
            "enterprise CRM admin dashboard", "Ops Console",
            output_format="markdown", framework="blazor",
        )
        cls.text = cls.result["text"]
        cls.ds = cls.result["design_system"]

    def test_blazor_profile_is_recorded_and_rendered(self):
        self.assertEqual(self.ds["implementation_framework"], "blazor")
        self.assertIn("### Blazor Implementation", self.text)
        self.assertIn("Blazor Web App", self.text)
        self.assertIn("MAUI Blazor Hybrid", self.text)
        self.assertIn("Never apply Blazor Web App render-mode guidance", self.text)

    def test_profile_maps_the_generated_palette(self):
        primary = self.ds["colors"]["primary"]
        self.assertIn(primary, self.text)
        self.assertIn("new Palette", self.text)
        self.assertIn("RadzenTheme", self.text)
        self.assertIn("FluentDesignTheme", self.text)
        self.assertIn("Syncfusion", self.text)
        self.assertIn("Telerik", self.text)
        self.assertIn("DevExpress", self.text)
        self.assertIn("Blazorise", self.text)

    def test_dark_system_themes_palette_dark(self):
        result = generate_design_system(
            "enterprise CRM admin dashboard dark mode", "Ops Console",
            output_format="markdown", framework="blazor",
        )
        self.assertEqual(result["design_system"]["color_mode"], "dark")
        self.assertIn("new PaletteDark", result["text"])
        self.assertNotIn("new PaletteLight", result["text"])

    def test_profile_only_references_exported_css_variables(self):
        from design_system import _flatten_tokens, build_design_tokens

        tokens = build_design_tokens(self.ds)
        emitted = set(_flatten_tokens(tokens.get("semantic") or {}, tokens, []))
        referenced = set(re.findall(r"var\((--[a-z0-9-]+)\)", self.text))
        self.assertTrue(referenced)
        self.assertEqual(set(), referenced - emitted)

    def test_profile_includes_token_and_rtl_guidance(self):
        self.assertIn("wwwroot/css/tokens.css", self.text)
        self.assertIn("--color-primary", self.text)
        self.assertNotIn("--spacing-", self.text)
        self.assertNotIn("var(--radius", self.text)
        self.assertIn('dir="rtl"', self.text)
        self.assertIn("375px", self.text)
        self.assertIn("dotnet build", self.text)

    def test_default_design_system_is_unchanged(self):
        default = generate_design_system(
            "enterprise CRM admin dashboard", "Ops Console", output_format="markdown"
        )
        self.assertNotIn("Blazor Implementation", default["text"])
        self.assertNotIn("implementation_framework", default["design_system"])

    def test_css_tokens_are_exportable_for_blazor(self):
        result = generate_design_system(
            "enterprise CRM admin dashboard", "Ops Console",
            token_format="css", framework="blazor",
        )
        css = result["text"]
        self.assertIn(":root", css)
        self.assertIn("--color-primary:", css)
        self.assertIn("--color-background:", css)
        self.assertNotIn("Blazor Implementation", css)

    def test_invalid_framework_is_rejected(self):
        with self.assertRaises(ValueError):
            generate_design_system("dashboard", framework="react")


class TestBlazorCli(unittest.TestCase):
    def run_search(self, *args):
        return subprocess.run(
            [sys.executable, str(SEARCH), *args],
            capture_output=True, text=True, encoding="utf-8",
        )

    def test_cli_generates_a_blazor_design_system(self):
        proc = self.run_search(
            "enterprise CRM dashboard", "--design-system",
            "-f", "markdown", "--framework", "blazor",
        )
        self.assertEqual(proc.returncode, 0, proc.stderr)
        self.assertIn("Blazor Implementation", proc.stdout)

    def test_cli_rejects_framework_without_design_system(self):
        proc = self.run_search("dashboard", "--framework", "blazor")
        self.assertNotEqual(proc.returncode, 0)
        self.assertIn("--framework requires --design-system", proc.stderr)

    def test_cli_stack_search_lists_blazor(self):
        proc = self.run_search("Blazor CSS isolation", "--stack", "blazor", "-n", "1")
        self.assertEqual(proc.returncode, 0, proc.stderr)
        self.assertIn("stacks/blazor.csv", proc.stdout)


if __name__ == "__main__":
    unittest.main()
