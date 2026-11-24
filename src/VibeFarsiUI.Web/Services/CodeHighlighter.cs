using System.Net;
using System.Text.RegularExpressions;

namespace VibeFarsiUI.Web.Services;

/// <summary>
/// Lightweight, dependency-free syntax highlighter. It works line by line so the
/// viewer can show stable line numbers; deliberately not a full parser.
/// </summary>
public static partial class CodeHighlighter
{
    private static readonly HashSet<string> CsKeywords =
    [
        "public", "private", "protected", "internal", "static", "readonly", "class", "record", "struct",
        "interface", "enum", "void", "string", "int", "double", "bool", "decimal", "long", "var", "new",
        "return", "if", "else", "foreach", "for", "while", "switch", "case", "break", "continue", "using",
        "namespace", "get", "set", "init", "async", "await", "true", "false", "null", "this", "base",
        "override", "virtual", "abstract", "sealed", "partial", "where", "out", "ref", "in", "is", "as"
    ];

    private static readonly HashSet<string> JsKeywords =
    [
        "const", "let", "var", "function", "return", "if", "else", "for", "while", "import", "export",
        "from", "default", "class", "extends", "new", "true", "false", "null", "undefined", "async",
        "await", "try", "catch", "finally", "throw", "typeof", "interface", "type", "as", "of", "in"
    ];

    private static readonly HashSet<string> CssKeywords =
    [
        "var", "calc", "color-mix", "oklch", "rgba", "rgb", "linear-gradient", "radial-gradient",
        "repeat", "minmax", "auto", "none", "inherit", "transparent", "important"
    ];

    public static string HighlightLine(string? line, string? language)
    {
        if (string.IsNullOrEmpty(line)) return string.Empty;
        var lang = (language ?? "text").ToLowerInvariant();
        return lang switch
        {
            "csharp" or "cs" or "razor" => HighlightCode(line, CsKeywords, razor: lang == "razor"),
            "js" or "javascript" or "ts" or "typescript" => HighlightCode(line, JsKeywords),
            "css" => HighlightCss(line),
            "json" => HighlightJson(line),
            "html" or "xml" => HighlightHtml(line),
            _ => WebUtility.HtmlEncode(line)
        };
    }

    private static string HighlightCode(string line, HashSet<string> keywords, bool razor = false)
    {
        var pattern = @"(?<com>//.*$|/\*.*?\*/)|(?<str>""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*')|(?<num>\b\d+(?:\.\d+)?\b)|(?<word>\b[A-Za-z_][A-Za-z0-9_]*\b)|(?<razor>@[A-Za-z_][A-Za-z0-9_]*)";
        return Regex.Replace(line, pattern, match =>
        {
            if (match.Groups["com"].Success) return Span("com", match.Value);
            if (match.Groups["str"].Success) return Span("str", match.Value);
            if (match.Groups["num"].Success) return Span("num", match.Value);
            if (match.Groups["razor"].Success) return Span("kw", match.Value);
            var word = match.Value;
            if (keywords.Contains(word)) return Span("kw", word);
            if (word.Length > 1 && char.IsUpper(word[0])) return Span("type", word);
            return WebUtility.HtmlEncode(word);
        });
    }

    private static string HighlightCss(string line)
    {
        var pattern = @"(?<com>/\*.*?\*/)|(?<str>""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*')|(?<prop>[a-zA-Z-]+(?=\s*:))|(?<num>#[0-9a-fA-F]{3,8}|\b\d+(?:\.\d+)?(?:rem|px|em|%|vh|vw|s|ms|ch|fr)?\b)|(?<at>@[a-zA-Z-]+)|(?<word>--[a-zA-Z0-9-]+)";
        return Regex.Replace(line, pattern, match =>
        {
            if (match.Groups["com"].Success) return Span("com", match.Value);
            if (match.Groups["str"].Success) return Span("str", match.Value);
            if (match.Groups["at"].Success) return Span("kw", match.Value);
            if (match.Groups["prop"].Success) return Span("key", match.Value);
            if (match.Groups["num"].Success) return Span("num", match.Value);
            if (match.Groups["word"].Success) return Span("type", match.Value);
            return WebUtility.HtmlEncode(match.Value);
        });
    }

    private static string HighlightJson(string line)
    {
        var pattern = @"(?<key>""(?:\\.|[^""\\])*""(?=\s*:))|(?<str>""(?:\\.|[^""\\])*"")|(?<num>-?\b\d+(?:\.\d+)?\b)|(?<lit>\btrue\b|\bfalse\b|\bnull\b)";
        return Regex.Replace(line, pattern, match =>
        {
            if (match.Groups["key"].Success) return Span("key", match.Value);
            if (match.Groups["str"].Success) return Span("str", match.Value);
            if (match.Groups["num"].Success) return Span("num", match.Value);
            if (match.Groups["lit"].Success) return Span("kw", match.Value);
            return WebUtility.HtmlEncode(match.Value);
        });
    }

    private static string HighlightHtml(string line)
    {
        var pattern = @"(?<com><!--.*?-->)|(?<str>""[^""]*""|'[^']*')|(?<tag></?[a-zA-Z][\w-]*)|(?<razor>@[A-Za-z_][\w.]*)|(?<attr>\b[a-zA-Z-]+(?==))";
        return Regex.Replace(line, pattern, match =>
        {
            if (match.Groups["com"].Success) return Span("com", match.Value);
            if (match.Groups["str"].Success) return Span("str", match.Value);
            if (match.Groups["tag"].Success) return Span("tag", match.Value);
            if (match.Groups["razor"].Success) return Span("kw", match.Value);
            if (match.Groups["attr"].Success) return Span("key", match.Value);
            return WebUtility.HtmlEncode(match.Value);
        });
    }

    private static string Span(string kind, string text)
        => $"<span class=\"tok-{kind}\">{WebUtility.HtmlEncode(text)}</span>";
}
