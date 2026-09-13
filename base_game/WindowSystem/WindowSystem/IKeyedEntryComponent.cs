using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public interface IKeyedEntryComponent
{
	List<UIComponent> Entries { get; set; }

	int Count { get; }

	void BeginAddingEntries();

	void EndAddingEntries();

	bool TryRemoveEntry(object key);

	bool TryGetEntry(object key, out UIComponent entry);

	void RemoveEntry(object key, UIComponent item);

	bool CanProcessEntryData(string entryTerm, string entryIconName, float? normalizedValue);

	UIComponent AddEntry(object key, string term, string caption, Action<UIComponent, bool> tooltipDisplayedCallback, string showTooltipSetProperty, bool? disableTooltipExpiry, int? tooltipWidth, string entryIconName, Color? iconColor, Color? termColor, int? orderingNumber, float? entryValue, bool showBar = false, float? normalizedValue = null, float? normalizedValue2 = null, int? barWidth = null, bool clickable = false, uint? entityID = null, UIComponent customComponent = null, int? height = null, int? paddingLeft = null, bool useIconBackground = false);

	void UpdateEntry(UIComponent existingEntry, string term, string caption, string entryTermTooltip, string captionTooltip, string entryIconName, Color? iconColor, Color? termColor, float? entryValue, float? normalizedValue1, float? normalizedValue2 = null, int? paddingLeft = null, int? paddingRight = null, bool useIconBackground = false, bool showFaded = false, bool rightAdjustTerm = false);

	bool AddSeparator(object key, int? orderingNumber, float? entryValue);

	bool AddSubHeader(string subHeaderName, int? orderingNumber, float? entryValue);

	UIComponent AddGroup(object key, bool hasBorder, int? marginLeft = null);

	object GetKeyFromIndex(int index);

	void CapNoOfEntries();

	void Sort(Grid.Sorting sortType, bool useFirstTag, Func<float, float> transformation = null);

	void Clear();
}
