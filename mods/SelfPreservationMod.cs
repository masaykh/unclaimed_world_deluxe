using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Jobs;

namespace UWGame.Mods;

/// <summary>
/// Colonists stop walking into fights nobody sent them to - by being reluctant to take the job,
/// not by being forbidden from it.
///
/// THE REPORT. "Non-combatant settlers sometimes is walking towards aggressive fauna and attack
/// them while being alone, then taking damage in fight and sometimes dying foolishly. Even if
/// their Stance is Vigilant or lower (expected to let them stay out of danger)." - Kastuk.
///
/// WHERE IT COMES FROM. The allegiance's ThreatJobManager creates a ThreatJob for anything it
/// judges dangerous, and <c>EvaluateAttackJobs</c> offers every one of them to every colonist as
/// work. Nothing in that path asks whether this particular person should be picking a fight: a
/// cook with a kitchen knife scores the job the same way a guard with a rifle does, and the
/// nearest body wins.
///
/// THE FIRST VERSION OF THIS MOD WAS WRONG, and how it was wrong is worth keeping written down.
/// It declined a threat job unless the colonist's stance was already Bold. But the stance is an
/// OUTPUT of taking the job, not an input to choosing it - CompositeGoal substitutes Bold for any
/// goal whose <c>RequiresBoldStance()</c> is true, and <c>EvaluateChangeThreatStance</c> puts them
/// back afterwards. So the gate was self-fulfilling: never chosen, therefore never Bold,
/// therefore never chosen. tripleacoder spotted it on the thread - "usually a colonist will change
/// their stance to Bold if the job requires it, not the other way around, and this stance is
/// temporary" - and Kastuk had the symptom from play: with the mod on, colonists slept and ate
/// peacefully while a Heapjaw walked through camp. Suppressing the colony's defence is a worse
/// bug than the one being fixed.
///
/// WHAT IT DOES NOW, which is tripleacoder's suggestion: leave the threat jobs alone and make
/// people RELUCTANT to pick them, by scaling the desirability the goal competes with and
/// returning zero where going is plainly a mistake. A reluctant colonist still defends the camp
/// when there is nothing better to do - which is the behaviour the stance gate removed.
///
/// FOUR THINGS IT DELIBERATELY LEAVES ALONE:
///
///   * ASSET threats - something attacking the colony's animals, crops or structures. Being
///     reluctant about those would mean watching a vermin eat the harvest.
///   * Hunting, and anything the player ordered. An order is the player saying "yes, you".
///   * The threat evaluation itself, which also drives fleeing and stance changes. Making people
///     less willing to ATTACK is one change; making them blind to danger would be another.
///   * The scores themselves. Nothing here computes a rating - it scales the one the game
///     produced, so every factor the studio weighs still decides who the best candidate is.
///
/// THE NUMBERS ARE NOT VERIFIED. 0.25 and 0.05 are the two multipliers and neither was measured
/// against a colony, because there is no way to do that without playing. They are switchable for
/// exactly that reason.
///
/// NOT DONE, from the rest of the request: traits and professions as an input (a guard should
/// answer a threat a cook should not - the game has no combat profession to read), and restricting
/// which weapons get spent on which enemy, which is the material-policy work rather than this.
/// </summary>
public static class SelfPreservationMod
{
    public const string ModId = "selfpreservation";

    /// <summary>Below this fraction of hitpoints, an unordered fight is not this one's business.</summary>
    public const float WoundedBelow = 0.75f;

    private const string Normal = "normal";

    private const string Reluctant = "reluctant";

    private const string VeryReluctant = "very reluctant";

    private static ModSetting unorderedThreats;

    private static ModSetting injuredStayOut;

    private static ModSetting unarmedStayOut;

    private static ModSetting animalsNeedCompany;

    /// <summary>How much a colonist dislikes a fight nobody ordered.</summary>
    public static ModSetting UnorderedThreats =>
        unorderedThreats ?? (unorderedThreats = ModSettings.Choice(
            ModId, "unorderedThreats", "UNORDERED FIGHTS",
            new string[3] { Normal, Reluctant, VeryReluctant }, Reluctant,
            toolTip: "How willing colonists are to take on a threat nobody ordered them to. " +
                     "Reluctant means they go when there is nothing better to do, which still " +
                     "defends the camp; very reluctant means almost never. Normal is the " +
                     "studio's own behaviour."));

    /// <summary>Whether a wounded colonist stays out of an unordered fight entirely.</summary>
    public static ModSetting InjuredStayOut =>
        injuredStayOut ?? (injuredStayOut = ModSettings.Toggle(
            ModId, "injuredStayOut", "THE WOUNDED STAY OUT", defaultValue: true,
            toolTip: "A colonist below three quarters of their hitpoints will not take a fight " +
                     "nobody ordered. They still defend the colony's things, and still do what " +
                     "you tell them."));

    /// <summary>Whether an unarmed colonist stays out of an unordered fight entirely.</summary>
    public static ModSetting UnarmedStayOut =>
        unarmedStayOut ?? (unarmedStayOut = ModSettings.Toggle(
            ModId, "unarmedStayOut", "THE UNARMED STAY OUT", defaultValue: true,
            toolTip: "A colonist whose best option is to attack with no weapon at all will not " +
                     "take a fight nobody ordered. 'Sometimes without proper weapon' was half of " +
                     "the original report."));

    /// <summary>Whether the colony's animals wait for a person before joining a fight.</summary>
    public static ModSetting AnimalsNeedCompany =>
        animalsNeedCompany ?? (animalsNeedCompany = ModSettings.Toggle(
            ModId, "animalsNeedCompany", "DOGS FIGHT ONLY ALONGSIDE PEOPLE", defaultValue: true,
            toolTip: "A colony animal joins a fight only once one of your people is already on " +
                     "it, instead of charging anything it sees on its own."));

    public static void RegisterSettings()
    {
        _ = UnorderedThreats;
        _ = InjuredStayOut;
        _ = UnarmedStayOut;
        _ = AnimalsNeedCompany;
    }

    /// <summary>Whether anything here is switched on.</summary>
    public static bool Enabled =>
        UnorderedThreats.Value != Normal || InjuredStayOut.On || UnarmedStayOut.On
        || AnimalsNeedCompany.On;

    /// <summary>
    /// Whether <paramref name="entity"/> should refuse to consider this unordered threat job
    /// outright, before it is scored at all.
    ///
    /// ONLY THE COLONY'S ANIMALS decide anything here now. A dog joining a fight a person is
    /// already handling is what a dog is for; a dog starting one is the thing being stopped, and
    /// that is a yes/no about the job rather than a matter of degree. People go through
    /// <see cref="AdjustThreatDesirability"/> instead - see the class comment for why a filter was
    /// the wrong shape for them.
    /// </summary>
    public static bool DeclinesThreat(Entity entity, AttackJob job, bool isAssetThreat)
    {
        if (entity == null || job == null || isAssetThreat)
        {
            return false;
        }
        if (!AnimalsNeedCompany.On || entity.Intelligence == null)
        {
            return false;
        }

        // EntityType.Person is what the game itself uses to decide who counts as one of the
        // colony's people - Allegiance.Persons is filled from exactly this test.
        if (entity.EntityType?.Person != null)
        {
            return false;
        }

        // AND IT MUST BE THE COLONY'S ANIMAL. Without this line the rule reached the whole
        // wildlife simulation, which is a far larger thing than it was asked to be.
        //
        // GoalThink gives EvaluateAttackJobs to EVERY entity whose IntelligenceType.CanAttack is
        // not false, each against its OWN allegiance's threat jobs - so a Heapjaw, a Twinkler and
        // a predator all evaluate threats exactly the way a colony dog does. "Not a person, and
        // no person already on the job" is true of every one of them, permanently, because no
        // person is ever on a wild allegiance's jobs. With animalsNeedCompany on by default, wild
        // predators quietly stopped responding to threats.
        //
        // Kastuk and tripleacoder both caught it by reading - "I assume Twinklers is pretty
        // intelligent", "any entity with attacking job will need a caretaker then?" - and
        // tripleacoder's suggestion was to select the dog specifically. AllegianceType is the
        // game's own version of that distinction and does not need a list of entity keys.
        if (entity.Intelligence.Allegiance?.AllegianceType != AllegianceType.Player)
        {
            return false;
        }
        return !HasHumanTaker(job);
    }

    /// <summary>
    /// The desirability an unordered threat job should compete with, given who is considering it.
    ///
    /// Called from <c>EvaluateAttackJobs.CalculateDesirability</c> with the score the game already
    /// computed for the best weapon combo, so everything the studio weighs - distance, weapon,
    /// fitness, the target's body - has already had its say. This decides only how hard that
    /// result argues against eating, sleeping and working.
    ///
    /// Zero means the goal loses to everything, which is how a goal declines without a filter.
    /// </summary>
    public static double AdjustThreatDesirability(Entity entity, WeaponInstanceCombo combo,
                                                  bool isAssetThreat, double score)
    {
        if (entity == null || isAssetThreat || score <= 0.0)
        {
            return score;
        }

        // Only the colony's people. An animal that got this far was let through by
        // DeclinesThreat, and second-guessing it here would be the same rule applied twice.
        if (entity.EntityType?.Person == null)
        {
            return score;
        }

        if (UnarmedStayOut.On && combo != null && combo.Weapon == null)
        {
            return 0.0;
        }

        if (InjuredStayOut.On && IsWounded(entity))
        {
            return 0.0;
        }

        string level = UnorderedThreats.Value;
        if (level == VeryReluctant)
        {
            return score * 0.05;
        }
        if (level == Reluctant)
        {
            return score * 0.25;
        }
        return score;
    }

    /// <summary>
    /// Whether this one has taken enough damage to sit an unordered fight out.
    ///
    /// Computed rather than read: Body.HitpointsFractionLeft is private, and the two values it
    /// divides are not.
    /// </summary>
    private static bool IsWounded(Entity entity)
    {
        if (!entity.Find<BodyComponent>(out var component) || component?.Body == null)
        {
            return false;
        }
        Body body = component.Body;
        if (body.MaxHitpoints <= 0f)
        {
            return false;
        }
        return body.GlobalHitpoints / body.MaxHitpoints < WoundedBelow;
    }

    /// <summary>Whether one of the colony's people is already working this job.</summary>
    private static bool HasHumanTaker(AttackJob job)
    {
        foreach (Entity taker in job.GetTakersSortedByDistance())
        {
            if (taker?.EntityType?.Person != null)
            {
                return true;
            }
        }
        return false;
    }
}
