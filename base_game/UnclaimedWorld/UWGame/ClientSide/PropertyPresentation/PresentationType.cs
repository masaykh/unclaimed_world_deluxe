using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide;

namespace UWGame.ClientSide.PropertyPresentation;

public class PresentationType : IGameData
{
	public TypeDependentPresentation TypeDependentPresentation;

	public NumberThresholdPresentation NumberThresholdPresentation;

	public TextFormatting ValueTextFormatting;

	public TextFormatting ValueTooltipTextFormatting;

	public TextFormatting CaptionTextFormatting;

	public TextFormatting CaptionTooltipTextFormatting;

	public bool RightAdjustValue;

	public int ValueRightPadding;

	public BarPresentation BarPresentation;

	public EntityTypePresentation EntityTypePresentation;

	public IconPresentation IconPresentation;

	public const string MissingTooltipNote = "No tooltip available.";

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public string GetTermTooltip(float value1, float? value2, string typeKey)
	{
		Threshold threshold = GetThreshold(value1, value2, typeKey);
		if ((threshold == null || threshold.UseValueTooltipFormatting) && ValueTooltipTextFormatting != null)
		{
			string term = null;
			if (threshold != null)
			{
				term = threshold.TermTooltip ?? "No tooltip available.";
			}
			return ValueTooltipTextFormatting.GetFormattedText(value1, term);
		}
		if (threshold != null)
		{
			return threshold.TermTooltip ?? "No tooltip available.";
		}
		return null;
	}

	public string GetValueTerm(float value1, float? value2, string typeKey)
	{
		Threshold threshold = GetThreshold(value1, value2, typeKey);
		if ((threshold == null || threshold.UseValueTextFormatting) && ValueTextFormatting != null)
		{
			string term = null;
			if (threshold != null)
			{
				term = threshold.Term;
			}
			float valueToUse = GetValueToUse(value1, value2);
			return ValueTextFormatting.GetFormattedText(valueToUse, term);
		}
		return threshold?.Term;
	}

	public string GetIcon(float value1, float? value2, string typeKey)
	{
		return GetThreshold(value1, value2, typeKey)?.Icon;
	}

	public string GetIcon(string value, string typeKey)
	{
		if (IconPresentation != null)
		{
			return value;
		}
		return null;
	}

	public string GetTerm(string value, string typeKey)
	{
		if (IconPresentation == null)
		{
			return value;
		}
		return null;
	}

	public void GetCustomPresentation(List<string> multiResult, out string propertyTerm, out Color? propertyColor, out string propertyIconName, out string propertyTermTooltip, string typeKey)
	{
		propertyColor = null;
		propertyTerm = null;
		propertyIconName = null;
		propertyTermTooltip = null;
		if (multiResult.Count == 2)
		{
			if (IconPresentation != null)
			{
				propertyIconName = multiResult[0];
			}
			else
			{
				propertyTerm = multiResult[0];
			}
			propertyTermTooltip = multiResult[1];
		}
	}

	public Color? GetIconColor(float value1, float? value2, string typeKey)
	{
		return GetThreshold(value1, value2, typeKey)?.IconTint;
	}

	public Color? GetTermColor(float value1, float? value2, string typeKey)
	{
		return GetThreshold(value1, value2, typeKey)?.TermTint;
	}

	public float? GetNormalizedValue(float value)
	{
		if (BarPresentation != null)
		{
			float num = Common.Clamp(value / BarPresentation.MaxValue, 0f, 1f);
			if (BarPresentation.SuppressIfMaximumValue && Common.IsEqual(num, 1f))
			{
				return null;
			}
			return num;
		}
		return null;
	}

	public Threshold GetThreshold(float value1, float? value2, string typeKey)
	{
		Threshold[] value3 = null;
		if (typeKey != null && TypeDependentPresentation != null)
		{
			TypeDependentPresentation.ThresholdsByType.TryGetValue(typeKey, out value3);
		}
		if (value3 == null)
		{
			if (NumberThresholdPresentation == null)
			{
				return null;
			}
			value3 = NumberThresholdPresentation.Thresholds;
		}
		float valueToUse = GetValueToUse(value1, value2);
		int stairstep;
		if (value3 != null)
		{
			return Common.GetStairStepIndex(valueToUse, value3, out stairstep);
		}
		return null;
	}

	private float GetValueToUse(float value1, float? value2)
	{
		if (!value2.HasValue)
		{
			return value1;
		}
		return NumberThresholdPresentation.NumberSource switch
		{
			NumberSource.First => value1, 
			NumberSource.Second => value2.Value, 
			NumberSource.Difference => value2.Value - value1, 
			_ => value1, 
		};
	}

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> errors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
