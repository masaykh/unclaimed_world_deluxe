using UWGame.Control.Commands;
using UWGame.Mods;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Commands;

/// <summary>
/// The player's order to a fighter to take their armour off - to pass it to a stronger fighter or
/// to undress an injured one - or to reverse it (HideGearMod), from the colonist's context menu.
///
/// A command rather than a direct write so that replays carry it, like SetAutoclaimCampKills. In a
/// build without the mod it does nothing - HideGearMod.Absent's SetRefusesArmour is empty.
/// </summary>
public class SetRefusesArmour : Command
{
	public long EntityID;

	public bool Refuses;

	public SetRefusesArmour()
	{
	}

	public SetRefusesArmour(EntityID entityID, bool refuses)
	{
		EntityID = (long)entityID;
		Refuses = refuses;
	}

	public override void Execute(bool giveClientFeedback)
	{
		Entity colonist = Entity.FindByID((UWGame.SimSide.Entities.EntityID)EntityID);
		if (colonist != null)
		{
			HideGearMod.SetRefusesArmour(colonist, Refuses);
		}
	}
}
