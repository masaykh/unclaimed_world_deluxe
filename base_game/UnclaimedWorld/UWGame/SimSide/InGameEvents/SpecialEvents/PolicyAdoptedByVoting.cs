using System.Collections.Generic;
using System.Text;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Tiers;

namespace UWGame.SimSide.InGameEvents.SpecialEvents;

public class PolicyAdoptedByVoting
{
	private EventActionDialog meetingEvent;

	public PolicyAdoptedByVoting()
	{
		meetingEvent = new EventActionDialog();
	}

	public void ShowPolicyAdoption(TierArea area, List<Entity> personsFor, List<Entity> personsAgainst, List<Entity> personsNotParticipating)
	{
		string text = ComposeMainText(area, personsFor, personsAgainst);
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = ComposeSummaryText(area, personsFor, personsAgainst, personsNotParticipating);
			text += text2;
			meetingEvent.Heading = "POLICY ADOPTED";
			meetingEvent.DisplayText = new DynamicText();
			meetingEvent.DisplayText.Text = text;
			meetingEvent.DisplayImage = "GroupMeeting";
			string failReason = null;
			meetingEvent.Execute(null, ref failReason);
		}
	}

	private string ComposeSummaryText(TierArea tierArea, List<Entity> personsFor, List<Entity> personsAgainst, List<Entity> personsNotParticipating)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendLine(stringBuilder);
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "MINUTES");
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder, "Adopting " + tierArea.ToString().ToUpper(Config.Culture));
		Common.Append(stringBuilder, "Vote for: ");
		Common.AppendLine(stringBuilder, Common.ListToCommaSeparatedString(personsFor, (Entity e) => SubstituteValue.FormatAllegianceMember(e)));
		Common.Append(stringBuilder, "Vote against: ");
		string line = Common.ListToCommaSeparatedString(personsAgainst, (Entity e) => SubstituteValue.FormatAllegianceMember(e));
		Common.AppendLine(stringBuilder, line);
		Common.Append(stringBuilder, "Not present: ");
		Common.AppendLine(stringBuilder, Common.ListToCommaSeparatedString(personsNotParticipating, (Entity e) => SubstituteValue.FormatAllegianceMember(e)));
		Common.AppendLine(stringBuilder);
		Common.Append(stringBuilder, "The following members now have ");
		Statistic.AppendRatingsTypeToStringAndIcon(stringBuilder, tierArea.Area);
		Common.Append(stringBuilder, " principles raised to ");
		Common.AppendPercentage(stringBuilder, tierArea.TierType.GetTierEdgeBelow(), useColoring: true, null);
		Common.AppendLine(stringBuilder, ": ");
		Common.AppendLine(stringBuilder, line);
		return stringBuilder.ToString();
	}

	private string ComposeMainText(TierArea area, List<Entity> personsFor, List<Entity> personsAgainst)
	{
		if (personsFor.Count + personsAgainst.Count == 1)
		{
			return null;
		}
		Entity randomListMember = Common.GetRandomListMember(personsFor, The.Sim.GameplayRandomGenerator);
		Entity entity = null;
		if (personsAgainst.Count > 0)
		{
			entity = Common.GetRandomListMember(personsAgainst, The.Sim.GameplayRandomGenerator);
		}
		string text = "";
		PropertyResult? content = null;
		if (randomListMember != null)
		{
			if (entity != null)
			{
				GetDialogTextProperties(area, out content);
			}
			else
			{
				GetDialogTextPropertiesAllAgree(area, out content);
			}
			if (content.HasValue)
			{
				text = content.Value.StringResult;
				text = text.Replace("#FOR", SubstituteValue.FormatAllegianceMember(randomListMember));
				if (entity != null)
				{
					text = text.Replace("#AGAINST", SubstituteValue.FormatAllegianceMember(entity));
				}
			}
		}
		return text;
	}

	private void GetDialogTextProperties(TierArea tierArea, out PropertyResult? content)
	{
		content = The.Sim.PlaySite.GetPropertyValue(tierArea.KeyName + "PolicyAdopted", null);
	}

	private void GetDialogTextProperties2People(RatingTypes mainIssue, out PropertyResult? content)
	{
		content = The.Sim.PlaySite.GetPropertyValue("meeting2Security", null);
	}

	private void GetDialogTextProperties3OrMore(TierArea tierArea, RatingTypes mainIssue, out PropertyResult? content)
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

	private void GetDialogTextPropertiesAllAgree(TierArea tierArea, out PropertyResult? content)
	{
		content = null;
		content = The.Sim.PlaySite.GetPropertyValue(tierArea.KeyName + "PolicyAdoptedAllAgree", null);
	}
}
