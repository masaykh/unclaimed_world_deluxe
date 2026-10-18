using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
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
/// THE BACKPACK AND THE HAND CART (step 3, then jerrybi, 2026-10-12: "Backpack should allow to
/// carry much more stuff ... up to 5 of weight", "the Cart ... 10 of weight with NO speed debuff").
/// A colonist hauls 1.0. Wearing a backpack it is 5, with a cart 10: <see cref="EquipmentChanged"/>
/// sets ItemStorage.TotalCapacity and its one storage space each time a pack or cart goes into
/// Equipment or comes out, the way AgentStorage.NotifyStomachFractionChanged already resizes the
/// stomach. It is computed from the colonist's TYPE plus what is worn, never from the stored value,
/// so it cannot drift; the stored value is saved, so a loaded game has what it had. Taking a pack
/// off with more than 1.0 aboard leaves the colonist over capacity until GoalHaul's
/// DropItemsOverCapacity sheds it on the next haul; meanwhile <see cref="Felt"/> clamps the load
/// at full, because Locomotor's Lerp is unclamped and would slow them towards a standstill. GoalHaul
/// fills a trip from TotalCapacity (FindExtraCargo), so a bigger capacity is fewer trips by itself.
/// How heavy the load FEELS is read in the two places the game turns "how full" into a cost -
/// Locomotor.ApplyBurdenModifier's slowdown and GoalTraverseEdgeBetweenWaypointsAtomic's exertion:
/// a pack 0.7 of it, a cart nothing at all (it rolls). Colonists remember each other's capacity as
/// last seen (MemoryFact), so another colonist's plans may be a sighting behind; nothing in the
/// studio's AI plans a haul from SOMEONE ELSE's capacity. Their profiles have no effects: the game
/// only treats an item as a gadget if it names one, and the profile is what the player sees.
///
/// FIGHTERS (jerrybi: "only they should be able to use armour"). A fighter is a colonist whose
/// profession is Security or Hunting - Intelligence.Profession, the profession of their best skill
/// once it reaches the expert level (Constants.ExpertSkillLevel, 0.9). <see cref="MayWear"/> keeps
/// armour out of CompositeGoal.IsGadgetForTask for anyone else.
///
/// WEAPONS AT HAND (step 4, its own switch). Why a colonist cannot keep a musket, a knife and a
/// spear: a weapon fetched for a fight - CompositeGoal.GatherToolsOrWeapons, from GoalAttack,
/// GoalPatrol and GoalFindPrey - goes into the HAUL storage, which is what hauling fills. The next
/// haul calls DropUnneededItemsToMakeCapacity and the weapon is left on the ground. The studio's
/// attack AI already handles several carried weapons (EvaluateAttackJobs compares an uncarried one
/// with the best carried, refuses a second of a type, and prefers the one in range), so what was
/// missing is only somewhere to keep them. That is the Equipment storage: 0.5 on a colonist, where a
/// musket, a knife and a spear take 0.29, and the hide gear fits beside them. With the switch on,
/// <see cref="CompartmentFor"/> sends a weapon fetched for a fight there, and keeps one already there
/// from being moved back to haul by a job that uses it as a tool. No storage grows, so nothing about
/// a save changes: a full Equipment drops what the job does not need, as the studio's code already
/// does. A pickaxe fetched to mine with is a tool, not a fight, and still goes to haul.
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
/// THE MACKINTOSH (Kastuk, "Colonist equipment", 2026-10-17: "New clothes need to protect against
/// weather too. Add new version of cloak with added marshcoat sap to make it water resistant
/// Mackintosh to withstand the rain seasons."). Then, 2026-10-18: "Truly mackintosh been made from
/// cotton textile." So it is textile, not the hide cloak, proofed with marshcot sap
/// (item:marshcotSap - "This emulsion has many of the same qualities found in natural rubber from
/// Earth"; the studio turns it into rubber for gaskets, makeGaskets). <see cref="MackintoshKey"/>,
/// made by makeMackintosh from 2 item:textile and 2 sap - by hand, bushcraft, in the cloak's time and
/// on the cloak's tool set; the studio's rubber is chemistry at a polymer workshop, but proofing a
/// cloak is brushing sap on, not making rubber sheets. Worn like the cloak - long trips and at night,
/// beyond normal range, Normal at night where the cloak is Minor, so a colonist who can reach one
/// takes it over the cloak - with the cloak's comfort under its own profile, "Waterproof clothing".
/// What it does against the weather is WeatherMod's (COLD AND WET HURT, <see cref="WeatherMod.GarmentCover"/>):
/// the cloak keeps off some of the cold and the wind, the Mackintosh the rain as well. Disassembled
/// (DisassemblyMod), it gives the textile back: the sap has cured into it.
///
/// ART. There is none of its own yet: the cloak is drawn with the tanned hide's sprite and the
/// armour with the grey hide's, both from the GUI sheet, until somebody draws them; the Mackintosh
/// with the studio's tarp.
/// </summary>
public static class HideGearMod
{
    public const string ModId = "hidegear";

    public const string CloakKey = "item:hideCloak";

    public const string ArmourKey = "item:hideArmour";

    public const string CloakEffectKey = "hideCloakComfort";

    public const string MackintoshKey = "item:mackintosh";

    /// <summary>The studio's woven fabric (item:textile), what a Mackintosh is made of.</summary>
    public const string TextileKey = "item:textile";

    /// <summary>The Mackintosh's profile and its comfort effect, keyed alike as the cloak's are.</summary>
    public const string MackintoshEffectKey = "mackintoshComfort";

    public const string ArmourEffectKey = "hideArmourStealth";

    public const string BackpackKey = "item:hideBackpack";

    /// <summary>The backpack's effect profile, which names no effect (see the class comment).</summary>
    public const string BackpackProfileKey = "hideBackpackLoad";

    /// <summary>How full the haul storage feels with a backpack on: a load of 1 counts as 0.7.</summary>
    public const float BackpackLoadFelt = 0.7f;

    /// <summary>What a colonist can haul wearing a backpack (a colonist alone: 1).</summary>
    public const float BackpackCapacity = 5f;

    public const string CartKey = "item:handCart";

    public const string CartProfileKey = "handCartLoad";

    /// <summary>What a colonist can haul with a cart, and with no slowdown at all.</summary>
    public const float CartCapacity = 10f;

    /// <summary>The professions that make a colonist a fighter (ProfessionType keys).</summary>
    public static readonly string[] FighterProfessions = new string[2] { "securityPerson", "hunter" };

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
            toolTip: "Adds a hide backpack (haul up to 5), a hand cart (haul up to 10, no slowdown), a hide cloak (megapod hide, some comfort, worn on long trips and at " +
                     "night), a Mackintosh (cotton textile proofed with marshcot sap: with COLD AND WET HURT it keeps the rain off) and hide armour (whipjaw and megapod hide, worn by Security and Hunting colonists on patrol: takes 30% of " +
                     "cuts and bites to the body, arms and legs, but harder to sneak in). Changes what a save contains: a save with these items needs this on.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    private static ModSetting weaponsAtHand;

    /// <summary>Whether a weapon fetched for a fight is kept in Equipment rather than haul storage.</summary>
    public static ModSetting WeaponsAtHand =>
        weaponsAtHand ?? (weaponsAtHand = ModSettings.Toggle(
            ModId, "weaponsAtHand", "WEAPONS KEPT AT HAND", defaultValue: false,
            toolTip: "A weapon fetched for a fight, a patrol or a hunt goes into the equipment a " +
                     "colonist wears rather than the space they haul with, so the next haul does not " +
                     "leave it on the ground. A colonist can keep a musket, a knife and a spear and " +
                     "pick the right one when a fight comes. Weapons kept this way are not free for " +
                     "others to take.",
            affectsSimulation: true));

    /// <summary>Registered from Program.Main BEFORE any data table is built: the loaders read the switch.</summary>
    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "HIDE GEAR");
        ModSettings.Describe(ModId, "Colonist equipment, step 1",
            "Clothing and armour made from tanned hide, picked up as gear: a cloak for comfort on long trips, a Mackintosh against the rain, armour for patrols.",
            "tarpBeige");
        _ = Enabled;
        _ = WeaponsAtHand;
        _ = FighterKitSetting;
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
            KeyName = MackintoshEffectKey,
            Name = "Waterproof clothing",
            Description = "The character is a little more comfortable, and keeps dry in the rain.",
            Effects = new string[1] { MackintoshEffectKey },
            SortOrder = 115
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
            Description = "Can haul up to 5, and a load feels 30% lighter: less slowed when heavily laden, and less tired by carrying.",
            Effects = new string[0],
            SortOrder = 130
        });
        list.Add(new EffectProfileType
        {
            KeyName = CartProfileKey,
            Name = "Hand cart",
            Description = "Can haul up to 10, with no slowdown however full.",
            Effects = new string[0],
            SortOrder = 140
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
            KeyName = MackintoshEffectKey,
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
            "Keeps the wearer warm on long trips and at night, which makes them a little more comfortable. " +
            "Slept in on a cold night out, it keeps off some of the cold and the wind, but not the rain.",
            0.12f, CloakEffectKey, "tarpBeige", atAnyDistance: false,
            new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
            {
                { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal },
                { ItemType.TaskType.NightActivities, ItemType.AppropriateLevel.Minor }
            }));

        // The cloak, proofed: the same trips, and preferred to it at night.
        listOfEntityTypes.Add(Gear(MackintoshKey, "Mackintosh",
            "A cotton coat proofed with marshcot sap.",
            "Keeps the wearer warm and dry on long trips and at night, which makes them a little more comfortable. " +
            "Slept in on a cold, wet night out, it keeps off most of the rain and some of the cold and the wind.",
            0.12f, MackintoshEffectKey, "tarp", atAnyDistance: false,
            new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
            {
                { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal },
                { ItemType.TaskType.NightActivities, ItemType.AppropriateLevel.Normal }
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
            "Taken for hauling. The wearer can haul up to 5 instead of 1, and a load feels 30% lighter: less slowed when heavily laden, and less tired by carrying.",
            0.1f, BackpackProfileKey, "leatherPouch", atAnyDistance: true,
            new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
            {
                { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Normal }
            }));

        // Best for hauling, so a colonist who can reach a cart takes it over a pack.
        listOfEntityTypes.Add(Gear(CartKey, "Hand cart",
            "A two-wheeled cart of trimmed spoak and hide straps.",
            "Taken for hauling. Holds up to 10, and rolls: no slowdown however full, and no more tiring than walking.",
            0.2f, CartProfileKey, "furniture", atAnyDistance: true,
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
        // The cloak brushed with sap, in the cloak's time on the cloak's tool set (the class comment).
        listOfProcessTypes.Add(Recipe("makeMackintosh", MackintoshKey, (TextileKey, 2), ("item:marshcotSap", 2)));
        listOfProcessTypes.Add(Recipe("makeHideArmour", ArmourKey, ("item:whipjawTannedHide", 2), ("item:megapodTannedHide", 1)));
        // Thunder chicken leather for the straps, as the bellows use it. Not rawhide string, which
        // this tool set already holds as a TOOL; the validator refuses an item as tool and input both.
        listOfProcessTypes.Add(Recipe("makeHideBackpack", BackpackKey, ("item:megapodTannedHide", 1), ("item:thunderChickenTannedHide", 1)));
        // Wood, so carpentry, like the studio's furniture. item:wheel exists but nothing makes it.
        // jerrybi: "megapod hide OR 4 metal sheet". The game has no metal sheet (goldSheet aside),
        // so the metal cart is bound with wrought iron, on the studio's wood-and-metal tool set.
        listOfProcessTypes.Add(CartRecipe("makeHandCart", "toolSetCarpentry", ("item:spoakBranchesTrimmed", 6), ("item:megapodTannedHide", 2)));
        listOfProcessTypes.Add(CartRecipe("makeHandCartWithIron", "toolSetAttachWoodAndMetal", ("item:spoakBranchesTrimmed", 6), ("item:wroughtIron", 4)));
    }

    private static ProcessType CartRecipe(string key, string toolSet, params (string item, int count)[] inputs)
    {
        ProcessType cart = Recipe(key, CartKey, inputs);
        cart.RequiredSkill = "carpentry";
        cart.ProcessToolSetKey = toolSet;
        cart.WorkOrTimeNeeded = new WorkOrTime
        {
            DaysNeeded = 1f / 10f
        };
        return cart;
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

    /// <summary>Whether this entity has an item of this key in its Equipment storage. WeatherMod asks it of the cloaks.</summary>
    public static bool Wears(Entity wearer, string itemKey)
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
    /// How full a load feels to this entity, given how full it is (stored / capacity): with a cart
    /// nothing, with a backpack 0.7 of it, otherwise all of it - and never more than full, because
    /// taking a pack off can leave a colonist over capacity for a while. 1 is "full", as in the
    /// studio's formulas. Called from Locomotor.ApplyBurdenModifier and
    /// GoalTraverseEdgeBetweenWaypointsAtomic.GetExertionLevel.
    /// </summary>
    public static float Felt(Entity carrier, float fraction)
    {
        if (!Enabled.On)
        {
            return fraction;
        }
        return FeltWith(Wears(carrier, CartKey), Wears(carrier, BackpackKey), fraction);
    }

    /// <summary>The arithmetic of <see cref="Felt"/>, for the self-test.</summary>
    public static float FeltWith(bool cart, bool backpack, float fraction)
    {
        float full = System.Math.Min(fraction, 1f);
        return cart ? 0f : backpack ? full * BackpackLoadFelt : full;
    }

    /// <summary>
    /// What a colonist of this type can haul with these items in Equipment: the type's own
    /// capacity, or the largest a carried pack or cart gives. Never less than the type's own.
    /// </summary>
    public static float HaulCapacity(float typeCapacity, IEnumerable<string> carriedKeys)
    {
        float capacity = typeCapacity;
        foreach (string key in carriedKeys)
        {
            if (key == CartKey) capacity = System.Math.Max(capacity, CartCapacity);
            else if (key == BackpackKey) capacity = System.Math.Max(capacity, BackpackCapacity);
        }
        return capacity;
    }

    /// <summary>
    /// Called from AgentStorage.StartEffects and EndEffects - every time an item goes into
    /// Equipment or comes out, is swapped, or breaks there. <paramref name="item"/> is counted as
    /// worn when <paramref name="equipped"/>, and as gone otherwise, whatever Equipment says yet.
    /// Resizes the haul storage when it is a pack or a cart; anything else returns at once.
    /// </summary>
    public static void EquipmentChanged(AgentStorage storage, Entity item, bool equipped)
    {
        if (!Enabled.On || storage?.ItemStorage == null || storage.Equipment == null || item == null
            || (item.EntityType.KeyName != BackpackKey && item.EntityType.KeyName != CartKey))
        {
            return;
        }
        AgentStorageType type = storage.Parent.EntityType.ContainerType as AgentStorageType;
        if (type?.ItemStorageType == null)
        {
            return;
        }
        List<string> carried = new List<string>();
        foreach (EntityID id in storage.Equipment.StoredItems)
        {
            Entity worn = Entity.FindByID(id);
            if (worn != null && worn != item)
            {
                carried.Add(worn.EntityType.KeyName);
            }
        }
        if (equipped)
        {
            carried.Add(item.EntityType.KeyName);
        }
        float capacity = HaulCapacity(type.ItemStorageType.GetTotalCapacity(), carried);
        // Both, as NotifyStomachFractionChanged does: the storage and its one space.
        storage.ItemStorage.TotalCapacity = capacity;
        if (UWGame.SimSide.GameData.Instance.AllStorageConditions.TryGetValue("isolated", out var isolated)
            && storage.ItemStorage.StorageSpaces.TryGetValue(isolated, out var space))
        {
            space.TotalCapacity = capacity;
        }
    }

    /// <summary>Whether this colonist is a fighter: Security or Hunting by profession.</summary>
    public static bool IsFighter(Entity colonist)
    {
        string profession = colonist?.Intelligence?.Profession?.KeyName;
        return profession != null && System.Array.IndexOf(FighterProfessions, profession) >= 0;
    }

    /// <summary>
    /// Whether this colonist may take this gadget: armour only if a fighter. Called from
    /// CompositeGoal.IsGadgetForTask; true for everything when the mod is off.
    /// </summary>
    public static bool MayWear(Entity colonist, EntityType gadget)
    {
        return !Enabled.On || gadget?.KeyName != ArmourKey || (IsFighter(colonist) && !RefusesArmour(colonist));
    }

    /// <summary>
    /// Which storage an item fetched by CompositeGoal.GatherToolsOrWeapons goes into. Called once
    /// per item; <paramref name="asked"/> is the caller's, Haul for every fight and every tool job.
    /// </summary>
    public static StorageCompartment CompartmentFor(Entity carrier, IKnownEntityData item, bool forFight, StorageCompartment asked)
    {
        if (!WeaponsAtHand.On || carrier?.AgentStorage?.Equipment == null || item?.EntityType?.ItemType == null)
        {
            return asked;
        }
        return Compartment(item.EntityType.ItemType.WeaponType != null, forFight,
            carrier.AgentStorage.Equipment.Contains(item.EntityID), asked);
    }

    /// <summary>The rule itself, for the self-test.</summary>
    public static StorageCompartment Compartment(bool isWeapon, bool forFight, bool inEquipment, StorageCompartment asked)
    {
        if (!isWeapon || asked != StorageCompartment.Haul)
        {
            return asked;
        }
        // Fetched to fight with, or already kept and only borrowed as a tool: Equipment.
        return forFight || inEquipment ? StorageCompartment.Equipment : asked;
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

    // ------------------------------------------------------------------------------------------
    // FIGHTERS KEEP A FULL KIT (jerrybi, 2026-10-12). Behind its own switch, FighterKitSetting.
    //
    //   * a fighter tries to keep one big weapon (ranged, or a spear) and one small one (knife,
    //     machete, hand axe), and armour once the expedition's security policy is at its third
    //     tier (medium) - fetched through FindOptionalEquipmentIfNeeded, which almost every goal
    //     already calls on its way to work (FighterKit, WeaponWanted)
    //   * while not kitted they lose comfort: a missing big weapon more than a missing small one,
    //     and armour only after ArmourMissingDays without it (ComfortPenalty, read in
    //     Intelligence.Comfort beside DiscomfortMod's)
    //   * "one fully equipped fighter covers all protection needs if there is no active threat
    //     nearby, 3 if there is": once that many are kitted, the rest feel no pressure and fetch
    //     nothing (CoverageMet); and a kitted fighter counts for more in the Security rating
    //     (DefenseFactor, in SecurityStatisticsForAllegiance.GetAgentDefenseRating)
    //   * kitted out - both weapons and armour - they hit more often and attack faster, and work at
    //     anything else more slowly (HitChanceFactor, AttackSpeedFactor, WorkFactor): the player's
    //     reason to think twice about the third tier
    //   * the player can order a fighter out of their armour, to pass it to a stronger fighter or
    //     to undress an injured one (SetRefusesArmour, from the colonist's context menu); they put
    //     it down the next time they look at their gear, and do not take armour again until the
    //     order is reversed
    //
    // Every number below is a first guess, named so it can be tuned.
    // ------------------------------------------------------------------------------------------

    private static ModSetting fighterKit;

    /// <summary>Whether fighters keep a full kit, feel its absence, and fight better wearing it.</summary>
    public static ModSetting FighterKitSetting =>
        fighterKit ?? (fighterKit = ModSettings.Toggle(
            ModId, "fighterKit", "FIGHTERS KEEP A FULL KIT", defaultValue: false,
            toolTip: "Security and Hunting colonists fetch a big weapon and a small one, and armour " +
                     "once the security policy is at its third tier. Missing them costs comfort; " +
                     "wearing the full kit, they fight better and do other work more slowly. One " +
                     "kitted fighter covers the camp, three when a threat is near; then the rest " +
                     "are not pressed. Needs HIDE CLOTHING AND ARMOUR for the armour.",
            affectsSimulation: true));

    /// <summary>Comfort lost without a big weapon (the studio's stimulant gives 0.05).</summary>
    public const float BigWeaponMissingComfort = 0.06f;

    /// <summary>Comfort lost without a small weapon: half the big one's.</summary>
    public const float SmallWeaponMissingComfort = 0.03f;

    /// <summary>Comfort lost without armour, once <see cref="ArmourMissingDays"/> have passed.</summary>
    public const float ArmourMissingComfort = 0.05f;

    public const float ArmourMissingDays = 5f;

    /// <summary>TierType.Index of the security policy from which fighters want armour: 2, "medium".</summary>
    public const int ArmourTierIndex = 2;

    public const int CoverageWhenQuiet = 1;

    public const int CoverageUnderThreat = 3;

    /// <summary>How near the camp's centre a threat has to be to count, in tiles.</summary>
    public const float ThreatNearTiles = 30f;

    public const float KitHitChance = 1.15f;

    public const float KitAttackSpeed = 1.1f;

    public const float KitOtherWork = 0.85f;

    public const float KitDefenseRating = 1.5f;

    /// <summary>Game time (seconds) since which this fighter has been expected to wear armour and has not.</summary>
    public const string NoArmourSinceField = "hidegear:noArmourSince";

    /// <summary>Set by the player's "take off armour" order; 1 while it stands.</summary>
    public const string RefusesArmourField = "hidegear:refusesArmour";

    public enum WeaponClass { None, Big, Small }

    /// <summary>
    /// Big: anything that shoots (an attack that uses ammunition - guns, the bow) or a spear.
    /// Small: a knife, a machete, a hand axe. Tools that happen to be weapons - pickaxe, hoe, spade,
    /// hammer - are neither, and nor is anything that is not a weapon.
    /// </summary>
    public static WeaponClass ClassOf(EntityType type)
    {
        WeaponType weapon = type?.ItemType?.WeaponType;
        if (weapon == null)
        {
            return WeaponClass.None;
        }
        string key = type.KeyName;
        bool shoots = weapon.AttackTypes != null && System.Array.Exists(weapon.AttackTypes, a => a != null && a.UsesAmmo != null);
        if (shoots || key.Contains("Spear"))
        {
            return WeaponClass.Big;
        }
        if (key.Contains("Knife") || key.Contains("Machete") || key.Contains("HandAxe"))
        {
            return WeaponClass.Small;
        }
        return WeaponClass.None;
    }

    /// <summary>What a colonist carries, anywhere on them: haul storage, Equipment, in hand.</summary>
    public static void Kit(Entity colonist, out bool big, out bool small, out bool armour)
    {
        big = small = armour = false;
        AgentStorage storage = colonist?.AgentStorage;
        if (storage == null)
        {
            return;
        }
        foreach (ItemStorage part in new ItemStorage[2] { storage.ItemStorage, storage.Equipment })
        {
            if (part == null)
            {
                continue;
            }
            foreach (EntityID id in part.StoredItems)
            {
                Entity item = Entity.FindByID(id);
                if (item == null)
                {
                    continue;
                }
                WeaponClass c = ClassOf(item.EntityType);
                big |= c == WeaponClass.Big;
                small |= c == WeaponClass.Small;
                armour |= item.EntityType.KeyName == ArmourKey;
            }
        }
    }

    private static bool FightersOn => Enabled.On && FighterKitSetting.On;

    /// <summary>Whether this expedition's security policy is at the tier where fighters wear armour.</summary>
    public static bool ArmourExpected(Expedition expedition)
    {
        return expedition?.Policy?.CurrentTiers != null
            && expedition.Policy.CurrentTiers.TryGetValue(RatingTypes.Security, out UWGame.SimSide.Tiers.TierType tier)
            && tier != null && tier.Index >= ArmourTierIndex;
    }

    /// <summary>A fighter with everything their expedition expects: both weapons, and armour if the policy wants it.</summary>
    public static bool KitComplete(Entity fighter)
    {
        Kit(fighter, out bool big, out bool small, out bool armour);
        return big && small && (armour || RefusesArmour(fighter) || !ArmourExpected(fighter.Intelligence?.CurrentExpedition));
    }

    /// <summary>Both weapons and armour on: what the buffs and the work penalty need.</summary>
    public static bool KittedOut(Entity colonist)
    {
        if (!FightersOn || !IsFighter(colonist))
        {
            return false;
        }
        Kit(colonist, out bool big, out bool small, out bool armour);
        return big && small && armour;
    }

    /// <summary>Whether a threat the allegiance knows of is within <see cref="ThreatNearTiles"/> of the camp.</summary>
    public static bool ThreatNear(Entity colonist, Expedition expedition)
    {
        var knowledge = colonist?.Intelligence?.Allegiance?.SharedKnowledge;
        Vector3? centre = expedition?.Center;
        if (knowledge?.AllKnownEntities?.ThreatJobs == null || !centre.HasValue)
        {
            return false;
        }
        float reach = ThreatNearTiles * MapManager.tileSize;
        foreach (var job in knowledge.AllKnownEntities.ThreatJobs)
        {
            if (job is ThreatJob threat && threat.Target.HasValue
                && !GoalEvaluator.EntityDataResultCausesSkip(knowledge.GetKnownData(threat.Target.Value, out IKnownEntityData target))
                && Vector3.DistanceSquared(target.PlaySiteLocation, centre.Value) <= reach * reach)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>How many kitted fighters the camp needs: 1, or 3 with a threat near.</summary>
    public static int CoverageNeeded(bool threatNear) => threatNear ? CoverageUnderThreat : CoverageWhenQuiet;

    /// <summary>Whether enough of this colonist's expedition's fighters have a complete kit.</summary>
    public static bool CoverageMet(Entity colonist)
    {
        Expedition expedition = colonist?.Intelligence?.CurrentExpedition;
        if (expedition == null)
        {
            return true;
        }
        int kitted = 0;
        foreach (EntityID id in expedition.Members)
        {
            Entity member = Entity.FindByID(id);
            if (member != null && IsFighter(member) && KitComplete(member))
            {
                kitted++;
            }
        }
        return kitted >= CoverageNeeded(ThreatNear(colonist, expedition));
    }

    /// <summary>Whether this colonist is a fighter who should be completing their kit now.</summary>
    public static bool Pressed(Entity colonist)
    {
        return FightersOn && IsFighter(colonist) && !KitComplete(colonist) && !CoverageMet(colonist);
    }

    private static float? Number(Entity colonist, string key)
    {
        return colonist?.CustomFields != null && colonist.CustomFields.TryGetValue(key, out PropertyResult r) ? r.NumberResult : null;
    }

    public static bool RefusesArmour(Entity colonist) => Number(colonist, RefusesArmourField) == 1f;

    /// <summary>
    /// Called from CompositeGoal.FindOptionalEquipmentIfNeeded: makes a pressed fighter look for
    /// the weapon class they lack and, at the armour tier, for armour (PatrolOrAttack gear), and
    /// keeps the "no armour since" stamp - simulation side, so the comfort getter only reads it.
    /// </summary>
    public static void FighterKit(Entity colonist, ref bool equipWeapon, ref List<ItemType.TaskType> gearTasks)
    {
        if (!FightersOn || !IsFighter(colonist))
        {
            return;
        }
        Kit(colonist, out bool big, out bool small, out bool armour);
        bool expected = ArmourExpected(colonist.Intelligence?.CurrentExpedition) && !RefusesArmour(colonist);
        double now = The.Sim.TotalUnPausedGameTimeInSeconds;
        if (expected && !armour)
        {
            if (!Number(colonist, NoArmourSinceField).HasValue)
            {
                Entity.SetPropertyValue(ref colonist.CustomFields, NoArmourSinceField, new PropertyResult { NumberResult = (float)now });
            }
        }
        else if (Number(colonist, NoArmourSinceField).HasValue)
        {
            Entity.SetPropertyValue(ref colonist.CustomFields, NoArmourSinceField, null);
        }
        if (CoverageMet(colonist))
        {
            return;
        }
        if (!big || !small)
        {
            equipWeapon = true;
        }
        if (expected && !armour)
        {
            AddPatrolGear(ref gearTasks);
        }
    }

    /// <summary>Whether a pressed fighter wants weapons at any distance from camp.</summary>
    public static bool WantsWeapon(Entity colonist)
    {
        if (!Pressed(colonist))
        {
            return false;
        }
        Kit(colonist, out bool big, out bool small, out _);
        return !big || !small;
    }

    /// <summary>
    /// Called from CompositeGoal.IsWeaponForEquipment: a pressed fighter only looks at the class
    /// of weapon they lack. Everyone else sees every weapon, as in the studio's game.
    /// </summary>
    public static bool WeaponWanted(Entity colonist, EntityType weapon)
    {
        if (!Pressed(colonist))
        {
            return true;
        }
        Kit(colonist, out bool big, out bool small, out _);
        WeaponClass c = ClassOf(weapon);
        return (c == WeaponClass.Big && !big) || (c == WeaponClass.Small && !small);
    }

    /// <summary>The comfort a pressed fighter loses for what their kit lacks (the arithmetic of ComfortPenalty).</summary>
    public static float PenaltyFor(bool big, bool small, bool armourOverdue)
    {
        return (big ? 0f : BigWeaponMissingComfort) + (small ? 0f : SmallWeaponMissingComfort) + (armourOverdue ? ArmourMissingComfort : 0f);
    }

    /// <summary>
    /// Called from the Intelligence.Comfort getter. Reads only - it is reached from the interface as
    /// well as the simulation.
    /// </summary>
    public static float ComfortPenalty(Entity colonist)
    {
        if (!Pressed(colonist))
        {
            return 0f;
        }
        Kit(colonist, out bool big, out bool small, out bool armour);
        float? since = Number(colonist, NoArmourSinceField);
        bool overdue = !armour && since.HasValue
            && The.Sim.TotalUnPausedGameTimeInSeconds - since.Value >= ArmourMissingDays * UWGame.SimSide.DateAndTime.secondsPerDay;
        return PenaltyFor(big, small, overdue);
    }

    /// <summary>Called from GoalDoAttack.ComputeChanceToHit, beside DangerousFaunaMod's.</summary>
    public static float HitChanceFactor(Entity attacker) => KittedOut(attacker) ? KitHitChance : 1f;

    /// <summary>Called from GoalDoAttack.AttackSpeed, beside DangerousFaunaMod's.</summary>
    public static float AttackSpeedFactor(Entity attacker) => KittedOut(attacker) ? KitAttackSpeed : 1f;

    /// <summary>Whether a skill is a fighter's own work: the combat skills and hunting.</summary>
    public static bool FightersWork(SkillType skill)
    {
        return skill == null || skill.KeyName == "hunting" || skill.Category?.KeyName == "combat";
    }

    /// <summary>Called from Intelligence.GetSkillProductionFactor: a kitted fighter works slower at anything else.</summary>
    public static float WorkFactor(Entity worker, SkillType skill)
    {
        return !FightersWork(skill) && KittedOut(worker) ? KitOtherWork : 1f;
    }

    /// <summary>Called from SecurityStatisticsForAllegiance.GetAgentDefenseRating: a kitted fighter counts for more.</summary>
    public static float DefenseFactor(Entity agent) => KittedOut(agent) ? KitDefenseRating : 1f;

    /// <summary>
    /// Called from CompositeGoal.FindOptionalEquipmentIfNeeded: the armour a colonist ordered out
    /// of it is still wearing, for that goal to put down; null otherwise.
    /// </summary>
    public static EntityID? ArmourToTakeOff(Entity colonist)
    {
        if (!Enabled.On || !RefusesArmour(colonist) || colonist.AgentStorage?.Equipment == null)
        {
            return null;
        }
        foreach (EntityID id in colonist.AgentStorage.Equipment.StoredItems)
        {
            Entity item = Entity.FindByID(id);
            if (item != null && item.EntityType.KeyName == ArmourKey)
            {
                return id;
            }
        }
        return null;
    }

    /// <summary>Executed by the SetRefusesArmour command: the player's "take off armour" order, or its reversal.</summary>
    public static void SetRefusesArmour(Entity colonist, bool refuses)
    {
        if (!Enabled.On || colonist == null)
        {
            return;
        }
        Entity.SetPropertyValue(ref colonist.CustomFields, RefusesArmourField, refuses ? new PropertyResult { NumberResult = 1f } : (PropertyResult?)null);
    }

    /// <summary>Whether the context menu offers the armour order for this entity: one of our fighters.</summary>
    public static bool HasArmourOrder(IKnownEntityData data)
    {
        return Enabled.On && data is Entity colonist && IsFighter(colonist);
    }
}
