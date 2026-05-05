// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Semver;

/// <summary>
/// Minimal NPM-flavored semver range matcher. Mirrors the subset of
/// <a href="https://github.com/npm/node-semver">node-semver</a> that
/// <c>@actions/tool-cache</c> uses: exact versions, comparators
/// (<c>=</c>, <c>!=</c>, <c>&gt;</c>, <c>&gt;=</c>, <c>&lt;</c>,
/// <c>&lt;=</c>), x-ranges (<c>1.x</c>, <c>1.2.x</c>, <c>1</c>, <c>1.2</c>,
/// <c>*</c>), tilde (<c>~</c>), caret (<c>^</c>), hyphen ranges
/// (<c>1.2.3 - 2.3.4</c>), space-AND, and <c>||</c>-OR.
/// </summary>
public static class NpmVersionRange
{
    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="version"/>
    /// satisfies <paramref name="range"/>. The .NET equivalent of
    /// <c>semver.satisfies(version, range)</c>.
    /// </summary>
    public static bool Satisfies(string? version, string? range)
    {
        if (!NpmVersion.TryParse(version, out var v))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(range) || range.Trim() == "*")
        {
            return !v.IsPreRelease;
        }

        var groups = range.Split("||", StringSplitOptions.None);
        foreach (var g in groups)
        {
            if (Match(v, g.Trim()))
            {
                return true;
            }
        }
        return false;
    }

    private static bool Match(NpmVersion v, string andGroup)
    {
        if (andGroup.Length == 0)
        {
            return !v.IsPreRelease;
        }

        var hyphen = SplitHyphen(andGroup);
        if (hyphen is { } pair)
        {
            return ExpandRangePart(pair.Lower, lower: true).Pass(v) &&
                   ExpandRangePart(pair.Upper, lower: false).Pass(v);
        }

        var tokens = Tokenize(andGroup);
        foreach (var token in tokens)
        {
            var comparator = ParseComparator(token);
            if (!comparator.Pass(v))
            {
                return false;
            }
        }

        return true;
    }

    private static (string Lower, string Upper)? SplitHyphen(string range)
    {
        var idx = range.IndexOf(" - ", StringComparison.Ordinal);
        if (idx <= 0) return null;
        return (range[..idx].Trim(), range[(idx + 3)..].Trim());
    }

    private static List<string> Tokenize(string range)
    {
        var list = new List<string>();
        var sb = new StringBuilder();
        for (var i = 0; i < range.Length; i++)
        {
            var c = range[i];
            if (char.IsWhiteSpace(c))
            {
                if (sb.Length > 0)
                {
                    list.Add(sb.ToString());
                    sb.Clear();
                }
                continue;
            }

            if (sb.Length == 0 || (sb.Length > 0 && IsOperatorChar(sb[^1]) == IsOperatorChar(c)))
            {
                sb.Append(c);
            }
            else
            {
                sb.Append(c);
            }
        }

        if (sb.Length > 0)
        {
            list.Add(sb.ToString());
        }

        return list;
    }

    private static bool IsOperatorChar(char c) =>
        c is '>' or '<' or '=' or '!' or '~' or '^';

    private static Comparator ParseComparator(string token)
    {
        if (token.Length == 0)
        {
            return Comparator.Any;
        }

        if (token[0] == '~')
        {
            return ExpandTilde(token[1..]);
        }

        if (token[0] == '^')
        {
            return ExpandCaret(token[1..]);
        }

        if (token.StartsWith(">=", StringComparison.Ordinal))
        {
            return SimpleOp(token[2..], Op.Gte);
        }
        if (token.StartsWith("<=", StringComparison.Ordinal))
        {
            return SimpleOp(token[2..], Op.Lte);
        }
        if (token.StartsWith("!=", StringComparison.Ordinal))
        {
            return SimpleOp(token[2..], Op.Neq);
        }
        if (token[0] == '>')
        {
            return SimpleOp(token[1..], Op.Gt);
        }
        if (token[0] == '<')
        {
            return SimpleOp(token[1..], Op.Lt);
        }
        if (token[0] == '=')
        {
            token = token[1..];
        }

        // Bare token: could be 1.2.3, 1.2, 1, 1.x, 1.2.x, *, x, X.
        return ExpandXRange(token);
    }

    private static Comparator SimpleOp(string raw, Op op)
    {
        if (TryFillIn(raw, out var v))
        {
            return new Comparator(op, v);
        }
        return Comparator.None;
    }

    private static Comparator ExpandRangePart(string token, bool lower)
    {
        if (TryFillIn(token, out var v))
        {
            return new Comparator(lower ? Op.Gte : Op.Lte, v);
        }

        // Hyphen ranges with x get reinterpreted: lower coerces "x" to 0,
        // upper coerces "x" to a half-open ceiling.
        var (M, m, p, hasMinor, hasPatch) = SplitXRange(token);
        if (lower)
        {
            return new Comparator(Op.Gte, V(M, m, p));
        }
        if (!hasMinor) return new Comparator(Op.Lt, V(M + 1, 0, 0));
        if (!hasPatch) return new Comparator(Op.Lt, V(M, m + 1, 0));
        return new Comparator(Op.Lte, V(M, m, p));
    }

    private static Comparator ExpandTilde(string token)
    {
        var (M, m, p, hasMinor, hasPatch) = SplitXRange(token);
        var lower = V(M, m, p);
        Comparator upper;
        if (hasMinor || hasPatch)
        {
            // ~1.2.3 := >=1.2.3 <1.3.0
            upper = new Comparator(Op.Lt, V(M, m + 1, 0));
        }
        else
        {
            // ~1 := >=1.0.0 <2.0.0
            upper = new Comparator(Op.Lt, V(M + 1, 0, 0));
        }
        return Comparator.Combine(new Comparator(Op.Gte, lower), upper);
    }

    private static Comparator ExpandCaret(string token)
    {
        var (M, m, p, _, _) = SplitXRange(token);
        var lower = V(M, m, p);
        Comparator upper;
        if (M > 0)
        {
            // ^1.2.3 := >=1.2.3 <2.0.0
            upper = new Comparator(Op.Lt, V(M + 1, 0, 0));
        }
        else if (m > 0)
        {
            // ^0.2.3 := >=0.2.3 <0.3.0
            upper = new Comparator(Op.Lt, V(M, m + 1, 0));
        }
        else
        {
            // ^0.0.3 := >=0.0.3 <0.0.4
            upper = new Comparator(Op.Lt, V(M, m, p + 1));
        }
        return Comparator.Combine(new Comparator(Op.Gte, lower), upper);
    }

    private static Comparator ExpandXRange(string token)
    {
        if (token.Length == 0)
        {
            return Comparator.Any;
        }
        if (TryFillIn(token, out var exact) && !ContainsX(token))
        {
            return new Comparator(Op.Eq, exact);
        }

        if (token == "*" || token == "x" || token == "X")
        {
            return Comparator.Any;
        }

        var (M, m, p, hasMinor, hasPatch) = SplitXRange(token);
        var lower = V(M, m, p);
        Comparator upper;
        if (!hasMinor)
        {
            upper = new Comparator(Op.Lt, V(M + 1, 0, 0));
        }
        else if (!hasPatch)
        {
            upper = new Comparator(Op.Lt, V(M, m + 1, 0));
        }
        else
        {
            return new Comparator(Op.Eq, V(M, m, p));
        }
        return Comparator.Combine(new Comparator(Op.Gte, lower), upper);
    }

    private static bool ContainsX(string token)
    {
        foreach (var c in token)
        {
            if (c is 'x' or 'X' or '*') return true;
        }
        return false;
    }

    private static bool TryFillIn(string token, out NpmVersion v)
    {
        v = default;
        if (string.IsNullOrWhiteSpace(token) || ContainsX(token))
        {
            return false;
        }
        if (token.Length > 0 && (token[0] == 'v' || token[0] == 'V' || token[0] == '='))
        {
            token = token[1..];
        }

        // Pad with .0 for missing parts (e.g. "1" => "1.0.0").
        var idx = token.IndexOfAny(['-', '+']);
        var core = idx < 0 ? token : token[..idx];
        var suffix = idx < 0 ? string.Empty : token[idx..];
        var parts = core.Split('.');
        if (parts.Length is 1 or 2)
        {
            var major = parts[0];
            var minor = parts.Length >= 2 ? parts[1] : "0";
            core = $"{major}.{minor}.0";
            token = core + suffix;
        }

        return NpmVersion.TryParse(token, out v);
    }

    private static (int M, int m, int p, bool HasMinor, bool HasPatch) SplitXRange(string token)
    {
        if (token.Length > 0 && (token[0] == 'v' || token[0] == 'V' || token[0] == '='))
        {
            token = token[1..];
        }
        var dashIdx = token.IndexOfAny(['-', '+']);
        if (dashIdx >= 0)
        {
            token = token[..dashIdx];
        }

        var parts = token.Split('.');
        var M = ParsePartOrZero(parts.Length > 0 ? parts[0] : "0");
        var m = parts.Length >= 2 ? ParsePartOrZero(parts[1]) : 0;
        var p = parts.Length >= 3 ? ParsePartOrZero(parts[2]) : 0;
        var hasMinor = parts.Length >= 2 && !IsX(parts[1]);
        var hasPatch = parts.Length >= 3 && !IsX(parts[2]);
        return (M, m, p, hasMinor, hasPatch);
    }

    private static bool IsX(string s) => s is "x" or "X" or "*" or "";

    private static int ParsePartOrZero(string s)
    {
        if (IsX(s)) return 0;
        return int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var v) ? v : 0;
    }

    private static NpmVersion V(int M, int m, int p) =>
        new(M, m, p, Array.Empty<string>(), string.Empty);

    private enum Op { Eq, Neq, Gt, Gte, Lt, Lte, Any, None }

    private sealed record Comparator(Op Op, NpmVersion V, Comparator? Inner = null)
    {
        public static Comparator Any { get; } = new(Op.Any, default, null);
        public static Comparator None { get; } = new(Op.None, default, null);

        public static Comparator Combine(Comparator a, Comparator b) => a with { Inner = b };

        public bool Pass(NpmVersion v) => Op switch
        {
            Op.Any => !v.IsPreRelease,
            Op.None => false,
            Op.Eq => v.CompareTo(V) == 0,
            Op.Neq => v.CompareTo(V) != 0,
            Op.Gt => v.CompareTo(V) > 0 && (!v.IsPreRelease || SamePreReleaseTuple(v, V)),
            Op.Gte => v.CompareTo(V) >= 0 && (!v.IsPreRelease || SamePreReleaseTuple(v, V)),
            Op.Lt => v.CompareTo(V) < 0 && (!v.IsPreRelease || SamePreReleaseTuple(v, V)),
            Op.Lte => v.CompareTo(V) <= 0 && (!v.IsPreRelease || SamePreReleaseTuple(v, V)),
            _ => false,
        } && (Inner is null || Inner.Pass(v));

        private static bool SamePreReleaseTuple(NpmVersion x, NpmVersion y) =>
            x.Major == y.Major && x.Minor == y.Minor && x.Patch == y.Patch && y.IsPreRelease;
    }
}
