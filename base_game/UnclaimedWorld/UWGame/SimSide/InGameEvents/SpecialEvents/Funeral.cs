using System.Collections.Generic;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.InGameEvents.SpecialEvents;

public class Funeral : ISnapshot
{
	private EventActionDialog funeralEvent;

	private AgentCondition canParticipateInFuneral;

	private List<PlayerEntityDeath> PlayerEntityDeaths = new List<PlayerEntityDeath>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Funeral()
	{
		funeralEvent = new EventActionDialog
		{
			DisplayText = new DynamicText
			{
				SubstitutionValues = new SubstituteValue[1]
				{
					new SubstituteValue
					{
						Placeholder = "#JOURNALNAMES",
						PropertyName = "getJournalHeaderNames"
					}
				}
			}
		};
		canParticipateInFuneral = UnhappinessGroupMeetingEvent.GetMeetingParticipantConditions();
	}

	public void PlayerEntityHasDied(Entity deadEntity, Entity carcassEntity, CauseOfDeath? causeOfDeath)
	{
		PlayerEntityDeaths.Add(new PlayerEntityDeath
		{
			EntityName = deadEntity.Name,
			Corpse = carcassEntity.EntityID,
			CauseOfDeath = causeOfDeath,
			HisHerIts = deadEntity.HisHerOrIts(),
			DisplayImageName = deadEntity.PersonEntity.ComposePortraitKey(),
			Profession = deadEntity.Intelligence.Profession
		});
	}

	public void Update()
	{
		HandlePlayerDeaths();
	}

	private string SubstituteBurialText(string text)
	{
		Entity randomPerson = The.Sim.PlaySite.PlayerAllegiance.GetRandomPerson((Entity e) => canParticipateInFuneral.IsFulfilled(e));
		string newValue = "";
		if (randomPerson != null)
		{
			newValue = SubstituteValue.FormatAllegianceMember(randomPerson);
		}
		string newValue2 = "";
		string newValue3;
		if (PlayerEntityDeaths.Count == 1)
		{
			PlayerEntityDeath playerEntityDeath = PlayerEntityDeaths[0];
			newValue3 = SubstituteValue.FormatEntity(playerEntityDeath.EntityName, playerEntityDeath.Profession, isInAllegiance: true);
			if (playerEntityDeath.CauseOfDeath.HasValue)
			{
				newValue2 = playerEntityDeath.CauseOfDeath.Value switch
				{
					CauseOfDeath.Starvation => " who perished from starvation", 
					CauseOfDeath.Wounds => " who died from " + playerEntityDeath.HisHerIts + " wounds", 
					_ => "", 
				};
			}
		}
		else
		{
			newValue3 = Common.ListToCommaSeparatedString(PlayerEntityDeaths, (PlayerEntityDeath e) => SubstituteValue.FormatEntity(e.EntityName, e.Profession, isInAllegiance: true));
		}
		text = text.Replace("#NAMEOFDECEASED", newValue3);
		text = text.Replace("#EUOLOGYGIVER", newValue);
		text = text.Replace("#CAUSEOFDEATH", newValue2);
		return text;
	}

	private string ComposeBurialEventMainText()
	{
		string text = "";
		PropertyResult? propertyValue = The.Sim.PlaySite.GetPropertyValue("includeDateInBurialHeader", null);
		PropertyResult? propertyValue2 = The.Sim.PlaySite.GetPropertyValue("burialTextStart", null);
		PropertyResult? propertyValue3 = The.Sim.PlaySite.GetPropertyValue("burialTextMultipleDeathsMultipleSurvivors", null);
		PropertyResult? propertyValue4 = The.Sim.PlaySite.GetPropertyValue("burialTextSingleDeathMultipleSurvivors", null);
		PropertyResult? propertyValue5 = The.Sim.PlaySite.GetPropertyValue("burialTextSingleDeathSingleSurvivor", null);
		PropertyResult? propertyValue6 = The.Sim.PlaySite.GetPropertyValue("burialTextMultipleDeathsSingleSurvivor", null);
		text = ((The.Sim.PlaySite.PlayerAllegiance.Members.Count > 1) ? ((PlayerEntityDeaths.Count != 1) ? GetBurialText(propertyValue3, propertyValue6) : GetBurialText(propertyValue4, propertyValue5)) : ((PlayerEntityDeaths.Count != 1) ? GetBurialText(propertyValue6, propertyValue3) : GetBurialText(propertyValue5, propertyValue4)));
		string text2 = "";
		if (propertyValue2.HasValue && !string.IsNullOrEmpty(propertyValue2.Value.StringResult))
		{
			if (propertyValue.HasValue && propertyValue.Value.BoolResult == true)
			{
				text2 = SubstituteValue.FormatDate(The.Sim.DateAndTime.CurrentTimeDateYear, FormattingOptions.BothDates) + " \n";
			}
			text2 += propertyValue2.Value.StringResult;
		}
		return SubstituteBurialText(text2 + text);
	}

	private string GetBurialText(PropertyResult? result, PropertyResult? alternativeResult)
	{
		if (result.HasValue && !string.IsNullOrEmpty(result.Value.StringResult))
		{
			return result.Value.StringResult;
		}
		if (alternativeResult.HasValue && !string.IsNullOrEmpty(alternativeResult.Value.StringResult))
		{
			return alternativeResult.Value.StringResult;
		}
		return "";
	}

	private void HandlePlayerDeaths()
	{
		if (funeralEvent == null || PlayerEntityDeaths.Count <= 0 || !OKToShowFuneral())
		{
			return;
		}
		string text = ComposeBurialEventMainText();
		if (text != null)
		{
			funeralEvent.DisplayText.Text = text;
			if (PlayerEntityDeaths.Count > 1)
			{
				funeralEvent.DisplayImage = "NightTime";
			}
			else if (PlayerEntityDeaths.Count == 1)
			{
				funeralEvent.DisplayImage = PlayerEntityDeaths[0].DisplayImageName;
			}
			string failReason = null;
			funeralEvent.Execute(null, ref failReason);
		}
		RemoveDeadPlayerEntities();
		The.Sim.PlaySite.SetPropertyValue("burialOccurred", new PropertyResult
		{
			BoolResult = true
		});
	}

	private void RemoveDeadPlayerEntities()
	{
		for (int i = 0; i < PlayerEntityDeaths.Count; i++)
		{
			EntityID corpse = PlayerEntityDeaths[i].Corpse;
			DetectableID? detectableID = null;
			Entity entity = Entity.FindByID(corpse);
			if (entity != null)
			{
				detectableID = entity.DetectableID;
				entity.Destroy();
			}
			The.Sim.PlaySite.PlayerAllegiance.SharedKnowledge.DeleteMemoryOfEntity(corpse, detectableID, removeAllKnowledge: true);
		}
		PlayerEntityDeaths.Clear();
	}

	private bool OKToShowFuneral()
	{
		if (The.Sim.PlaySite.PlayerAllegiance.IsUnderThreat())
		{
			return false;
		}
		return The.Sim.PlaySite.PlayerAllegiance.GetNoOfPersons((Entity e) => canParticipateInFuneral.IsFulfilled(e)) > 0;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		PlayerEntityDeaths = sn.DoList(PlayerEntityDeaths);
		sn.Ignore(funeralEvent);
		sn.Ignore(canParticipateInFuneral);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
