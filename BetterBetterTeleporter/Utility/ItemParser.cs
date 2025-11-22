using System;
using System.Collections.Generic;
using System.Linq;
using BetterBetterTeleporter.Adapters;

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

    public static bool ShouldDrop(IPlayerInfo player, IItemInfo item, bool behavior, List<ItemRule> rules)
    {
        return item != null && behavior ^ !rules.Any(rule => rule.IsMatch(player, item));
    }

    private static ItemRule ParseItemRule(string source)
    {
        if (source[0] != '[' || source[^1] != ']') return new ItemNameRule(Unescape(source));

        var filter = source[1..^1].ToLowerInvariant().Split(":not", 2, StringSplitOptions.RemoveEmptyEntries);
        List<ItemRule> except = [];
        if (filter.Length > 1 && filter[1][0] == '(' && filter[1][^1] == ')')
        {
            except = ParseConfig(filter[1][1..^1]);
        }
        return RuleFilter.FromId(Unescape(filter[0]), except);
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
                    parts.Add(source[start..i]);
                    start = i + 1;
                    break;
            }
        }
        if (start <= source.Length) parts.Add(source[start..]);
        return parts;
    }

    private static int? IndexOfUnescaped(string source, char delimiter, char escapeChar = '\\')
    {
        if (string.IsNullOrEmpty(source)) return null;
        source = source.Trim(delimiter);
        bool escaped = false;
        for (int i = 1; i < source.Length - 1; i++)
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
        for (int i = 1; i < source.Length - 1; i++)
        {
            if (source[i] == escapeChar && !escaped) escaped = !escaped;
            else if (source[i] == escapeChar) source = source.Remove(i, 1);
            else escaped = false;
        }
        return source;
    }
}
