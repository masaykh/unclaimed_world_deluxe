using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UW.Tools.MgfxTranscode;

/// <summary>
/// CLI for rewriting the game's MGFX v8 effects as MGFX v10. See <see cref="MgfxRewriter"/>
/// for the format details and <see cref="XnbFile"/> for the container handling.
/// </summary>
internal static class Program
{
    private const string Usage = """
        mgfxtranscode - rewrite MonoGame 3.6 MGFX v8 effects as MGFX v10 (MonoGame 3.8.1+)

          scan <dir>
              Report every .xnb under <dir> whose primary object is an Effect, with its MGFX
              version, shader profile and whether the container is LZ4-compressed. Read-only.

          transcode <dir> [--backup <dir>] [--dry-run]
              Rewrite every MGFX v8 effect .xnb under <dir> in place. Effects already at v10
              are skipped. With --backup, each original is copied there first, preserving its
              relative path.

          raw <in.mgfxo> <out.mgfxo>
              Rewrite a bare MGFX blob with no XNB container. Used to validate the rewriter
              against MonoGame's own stock effects.

          inject <template.xnb> <effect.mgfxo> <out.xnb> [<defaults-from.xnb>]
              Wrap a bare MGFX blob (as produced by `mgfxc`) in an XNB container, reusing the
              container of <template.xnb> - normally the shipped effect of the same name.
              This is how a recompiled effect becomes something ContentManager can load
              without writing an XNB writer: the type-reader table, shared-resource count and
              primary type id are all taken verbatim from a container the game already reads.
              Used to build the OpenGL-profile effects for the DesktopGL target.
              With <defaults-from.xnb>, each parameter's INITIAL VALUE is copied from that
              effect: mgfxc's OpenGL path writes zero for every default while its DirectX
              path writes the HLSL initialiser, and this game leaves several uniforms
              (LightColor, Alpha, AlphaFactor...) entirely to their initialisers.

          shaders <path>...
              Per-shader DXBC report: shader model, whether an Aon9 (DX9 fallback) chunk is
              present, and the STAT chunk's instruction / temp-register / texture counts,
              with a verdict on whether the shader would fit ps_3_0 - the ceiling of
              MonoGame's OpenGL shader profile. Read-only.

          validate <path>...
              Parse each file as MGFX v10 and require that the whole blob is consumed and the
              tail signature is present - the same completeness check MonoGame's own Effect
              constructor makes. Accepts .xnb containers and bare .mgfxo blobs, and directories
              (searched recursively for .xnb). Read-only.

          compare-params <reference-dir> <candidate-dir>
              Compare the parameter NAMES each effect exposes, matching by relative path.
              Fails if any effect in <candidate-dir> is missing a parameter its counterpart
              in <reference-dir> has. The game reaches parameters by name without checking,
              so a rebuilt effect that lost one crashes the first frame that draw runs.
              Read-only.

          decompress <dir>
              Rewrite every compressed .xnb under <dir> in place as an UNCOMPRESSED one.
              MonoGame compresses with LZ4 (flag 0x40); XNA used LZX (0x80) and FNA, which
              implements XNA, tests only for 0x80 - so it reads a MonoGame container as if it
              were uncompressed and dies on an EMPTY type-reader name. An uncompressed XNB is
              valid for XNA, MonoGame and FNA alike.

        Exit code is non-zero if any file failed.
        """;

    private static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
        {
            Console.WriteLine(Usage);
            return args.Length == 0 ? 2 : 0;
        }

        try
        {
            return args[0] switch
            {
                "scan" when args.Length == 2 => Scan(args[1]),
                "transcode" when args.Length >= 2 => Transcode(args),
                "raw" when args.Length == 3 => Raw(args[1], args[2]),
                "validate" when args.Length >= 2 => Validate(args[1..]),
                "inject" when args.Length is 4 or 5 => Inject(args[1], args[2], args[3], args.Length == 5 ? args[4] : null),
                "shaders" when args.Length >= 2 => Shaders(args[1..]),
                "compare-params" when args.Length == 3 => CompareParams(args[1], args[2]),
                "decompress" when args.Length == 2 => Decompress(args[1]),
                _ => Fail("Unrecognised arguments.\n\n" + Usage),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FATAL: {ex.Message}");
            return 1;
        }
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 2;
    }

    private static IEnumerable<string> XnbFiles(string dir) =>
        Directory.EnumerateFiles(dir, "*.xnb", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal);

    /// <summary>Peeks the MGFX header without a full parse: signature, version, profile.</summary>
    private static (int Version, int Profile) PeekMgfx(byte[] blob) => (blob[4], blob[5]);

    /// <summary>Whether a default is all zeros, i.e. carries no initialiser worth comparing.</summary>
    private static bool IsAllZero(string values) =>
        values.Split(',').All(v => v.Trim() == "0");

    /// <summary>
    /// Compares the parameter names of two sets of effects, matched by relative path.
    ///
    /// WHY A GATE AND NOT A NOTE. The game does
    /// <c>effect.Parameters["WindowPosition"].SetValue(...)</c> with no null check - which is
    /// correct against the effects it shipped with - so an effect rebuilt from the studio's
    /// current HLSL that no longer exposes that parameter is a NullReferenceException in
    /// GameWorldRenderer, on the first frame the overlay draws, with nothing in the build to
    /// suggest it. mgfxc drops any uniform the shader does not read, and the sources carry
    /// commented-out code whose parameters the shipped binaries still declare, so this is the
    /// normal outcome of a faithful rebuild rather than a rare accident.
    ///
    /// EXTRA parameters are reported but not failed: a candidate that declares more than the
    /// reference can only mean a name the game never asks for.
    /// </summary>
    private static int CompareParams(string referenceDir, string candidateDir)
    {
        int compared = 0, missing = 0, absent = 0;

        foreach (string candidate in XnbFiles(candidateDir))
        {
            string relative = Path.GetRelativePath(candidateDir, candidate);
            string reference = Path.Combine(referenceDir, relative);
            if (!File.Exists(reference))
            {
                Console.WriteLine($"  ?? {relative,-40} no counterpart in {referenceDir}");
                absent++;
                continue;
            }

            List<string> referenceNames, candidateNames;
            try
            {
                referenceNames = MgfxV10Reader.ReadParameterNames(XnbFile.Read(reference).ReadEffectBlob());
                candidateNames = MgfxV10Reader.ReadParameterNames(XnbFile.Read(candidate).ReadEffectBlob());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ?? {relative,-40} unreadable: {ex.Message}");
                absent++;
                continue;
            }

            compared++;
            var lost = referenceNames.Except(candidateNames, StringComparer.Ordinal).ToList();
            var gained = candidateNames.Except(referenceNames, StringComparer.Ordinal).ToList();

            // Initial values, which matter as much as the names. Several uniforms are declared
            // with an initialiser and never assigned by the game - Billboard's LightColor and
            // AmbientColorForNormalMapping are the lighting for every sprite in the world - so an
            // effect that keeps a parameter but loses its default multiplies by zero and renders
            // the whole scene as black silhouettes, with nothing missing and nothing to catch.
            var changedDefaults = new List<string>();
            try
            {
                var referenceDefaults = MgfxV10Reader.ReadParameterDefaults(XnbFile.Read(reference).ReadEffectBlob());
                var candidateDefaults = MgfxV10Reader.ReadParameterDefaults(XnbFile.Read(candidate).ReadEffectBlob());
                foreach (KeyValuePair<string, string> entry in referenceDefaults)
                {
                    if (!candidateNames.Contains(entry.Key, StringComparer.Ordinal))
                    {
                        continue;   // already reported as missing
                    }
                    if (candidateDefaults.TryGetValue(entry.Key, out string mine) &&
                        string.Equals(mine, entry.Value, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    // A reference default of all zeros carries no information, so a difference
                    // against it is not a lost initialiser. This is what the matrices look like:
                    // mgfxc's OpenGL path narrows an unused float4x4 to 12 or 8 floats, and both
                    // sides are zero because the game assigns them every frame. Reporting those
                    // would bury the ones that matter.
                    if (IsAllZero(entry.Value))
                    {
                        continue;
                    }

                    changedDefaults.Add($"{entry.Key}: [{entry.Value}] -> [{mine ?? "none"}]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"       (defaults unreadable: {ex.Message})");
            }

            if (lost.Count == 0 && gained.Count == 0 && changedDefaults.Count == 0)
            {
                Console.WriteLine($"  OK {relative,-40} {candidateNames.Count} parameter(s)");
                continue;
            }

            Console.WriteLine($"  !! {relative,-40} {referenceNames.Count} -> {candidateNames.Count} parameter(s)");
            foreach (string change in changedDefaults)
            {
                Console.WriteLine($"       DEFAULT  {change}");
                missing++;
            }
            foreach (string name in lost)
            {
                Console.WriteLine($"       MISSING  {name}");
                missing++;
            }
            foreach (string name in gained)
            {
                Console.WriteLine($"       extra    {name}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{compared} effect(s) compared, {missing} missing parameter(s), " +
                          $"{absent} without a readable counterpart.");
        if (missing > 0)
        {
            Console.WriteLine();
            Console.WriteLine("A missing parameter is a NullReferenceException the first time the game sets it.");
            Console.WriteLine("Either keep the parameter alive in the .fx, or make the caller tolerate its absence.");
        }
        return missing == 0 ? 0 : 1;
    }

    /// <summary>
    /// Rewrites every compressed XNB under a directory as an uncompressed one, in place.
    ///
    /// See <see cref="XnbFile.WriteDecompressed"/> for why: FNA only understands XNA's LZX, and
    /// this game's content is MonoGame's LZ4. Files that are already uncompressed are left
    /// untouched, so this is idempotent and safe to re-run over a package.
    /// </summary>
    private static int Decompress(string dir)
    {
        int converted = 0, alreadyPlain = 0, failed = 0;
        long before = 0, after = 0;

        foreach (string path in XnbFiles(dir))
        {
            string relative = Path.GetRelativePath(dir, path);
            try
            {
                XnbFile xnb = XnbFile.Read(path);
                if (!xnb.WasCompressed)
                {
                    alreadyPlain++;
                    continue;
                }

                long was = new FileInfo(path).Length;
                xnb.WriteDecompressed(path);
                before += was;
                after += new FileInfo(path).Length;
                converted++;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  !! {relative}: {ex.GetType().Name}: {ex.Message}");
                failed++;
            }
        }

        Console.WriteLine($"{converted} decompressed, {alreadyPlain} already uncompressed, {failed} failed.");
        if (converted > 0)
        {
            Console.WriteLine($"  {before / 1024 / 1024} MB -> {after / 1024 / 1024} MB");
        }
        return failed == 0 ? 0 : 1;
    }

    private static int Scan(string dir)
    {
        int effects = 0, failures = 0;
        var versions = new SortedDictionary<int, int>();

        foreach (string path in XnbFiles(dir))
        {
            XnbFile xnb;
            try
            {
                xnb = XnbFile.Read(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  !! {Rel(dir, path)}: {ex.Message}");
                failures++;
                continue;
            }

            if (!xnb.PrimaryReaderName.StartsWith("Microsoft.Xna.Framework.Content.EffectReader", StringComparison.Ordinal))
                continue;

            effects++;
            byte[] blob = xnb.ReadEffectBlob();
            var (version, profile) = PeekMgfx(blob);
            versions[version] = versions.GetValueOrDefault(version) + 1;

            string profileName = profile switch { 0 => "OpenGL", 1 => "DirectX_11", _ => $"unknown({profile})" };
            Console.WriteLine($"  v{version}  {profileName,-12} {blob.Length,8} B  " +
                              $"{(xnb.WasCompressed ? "LZ4 " : "raw ")} {Rel(dir, path)}");
        }

        Console.WriteLine();
        Console.WriteLine($"{effects} effect(s) found; MGFX versions: " +
                          string.Join(", ", versions.Select(kv => $"v{kv.Key} x{kv.Value}")));
        if (failures > 0) Console.WriteLine($"{failures} file(s) could not be read.");
        return failures == 0 ? 0 : 1;
    }

    private static int Transcode(string[] args)
    {
        string dir = args[1];
        string? backupDir = null;
        bool dryRun = false;

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--backup" when i + 1 < args.Length:
                    backupDir = args[++i];
                    break;
                case "--dry-run":
                    dryRun = true;
                    break;
                default:
                    return Fail($"Unrecognised option '{args[i]}'.\n\n" + Usage);
            }
        }

        int rewritten = 0, skipped = 0, unsupported = 0, failures = 0;

        foreach (string path in XnbFiles(dir))
        {
            XnbFile xnb;
            try
            {
                xnb = XnbFile.Read(path);
            }
            catch
            {
                // Not our business: `scan` reports unreadable files, `transcode` only acts on
                // effects it can fully parse.
                continue;
            }

            if (!xnb.PrimaryReaderName.StartsWith("Microsoft.Xna.Framework.Content.EffectReader", StringComparison.Ordinal))
                continue;

            string rel = Rel(dir, path);
            try
            {
                byte[] source = xnb.ReadEffectBlob();
                var (version, _) = PeekMgfx(source);

                if (version == MgfxRewriter.TargetVersion)
                {
                    Console.WriteLine($"  == {rel} (already v{version})");
                    skipped++;
                    continue;
                }

                if (version != MgfxRewriter.SourceVersion)
                {
                    // Only v8 is understood. The game ships exactly one exception:
                    // skinFX_0.xnb is v7, which the game's OWN MonoGame 3.6 build rejected
                    // (its ReadHeader threw for any version below 8), and no model XNB or line
                    // of code references it. It is stale build output that was never loadable,
                    // so it is left exactly as shipped rather than guessed at.
                    Console.WriteLine($"  ?? {rel} (MGFX v{version} - not v{MgfxRewriter.SourceVersion}, left untouched)");
                    unsupported++;
                    continue;
                }

                byte[] target = MgfxRewriter.Rewrite(source, out int profile, out int effectKey);

                if (dryRun)
                {
                    Console.WriteLine($"  -> {rel}  v{version} -> v{MgfxRewriter.TargetVersion}  " +
                                      $"{source.Length} -> {target.Length} B  profile={profile} key=0x{effectKey:X8}  (dry run)");
                    rewritten++;
                    continue;
                }

                if (backupDir is not null)
                {
                    string dest = Path.Combine(backupDir, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                    File.Copy(path, dest, overwrite: true);
                }

                xnb.WriteWithEffectBlob(path, target);

                // Read the result back and confirm it is a v10 effect that parses.
                var check = XnbFile.Read(path);
                byte[] checkBlob = check.ReadEffectBlob();
                var (checkVersion, _) = PeekMgfx(checkBlob);
                if (checkVersion != MgfxRewriter.TargetVersion || checkBlob.Length != target.Length)
                    throw new InvalidDataException(
                        $"verification failed: wrote v{MgfxRewriter.TargetVersion}/{target.Length}B, " +
                        $"read back v{checkVersion}/{checkBlob.Length}B");

                Console.WriteLine($"  -> {rel}  v{version} -> v{MgfxRewriter.TargetVersion}  " +
                                  $"{source.Length} -> {target.Length} B  profile={profile}");
                rewritten++;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  !! {rel}: {ex.Message}");
                failures++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{rewritten} rewritten, {skipped} already current, " +
                          $"{unsupported} unsupported version, {failures} failed.");
        return failures == 0 ? 0 : 1;
    }

    private static int Raw(string input, string output)
    {
        byte[] source = File.ReadAllBytes(input);
        var (version, _) = PeekMgfx(source);
        byte[] target = MgfxRewriter.Rewrite(source, out int profile, out int effectKey);
        File.WriteAllBytes(output, target);
        Console.WriteLine($"{Path.GetFileName(input)}: v{version} -> v{MgfxRewriter.TargetVersion}  " +
                          $"{source.Length} -> {target.Length} B  profile={profile} key=0x{effectKey:X8}");
        return 0;
    }

    /// <summary>
    /// Wraps a bare MGFX blob from mgfxc in an XNB container by reusing an existing effect
    /// container's header. This avoids needing an XNB *writer*: the type-reader table, the
    /// shared-resource count and the primary type id are copied verbatim from a container the
    /// game already loads successfully, and only the length-prefixed effect payload changes.
    /// </summary>
    private static int Inject(string templateXnb, string mgfxo, string outXnb, string defaultsFromXnb)
    {
        XnbFile template = XnbFile.Read(templateXnb);

        // Reading the template's own blob first proves it really is an effect container
        // (ReadEffectBlob throws otherwise) before we reuse its header for something else.
        template.ReadEffectBlob();

        byte[] blob = File.ReadAllBytes(mgfxo);

        // Before anything else: repair GLSL that mgfxc emits and no driver will compile. This
        // cannot wait until a problem is noticed, because on the OpenGL profile a broken shader
        // is invisible until the draw call it belongs to silently does nothing. See GlslFixups.
        GlslFixups.Result fixups = GlslFixups.Apply(blob);
        blob = fixups.Blob;

        // And restore the parameter initial values mgfxc's OpenGL path zeroes. Several uniforms
        // in this game are declared with an initialiser and never assigned from code, so those
        // zeroes render the world as black silhouettes and every character fully transparent.
        // See MgfxDefaults.
        MgfxDefaults.Result defaults = default;
        if (defaultsFromXnb != null)
        {
            defaults = MgfxDefaults.Apply(blob, XnbFile.Read(defaultsFromXnb).ReadEffectBlob());
            blob = defaults.Blob;
        }

        MgfxV10Reader.Stats stats = MgfxV10Reader.Validate(blob);

        string? outDir = Path.GetDirectoryName(Path.GetFullPath(outXnb));
        if (!string.IsNullOrEmpty(outDir))
        {
            Directory.CreateDirectory(outDir);
        }
        template.WriteWithEffectBlob(outXnb, blob);

        // Read it straight back through the same path ContentManager will use.
        XnbFile check = XnbFile.Read(outXnb);
        MgfxV10Reader.Stats checkStats = MgfxV10Reader.Validate(check.ReadEffectBlob());

        string profileName = stats.Profile switch
        {
            0 => "OpenGL",
            1 => "DirectX_11",
            _ => "unknown(" + stats.Profile + ")",
        };
        Console.WriteLine(Path.GetFileName(outXnb) + ": " + profileName + " " + checkStats);
        if (fixups.ConditionsFixed > 0)
        {
            Console.WriteLine("  " + fixups);
        }
        if (defaults.Patched > 0 || (defaults.Skipped?.Count ?? 0) > 0)
        {
            Console.WriteLine("  " + defaults);
        }
        return 0;
    }

    /// <summary>
    /// Per-shader DXBC report, to answer whether an effect could be recompiled for MonoGame's
    /// OpenGL profile - which caps at Shader Model 3. The Aon9 chunk only tells you the shader
    /// was built at a *_4_0_level_9_x profile; its absence means plain 4_0 or higher, which is
    /// NOT the same as needing SM4 features. The STAT chunk's instruction/temp/flow-control
    /// counts are what actually decide whether ps_3_0 could hold it.
    /// </summary>
    private static int Shaders(string[] paths)
    {
        var files = new List<string>();
        foreach (string p in paths)
        {
            if (Directory.Exists(p)) files.AddRange(XnbFiles(p));
            else files.Add(p);
        }

        Console.WriteLine("ps_3_0/vs_3_0 limits: 512 instruction slots, 32 temp registers, 16 samplers.");
        Console.WriteLine();
        Console.WriteLine("{0,-26} {1,3} {2,-7} {3,-6} {4,5} {5,5} {6,4} {7,4} {8}",
            "EFFECT", "#", "MODEL", "Aon9", "INSTR", "TEMPS", "TEX", "SMP", "ps_3_0 VERDICT");

        int total = 0, fits = 0, risky = 0;

        foreach (string path in files)
        {
            byte[] blob;
            try
            {
                if (Path.GetExtension(path).Equals(".xnb", StringComparison.OrdinalIgnoreCase))
                {
                    var xnb = XnbFile.Read(path);
                    if (!xnb.PrimaryReaderName.StartsWith("Microsoft.Xna.Framework.Content.EffectReader", StringComparison.Ordinal))
                        continue;
                    blob = xnb.ReadEffectBlob();
                }
                else
                {
                    blob = File.ReadAllBytes(path);
                }
            }
            catch { continue; }

            List<MgfxV10Reader.ShaderBlock> blocks;
            try { blocks = MgfxV10Reader.ReadShaders(blob); }
            catch (Exception ex)
            {
                Console.WriteLine("{0,-26} -- unreadable: {1}", Path.GetFileNameWithoutExtension(path), ex.Message);
                continue;
            }

            string name = Path.GetFileNameWithoutExtension(path);
            for (int i = 0; i < blocks.Count; i++)
            {
                var b = blocks[i];
                var s = DxbcInfo.Read(b.Bytecode);
                total++;
                if (s.Ps30Verdict.StartsWith("fits", StringComparison.Ordinal)) fits++;
                else if (s.Ps30Verdict.StartsWith("RISK", StringComparison.Ordinal)) risky++;

                Console.WriteLine("{0,-26} {1,3} {2,-7} {3,-6} {4,5} {5,5} {6,4} {7,4} {8}",
                    i == 0 ? name : "", i, s.ShaderModel, s.HasAon9 ? "yes" : "NO",
                    s.InstructionCount, s.TempRegisterCount, s.TextureInstructions, b.Samplers,
                    s.Ps30Verdict);
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{total} shader(s): {fits} fit ps_3_0 limits, {risky} risky, " +
                          $"{total - fits - risky} unknown/too big.");
        Console.WriteLine();
        Console.WriteLine("NOTE: mgfxc strips DXBC reflection, so the shipped effects carry no STAT chunk and");
        Console.WriteLine("instruction counts are unrecoverable. A ps_4_0 record means that is what the studio");
        Console.WriteLine("compiled at, NOT that the shader needs SM4 - `compile ps_4_0` is simply what one writes");
        Console.WriteLine("for DX11. The only honest test is to recover the HLSL and ask fxc for ps_3_0.");
        return 0;
    }

    private static int Validate(string[] paths)
    {
        var files = new List<string>();
        foreach (string p in paths)
        {
            if (Directory.Exists(p)) files.AddRange(XnbFiles(p));
            else files.Add(p);
        }

        int ok = 0, bad = 0, notEffects = 0;

        foreach (string path in files)
        {
            byte[] blob;
            try
            {
                if (Path.GetExtension(path).Equals(".xnb", StringComparison.OrdinalIgnoreCase))
                {
                    var xnb = XnbFile.Read(path);
                    if (!xnb.PrimaryReaderName.StartsWith("Microsoft.Xna.Framework.Content.EffectReader", StringComparison.Ordinal))
                    {
                        notEffects++;
                        continue;
                    }
                    blob = xnb.ReadEffectBlob();
                }
                else
                {
                    blob = File.ReadAllBytes(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  !! {Path.GetFileName(path)}: unreadable: {ex.Message}");
                bad++;
                continue;
            }

            try
            {
                var stats = MgfxV10Reader.Validate(blob);
                Console.WriteLine($"  OK  {Path.GetFileName(path),-56} {stats}");
                ok++;
            }
            catch (Exception ex)
            {
                // stdout, not stderr: this is a report. The exit code signals failure, and
                // writing to stderr makes PowerShell 5.1 wrap each line in an ErrorRecord,
                // which makes a successful install look like it errored.
                Console.WriteLine($"  !! {Path.GetFileName(path),-56} {ex.Message}");
                bad++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{ok} valid MGFX v10, {bad} invalid" +
                          (notEffects > 0 ? $", {notEffects} non-effect xnb(s) ignored" : "") + ".");
        return bad == 0 ? 0 : 1;
    }

    private static string Rel(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');
}
