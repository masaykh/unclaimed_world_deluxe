using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UW.Tools.DataExport;

/// <summary>
/// Running a loaded game with no window, for --save-stuck. The simulation reaches into the
/// client in a few hundred places - renderables above all - and none of that decides where
/// anybody walks. Each stand-in below is the least that lets the simulation run, and each was
/// added for the exception it stops, not in advance.
/// </summary>
internal static partial class Program
{
    /// <summary>Methods that run as nothing: client feedback and cosmetics the simulation calls.</summary>
    private static readonly string[] HeadlessSkips =
    {
        "UWGame.SimSide.Resources.TileResourceContainer.CreateRenderable",
        "UWGame.SimSide.Sim.RecreateClientAfterInGameSave",
        "UWGame.SimSide.Entities.Intelligence.HandleTurnToInterest",
        "UWGame.SimSide.Entities.Intelligence.HandleNoInterest",
        "UWGame.SimSide.Entities.Intelligence.UpdateHeadTurnAngles",
        "UWGame.ClientSide.Interface.Inventory.InventorySettings.HasResources",
        "UWGame.SimSide.Allegiances.Statistics.ProductionStatistics.AddEvent", // ends in CheckAchievements, which reaches Steam
    };

    /// <summary>
    /// Methods whose exception is counted and dropped. MemoryFact.LoadPostProcess resolves its
    /// references first and builds its renderable last; Entity.Update is where a goal that reads
    /// a renderable's animation state throws, and one creature's throw would otherwise end the
    /// update for every creature after it.
    /// </summary>
    private static readonly string[] HeadlessSwallows =
    {
        "UWGame.SimSide.AI.MemoryFact.LoadPostProcess",
        "UWGame.SimSide.Entities.Entity.Update",
    };

    private static readonly Dictionary<string, int> swallowed = new();

    private static void PatchForHeadlessSim()
    {
        var harmony = new Harmony("uw.dataexport.headless");
        var skip = new HarmonyMethod(typeof(Program).GetMethod(nameof(SkipReturnDefault), AnyStatic));
        var swallow = new HarmonyMethod(typeof(Program).GetMethod(nameof(SwallowException), AnyStatic));

        // Every renderable is an inert stand-in, and everything the Client declares does nothing.
        foreach (MethodInfo m in typeof(RenderableFactory).GetMethods(AnyStatic | BindingFlags.DeclaredOnly).Where(m => m.Name == "Produce"))
        {
            harmony.Patch(m, prefix: new HarmonyMethod(typeof(Program).GetMethod(nameof(ProduceInertRenderable), AnyStatic)));
        }
        foreach (Type t in new[] { typeof(Renderable), typeof(UWGame.ClientSide.Client) })
        {
            foreach (MethodInfo m in t.GetMethods(AnyInstance | AnyStatic | BindingFlags.DeclaredOnly))
            {
                if (m.IsAbstract || m.ContainsGenericParameters || m.GetMethodBody() == null || (t != typeof(Renderable) && m.IsSpecialName)) continue;
                harmony.Patch(m, prefix: skip);
            }
        }
        foreach (string name in HeadlessSkips) PatchByName(harmony, name, prefix: skip);
        foreach (string name in HeadlessSwallows) PatchByName(harmony, name, finalizer: swallow);

        // The statistics end in CheckAchievements, which reaches Steam; the rest of the update is kept.
        harmony.Patch(typeof(Allegiance).GetMethod(nameof(Allegiance.UpdatePlaySite)),
            prefix: new HarmonyMethod(typeof(Program).GetMethod(nameof(UpdatePlaySiteWithoutAchievements), AnyStatic)));
    }

    private static void PatchByName(Harmony harmony, string fullName, HarmonyMethod prefix = null, HarmonyMethod finalizer = null)
    {
        int dot = fullName.LastIndexOf('.');
        Type t = typeof(Sim).Assembly.GetType(fullName.Substring(0, dot), throwOnError: true);
        foreach (MethodInfo m in t.GetMethods(AnyInstance | AnyStatic | BindingFlags.DeclaredOnly).Where(m => m.Name == fullName.Substring(dot + 1)))
        {
            harmony.Patch(m, prefix: prefix, finalizer: finalizer);
        }
    }

    private static bool SkipReturnDefault() => false;

    private static bool ProduceInertRenderable(ref Renderable __result)
    {
        __result = (Renderable)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        return false;
    }

    private static Exception SwallowException(Exception __exception, MethodBase __originalMethod)
    {
        if (__exception != null)
        {
            MethodBase thrower = new System.Diagnostics.StackTrace(__exception, false).GetFrame(0)?.GetMethod();
            string key = __originalMethod.DeclaringType?.Name + "." + __originalMethod.Name + ": "
                         + __exception.GetType().Name + " in " + thrower?.DeclaringType?.Name + "." + thrower?.Name;
            swallowed[key] = swallowed.TryGetValue(key, out int n) ? n + 1 : 1;
        }
        return null;
    }

    private static bool UpdatePlaySiteWithoutAchievements(Allegiance __instance, GameTime gameTime)
    {
        __instance.ThreatAndCombatJobManager.Update(gameTime);
        __instance.HumanActivities?.Update(gameTime);
        __instance.SharedKnowledge.Update(gameTime);
        return false;
    }

    /// <summary>Sim.Update's game-mode body, in its order, less the client's performance marks.</summary>
    private static void StepSim(Sim sim, double seconds, double elapsedPerUpdate, Action<double> everyTenSeconds)
    {
        object entities = FindField(typeof(Sim), "entities").GetValue(sim);
        MethodInfo updateEntities = entities.GetType().GetMethod("Update", new[] { typeof(GameTime) });
        PropertyInfo unpaused = typeof(Sim).GetProperty(nameof(Sim.TotalUnPausedGameTime));
        TimeSpan elapsed = TimeSpan.FromSeconds(elapsedPerUpdate);
        double nextReport = 0;
        for (double t = 0; t < seconds; t += elapsedPerUpdate)
        {
            if (t >= nextReport)
            {
                everyTenSeconds(t);
                nextReport += 10;
            }
            var gameTime = new GameTime(sim.GameTime.TotalGameTime + elapsed, elapsed);
            sim.GameTime = gameTime;
            unpaused.SetValue(sim, sim.TotalUnPausedGameTime + elapsed);
            Guard("DateAndTime", () => sim.DateAndTime.Update(gameTime));
            Guard("TriggerSystem", () => sim.TriggerSystem.Update(gameTime));
            Guard("World", () => sim.World.Update(gameTime));
            Guard("entities", () => updateEntities.Invoke(entities, new object[] { gameTime }));
            Guard("EntityGroups", () => LookUp<EntityGroup, EntityGroupID>.IterateMembers(g => g.Update(gameTime)));
            Guard("CycleManager", () => sim.CycleManager.Update());
            Guard("Map", () => The.Map.Update(gameTime));
        }
        everyTenSeconds(seconds);
    }

    private static void Guard(string what, Action step)
    {
        try
        {
            step();
        }
        catch (Exception ex)
        {
            Exception e = ex is TargetInvocationException { InnerException: not null } tie ? tie.InnerException : ex;
            string key = what + ": " + e.GetType().Name + " in " + e.TargetSite?.DeclaringType?.Name + "." + e.TargetSite?.Name;
            swallowed[key] = swallowed.TryGetValue(key, out int n) ? n + 1 : 1;
        }
    }
}
