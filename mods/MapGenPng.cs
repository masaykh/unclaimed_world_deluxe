using System;
using System.IO;
using System.IO.Compression;

namespace UWGame.Mods;

/// <summary>
/// PNG in and out for <see cref="MapGenMod"/>, with no graphics device.
///
/// The game reads a map's layers with Texture2D.FromStream, which needs a GraphicsDevice; the
/// generator runs in DataExport and in tests, where there is none, and must also READ the shipped
/// maps to learn from them. This is the smallest PNG that covers both: 8-bit greyscale, RGB and
/// RGBA (and palette, for reading), non-interlaced, all five row filters on reading. Writing is
/// always 8-bit RGB with no filtering, which every decoder accepts.
/// </summary>
internal static class MapGenPng
{
    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    /// <summary>An image as 8-bit RGBA, row by row from the top.</summary>
    internal sealed class Image
    {
        public int Width;
        public int Height;
        public byte[] Rgba;

        public byte R(int x, int y) => Rgba[(y * Width + x) * 4];
    }

    /// <summary>Writes an 8-bit RGB PNG; <paramref name="rgb"/> is Width x Height x 3 bytes.</summary>
    internal static void WriteRgb(string path, int width, int height, byte[] rgb) => Write(path, width, height, rgb, 3);

    /// <summary>
    /// Writes an 8-bit RGBA PNG. Only waterColors wants one: its alpha is the tint's greatest
    /// opacity. Anywhere else keep alpha 255 - MonoGame loads a pixel with alpha 0 as black.
    /// </summary>
    internal static void WriteRgba(string path, int width, int height, byte[] rgba) => Write(path, width, height, rgba, 4);

    private static void Write(string path, int width, int height, byte[] pixels, int channels)
    {
        int stride = width * channels;
        if (pixels.Length != stride * height)
        {
            throw new ArgumentException($"expected {stride * height} bytes, got {pixels.Length}");
        }
        byte[] raw = new byte[(stride + 1) * height];
        for (int y = 0; y < height; y++)
        {
            raw[y * (stride + 1)] = 0;   // filter: none
            Buffer.BlockCopy(pixels, y * stride, raw, y * (stride + 1) + 1, stride);
        }
        using var file = File.Create(path);
        file.Write(Signature, 0, Signature.Length);
        byte[] ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, (uint)width);
        WriteBigEndian(ihdr, 4, (uint)height);
        ihdr[8] = 8;                            // bit depth
        ihdr[9] = channels == 4 ? (byte)6 : (byte)2;   // colour type: RGBA or RGB
        WriteChunk(file, "IHDR", ihdr);
        using (var packed = new MemoryStream())
        {
            using (var z = new ZLibStream(packed, CompressionLevel.Optimal, leaveOpen: true))
            {
                z.Write(raw, 0, raw.Length);
            }
            WriteChunk(file, "IDAT", packed.ToArray());
        }
        WriteChunk(file, "IEND", Array.Empty<byte>());
    }

    /// <summary>A single-channel layer written as grey RGB, the way the shipped maps store them.</summary>
    internal static void WriteGrey(string path, int width, int height, byte[] values)
    {
        byte[] rgb = new byte[width * height * 3];
        for (int i = 0; i < values.Length; i++)
        {
            rgb[i * 3] = rgb[i * 3 + 1] = rgb[i * 3 + 2] = values[i];
        }
        WriteRgb(path, width, height, rgb);
    }

    internal static Image Read(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        for (int i = 0; i < Signature.Length; i++)
        {
            if (data[i] != Signature[i]) throw new InvalidDataException(path + ": not a PNG");
        }
        int width = 0, height = 0, depth = 0, type = 0, interlace = 0;
        byte[] palette = null, paletteAlpha = null;
        var idat = new MemoryStream();
        int pos = 8;
        while (pos + 8 <= data.Length)
        {
            int length = (int)ReadBigEndian(data, pos);
            string name = System.Text.Encoding.ASCII.GetString(data, pos + 4, 4);
            int body = pos + 8;
            switch (name)
            {
                case "IHDR":
                    width = (int)ReadBigEndian(data, body);
                    height = (int)ReadBigEndian(data, body + 4);
                    depth = data[body + 8];
                    type = data[body + 9];
                    interlace = data[body + 12];
                    break;
                case "PLTE":
                    palette = new byte[length];
                    Buffer.BlockCopy(data, body, palette, 0, length);
                    break;
                case "tRNS":
                    paletteAlpha = new byte[length];
                    Buffer.BlockCopy(data, body, paletteAlpha, 0, length);
                    break;
                case "IDAT":
                    idat.Write(data, body, length);
                    break;
            }
            if (name == "IEND") break;
            pos = body + length + 4;   // + CRC
        }
        if (depth != 8 || interlace != 0)
        {
            throw new NotSupportedException($"{path}: {depth}-bit, interlace {interlace}; only 8-bit non-interlaced is read");
        }
        int channels = type switch { 0 => 1, 2 => 3, 3 => 1, 4 => 2, 6 => 4, _ => throw new NotSupportedException($"{path}: colour type {type}") };
        int stride = width * channels;
        byte[] raw;
        idat.Position = 0;
        using (var z = new ZLibStream(idat, CompressionMode.Decompress))
        using (var outStream = new MemoryStream())
        {
            z.CopyTo(outStream);
            raw = outStream.ToArray();
        }
        byte[] pixels = new byte[stride * height];
        for (int y = 0; y < height; y++)
        {
            int filter = raw[y * (stride + 1)];
            int src = y * (stride + 1) + 1;
            int dst = y * stride;
            for (int x = 0; x < stride; x++)
            {
                int a = x >= channels ? pixels[dst + x - channels] : 0;
                int b = y > 0 ? pixels[dst - stride + x] : 0;
                int c = (x >= channels && y > 0) ? pixels[dst - stride + x - channels] : 0;
                int v = raw[src + x];
                v += filter switch
                {
                    0 => 0,
                    1 => a,
                    2 => b,
                    3 => (a + b) / 2,
                    4 => Paeth(a, b, c),
                    _ => throw new InvalidDataException($"{path}: row filter {filter}"),
                };
                pixels[dst + x] = (byte)v;
            }
        }
        var image = new Image { Width = width, Height = height, Rgba = new byte[width * height * 4] };
        for (int i = 0; i < width * height; i++)
        {
            byte r, g, b2, a2 = 255;
            switch (type)
            {
                case 0: r = g = b2 = pixels[i]; break;
                case 4: r = g = b2 = pixels[i * 2]; a2 = pixels[i * 2 + 1]; break;
                case 2: r = pixels[i * 3]; g = pixels[i * 3 + 1]; b2 = pixels[i * 3 + 2]; break;
                case 6: r = pixels[i * 4]; g = pixels[i * 4 + 1]; b2 = pixels[i * 4 + 2]; a2 = pixels[i * 4 + 3]; break;
                default:
                    int p = pixels[i];
                    r = palette[p * 3]; g = palette[p * 3 + 1]; b2 = palette[p * 3 + 2];
                    if (paletteAlpha != null && p < paletteAlpha.Length) a2 = paletteAlpha[p];
                    break;
            }
            image.Rgba[i * 4] = r;
            image.Rgba[i * 4 + 1] = g;
            image.Rgba[i * 4 + 2] = b2;
            image.Rgba[i * 4 + 3] = a2;
        }
        return image;
    }

    private static int Paeth(int a, int b, int c)
    {
        int p = a + b - c;
        int pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }

    private static void WriteChunk(Stream s, string name, byte[] body)
    {
        byte[] header = new byte[8];
        WriteBigEndian(header, 0, (uint)body.Length);
        System.Text.Encoding.ASCII.GetBytes(name, 0, 4, header, 4);
        s.Write(header, 0, 8);
        s.Write(body, 0, body.Length);
        uint crc = Crc(header, 4, 4, 0xFFFFFFFFu);
        crc = Crc(body, 0, body.Length, crc) ^ 0xFFFFFFFFu;
        byte[] tail = new byte[4];
        WriteBigEndian(tail, 0, crc);
        s.Write(tail, 0, 4);
    }

    private static uint[] crcTable;

    private static uint Crc(byte[] buffer, int offset, int count, uint crc)
    {
        if (crcTable == null)
        {
            var table = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
                table[n] = c;
            }
            crcTable = table;
        }
        for (int i = offset; i < offset + count; i++)
        {
            crc = crcTable[(crc ^ buffer[i]) & 0xFF] ^ (crc >> 8);
        }
        return crc;
    }

    private static void WriteBigEndian(byte[] b, int at, uint v)
    {
        b[at] = (byte)(v >> 24); b[at + 1] = (byte)(v >> 16); b[at + 2] = (byte)(v >> 8); b[at + 3] = (byte)v;
    }

    private static uint ReadBigEndian(byte[] b, int at) =>
        (uint)(b[at] << 24 | b[at + 1] << 16 | b[at + 2] << 8 | b[at + 3]);
}
