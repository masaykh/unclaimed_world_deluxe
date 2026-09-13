using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UW.Tools.MgfxTranscode;

namespace UW.Tools.XnbVertexWiden;

/// <summary>
/// Rewrites compiled model XNBs so that no vertex element uses
/// <c>VertexElementFormat.Color</c> for anything other than a colour.
///
/// WHY, AND WHY AT BUILD TIME. Every skinned model in this game declares its blend weights as
/// <c>BlendWeight0:Color</c> - four packed bytes, which XNA defines as mapping to 0..1. Both
/// MonoGame and FNA decide whether to normalize a vertex attribute from the element's USAGE
/// rather than its FORMAT, so a Color-format element whose usage is BlendWeight is bound
/// un-normalized and the weights arrive as 0..255. skinFX multiplies bone-transformed positions
/// by them, and every skinned mesh comes out scaled by about 255 - shards fanning across the
/// screen. (MonoGame's own source carries the TODO wondering which way XNA did it. Both guessed
/// the same way, and both guessed wrong.)
///
/// PORT DEVIATION 14 fixed that at run time, in UWGame/Port/SkinnedVertexCompat.cs, by rebuilding
/// each model's vertex buffers as they load. That works on MonoGame and CANNOT work on FNA:
/// XNA's <c>ModelMeshPart.VertexBuffer</c> is read-only and MonoGame added the setter. Rather
/// than write a second workaround for a third backend, the fix belongs in the data - where it
/// costs nothing at run time, applies to every backend at once, and lets the runtime shim go.
///
/// HOW IT FINDS THEM. A model XNB's vertex buffers are self-describing and carry no absolute
/// offsets, so the whole object graph does not have to be parsed. From FNA's readers, which are
/// XNA's format:
///
///     VertexDeclaration : int32 stride, int32 elementCount,
///                         elementCount x { int32 offset, format, usage, usageIndex }
///     VertexBuffer      : VertexDeclaration, uint32 vertexCount, vertexCount*stride bytes
///
/// The scan accepts a candidate only when every field is in range, the elements fit inside the
/// stride and do not overlap, one of them is POSITION0 (every vertex buffer has one), and the
/// declared vertex data fits exactly in what remains of the file. Those together make a false
/// positive essentially impossible - and a false positive would be caught anyway, because the
/// result is verified by re-scanning the rewritten file before it is written.
/// </summary>
internal static class Program
{
    private const int UsagePosition = 0;
    private const int UsageColor = 1;
    private const int FormatColor = 4;
    private const int FormatVector4 = 3;

    /// <summary>Bytes each VertexElementFormat occupies. Index is the enum value.</summary>
    private static readonly int[] FormatSize =
    {
        4,  // Single
        8,  // Vector2
        12, // Vector3
        16, // Vector4
        4,  // Color
        4,  // Byte4
        4,  // Short2
        8,  // Short4
        4,  // NormalizedShort2
        8,  // NormalizedShort4
        4,  // HalfVector2
        8,  // HalfVector4
    };

    private static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("usage: xnbvertexwiden <dir> [--dry-run]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  Rewrites every .xnb under <dir> in place, widening any");
            Console.Error.WriteLine("  Color-format vertex element whose usage is not Color to");
            Console.Error.WriteLine("  Vector4. Files needing no change are not touched.");
            return 2;
        }

        string dir = args[0];
        bool dryRun = args.Contains("--dry-run");
        if (!Directory.Exists(dir))
        {
            Console.Error.WriteLine($"no such directory: {dir}");
            return 2;
        }

        int changed = 0, untouched = 0, failed = 0, buffers = 0;
        long before = 0, after = 0;

        foreach (string path in Directory.EnumerateFiles(dir, "*.xnb", SearchOption.AllDirectories)
                                         .OrderBy(p => p, StringComparer.Ordinal))
        {
            string relative = Path.GetRelativePath(dir, path);
            try
            {
                XnbFile xnb = XnbFile.Read(path);
                byte[] payload = xnb.Payload;

                List<Candidate> sites = FindVertexBuffers(payload)
                    .Where(NeedsWidening)
                    .ToList();

                if (sites.Count == 0)
                {
                    untouched++;
                    continue;
                }

                byte[] rewritten = Rewrite(payload, sites);

                // Verify before writing: the rewritten payload must parse, and nothing in it may
                // still declare a miscast Color element. A silent half-conversion here would be
                // invisible until a character rendered wrongly.
                List<Candidate> leftover = FindVertexBuffers(rewritten).Where(NeedsWidening).ToList();
                if (leftover.Count > 0)
                {
                    Console.Error.WriteLine(
                        $"  !! {relative}: {leftover.Count} buffer(s) still miscast after rewrite - not written");
                    failed++;
                    continue;
                }

                buffers += sites.Count;
                before += new FileInfo(path).Length;
                if (!dryRun)
                {
                    xnb.WriteWithPayload(path, rewritten);
                }
                after += dryRun ? 0 : new FileInfo(path).Length;
                changed++;

                string what = string.Join(", ", sites.SelectMany(s => s.Widened)
                    .Select(e => Usage(e.Usage) + e.UsageIndex).Distinct());
                Console.WriteLine($"  {(dryRun ? "would widen" : "widened")} {relative}: " +
                                  $"{sites.Count} buffer(s), {what}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  !! {relative}: {ex.GetType().Name}: {ex.Message}");
                failed++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{changed} model(s) rewritten ({buffers} vertex buffer(s)), " +
                          $"{untouched} unchanged, {failed} failed.");
        if (!dryRun && changed > 0)
        {
            Console.WriteLine($"  {before / 1024} KB -> {after / 1024} KB");
        }
        return failed == 0 ? 0 : 1;
    }

    private readonly record struct Element(int Offset, int Format, int Usage, int UsageIndex);

    /// <summary>One vertex buffer located in a payload, with its byte extent.</summary>
    private sealed class Candidate
    {
        public int DeclarationStart;     // first byte of the stride field
        public int Stride;
        public List<Element> Elements = new();
        public int VertexCountOffset;    // the uint32 after the element list
        public int VertexCount;
        public int DataStart;
        public int End;                  // one past the last data byte

        public List<Element> Widened =>
            Elements.Where(e => e.Format == FormatColor && e.Usage != UsageColor).ToList();
    }

    private static bool NeedsWidening(Candidate c) => c.Widened.Count > 0;

    /// <summary>
    /// Every vertex buffer in a payload, found by signature rather than by parsing the model
    /// graph. See the class remarks for why that is safe here.
    /// </summary>
    private static List<Candidate> FindVertexBuffers(byte[] payload)
    {
        var found = new List<Candidate>();
        int scan = 0;

        while (scan + 8 < payload.Length)
        {
            Candidate? c = TryParse(payload, scan);
            if (c == null)
            {
                scan++;
                continue;
            }

            found.Add(c);
            scan = c.End;   // never look for a declaration inside vertex data
        }

        return found;
    }

    private static Candidate? TryParse(byte[] payload, int p)
    {
        if (p + 8 > payload.Length) return null;

        int stride = BitConverter.ToInt32(payload, p);
        if (stride < 4 || stride > 1024 || stride % 4 != 0) return null;

        int count = BitConverter.ToInt32(payload, p + 4);
        if (count < 1 || count > 16) return null;
        if (p + 8 + count * 16 + 4 > payload.Length) return null;

        var elements = new List<Element>(count);
        bool hasPosition0 = false;
        var covered = new bool[stride];

        for (int i = 0; i < count; i++)
        {
            int e = p + 8 + i * 16;
            int offset = BitConverter.ToInt32(payload, e);
            int format = BitConverter.ToInt32(payload, e + 4);
            int usage = BitConverter.ToInt32(payload, e + 8);
            int usageIndex = BitConverter.ToInt32(payload, e + 12);

            if (format < 0 || format >= FormatSize.Length) return null;
            if (usage < 0 || usage > 12) return null;
            if (usageIndex < 0 || usageIndex > 15) return null;
            if (offset < 0 || offset + FormatSize[format] > stride) return null;

            // Elements must not overlap. This is what rejects runs of plausible-looking integers.
            for (int b = offset; b < offset + FormatSize[format]; b++)
            {
                if (covered[b]) return null;
                covered[b] = true;
            }

            if (usage == UsagePosition && usageIndex == 0) hasPosition0 = true;
            elements.Add(new Element(offset, format, usage, usageIndex));
        }

        if (!hasPosition0) return null;

        int vertexCountOffset = p + 8 + count * 16;
        uint vertexCount = BitConverter.ToUInt32(payload, vertexCountOffset);
        if (vertexCount == 0 || vertexCount > 10_000_000) return null;

        long dataStart = vertexCountOffset + 4;
        long end = dataStart + (long)vertexCount * stride;
        if (end > payload.Length) return null;

        return new Candidate
        {
            DeclarationStart = p,
            Stride = stride,
            Elements = elements,
            VertexCountOffset = vertexCountOffset,
            VertexCount = (int)vertexCount,
            DataStart = (int)dataStart,
            End = (int)end,
        };
    }

    /// <summary>
    /// Rebuilds the payload with each affected buffer widened.
    ///
    /// The original 4-byte Color slot is LEFT IN PLACE and the four floats are appended past the
    /// old stride, so every other element keeps its offset and only the tail moves. That wastes
    /// four bytes per vertex and is deliberate: it is what the proven runtime implementation does
    /// (UWGame/Port/SkinnedVertexCompat.Widen), and matching it exactly means this produces the
    /// same vertices the GL build has been rendering correctly.
    /// </summary>
    private static byte[] Rewrite(byte[] payload, List<Candidate> sites)
    {
        using var output = new MemoryStream(payload.Length + sites.Sum(s => s.VertexCount * 16));
        int copied = 0;

        foreach (Candidate c in sites.OrderBy(s => s.DeclarationStart))
        {
            output.Write(payload, copied, c.DeclarationStart - copied);

            List<Element> widened = c.Widened;
            int newStride = c.Stride + 16 * widened.Count;

            // --- declaration
            output.Write(BitConverter.GetBytes(newStride), 0, 4);
            output.Write(BitConverter.GetBytes(c.Elements.Count), 0, 4);

            var moved = new List<(int From, int To)>();
            int nextOffset = c.Stride;
            foreach (Element e in c.Elements)
            {
                bool widen = e.Format == FormatColor && e.Usage != UsageColor;
                int offset = widen ? nextOffset : e.Offset;
                int format = widen ? FormatVector4 : e.Format;
                if (widen)
                {
                    moved.Add((e.Offset, nextOffset));
                    nextOffset += 16;
                }
                output.Write(BitConverter.GetBytes(offset), 0, 4);
                output.Write(BitConverter.GetBytes(format), 0, 4);
                output.Write(BitConverter.GetBytes(e.Usage), 0, 4);
                output.Write(BitConverter.GetBytes(e.UsageIndex), 0, 4);
            }

            // --- vertex data
            output.Write(BitConverter.GetBytes(c.VertexCount), 0, 4);
            var vertex = new byte[newStride];
            for (int v = 0; v < c.VertexCount; v++)
            {
                Array.Clear(vertex, 0, newStride);
                Buffer.BlockCopy(payload, c.DataStart + v * c.Stride, vertex, 0, c.Stride);

                foreach ((int from, int to) in moved)
                {
                    // Color is packed R,G,B,A in ascending bytes and the shader reads the
                    // attribute as (x,y,z,w) in that same order.
                    for (int component = 0; component < 4; component++)
                    {
                        float value = payload[c.DataStart + v * c.Stride + from + component] / 255f;
                        BitConverter.TryWriteBytes(
                            new Span<byte>(vertex, to + component * 4, 4), value);
                    }
                }

                output.Write(vertex, 0, newStride);
            }

            copied = c.End;
        }

        output.Write(payload, copied, payload.Length - copied);
        return output.ToArray();
    }

    private static string Usage(int usage) => usage switch
    {
        0 => "Position",
        1 => "Color",
        2 => "TextureCoordinate",
        3 => "Normal",
        4 => "Binormal",
        5 => "Tangent",
        6 => "BlendIndices",
        7 => "BlendWeight",
        8 => "Depth",
        9 => "Fog",
        10 => "PointSize",
        11 => "Sample",
        12 => "TessellateFactor",
        _ => "Usage" + usage,
    };
}
