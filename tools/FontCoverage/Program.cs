using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UW.Tools.FontCoverage;

/// <summary>
/// fontcoverage &lt;font.ttf|.otf&gt; &lt;start-end&gt;...
///
/// Prints, on one line, the parts of the requested ranges the font has glyphs for, as
/// "start-end" pairs in decimal - the shape tools/build/36-build-fonts.sh writes into a
/// .spritefont's CharacterRegions. Prints nothing (and exits 0) when none are covered; exits 1
/// on an unreadable font.
///
/// WHY. MonoGame's font processor rasterises every character it is asked for, and one the
/// typeface does not have comes out as its empty .notdef box. Electrolize, the game's main UI
/// face, has no Cyrillic at all: asked for U+0400-04FF it produced 256 identical boxes. A
/// character left OUT of the SpriteFont instead falls to its DefaultCharacter, which is
/// honest about being missing and costs no texture space.
///
/// Reads the cmap table only - format 4 (BMP) and format 12 (full Unicode), from the Windows
/// Unicode or Unicode-platform subtables, preferring format 12. That covers every font a
/// SpriteFont can be built from.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: fontcoverage <font.ttf|.otf> <start-end>...");
            return 2;
        }

        HashSet<int> covered;
        try
        {
            covered = ReadCmap(File.ReadAllBytes(args[0]));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"fontcoverage: {args[0]}: {ex.Message}");
            return 1;
        }

        var have = new List<int>();
        foreach (string range in args.Skip(1))
        {
            string[] parts = range.Split('-');
            int start = int.Parse(parts[0]);
            int end = parts.Length > 1 ? int.Parse(parts[1]) : start;
            for (int c = start; c <= end; c++)
            {
                if (covered.Contains(c)) have.Add(c);
            }
        }

        Console.WriteLine(string.Join(" ", Compress(have.Distinct().OrderBy(c => c))));
        return 0;
    }

    /// <summary>Consecutive code points as "start-end".</summary>
    private static IEnumerable<string> Compress(IEnumerable<int> codePoints)
    {
        int? start = null, previous = null;
        foreach (int c in codePoints)
        {
            if (previous.HasValue && c == previous.Value + 1)
            {
                previous = c;
                continue;
            }
            if (start.HasValue) yield return $"{start}-{previous}";
            start = previous = c;
        }
        if (start.HasValue) yield return $"{start}-{previous}";
    }

    private static HashSet<int> ReadCmap(byte[] font)
    {
        int tables = U16(font, 4);
        int cmap = -1;
        for (int i = 0; i < tables; i++)
        {
            int record = 12 + i * 16;
            if (font[record] == 'c' && font[record + 1] == 'm' && font[record + 2] == 'a' && font[record + 3] == 'p')
            {
                cmap = (int)U32(font, record + 8);
            }
        }
        if (cmap < 0) throw new InvalidDataException("no cmap table");

        int best = -1, bestFormat = 0;
        int subtables = U16(font, cmap + 2);
        for (int i = 0; i < subtables; i++)
        {
            int platform = U16(font, cmap + 4 + i * 8);
            int encoding = U16(font, cmap + 6 + i * 8);
            int offset = cmap + (int)U32(font, cmap + 8 + i * 8);
            bool unicode = platform == 0 || (platform == 3 && (encoding == 1 || encoding == 10));
            int format = U16(font, offset);
            if (unicode && (format == 12 || (format == 4 && bestFormat != 12)))
            {
                best = offset;
                bestFormat = format;
            }
        }
        if (best < 0) throw new InvalidDataException("no Unicode cmap subtable (format 4 or 12)");

        var set = new HashSet<int>();
        if (bestFormat == 12)
        {
            long groups = U32(font, best + 12);
            for (long g = 0; g < groups; g++)
            {
                int at = best + 16 + (int)g * 12;
                long first = U32(font, at), last = U32(font, at + 4), glyph = U32(font, at + 8);
                for (long c = first; c <= last; c++)
                {
                    if (glyph + (c - first) != 0) set.Add((int)c);
                }
            }
        }
        else
        {
            int segments = U16(font, best + 6) / 2;
            int ends = best + 14, starts = ends + segments * 2 + 2;
            int deltas = starts + segments * 2, rangeOffsets = deltas + segments * 2;
            for (int s = 0; s < segments; s++)
            {
                int end = U16(font, ends + s * 2), start = U16(font, starts + s * 2);
                int delta = (short)U16(font, deltas + s * 2);
                int rangeOffset = U16(font, rangeOffsets + s * 2);
                for (int c = start; c <= end && c != 0xFFFF; c++)
                {
                    int glyph;
                    if (rangeOffset == 0)
                    {
                        glyph = (c + delta) & 0xFFFF;
                    }
                    else
                    {
                        int at = rangeOffsets + s * 2 + rangeOffset + (c - start) * 2;
                        glyph = U16(font, at);
                        if (glyph != 0) glyph = (glyph + delta) & 0xFFFF;
                    }
                    if (glyph != 0) set.Add(c);
                }
            }
        }
        return set;
    }

    private static int U16(byte[] b, int at) => (b[at] << 8) | b[at + 1];

    private static long U32(byte[] b, int at) => ((long)b[at] << 24) | ((long)b[at + 1] << 16) | ((long)b[at + 2] << 8) | b[at + 3];
}
