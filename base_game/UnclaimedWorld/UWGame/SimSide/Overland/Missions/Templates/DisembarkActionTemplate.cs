using System.Collections.Generic;

namespace UWGame.SimSide.Overland.Missions.Templates;

public class DisembarkActionTemplate : MissionActionTemplate
{
	public override string Name => TemplateName;

	public static string TemplateName => "Disembark";

	public override ActionTypes ActionType => ActionTypes.Disembark;

	public DisembarkActionTemplate()
	{
	}

	public DisembarkActionTemplate(MissionStopTemplate missionStopTemplate, bool allowDeleting)
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
		return new DisembarkAction(mission, this);
	}
}
