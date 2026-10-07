using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.Entities;

namespace UW.Tools.DataExport;

/// <summary>
/// --discomfort-selftest: DiscomfortMod against the real tables.
///
/// WHAT. Kastuk, "Discomfort", 2026-10-05: filthy items and busy workplaces within 5 tiles of
/// where a colonist sleeps cost 3% Comfort per 5 units, at most 40%, measured once a day. The
/// mod names its filth and its noise by category and by key, so a key that stops matching would
/// silently exempt it - each is checked against the loaded and validated tables. Then the
/// arithmetic, and the day stamp: a measurement holds today and tomorrow, and lapses after.
///
/// NOT HERE. DiscomfortMod.Measure walks the map's tiles, which a data tool has none of; the walk
/// itself is the BirdHopMod.Startler loop the game already runs.
/// </summary>
internal static partial class Program
{
    private static int DiscomfortSelfTest()
    {
        UWGame.The.Sim = new Sim { Mode = Sim.EngineMode.Game };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(UWGame.The.Sim, null);
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        Console.WriteLine("==> discomfort self-test");
        UWGame.Mods.DiscomfortMod.RegisterSettings();
        var enabled = UWGame.Mods.ModSettings.Find("discomfort.enabled");
        if (enabled == null)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        var types = GameData.Instance.AllEntityTypes;
        EntityType Type(string key) => types.TryGetValue(key, out var t) ? t : null;

        Check(enabled.DefaultValue == "false", "off by default");
        foreach (string key in new[] { "item:organicFertilizer", "item:guanoFertilizer", "item:guano", "item:sulfurBlocks", "item:sulfurPowder",
                                       "item:megapodGreenHide", "item:whipjawGreenHide", "item:thunderChickenGreenHide" })
        {
            Check(UWGame.Mods.DiscomfortMod.IsFilthy(Type(key)), key + " stinks");
        }
        EntityType waste = null, carcass = null;
        foreach (EntityType t in types.Values)
        {
            if (waste == null && t.ItemType != null && t.CategoryKey == "waste") waste = t;
            if (carcass == null && t.ItemType?.CarcassType != null) carcass = t;
        }
        Check(waste != null && UWGame.Mods.DiscomfortMod.IsFilthy(waste), $"waste stinks ({waste?.KeyName})");
        Check(carcass != null && UWGame.Mods.DiscomfortMod.IsFilthy(carcass), $"a carcass stinks ({carcass?.KeyName})");
        Check(!UWGame.Mods.DiscomfortMod.IsFilthy(Type("item:clamwichSoup")) && !UWGame.Mods.DiscomfortMod.IsFilthy(Type("item:firewood")),
              "clamwich soup and firewood do not");

        foreach (string key in new[] { "structure:improvisedWorkbench", "structure:mudBrickWorkbench", "structure:improvisedSmithy",
                                       "structure:simpleSmithy", "structure:workshopBuilding", "structure:rareMetalRefinery", "structure:saltMine" })
        {
            Check(UWGame.Mods.DiscomfortMod.IsNoisy(Type(key)), key + " is loud while worked at");
        }
        Check(Type("structure:landMine") != null && !UWGame.Mods.DiscomfortMod.IsNoisy(Type("structure:landMine")), "a land mine is not");
        Check(!UWGame.Mods.DiscomfortMod.IsNoisy(Type("structure:compostPit")) && !UWGame.Mods.DiscomfortMod.IsNoisy(Type("item:handDrill")),
              "a compost pit and a hand drill are not");

        // 3% per whole 5 units, at most 40%; a worker at a loud workplace is 5 units.
        static bool Near(float a, float b) => Math.Abs(a - b) < 1e-6f;
        Check(Near(UWGame.Mods.DiscomfortMod.PenaltyFor(4, 0, 3, 40), 0f), "4 filthy items: nothing yet");
        Check(Near(UWGame.Mods.DiscomfortMod.PenaltyFor(5, 0, 3, 40), 0.03f) && Near(UWGame.Mods.DiscomfortMod.PenaltyFor(12, 0, 3, 40), 0.06f),
              "5 items: -3%, 12 items: -6%");
        Check(Near(UWGame.Mods.DiscomfortMod.PenaltyFor(5, UWGame.Mods.DiscomfortMod.UnitsPerWorker, 3, 40), 0.06f),
              "5 items and one busy workbench: -6%");
        Check(Near(UWGame.Mods.DiscomfortMod.PenaltyFor(1000, 50, 3, 40), 0.40f), "a midden: capped at -40%");
        Check(Near(UWGame.Mods.DiscomfortMod.PenaltyFor(10, 0, 10, 20), 0.20f) && Near(UWGame.Mods.DiscomfortMod.PenaltyFor(5, 0, 10, 20), 0.10f),
              "the test settings move it: 10% per 5, capped at 20%");

        // The day stamp, as OnSleepStart leaves it on a colonist.
        UWGame.The.Sim.DateAndTime = new DateAndTime();
        var colonist = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        int today = UWGame.The.Sim.DateAndTime.Year * 1000 + UWGame.The.Sim.DateAndTime.Day;
        Entity.SetPropertyValue(ref colonist.CustomFields, "discomfort:day", new UWGame.ClientSide.PropertyPresentation.PropertyResult { NumberResult = today });
        Entity.SetPropertyValue(ref colonist.CustomFields, "discomfort:penalty", new UWGame.ClientSide.PropertyPresentation.PropertyResult { NumberResult = 0.09f });
        enabled.Value = "false";
        Check(UWGame.Mods.DiscomfortMod.Penalty(colonist) == 0f, "switch off: no comfort lost, whatever was measured");
        enabled.Value = "true";
        Check(Near(UWGame.Mods.DiscomfortMod.Penalty(colonist), 0.09f), "measured today: -9% holds");
        UWGame.The.Sim.DateAndTime.Day++;
        Check(Near(UWGame.Mods.DiscomfortMod.Penalty(colonist), 0.09f), "  and tomorrow, until the next sleep measures again");
        UWGame.The.Sim.DateAndTime.Day++;
        Check(UWGame.Mods.DiscomfortMod.Penalty(colonist) == 0f, "  and lapses after a day with no sleep");
        var unmeasured = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        Check(UWGame.Mods.DiscomfortMod.Penalty(unmeasured) == 0f, "a colonist never measured loses nothing");
        enabled.Value = enabled.DefaultValue;

        Console.WriteLine(failures == 0 ? "discomfort self-test OK" : $"discomfort self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }
}
