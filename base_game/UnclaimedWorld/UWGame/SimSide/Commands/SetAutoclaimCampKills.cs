using UWGame.Control.Commands;
using UWGame.Mods;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

/// <summary>
/// Switches an expedition's "claim animals that die in camp" policy (HuntingMod), from the
/// policy window's weapons page.
///
/// A command rather than a direct write so that replays carry it, like AllowAmmoForVermin beside
/// it. In a build without the mod it does nothing - HuntingMod.Absent's SetAutoclaimCampKills is
/// empty.
/// </summary>
public class SetAutoclaimCampKills : Command
{
	public long ExpeditionID;

	public bool Claim;

	public SetAutoclaimCampKills()
	{
	}

	public SetAutoclaimCampKills(ExpeditionID expeditionID, bool claim)
	{
		ExpeditionID = (long)expeditionID;
		Claim = claim;
	}

	public override void Execute(bool giveClientFeedback)
	{
		Expedition expedition = LookUp<Expedition, UWGame.SimSide.Expeditions.ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);
		if (expedition != null)
		{
			HuntingMod.SetAutoclaimCampKills(expedition, Claim);
		}
	}
}
