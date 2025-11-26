using System;
using System.Collections.Generic;

namespace BetterBetterTeleporter.Utility;

public static class ItemParser
{
    public static List<ItemRule> ParseConfig(string source)
    {
        if (string.IsNullOrEmpty(source)) return [];
        List<ItemRule> itemRules = [];

        var idx = IndexOfUnescaped(source, ',');
        if (idx == null) return [ParseItemRule(source)];

        foreach (var piece in SplitCurrentGeneration(source))
        {
            itemRules.Add(ParseItemRule(piece));
        }
        return itemRules;
    }

    private static ItemRule ParseItemRule(string source)
    {
        if (source[0] != '[' || source[^1] != ']') return new ItemNameRule(Unescape(source));

        var filter = source[1..^1].ToLowerInvariant().Split(":", 2, StringSplitOptions.RemoveEmptyEntries);

        bool isExclusive = false;
        List<ItemRule> itemRules = [];
        if (filter.Length > 1)
        {
            isExclusive = filter[1].StartsWith("not(") && filter[1][^1] == ')';
            itemRules = ParseConfig(isExclusive ? filter[1][4..^1] : filter[1]);
        }
        return ItemRules.FromId(Unescape(filter[0]), new(isExclusive, itemRules));
    }

    private static List<string> SplitCurrentGeneration(string source)
    {
        var parts = new List<string>();
        int depth = 0;
        int start = 0;

        for (int i = 0; i < source.Length; i++)
        {
            switch (source[i])
            {
                case '[':
                    depth++;
                    break;
                case ']':
                    depth = Math.Max(depth - 1, 0);
                    break;
                case ',':
                    if (depth != 0) continue;
                    if (i - start > 0) parts.Add(source[start..i]);
                    start = i + 1;
                    break;
            }
        }
        if (start < source.Length) parts.Add(source[start..]);
        return parts;
    }

    private static int? IndexOfUnescaped(string source, char delimiter, char escapeChar = '\\')
    {
        if (string.IsNullOrEmpty(source)) return null;
        bool escaped = false;
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == escapeChar) escaped = !escaped;
            else if (source[i] == delimiter && !escaped) return i;
            else escaped = false;
        }
        return null;
    }

    private static string Unescape(string source, char escapeChar = '\\')
    {
        bool escaped = false;
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == escapeChar && !escaped) escaped = !escaped;
            else if (source[i] == escapeChar) source = source.Remove(i, 1);
            else escaped = false;
        }
        return source;
    }
}
