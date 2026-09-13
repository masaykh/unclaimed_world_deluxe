using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions.Templates;

public class EmbarkActionTemplate : MissionActionTemplate
{
	public PassengerListTemplate PassengerListTemplate;

	public override string Name => TemplateName;

	public static string TemplateName => "Embark";

	public override ActionTypes ActionType => ActionTypes.Embark;

	public EmbarkActionTemplate()
	{
	}

	public EmbarkActionTemplate(MissionStopTemplate missionStopTemplate, List<EntityID> passengers, bool allowDeleting)
		: base(missionStopTemplate, allowDeleting)
	{
		PassengerListTemplate = new PassengerListTemplate
		{
			Passengers = passengers.Select((EntityID p) => (long)p).ToList(),
			StartingLocation = MissionStopTemplate.TravelLocation
		};
	}

	public static bool ValidateEmbarkTerminal(ref List<string> errors, IKnownEntityData terminalData)
	{
		if (!MissionActionTemplate.ValidateWorkingTerminal(terminalData, ref errors))
		{
			return false;
		}
		return true;
	}

	public static bool ValidateEmbark(Allegiance thisAllegiance, IKnownEntityData terminal, Allegiance allegiance, ref List<string> errors)
	{
		if (!ValidateEmbarkTerminal(ref errors, terminal))
		{
			return false;
		}
		if (thisAllegiance == allegiance)
		{
			Common.AddToList(ref errors, "For now, we can only embark passengers at other sites.");
			return false;
		}
		return true;
	}

	public override bool Validate(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors)
	{
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
		if (allegiance == null || !MissionStopTemplate.TravelLocation.ResolveLocation(allegiance.SharedKnowledge, out var _, out var allegiance2, out var _, out var terminalData))
		{
			return false;
		}
		if (!ValidateEmbark(allegiance, terminalData, allegiance2, ref errors))
		{
			return false;
		}
		if (PassengerListTemplate != null && PassengerListTemplate.Passengers.Count > 0)
		{
			hasMeaning = true;
		}
		return true;
	}

	public override MissionAction CreateMissionAction(Mission mission)
	{
		return new EmbarkAction(mission, this);
	}

	public override float ComputeTotalCargoBulk()
	{
		float num = 0f;
		if (PassengerListTemplate != null)
		{
			foreach (long passenger in PassengerListTemplate.Passengers)
			{
				Entity entity = Entity.FindByID((EntityID)passenger);
				num = ((entity == null) ? (num + 1f) : (num + entity.Bulk));
			}
		}
		return num;
	}

	public override decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost)
	{
		decimal result = default(decimal);
		soldItemsCost = default(decimal);
		boughtItemsCost = default(decimal);
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
		if (!MissionStopTemplate.TravelLocation.ResolveLocation(allegiance.SharedKnowledge, out var _, out var _, out var _, out var _))
		{
			return 0m;
		}
		return result;
	}

	public override void AssignIDs()
	{
		base.AssignIDs();
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		PassengerListTemplate = (PassengerListTemplate)sn.DoISnapshot(PassengerListTemplate);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
	}
}
