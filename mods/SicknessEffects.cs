using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.SimEffects;

namespace UWGame.Mods;

/// <summary>
/// What a sickness does that the studio's effects can not: walking, work and energy. Shared by the
/// mods that make people ill - EmergencyEatingMod's "Bad stomach" and WeatherMod's "Cold".
///
/// THE STUDIO'S EFFECTS (Sim/SimEffects): an EffectProfileType names NumberEffectTypes, each
/// affecting one of AffectsNumbers - comfort among them, but not speed, work time or energy. So a
/// sickness is an always-registered effect profile (shown in the colonist's effect panel, saved
/// with the colonist, expiring by itself), whose comfort is its own effect, plus an entry here for
/// the rest, by the profile's key:
///   Locomotor.CalculateSpeed                  x <see cref="SpeedFactor"/>
///   ProcessJob.CalculateWorkDurationInDays    x <see cref="WorkTimeFactor"/>
///   BiologicalEntity.EnergyLevel              at most <see cref="CapEnergy"/>
/// A mod registers its profiles (<see cref="Register"/>) where it adds them to the tables, whether or
/// not it is switched on, so what the panel shows is what applies, until it expires.
///
/// Two at once multiply (walking and work) and the lower energy cap holds. People only: an animal
/// never has these profiles, and is not looked at.
/// </summary>
public static class SicknessEffects
{
    /// <summary>What one sickness does: walking and work-time factors and an energy cap.</summary>
    public readonly struct Factors
    {
        public readonly float Speed;
        public readonly float WorkTime;
        public readonly float EnergyCap;

        public Factors(float speed, float workTime, float energyCap)
        {
            Speed = speed;
            WorkTime = workTime;
            EnergyCap = energyCap;
        }
    }

    private static readonly Dictionary<string, Factors> byProfile = new(StringComparer.Ordinal);

    /// <summary>
    /// Called by a mod as it adds an effect profile to the tables: while a person has it, walking is
    /// x <paramref name="speedFactor"/>, work takes x <paramref name="workTimeFactor"/>, and energy is
    /// at most <paramref name="energyCap"/>.
    /// </summary>
    public static void Register(string profileKey, float speedFactor, float workTimeFactor, float energyCap) =>
        byProfile[profileKey] = new Factors(speedFactor, workTimeFactor, energyCap);

    /// <summary>
    /// The sicknesses a person has now, together: walking and work multiplied, the lowest energy cap
    /// (+infinity without one). 1, 1, +infinity for anyone without.
    /// </summary>
    public static Factors Of(Entity entity)
    {
        float speed = 1f, work = 1f, cap = float.PositiveInfinity;
        List<SimEffectProfile> active = entity?.EntityType?.Person == null ? null : entity.SimEffects?.EffectProfiles;
        if (active != null)
        {
            foreach (SimEffectProfile profile in active)
            {
                string key = profile?.EffectProfileType?.KeyName;
                if (key != null && byProfile.TryGetValue(key, out Factors f))
                {
                    speed *= f.Speed;
                    work *= f.WorkTime;
                    cap = Math.Min(cap, f.EnergyCap);
                }
            }
        }
        return new Factors(speed, work, cap);
    }

    /// <summary>Called from Locomotor.CalculateSpeed; 1 for anyone not sick.</summary>
    public static float SpeedFactor(Entity entity) => Of(entity).Speed;

    /// <summary>Called from ProcessJob.CalculateWorkDurationInDays; 1 for anyone not sick.</summary>
    public static float WorkTimeFactor(Entity worker) => Of(worker).WorkTime;

    /// <summary>Called from the BiologicalEntity.EnergyLevel getter: the energy, at most the cap of a sickness.</summary>
    public static float CapEnergy(Entity entity, float energy)
    {
        float cap = Of(entity).EnergyCap;
        return float.IsPositiveInfinity(cap) ? energy : Math.Min(energy, cap);
    }
}
