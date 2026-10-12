using System.Collections.Generic;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.XmlCollections;

namespace UWGame.Mods;

/// <summary>
/// Clothing and armour from tanned hide, worn as gadgets. Step 1 of "Colonist equipment" (the
/// #tasks thread, from jerrybi, tripleacoder and Kastuk in #general, 2026-10-10; jerrybi: "Do in
/// numeric order").
///
/// HOW GEAR WORKS IN THE STUDIO'S GAME. A gadget is an item with EffectsWhenEquipped and
/// TaskAppropriateLevels. A goal that wants gear names its task types, and
/// CompositeGoal.FindOptionalEquipmentIfNeeded picks the single best gadget for them and puts it
/// in the colonist's Equipment storage, where AgentStorage.StartEffects switches its effects on
/// until it leaves. The cloak, the night-vision goggles and the ground scanner are the stock ones.
///
/// WHAT AN EFFECT CAN DO. AffectsNumbers has seven members, and only some are read: Stealth
/// (Entity's detection factor, in every activity), AgentComfort (Intelligence.Comfort, which the
/// comfort rating averages and emigration weighs), NightSensorRange and Detection. Morale and Need
/// are defined and read by nothing. So the hide cloak gives comfort and the armour costs stealth;
/// the armour's protection is step 2, a damage-resistance effect read in AttackType.ComputeDamage.
///
/// WHAT IS ASKED FOR GEAR. The studio's goals ask for UnspecifiedHunting (finding prey), Scouting
/// or Examining (checking, scouting) and, after dark, NightActivities. PatrolOrAttack is a task type
/// with no goal asking for it: GoalPatrol asks for night gear only. <see cref="AddPatrolGear"/> is
/// the one line that makes it ask. Attacking does not ask for gear at all, and is left alone: an
/// attack is a reaction, and stopping to dress for it would be worse than going without.
///
/// NEW ITEM KEYS. This is the first mod to add EntityType keys rather than only recipes (PeatMod's
/// header says why it stopped doing that). A save holding a hide cloak names item:hideCloak, which
/// a build without this switch cannot resolve. The switch is simulation-affecting and its stock
/// value is off, so a save made with it on is stamped hidegear.enabled=true (ModSettings.Signature)
/// and opening it switches it on again. It is off by default for the same reason.
///
/// ART. There is none of its own yet: the cloak is drawn with the tanned hide's sprite and the
/// armour with the grey hide's, both from the GUI sheet, until somebody draws them.
/// </summary>
public static class HideGearMod
{
    public const string ModId = "hidegear";

    public const string CloakKey = "item:hideCloak";

    public const string ArmourKey = "item:hideArmour";

    public const string CloakEffectKey = "hideCloakComfort";

    public const string ArmourEffectKey = "hideArmourStealth";

    /// <summary>Comfort while wearing the cloak: the studio's stimulant gives 0.05, a strong one 0.08.</summary>
    public const float CloakComfort = 0.05f;

    /// <summary>
    /// Stealth while wearing the armour. Added to a detection factor clamped to 0..1, where the
    /// studio's cloak adds 0.3 and a skilled hunter's sneaking up to 0.8.
    /// </summary>
    public const float ArmourStealth = -0.15f;

    private static ModSetting enabled;

    /// <summary>Whether the two items, their recipes and the patrol's request for armour are in.</summary>
    public static ModSetting Enabled =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "HIDE CLOTHING AND ARMOUR", defaultValue: false,
            toolTip: "Adds a hide cloak (megapod hide, some comfort, worn on long trips and at " +
                     "night) and hide armour (whipjaw and megapod hide, worn on patrol, harder to " +
                     "sneak in). Changes what a save contains: a save with these items needs this on.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    /// <summary>Registered from Program.Main BEFORE any data table is built: the loaders read the switch.</summary>
    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "HIDE GEAR");
        ModSettings.Describe(ModId, "Colonist equipment, step 1",
            "Clothing and armour made from tanned hide, picked up as gear: a cloak for comfort on long trips, armour for patrols.",
            "tarpBeige");
        _ = Enabled;
    }

    /// <summary>
    /// Called at the end of BaseDataLoader.InitEffectProfileTypes. An item's EffectsWhenEquipped
    /// names a PROFILE (ItemType.PostDataCompleteInitialize looks it up in AllEffectProfileTypes),
    /// which names the effects and carries what the interface shows. One each, keyed like the
    /// effect, as the studio's cloak does it.
    /// </summary>
    public static void AddEffectProfiles(List<EffectProfileType> list)
    {
        if (!Enabled.On)
        {
            return;
        }
        list.Add(new EffectProfileType
        {
            KeyName = CloakEffectKey,
            Name = "Warm clothing",
            Description = "The character is a little more comfortable.",
            Effects = new string[1] { CloakEffectKey },
            SortOrder = 110
        });
        list.Add(new EffectProfileType
        {
            KeyName = ArmourEffectKey,
            Name = "Stiff armour",
            Description = "The character is easier to detect.",
            Effects = new string[1] { ArmourEffectKey },
            SortOrder = 120
        });
    }

    /// <summary>Called at the end of BaseDataLoader.InitEffectTypes.</summary>
    public static void AddEffects(List<EffectType> list)
    {
        if (!Enabled.On)
        {
            return;
        }
        list.Add(new NumberEffectType
        {
            KeyName = CloakEffectKey,
            Affects = AffectsNumbers.AgentComfort,
            FormatAsPercentage = true,
            Intensity = CloakComfort
        });
        list.Add(new NumberEffectType
        {
            KeyName = ArmourEffectKey,
            Affects = AffectsNumbers.Stealth,
            Operator = NumberEffectOperator.Add,
            Intensity = ArmourStealth
        });
    }

    /// <summary>Called at the end of BaseDataLoader.InitEntityTypes.</summary>
    public static void AddItems(List<EntityType> listOfEntityTypes)
    {
        if (!Enabled.On)
        {
            return;
        }

        // Long trips and nights out, as the studio's cloak is for hunting: never "Best", so a
        // stock gadget made for the task still wins. Only beyond normal range, like the cloak.
        listOfEntityTypes.Add(Gear(CloakKey, "Hide cloak",
            "A heavy cloak of tanned megapod hide.",
            "Keeps the wearer warm and dry on long trips and at night, which makes them a little more comfortable.",
            0.12f, CloakEffectKey, "tarpBeige", atAnyDistance: false,
            new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
            {
                { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal },
                { ItemType.TaskType.NightActivities, ItemType.AppropriateLevel.Minor }
            }));

        // Patrol at any distance: a guard zone is usually next to the camp, inside the range the
        // studio leaves gear behind for.
        listOfEntityTypes.Add(Gear(ArmourKey, "Hide armour",
            "Layered whipjaw hide over a megapod hide jerkin.",
            "Worn on patrol. Stiff and loud, so the wearer is easier to notice.",
            0.2f, ArmourEffectKey, "hideGrey", atAnyDistance: true,
            new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
            {
                { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best }
            }));
    }

    private static EntityType Gear(string key, string name, string summary, string description,
        float bulk, string effect, string sprite, bool atAnyDistance,
        SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel> tasks)
    {
        return new EntityType(key)
        {
            Name = name,
            SummaryDescription = summary,
            Description = description,
            ItemType = new ItemType
            {
                MaximumBulk = bulk,
                EffectsWhenEquipped = new string[1] { effect },
                TaskAppropriateLevels = tasks,
                UseGearAtAnyDistanceFromExpedition = atAnyDistance
            },
            TierOrArea = new TierOrArea
            {
                Tier = "basic"
            },
            NonLivingType = new NonLivingType
            {
                Repairability = 1f,
                DegradeType = "equipment"
            },
            CategoryKey = "equipment",
            RenderableType = new RenderableType
            {
                DefaultClientState = new ClientStateInfo
                {
                    RenderAsBillboardType = new RenderAsBillboardType[1]
                    {
                        new RenderAsBillboardType
                        {
                            AssetName = sprite
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Called in BaseDataLoader.InitProcessTypes, before DisassemblyMod reads the table - so taking
    /// a piece apart gives its hides back like any other assembled item.
    ///
    /// No attainability hook, unlike PeatMod's: that one adds a second route to an item which
    /// already had an entry. These items are new, and InventorySettings computes theirs from
    /// GameData's production recipes like any other.
    /// </summary>
    public static void AddProcesses(List<ProcessType> listOfProcessTypes)
    {
        if (!Enabled.On)
        {
            return;
        }
        // Made like the bellows: hide, by hand, bushcraft, a tool set any camp has.
        listOfProcessTypes.Add(Recipe("makeHideCloak", CloakKey, ("item:megapodTannedHide", 2)));
        listOfProcessTypes.Add(Recipe("makeHideArmour", ArmourKey, ("item:whipjawTannedHide", 2), ("item:megapodTannedHide", 1)));
    }

    private static ProcessType Recipe(string key, string output, params (string item, int count)[] hides)
    {
        Input[] inputs = new Input[hides.Length];
        for (int i = 0; i < hides.Length; i++)
        {
            inputs[i] = new Input
            {
                Entity = hides[i].item,
                IsConsumed = true,
                Amount = new InputAmount
                {
                    NoOfItems = hides[i].count
                }
            };
        }
        return new ProcessType
        {
            Name = "Producing",
            KeyName = key,
            JobTypeKey = "craftingJobType",
            RequiredSkill = "bushcraft",
            PhysicalWorkFactor = 4f,
            Stances = ProcessLoader.GetToolMakingStances(),
            Inputs = inputs,
            Outputs = new Output[1]
            {
                new Output
                {
                    EntityTypeToCreate = output,
                    Amount = new OutputAmount
                    {
                        NoOfItems = 1
                    }
                }
            },
            WorkOrTimeNeeded = new WorkOrTime
            {
                DaysNeeded = 1f / 15f
            },
            ProcessToolSetKey = "toolSetCombineLightImprovisedObjects",
            AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
        };
    }

    /// <summary>
    /// Called from GoalPatrol beside the studio's AddNightActivityGear: a patrol asks for patrol
    /// gear, which the studio defined (ItemType.TaskType.PatrolOrAttack) and nothing asked for.
    /// </summary>
    public static void AddPatrolGear(ref List<ItemType.TaskType> gearTasks)
    {
        if (!Enabled.On)
        {
            return;
        }
        if (gearTasks == null)
        {
            gearTasks = new List<ItemType.TaskType>();
        }
        if (!gearTasks.Contains(ItemType.TaskType.PatrolOrAttack))
        {
            gearTasks.Add(ItemType.TaskType.PatrolOrAttack);
        }
    }
}
