using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.InGameEvents.SpecialEvents;

public class UnhappinessGroupMeetingEvent : ISnapshot
{
	private EventActionDialog meetingEvent;

	private AgentCondition canParticipateInMeeting;

	private double? totalTimepointInSecondsOfLastMeeting;

	private Dictionary<ExpeditionID, double> timepointInSecondsOfLastMeeting;

	private Dictionary<RatingTypes, List<Entity>> UnhappyPersons = new Dictionary<RatingTypes, List<Entity>>();

	private List<Entity> MeetingParticipants = new List<Entity>();

	private Regulator updateRegulator;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public UnhappinessGroupMeetingEvent()
	{
		meetingEvent = new EventActionDialog();
		canParticipateInMeeting = GetMeetingParticipantConditions();
		if (!Snapshotter.IsSnapshotting)
		{
			timepointInSecondsOfLastMeeting = new Dictionary<ExpeditionID, double>();
			CreateRegulators();
		}
	}

	public static AgentCondition GetMeetingParticipantConditions()
	{
		return new AgentCondition
		{
			AllowEmigrating = false,
			AllowFighting = false,
			AllowTravelling = false,
			AllowSleeping = false,
			AllowThreatened = false,
			AllowUnconscious = false
		};
	}

	private void CreateRegulators()
	{
		updateRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.10000000149011612, "UnhappinessGroupMeetingEvent");
	}

	public void Update()
	{
		if (IsTimeForGroupMeeting(out var expeditionWithIssues))
		{
			ShowGroupMeeting(expeditionWithIssues);
		}
	}

	private bool IsTimeForGroupMeeting(out Expedition expeditionWithIssues)
	{
		if (updateRegulator.IsReady())
		{
			double firstTimepoint = GameData.Instance.GUIConstants.DefaultTimeInGameSecondsBeforeGroupMeeting;
			if (!totalTimepointInSecondsOfLastMeeting.HasValue)
			{
				PropertyResult? propertyValue = The.Sim.PlaySite.GetPropertyValue("timeInGameSecondsBeforeGroupMeeting", null);
				if (propertyValue.HasValue && propertyValue.Value.NumberResult.HasValue)
				{
					firstTimepoint = propertyValue.Value.NumberResult.Value;
				}
			}
			if (The.Sim.TimepointReached(totalTimepointInSecondsOfLastMeeting, firstTimepoint, GameData.Instance.GUIConstants.MinimumTimeInInGameSecondsBetweenGroupMeetings))
			{
				PropertyResult? propertyValue2 = The.Sim.PlaySite.GetPropertyValue("enableGroupMeetings", null);
				if (propertyValue2.HasValue && propertyValue2.Value.BoolResult == true)
				{
					expeditionWithIssues = GetExpeditionWithIssues();
					if (expeditionWithIssues != null)
					{
						return true;
					}
				}
			}
		}
		expeditionWithIssues = null;
		return false;
	}

	private Expedition GetExpeditionWithIssues()
	{
		List<Tuple<Expedition, double>> list = new List<Tuple<Expedition, double>>();
		foreach (Expedition expedition in The.Sim.PlaySite.PlayerAllegiance.Expeditions)
		{
			if (timepointInSecondsOfLastMeeting.TryGetValue(expedition.ID, out var value))
			{
				list.Add(new Tuple<Expedition, double>(expedition, value));
			}
			else
			{
				list.Add(new Tuple<Expedition, double>(expedition, double.MaxValue));
			}
		}
		if (timepointInSecondsOfLastMeeting.Count > The.Sim.PlaySite.PlayerAllegiance.Expeditions.Count)
		{
			Dictionary<ExpeditionID, double> dictionary = new Dictionary<ExpeditionID, double>();
			foreach (KeyValuePair<ExpeditionID, double> item in timepointInSecondsOfLastMeeting)
			{
				if (The.Sim.PlaySite.PlayerAllegiance.Expeditions.Exists((Expedition e) => e.ID == item.Key))
				{
					dictionary.Add(item.Key, item.Value);
				}
			}
			timepointInSecondsOfLastMeeting = dictionary;
		}
		foreach (Tuple<Expedition, double> item2 in list.OrderByDescending((Tuple<Expedition, double> t) => t.Item2))
		{
			if (ExpeditionHasIssues(item2.Item1))
			{
				return item2.Item1;
			}
		}
		return null;
	}

	private bool ExpeditionHasIssues(Expedition expedition)
	{
		int num = 0;
		int num2 = 0;
		List<EntityID> list = null;
		foreach (EntityID independentMember in expedition.IndependentMembers)
		{
			Entity entity = Entity.FindByID(independentMember);
			if (entity != null && entity.Intelligence.HasHappiness() && entity.PersonEntity.Personality.CanComplainProperty())
			{
				num2++;
				if (entity.PersonEntity.Personality.Happiness < GameData.Instance.AIConstants.Ratings.HappinessLimitForGroupMeeting && canParticipateInMeeting.IsFulfilled(entity))
				{
					num++;
				}
			}
			else
			{
				Common.AddToList(ref list, independentMember);
			}
		}
		if (list != null)
		{
			foreach (EntityID item in list)
			{
				expedition.RemoveMemberID(item);
			}
		}
		if ((float)num / (float)num2 >= GameData.Instance.AIConstants.Ratings.UnhappyExpeditionMembersPercentageForGroupMeeting)
		{
			return true;
		}
		return false;
	}

	private void ShowGroupMeeting(Expedition expedition)
	{
		GroupMembersByIssues(expedition);
		string text = ComposeMainText();
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = ComposeMinutesText();
			text += text2;
			meetingEvent.Heading = "GROUP MEETING";
			meetingEvent.DisplayText = new DynamicText();
			meetingEvent.DisplayText.Text = text;
			meetingEvent.DisplayImage = "GroupMeeting";
			string failReason = null;
			meetingEvent.Execute(null, ref failReason);
		}
		totalTimepointInSecondsOfLastMeeting = The.Sim.TotalUnPausedGameTimeInSeconds;
		timepointInSecondsOfLastMeeting[expedition.ID] = The.Sim.TotalUnPausedGameTimeInSeconds;
		UnhappyPersons.Clear();
		MeetingParticipants.Clear();
	}

	private void GroupMembersByIssues(Expedition expedition)
	{
		Array values = Enum.GetValues(typeof(RatingTypes));
		foreach (EntityID independentMember in expedition.IndependentMembers)
		{
			Entity entity = Entity.FindByID(independentMember);
			if (entity == null || !entity.Intelligence.HasHappiness() || !entity.PersonEntity.Personality.CanComplainProperty() || !canParticipateInMeeting.IsFulfilled(entity))
			{
				continue;
			}
			MeetingParticipants.Add(entity);
			foreach (object item in values)
			{
				if (entity.PersonEntity.Personality.ComputeHappinessComponent((RatingTypes)item) < GameData.Instance.AIConstants.Ratings.HappinessLimitForGroupMeeting)
				{
					Common.AddToMultiList(UnhappyPersons, (RatingTypes)item, entity);
				}
			}
		}
	}

	private string ComposeMinutesText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendLine(stringBuilder);
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "MINUTES");
		Common.AppendLine(stringBuilder);
		string value = "Complains about ";
		foreach (KeyValuePair<RatingTypes, List<Entity>> unhappyPerson in UnhappyPersons)
		{
			if (unhappyPerson.Value.Count <= 0)
			{
				continue;
			}
			stringBuilder.Append(value);
			Statistic.AppendRatingsTypeToStringAndIcon(stringBuilder, unhappyPerson.Key);
			stringBuilder.Append(" conditions: ");
			string value2 = "";
			foreach (Entity item in unhappyPerson.Value)
			{
				stringBuilder.Append(value2);
				stringBuilder.Append(SubstituteValue.FormatAllegianceMember(item));
				value2 = ", ";
			}
			value2 = "";
			bool flag = false;
			foreach (Entity item2 in unhappyPerson.Value)
			{
				if (item2.Intelligence.EmigrateDecider != null && item2.Intelligence.EmigrateDecider.CanEmigrateToAnyTarget() && item2.Intelligence.EmigrateDecider.MigrationRisk > 0f)
				{
					if (!flag)
					{
						Common.AppendLine(stringBuilder);
						stringBuilder.Append("Thinking of leaving for this reason: ");
					}
					Common.AppendLine(stringBuilder);
					stringBuilder.Append(SubstituteValue.FormatAllegianceMember(item2));
					Common.Append(stringBuilder, " ");
					Common.AppendPercentage(stringBuilder, item2.Intelligence.EmigrateDecider.MigrationRisk, useColoring: false, null);
					flag = true;
				}
			}
			Common.AppendLine(stringBuilder);
			Common.AppendLine(stringBuilder);
			value = "Wants improvements to ";
		}
		return stringBuilder.ToString();
	}

	private string ComposeMainText()
	{
		RatingTypes mainIssue = RatingTypes.Security;
		int num = 0;
		foreach (KeyValuePair<RatingTypes, List<Entity>> unhappyPerson in UnhappyPersons)
		{
			if (unhappyPerson.Value.Count > num)
			{
				num = unhappyPerson.Value.Count;
				mainIssue = unhappyPerson.Key;
			}
		}
		string text = "";
		Entity randomListMember = Common.GetRandomListMember(UnhappyPersons[mainIssue], The.Sim.GameplayRandomGenerator);
		if (randomListMember.Intelligence.EmigrateDecider.CanEmigrateToAnyTarget() && randomListMember.Intelligence.EmigrateDecider.PreferredMigrationTarget.HasValue && randomListMember.Intelligence.EmigrateDecider.MigrationRisk > 0f)
		{
			Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(randomListMember.Intelligence.EmigrateDecider.PreferredMigrationTarget.Value);
			if (allegiance != null)
			{
				text = allegiance.Site.Name;
			}
		}
		List<Entity> list = new List<Entity>(MeetingParticipants);
		list.RemoveAll((Entity p) => UnhappyPersons[mainIssue].Contains(p));
		Entity entity = null;
		PropertyResult? propertyValue;
		if (list.Count > 0)
		{
			entity = Common.GetRandomListMember(list, The.Sim.GameplayRandomGenerator);
			propertyValue = The.Sim.PlaySite.GetPropertyValue("meetingEmigrateThreat", null);
		}
		else
		{
			propertyValue = The.Sim.PlaySite.GetPropertyValue("meetingEmigrateThreatAllUnhappy", null);
		}
		string result = "";
		PropertyResult? content = null;
		if (randomListMember != null)
		{
			if (entity != null)
			{
				if (MeetingParticipants.Count == 2)
				{
					GetDialogTextProperties2People(mainIssue, out content);
				}
				if (!content.HasValue)
				{
					GetDialogTextProperties3OrMore(mainIssue, out content);
				}
			}
			else
			{
				GetDialogTextPropertiesAllAgree(mainIssue, out content);
			}
			if (content.HasValue)
			{
				result = content.Value.StringResult;
				result = result.Replace("#UNHAPPY", SubstituteValue.FormatAllegianceMember(randomListMember));
				if (entity != null)
				{
					result = result.Replace("#CONTENT", SubstituteValue.FormatAllegianceMember(entity));
				}
				if (!string.IsNullOrEmpty(text) && propertyValue.HasValue)
				{
					string stringResult = propertyValue.Value.StringResult;
					stringResult = stringResult.Replace("#EMIGRATETO", text);
					result = result.Replace("#EMIGRATETHREAT", stringResult);
				}
				else
				{
					result = result.Replace("#EMIGRATETHREAT", "");
				}
			}
		}
		return result;
	}

	private void GetDialogTextProperties2People(RatingTypes mainIssue, out PropertyResult? content)
	{
		content = null;
		switch (mainIssue)
		{
		case RatingTypes.Security:
			content = The.Sim.PlaySite.GetPropertyValue("meeting2Security", null);
			break;
		case RatingTypes.Comfort:
			content = The.Sim.PlaySite.GetPropertyValue("meeting2Comfort", null);
			break;
		case RatingTypes.Food:
			content = The.Sim.PlaySite.GetPropertyValue("meeting2Food", null);
			break;
		}
	}

	private void GetDialogTextProperties3OrMore(RatingTypes mainIssue, out PropertyResult? content)
	{
		content = null;
		switch (mainIssue)
		{
		case RatingTypes.Security:
			content = The.Sim.PlaySite.GetPropertyValue("meeting3Security", null);
			break;
		case RatingTypes.Comfort:
			content = The.Sim.PlaySite.GetPropertyValue("meeting3Comfort", null);
			break;
		case RatingTypes.Food:
			content = The.Sim.PlaySite.GetPropertyValue("meeting3Food", null);
			break;
		}
	}

	private void GetDialogTextPropertiesAllAgree(RatingTypes mainIssue, out PropertyResult? content)
	{
		content = null;
		switch (mainIssue)
		{
		case RatingTypes.Security:
			content = The.Sim.PlaySite.GetPropertyValue("meetingSecurityAllUnhappy", null);
			break;
		case RatingTypes.Comfort:
			content = The.Sim.PlaySite.GetPropertyValue("meetingComfortAllUnhappy", null);
			break;
		case RatingTypes.Food:
			content = The.Sim.PlaySite.GetPropertyValue("meetingFoodAllUnhappy", null);
			break;
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		timepointInSecondsOfLastMeeting = sn.DoDictionary(timepointInSecondsOfLastMeeting);
		totalTimepointInSecondsOfLastMeeting = sn.DoDoubleNullable(totalTimepointInSecondsOfLastMeeting);
		sn.Ignore(meetingEvent);
		sn.Ignore(MeetingParticipants);
		sn.Ignore(canParticipateInMeeting);
		sn.Ignore(UnhappyPersons);
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
		CreateRegulators();
	}
}
