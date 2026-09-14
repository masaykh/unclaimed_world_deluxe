using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The healing mod, compiled OUT. Selected by building without the gameplay mods -
/// <c>-p:UwGameplayMods=false</c> - which is also what happens on its own when the repository's
/// mods/ directory is not there at all.
///
/// WHY A STUB RATHER THAN #if AT EVERY CALL SITE: see the header of UnhiddenMod.Absent.cs. The
/// short of it is that Body.RegainHitpoints reads better with two ordinary calls in it than with
/// two preprocessor blocks, and the patch series in kit/ stays smaller.
///
/// EVERY MEMBER HERE RETURNS THE STUDIO'S OWN ANSWER, not a neutral-looking one. That is the
/// whole contract of an Absent stub and it is easy to get subtly wrong: <see cref="RateFactor"/>
/// returning 0 would stop healing altogether rather than leave it alone. The values below are the
/// expressions that stood in the studio's source before the mod replaced them.
/// </summary>
public static class HealingMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "healing";

    /// <summary>Registers nothing - a build without the mod should not carry its switches.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>
    /// The studio's ceiling, unchanged. Their line computed it and used it; the mod's version may
    /// raise it to 1. Handing back what was passed in is what "the mod is not installed" means.
    /// </summary>
    public static float RecoveryCeiling(float studioCeiling)
    {
        return studioCeiling;
    }

    /// <summary>
    /// 1, because the studio's rate had no factor in it - the mod introduced one. Anything else
    /// here would change healing speed in a build that is supposed to have no healing mod.
    /// </summary>
    public static float RateFactor(Entity entity)
    {
        return 1f;
    }
}
