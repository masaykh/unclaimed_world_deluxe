using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Locomotors;

namespace UWGame.SimSide.Vehicles;

/// <summary>
/// BASE: an aircraft whose pilot dies in the air falls, instead of hovering where he died forever
/// (issue #26). Only the pilot's flight goals (GoalTakeoff, GoalFlyToPosition, GoalLand) ever moved
/// an aircraft, and his goals end with him.
///
/// This is the second half of Lars's plan for #26: "another locomotor could be set active on the
/// now pilotless aircraft" - the BallisticLocomotor the arrows use, so the fall is saved and loaded
/// with the Locomotor it belongs to and needs no goal. The first half, moving the flight itself out
/// of the goals into a locomotor, is not done here: those goals are being animated.
///
/// The numbers below are first guesses, named to be tuned.
/// </summary>
public static class AircraftCrash
{
    /// <summary>Gravity for a falling aircraft, px/s². Arrows use 10; from cruise height that is a five-second fall.</summary>
    public const double FallGravity = 60.0;

    /// <summary>Condition taken from each part of the aircraft at <see cref="ReferenceImpactSpeed"/>.</summary>
    public const float AircraftDamageAtReferenceSpeed = 0.5f;

    /// <summary>
    /// Each living occupant takes this fraction of one random body part's full hit points, at
    /// <see cref="ReferenceImpactSpeed"/>. A vital part brought low enough collapses him the usual way.
    /// </summary>
    public const float InjuryFractionAtReferenceSpeed = 0.6f;

    /// <summary>The speed of a fall from about 120 px at <see cref="FallGravity"/>; damage scales linearly with it, capped at twice.</summary>
    public const float ReferenceImpactSpeed = 120f;

    /// <summary>
    /// The aircraft <paramref name="dying"/> is flying, if he is its pilot and it is in the air;
    /// asked before Entity.Kill swaps his carcass into the seat.
    /// </summary>
    public static Entity AircraftLosingItsPilot(Entity dying)
    {
        if (!dying.DrivingVehicle.HasValue)
        {
            return null;
        }
        Entity aircraft = Entity.FindByID(dying.DrivingVehicle.Value);
        if (aircraft == null || !GoalExitVehicle.IsAirborne(aircraft))
        {
            return null;
        }
        return aircraft;
    }

    /// <summary>Starts the fall, carrying on at the speed and heading it was flying.</summary>
    public static void Start(Entity aircraft)
    {
        Locomotor locomotor = aircraft.Locomotor;
        if (locomotor == null)
        {
            return;
        }
        if (locomotor.BallisticLocomotor == null)
        {
            // The skimmers' LocomotorType has no BallisticLocomotorType; Locomotor saves whichever it holds.
            locomotor.BallisticLocomotor = new BallisticLocomotor(locomotor);
        }
        Vector3 velocity = new Vector3(aircraft.FacingNormal.X, aircraft.FacingNormal.Y, 0f) * locomotor.MoveSpeed;
        locomotor.MoveSpeed = 0f;
        if (aircraft.Vehicle?.Aircraft != null)
        {
            aircraft.Vehicle.Aircraft.VerticalSpeed = 0f;
        }
        locomotor.BallisticLocomotor.Drop(velocity, FallGravity);
    }

    /// <summary>How hard a fall at <paramref name="impactSpeed"/> hits, 0 to 2.</summary>
    public static float Severity(float impactSpeed)
    {
        return MathHelper.Clamp(impactSpeed / ReferenceImpactSpeed, 0f, 2f);
    }

    /// <summary>The aircraft has hit the ground at <paramref name="impactSpeed"/> px/s.</summary>
    public static void Impact(Entity aircraft, float impactSpeed)
    {
        float severity = Severity(impactSpeed);
        if (severity <= 0f)
        {
            return;
        }
        // The occupants first: damaging the aircraft can destroy it, and that ejects them.
        List<Entity> occupants = new List<Entity>();
        aircraft.Contains?.IterateContained(e => occupants.Add(e));
        foreach (Entity occupant in occupants)
        {
            if (occupant.IsDead || occupant.Body == null)
            {
                continue;
            }
            BodyPart part = occupant.Body.GetRandomBodyPartToHit(BodyPart.AttackDirection.Front);
            if (part != null)
            {
                part.DoDamage(InjuryFractionAtReferenceSpeed * severity * part.MaxHitpoints);
            }
        }
        aircraft.DoDamage(AircraftDamageAtReferenceSpeed * severity);
    }
}
