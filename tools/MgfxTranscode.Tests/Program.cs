using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using UW.Tools.MgfxTranscode;

if (args.Length != 2) throw new ArgumentException("Expected Vehicle.xnb and report directory.");
byte[] original = XnbFile.Read(args[0]).ReadEffectBlob();
var result = GlslFixups.Apply(original);
if (result.TransposesFixed != 3 || result.ShadersPatched != 3)
    throw new Exception("Expected exactly three affected Vehicle shaders.");
MgfxV10Reader.Validate(result.Blob);
if (!GlslFixups.Apply(result.Blob).Blob.SequenceEqual(result.Blob))
    throw new Exception("Repair is not idempotent.");
if (!MgfxV10Reader.ReadParameterNames(original).SequenceEqual(MgfxV10Reader.ReadParameterNames(result.Blob)))
    throw new Exception("Parameter names changed.");
var defaults = MgfxV10Reader.ReadParameterDefaults(original);
var repairedDefaults = MgfxV10Reader.ReadParameterDefaults(result.Blob);
if (defaults.Count != repairedDefaults.Count || defaults.Any(p => repairedDefaults[p.Key] != p.Value))
    throw new Exception("Parameter defaults changed.");
byte[] dx = (byte[])original.Clone(); dx[5] = 1;
if (!GlslFixups.Apply(dx).Blob.SequenceEqual(dx)) throw new Exception("DirectX modified.");
var before = MgfxV10Reader.ReadShaders(original);
var after = MgfxV10Reader.ReadShaders(result.Blob);
if (before.Count != after.Count) throw new Exception("Shader count changed.");
Directory.CreateDirectory(args[1]);
for (int i = 0; i < before.Count; i++)
{
    string source = Encoding.ASCII.GetString(before[i].Bytecode);
    string repaired = Encoding.ASCII.GetString(after[i].Bytecode);
    if (!source.Contains("transpose(" ) && !before[i].Bytecode.SequenceEqual(after[i].Bytecode))
        throw new Exception("Unrelated shader modified.");
    if (repaired.Contains("transpose(")) throw new Exception("Unsupported transpose remains.");
}
File.WriteAllText(Path.Combine(args[1], "vehicle-shaders.json"), JsonSerializer.Serialize(new {
    before = before.Select(b => new { vertex = b.IsVertexShader, source = Encoding.ASCII.GetString(b.Bytecode) }),
    after = after.Select(b => new { vertex = b.IsVertexShader, source = Encoding.ASCII.GetString(b.Bytecode) })
}));
Console.WriteLine("PASS: three shaders repaired; idempotence, parameters, defaults, DirectX and unrelated shaders preserved.");
