using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Trees;

namespace UW.Tools.DataExport;

/// <summary>
/// --editor-plants-selftest: the Map Editor's plants and asset lists, against the real tables.
///
/// WHAT. Kastuk, 2026-10-11, in #dev_chat_with_claude:
///   4. "All plants is placed in young state despite of selected Grown state." Shadeleaf,
///      wingweed, spoak, daysheen and water cane show their grown sprite only with HasBranches,
///      which a crop sets, and a plant placed in the editor had no crops - so it showed the cut
///      sprite at any age. MapLoader.SetDefaultCrops now gives it crops as a loaded map does. Here:
///      each such plant's grown sprite does need the flag, and its DefaultCrops can supply it.
///   5. "A slider of tree age (AgeInYears, range up to MaxAge ...)". Here: the slider's left end is
///      a random age, its right end the plant's MaxAge, and an age past MatureAge gives the grown
///      size (Tree.SetRandomSize) while one under half of it gives the young one.
///   1. "Sort assets in lists of Terrain and Plants by alphabet inside their subcategories (like
///      all deposits must stay nearby)." Here: SidePanelEditorEntity.EditorListOrder keeps the
///      deposits and the sounds together, puts rockwall2 before rockwall10, and "X - dead" after X.
///
/// NOT HERE. The placing itself (SidePanelEditorEntity.CreateThisEntityForPlacement) needs a
/// play site and a renderable, and the slider and the rebuild on a changed option are interface.
/// </summary>
internal static partial class Program
{
    private static int EditorPlantsSelfTest()
    {
        The.Sim = new Sim { Mode = Sim.EngineMode.Edit };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(The.Sim, null);
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        Console.WriteLine("==> Map Editor plants self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }

        CheckCropGatedSprites(Check);
        CheckAgeSlider(Check);
        CheckListOrder(Check);

        Console.WriteLine(failures == 0 ? "editor plants self-test OK" : $"editor plants self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    private static string GrownSprite(EntityType type, params StateModifier[] flags)
    {
        var mask = new BitMask64(typeof(StateModifier));
        mask.Set(StateModifier.Flavour1);
        foreach (StateModifier flag in flags)
        {
            mask.Set(flag);
        }
        RenderableType renderable = type.RenderableType;
        RenderableType.FindBestStaticInfo(mask, renderable.ClientStateConditions, renderable.DefaultClientState, out IStateInfo best);
        return (best as ClientStateInfo)?.RenderAsBillboardType?.FirstOrDefault()?.AssetName ?? "";
    }

    private static void CheckCropGatedSprites(Action<bool, string> check)
    {
        var gated = new List<string>();
        foreach (EntityType type in GameData.Instance.AllTreeTypes.Values)
        {
            if (type.TreeType.Crops == null || type.KeyName.Contains("dead"))
            {
                continue;
            }
            StateModifier[] flags = type.TreeType.Crops
                .Select(key => GameData.Instance.AllResourceTypes[key].CropType?.TreeSpriteFlag)
                .Where(f => f.HasValue).Select(f => f.Value).Distinct().ToArray();
            string bare = GrownSprite(type);
            string withCrops = GrownSprite(type, flags);
            if (bare == withCrops)
            {
                continue;
            }
            gated.Add(type.KeyName);
            bool suppliable = type.TreeType.DefaultCrops != null && type.TreeType.DefaultCrops.Any(d =>
                GameData.Instance.AllResourceTypes[d.KeyName].CropType?.TreeSpriteFlag is StateModifier f && flags.Contains(f)
                && d.MaxItemsForFullGrownPlant.GetValueOrDefault() > 0);
            check(bare.Contains("_cut") && !withCrops.Contains("_cut") && suppliable,
                $"{type.KeyName}: without crops {bare}, with them {withCrops}; its default crops can give the flag");
        }
        foreach (string key in new[] { "tree:shadeleaf", "tree:paleshadeleaf", "tree:daysheen", "tree:spoak", "tree:wingweed", "tree:desertWingweed", "tree:riveraxle" })
        {
            check(gated.Contains(key), $"{key} is one of them - the plants Kastuk named");
        }
    }

    private static void CheckAgeSlider(Action<bool, string> check)
    {
        check(SidePanelEditorEntity.AgeFromSlider(0) == null, "AGE at its left end is a random age");
        check(SidePanelEditorEntity.AgeFromSlider(35) == 3.5f, $"  35 tenths is 3.5 years (got {SidePanelEditorEntity.AgeFromSlider(35)?.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
        int wrong = 0;
        string firstWrong = null;
        foreach (EntityType type in GameData.Instance.AllTreeTypes.Values)
        {
            TreeType tree = type.TreeType;
            float oldest = SidePanelEditorEntity.AgeFromSlider((int)Math.Round(10f * tree.MaxAge)) ?? -1f;
            float young = 0.4f * tree.MatureAge;
            float grown = Math.Min(tree.MaxAge, tree.MatureAge + 0.1f);
            bool ok = Math.Abs(oldest - tree.MaxAge) < 0.05f
                      && SizeAt(type, young) < 0.9f
                      && SizeAt(type, grown) >= 0.9f
                      && SizeAt(type, oldest) >= 0.9f;
            if (!ok)
            {
                wrong++;
                firstWrong ??= $"{type.KeyName}: right end {oldest} of MaxAge {tree.MaxAge}, size {SizeAt(type, young)} at {young}, {SizeAt(type, grown)} at {grown}";
            }
        }
        check(wrong == 0, "every plant: the right end is its MaxAge, an age past MatureAge is grown, under half of it young"
                          + (wrong == 0 ? "" : $" - {wrong} not, e.g. {firstWrong}"));
    }

    /// <summary>The size Tree.Initialize gives a plant of this age (Tree.SetRandomSize).</summary>
    private static float SizeAt(EntityType type, float age)
    {
        // No play site and no random stream here, so neither the Entity nor the Tree is constructed.
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        typeof(Entity).GetProperty(nameof(Entity.EntityType))!.SetValue(entity, type);
        var tree = (Tree)RuntimeHelpers.GetUninitializedObject(typeof(Tree));
        tree.Parent = entity;
        tree.SetRandomSize(age);
        return tree.Size;
    }

    private static void CheckListOrder(Action<bool, string> check)
    {
        List<EntityType> terrain = SidePanelEditorEntity.EditorListOrder(
            GameData.Instance.AllTerrainFeatureTypes.Values.Where(t => t.TerrainType.CanBeMapEditorPlaced));
        List<string> names = terrain.Select(t => t.Name).ToList();
        bool Together(Func<EntityType, bool> member)
        {
            int first = terrain.FindIndex(t => member(t));
            int last = terrain.FindLastIndex(t => member(t));
            return first >= 0 && terrain.Skip(first).Take(last - first + 1).All(member);
        }
        check(Together(t => t.KeyName.EndsWith("Deposit")), "the deposits stay together: " + string.Join(", ", terrain.Where(t => t.KeyName.EndsWith("Deposit")).Select(t => t.Name)));
        check(Together(t => t.Name.StartsWith("S: ")), "  and the sounds");
        check(names.IndexOf("rockwall2") < names.IndexOf("rockwall10") || names.IndexOf("rockwall10") < 0, "  rockwall2 before rockwall10 - numbers by value");
        check(names.IndexOf("ovalrocks9") < names.IndexOf("ovalrocks10"), "  ovalrocks9 before ovalrocks10");
        bool sorted = true;
        for (int i = 1; i < terrain.Count; i++)
        {
            if (SidePanelEditorEntity.EditorListGroup(terrain[i - 1]) == SidePanelEditorEntity.EditorListGroup(terrain[i])
                && string.CompareOrdinal(terrain[i - 1].Name.ToLowerInvariant(), terrain[i].Name.ToLowerInvariant()) > 0
                && !(char.IsDigit(terrain[i - 1].Name.Last()) && char.IsDigit(terrain[i].Name.Last())))
            {
                sorted = false;
                Console.WriteLine($"        {terrain[i - 1].Name} before {terrain[i].Name}");
            }
        }
        check(sorted, "  inside each group, by name");

        List<string> plants = SidePanelEditorEntity.EditorListOrder(GameData.Instance.AllTreeTypes.Values).Select(t => t.Name).ToList();
        bool deadAfterLiving = true;
        foreach (string dead in plants.Where(n => n.Contains(" - dead")))
        {
            string living = dead.Substring(0, dead.IndexOf(" - dead", StringComparison.Ordinal));
            int at = plants.IndexOf(living);
            if (at >= 0 && plants.Skip(at + 1).TakeWhile(n => n != dead).Any(n => !n.StartsWith(living + " - ", StringComparison.Ordinal)))
            {
                deadAfterLiving = false;
                Console.WriteLine($"        {dead} is not right after {living}");
            }
        }
        check(string.Join("|", plants) == string.Join("|", plants.OrderBy(n => n.ToLowerInvariant(), StringComparer.Ordinal)), "plants by name: " + string.Join(", ", plants.Take(6)) + ", ...");
        check(deadAfterLiving, "  each dead plant right after its living one");
    }
}
