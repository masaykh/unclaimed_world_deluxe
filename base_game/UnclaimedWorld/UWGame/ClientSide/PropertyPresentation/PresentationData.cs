using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.PropertyPresentation;

public class PresentationData
{
	public string TermTooltip;

	public string Caption;

	public string Term;

	public string IconName;

	public Color? Color;

	public string Key;

	public EntityID? ClickablePropertyEntityID;

	public PropertyResult? Result;

	public float? NormalizedValue;

	public PresentationData(string presentationCaption, string presentationTerm, string presentationIcon, PropertyResult? propertyValue, float? normalizedValue, string presentationKey, EntityID? clickablePropertyEntityID, Color? color, string termTooltipValue)
	{
		TermTooltip = termTooltipValue;
		Caption = presentationCaption;
		Term = presentationTerm;
		IconName = presentationIcon;
		Result = propertyValue;
		Key = presentationKey;
		ClickablePropertyEntityID = clickablePropertyEntityID;
		Color = color;
		NormalizedValue = normalizedValue;
	}

	public PresentationData()
	{
	}
}
