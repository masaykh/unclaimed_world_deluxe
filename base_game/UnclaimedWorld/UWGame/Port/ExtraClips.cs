using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Xclna.Xna.Animation;

namespace UWGame.Port;

/// <summary>
/// PORT: animation clips added to a model the studio built, without rebuilding it.
///
/// WHY. The game keeps every colonist animation inside Models/man_idle and Models/woman_idle -
/// the studio's content project merged ~100 man_*.fbx clips into each - and those cannot be
/// rebuilt exactly: the sources are FBX 6.1. A new one (pushing a cart) is made on the colonist
/// rig instead (docs/modding.md, "New colonist animations") and built on its own by
/// tools/build/38-build-models.sh into Models/clips/man_&lt;clip&gt;.xnb.
///
/// HOW. When UwContentManager loads a model named in <see cref="ClipPrefixes"/>, every
/// Models/clips/&lt;prefix&gt;*.xnb (in port-content\ or Content\) is loaded and its clips added
/// to the model's AnimationInfoCollection under their own names (man_pushCart gives "pushCart").
/// The woman gets the man's clips, as the studio's project gave her: they share the skeleton.
///   - a clip whose name the model already has is not added: the studio's wins;
///   - only channels of bones the model has are kept;
///   - a bone that every studio clip animating it holds at one and the same key keeps that key.
///     These are the hand attachments; AttachorPalmRight's key is mirrored (scale 1, -1, 1), and
///     Blender's FBX cannot carry a mirror, so a clip made there would turn the held item inside out.
/// Running again on the cached model adds nothing, so it is safe on every Load.
/// </summary>
public static class ExtraClips
{
    /// <summary>The folder clips are built into, under Models.</summary>
    public const string Folder = "Models/clips";

    /// <summary>Which models take which clips: asset name -> clip file prefix.</summary>
    public static readonly IReadOnlyDictionary<string, string> ClipPrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Models/man_idle"] = "man_",
        ["Models/woman_idle"] = "man_",
    };

    /// <summary>
    /// The clip assets for <paramref name="modelAsset"/>, by name, from the folders given;
    /// empty for a model not in <see cref="ClipPrefixes"/>.
    /// </summary>
    public static IReadOnlyList<string> ClipAssetsFor(string modelAsset, params string[] roots)
    {
        if (modelAsset == null || !ClipPrefixes.TryGetValue(modelAsset.Replace('\\', '/'), out string prefix))
        {
            return Array.Empty<string>();
        }
        var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string root in roots)
        {
            string dir = root == null ? null : Path.Combine(root, Folder);
            if (dir == null || !Directory.Exists(dir))
            {
                continue;
            }
            foreach (string file in Directory.GetFiles(dir, prefix + "*.xnb"))
            {
                names.Add(Folder + "/" + Path.GetFileNameWithoutExtension(file));
            }
        }
        return names.ToList();
    }

    /// <summary>Adds the clips of <paramref name="source"/> to <paramref name="target"/>; returns the names added.</summary>
    public static IReadOnlyList<string> Merge(Model target, Model source)
    {
        var added = new List<string>();
        AnimationInfoCollection to = AnimationInfoCollection.FromModel(target);
        AnimationInfoCollection from = AnimationInfoCollection.FromModel(source);
        if (to.Count == 0 || from.Count == 0)
        {
            return added;
        }
        var bones = new HashSet<string>(target.Bones.Select(b => b.Name));
        Dictionary<string, BoneKeyFrame> held = HeldKeys(to);
        foreach (KeyValuePair<string, AnimationInfo> clip in from)
        {
            if (to.ContainsKey(clip.Key))
            {
                continue;
            }
            uint duration = clip.Value.Duration;
            var channels = new List<BoneKeyFrameCollection>();
            foreach (BoneKeyFrameCollection channel in clip.Value.AnimationChannels)
            {
                if (!bones.Contains(channel.BoneName))
                {
                    continue;
                }
                if (held.TryGetValue(channel.BoneName, out BoneKeyFrame key))
                {
                    channels.Add(new BoneKeyFrameCollection(channel.BoneName, new[]
                    {
                        new BoneKeyFrame(key.Rotation, key.Translation, key.Scaling, 0),
                        new BoneKeyFrame(key.Rotation, key.Translation, key.Scaling, duration),
                    }));
                }
                else
                {
                    channels.Add(channel);
                }
            }
            var info = new AnimationInfo(clip.Key, new AnimationChannelCollection(channels))
            {
                ActionPointInSeconds = clip.Value.ActionPointInSeconds,
                StartOffset = clip.Value.StartOffset,
                HideHandAttachments = clip.Value.HideHandAttachments,
                IsGaitAnim = clip.Value.IsGaitAnim,
                OKToBlendAnimation = clip.Value.OKToBlendAnimation,
            };
            to.Add(clip.Key, info);
            added.Add(clip.Key);
        }
        return added;
    }

    /// <summary>
    /// Bones that every clip of <paramref name="clips"/> which animates them holds at one and the
    /// same key, with that key. (In man_idle: the two hand attachments - 95 of 104 clips hold each
    /// at one key, and the other 9 do not key it at all.)
    /// </summary>
    public static Dictionary<string, BoneKeyFrame> HeldKeys(AnimationInfoCollection clips)
    {
        var held = new Dictionary<string, BoneKeyFrame>();
        var varies = new HashSet<string>();
        foreach (AnimationInfo clip in clips.Values)
        {
            foreach (BoneKeyFrameCollection channel in clip.AnimationChannels)
            {
                string bone = channel.BoneName;
                if (varies.Contains(bone))
                {
                    continue;
                }
                foreach (BoneKeyFrame k in channel)
                {
                    if (!held.TryGetValue(bone, out BoneKeyFrame first))
                    {
                        held[bone] = k;
                    }
                    else if (!Same(first, k))
                    {
                        varies.Add(bone);
                        held.Remove(bone);
                        break;
                    }
                }
            }
        }
        return held;
    }

    private static bool Same(BoneKeyFrame a, BoneKeyFrame b) =>
        Close(a.Rotation.X, b.Rotation.X) && Close(a.Rotation.Y, b.Rotation.Y) && Close(a.Rotation.Z, b.Rotation.Z) && Close(a.Rotation.W, b.Rotation.W)
        && Close(a.Translation.X, b.Translation.X) && Close(a.Translation.Y, b.Translation.Y) && Close(a.Translation.Z, b.Translation.Z)
        && Close(a.Scaling.X, b.Scaling.X) && Close(a.Scaling.Y, b.Scaling.Y) && Close(a.Scaling.Z, b.Scaling.Z);

    private static bool Close(float a, float b) => Math.Abs(a - b) < 1e-4f;
}
