using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class AllowAmmoForVermin : Command
{
	public long ExpeditionID;

	public string AmmoType;

	public bool Allow;

	public AllowAmmoForVermin()
	{
	}

	public AllowAmmoForVermin(ExpeditionID expeditionID, EntityType ammo, bool value, bool giveClientFeedback)
	{
		ExpeditionID = (long)expeditionID;
		AmmoType = ammo.KeyName;
		Allow = value;
	}

	public override void Execute(bool giveClientFeedback)
	{
		Expedition expedition = LookUp<Expedition, UWGame.SimSide.Expeditions.ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);
		GameData.Instance.AllEntityTypes.TryGetValue(AmmoType, out var value);
		if (expedition != null && value != null)
		{
			expedition.Policy.SetAllowAmmoForVermin(value, Allow);
		}
	}
}
