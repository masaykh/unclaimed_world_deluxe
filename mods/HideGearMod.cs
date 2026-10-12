using System.Collections.Generic;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
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
/// are defined and read by nothing. So the hide cloak gives comfort and the armour costs stealth.
///
/// PROTECTION (step 2). There is no effect for it: a body part's resistance is its type's fixed
/// ArmorLayerType, the same for every colonist. So the armour is read where a blow lands -
/// AttackType.HitTarget, beside DangerousFaunaMod's toughness - by <see cref="ArmourFactor(Entity,
/// BodyPartType, DamageType)"/>, and it covers what the studio already calls clothed: every body
/// part whose layer is clothesLayer, which on a human is the torso, arms and legs, not the head.
///
/// A FACTOR AND NOTHING ELSE, deliberately. The studio's formula is roll x (1 - resistance) -
/// constant. A flat constant would make small bites vanish, and the AI's estimate could not follow
/// it: that is precomputed per attack type and body-part TYPE (GameData.AttackScoresAgainstBodyParts),
/// so it cannot know who is wearing what. A pure factor scales the estimate exactly as it scales
/// the blow, so the three places the estimate is read for one target (EvaluateAttackJobs twice,
/// GoalHunt once) multiply it by <see cref="ArmourFactor(BodyPart, AttackType)"/> and an attacker
/// sees an armoured colonist as the harder target it is.
///
/// THE BACKPACK (step 3). Not more haul space, which is what a backpack sounds like: haul capacity
/// is ItemStorage.TotalCapacity, saved with the game and copied into each storage space and into
/// what colonists remember of each other, so changing it on putting a pack on would have to be
/// undone exactly on taking it off, through a save. Instead the load FEELS lighter: the two places
/// the game turns "how full is the haul storage" into a cost - Locomotor.ApplyBurdenModifier's
/// slowdown when heavily laden, and GoalTraverseEdgeBetweenWaypointsAtomic.GetExertionLevel's
/// tiredness from carrying - read that fraction through <see cref="LoadFelt"/>. Nothing is stored,
/// so nothing can go wrong in a save. Its effect profile has no effects: the game only treats an
/// item as a gadget if it names one, and the profile is what the player sees while it is worn.
///
/// Hauling asks for it. GoalHaul already calls FindOptionalEquipmentIfNeeded, for a weapon and
/// food on a long haul, but with no gear task, and ItemType.TaskType.Hauling is marked "not in
/// use". <see cref="AddHaulGear"/> passes it. Like the cloak, only on hauls beyond normal range:
/// fetching a pack for a short carry would cost more than it saves.
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

    public const string BackpackKey = "item:hideBackpack";

    /// <summary>The backpack's effect profile, which names no effect (see the class comment).</summary>
    public const string BackpackProfileKey = "hideBackpackLoad";

    /// <summary>How full the haul storage feels with a backpack on: a load of 1 counts as 0.7.</summary>
    public const float BackpackLoadFelt = 0.7f;

    /// <summary>Comfort while wearing the cloak: the studio's stimulant gives 0.05, a strong one 0.08.</summary>
    public const float CloakComfort = 0.05f;

    /// <summary>
    /// Stealth while wearing the armour. Added to a detection factor clamped to 0..1, where the
    /// studio's cloak adds 0.3 and a skilled hunter's sneaking up to 0.8.
    /// </summary>
    public const float ArmourStealth = -0.15f;

    /// <summary>
    /// The share of a blow the hide armour takes, by DamageType key; anything not listed passes.
    /// Against the studio's own layers: human skin takes 0.2 of a cut, bite or blow (and a flat 4),
    /// an animal's hide 0.1 (and 2). Tanned and layered hide is better than either at cuts and
    /// bites, half that at blows, and stops little of a musket ball or an arrow.
    /// </summary>
    public static readonly Dictionary<string, float> ArmourProtection = new Dictionary<string, float>
    {
        { "sharp", 0.3f },
        { "bite", 0.3f },
        { "blunt", 0.15f },
        { "piercing", 0.1f }
    };

    /// <summary>The body layer the armour goes over: what the studio's bodies call clothed.</summary>
    public const string CoveredLayer = "clothesLayer";

    private static ModSetting enabled;

    /// <summary>Whether the two items, their recipes and the patrol's request for armour are in.</summary>
    public static ModSetting Enabled =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "HIDE CLOTHING AND ARMOUR", defaultValue: false,
            toolTip: "Adds a hide backpack (long hauls feel 30% lighter), a hide cloak (megapod hide, some comfort, worn on long trips and at " +
                     "night) and hide armour (whipjaw and megapod hide, worn on patrol: takes 30% of " +
                     "cuts and bites to the body, arms and legs, but harder to sneak in). Changes what a save contains: a save with these items needs this on.",
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
            Name = "Hide armour",
            Description = "The character is easier to detect, and takes less from cuts and bites to the body, arms and legs.",
            Effects = new string[1] { ArmourEffectKey },
            SortOrder = 120
        });
        list.Add(new EffectProfileType
        {
            KeyName = BackpackProfileKey,
            Name = "Backpack",
            Description = "A load feels 30% lighter: less slowed when heavily laden, and less tired by carrying.",
            Effects = new string[0],
            SortOrder = 130
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
            "Worn on patrol. Takes 30% of a cut or a bite to the body, arms and legs, 15% of a blow " +
            "and 10% of a shot, but not to the head. Stiff and loud, so the wearer is easier to notice.",
            0.2f, ArmourEffectKey, "hideGrey", atAnyDistance: true,
            new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
            {
                { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best }
            }));

        // Long hauls only, like the cloak's long trips.
        listOfEntityTypes.Add(Gear(BackpackKey, "Hide backpack",
            "A pack of tanned megapod hide on thunder chicken leather straps.",
            "Worn on long hauls. A load feels 30% lighter: the wearer is less slowed when heavily laden, and less tired by carrying.",
            0.1f, BackpackProfileKey, "leatherPouch", atAnyDistance: false,
            new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
            {
                { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Best }
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
        // Thunder chicken leather for the straps, as the bellows use it. Not rawhide string, which
        // this tool set already holds as a TOOL; the validator refuses an item as tool and input both.
        listOfProcessTypes.Add(Recipe("makeHideBackpack", BackpackKey, ("item:megapodTannedHide", 1), ("item:thunderChickenTannedHide", 1)));
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
    /// What is left of a blow of this damage to this body part of this entity, for what it wears:
    /// 1 when the mod is off, the part is not one clothes cover, or nothing worn protects.
    /// Called from AttackType.HitTarget.
    /// </summary>
    public static float ArmourFactor(Entity wearer, BodyPartType part, DamageType damage)
    {
        if (!Enabled.On || wearer?.AgentStorage?.Equipment == null || damage == null
            || part?.ArmorLayerType == null || part.ArmorLayerType.KeyName != CoveredLayer)
        {
            return 1f;
        }
        return Wears(wearer, ArmourKey) ? Remaining(damage.KeyName) : 1f;
    }

    /// <summary>Whether this entity has an item of this key in its Equipment storage.</summary>
    private static bool Wears(Entity wearer, string itemKey)
    {
        if (wearer?.AgentStorage?.Equipment == null)
        {
            return false;
        }
        foreach (EntityID id in wearer.AgentStorage.Equipment.StoredItems)
        {
            Entity item = Entity.FindByID(id);
            if (item != null && item.EntityType.KeyName == itemKey)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// How full a load feels to this entity, as a factor on the stored fraction: the backpack's
    /// <see cref="BackpackLoadFelt"/>, or 1. Called from Locomotor.ApplyBurdenModifier and
    /// GoalTraverseEdgeBetweenWaypointsAtomic.GetExertionLevel.
    /// </summary>
    public static float LoadFelt(Entity carrier)
    {
        return Enabled.On && Wears(carrier, BackpackKey) ? BackpackLoadFelt : 1f;
    }

    /// <summary>
    /// Called from GoalHaul where it asks for optional equipment: a long haul asks for hauling gear.
    /// </summary>
    public static void AddHaulGear(ref List<ItemType.TaskType> gearTasks)
    {
        if (!Enabled.On)
        {
            return;
        }
        if (gearTasks == null)
        {
            gearTasks = new List<ItemType.TaskType>();
        }
        if (!gearTasks.Contains(ItemType.TaskType.Hauling))
        {
            gearTasks.Add(ItemType.TaskType.Hauling);
        }
    }

    /// <summary>The factor for one damage type: the pure arithmetic, for the self-test.</summary>
    public static float Remaining(string damageKey)
    {
        return ArmourProtection.TryGetValue(damageKey, out float taken) ? 1f - taken : 1f;
    }

    /// <summary>
    /// The same factor, for an attacker's ESTIMATE against a body part it is scoring: the target
    /// it sees, or the one it remembers (a body in a MemoryFact, which names the entity). What it
    /// wears is read from the entity itself - armour is not something a colonist can hide.
    /// Called where EvaluateAttackJobs and GoalHunt read GameData.AttackScoresAgainstBodyParts.
    /// </summary>
    public static float ArmourFactor(BodyPart part, AttackType attack)
    {
        if (!Enabled.On || part?.Body == null || attack == null)
        {
            return 1f;
        }
        Entity wearer = part.Body.Parent;
        if (wearer == null && part.Body.ParentMemoryFact != null)
        {
            wearer = Entity.FindByID(part.Body.ParentMemoryFact.EntityID);
        }
        return ArmourFactor(wearer, part.BodyPartType, attack.DamageFinal);
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
