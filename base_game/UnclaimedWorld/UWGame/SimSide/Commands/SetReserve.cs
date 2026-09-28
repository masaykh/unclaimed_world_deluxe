using UWGame.Control.Commands;
using UWGame.Mods;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

/// <summary>
/// Sets how many of one item an expedition keeps back from eating and production (ReserveMod).
///
/// A command rather than a direct write so that replays carry it, like SetStandingOrder. In a
/// build without the mod it does nothing - ReserveMod.Absent's SetReserve is empty.
/// </summary>
public class SetReserve : Command
{
	public long ExpeditionID;

	public string EntityTypeKey;

	public int Amount;

	public SetReserve()
	{
	}

	public SetReserve(ExpeditionID expeditionID, string entityTypeKey, int amount)
	{
		ExpeditionID = (long)expeditionID;
		EntityTypeKey = entityTypeKey;
		Amount = amount;
	}

	public override void Execute(bool giveClientFeedback)
	{
		Expedition expedition = LookUp<Expedition, UWGame.SimSide.Expeditions.ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);
		if (expedition != null && GameData.Instance.AllEntityTypes.TryGetValue(EntityTypeKey, out EntityType entityType))
		{
			ReserveMod.SetReserve(expedition, entityType, Amount);
		}
	}
}
