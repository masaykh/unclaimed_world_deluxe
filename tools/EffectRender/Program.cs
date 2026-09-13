using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace UW.Tools.EffectRender;

internal static class Program
{
    private static int Main(string[] args)
    {
        string? contentDir = null, outDir = null, only = null;
        int size = 128;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--effect": if (i + 1 < args.Length) only = args[++i]; break;
                case "--size": if (i + 1 < args.Length) int.TryParse(args[++i], out size); break;
                default:
                    if (contentDir == null) contentDir = args[i];
                    else if (outDir == null) outDir = args[i];
                    break;
            }
        }

        if (contentDir == null || outDir == null)
        {
            Console.Error.WriteLine("usage: effectrender <content-dir> <output-dir> [--effect NAME] [--size N]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  Renders every technique and pass of every effect to");
            Console.Error.WriteLine("  <output-dir>/<effect>.<technique>.<pass>.png, and writes a manifest.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  Parameter values are derived from parameter NAMES, so two runs - and the");
            Console.Error.WriteLine("  DirectX and OpenGL builds in particular - feed every shader identical");
            Console.Error.WriteLine("  input. That is what makes the two output sets comparable.");
            return 2;
        }

        Directory.CreateDirectory(outDir);
        using var renderer = new Renderer(contentDir, outDir, only, size);
        renderer.Run();
        return renderer.Failures == 0 ? 0 : 1;
    }

    private sealed class Renderer : Game
    {
        private readonly GraphicsDeviceManager _gdm;
        private readonly string _contentDir, _outDir;
        private readonly string? _only;
        private readonly int _size;

        public int Failures { get; private set; }

        public Renderer(string contentDir, string outDir, string? only, int size)
        {
            _contentDir = contentDir;
            _outDir = outDir;
            _only = only;
            _size = size;
            _gdm = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
                PreferredBackBufferWidth = size,
                PreferredBackBufferHeight = size,
            };
            Content.RootDirectory = contentDir;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Console.WriteLine($"content:  {_contentDir}");
            Console.WriteLine($"graphics: {GraphicsDevice.Adapter.Description} ({_gdm.GraphicsProfile})");
            Console.WriteLine();

            var manifest = new List<string>();

            foreach (string name in EffectNames())
            {
                if (_only != null && !string.Equals(name, _only, StringComparison.OrdinalIgnoreCase)) continue;

                Effect effect;
                try
                {
                    effect = Content.Load<Effect>(name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  SKIP  {name,-28} {Root(ex).GetType().Name}: {Root(ex).Message}");
                    continue;
                }

                foreach (EffectTechnique technique in effect.Techniques)
                {
                    for (int p = 0; p < technique.Passes.Count; p++)
                    {
                        string label = $"{name.Replace('/', '_')}.{technique.Name}.{p}";
                        try
                        {
                            var stats = RenderPass(effect, technique, p, label);
                            Console.WriteLine($"  OK    {label,-46} {stats}");
                            manifest.Add($"{label}\t{stats}");
                        }
                        catch (Exception ex)
                        {
                            Exception root = Root(ex);
                            Console.WriteLine($"  FAIL  {label,-46} {root.GetType().Name}: {root.Message}");
                            manifest.Add($"{label}\tFAILED\t{root.GetType().Name}: {root.Message}");
                            ReportShaderCompileFailure(root, label);
                            Failures++;
                        }
                    }
                }
            }

            File.WriteAllLines(Path.Combine(_outDir, "manifest.txt"), manifest);
            Console.WriteLine();
            Console.WriteLine($"{manifest.Count} pass(es) rendered into {_outDir}, {Failures} failed.");
            Exit();
        }

        protected override void Update(GameTime gameTime) => Exit();
        protected override void Draw(GameTime gameTime) { }

        private static Exception Root(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex;
        }

        /// <summary>
        /// Prints the GL driver's compile log for a shader that would not build, and saves the
        /// GLSL beside the frames.
        ///
        /// WHY THIS IS NOT JUST ex.Message. On DesktopGL the GLSL is compiled by the DRIVER at
        /// first use, not by mgfxc at build time - so an effect can load, validate and inject
        /// perfectly and still have a pixel shader no GPU will accept. The exception MonoGame
        /// raises then says only "Failed to compile pixel shader X. See Y.fx.", which names the
        /// shader but not the fault; the driver's own diagnostic, with line numbers into the
        /// generated GLSL, is carried in <c>ShaderCompilerException.Errors</c> and thrown away
        /// unless something reads it. That log is the difference between knowing multiTex fails
        /// and knowing WHY, and the GLSL dump is what makes its line numbers mean anything.
        /// </summary>
        private void ReportShaderCompileFailure(Exception root, string label)
        {
            if (root is not ShaderCompilerException sce)
            {
                return;
            }

            foreach (string line in (sce.Errors ?? string.Empty)
                     .Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                Console.WriteLine("          " + line.TrimEnd());
            }

            try
            {
                string path = Path.Combine(_outDir, label + ".glsl");
                File.WriteAllText(path, sce.SourceCode ?? string.Empty);
                Console.WriteLine("          GLSL written to " + Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine("          could not save GLSL: " + ex.Message);
            }
        }

        /// <summary>
        /// The effects to render. Taken from the content directory rather than a hardcoded list,
        /// so this follows whatever is actually there - including a directory where only some
        /// effects have been converted.
        /// </summary>
        private IEnumerable<string> EffectNames()
        {
            var names = new List<string>();
            foreach (string path in Directory.EnumerateFiles(_contentDir, "*.xnb", SearchOption.AllDirectories))
            {
                if (!LooksLikeEffect(path)) continue;
                string rel = Path.GetRelativePath(_contentDir, path);
                names.Add(rel[..^4].Replace('\\', '/'));
            }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        /// <summary>
        /// Cheap header sniff for an EffectReader. Matching the FULL reader name matters:
        /// "EffectReader" is a substring of "SoundEffectReader", and the game ships 220 sound
        /// effects that would otherwise all be treated as shaders.
        /// </summary>
        private static bool LooksLikeEffect(string path)
        {
            try
            {
                using FileStream fs = File.OpenRead(path);
                var head = new byte[512];
                int read = fs.Read(head, 0, head.Length);
                return System.Text.Encoding.ASCII.GetString(head, 0, read)
                    .Contains("Microsoft.Xna.Framework.Content.EffectReader", StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private string RenderPass(Effect effect, EffectTechnique technique, int passIndex, string label)
        {
            var pass = technique.Passes[passIndex];

            using var target = new RenderTarget2D(
                GraphicsDevice, _size, _size, false, SurfaceFormat.Color, DepthFormat.Depth24);

            GraphicsDevice.SetRenderTarget(target);
            // A mid-grey clear, so a shader that writes nothing is obvious and so is one that
            // writes black or white.
            GraphicsDevice.Clear(new Color(64, 64, 64, 255));

            SetParameters(effect);
            effect.CurrentTechnique = technique;

            string how = "direct";
            try
            {
                var (vertices, declaration) = BuildGeometry(pass);

                // An explicit VertexBuffer rather than DrawUserPrimitives. The user-primitives
                // path on OpenGL threw "Offset and length were out of bounds" for this 216-byte
                // vertex, and its internal staging buffer is not worth fighting when the
                // explicit route is both what real code does and one line longer.
                using var vb = new VertexBuffer(
                    GraphicsDevice, declaration, vertices.Length, BufferUsage.WriteOnly);
                vb.SetData(vertices);

                GraphicsDevice.SetVertexBuffer(vb);
                pass.Apply();

                // Overridden AFTER Apply, on purpose, so the pass's own blend/depth/cull state
                // does not decide whether anything is visible.
                //
                // This is the difference between a test and a formality. With the passes' own
                // state, 24 of 36 passes produced a frame bit-identical to the clear colour on
                // both backends - which the comparison scored as perfect agreement while
                // exercising nothing. Most of those are alpha-blended passes whose output alpha
                // is near zero for these inputs, so the blend result IS the clear colour.
                //
                // Opaque makes the shader's RGB visible whatever its alpha; depth off because
                // there is nothing to occlude; culling off because the quad's winding is
                // otherwise a coin flip against what the pass expects. Both backends get exactly
                // the same treatment, which is what keeps the comparison fair.
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.None;
                GraphicsDevice.RasterizerState = RasterizerState.CullNone;

                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertices.Length / 3);
                GraphicsDevice.SetVertexBuffer(null);
            }
            catch (Exception directError)
            {
                // A pass with no vertex shader of its own - BloomExtract and BloomCombine both -
                // cannot be drawn directly, because nothing transforms the vertices. The game
                // runs those through SpriteBatch, which supplies its own sprite vertex shader
                // and applies the effect's pixel shader on top, so that is what is reproduced
                // here rather than inventing a vertex shader for them.
                //
                // Trying the direct draw first and falling back is deliberate: MonoGame does not
                // expose a pass's shader indices, so every way of deciding from the outside was
                // a proxy for a question the device can simply answer.
                //
                // The SpriteBatch is per-attempt and disposed, which is not fastidiousness: an
                // exception thrown between Begin and End leaves that instance permanently
                // "begun", and a shared one then fails every later effect with "Begin cannot be
                // called again until End has been successfully called". One real failure turned
                // into fourteen that way.
                GraphicsDevice.Clear(new Color(64, 64, 64, 255));
                try
                {
                    using var batch = new SpriteBatch(GraphicsDevice);
                    batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp,
                                DepthStencilState.None, RasterizerState.CullNone, effect);
                    batch.Draw(TestTexture(), new Rectangle(0, 0, _size, _size), Color.White);
                    batch.End();
                }
                catch (Exception batchError)
                {
                    // Report the DIRECT error: for a pass that really does have a vertex shader,
                    // that is the actual problem and the SpriteBatch attempt is a red herring.
                    throw new InvalidOperationException(
                        $"direct draw failed ({directError.GetType().Name}: {directError.Message}); " +
                        $"SpriteBatch fallback also failed ({batchError.GetType().Name}: {batchError.Message})",
                        directError);
                }
                how = "spritebatch";
            }

            GraphicsDevice.SetRenderTarget(null);

            var pixels = new Color[_size * _size];
            target.GetData(pixels);

            string file = Path.Combine(_outDir, label + ".png");
            using (FileStream fs = File.Create(file))
                target.SaveAsPng(fs, _size, _size);

            // The same frame as raw RGBA. The PNG is for looking at; this is for comparing,
            // and having it means the DirectX/OpenGL diff needs no PNG decoder and cannot be
            // confused by two encoders producing different bytes for the same image.
            var raw = new byte[pixels.Length * 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                raw[i * 4 + 0] = pixels[i].R;
                raw[i * 4 + 1] = pixels[i].G;
                raw[i * 4 + 2] = pixels[i].B;
                raw[i * 4 + 3] = pixels[i].A;
            }
            File.WriteAllBytes(Path.Combine(_outDir, label + ".rgba"), raw);

            return Describe(pixels) + " via=" + how;
        }

        /// <summary>
        /// Summarises the image so a regression is visible in text as well as in the PNG, and so
        /// "nothing was drawn" is distinguishable from "something was drawn".
        /// </summary>
        private static string Describe(Color[] pixels)
        {
            long r = 0, g = 0, b = 0, a = 0;
            int clear = 0;
            var distinct = new HashSet<uint>();
            foreach (Color c in pixels)
            {
                r += c.R; g += c.G; b += c.B; a += c.A;
                if (c.R == 64 && c.G == 64 && c.B == 64) clear++;
                if (distinct.Count < 4096) distinct.Add(c.PackedValue);
            }
            int n = pixels.Length;
            return $"mean=({r / n},{g / n},{b / n},{a / n}) colours={distinct.Count} " +
                   $"untouched={100 * clear / n}%";
        }

        // ---------------------------------------------------------------------------------------

        /// <summary>
        /// Builds a full-screen-ish quad whose vertex declaration is derived from the pass's own
        /// vertex-shader attribute usages, so every attribute the shader binds is fed something.
        ///
        /// This doubles as a direct test of the attribute-binding fix: if a shader's declared
        /// usages did not match a real vertex declaration, MonoGame would bind nothing and the
        /// draw would produce an untouched frame.
        /// </summary>
        private (TestVertex[], VertexDeclaration) BuildGeometry(EffectPass pass)
        {
            // Positions span clip space so the quad covers the target whether the shader
            // transforms by an identity matrix or passes position straight through.
            var quad = new[]
            {
                Vertex(-1f, -1f, 0f, 1f),
                Vertex(-1f,  1f, 0f, 0f),
                Vertex( 1f,  1f, 1f, 0f),

                Vertex(-1f, -1f, 0f, 1f),
                Vertex( 1f,  1f, 1f, 0f),
                Vertex( 1f, -1f, 1f, 1f),
            };

            return (quad, TestVertex.Declaration);
        }

        private static TestVertex Vertex(float x, float y, float u, float v)
        {
            var t = new TestVertex
            {
                Position = new Vector3(x, y, 0f),
                Color = new Color(17, 34, 51, 255),
                Normal = new Vector3(0.25f, 0.5f, 0.75f),
                // Four packed bytes 0x99,0x4D,0x1A,0x00 smuggled through a float, so the Color
                // element on DirectX reads weights 0.600, 0.302, 0.102, 0 - a plausible skinning
                // split that sums to 1. BlendWeightWide carries the same four values as floats
                // for the OpenGL declaration, so both backends see the same weights.
                BlendWeight = new Vector4(BitConverter.UInt32BitsToSingle(0x001A4D99u), 0f, 0f, 0f),
                BlendWeightWide = new Vector4(153f / 255f, 77f / 255f, 26f / 255f, 0f),
                // Distinct bone indices, so that a shader indexing the matrix palette wrongly
                // shows up. All-zero indices made every palette read hit the same element and
                // hid the indexed path entirely.
                BlendIndex0 = 1, BlendIndex1 = 2, BlendIndex2 = 3, BlendIndex3 = 4,
                Tangent = new Vector3(0.9f, 0.8f, 0.7f),
                Binormal = new Vector3(0f, 1f, 0f),
                Position1 = new Vector3(x, y, 0f),
            };
            // Every texcoord set gets the same UV, so a shader that reads TEXCOORD3 (and several
            // do) still samples something meaningful rather than zeros.
            t.Tex0 = t.Tex1 = t.Tex2 = t.Tex3 = t.Tex4 = t.Tex5 = t.Tex6 = t.Tex7 =
                new Vector4(u, v, 0f, 1f);
            return t;
        }

        /// <summary>
        /// A vertex carrying every usage the shipped shaders declare.
        ///
        /// It has to be this complete: on DirectX the input layout must satisfy the shader's
        /// input signature, and a quad of position/colour/texcoord alone made 62 of 69 passes
        /// fail with E_INVALIDARG. Extra elements are harmless - each backend binds only what
        /// its shader declares - so over-providing is the right trade against deriving a
        /// declaration per shader, which would need Shader.Attributes and that is internal.
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(
            System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
        private struct TestVertex
        {
            public Vector3 Position;      //   0
            public Color Color;           //  12
            public Vector3 Normal;        //  16
            public Vector4 Tex0;          //  28
            public Vector4 Tex1;          //  44
            public Vector4 Tex2;          //  60
            public Vector4 Tex3;          //  76
            public Vector4 Tex4;          //  92
            public Vector4 Tex5;          // 108
            public Vector4 Tex6;          // 124
            public Vector4 Tex7;          // 140
            public Vector4 BlendWeight;   // 156
            // Short4, not Byte4. skinFX declares BLENDINDICES0 with component type *int*, and
            // DirectX requires the vertex format's type class to match the register's: Byte4 is
            // R8G8B8A8_UINT, which against a SINT register fails CreateInputLayout with
            // E_INVALIDARG. Short4 is R16G16B16A16_SINT.
            public short BlendIndex0;     // 172
            public short BlendIndex1;     // 174
            public short BlendIndex2;     // 176
            public short BlendIndex3;     // 178
            public Vector3 Tangent;       // 180
            public Vector3 Binormal;      // 192
            // Position with semantic index 1. Billboard, RoadsAndPaths and LightSourcesEffect
            // all declare POSITION1 alongside POSITION0 - a second position stream, not a typo -
            // and without it their input layouts are incomplete.
            public Vector3 Position1;     // 204
            // The same four weights as floats, for the GL declaration below. Both encodings are
            // always present so the struct is one size on both backends and only the
            // VertexDeclaration differs.
            public Vector4 BlendWeightWide; // 216
                                          // 232 bytes

            // The stride is stated rather than inferred. A VertexDeclaration built from elements
            // alone takes its stride from the last element, and the two backends no longer end on
            // the same one - DirectX stops at Position1 (216) while OpenGL reads BlendWeightWide
            // (232). That mismatch against the 232-byte struct is rejected with "The vertex stride
            // is larger than the vertex buffer" on 60 of 69 passes.
            public const int Stride = 232;

            public static readonly VertexDeclaration Declaration = new(
                Stride,
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(28, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(44, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(60, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(76, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(92, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
                new VertexElement(108, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5),
                new VertexElement(124, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 6),
                new VertexElement(140, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 7),
                // Blend weights, declared PER BACKEND because the game declares them per backend.
                //
                // Every skinned model ships them as BlendWeight0:Color@36 - four packed bytes,
                // which XNA maps to 0..1. MonoGame's OpenGL backend decides normalization from an
                // element's USAGE rather than its format, so a Color-format element whose usage
                // is BlendWeight binds un-normalized and the weights arrive as 0..255, scaling
                // skinned geometry by about 255. PORT DEVIATION 14 fixes that in the data: on
                // DesktopGL, UwContentManager widens every skinned model's weights to Vector4 as
                // it loads them.
                //
                // So Color here is right for DirectX and WRONG for OpenGL - it reproduces a bug
                // the game no longer has, and reported all seven skinFX passes and six of
                // Vehicle's eight as differing when the shaders were fine. Matching the game on
                // each backend is what makes a disagreement mean something about the SHADER,
                // which is the only thing this harness exists to measure.
#if UW_GL
                new VertexElement(216, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
#else
                new VertexElement(156, VertexElementFormat.Color, VertexElementUsage.BlendWeight, 0),
#endif
                new VertexElement(172, VertexElementFormat.Short4, VertexElementUsage.BlendIndices, 0),
                new VertexElement(180, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
                new VertexElement(192, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
                new VertexElement(204, VertexElementFormat.Vector3, VertexElementUsage.Position, 1));
        }

        /// <summary>
        /// Gives every parameter a value derived from its NAME, so the DirectX and OpenGL runs
        /// feed each shader identical input and the two output sets can be compared directly.
        ///
        /// Matrices are the exception that has to be handled rather than randomised: a random
        /// world/view/projection would transform the quad off screen and BOTH backends would
        /// render an empty frame, which reads as agreement while proving nothing. They get
        /// identity instead.
        /// </summary>
        private void SetParameters(Effect effect)
        {
            foreach (EffectParameter p in effect.Parameters)
            {
                try
                {
                    SetParameter(p);
                }
                catch
                {
                    // A parameter class this does not handle is not worth failing the render
                    // over; it keeps whatever default the effect shipped with.
                }
            }
        }

        private void SetParameter(EffectParameter p)
        {
            if (p.ParameterClass == EffectParameterClass.Object)
            {
                if (p.ParameterType is EffectParameterType.Texture or EffectParameterType.Texture2D)
                    p.SetValue(TestTexture());
                return;
            }

            uint seed = Hash(p.Name);

            if (p.ParameterClass == EffectParameterClass.Matrix)
            {
                // A matrix ARRAY is a bone palette, and filling it with identities makes the test
                // blind to the thing most likely to be wrong about it: which element the shader
                // indexes. Every index then gives the same answer, so skinFX compared identical
                // while its indexed palette read was never exercised at all. Each element gets a
                // small distinct translation instead - enough that picking the wrong bone moves
                // the geometry visibly, small enough that it stays on screen and comparable.
                if (p.Elements.Count > 0)
                {
                    var matrices = new Matrix[p.Elements.Count];
                    for (int i = 0; i < matrices.Length; i++)
                    {
                        matrices[i] = Matrix.CreateTranslation(
                            0.01f * (i % 8), 0.013f * (i % 5), 0.007f * (i % 3));
                    }
                    p.SetValue(matrices);
                }
                else
                {
                    // NOT identity. An identity matrix is symmetric, so it cannot tell a correct
                    // constant-buffer layout from a transposed one - the single most likely thing
                    // to be wrong about a matrix parameter, and one that shows up in game as
                    // geometry flung across the screen while the bench reports agreement. This is
                    // asymmetric enough to expose a transpose and mild enough to keep the test
                    // quad on screen.
                    p.SetValue(
                        Matrix.CreateRotationZ(0.20f) *
                        Matrix.CreateRotationY(0.11f) *
                        Matrix.CreateTranslation(0.07f, 0.03f, 0.02f));
                }
                return;
            }

            int count = Math.Max(p.Elements.Count, 1) * Math.Max(p.RowCount, 1) * Math.Max(p.ColumnCount, 1);
            switch (p.ParameterType)
            {
                case EffectParameterType.Bool:
                {
                    // Deterministically true for about half the flags, so both branches of the
                    // shipped shaders get exercised across the set.
                    bool value = (seed & 1) != 0;
                    if (p.Elements.Count > 0 || count > 1) return;   // arrays of bools are rare and awkward
                    p.SetValue(value);
                    return;
                }
                case EffectParameterType.Int32:
                {
                    if (count > 1) return;
                    p.SetValue((int)(seed % 4));
                    return;
                }
                case EffectParameterType.Single:
                {
                    // A surface's pixel dimensions are the other value that cannot be randomised.
                    // GUI/CRT and GUI/LCD divide the vertex position by ViewportSize to get clip
                    // space, so a fractional value throws the quad far off screen and both
                    // backends render nothing - which reads as agreement and tested neither of
                    // the two effects the panel rendering actually depends on. The shipped set
                    // has three such parameters (ViewportSize, ScreenResolution and
                    // ScanlinesTextureDimensions) and all three are float2, while every
                    // similarly-named scalar (EdgeWidth, WindWaveSize, BillboardWidth) means
                    // something else - hence the component count in the test.
                    if (count == 2 && LooksLikeSurfaceSize(p.Name))
                    {
                        p.SetValue(new Vector2(_size, _size));
                        return;
                    }

                    // The handful of parameters that decide whether anything lands on screen at
                    // all. A hashed fraction for a near/far plane makes the depth divisor
                    // nonsense and the geometry vanishes on BOTH backends, which reads as
                    // agreement while testing nothing - multiTex's 17 passes were all blank for
                    // exactly this reason, leaving the terrain shaders entirely unvalidated.
                    float? known = KnownSceneValue(p.Name);
                    if (known.HasValue && count == 1)
                    {
                        p.SetValue(known.Value);
                        return;
                    }
                    if (count == 2 && LooksLikeWorldOffset(p.Name))
                    {
                        p.SetValue(Vector2.Zero);
                        return;
                    }

                    var values = new float[count];
                    for (int i = 0; i < count; i++)
                        values[i] = Unit(Hash(p.Name + ":" + i.ToString(CultureInfo.InvariantCulture)));
                    if (count == 1) p.SetValue(values[0]);
                    else p.SetValue(values);
                    return;
                }
            }
        }


        private Texture2D? _testTexture;

        /// <summary>
        /// A deterministic texture with structure in it: a checkerboard tinted by a gradient, so
        /// a sampling difference between the two backends shows up as a visible pattern shift
        /// rather than a uniform colour change.
        /// </summary>
        private Texture2D TestTexture()
        {
            if (_testTexture != null) return _testTexture;

            const int n = 64;
            var pixels = new Color[n * n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    bool light = ((x / 8) + (y / 8)) % 2 == 0;
                    byte baseValue = (byte)(light ? 220 : 60);
                    pixels[y * n + x] = new Color(
                        (byte)(baseValue * x / n + 20),
                        (byte)(baseValue * y / n + 20),
                        baseValue,
                        (byte)255);
                }
            }

            _testTexture = new Texture2D(GraphicsDevice, n, n);
            _testTexture.SetData(pixels);
            return _testTexture;
        }

        /// <summary>FNV-1a over the name: stable across runs, processes and backends.</summary>
        private static uint Hash(string s)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (char c in s) h = (h ^ c) * 16777619u;
                return h;
            }
        }

        /// <summary>A value in [0.1, 1.0] - never exactly zero, so a parameter that is used is visible.</summary>
        private static float Unit(uint hash) => 0.1f + 0.9f * ((hash % 1000) / 1000f);

        /// <summary>
        /// Values for the parameters that place geometry in depth. These are named rather than
        /// hashed because a wrong one does not perturb the image, it empties it.
        /// </summary>
        private static float? KnownSceneValue(string name)
        {
            if (name.IndexOf("NearPlane", StringComparison.OrdinalIgnoreCase) >= 0) return 1f;
            if (name.IndexOf("FarPlane", StringComparison.OrdinalIgnoreCase) >= 0) return 10000f;
            if (name.IndexOf("ZOffset", StringComparison.OrdinalIgnoreCase) >= 0) return 0f;
            return null;
        }

        /// <summary>A world- or window-space origin, which belongs at zero rather than randomised.</summary>
        private static bool LooksLikeWorldOffset(string name) =>
            name.IndexOf("WindowPosition", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("Offset", StringComparison.OrdinalIgnoreCase) >= 0;

        /// <summary>Whether a parameter name denotes the pixel dimensions of a surface.</summary>
        private static bool LooksLikeSurfaceSize(string name) =>
            name.IndexOf("Size", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("Resolution", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("Dimensions", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
