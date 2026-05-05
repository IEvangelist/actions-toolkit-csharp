// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Semver;

/// <summary>
/// Minimal NPM-flavored semantic version. Mirrors the upstream
/// <a href="https://github.com/npm/node-semver">node-semver</a> features used
/// by <c>@actions/tool-cache</c>: parse, compare, clean, and validity checks.
/// </summary>
/// <remarks>
/// <para>
/// Supported grammar:
/// <code>
/// version    := [v]MAJOR.MINOR.PATCH[-PRE][+BUILD]
/// </code>
/// <c>PRE</c> follows the <see href="https://semver.org/">SemVer 2.0.0</see>
/// rule of dot-separated identifiers; <c>BUILD</c> metadata is preserved on
/// the parsed instance but is ignored for ordering, matching upstream
/// behavior. Hand-rolled to avoid a dependency on <c>NuGet.Versioning</c>.
/// </para>
/// </remarks>
public readonly record struct NpmVersion(
    int Major,
    int Minor,
    int Patch,
    IReadOnlyList<string> PreRelease,
    string Build) : IComparable<NpmVersion>
{
    /// <summary>
    /// Whether this version has a pre-release suffix (e.g. <c>1.2.3-beta</c>).
    /// </summary>
    public bool IsPreRelease => PreRelease.Count > 0;

    /// <summary>Returns whether <paramref name="left"/> precedes <paramref name="right"/>.</summary>
    public static bool operator <(NpmVersion left, NpmVersion right) => left.CompareTo(right) < 0;
    /// <summary>Returns whether <paramref name="left"/> precedes or equals <paramref name="right"/>.</summary>
    public static bool operator <=(NpmVersion left, NpmVersion right) => left.CompareTo(right) <= 0;
    /// <summary>Returns whether <paramref name="left"/> succeeds <paramref name="right"/>.</summary>
    public static bool operator >(NpmVersion left, NpmVersion right) => left.CompareTo(right) > 0;
    /// <summary>Returns whether <paramref name="left"/> succeeds or equals <paramref name="right"/>.</summary>
    public static bool operator >=(NpmVersion left, NpmVersion right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// Tries to parse a version string. Tolerates a leading <c>v</c> prefix
    /// and surrounding whitespace.
    /// </summary>
    public static bool TryParse(string? input, out NpmVersion version)
    {
        version = default;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var s = input.Trim();
        if (s.Length > 0 && (s[0] == 'v' || s[0] == 'V' || s[0] == '='))
        {
            s = s[1..].TrimStart();
        }

        var build = string.Empty;
        var plusIdx = s.IndexOf('+', StringComparison.Ordinal);
        if (plusIdx >= 0)
        {
            build = s[(plusIdx + 1)..];
            s = s[..plusIdx];
        }

        IReadOnlyList<string> pre = Array.Empty<string>();
        var dashIdx = s.IndexOf('-', StringComparison.Ordinal);
        if (dashIdx >= 0)
        {
            var preStr = s[(dashIdx + 1)..];
            s = s[..dashIdx];
            if (preStr.Length == 0)
            {
                return false;
            }

            var ids = preStr.Split('.');
            foreach (var id in ids)
            {
                if (id.Length == 0)
                {
                    return false;
                }

                foreach (var c in id)
                {
                    if (!IsValidIdentifierChar(c))
                    {
                        return false;
                    }
                }
            }
            pre = ids;
        }

        var parts = s.Split('.');
        if (parts.Length != 3)
        {
            return false;
        }

        if (!TryParsePart(parts[0], out var major) ||
            !TryParsePart(parts[1], out var minor) ||
            !TryParsePart(parts[2], out var patch))
        {
            return false;
        }

        version = new NpmVersion(major, minor, patch, pre, build);
        return true;
    }

    /// <summary>
    /// Returns the canonical <c>MAJOR.MINOR.PATCH[-PRE]</c> form of a version
    /// string, or <see langword="null"/> if the input cannot be parsed. This
    /// is the C# equivalent of upstream <c>semver.clean</c>.
    /// </summary>
    public static string? Clean(string? input) =>
        TryParse(input, out var v) ? v.ToString() : null;

    /// <summary>
    /// Returns whether <paramref name="input"/> is a fully-qualified explicit
    /// semantic version (e.g. <c>1.2.3</c>). Mirrors upstream
    /// <c>tc.isExplicitVersion</c>.
    /// </summary>
    public static bool IsExplicit(string? input) =>
        Clean(input) is { Length: > 0 };

    /// <inheritdoc />
    public int CompareTo(NpmVersion other)
    {
        var c = Major.CompareTo(other.Major);
        if (c != 0) return c;
        c = Minor.CompareTo(other.Minor);
        if (c != 0) return c;
        c = Patch.CompareTo(other.Patch);
        if (c != 0) return c;
        return ComparePreRelease(PreRelease, other.PreRelease);
    }

    /// <summary>
    /// Returns the canonical <c>MAJOR.MINOR.PATCH[-PRE]</c> form of this
    /// version. Build metadata is intentionally omitted from this projection,
    /// matching upstream's clean output.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Major).Append('.').Append(Minor).Append('.').Append(Patch);
        if (PreRelease.Count > 0)
        {
            sb.Append('-');
            sb.Append(string.Join(".", PreRelease));
        }
        return sb.ToString();
    }

    private static int ComparePreRelease(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        // SemVer 2.0.0 §11: A version with a pre-release tag has lower
        // precedence than the same version without one.
        if (a.Count == 0 && b.Count == 0) return 0;
        if (a.Count == 0) return 1;
        if (b.Count == 0) return -1;

        var min = Math.Min(a.Count, b.Count);
        for (var i = 0; i < min; i++)
        {
            var c = CompareIdentifier(a[i], b[i]);
            if (c != 0) return c;
        }
        return a.Count.CompareTo(b.Count);
    }

    private static int CompareIdentifier(string a, string b)
    {
        var aIsNum = TryParsePart(a, out var an);
        var bIsNum = TryParsePart(b, out var bn);
        if (aIsNum && bIsNum) return an.CompareTo(bn);
        if (aIsNum) return -1;
        if (bIsNum) return 1;
        return string.CompareOrdinal(a, b);
    }

    private static bool TryParsePart(string s, out int value)
    {
        value = 0;
        if (s.Length == 0) return false;

        // Reject leading zero (except the literal "0").
        if (s.Length > 1 && s[0] == '0') return false;

        foreach (var c in s)
        {
            if (c < '0' || c > '9') return false;
        }
        return int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    private static bool IsValidIdentifierChar(char c) =>
        (c >= '0' && c <= '9') ||
        (c >= 'A' && c <= 'Z') ||
        (c >= 'a' && c <= 'z') ||
        c == '-';
}
