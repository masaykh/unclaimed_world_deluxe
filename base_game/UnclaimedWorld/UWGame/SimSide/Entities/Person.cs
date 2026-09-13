using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.SimSide.Entities;

public class Person : Component, IHasEntityGroup, ILookUp<IHasEntityGroup, HasEntityGroupID>, IOwner, ILookUp<IOwner, OwnerID>
{
	private bool portraitRectIsDirty = true;

	public Rectangle portraitRectStatus;

	private Rectangle portraitRectEntityPanel;

	public int PortraitFlavour = 1;

	public Personality Personality;

	private decimal? tradeCredits = default(decimal);

	public Household Household;

	private HouseholdID? snapshotHousehold;

	private EntityGroup ownedEntities;

	private EntityGroupID snapshotOwnedEntities;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private HasEntityGroupID hasEntityGroupID;

	private OwnerID ownerID;

	public decimal? TradeCredits
	{
		get
		{
			return tradeCredits;
		}
		set
		{
			tradeCredits = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public Allegiance Allegiance => Parent.Intelligence.Allegiance;

	public EntityGroup OwnedEntities => ownedEntities;

	public Vector3? Location => Parent.Location;

	public int NoOfWorkers => 1;

	HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.ID => hasEntityGroupID;

	OwnerID ILookUp<IOwner, OwnerID>.ID => ownerID;

	public bool IsEatable(EntityType entityType)
	{
		if (Parent.EntityType.BiologicalType != null)
		{
			Parent.Find<BiologicalEntity>(out var c);
			if (c.IsEatable(entityType))
			{
				return true;
			}
		}
		return false;
	}

	public Rectangle GetPortraitForTalkDisplay(GUIManager gui)
	{
		Rectangle portraitForStatusDisplay = GetPortraitForStatusDisplay(gui);
		portraitForStatusDisplay.Inflate(-6, -6);
		return portraitForStatusDisplay;
	}

	public Rectangle GetPortraitForStatusDisplay(GUIManager gui)
	{
		if (portraitRectIsDirty)
		{
			UpdatePortrait(gui);
		}
		return portraitRectStatus;
	}

	public void UpdatePortrait(GUIManager gui, string portrait)
	{
		portraitRectStatus = gui.GUI_CRT_SpriteSheet.GetSourceRectangle(portrait);
		portraitRectEntityPanel = portraitRectStatus;
		portraitRectEntityPanel.X = portraitRectStatus.X + 95;
		portraitRectEntityPanel.Width = portraitRectStatus.Width - 106;
		portraitRectIsDirty = false;
	}

	public string ComposePortraitKey()
	{
		return $"human_{MapRaceToPortraitRaces(Parent.BiologicalEntity.RaceType)}_{MapCasteToPortraitSex(Parent.BiologicalEntity.CasteType)}_{MapAgeGroupToPortraitAge(Parent.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup)}_{PortraitFlavour}";
	}

	private void UpdatePortrait(GUIManager gui)
	{
		if (!GetPortraitRectangle(gui))
		{
			PortraitFlavour = 1;
			if (!GetPortraitRectangle(gui))
			{
				GetDefaultPortrait(gui);
			}
		}
		portraitRectEntityPanel = portraitRectStatus;
		portraitRectEntityPanel.X = portraitRectStatus.X + 95;
		portraitRectEntityPanel.Width = portraitRectStatus.Width - 106;
		portraitRectIsDirty = false;
	}

	private bool GetPortraitRectangle(GUIManager gui)
	{
		string text = ComposePortraitKey();
		if (gui.GUI_CRT_SpriteSheet.spriteNames.ContainsKey(text))
		{
			portraitRectStatus = gui.GUI_CRT_SpriteSheet.GetSourceRectangle(text);
			return true;
		}
		return false;
	}

	private void GetDefaultPortrait(GUIManager gui)
	{
		if (MapCasteToPortraitSex(Parent.BiologicalEntity.CasteType) == "m")
		{
			portraitRectStatus = gui.GUI_CRT_SpriteSheet.GetSourceRectangle("human_w_m_adult_1");
		}
		else
		{
			portraitRectStatus = gui.GUI_CRT_SpriteSheet.GetSourceRectangle("human_b_f_adult_1");
		}
	}

	public Rectangle GetPortraitForEntityPanel(GUIManager gui)
	{
		if (portraitRectIsDirty)
		{
			UpdatePortrait(gui);
		}
		return portraitRectEntityPanel;
	}

	public Person(Entity parent)
		: base(parent, GameData.Instance.Constants.UpdateIntervalForEntityComponents)
	{
		((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).AddToLookup();
		((ILookUp<IOwner, OwnerID>)this).AddToLookup();
		ownedEntities = new EntityGroup(this, manageTrade: true, manageProduction: true);
		ownedEntities.HaulingJobManager = new HaulingJobManager(OwnedEntities);
		ownedEntities.OtherJobManager = new OtherJobManager(OwnedEntities);
	}

	public void NotifyFoodProcessesChanged()
	{
		if (Household != null)
		{
			Household.FoodExtraction.SetIsDirty();
		}
	}

	public Person()
	{
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		hasEntityGroupID = sn.DoEnum(hasEntityGroupID);
		ownerID = sn.DoEnum(ownerID);
		snapshotHousehold = sn.SnapshotID<Household, HouseholdID>(Household);
		snapshotOwnedEntities = sn.SnapshotID<EntityGroup, EntityGroupID>(ownedEntities).Value;
		Personality = (Personality)sn.DoISnapshot(Personality);
		tradeCredits = sn.DoDecimalNullable(tradeCredits);
		portraitRectEntityPanel = sn.DoRectangle(portraitRectEntityPanel);
		portraitRectIsDirty = sn.DoBool(portraitRectIsDirty);
		portraitRectStatus = sn.DoRectangle(portraitRectStatus);
		PortraitFlavour = sn.DoInt32(PortraitFlavour);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (snapshotHousehold.HasValue)
		{
			Household = LookUp<Household, HouseholdID>.FindByID(snapshotHousehold.Value);
		}
		ownedEntities = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnedEntities);
		Personality.Parent = Parent;
		Personality.LoadPostProcess(sn);
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void Initialize()
	{
		if (Personality == null)
		{
			PersonalityType randomListMember = Common.GetRandomListMember(GameData.Instance.AllPersonalityTypes.Values.ToList(), The.Sim.GameplayRandomGenerator);
			Personality = new Personality(Parent, randomListMember);
		}
		Personality.Initialize();
	}

	public bool CanDrive()
	{
		AIAgeGroup aIAgeGroup = Parent.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup;
		if (aIAgeGroup != AIAgeGroup.Child && aIAgeGroup != AIAgeGroup.Baby)
		{
			return aIAgeGroup != AIAgeGroup.YoungAdult;
		}
		return false;
	}

	public override double? GetUpdateInterval()
	{
		return GameData.Instance.Constants.UpdateIntervalForEntityComponents;
	}

	protected override void UpdatePlaySiteRegulated(double? timeSinceLastUpdate)
	{
		base.UpdatePlaySiteRegulated(timeSinceLastUpdate);
		Personality.Update(timeSinceLastUpdate);
	}

	private string MapRaceToPortraitRaces(RaceType raceType)
	{
		return raceType.PortraitSkinType switch
		{
			"whitePortrait" => "w", 
			"hispanicPortrait" => "h", 
			"blackPortrait" => "b", 
			"asianPortrait" => "a", 
			_ => "w", 
		};
	}

	private string MapCasteToPortraitSex(CasteType casteType)
	{
		return casteType.Reproduction switch
		{
			Reproduction.Male => "m", 
			Reproduction.Female => "f", 
			_ => "m", 
		};
	}

	private string MapAgeGroupToPortraitAge(AIAgeGroup ageGroup)
	{
		return ageGroup switch
		{
			AIAgeGroup.Baby => "baby", 
			AIAgeGroup.Child => "child", 
			AIAgeGroup.YoungAdult => "youngadult", 
			AIAgeGroup.Adult => "adult", 
			AIAgeGroup.Old => "old", 
			_ => "adult", 
		};
	}

	public void UpdateAgeGroup()
	{
		portraitRectIsDirty = true;
	}

	private void AddHeirIfAlive(List<Entity> listOfHeirs, Entity potentialHeir)
	{
		if (EntityExistsAndAlive(potentialHeir) && !listOfHeirs.Contains(potentialHeir))
		{
			listOfHeirs.Add(potentialHeir);
		}
	}

	public bool IsHeadOfHousehold()
	{
		if (Household.HeadOfHousehold1 != Parent)
		{
			return Household.HeadOfHousehold2 == Parent;
		}
		return true;
	}

	public bool IsChild()
	{
		AIAgeGroup aIAgeGroup = Parent.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup;
		if (aIAgeGroup != AIAgeGroup.Baby && aIAgeGroup != AIAgeGroup.Child)
		{
			return aIAgeGroup == AIAgeGroup.YoungAdult;
		}
		return true;
	}

	private static bool EntityExistsAndAlive(Entity entity)
	{
		if (entity != null)
		{
			return !entity.IsDead;
		}
		return false;
	}

	public List<Entity> GetHeirs()
	{
		Entity thisEntity = Parent;
		Entity biologicalMother = Parent.BiologicalEntity.BiologicalMother;
		Entity biologicalFather = Parent.BiologicalEntity.BiologicalFather;
		BiologicalEntity biologicalEntity = Parent.BiologicalEntity;
		List<Entity> listOfHeirs = new List<Entity>();
		if (EntityExistsAndAlive(biologicalEntity.Mate))
		{
			listOfHeirs.Add(biologicalEntity.Mate);
		}
		else if (biologicalEntity.BiologicalChildren.Count > 0 || (IsHeadOfHousehold() && Household.NoOfChildren() > 0))
		{
			foreach (Entity biologicalChild in biologicalEntity.BiologicalChildren)
			{
				AddHeirIfAlive(listOfHeirs, biologicalChild);
			}
			if (IsHeadOfHousehold())
			{
				Household.IterateMembers(delegate(Entity member)
				{
					if (member.PersonEntity.IsChild() && member != thisEntity)
					{
						AddHeirIfAlive(listOfHeirs, member);
					}
				});
			}
		}
		else if (EntityExistsAndAlive(biologicalMother) || EntityExistsAndAlive(biologicalFather))
		{
			AddHeirIfAlive(listOfHeirs, biologicalMother);
			AddHeirIfAlive(listOfHeirs, biologicalFather);
		}
		else if ((biologicalMother != null && biologicalMother.BiologicalEntity.BiologicalChildren.Count > 0) || (biologicalFather != null && biologicalFather.BiologicalEntity.BiologicalChildren.Count > 0))
		{
			foreach (Entity biologicalChild2 in biologicalMother.BiologicalEntity.BiologicalChildren)
			{
				if (biologicalChild2 != thisEntity)
				{
					AddHeirIfAlive(listOfHeirs, biologicalChild2);
				}
			}
			foreach (Entity biologicalChild3 in biologicalFather.BiologicalEntity.BiologicalChildren)
			{
				if (biologicalChild3 != thisEntity)
				{
					AddHeirIfAlive(listOfHeirs, biologicalChild3);
				}
			}
		}
		else if (HasLiveGrandChildren())
		{
			foreach (Entity biologicalChild4 in thisEntity.BiologicalEntity.BiologicalChildren)
			{
				foreach (Entity biologicalChild5 in biologicalChild4.BiologicalEntity.BiologicalChildren)
				{
					AddHeirIfAlive(listOfHeirs, biologicalChild5);
				}
			}
		}
		else if (HasLiveGrandParents())
		{
			if (biologicalMother != null)
			{
				AddHeirIfAlive(listOfHeirs, biologicalMother.BiologicalEntity.BiologicalMother);
				AddHeirIfAlive(listOfHeirs, biologicalMother.BiologicalEntity.BiologicalFather);
			}
			if (biologicalFather != null)
			{
				AddHeirIfAlive(listOfHeirs, biologicalFather.BiologicalEntity.BiologicalMother);
				AddHeirIfAlive(listOfHeirs, biologicalFather.BiologicalEntity.BiologicalFather);
			}
		}
		else if (HasLiveNiecesOrNephews())
		{
			if (biologicalMother != null)
			{
				foreach (Entity biologicalChild6 in biologicalMother.BiologicalEntity.BiologicalChildren)
				{
					if (biologicalChild6 == thisEntity)
					{
						continue;
					}
					foreach (Entity biologicalChild7 in biologicalChild6.BiologicalEntity.BiologicalChildren)
					{
						AddHeirIfAlive(listOfHeirs, biologicalChild7);
					}
				}
			}
			if (biologicalFather != null)
			{
				foreach (Entity biologicalChild8 in biologicalFather.BiologicalEntity.BiologicalChildren)
				{
					if (biologicalChild8 == thisEntity)
					{
						continue;
					}
					foreach (Entity biologicalChild9 in biologicalChild8.BiologicalEntity.BiologicalChildren)
					{
						AddHeirIfAlive(listOfHeirs, biologicalChild9);
					}
				}
			}
		}
		return listOfHeirs;
	}

	private bool HasLiveGrandParents()
	{
		Entity biologicalMother = Parent.BiologicalEntity.BiologicalMother;
		Entity biologicalFather = Parent.BiologicalEntity.BiologicalFather;
		if ((biologicalMother == null || !EntityExistsAndAlive(biologicalMother.BiologicalEntity.BiologicalMother)) && (biologicalMother == null || !EntityExistsAndAlive(biologicalMother.BiologicalEntity.BiologicalFather)) && (biologicalFather == null || !EntityExistsAndAlive(biologicalFather.BiologicalEntity.BiologicalMother)))
		{
			if (biologicalMother != null)
			{
				return EntityExistsAndAlive(biologicalFather.BiologicalEntity.BiologicalFather);
			}
			return false;
		}
		return true;
	}

	private bool HasLiveNiecesOrNephews()
	{
		Entity biologicalMother = Parent.BiologicalEntity.BiologicalMother;
		Entity biologicalFather = Parent.BiologicalEntity.BiologicalFather;
		Entity parent = Parent;
		if (biologicalMother != null)
		{
			foreach (Entity biologicalChild in biologicalMother.BiologicalEntity.BiologicalChildren)
			{
				if (biologicalChild == parent)
				{
					continue;
				}
				foreach (Entity biologicalChild2 in biologicalChild.BiologicalEntity.BiologicalChildren)
				{
					if (EntityExistsAndAlive(biologicalChild2))
					{
						return true;
					}
				}
			}
		}
		else if (biologicalFather != null)
		{
			foreach (Entity biologicalChild3 in biologicalFather.BiologicalEntity.BiologicalChildren)
			{
				if (biologicalChild3 == parent)
				{
					continue;
				}
				foreach (Entity biologicalChild4 in biologicalChild3.BiologicalEntity.BiologicalChildren)
				{
					if (EntityExistsAndAlive(biologicalChild4))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool HasLiveGrandChildren()
	{
		foreach (Entity biologicalChild in Parent.BiologicalEntity.BiologicalChildren)
		{
			foreach (Entity biologicalChild2 in biologicalChild.BiologicalEntity.BiologicalChildren)
			{
				if (EntityExistsAndAlive(biologicalChild2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Destroy()
	{
		DistributeBelongingsOnDeath();
		ownedEntities.Destroy();
		((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).RemoveIDEntry();
		((ILookUp<IOwner, OwnerID>)this).RemoveIDEntry();
		if (Household != null)
		{
			Household.RemoveMember(Parent);
		}
	}

	private void DistributeBelongingsOnDeath()
	{
		GetHeirsAsOwners(out var _, out var owners);
		DivideItems(ownedEntities, owners);
	}

	public void GetHeirsAsOwners(out List<Entity> heirs, out List<IOwner> owners)
	{
		heirs = GetHeirs();
		owners = new List<IOwner>();
		if (heirs.Count > 0)
		{
			foreach (Entity heir in heirs)
			{
				owners.Add(heir.PersonEntity);
			}
			return;
		}
		if (Parent.Intelligence.CurrentExpedition != null)
		{
			owners.Add(Parent.Intelligence.CurrentExpedition);
		}
	}

	public static void DivideItems(EntityGroup itemsToDivide, List<IOwner> newOwners)
	{
		int noOfPortions = newOwners.Count;
		int currentTurn = The.Sim.GameplayRandomGenerator.Next(0, noOfPortions, "DivideItems");
		itemsToDivide.IterateEntities(delegate(Entity e)
		{
			if (e.PartOf == null)
			{
				e.ChangeOwnership(newOwners[currentTurn]);
				currentTurn++;
				currentTurn %= noOfPortions;
			}
		});
	}

	HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.GetUniqueID()
	{
		return HasEntityGroup.GetUniqueID();
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.AddToLookup()
	{
		hasEntityGroupID = ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).GetUniqueID();
		if (hasEntityGroupID != HasEntityGroupID.Invalid)
		{
			LookUpHasEntityGroup.Add(hasEntityGroupID, this);
		}
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.RemoveIDEntry()
	{
		LookUpHasEntityGroup.Remove(this);
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.ResetIDCounter()
	{
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.CreateLookupCollection()
	{
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.SetInvalid()
	{
		hasEntityGroupID = HasEntityGroupID.Invalid;
	}

	OwnerID ILookUp<IOwner, OwnerID>.GetUniqueID()
	{
		return Owner.GetUniqueID();
	}

	void ILookUp<IOwner, OwnerID>.AddToLookup()
	{
		ownerID = ((ILookUp<IOwner, OwnerID>)this).GetUniqueID();
		if (ownerID != OwnerID.Invalid)
		{
			LookUpOwners.Add(ownerID, this);
		}
	}

	void ILookUp<IOwner, OwnerID>.RemoveIDEntry()
	{
		LookUpOwners.Remove(this);
	}

	void ILookUp<IOwner, OwnerID>.ResetIDCounter()
	{
	}

	void ILookUp<IOwner, OwnerID>.SetInvalid()
	{
		ownerID = OwnerID.Invalid;
	}

	void ILookUp<IOwner, OwnerID>.CreateLookupCollection()
	{
	}
}
