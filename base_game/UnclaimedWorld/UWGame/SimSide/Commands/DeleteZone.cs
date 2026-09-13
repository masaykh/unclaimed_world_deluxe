using UWGame.Control.Commands;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class DeleteZone : Command
{
	public long zoneID;

	public DeleteZone()
	{
	}

	public DeleteZone(ZoneID zoneID)
	{
		this.zoneID = (long)zoneID;
	}

	public override void Execute(bool giveClientFeedback)
	{
		LookUp<Zone, ZoneID>.FindByID((ZoneID)zoneID).Destroy();
	}
}
