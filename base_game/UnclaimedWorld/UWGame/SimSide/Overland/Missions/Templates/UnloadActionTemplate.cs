using System.Collections.Generic;

namespace UWGame.SimSide.Overland.Missions.Templates;

public class UnloadActionTemplate : MissionActionTemplate
{
	public override string Name => TemplateName;

	public static string TemplateName => "Unload";

	public override ActionTypes ActionType => ActionTypes.Unload;

	public UnloadActionTemplate()
	{
	}

	public UnloadActionTemplate(MissionStopTemplate missionStopTemplate, bool allowDeleting)
		: base(missionStopTemplate, allowDeleting)
	{
	}

	public override float ComputeTotalCargoBulk()
	{
		return 0f;
	}

	public override decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost)
	{
		boughtItemsCost = default(decimal);
		soldItemsCost = default(decimal);
		return 0m;
	}

	public override bool Validate(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors)
	{
		return true;
	}

	public override MissionAction CreateMissionAction(Mission mission)
	{
		return new UnloadAction(mission, this);
	}
}
