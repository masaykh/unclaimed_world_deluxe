using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.PropertyPresentation;

[XmlInclude(typeof(GroupNode))]
[XmlInclude(typeof(LeafNode))]
public abstract class PresentationNode
{
	public int? SortOrder;

	[XmlIgnore]
	public int Index;

	public bool SetCountAsSummary;

	protected const int IndentedMemberIconPaddingLeft = 19;

	protected const int IndentedMemberTextPaddingLeft = 20;

	protected const int NormalIconPaddingLeft = 5;

	protected const int NormalTextPaddingLeft = 8;

	public abstract void Display(PresentationTypeCategory categoryToProcess, IHasExposedProperties hasExposedProperties, IHasExposedProperties parent, IKeyedEntryComponent populatable, bool isIndented, ref Dictionary<object, object> entryKeys, ref int? numberOfItems);

	protected bool DisplayEntry(PresentationTypeCategory categoryToProcess, IHasExposedProperties hasExposedProperties, IHasExposedProperties parent, IKeyedEntryComponent populatable, Presentation presentation, int? sortOrder, ref Dictionary<object, object> entryKeys, int? height = null, int? iconPaddingLeft = null, int? textPaddingLeft = null, bool alwaysAdd = false, bool showIconBackground = false)
	{
		PresentationType presentationTypeFromData = PresentationTypeCategoryProcessor.GetPresentationTypeFromData(presentation);
		GetDataToDisplay(hasExposedProperties, parent, null, null, presentation, presentationTypeFromData, out var iconColor, out var termColor, out var propertyTerm, out var propertyIconName, out var caption, out var _, out var entryKey, out var propertyTermTooltip, out var captionTooltip, out var valueTooltipSetting, out var propertyResult, out var showBar, out var normalizedValue, out var normalizedValue2, out var barWidth, out var entityID, out var makeClickable, out var showTypeButton, out var isSeenDirectly);
		if (alwaysAdd || populatable.CanProcessEntryData(propertyTerm, propertyIconName, normalizedValue))
		{
			int? num = null;
			num = ((propertyIconName == null) ? textPaddingLeft : iconPaddingLeft);
			bool rightAdjustTerm = false;
			int? paddingRight = null;
			if (presentationTypeFromData != null)
			{
				rightAdjustTerm = presentationTypeFromData.RightAdjustValue;
				paddingRight = presentationTypeFromData.ValueRightPadding;
			}
			AddUpdateEntry(populatable, propertyTerm, caption, propertyTermTooltip, captionTooltip, valueTooltipSetting, propertyIconName, iconColor, termColor, entryKey, propertyResult, showBar, showTypeButton, normalizedValue, normalizedValue2, barWidth, entityID, makeClickable, sortOrder, height, num, paddingRight, showIconBackground, isSeenDirectly != true, rightAdjustTerm);
			Common.AddToDictionary(ref entryKeys, entryKey, entryKey);
			return true;
		}
		return false;
	}

	public UIComponent AddEntityTypeButton(GUIManager gui, string typeKey)
	{
		if (GameData.Instance.AllEntityTypes.TryGetValue(typeKey, out var value))
		{
			DataTypeButton dataTypeButton = new DataTypeButton(gui, DataSheet.InfoToShow.Production, value, null, useUIOwner: true);
			dataTypeButton.Init(TextButton.TextButtonType.LCDToolTipBlack);
			dataTypeButton.ID = UIComponent.DataControlID.Status;
			dataTypeButton.IsRoot = true;
			dataTypeButton.Text = value.Name;
			dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
			dataTypeButton.DebugTag = "cropType";
			dataTypeButton.Width = 125;
			return dataTypeButton;
		}
		return null;
	}

	private void AddUpdateEntry(IKeyedEntryComponent entryComponent, string term, string caption, string termTooltip, string captionTooltip, TooltipSettings valueTooltipSettings, string icon, Color? iconColor, Color? termColor, string entryKey, PropertyResult? result, bool showBar, bool showEntityTypeButton, float? normalizedValue1, float? normalizedValue2, int? barWidth, EntityID? entityID, bool makeClickable, int? orderingNumber, int? height = null, int? paddingLeft = null, int? paddingRight = null, bool showIconBackground = false, bool showFaded = false, bool rightAdjustTerm = false)
	{
		float? entryValue = null;
		if (result.HasValue)
		{
			entryValue = result.Value.NumberResult;
		}
		entryKey.Contains("2progressprogressStatusIcons");
		if (!entryComponent.TryGetEntry(entryKey, out var entry))
		{
			uint? entityID2 = null;
			if (entityID.HasValue)
			{
				entityID2 = (uint)entityID.Value;
			}
			string showTooltipSetProperty = null;
			bool? disableTooltipExpiry = null;
			int? tooltipWidth = null;
			if (valueTooltipSettings != null)
			{
				showTooltipSetProperty = valueTooltipSettings.TooltipActivationProperty;
				disableTooltipExpiry = valueTooltipSettings.DisableExpiry;
				tooltipWidth = valueTooltipSettings.TooltipWidth;
			}
			UIComponent customComponent = null;
			if (showEntityTypeButton)
			{
				customComponent = AddEntityTypeButton(((UIComponent)entryComponent).guiManager, term);
			}
			entry = entryComponent.AddEntry(entryKey, term, caption, tooltipDisplayedCallback, showTooltipSetProperty, disableTooltipExpiry, tooltipWidth, icon, iconColor, termColor, orderingNumber, entryValue, showBar, normalizedValue1, normalizedValue2, barWidth, makeClickable, entityID2, customComponent, height, paddingLeft, showIconBackground);
		}
		entryComponent.UpdateEntry(entry, term, caption, termTooltip, captionTooltip, icon, iconColor, termColor, entryValue, normalizedValue1, normalizedValue2, paddingLeft, paddingRight, showIconBackground, showFaded, rightAdjustTerm);
	}

	private void tooltipDisplayedCallback(UIComponent sender, bool value)
	{
		if (sender.Tag2 != null && sender.Tag2 is Tuple<uint, string> tuple)
		{
			Entity.FindByID((EntityID)tuple.Item1)?.SetPropertyValue(tuple.Item2, new PropertyResult
			{
				BoolResult = value
			});
		}
	}

	public static void GetDataToDisplay(IHasExposedProperties hasExposedProperties, IHasExposedProperties parent, object key, Dictionary<object, PresentationData> processedData, Presentation presentation, PresentationType presentationType, out Color? iconColor, out Color? termColor, out string propertyTerm, out string propertyIconName, out string caption, out string keyNameToCheckFor, out string entryKey, out string propertyTermTooltip, out string captionTooltip, out TooltipSettings valueTooltipSetting, out PropertyResult? propertyResult, out bool showBar, out float? normalizedValue1, out float? normalizedValue2, out int? barWidth, out EntityID? entityID, out bool makeClickable, out bool showTypeButton, out bool? isSeenDirectly)
	{
		propertyTermTooltip = null;
		iconColor = null;
		termColor = null;
		propertyTerm = null;
		propertyIconName = null;
		caption = null;
		keyNameToCheckFor = null;
		entryKey = null;
		entityID = null;
		propertyResult = null;
		normalizedValue1 = null;
		normalizedValue2 = null;
		barWidth = null;
		isSeenDirectly = null;
		captionTooltip = null;
		showTypeButton = false;
		showBar = false;
		makeClickable = presentation.MakePropertyClickable;
		entityID = hasExposedProperties.GetEntityID();
		if (presentationType != null)
		{
			if (presentationType.BarPresentation != null)
			{
				showBar = true;
				barWidth = presentationType.BarPresentation.Width;
			}
			if (presentationType.EntityTypePresentation != null)
			{
				showTypeButton = true;
			}
		}
		isSeenDirectly = hasExposedProperties.GetIsSeenDirectly();
		if (presentation.PropertyNameForValue != null)
		{
			propertyResult = hasExposedProperties.GetPropertyValue(presentation.PropertyNameForValue, The.InGameUI.UIAllegiance.SharedKnowledge, parent);
			if (propertyResult.HasValue)
			{
				keyNameToCheckFor = PresentationTypeCategoryProcessor.DetermineKeyName(presentation, propertyResult.Value, hasExposedProperties);
				float? numberResult = propertyResult.Value.NumberResult;
				Pair<float, float> numberPairResult = propertyResult.Value.NumberPairResult;
				string stringResult = propertyResult.Value.StringResult;
				_ = propertyResult.Value;
				DateAndTime.TimeDateYear? dateResult = propertyResult.Value.DateResult;
				if (numberResult.HasValue)
				{
					if (presentationType != null)
					{
						propertyIconName = presentationType.GetIcon(numberResult.Value, null, keyNameToCheckFor);
						propertyTerm = presentationType.GetValueTerm(numberResult.Value, null, keyNameToCheckFor);
						propertyTermTooltip = presentationType.GetTermTooltip(numberResult.Value, null, keyNameToCheckFor);
						iconColor = presentationType.GetIconColor(numberResult.Value, null, keyNameToCheckFor);
						termColor = presentationType.GetTermColor(numberResult.Value, null, keyNameToCheckFor);
						normalizedValue1 = presentationType.GetNormalizedValue(numberResult.Value);
					}
					else
					{
						propertyTerm = numberResult.Value.ToString(Config.Culture);
					}
				}
				else if (stringResult != null)
				{
					if (presentationType != null)
					{
						propertyIconName = presentationType.GetIcon(stringResult, keyNameToCheckFor);
						propertyTerm = presentationType.GetTerm(stringResult, keyNameToCheckFor);
					}
					else
					{
						propertyTerm = stringResult;
					}
				}
				else if (dateResult.HasValue)
				{
					propertyTerm = dateResult.ToString();
				}
				else if (numberPairResult != null && presentationType != null)
				{
					propertyIconName = presentationType.GetIcon(numberPairResult.First, numberPairResult.Second, keyNameToCheckFor);
					propertyTerm = presentationType.GetValueTerm(numberPairResult.First, numberPairResult.Second, keyNameToCheckFor);
					propertyTermTooltip = presentationType.GetTermTooltip(numberPairResult.First, numberPairResult.Second, keyNameToCheckFor);
					iconColor = presentationType.GetIconColor(numberPairResult.First, numberPairResult.Second, keyNameToCheckFor);
					termColor = presentationType.GetTermColor(numberPairResult.First, numberPairResult.Second, keyNameToCheckFor);
					normalizedValue1 = presentationType.GetNormalizedValue(numberPairResult.First);
					normalizedValue2 = presentationType.GetNormalizedValue(numberPairResult.Second);
				}
			}
		}
		if (presentation.Caption != null)
		{
			caption = presentation.Caption.GetValue(hasExposedProperties, parent);
			if (presentationType != null && presentationType.CaptionTextFormatting != null)
			{
				caption = presentationType.CaptionTextFormatting.GetFormattedText(null, caption);
			}
			if (showBar)
			{
				if (normalizedValue1.HasValue)
				{
					caption += ": ";
				}
			}
			else if (!string.IsNullOrEmpty(propertyTerm) && !string.IsNullOrEmpty(caption))
			{
				caption += ": ";
			}
		}
		if (presentation.CaptionTooltip != null)
		{
			captionTooltip = presentation.CaptionTooltip.GetValue(hasExposedProperties, parent);
			if (presentationType != null && presentationType.CaptionTooltipTextFormatting != null)
			{
				captionTooltip = presentationType.CaptionTooltipTextFormatting.GetFormattedText(null, captionTooltip);
			}
		}
		captionTooltip = captionTooltip ?? "No tooltip available.";
		valueTooltipSetting = presentation.ValueTooltipSettings;
		if (presentation.ValueTooltip != null)
		{
			propertyTermTooltip = presentation.ValueTooltip.GetValue(hasExposedProperties, parent);
			if (presentationType != null && presentationType.ValueTooltipTextFormatting != null)
			{
				propertyTermTooltip = presentationType.ValueTooltipTextFormatting.GetFormattedText(null, propertyTermTooltip);
			}
		}
		bool? flag = isSeenDirectly;
		bool flag2 = false;
		if (flag == true == flag2 && flag.HasValue && propertyTermTooltip != null && propertyTermTooltip != "No tooltip available.")
		{
			propertyTermTooltip += "\n ( last known status! )";
		}
		propertyTermTooltip = propertyTermTooltip ?? "No tooltip available.";
		PresentationTypeCategoryProcessor.DetermineEntryKey(caption, presentation, hasExposedProperties, out entryKey);
		if (key != null && processedData != null && (propertyTerm != null || propertyIconName != null || normalizedValue1.HasValue))
		{
			processedData.Add(key, new PresentationData(caption, propertyTerm, propertyIconName, propertyResult, normalizedValue1, entryKey, entityID, iconColor, propertyTermTooltip));
		}
	}

	public abstract void PreInitValidate(ref List<string> listOfErrors);
}
