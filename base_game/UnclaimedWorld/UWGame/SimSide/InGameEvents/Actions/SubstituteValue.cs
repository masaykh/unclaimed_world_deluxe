using System.Collections.Generic;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Skills;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using WindowSystem;

namespace UWGame.SimSide.InGameEvents.Actions;

public class SubstituteValue
{
	public string Placeholder;

	public EvalNode Property;

	public string PropertyName;

	public TargetObject TargetObject;

	public FormattingOptions? Formatting;

	public string Color;

	public string Evaluate(EventAction eventAction)
	{
		if (TargetObject != null)
		{
			List<IHasExposedProperties> result = TargetObject.GetResult(eventAction);
			string value = "";
			StringBuilder stringBuilder = new StringBuilder();
			foreach (IHasExposedProperties item in result)
			{
				if (item is Entity entity)
				{
					FormatEntity(stringBuilder, entity, Formatting, Color);
				}
				stringBuilder.Append(value);
				value = ", ";
			}
			return stringBuilder.ToString();
		}
		if (Property != null)
		{
			PropertyResult? propertyResult = Property.Evaluate(eventAction);
			return FormatPropertyResult(propertyResult);
		}
		if (PropertyName != null)
		{
			PropertyResult? propertyValue = ((IHasExposedProperties)The.Sim.PlaySite).GetPropertyValue(PropertyName, (SharedKnowledge)null, (IHasExposedProperties)null);
			return FormatPropertyResult(propertyValue);
		}
		return null;
	}

	public static string FormatEntity(Entity entity)
	{
		StringBuilder stringBuilder = new StringBuilder();
		FormatEntity(stringBuilder, entity);
		return stringBuilder.ToString();
	}

	public static string FormatEntity(string name, ProfessionType profession, bool isInAllegiance)
	{
		StringBuilder stringBuilder = new StringBuilder();
		FormatEntity(stringBuilder, name, profession, isInAllegiance);
		return stringBuilder.ToString();
	}

	public static string FormatAllegianceMember(Entity entity)
	{
		return FormatEntity(entity.GetDisplayName(), entity.Intelligence.Profession, isInAllegiance: true);
	}

	public static void FormatEntity(StringBuilder text, Entity entity, FormattingOptions? formatting = null, string color = null)
	{
		string displayName = entity.GetDisplayName();
		ProfessionType profession = null;
		if (entity.EntityType.IntelligenceType != null)
		{
			profession = entity.Intelligence.Profession;
		}
		FormatEntity(text, displayName, profession, entity.Intelligence.Allegiance == The.InGameUI.UIAllegiance, formatting, color);
	}

	public static void FormatEntity(StringBuilder text, string name, ProfessionType profession, bool isInAllegiance, FormattingOptions? formatting = null, string color = null)
	{
		string text2 = null;
		if (formatting != FormattingOptions.NameOnly && profession != null)
		{
			text.Append(Icon.ToIcon(profession.Icon, "#1E525C"));
		}
		if (isInAllegiance)
		{
			text2 = "#COLORMEMBER";
		}
		text2 = color ?? text2;
		if (text2 != null)
		{
			text.Append(Label.ToLabel(name, text2));
		}
		else
		{
			text.Append(name);
		}
	}

	private string FormatPropertyResult(PropertyResult? propertyResult)
	{
		string text = null;
		if (propertyResult.HasValue)
		{
			DateAndTime.TimeDateYear? dateResult = propertyResult.Value.DateResult;
			if (dateResult.HasValue)
			{
				return FormatDate(dateResult, Formatting);
			}
			text = propertyResult.Value.ToString();
			if (Color != null)
			{
				text = Label.ToLabel(text, Color);
			}
		}
		return text;
	}

	public static string FormatDate(DateAndTime.TimeDateYear? date, FormattingOptions? formatting)
	{
		string text = null;
		string text2 = null;
		if (formatting == FormattingOptions.BothDates)
		{
			text = date.Value.ToString();
			text = Label.ToLabel(text, "#COLORDATE");
			return text + $"({date.Value.GetEarthDate()})";
		}
		text2 = "#COLORDATE";
		text = ((formatting != FormattingOptions.EarthDate) ? date.Value.ToString() : date.Value.GetEarthDate());
		return Label.ToLabel(text, text2);
	}
}
