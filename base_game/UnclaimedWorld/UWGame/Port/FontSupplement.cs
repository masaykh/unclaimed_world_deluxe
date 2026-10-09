using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.Port;

/// <summary>
/// PORT: letters a hand-drawn font does not have, added from a font built from a TrueType face.
///
/// WHY. Three of the studio's fonts are bitmaps drawn by hand - Fonts/newtown_8pt (the text on
/// buttons), Fonts/CRTGlow and Fonts/CRT_18pt (the status screen) - with ASCII and nothing else.
/// tools/build/36-build-fonts.sh rebuilds the TrueType fonts with Cyrillic and Greek, but these
/// cannot be rebuilt: there is no face to rebuild them from. A translated button drew as dots.
///
/// HOW. 36-build-fonts.sh also writes "&lt;font&gt;.supplement" beside them - a TrueType face at a
/// size close to the studio glyphs', holding only the letters beyond ASCII. When the game loads a
/// font and a supplement for it exists (UwContentManager.Load), the two become one SpriteFont:
/// the studio's texture and glyphs exactly as they were, the supplement's glyphs placed beside
/// them in a wider texture, moved down or up so that their baseline is the studio's (measured on
/// the ink of 'A' and Cyrillic 'А', or 'O' and Greek 'Ο'). English text looks exactly as before.
///
/// Where the studio font draws lower case as capitals - newtown does - the supplement's lower
/// case letters do the same, taken from its capitals.
/// </summary>
public static class FontSupplement
{
    /// <summary>The asset name a font's supplement is loaded under.</summary>
    public static string AssetName(string fontAsset) => fontAsset + ".supplement";

    /// <summary>
    /// The studio font with the supplement's letters added; <paramref name="studio"/> itself when
    /// there is nothing to add or the textures cannot be read.
    /// </summary>
    public static SpriteFont Merge(SpriteFont studio, SpriteFont supplement, GraphicsDevice device)
    {
        if (studio == null || supplement == null || device == null
            || studio.Texture.Format != SurfaceFormat.Color || supplement.Texture.Format != SurfaceFormat.Color)
        {
            return studio;
        }
        Dictionary<char, SpriteFont.Glyph> studioGlyphs = studio.GetGlyphs();
        Dictionary<char, SpriteFont.Glyph> extraGlyphs = supplement.GetGlyphs();
        var added = new List<char>();
        foreach (char c in extraGlyphs.Keys)
        {
            if (!studioGlyphs.ContainsKey(c))
            {
                added.Add(c);
            }
        }
        if (added.Count == 0)
        {
            return studio;
        }

        Color[] a = Pixels(studio.Texture);
        Color[] b = Pixels(supplement.Texture);
        int width = studio.Texture.Width + 1 + supplement.Texture.Width;
        int height = Math.Max(studio.Texture.Height, supplement.Texture.Height);
        var merged = new Color[width * height];
        Blit(a, studio.Texture.Width, studio.Texture.Height, merged, width, 0);
        int offsetX = studio.Texture.Width + 1;
        Blit(b, supplement.Texture.Width, supplement.Texture.Height, merged, width, offsetX);

        int shift = BaselineShift(studio, a, supplement, b);
        bool capitalsOnly = DrawsLowerCaseAsCapitals(studio, a);

        var bounds = new List<Rectangle>();
        var cropping = new List<Rectangle>();
        var characters = new List<char>();
        var kerning = new List<Vector3>();
        foreach (KeyValuePair<char, SpriteFont.Glyph> pair in studioGlyphs)
        {
            Add(pair.Key, pair.Value, 0, 0);
        }
        added.Sort();
        foreach (char c in added)
        {
            SpriteFont.Glyph glyph = extraGlyphs[c];
            char upper = char.ToUpperInvariant(c);
            if (capitalsOnly && upper != c && extraGlyphs.TryGetValue(upper, out SpriteFont.Glyph capital))
            {
                glyph = capital;
            }
            Add(c, glyph, offsetX, shift);
        }

        var texture = new Texture2D(device, width, height, false, SurfaceFormat.Color);
        texture.SetData(merged);
        return new SpriteFont(texture, bounds, cropping, characters, studio.LineSpacing, studio.Spacing, kerning, studio.DefaultCharacter);

        void Add(char c, SpriteFont.Glyph g, int dx, int dy)
        {
            Rectangle r = g.BoundsInTexture;
            r.X += dx;
            bounds.Add(r);
            Rectangle crop = g.Cropping;
            crop.Y += dy;
            cropping.Add(crop);
            characters.Add(c);
            kerning.Add(new Vector3(g.LeftSideBearing, g.Width, g.RightSideBearing));
        }
    }

    /// <summary>
    /// The rows of <paramref name="c"/>'s ink, measured from the top of the line: the first row
    /// with any pixel showing and the one after the last. Null when the font lacks it.
    /// </summary>
    public static (int top, int bottom)? Ink(SpriteFont font, char c)
    {
        return font.Texture.Format == SurfaceFormat.Color ? Ink(font, Pixels(font.Texture), c) : null;
    }

    private static (int top, int bottom)? Ink(SpriteFont font, Color[] pixels, char c)
    {
        if (!font.GetGlyphs().TryGetValue(c, out SpriteFont.Glyph g))
        {
            return null;
        }
        int w = font.Texture.Width;
        int top = -1, bottom = -1;
        for (int y = 0; y < g.BoundsInTexture.Height; y++)
        {
            for (int x = 0; x < g.BoundsInTexture.Width; x++)
            {
                if (pixels[(g.BoundsInTexture.Y + y) * w + g.BoundsInTexture.X + x].A > 64)
                {
                    if (top < 0) top = y;
                    bottom = y + 1;
                    break;
                }
            }
        }
        return top < 0 ? null : (top + g.Cropping.Y, bottom + g.Cropping.Y);
    }

    /// <summary>How far down to move the supplement's glyphs so their baseline is the studio's.</summary>
    private static int BaselineShift(SpriteFont studio, Color[] a, SpriteFont supplement, Color[] b)
    {
        foreach ((char latin, char other) in new[] { ('A', 'А'), ('O', 'Ο'), ('H', 'Н') })
        {
            var mine = Ink(studio, a, latin);
            var theirs = Ink(supplement, b, other) ?? Ink(supplement, b, latin);
            if (mine.HasValue && theirs.HasValue)
            {
                return mine.Value.bottom - theirs.Value.bottom;
            }
        }
        return 0;
    }

    /// <summary>Whether 'a' and 'A' are the same picture - a capitals-only face.</summary>
    private static bool DrawsLowerCaseAsCapitals(SpriteFont font, Color[] pixels)
    {
        Dictionary<char, SpriteFont.Glyph> glyphs = font.GetGlyphs();
        int same = 0, compared = 0;
        foreach (char lower in "aehmrs")
        {
            if (!glyphs.TryGetValue(lower, out var l) || !glyphs.TryGetValue(char.ToUpperInvariant(lower), out var u))
            {
                continue;
            }
            compared++;
            if (SamePicture(pixels, font.Texture.Width, l.BoundsInTexture, u.BoundsInTexture))
            {
                same++;
            }
        }
        return compared > 0 && same == compared;
    }

    private static bool SamePicture(Color[] pixels, int width, Rectangle p, Rectangle q)
    {
        if (p.Width != q.Width || p.Height != q.Height)
        {
            return false;
        }
        for (int y = 0; y < p.Height; y++)
        {
            for (int x = 0; x < p.Width; x++)
            {
                if (pixels[(p.Y + y) * width + p.X + x].A != pixels[(q.Y + y) * width + q.X + x].A)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static Color[] Pixels(Texture2D texture)
    {
        var data = new Color[texture.Width * texture.Height];
        texture.GetData(data);
        return data;
    }

    private static void Blit(Color[] source, int sw, int sh, Color[] target, int tw, int dx)
    {
        for (int y = 0; y < sh; y++)
        {
            Array.Copy(source, y * sw, target, y * tw + dx, sw);
        }
    }
}
