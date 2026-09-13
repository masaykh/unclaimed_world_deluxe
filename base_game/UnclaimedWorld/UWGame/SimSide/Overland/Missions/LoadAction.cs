using Microsoft.Xna.Framework;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public class LoadAction : MissionAction
{
	private const double timeInDaysToLoad = 0.1;

	private double elapsedTime;

	public LoadActionTemplate LoadActionType;

	private MissionActionTemplateID snapshotActionTemplateID;

	public LoadAction()
	{
	}

	public LoadAction(Mission parent, LoadActionTemplate template)
		: base(parent)
	{
		LoadActionType = template;
	}

	public override bool Update(GameTime elapsed)
	{
		base.Update(elapsed);
		elapsedTime += The.Sim.DateAndTime.DaysPerSecond * elapsed.ElapsedGameTime.TotalSeconds;
		if (elapsedTime > 0.1)
		{
			return true;
		}
		return false;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotActionTemplateID = sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(LoadActionType).Value;
		return base.DoSnapshot(sn);
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		LoadActionType = (LoadActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);
		base.LoadPostProcess(sn);
	}
}
