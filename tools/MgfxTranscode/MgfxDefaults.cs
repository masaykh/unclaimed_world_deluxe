using System;
using System.Collections.Generic;

namespace UW.Tools.MgfxTranscode;

/// <summary>
/// Restores the initial values of effect parameters, copied from the effect that shipped.
///
/// THE BUG THIS REPAIRS. mgfxc's OpenGL path writes ZERO for every parameter's initial value.
/// Its DirectX path writes the initialiser from the HLSL. Same compiler, same source, same run -
/// the profile alone decides whether `uniform const float3 LightColor = float3(1.15, 1.15, 1.0);`
/// arrives as 1.15 or as nothing.
///
/// For this game that is not cosmetic, because several uniforms are declared with an initialiser
/// and NEVER assigned from code. The initialiser is the value. Zeroed, they produce:
///
///   Billboard / RoadsAndPaths / multiTex   LightColor, AmbientColorForNormalMapping = 0
///                                          -> every sprite is tex * 0: the world renders as
///                                             correctly-shaped BLACK SILHOUETTES
///   skinFX / Vehicle                       Alpha, AlphaFactor, DiffuseColor = 0
///                                          -> every character and vehicle is fully transparent
///   EdgeDetect                             all six thresholds = 0
///   CloudShadows                           ShadowColor = 0
///   RoundLine                              blurThreshold = 0
///
/// and not one of them is an error: the effects load, the shaders compile, the draws happen, the
/// parameters are all present. Only the pixels are wrong.
///
/// WHY IT SURVIVED THE BENCH. tools/EffectRender assigns every parameter by name before it
/// renders, so it overwrote each zeroed default with a test value and reported the effects
/// IDENTICAL to DirectX. A harness that sets everything cannot see a default at all. This is why
/// the check lives here, against the shipped binary, rather than in a render comparison.
///
/// HOW. The values are fixed-width and stored in place, so this is a byte patch: find each
/// top-level parameter's span in both blobs, and where the name, type and value count all agree,
/// copy the reference bytes over. Nothing is re-serialised and nothing else moves. A parameter
/// whose shape differs is skipped and reported rather than guessed at - mgfxc's OpenGL path also
/// narrows unused matrix rows (float4x4 -> 16 floats on DirectX, 12 or 8 here), and those are
/// assigned from code every frame anyway.
/// </summary>
internal static class MgfxDefaults
{
    internal readonly record struct Result(byte[] Blob, int Patched, List<string> Skipped)
    {
        public override string ToString() =>
            Patched == 0
                ? "no parameter defaults needed restoring"
                : $"{Patched} parameter default(s) restored from the shipped effect" +
                  (Skipped.Count > 0 ? $", {Skipped.Count} skipped ({string.Join(", ", Skipped)})" : "");
    }

    /// <summary>
    /// Returns <paramref name="target"/> with every parameter default replaced by the one in
    /// <paramref name="reference"/>, for parameters the two agree on.
    /// </summary>
    public static Result Apply(byte[] target, byte[] reference)
    {
        List<MgfxV10Reader.DefaultSpan> targetSpans = MgfxV10Reader.ReadDefaultSpans(target);
        List<MgfxV10Reader.DefaultSpan> referenceSpans = MgfxV10Reader.ReadDefaultSpans(reference);

        var byName = new Dictionary<string, MgfxV10Reader.DefaultSpan>(StringComparer.Ordinal);
        foreach (MgfxV10Reader.DefaultSpan span in referenceSpans)
        {
            byName[span.Name] = span;
        }

        byte[] patched = null;
        int count = 0;
        var skipped = new List<string>();

        foreach (MgfxV10Reader.DefaultSpan mine in targetSpans)
        {
            if (!byName.TryGetValue(mine.Name, out MgfxV10Reader.DefaultSpan theirs))
            {
                continue;   // not in the shipped effect; nothing to copy
            }

            if (theirs.Type != mine.Type || theirs.ValueCount != mine.ValueCount ||
                theirs.ByteLength != mine.ByteLength)
            {
                // Different shape - the OpenGL build narrowed the parameter. Copying bytes across
                // a shape change would write one parameter's value into the next one's storage.
                skipped.Add($"{mine.Name} {theirs.ValueCount}->{mine.ValueCount}");
                continue;
            }

            bool same = true;
            for (int i = 0; i < mine.ByteLength; i++)
            {
                if (target[mine.Offset + i] != reference[theirs.Offset + i])
                {
                    same = false;
                    break;
                }
            }
            if (same)
            {
                continue;
            }

            patched ??= (byte[])target.Clone();
            Buffer.BlockCopy(reference, theirs.Offset, patched, mine.Offset, mine.ByteLength);
            count++;
        }

        return new Result(patched ?? target, count, skipped);
    }
}
