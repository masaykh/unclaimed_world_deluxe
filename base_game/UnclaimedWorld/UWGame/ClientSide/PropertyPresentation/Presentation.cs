using System.Collections.Generic;

namespace UWGame.ClientSide.PropertyPresentation;

public class Presentation
{
	public enum KeyNameForTypeDependentPresentation
	{
		Standard,
		Custom
	}

	public string PropertyNameForValue;

	public StringSource Caption;

	public StringSource CaptionTooltip;

	public StringSource ValueTooltip;

	public TooltipSettings ValueTooltipSettings;

	public string PresentationTypeKey;

	public KeyNameForTypeDependentPresentation KeyNameForTypeDependentPresentationToUse;

	public bool MakePropertyClickable;

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}
}
