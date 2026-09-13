using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.Port;

/// <summary>
/// A one-shot record of what the world pass actually drew, and of what each render target held
/// when it was finished with.
///
/// WHY. On DesktopGL the interface renders and the world does not, and it fails SILENTLY: no
/// exception, nothing in Errors.txt, every effect loads. That leaves two very different
/// possibilities and no way to tell them apart by looking at the screen:
///
///   1. nothing is being submitted  - culling, camera, empty batches; the draw calls never happen
///   2. everything is submitted and lands nowhere - a render target, a blend/depth state, or a
///      shader that runs but writes no pixels
///
/// Counting draw calls separates those two in one run, and dumping each render target to a PNG
/// says exactly where in the chain the picture stops existing: the world is composited through
/// DiffuseMSRenderTarget -> diffuseFinalRenderTarget -> back buffer, and the interface is drawn
/// straight to the back buffer afterwards, which is consistent with UI surviving while the world
/// does not.
///
/// HOW TO USE IT. Set <c>port.renderTrace</c> in user/ModSettings.xml (or the MODS section of the
/// options menu), start a game, and look for RenderTrace.txt and RenderTrace\*.png beside the
/// executable. It captures ONE frame - by default the 120th after the world renderer starts
/// drawing, which is late enough for a map to be loaded and lit - then switches itself off, so it
/// costs one comparison per draw for the rest of the session.
///
/// Everything here is diagnostic and additive: with the switch off, <see cref="Active"/> is false
/// and every call returns immediately.
/// </summary>
public static class RenderTrace
{
    /// <summary>Which frame of the world pass to capture. The 120th is ~2 seconds in.</summary>
    private const int CaptureFrame = 120;

    private static bool? enabled;
    private static int frame;
    private static bool captured;
    private static readonly List<string> Lines = new List<string>();
    private static readonly Dictionary<string, int> Draws = new Dictionary<string, int>(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> Primitives = new Dictionary<string, int>(StringComparer.Ordinal);
    private static string outputDirectory = "RenderTrace";

    /// <summary>
    /// Whether tracing is switched on at all. Read once: the setting cannot change mid-frame, and
    /// this is tested on paths that run per draw call.
    /// </summary>
    public static bool Enabled
    {
        get
        {
            if (!enabled.HasValue)
            {
                enabled = UWGame.Mods.ModSettings.IsOn("port.renderTrace");
            }
            return enabled.Value;
        }
    }

    /// <summary>Whether this particular frame is the one being recorded.</summary>
    public static bool Active { get; private set; }

    /// <summary>
    /// Whether notes and counts are being collected at all, captured frame or not.
    ///
    /// The terrain is drawn into per-slice render targets ONCE and blitted from them every frame
    /// afterwards, so the work that matters happens long before any particular frame. Recording
    /// from the moment the switch is read - and only writing the file on the captured frame - is
    /// what makes that one-time work visible. The first version of this class gated everything on
    /// <see cref="Active"/> and reported "nothing was submitted" for a frame that was, correctly,
    /// only blitting cached textures.
    /// </summary>
    public static bool Recording => Enabled && !captured;

    private static readonly HashSet<string> Dumped = new HashSet<string>(StringComparer.Ordinal);
    private static bool backendLogged;
    private const int MaxLines = 400;

    /// <summary>
    /// Call once at the top of the world pass. Advances the frame counter and decides whether this
    /// frame is the one to capture.
    /// </summary>
    public static void BeginFrame()
    {
        if (!Enabled || captured)
        {
            Active = false;
            return;
        }

        frame++;
        Active = frame == CaptureFrame;
        if (!Active)
        {
            return;
        }

        // Deliberately NOT cleared. The terrain slices are drawn once, early, and blitted from
        // cache thereafter, so everything interesting has already been recorded by the time the
        // captured frame arrives. An earlier version cleared here and threw away exactly the
        // evidence this class was extended to collect - the report then said the world submitted
        // nothing, when what it had really done was record nine blits and discard nine redraws.
        Log("--- captured frame " + frame.ToString(CultureInfo.InvariantCulture) +
            " of the world pass; everything above happened before it ---");
    }

    /// <summary>A free-text note, in order. Collected from the moment tracing is on.</summary>
    public static void Log(string text)
    {
        if (Recording && Lines.Count < MaxLines)
        {
            Lines.Add(text);
        }
    }

    /// <summary>
    /// One submission of geometry: what it was, and how many primitives went with it. Called from
    /// the draw paths themselves, so a category with 0 draws is as informative as a large count -
    /// it means that part of the world never asked to be drawn.
    /// </summary>
    public static void Submit(string category, int primitives = 0)
    {
        if (!Recording)
        {
            return;
        }
        Draws.TryGetValue(category, out int n);
        Draws[category] = n + 1;
        Primitives.TryGetValue(category, out int p);
        Primitives[category] = p + primitives;
    }

    /// <summary>Which technique an effect was about to run, and how many passes it has.</summary>
    public static void Technique(string effect, Effect instance)
    {
        if (!Recording)
        {
            return;
        }
        if (instance == null)
        {
            Log("effect " + effect + ": NULL");
            return;
        }
        EffectTechnique technique = instance.CurrentTechnique;
        Log("effect " + effect + ": technique " + (technique?.Name ?? "(none)") +
            ", passes " + (technique?.Passes?.Count ?? 0).ToString(CultureInfo.InvariantCulture));
    }

    private static readonly HashSet<string> ModelsLogged = new HashSet<string>(StringComparer.Ordinal);

    private static readonly SortedSet<string> MissingParameters =
        new SortedSet<string>(StringComparer.Ordinal);

    private static readonly SortedSet<string> PresentParameters =
        new SortedSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Records that a parameter the game sets was, or was not, declared by the effect.
    ///
    /// WHY THIS IS WORTH RECORDING. Every backend compiles the same HLSL with a different
    /// compiler, and each prunes differently: MonoGame's OpenGL path drops eight parameters the
    /// shipped DirectX effects declare, and the FNA path drops a different set again. The game
    /// reaches parameters by name with no null check, so <see cref="EffectCompat"/> exists to
    /// tolerate that - and tolerating it silently means a value the shader genuinely needs can go
    /// unset with nothing to show for it but slightly wrong pixels.
    ///
    /// Recording BOTH sides is deliberate. "multiTex.NormalMap missing" on its own could mean the
    /// parameter was dropped or that the draw never ran; seeing NormalMap missing while
    /// ShadowFactor from the same header is present distinguishes those immediately.
    /// </summary>
    public static void Parameter(string effect, string name, bool declared)
    {
        if (!Recording)
        {
            return;
        }
        (declared ? PresentParameters : MissingParameters).Add(effect + "." + name);
    }

    /// <summary>
    /// One model about to be drawn: what would actually reach the GPU, and what it is being drawn
    /// with.
    ///
    /// The terrain and the characters fail differently and the screen shows both as "missing".
    /// The terrain draws through one shared effect and a batch this code owns, so counting its
    /// submissions was enough. A model draws through <c>ModelAnimator</c>, over its own meshes and
    /// mesh parts, with per-part skips (<c>Tag == "skip"</c>, no vertices, no primitives) and its
    /// own effect instance per part - so "nothing appeared" can mean the model was never offered,
    /// every part was skipped, the technique was not the one intended, or the skinning palette was
    /// not bound. Each wants a different fix, and only the counts separate them.
    ///
    /// The per-model detail is logged once per label: a crowded map draws the same few models
    /// hundreds of times a frame and the interesting part does not change between them, while the
    /// SUBMISSION counts keep accumulating so a model drawn zero times is still visible as such.
    /// </summary>
    public static void ModelDraw(string label, Model model, Effect effect, bool skinned)
    {
        if (!Recording || model == null)
        {
            return;
        }

        int parts = 0, skipped = 0, primitives = 0, vertices = 0;
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                if ((string)part.Tag == "skip" || part.NumVertices == 0 || part.PrimitiveCount == 0)
                {
                    skipped++;
                    continue;
                }
                parts++;
                primitives += part.PrimitiveCount;
                vertices += part.NumVertices;
            }
        }

        Submit("model " + label, primitives);

        // Detail is logged on the CAPTURED frame, not on first sight of a label.
        //
        // Logging at first encounter samples an early frame - typically before the animation
        // system has posed anything - so the bone matrices read back all-zero and look exactly
        // like the failure they were added to detect. That cost two rounds of chasing a bug that
        // was not there. Submit() above stays unconditional, because a model drawn zero times
        // must still be visible as such.
        if (!Active || !ModelsLogged.Add(label))
        {
            return;
        }

        var sb = new StringBuilder();
        sb.Append("model ").Append(label)
          .Append(": meshes ").Append(model.Meshes.Count)
          .Append(", parts drawn ").Append(parts)
          .Append(" (skipped ").Append(skipped).Append(')')
          .Append(", ").Append(primitives).Append(" primitive(s), ")
          .Append(vertices).Append(" vertex/vertices");
        Log(sb.ToString());

        LogVertexDeclaration(model);

        if (effect == null)
        {
            Log("  effect: NULL - nothing can draw");
            return;
        }

        EffectTechnique technique = effect.CurrentTechnique;
        EffectParameter palette = effect.Parameters["MatrixPalette"];
        Log("  effect technique " + (technique?.Name ?? "(none)") +
            ", passes " + (technique?.Passes?.Count ?? 0).ToString(CultureInfo.InvariantCulture) +
            ", MatrixPalette " + (palette != null
                ? palette.Elements.Count.ToString(CultureInfo.InvariantCulture) + " element(s)"
                : skinned
                    ? "ABSENT - a skinned model cannot be posed without it"
                    : "none (this effect does not skin)"));

        LogMatrixPalette(palette);
        LogWorldViewProjection(effect);
    }

    /// <summary>
    /// The first few bone matrices as the effect currently holds them.
    ///
    /// The declarations can all be right and the result still be wrong: correct vertex format,
    /// correct technique, a 56-element palette bound, and every part submitted - and skinned
    /// meshes still explode into shards, which is what happened on FNA after the blend weights
    /// were widened. That leaves the VALUES, and a bone matrix is the one input big enough to
    /// throw geometry across the screen when it is wrong.
    ///
    /// What to look for: a bone matrix should be a rigid transform - the 3x3 part roughly
    /// orthonormal (rows of length ~1) and the translation in world units. Wildly large numbers,
    /// NaN, all-zero, or a matrix that looks like the transpose of a sane one all point somewhere
    /// different, and they are trivial to tell apart by eye once printed.
    /// </summary>
    private static void LogMatrixPalette(EffectParameter palette)
    {
        if (palette == null)
        {
            return;
        }

        try
        {
            // Both MonoGame and FNA take the element count here, so no platform split is needed.
            Matrix[] bones = palette.GetValueMatrixArray(palette.Elements.Count);
            if (bones == null || bones.Length == 0)
            {
                Log("  MatrixPalette: readback returned nothing");
                return;
            }

            for (int i = 0; i < Math.Min(2, bones.Length); i++)
            {
                Log("  bone[" + i.ToString(CultureInfo.InvariantCulture) + "] " + Describe(bones[i]));
            }
        }
        catch (Exception ex)
        {
            Log("  MatrixPalette: could not read back - " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    /// <summary>The three transforms every model shares, for the same reason as the palette.</summary>
    private static void LogWorldViewProjection(Effect effect)
    {
        foreach (string name in new[] { "World", "View", "Projection" })
        {
            EffectParameter p = effect.Parameters[name];
            if (p == null)
            {
                Log("  " + name + ": ABSENT");
                continue;
            }
            try
            {
                Log("  " + name + " " + Describe(p.GetValueMatrix()));
            }
            catch (Exception ex)
            {
                Log("  " + name + ": could not read back - " + ex.GetType().Name);
            }
        }
    }

    /// <summary>A matrix on one line: the three basis row lengths, and the translation.</summary>
    private static string Describe(Matrix m)
    {
        float rx = new Vector3(m.M11, m.M12, m.M13).Length();
        float ry = new Vector3(m.M21, m.M22, m.M23).Length();
        float rz = new Vector3(m.M31, m.M32, m.M33).Length();
        return "rows(" + F(rx) + "," + F(ry) + "," + F(rz) + ") " +
               "translation(" + F(m.M41) + "," + F(m.M42) + "," + F(m.M43) + ") " +
               "m44=" + F(m.M44);
    }

    private static string F(float v) =>
        float.IsNaN(v) ? "NaN" : float.IsInfinity(v) ? "Inf" : v.ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>
    /// The vertex layout a model is actually drawn with: every element's usage, format and
    /// offset, plus the stride.
    ///
    /// THIS IS THE ONE THAT DIAGNOSES SKINNING. Both MonoGame and FNA decide whether to normalize
    /// a vertex attribute from the element's USAGE rather than its FORMAT, so a
    /// <c>BlendWeight0</c> declared as <c>Color</c> - four packed bytes, which XNA defines as
    /// 0..1 - is bound un-normalized and the weights reach the shader as 0..255. skinFX multiplies
    /// bone-transformed positions by them, and the mesh comes out scaled by about 255: shards
    /// fanning across the screen. PORT DEVIATION 14 widens those to <c>Vector4</c> at load.
    ///
    /// So this line says, in one glance, whether the fix ran on this backend:
    ///
    ///     BlendWeight0:Color@36     -> NOT converted; expect shards
    ///     BlendWeight0:Vector4@40   -> converted; skinning should be correct
    ///
    /// Without it the two cases look identical from the draw counts, which is exactly how the
    /// question "does FNA have the same bug?" stayed open.
    /// </summary>
    private static void LogVertexDeclaration(Model model)
    {
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                VertexDeclaration declaration = part.VertexBuffer?.VertexDeclaration;
                if (declaration == null)
                {
                    continue;
                }

                var sb = new StringBuilder();
                sb.Append("  vertex stride ").Append(declaration.VertexStride).Append(": ");
                bool first = true;
                foreach (VertexElement element in declaration.GetVertexElements())
                {
                    if (!first) sb.Append(' ');
                    first = false;
                    sb.Append(element.VertexElementUsage).Append(element.UsageIndex)
                      .Append(':').Append(element.VertexElementFormat)
                      .Append('@').Append(element.Offset);
                }
                Log(sb.ToString());
                return;   // one layout per model is enough; every part shares it
            }
        }
    }

    /// <summary>The device state that most often explains pixels that never appear.</summary>
    public static void DeviceState(string where, GraphicsDevice device)
    {
        if (!Recording || device == null)
        {
            return;
        }

        // Which backend produced this trace. Three of them now build from this tree and their
        // traces are otherwise indistinguishable, which makes a pasted log ambiguous exactly when
        // it matters most.
        if (backendLogged == false)
        {
            backendLogged = true;
            string backend =
#if UW_FNA
                "FNA";
#elif UW_GL
                "MonoGame DesktopGL";
#else
                "MonoGame WindowsDX";
#endif
            Log("backend " + backend + ", adapter " +
                (GraphicsAdapter.DefaultAdapter?.Description ?? "?") +
                ", profile " + device.GraphicsProfile);
        }

        Viewport viewport = device.Viewport;
        var sb = new StringBuilder();
        sb.Append("state at ").Append(where)
          .Append(": viewport ").Append(viewport.Width).Append('x').Append(viewport.Height)
          .Append(" at ").Append(viewport.X).Append(',').Append(viewport.Y)
          .Append(", blend ").Append(Describe(device.BlendState))
          .Append(", depth ").Append(Describe(device.DepthStencilState))
          .Append(", raster cull ").Append(device.RasterizerState?.CullMode.ToString() ?? "?")
          .Append(", scissor ").Append(device.ScissorRectangle.ToString());
        Log(sb.ToString());
    }

    private static string Describe(BlendState state)
    {
        if (state == null) return "?";
        if (state == BlendState.Opaque) return "Opaque";
        if (state == BlendState.AlphaBlend) return "AlphaBlend";
        if (state == BlendState.Additive) return "Additive";
        if (state == BlendState.NonPremultiplied) return "NonPremultiplied";
        return state.Name ?? "custom";
    }

    private static string Describe(DepthStencilState state)
    {
        if (state == null) return "?";
        if (state == DepthStencilState.Default) return "Default";
        if (state == DepthStencilState.DepthRead) return "DepthRead";
        if (state == DepthStencilState.None) return "None";
        return state.Name ?? "custom";
    }

    /// <summary>
    /// Writes a render target to PNG and records its size, format and sample count.
    ///
    /// This is the measurement the screen cannot give: if DiffuseMSRenderTarget holds the world
    /// and the back buffer does not, the fault is in the composite; if it is blank, the world pass
    /// wrote nothing and the shaders or the states are the place to look. Call AFTER unbinding the
    /// target - a bound target cannot be read.
    /// </summary>
    public static void DumpTarget(string name, RenderTarget2D target)
    {
        if (!Active || !Dumped.Add(name))
        {
            return;
        }
        if (target == null)
        {
            Log("target " + name + ": NULL");
            return;
        }

        Log("target " + name + ": " + target.Width + "x" + target.Height +
            ", " + target.Format + ", samples " + target.MultiSampleCount +
            ", depth " + target.DepthStencilFormat + ", usage " + target.RenderTargetUsage);

        try
        {
            Directory.CreateDirectory(outputDirectory);
            using FileStream file = File.Create(Path.Combine(outputDirectory, name + ".png"));
            target.SaveAsPng(file, target.Width, target.Height);
        }
        catch (Exception ex)
        {
            // A target that cannot be read back is itself a finding - say so rather than throwing
            // out of a diagnostic and taking the frame with it.
            Log("  could not save " + name + ": " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    /// <summary>
    /// Call at the end of the world pass. Writes RenderTrace.txt and stops tracing for good.
    /// </summary>
    public static void EndFrame()
    {
        if (!Active)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Unclaimed World - render trace");
        sb.AppendLine("Written because port.renderTrace is on. One frame only.");
        sb.AppendLine();

        foreach (string line in Lines)
        {
            sb.AppendLine(line);
        }

        sb.AppendLine();
        sb.AppendLine("draw calls and primitives by category:");
        if (Draws.Count == 0)
        {
            sb.AppendLine("  NOTHING WAS SUBMITTED - the world pass ran and drew nothing at all.");
        }
        else
        {
            foreach (KeyValuePair<string, int> entry in Draws)
            {
                Primitives.TryGetValue(entry.Key, out int prims);
                sb.Append("  ").Append(entry.Key.PadRight(34))
                  .Append(entry.Value.ToString(CultureInfo.InvariantCulture).PadLeft(6)).Append(" call(s), ")
                  .Append(prims.ToString(CultureInfo.InvariantCulture)).AppendLine(" primitive(s)");
            }
        }

        // Parameters the game SET on a path that actually drew. Reported as two lists because
        // "missing" alone cannot distinguish a dropped parameter from a draw that never ran.
        sb.AppendLine();
        sb.AppendLine("effect parameters the game set, and whether the effect declared them:");
        if (MissingParameters.Count == 0 && PresentParameters.Count == 0)
        {
            sb.AppendLine("  (none went through EffectCompat this frame)");
        }
        else
        {
            foreach (string name in MissingParameters)
            {
                sb.Append("  MISSING  ").AppendLine(name);
            }
            foreach (string name in PresentParameters)
            {
                sb.Append("  present  ").AppendLine(name);
            }
        }

        try
        {
            File.WriteAllText("RenderTrace.txt", sb.ToString());
        }
        catch (Exception)
        {
            // Nowhere to report it to - the log IS the report. Losing the file must not cost the
            // player their session.
        }

        captured = true;
        Active = false;
    }
}
