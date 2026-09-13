using UWGame.Control.Commands;
using UWGame.SimSide.Overland.Missions.Templates;

namespace UWGame.SimSide.Commands;

public class CreateMissionTemplate : Command
{
	public MissionTemplate MissionTemplate;

	public CreateMissionTemplate(MissionTemplate missionTemplate)
	{
		MissionTemplate = missionTemplate;
	}

	public CreateMissionTemplate()
	{
	}

	public override void Execute(bool giveClientFeedback)
	{
		MissionTemplate.AssignIDs();
	}
}
