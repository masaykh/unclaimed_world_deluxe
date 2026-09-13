using Microsoft.Xna.Framework;
using UWGame.Control.Commands;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands;

public class PlaceExpedition : Command
{
	public Vector3 Location;

	public bool GiveClientFeedback;

	public long Expedition;

	public PlaceExpedition()
	{
	}

	public PlaceExpedition(ExpeditionID expedition, Vector3 location, bool giveClientFeedback)
	{
		Expedition = (long)expedition;
		Location = location;
		GiveClientFeedback = giveClientFeedback;
	}

	public override void Execute(bool giveClientFeedback)
	{
		bool flag = DoPlaceExpedition();
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnPlaceExpedition();
		}
	}

	private bool DoPlaceExpedition()
	{
		if (!The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], MapManager.WorldPosToSubtile(Location)))
		{
			The.Sim.PlaySite.GetFirstPlayerExpedition().Center = Location;
			The.Sim.PlaySite.GetFirstPlayerExpedition().RemoveGatheringSite();
			return true;
		}
		return false;
	}
}
