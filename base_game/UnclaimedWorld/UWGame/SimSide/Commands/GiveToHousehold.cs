using System.Xml.Serialization;
using System.Collections.Generic;
using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

/// <summary>
/// Hands items to a household: each becomes the household's, through Entity.ChangeOwnership, the
/// same call the studio uses whenever an item changes hands.
///
/// OwnershipMod's pay (rations at the end of the workday) goes through this, for the same reason as
/// SetHouseholdProduction: planners act through commands (the studio's rule in PhysicalNeedsPlanner,
/// and tripleacoder's request). An item that no longer exists, or is gone from the play site, is
/// skipped.
/// </summary>
public class GiveToHousehold : Command
{
	public ulong HouseholdID;

	public List<long> EntityIDs = new List<long>();

	public GiveToHousehold()
	{
	}

	public GiveToHousehold(UWGame.SimSide.Entities.HouseholdID householdID, IEnumerable<EntityID> items)
	{
		HouseholdID = (ulong)householdID;
		foreach (EntityID id in items)
		{
			EntityIDs.Add((long)id);
		}
	}

	/// <summary>How many items the last Execute handed over.</summary>
	[XmlIgnore]
	public int Given { get; private set; }

	public override void Execute(bool giveClientFeedback)
	{
		Given = 0;
		Household household = LookUp<Household, UWGame.SimSide.Entities.HouseholdID>.FindByID((UWGame.SimSide.Entities.HouseholdID)HouseholdID);
		if (household == null)
		{
			return;
		}
		foreach (long id in EntityIDs)
		{
			Entity item = Entity.FindByID((EntityID)id);
			if (item != null && item.IsOnPlaySite())
			{
				item.ChangeOwnership(household);
				Given++;
			}
		}
	}
}
