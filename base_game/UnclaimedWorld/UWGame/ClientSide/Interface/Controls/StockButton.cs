using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls;

public class StockButton : TextButton
{
	public EntityType EntityType;

	public List<EntityID> EntityList = new List<EntityID>();

	private const string clickToSeeListTooltip = "\n \nClick to see the list";

	public StockButton(GUIManager gui, EntityType entityType)
		: base(gui)
	{
		Init(TextButtonType.LCDAmount);
		ToolTip = "Click to see the list of items";
		EntityType = entityType;
	}

	public void UpdateStockButton(int? noOfAvailableItems, int? noOfIncompleteItems, int? noOfItemsUsedAsParts, int? noOfItemsOffSite, int? noOfItemsOwnedByOthers, List<EntityID> listOfEntities, bool hideIfZero)
	{
		EntityList.Clear();
		if (listOfEntities != null)
		{
			EntityList.AddRange(listOfEntities);
		}
		int? num = null;
		if (noOfIncompleteItems.HasValue || noOfItemsUsedAsParts.HasValue || noOfItemsOffSite.HasValue || noOfItemsOwnedByOthers.HasValue)
		{
			num = (noOfIncompleteItems ?? 0) + (noOfItemsUsedAsParts ?? 0) + (noOfItemsOffSite ?? 0) + (noOfItemsOwnedByOthers ?? 0);
		}
		bool flag = noOfAvailableItems > 0;
		Color labelColor = DataTypeButton.GetStockStatusColor(flag, GetNormalColor(), base.Type);
		if ((noOfIncompleteItems != 0 && noOfAvailableItems == 0) || (noOfItemsUsedAsParts != 0 && noOfAvailableItems == 0))
		{
			labelColor = "#FFFFFF".ColorFromHex();
		}
		base.LabelColor = labelColor;
		Tag1 = flag;
		if (noOfAvailableItems.HasValue && num.HasValue)
		{
			base.Text = noOfAvailableItems + "|" + num;
			ToolTip = noOfAvailableItems.Value + " items are available.";
			string unavailableItemsBreakdown = GetUnavailableItemsBreakdown(noOfIncompleteItems, noOfItemsUsedAsParts, noOfItemsOffSite, noOfItemsOwnedByOthers);
			if (!string.IsNullOrEmpty(unavailableItemsBreakdown))
			{
				ToolTip = ToolTip + " \n" + unavailableItemsBreakdown;
			}
			ToolTip += "\n \nClick to see the list";
		}
		else if (noOfAvailableItems.HasValue)
		{
			base.Text = noOfAvailableItems.Value.ToString();
			ToolTip = "We have " + noOfAvailableItems.Value + " items in inventory.";
			if (noOfAvailableItems > 0)
			{
				Visible = true;
				ToolTip += "\n \nClick to see the list";
			}
			else if (hideIfZero)
			{
				Visible = false;
			}
		}
		else
		{
			base.Text = num.Value.ToString();
			ToolTip = GetUnavailableItemsBreakdown(noOfIncompleteItems, noOfItemsUsedAsParts, noOfItemsOffSite, noOfItemsOwnedByOthers);
			if (num > 0)
			{
				Visible = true;
				ToolTip += "\n \nClick to see the list";
			}
			else if (hideIfZero)
			{
				Visible = false;
			}
		}
		if (noOfAvailableItems > 0 || num > 0)
		{
			Tag1 = true;
			Enabled = true;
		}
		else
		{
			Enabled = false;
		}
		ScaleWidthToFitText();
	}

	private string GetUnavailableItemsBreakdown(int? noOfIncompleteItems, int? noOfItemsUsedAsParts, int? noOfItemsOffSite, int? noOfItemsOwnedByOthers)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		if (noOfIncompleteItems.HasValue && noOfIncompleteItems.Value > 0)
		{
			stringBuilder.Append(noOfIncompleteItems.Value);
			stringBuilder.Append(" items are being produced");
			flag = true;
		}
		if (noOfItemsUsedAsParts.HasValue && noOfItemsUsedAsParts.Value > 0)
		{
			if (flag)
			{
				stringBuilder.Append(" \n");
			}
			stringBuilder.Append(noOfItemsUsedAsParts.Value);
			stringBuilder.Append(" items are parts");
			flag = true;
		}
		if (noOfItemsOffSite.HasValue && noOfItemsOffSite.Value > 0)
		{
			if (flag)
			{
				stringBuilder.Append(" \n");
			}
			stringBuilder.Append(noOfItemsOffSite.Value);
			stringBuilder.Append(" items are off-site");
			flag = true;
		}
		if (noOfItemsOwnedByOthers.HasValue && noOfItemsOwnedByOthers.Value > 0)
		{
			if (flag)
			{
				stringBuilder.Append(" \n");
			}
			stringBuilder.Append(noOfItemsOwnedByOthers.Value);
			stringBuilder.Append(" items are owned by others");
			flag = true;
		}
		return stringBuilder.ToString();
	}
}
