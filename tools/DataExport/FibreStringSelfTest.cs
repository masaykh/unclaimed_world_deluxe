using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;

namespace UW.Tools.DataExport;

/// <summary>
/// --fibre-string-selftest[=off]: RawhideStringMod's PLANT FIBRE STRING.
///
/// WHAT. Kastuk, "Fishing net recipe", 2026-10-17: string from bark fibres "as new item. Can be
/// created from vines and also from elastic shadeleaf canes. Strings are used as lowest tier tool
/// under rawhide strings in processes." Against the real tables, after the game's own validation:
/// the item is the studio's rawhide string made weaker; both recipes are makeRawhideString's with
/// the documented inputs and time; every list of tool alternatives taking rawhide string takes the
/// new string beside it, less productively; nothing takes the strings by tag. Switched off, the
/// recipes and alternatives are gone and the ITEM stays, so a save holding one still loads.
///
/// NOT HERE. A colonist making one, or using it as a tool in a running colony.
/// </summary>
internal static partial class Program
{
    private static int FibreStringSelfTest(bool on)
    {
        UWGame.Mods.ModSetting setting = UWGame.Mods.ModSettings.Find("rawhidestring.plantFibreString");
        if (setting == null)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        setting.Value = on ? "true" : "false";
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        Console.WriteLine($"==> plant fibre string self-test (switch {(on ? "on" : "off")})");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }

        const string fibreKey = UWGame.Mods.RawhideStringMod.FibreStringKey;
        const string rawhideKey = UWGame.Mods.RawhideStringMod.RawhideStringKey;
        string[] recipes = { UWGame.Mods.RawhideStringMod.FibreFromVineKey, UWGame.Mods.RawhideStringMod.FibreFromCanesKey };
        var data = GameData.Instance;

        Check(setting.DefaultValue == "true" && setting.AffectsSimulation && setting.TakesEffectOnNextLoad && setting.IsStock != on,
              "on by default, simulation-affecting, from the next load, and stock only when off");
        Check(UWGame.Mods.ModSettings.Signature().Contains("rawhidestring.plantFibreString=true") == on,
              on ? "a save is stamped rawhidestring.plantFibreString=true" : "a save is not stamped");
        Check(data.AllProcessTypes.ContainsKey(UWGame.Mods.RawhideStringMod.FibreNetKey) == on,
              on ? "the net from plant fibre string is there" : "the net from plant fibre string goes with it: nothing makes the string");

        // The item, either way: a save holding one must load.
        Check(data.AllEntityTypes.TryGetValue(fibreKey, out EntityType fibre) && fibre.ToolType != null,
              fibreKey + " is a tool in the tables" + (on ? "" : ", although switched off (a save may hold one)"));
        EntityType rawhide = data.AllEntityTypes[rawhideKey];
        if (fibre?.ToolType != null)
        {
            Check(fibre.ToolType.ToolTag != null && rawhide.ToolType.ToolTag != null
                  && fibre.ToolType.ToolTag.OrderBy(t => t).SequenceEqual(rawhide.ToolType.ToolTag.OrderBy(t => t)),
                  "its tool tags are rawhide string's: " + string.Join(", ", fibre.ToolType.ToolTag ?? new string[0]));
            Check(fibre.ToolType.ToolHandling == rawhide.ToolType.ToolHandling && fibre.ToolType.ToolHandling == ToolHandlingType.HandTool,
                  "a hand tool, like rawhide string");
            Check(fibre.ToolType.Durability == UWGame.Mods.RawhideStringMod.FibreDurability
                  && fibre.ToolType.Durability < rawhide.ToolType.Durability,
                  FormattableString.Invariant($"durability {fibre.ToolType.Durability}, below rawhide string's {rawhide.ToolType.Durability}"));
            Check(fibre.ItemType?.MaximumBulk == rawhide.ItemType?.MaximumBulk && fibre.CategoryKey == rawhide.CategoryKey,
                  "the bulk and category of rawhide string");
            Check(fibre.RenderableType?.DefaultClientState?.RenderAsBillboardType?.FirstOrDefault()?.AssetName == "wire",
                  "drawn with the studio's string sprite (wire)");
        }

        // Nothing in the studio's tool sets takes a string by TAG: if one ever does, the new item
        // joins it at the tag's productivity, not below rawhide string's.
        var toolSets = data.AllProcessToolSets.Values.ToList();
        var byTag = toolSets.Where(s => s.Tools != null && s.Tools.Any(a => a.Tools.Any(t => t.UsesToolTag == "lightString" || t.UsesToolTag == "cordage")))
                            .Select(s => s.KeyName).ToList();
        Check(byTag.Count == 0, "no tool set takes the \"lightString\" or \"cordage\" tag" + (byTag.Count == 0 ? "" : ": " + string.Join(", ", byTag)));

        // Every list of alternatives naming rawhide string, and what it says of the new string.
        int lists = 0, paired = 0;
        var strays = new List<string>();
        foreach (ProcessToolSet set in toolSets)
        {
            foreach (ToolAlternatives alternatives in set.Tools ?? new ToolAlternatives[0])
            {
                Tool hide = alternatives.Tools.FirstOrDefault(t => t.UsesToolKeyName == rawhideKey);
                Tool fib = alternatives.Tools.FirstOrDefault(t => t.UsesToolKeyName == fibreKey);
                if (hide == null)
                {
                    if (fib != null) strays.Add(set.KeyName);
                    continue;
                }
                lists++;
                if (!on)
                {
                    if (fib != null) Check(false, set.KeyName + ": plant fibre string is there with the switch off");
                    continue;
                }
                bool ok = fib != null && hide.ProductivityFactor.HasValue && fib.ProductivityFactor.HasValue
                          && Math.Abs(fib.ProductivityFactor.Value - hide.ProductivityFactor.Value * UWGame.Mods.RawhideStringMod.FibreProductivityFactor) < 1e-6f
                          && fib.ProductivityFactor.Value < hide.ProductivityFactor.Value
                          && fib.DegradePerSecondOfUse == hide.DegradePerSecondOfUse;
                if (ok) paired++;
                Check(ok, FormattableString.Invariant($"{set.KeyName}: rawhide string {hide.ProductivityFactor}, plant fibre string {fib?.ProductivityFactor}"));
            }
        }
        Check(strays.Count == 0, "no plant fibre string in a list without rawhide string" + (strays.Count == 0 ? "" : ": " + string.Join(", ", strays)));
        Check(lists >= 12, $"{lists} lists of tool alternatives take rawhide string (the studio's tables have 12)");
        Check(!on || paired == lists, on ? $"every one of them takes plant fibre string, less productively ({paired} of {lists})"
                                         : "none of them takes plant fibre string with the switch off");

        if (!on)
        {
            foreach (string key in recipes) Check(!data.AllProcessTypes.ContainsKey(key), key + " is not in the tables");
            Check(!data.AllProcessTypes.Values.Any(p => p.Outputs != null && p.Outputs.Any(o => o.EntityTypeToCreate == fibreKey)),
                  "nothing makes plant fibre string");
            Console.WriteLine(failures == 0 ? "plant fibre string self-test OK" : $"plant fibre string self-test: {failures} FAILED");
            return failures == 0 ? 0 : 1;
        }

        // The two recipes: makeRawhideString with its rawhide swapped.
        ProcessType stock = data.AllProcessTypes["makeRawhideString"];
        (string recipe, string material, int count)[] expected =
        {
            (UWGame.Mods.RawhideStringMod.FibreFromVineKey, "item:vine", UWGame.Mods.RawhideStringMod.VinesPerString),
            (UWGame.Mods.RawhideStringMod.FibreFromCanesKey, "item:shadeleafCanes", UWGame.Mods.RawhideStringMod.CanesPerString)
        };
        Check(UWGame.Mods.RawhideStringMod.VinesPerString == 2 && UWGame.Mods.RawhideStringMod.CanesPerString == 1
              && UWGame.Mods.RawhideStringMod.FibreTimeFactor == 1.5f,
              "the documented numbers: 2 vines or 1 shadeleaf cane, 1.5 times rawhide string's time");
        foreach (var (key, material, count) in expected)
        {
            if (!data.AllProcessTypes.TryGetValue(key, out ProcessType p))
            {
                Check(false, key + " is in the tables");
                continue;
            }
            Check(p.Inputs.Length == 1 && p.Inputs[0].Entity == material && p.Inputs[0].Amount.NoOfItems == count && p.Inputs[0].IsConsumed == true,
                  $"{key}: consumes {count} {material}, and nothing else");
            Check(p.Outputs.Length == 1 && p.Outputs[0].FinalEntityTypeToCreate == fibre && p.Outputs[0].Amount.NoOfItems == 1,
                  $"{key}: makes 1 plant fibre string");
            Check(p.RequiredSkill == stock.RequiredSkill && p.ProcessToolSetKey == stock.ProcessToolSetKey
                  && p.PhysicalWorkFactor == stock.PhysicalWorkFactor && p.JobTypeKey == stock.JobTypeKey,
                  $"{key}: makeRawhideString's skill ({p.RequiredSkill}), tool set ({p.ProcessToolSetKey}), work and job");
            float? days = p.WorkOrTimeNeeded?.DaysNeeded, stockDays = stock.WorkOrTimeNeeded?.DaysNeeded;
            Check(days.HasValue && stockDays.HasValue && Math.Abs(days.Value - stockDays.Value * UWGame.Mods.RawhideStringMod.FibreTimeFactor) < 1e-7f,
                  FormattableString.Invariant($"{key}: {days} days, 1.5 times makeRawhideString's {stockDays}"));
            // What GameData.AddProcessToProductionGraph asks before it puts a recipe in
            // ProcessYieldsThisOutput and AllProductionProcesses, which InventorySettings computes
            // attainability from. That runs in GameData.Initialize, after the content loads.
            Check(p.IsPartOfProductionChain() && p.IsPartOfAttainableCalculation()
                  && stock.IsPartOfProductionChain() && stock.IsPartOfAttainableCalculation(),
                  $"{key}: in the production graph and the attainability calculation, as makeRawhideString is");
        }

        Console.WriteLine(failures == 0 ? "plant fibre string self-test OK" : $"plant fibre string self-test: {failures} FAILED");
        return failures == 0 ? 0 : 1;
    }
}
