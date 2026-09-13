using UWGame.SimSide;

namespace UWGame.ClientSide.PropertyPresentation;

public class TextFormatting
{
	public float? NumberFactor;

	public string NumberFormatString;

	public string TextWithPlaceholders;

	public string GetFormattedText(float? value, string term)
	{
		string text = null;
		if (value.HasValue)
		{
			if (NumberFactor.HasValue)
			{
				value *= NumberFactor.Value;
			}
			if (NumberFormatString != null)
			{
				string numberFormatString = NumberFormatString;
				if (numberFormatString == "SecondsToInGameDays")
				{
					if (Common.IsGreaterThan(value.Value, 0f))
					{
						text = DateAndTime.GetSecondsToIngameDays(value.Value).ToIntervalString();
					}
				}
				else
				{
					text = value.Value.ToString(NumberFormatString, Config.Culture);
				}
			}
		}
		string text2 = "";
		if (TextWithPlaceholders != null)
		{
			string arg = term ?? "";
			return string.Format(TextWithPlaceholders, arg, text ?? "");
		}
		return term ?? text;
	}
}
