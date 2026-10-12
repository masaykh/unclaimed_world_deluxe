using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;

namespace UW.Tools.SpriteExport;

/// <summary>
/// Cuts the game's compiled sprite atlases back into one PNG per sprite, named as the game names
/// it - the name RenderableType and the data tables look sprites up by.
///
/// Usage:
///   spriteexport &lt;ContentDir&gt; &lt;OutDir&gt; [--sources &lt;assets/Content&gt;] [--normals]
///
/// Writes, per atlas, OutDir/&lt;Atlas&gt;/&lt;sprite&gt;.png and OutDir/&lt;Atlas&gt;/index.html (every
/// sprite with its name, size and source file), plus OutDir/index.html and OutDir/sprites.csv.
///
/// WHY FROM THE COMPILED ATLAS, when assets/Content holds the studio's source PNGs under the same
/// names: the atlas is what the game draws. The pipeline premultiplies alpha, greys and blurs the
/// ghosted buildings, and builds an atlas from whatever was in a folder on the day - so a source
/// folder is a guess at a sheet and the XNB is the sheet. With --sources the two are compared, and
/// a sprite with no source file, or one whose size differs, is listed.
///
/// Exit code is the number of atlases that failed to load.
/// </summary>
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        string[] positional = args.Where((a, i) => !a.StartsWith("--") && (i == 0 || args[i - 1] != "--sources")).ToArray();
        if (positional.Length != 2)
        {
            Console.Error.WriteLine("usage: spriteexport <ContentDir> <OutDir> [--sources <assets/Content>] [--normals]");
            return -1;
        }

        string contentDir = Path.GetFullPath(positional[0]);
        string outDir = Path.GetFullPath(positional[1]);
        int s = Array.IndexOf(args, "--sources");
        string? sourcesDir = s >= 0 && s + 1 < args.Length ? Path.GetFullPath(args[s + 1]) : null;
        bool normals = args.Contains("--normals");

        if (!Directory.Exists(contentDir))
        {
            Console.Error.WriteLine($"no such directory: {contentDir}");
            return -1;
        }
        if (sourcesDir != null && !Directory.Exists(sourcesDir))
        {
            Console.Error.WriteLine($"no such directory: {sourcesDir}");
            return -1;
        }

        Dictionary<string, List<string>> sources = sourcesDir == null
            ? new Dictionary<string, List<string>>()
            : IndexSources(sourcesDir);

        using var host = new DeviceHost();
        var content = new ContentManager(host, contentDir);

        Directory.CreateDirectory(outDir);
        var csv = new StringBuilder("atlas,sprite,x,y,width,height,source\r\n");
        var atlasRows = new List<(string Atlas, int Count, int NoSource, int SizeDiffers)>();
        int failures = 0;

        foreach (string asset in FindAtlases(contentDir))
        {
            SpriteSheet sheet;
            try
            {
                sheet = content.Load<SpriteSheet>(asset);
            }
            catch (Exception ex)
            {
                Exception root = ex;
                while (root.InnerException != null) root = root.InnerException;
                Console.WriteLine($"  FAIL  {asset}: {root.GetType().Name}: {root.Message}");
                failures++;
                continue;
            }

            string atlasDir = Path.Combine(outDir, asset.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(atlasDir);

            Color[] pixels = ReadPixels(sheet.Texture, asset);
            Color[]? normalPixels = normals && sheet is ExtendedSpriteSheet ext && ext.NormalTexture != null
                ? ReadPixels(ext.NormalTexture, asset + " normals")
                : null;

            var rows = new List<SpriteRow>();
            foreach (KeyValuePair<string, int> named in sheet.spriteNames.OrderBy(n => n.Key, StringComparer.OrdinalIgnoreCase))
            {
                if (named.Value < 0 || named.Value >= sheet.spriteRectangles.Count)
                {
                    continue;
                }
                Rectangle r = sheet.spriteRectangles[named.Value];
                string file = SafeFileName(named.Key) + ".png";
                SavePng(host.GraphicsDevice, pixels, sheet.Texture.Width, r, Path.Combine(atlasDir, file), unpremultiply: true);
                // A sprite with no _d depth file gets a normal map of zeros, alpha included - all the
                // trees, for instance. Writing those would fill the folder with blank images.
                if (normalPixels != null && AnyVisible(normalPixels, sheet.Texture.Width, r))
                {
                    string normalDir = Path.Combine(atlasDir, "_normals");
                    Directory.CreateDirectory(normalDir);
                    SavePng(host.GraphicsDevice, normalPixels, sheet.Texture.Width, r, Path.Combine(normalDir, file), unpremultiply: false);
                }

                string? source = null;
                bool sizeDiffers = false;
                if (sourcesDir != null && sources.TryGetValue(named.Key.ToLowerInvariant(), out List<string>? candidates))
                {
                    source = candidates[0];
                    sizeDiffers = !candidates.Any(c => SourceSize(Path.Combine(sourcesDir, c)) == (r.Width, r.Height));
                }
                rows.Add(new SpriteRow(named.Key, file, r, source, sizeDiffers));
                csv.Append(CultureInfo.InvariantCulture,
                    $"{Csv(asset)},{Csv(named.Key)},{r.X},{r.Y},{r.Width},{r.Height},{Csv(source ?? "")}\r\n");
            }

            int noSource = sourcesDir == null ? 0 : rows.Count(row => row.Source == null);
            int differs = rows.Count(row => row.SizeDiffers);
            File.WriteAllText(Path.Combine(atlasDir, "index.html"), AtlasPage(asset, rows, sourcesDir != null), Encoding.UTF8);
            atlasRows.Add((asset, rows.Count, noSource, differs));

            Console.WriteLine($"  ok    {asset,-28} {rows.Count,5} sprites  {sheet.Texture.Width}x{sheet.Texture.Height}" +
                (sourcesDir == null ? "" : $"  no source: {noSource}  size differs: {differs}"));
            content.Unload();
        }

        File.WriteAllText(Path.Combine(outDir, "sprites.csv"), csv.ToString(), Encoding.UTF8);
        File.WriteAllText(Path.Combine(outDir, "index.html"), IndexPage(atlasRows, sourcesDir != null), Encoding.UTF8);

        Console.WriteLine();
        Console.WriteLine($"{atlasRows.Sum(a => a.Count)} sprites from {atlasRows.Count} atlases, {failures} failed. Open {Path.Combine(outDir, "index.html")}");
        return failures;
    }

    private sealed record SpriteRow(string Name, string File, Rectangle Rect, string? Source, bool SizeDiffers);

    /// <summary>
    /// Every XNB whose reader is a SpriteSheetRuntime type. The reader is named in plain text near
    /// the top of the file, as ContentProbe identifies effects; listing the atlases by hand would
    /// miss the next one somebody adds.
    /// </summary>
    private static IEnumerable<string> FindAtlases(string contentDir)
    {
        foreach (string path in Directory.EnumerateFiles(contentDir, "*.xnb", SearchOption.AllDirectories)
                                         .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            byte[] head = new byte[Math.Min(1024, (int)new FileInfo(path).Length)];
            using (FileStream fs = File.OpenRead(path))
            {
                _ = fs.Read(head, 0, head.Length);
            }
            if (Encoding.ASCII.GetString(head).Contains("SpriteSheetRuntime"))
            {
                yield return Path.ChangeExtension(Path.GetRelativePath(contentDir, path), null)!.Replace('\\', '/');
            }
        }
    }

    /// <summary>
    /// The diffuse atlases are Color. The normal atlas is Vector4: unit normals, -1..1 per axis,
    /// with the sprite's alpha in W (ExtendedSpriteSheetProcessor.ConvertDepthToNormals), so it is
    /// written the way normal maps usually are, (n + 1) / 2 per channel.
    /// </summary>
    private static Color[] ReadPixels(Texture2D texture, string what)
    {
        var pixels = new Color[texture.Width * texture.Height];
        if (texture.Format == SurfaceFormat.Color)
        {
            texture.GetData(pixels);
        }
        else if (texture.Format == SurfaceFormat.Vector4)
        {
            var normals = new Vector4[pixels.Length];
            texture.GetData(normals);
            for (int i = 0; i < normals.Length; i++)
            {
                Vector4 n = normals[i];
                pixels[i] = new Color((n.X + 1) / 2, (n.Y + 1) / 2, (n.Z + 1) / 2, n.W);
            }
        }
        else
        {
            throw new NotSupportedException($"{what}: texture format {texture.Format}; only Color and Vector4 atlases are read");
        }
        return pixels;
    }

    private static bool AnyVisible(Color[] atlas, int atlasWidth, Rectangle r)
    {
        for (int y = r.Y; y < r.Bottom; y++)
        {
            for (int x = r.X; x < r.Right; x++)
            {
                if (atlas[y * atlasWidth + x].A != 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// The pipeline premultiplied every sprite (SpriteSheetProcessor.LoadSpriteTexture,
    /// "XNA 4 - with premultiply alpha!"). Saved as they are, half-transparent edges come out dark,
    /// so the colour is divided back out. Normal maps were never premultiplied.
    /// </summary>
    private static void SavePng(GraphicsDevice device, Color[] atlas, int atlasWidth, Rectangle r, string path, bool unpremultiply)
    {
        var cut = new Color[r.Width * r.Height];
        for (int y = 0; y < r.Height; y++)
        {
            for (int x = 0; x < r.Width; x++)
            {
                Color c = atlas[(r.Y + y) * atlasWidth + r.X + x];
                if (unpremultiply && c.A > 0 && c.A < 255)
                {
                    c = new Color(
                        Math.Min(255, c.R * 255 / c.A),
                        Math.Min(255, c.G * 255 / c.A),
                        Math.Min(255, c.B * 255 / c.A),
                        (int)c.A);
                }
                cut[y * r.Width + x] = c;
            }
        }
        using var texture = new Texture2D(device, r.Width, r.Height, false, SurfaceFormat.Color);
        texture.SetData(cut);
        using FileStream fs = File.Create(path);
        texture.SaveAsPng(fs, r.Width, r.Height);
    }

    /// <summary>
    /// Source images by lower-case name. The pipeline names a sprite after its file and skips the
    /// _d (depth), _u (utility) and _g suffixed companions, so those are left out here too.
    /// </summary>
    private static Dictionary<string, List<string>> IndexSources(string sourcesDir)
    {
        var index = new Dictionary<string, List<string>>();
        foreach (string path in Directory.EnumerateFiles(sourcesDir, "*.*", SearchOption.AllDirectories))
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext != ".png" && ext != ".tga")
            {
                continue;
            }
            string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (!index.TryGetValue(name, out List<string>? list))
            {
                index[name] = list = new List<string>();
            }
            list.Add(Path.GetRelativePath(sourcesDir, path).Replace('\\', '/'));
        }
        return index;
    }

    /// <summary>
    /// Width and height from a PNG or TGA header. Both, because 129 of the sprites come from TGA
    /// files, and the first version read PNG only and reported every one of them as a mismatch.
    /// </summary>
    private static (int, int)? SourceSize(string path)
    {
        byte[] head = new byte[24];
        using (FileStream fs = File.OpenRead(path))
        {
            if (fs.Read(head, 0, 24) < 24)
            {
                return null;
            }
        }
        if (path.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
        {
            return (head[12] | (head[13] << 8), head[14] | (head[15] << 8));
        }
        int w = (head[16] << 24) | (head[17] << 16) | (head[18] << 8) | head[19];
        int h = (head[20] << 24) | (head[21] << 16) | (head[22] << 8) | head[23];
        return (w, h);
    }

    private static string SafeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name;
    }

    private static string Csv(string value) =>
        value.IndexOfAny(new[] { ',', '"' }) < 0 ? value : "\"" + value.Replace("\"", "\"\"") + "\"";

    private static string H(string value) => WebUtility.HtmlEncode(value);

    private const string Style = @"<style>
body{font:13px system-ui,sans-serif;margin:16px;background:#2b2b2b;color:#ddd}
a{color:#9cf}input{font:inherit;padding:4px 8px;width:280px}
.grid{display:flex;flex-wrap:wrap;gap:8px;margin-top:12px}
.s{background:#3a3a3a;padding:6px;width:150px;text-align:center;word-break:break-all}
.s img{max-width:140px;max-height:140px;image-rendering:pixelated;background:repeating-conic-gradient(#555 0 25%,#444 0 50%) 0 0/12px 12px}
.n{font-weight:600;margin-top:4px}.d{color:#999;font-size:11px}.w{color:#f96}
td,th{padding:3px 10px;text-align:left}
</style>";

    private static string AtlasPage(string atlas, List<SpriteRow> rows, bool compared)
    {
        var html = new StringBuilder();
        html.Append($"<!doctype html><meta charset=utf-8><title>{H(atlas)} sprites</title>{Style}");
        html.Append($"<p><a href=\"{H(string.Concat(Enumerable.Repeat("../", atlas.Count(c => c == '/') + 1)))}index.html\">all atlases</a></p>");
        html.Append($"<h1>{H(atlas)} &middot; {rows.Count} sprites</h1>");
        html.Append("<input id=f placeholder=\"filter by name\" oninput=\"for(const e of document.querySelectorAll('.s'))e.hidden=!e.dataset.n.includes(this.value.toLowerCase())\">");
        html.Append("<div class=grid>");
        foreach (SpriteRow row in rows)
        {
            html.Append($"<div class=s data-n=\"{H(row.Name.ToLowerInvariant())}\"><img loading=lazy src=\"{H(Uri.EscapeDataString(row.File))}\">");
            html.Append($"<div class=n>{H(row.Name)}</div><div class=d>{row.Rect.Width}&times;{row.Rect.Height}</div>");
            if (compared)
            {
                html.Append(row.Source == null
                    ? "<div class=\"d w\">no source file</div>"
                    : $"<div class=\"d{(row.SizeDiffers ? " w" : "")}\">{H(row.Source)}{(row.SizeDiffers ? " (size differs)" : "")}</div>");
            }
            html.Append("</div>");
        }
        html.Append("</div>");
        return html.ToString();
    }

    private static string IndexPage(List<(string Atlas, int Count, int NoSource, int SizeDiffers)> atlases, bool compared)
    {
        var html = new StringBuilder();
        html.Append($"<!doctype html><meta charset=utf-8><title>Unclaimed World sprites</title>{Style}");
        html.Append("<h1>Unclaimed World sprite atlases</h1><table><tr><th>atlas</th><th>sprites</th>");
        if (compared)
        {
            html.Append("<th>no source file</th><th>size differs</th>");
        }
        html.Append("</tr>");
        foreach (var a in atlases)
        {
            html.Append($"<tr><td><a href=\"{H(a.Atlas)}/index.html\">{H(a.Atlas)}</a></td><td>{a.Count}</td>");
            if (compared)
            {
                html.Append($"<td>{a.NoSource}</td><td>{a.SizeDiffers}</td>");
            }
            html.Append("</tr>");
        }
        html.Append("</table><p>sprites.csv lists every sprite with its rectangle in the atlas.</p>");
        return html.ToString();
    }

    /// <summary>
    /// A GraphicsDevice on a Form that is never shown. ContentManager finds the device through
    /// IGraphicsDeviceService, which is all a Texture2D reader asks for - no Game, no window.
    /// </summary>
    private sealed class DeviceHost : IServiceProvider, IGraphicsDeviceService, IDisposable
    {
        private readonly Form form = new Form();

        public DeviceHost()
        {
            GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef,
                new PresentationParameters
                {
                    BackBufferWidth = 64,
                    BackBufferHeight = 64,
                    DeviceWindowHandle = form.Handle,
                    IsFullScreen = false,
                });
        }

        public GraphicsDevice GraphicsDevice { get; }

        public event EventHandler<EventArgs>? DeviceCreated { add { } remove { } }
        public event EventHandler<EventArgs>? DeviceDisposing { add { } remove { } }
        public event EventHandler<EventArgs>? DeviceReset { add { } remove { } }
        public event EventHandler<EventArgs>? DeviceResetting { add { } remove { } }

        public object? GetService(Type serviceType) =>
            serviceType == typeof(IGraphicsDeviceService) ? this : null;

        public void Dispose()
        {
            GraphicsDevice.Dispose();
            form.Dispose();
        }
    }
}
