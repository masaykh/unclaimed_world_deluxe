using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xclna.Xna.Animation;

namespace UW.Tools.ContentProbe;

/// <summary>
/// --export-rig &lt;dir&gt; [--clips a,b,...]: a skinned model as the game holds it in memory -
/// bones with their rest transforms, the skin, the skinned mesh, and the named animation clips -
/// written to &lt;dir&gt;/&lt;model&gt;.rig.json in raw game units.
///
/// WHY. A new colonist animation (pushing a cart) has to be made on the skeleton the game plays
/// clips on, and Blender cannot open the studio's FBX 6.1 sources. tools/ModelPipeline/make_rig.py
/// rebuilds this as a Blender armature with a skinned mesh, and puts the clips on it as actions -
/// which also makes the round trip testable: a studio clip taken through Blender and back through
/// tools/build/38-build-models.sh must come out as the keyframes written here.
///
/// Matrices are XNA's, row-major, row vectors (a point is transformed as p * M); a keyframe is
/// [time in ticks, rotation x y z w, translation x y z, scale x y z], local to the parent bone, as
/// BoneKeyFrame.ComputeMatrix composes it: scale, then rotation, then translation.
/// </summary>
internal static class RigExport
{
    public static string Write(Model m, string asset, string dir, IReadOnlyCollection<string>? clipNames)
    {
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, Path.GetFileName(asset) + ".rig.json");
        var tag = m.Tag as Dictionary<string, object>;

        var bones = m.Bones.Select(b => new Dictionary<string, object?>
        {
            ["name"] = b.Name,
            ["parent"] = b.Parent?.Name,
            ["local"] = Floats(b.Transform),
        }).ToList();

        var skins = new List<object>();
        if (tag != null && tag.TryGetValue("SkinInfo", out object? skinObj) && skinObj is SkinInfoCollection[] perMesh)
        {
            foreach (SkinInfoCollection skin in perMesh)
            {
                skins.Add(skin.Select(s => new Dictionary<string, object>
                {
                    ["palette"] = s.PaletteIndex,
                    ["bone"] = s.BoneName,
                    ["inverseBindPose"] = Floats(s.InverseBindPoseTransform),
                }).ToList());
            }
        }

        var meshes = new List<object>();
        foreach (ModelMesh mesh in m.Meshes)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                VertexDeclaration decl = part.VertexBuffer.VertexDeclaration;
                int stride = decl.VertexStride;
                var bytes = new byte[part.NumVertices * stride];
                part.VertexBuffer.GetData(part.VertexOffset * stride, bytes, 0, bytes.Length, 1);
                VertexElement[] elements = decl.GetVertexElements();
                int Offset(VertexElementUsage usage) => elements.Where(e => e.VertexElementUsage == usage && e.UsageIndex == 0).Select(e => e.Offset).DefaultIfEmpty(-1).First();
                VertexElementFormat FormatOf(VertexElementUsage usage) => elements.First(e => e.VertexElementUsage == usage && e.UsageIndex == 0).VertexElementFormat;
                int pos = Offset(VertexElementUsage.Position), nrm = Offset(VertexElementUsage.Normal), uv = Offset(VertexElementUsage.TextureCoordinate);
                int bi = Offset(VertexElementUsage.BlendIndices), bw = Offset(VertexElementUsage.BlendWeight);
                var positions = new List<float>(); var normals = new List<float>(); var uvs = new List<float>();
                var indices = new List<int>(); var weights = new List<float>();
                for (int v = 0; v < part.NumVertices; v++)
                {
                    int o = v * stride;
                    for (int k = 0; k < 3; k++) positions.Add(BitConverter.ToSingle(bytes, o + pos + 4 * k));
                    if (nrm >= 0) for (int k = 0; k < 3; k++) normals.Add(BitConverter.ToSingle(bytes, o + nrm + 4 * k));
                    if (uv >= 0) for (int k = 0; k < 2; k++) uvs.Add(BitConverter.ToSingle(bytes, o + uv + 4 * k));
                    if (bi >= 0) for (int k = 0; k < 4; k++) indices.Add(bytes[o + bi + k]);
                    if (bw >= 0)
                    {
                        if (FormatOf(VertexElementUsage.BlendWeight) == VertexElementFormat.Vector4)
                            for (int k = 0; k < 4; k++) weights.Add(BitConverter.ToSingle(bytes, o + bw + 4 * k));
                        else
                            for (int k = 0; k < 4; k++) weights.Add(bytes[o + bw + k] / 255f);
                    }
                }
                int count = part.PrimitiveCount * 3;
                var tris = new int[count];
                if (part.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
                {
                    var s = new ushort[count];
                    part.IndexBuffer.GetData(part.StartIndex * 2, s, 0, count);
                    for (int i = 0; i < count; i++) tris[i] = s[i];
                }
                else
                {
                    part.IndexBuffer.GetData(part.StartIndex * 4, tris, 0, count);
                }
                meshes.Add(new Dictionary<string, object?>
                {
                    ["name"] = mesh.Name,
                    ["parentBone"] = mesh.ParentBone?.Name,
                    ["positions"] = positions, ["normals"] = normals, ["uvs"] = uvs,
                    ["blendIndices"] = indices, ["blendWeights"] = weights,
                    ["triangles"] = tris,
                });
            }
        }

        var clips = new Dictionary<string, object>();
        AnimationInfoCollection all = AnimationInfoCollection.FromModel(m);
        foreach (KeyValuePair<string, AnimationInfo> kv in all)
        {
            if (clipNames != null && !clipNames.Contains(kv.Key)) continue;
            clips[kv.Key] = new Dictionary<string, object>
            {
                ["durationTicks"] = kv.Value.Duration,
                ["startOffsetTicks"] = kv.Value.StartOffset,
                ["actionPointSeconds"] = kv.Value.ActionPointInSeconds,
                ["hideHandAttachments"] = kv.Value.HideHandAttachments,
                ["isGait"] = kv.Value.IsGaitAnim,
                ["channels"] = kv.Value.AnimationChannels.ToDictionary(
                    c => c.BoneName,
                    c => c.Select(k => new float[]
                    {
                        k.Time, k.Rotation.X, k.Rotation.Y, k.Rotation.Z, k.Rotation.W,
                        k.Translation.X, k.Translation.Y, k.Translation.Z, k.Scaling.X, k.Scaling.Y, k.Scaling.Z,
                    }).ToList()),
            };
        }

        var doc = new Dictionary<string, object>
        {
            ["asset"] = asset,
            ["units"] = "game units; matrices XNA row-major, row vectors",
            ["bones"] = bones,
            ["skins"] = skins,
            ["meshes"] = meshes,
            ["clips"] = clips,
            ["allClipNames"] = all.Keys.ToList(),
        };
        File.WriteAllText(path, JsonSerializer.Serialize(doc));
        return path + $" ({bones.Count} bones, {meshes.Count} mesh parts, {clips.Count} of {all.Count} clips)";
    }

    private static float[] Floats(Matrix x) => new[]
    {
        x.M11, x.M12, x.M13, x.M14, x.M21, x.M22, x.M23, x.M24,
        x.M31, x.M32, x.M33, x.M34, x.M41, x.M42, x.M43, x.M44,
    };
}
