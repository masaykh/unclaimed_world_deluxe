using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
// XnbFile and Lz4DecoderStream are compiled in from ..\MgfxTranscode (see the .csproj), so they
// keep that tool's namespace even though they live in this assembly.
using UW.Tools.MgfxTranscode;

namespace UW.Tools.XnbExtract;

/// <summary>
/// Reads compiled .xnb assets back out.
///
/// WHY THIS EXISTS. The studio's published source is missing a handful of the assets its own
/// content project builds - two textures, a sprite-effects folder, and the 47 model meshes. The
/// shipped game carries the compiled result of every one of them, and an .xnb is not compiled code:
/// it is a documented container (header, optional LZ4, a table of type readers, then a serialized
/// object graph). What the runtime needs is all in there, so a texture can be handed back as a PNG
/// the content pipeline will accept again.
///
/// WHAT IT DOES NOT CLAIM. This recovers a BUILD INPUT, not an artist's source file. For a texture
/// built as SurfaceFormat.Color the pixels are exact; for anything the processor compressed (DXT),
/// the loss already happened at build time and cannot be undone.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Usage();
            return 2;
        }

        try
        {
            return args[0] switch
            {
                "inventory" => Inventory(args.Skip(1).ToArray()),
                "textures"  => Textures(args.Skip(1).ToArray()),
                "sheet"     => Sheet(args.Skip(1).ToArray()),
                "dump"      => Dump(args.Skip(1).ToArray()),
                _           => Usage(),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FATAL: " + ex.Message);
            return 1;
        }
    }

    private static int Usage()
    {
        Console.Error.WriteLine("usage: xnbextract inventory <content-dir>");
        Console.Error.WriteLine("       xnbextract textures <out-dir> <file.xnb> [...] [--keep-premultiplied]");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  inventory  lists every .xnb with its primary type reader and compression,");
        Console.Error.WriteLine("             then a summary by type. Reads headers only - fast, no decoding.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  textures   decodes Texture2D assets to PNG. Pixels stored premultiplied are");
        Console.Error.WriteLine("             un-premultiplied by default, because the pipeline premultiplies");
        Console.Error.WriteLine("             again on the way back in and doing it twice darkens every edge.");
        return 2;
    }

    // ------------------------------------------------------------------------------ inventory

    private static int Inventory(string[] args)
    {
        if (args.Length < 1) return Usage();
        string root = args[0];
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);

        var byType = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var failed = new List<string>();
        int total = 0, compressed = 0;

        foreach (string path in Directory.EnumerateFiles(root, "*.xnb", SearchOption.AllDirectories)
                                         .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            total++;
            string name = Path.GetRelativePath(root, path);
            try
            {
                var xnb = XnbFile.Read(path);
                string reader = Shorten(xnb.PrimaryReaderName);
                byType[reader] = byType.TryGetValue(reader, out int n) ? n + 1 : 1;
                if (xnb.WasCompressed) compressed++;
                Console.WriteLine($"{name,-56} {reader,-28} {(xnb.WasCompressed ? "lz4" : "-"),-4} {new FileInfo(path).Length,9:N0}");
            }
            catch (Exception ex)
            {
                failed.Add($"{name}: {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"==> {total} asset(s), {compressed} compressed, {failed.Count} unreadable");
        foreach (var kv in byType.OrderByDescending(k => k.Value))
            Console.WriteLine($"    {kv.Value,5}  {kv.Key}");
        foreach (string f in failed)
            Console.WriteLine($"    FAIL  {f}");

        return failed.Count == 0 ? 0 : 1;
    }

    private static string Shorten(string reader)
    {
        string name = reader.Split(',')[0];
        int dot = name.LastIndexOf('.');
        return dot >= 0 ? name[(dot + 1)..] : name;
    }

    // ------------------------------------------------------------------------------- textures

    private static int Textures(string[] args)
    {
        bool keepPremultiplied = args.Contains("--keep-premultiplied");
        string[] positional = args.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToArray();
        if (positional.Length < 2) return Usage();

        string outDir = positional[0];
        Directory.CreateDirectory(outDir);

        int done = 0, failed = 0;
        foreach (string path in positional.Skip(1))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            try
            {
                var xnb = XnbFile.Read(path);
                if (!xnb.PrimaryReaderName.StartsWith("Microsoft.Xna.Framework.Content.Texture2DReader",
                                                      StringComparison.Ordinal))
                    throw new InvalidDataException($"primary reader is {Shorten(xnb.PrimaryReaderName)}, not Texture2DReader");

                var (width, height, pixels) = ReadTexture2D(xnb);
                if (!keepPremultiplied) UnPremultiply(pixels);

                string outPath = Path.Combine(outDir, name + ".png");
                WritePng(outPath, width, height, pixels);
                Console.WriteLine($"    {name,-36} {width}x{height} -> {Path.GetFileName(outPath)}");
                done++;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"    FAIL  {name}: {ex.Message}");
                failed++;
            }
        }

        Console.WriteLine($"==> {done} texture(s) written, {failed} failed");
        return failed == 0 ? 0 : 1;
    }

    /// <summary>
    /// Prints the container's reader table, its shared-resource count and the first bytes of the
    /// primary object. For working out an asset's layout when a guess at it does not read.
    /// </summary>
    private static int Dump(string[] args)
    {
        if (args.Length < 1) return Usage();
        var xnb = XnbFile.Read(args[0]);

        Console.WriteLine($"==> {Path.GetFileName(args[0])}");
        Console.WriteLine($"    compressed: {xnb.WasCompressed}, payload {xnb.Payload.Length:N0} bytes");
        Console.WriteLine($"    shared resources: {xnb.SharedResourceCount}");
        Console.WriteLine($"    primary type id: {xnb.PrimaryTypeId} ({Shorten(xnb.PrimaryReaderName)})");
        for (int i = 0; i < xnb.TypeReaders.Count; i++)
            Console.WriteLine($"      [{i + 1}] {xnb.TypeReaders[i].Name.Split(',')[0]}");

        int offset = xnb.PrimaryDataOffset;
        int count = Math.Min(64, xnb.Payload.Length - offset);
        Console.WriteLine($"    first {count} byte(s) of the primary object:");
        for (int i = 0; i < count; i += 16)
        {
            var hex = string.Join(" ", Enumerable.Range(i, Math.Min(16, count - i))
                                                 .Select(j => xnb.Payload[offset + j].ToString("x2")));
            Console.WriteLine($"      {i:x4}  {hex}");
        }
        return 0;
    }

    // ---------------------------------------------------------------------------- sprite sheets

    /// <summary>
    /// Reads a SpriteSheetRuntime.SpriteSheet asset: its atlas, the source rectangles, and the
    /// name-to-index map. The layout comes from the studio's own SpriteSheet.ReadContent -
    /// Texture2D, then List&lt;Rectangle&gt;, then Dictionary&lt;string,int&gt;.
    ///
    /// With --export it writes one PNG per sprite, cropped from the atlas, named after the sprite.
    /// That is what turns a sheet back into the folder of loose images the SpriteSheetProcessor
    /// expects as INPUT, which is how a sheet whose source folder was never published can be
    /// rebuilt from the shipped game.
    /// </summary>
    private static int Sheet(string[] args)
    {
        string[] positional = args.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToArray();
        if (positional.Length < 1) return Usage();

        string path = positional[0];
        string? exportDir = args.Contains("--export") && positional.Length > 1 ? positional[1] : null;
        bool keepPremultiplied = args.Contains("--keep-premultiplied");

        var xnb = XnbFile.Read(path);
        using var ms = new MemoryStream(xnb.Payload, xnb.PrimaryDataOffset,
                                        xnb.Payload.Length - xnb.PrimaryDataOffset, writable: false);
        using var reader = new BinaryReader(ms);

        // Each of the three ReadObject calls is preceded by its reader's 1-based type id.
        Read7Bit(reader);
        var (width, height, pixels) = ReadTexture2DBody(reader);

        Read7Bit(reader);
        int rectCount = (int)reader.ReadUInt32();
        var rects = new (int X, int Y, int W, int H)[rectCount];
        for (int i = 0; i < rectCount; i++)
            rects[i] = (reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

        Read7Bit(reader);
        int nameCount = (int)reader.ReadUInt32();
        var names = new (string Name, int Index)[nameCount];
        for (int i = 0; i < nameCount; i++)
        {
            // The key is a string - a REFERENCE type - so the dictionary reader writes its type id
            // before each one. Rectangle, being a value type, gets no such prefix, which is why the
            // list above reads straight through.
            Read7Bit(reader);
            names[i] = (reader.ReadString(), reader.ReadInt32());
        }

        Console.WriteLine($"==> {Path.GetFileName(path)}: atlas {width}x{height}, {rectCount} rectangle(s), {nameCount} name(s)");

        foreach (var (name, index) in names.OrderBy(n => n.Name, StringComparer.Ordinal))
        {
            if (index < 0 || index >= rectCount)
                throw new InvalidDataException($"sprite '{name}' points at rectangle {index} of {rectCount}");
        }

        if (exportDir == null)
        {
            foreach (var (name, index) in names.OrderBy(n => n.Name, StringComparer.Ordinal))
            {
                var r = rects[index];
                Console.WriteLine($"    {name,-44} {r.W,4}x{r.H,-4} @ {r.X},{r.Y}");
            }
            return 0;
        }

        if (!keepPremultiplied) UnPremultiply(pixels);
        Directory.CreateDirectory(exportDir);

        int written = 0;
        foreach (var (name, index) in names.OrderBy(n => n.Name, StringComparer.Ordinal))
        {
            var r = rects[index];
            var crop = new byte[r.W * r.H * 4];
            for (int y = 0; y < r.H; y++)
                Array.Copy(pixels, ((r.Y + y) * width + r.X) * 4, crop, y * r.W * 4, r.W * 4);

            WritePng(Path.Combine(exportDir, name + ".png"), r.W, r.H, crop);
            written++;
        }

        Console.WriteLine($"==> {written} sprite(s) written to {exportDir}");
        return 0;
    }

    private static int Read7Bit(BinaryReader reader)
    {
        int result = 0, shift = 0;
        while (true)
        {
            byte b = reader.ReadByte();
            result |= (b & 0x7F) << shift;
            if ((b & 0x80) == 0) return result;
            shift += 7;
        }
    }

    /// <summary>
    /// Texture2DReader's payload: surface format, width, height, mip count, then each level as a
    /// length-prefixed block. Only level 0 is wanted - the rest are generated again on rebuild.
    /// </summary>
    private static (int Width, int Height, byte[] Pixels) ReadTexture2D(XnbFile xnb)
    {
        using var ms = new MemoryStream(xnb.Payload, xnb.PrimaryDataOffset,
                                        xnb.Payload.Length - xnb.PrimaryDataOffset, writable: false);
        using var reader = new BinaryReader(ms);
        return ReadTexture2DBody(reader);
    }

    /// <summary>The texture body itself, with the reader already positioned on the format field.</summary>
    private static (int Width, int Height, byte[] Pixels) ReadTexture2DBody(BinaryReader reader)
    {
        int surfaceFormat = reader.ReadInt32();
        int width = (int)reader.ReadUInt32();
        int height = (int)reader.ReadUInt32();
        int levels = (int)reader.ReadUInt32();
        if (levels < 1) throw new InvalidDataException("texture has no mip levels");

        // Every level has to be consumed even though only the first is wanted: when a texture is
        // nested inside a bigger asset - a sprite sheet, say - whatever follows it is read from
        // this same stream, and stopping after level 0 leaves the reader pointing at mip data.
        byte[] data = Array.Empty<byte>();
        for (int level = 0; level < levels; level++)
        {
            int size = (int)reader.ReadUInt32();
            byte[] bytes = reader.ReadBytes(size);
            if (bytes.Length != size) throw new InvalidDataException("texture data truncated");
            if (level == 0) data = bytes;
        }

        // SurfaceFormat.Color == 0: 8 bits per channel, R G B A in memory order.
        if (surfaceFormat != 0)
            throw new NotSupportedException(
                $"surface format {surfaceFormat} is not SurfaceFormat.Color. The pipeline compressed " +
                "this texture at build time, so no lossless source can be recovered from it.");

        if (data.Length != width * height * 4)
            throw new InvalidDataException($"expected {width * height * 4} bytes, got {data.Length}");

        return (width, height, data);
    }

    /// <summary>
    /// Undoes PremultiplyAlpha. The pipeline applies it on the way in, so handing back premultiplied
    /// pixels would apply it twice and darken everything with soft edges.
    /// </summary>
    private static void UnPremultiply(byte[] rgba)
    {
        for (int i = 0; i < rgba.Length; i += 4)
        {
            byte a = rgba[i + 3];
            if (a == 0 || a == 255) continue;
            for (int c = 0; c < 3; c++)
            {
                int v = rgba[i + c] * 255 / a;
                rgba[i + c] = (byte)(v > 255 ? 255 : v);
            }
        }
    }

    // ------------------------------------------------------------------------------ png writer

    private static void WritePng(string path, int width, int height, byte[] rgba)
    {
        using var file = File.Create(path);
        file.Write(new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A });

        var ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, width);
        WriteBigEndian(ihdr, 4, height);
        ihdr[8] = 8;    // bit depth
        ihdr[9] = 6;    // colour type: RGBA
        WriteChunk(file, "IHDR", ihdr);

        // Each scanline is prefixed with its filter type; 0 (None) keeps this simple and still
        // compresses well for game art.
        var raw = new byte[height * (width * 4 + 1)];
        for (int y = 0; y < height; y++)
        {
            int src = y * width * 4;
            int dst = y * (width * 4 + 1);
            raw[dst] = 0;
            Array.Copy(rgba, src, raw, dst + 1, width * 4);
        }

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(raw, 0, raw.Length);
        WriteChunk(file, "IDAT", compressed.ToArray());

        WriteChunk(file, "IEND", Array.Empty<byte>());
    }

    private static void WriteChunk(Stream output, string type, byte[] data)
    {
        var length = new byte[4];
        WriteBigEndian(length, 0, data.Length);
        output.Write(length);

        var typeAndData = new byte[4 + data.Length];
        for (int i = 0; i < 4; i++) typeAndData[i] = (byte)type[i];
        Array.Copy(data, 0, typeAndData, 4, data.Length);
        output.Write(typeAndData);

        var crc = new byte[4];
        WriteBigEndian(crc, 0, unchecked((int)Crc32(typeAndData)));
        output.Write(crc);
    }

    private static void WriteBigEndian(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    private static readonly uint[] CrcTable = BuildCrcTable();

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            table[n] = c;
        }
        return table;
    }

    private static uint Crc32(byte[] data)
    {
        uint c = 0xFFFFFFFFu;
        foreach (byte b in data)
            c = CrcTable[(c ^ b) & 0xFF] ^ (c >> 8);
        return c ^ 0xFFFFFFFFu;
    }
}
