using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The sickness factors shared by the gameplay mods, compiled OUT. See PeatMod.Absent.cs for the
/// pattern. Nobody is sick: walking, work time and energy are the studio's.
/// </summary>
public static class SicknessEffects
{
    public static float SpeedFactor(Entity entity) => 1f;

    public static float WorkTimeFactor(Entity worker) => 1f;

    public static float CapEnergy(Entity entity, float energy) => energy;
}
