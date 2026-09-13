using System;
using InputEventSystem;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls;

public class DataTypeButton : TextButton
{
	public CommonInterface.AnchorSide? SideToAnchorOn;

	public DataSheet.InfoToShow InfoToShow;

	public bool IsRoot = true;

	public EntityType entityType;

	private EntityID? EntityID;

	public static Color notInStockColorDark = "#87824c".ColorFromHex();

	public static Color notInStockColorLight = "#cac172".ColorFromHex();

	public ProcessType processType { get; private set; }

	public DataTypeButton(GUIManager gui, DataSheet.InfoToShow infoToShow, EntityType entityType, EntityGroupID? owner, bool useUIOwner, EntityID? entityID = null)
		: base(gui)
	{
		base.Click += EntityTypeButton_Click;
		FillEntityType(infoToShow, entityType, owner, useUIOwner, entityType.PluralName, entityID);
	}

	public DataTypeButton(GUIManager gui, DataSheet.InfoToShow infoToShow, ProcessType processType, EntityGroupID? owner, bool useUIOwner)
		: base(gui)
	{
		base.Click += EntityTypeButton_Click;
		FillProcessType(infoToShow, processType, owner, useUIOwner, processType.Name);
	}

	public void FillEntityType(DataSheet.InfoToShow infoToShow, EntityType entityType, EntityGroupID? owner, bool useUIOwner, string newText = null, EntityID? entityID = null)
	{
		if (entityType != this.entityType || EntityID != entityID)
		{
			this.entityType = entityType;
			EntityID = entityID;
			InfoToShow = infoToShow;
			EventArgs = new DataTypeButtonEventArgs(owner, useUIOwner);
			if (newText != null)
			{
				base.Text = newText;
			}
		}
	}

	private void FillProcessType(DataSheet.InfoToShow infoToShow, ProcessType processType, EntityGroupID? owner, bool useUIOwner, string newText = null)
	{
		if (processType != this.processType)
		{
			this.processType = processType;
			InfoToShow = infoToShow;
			EventArgs = new DataTypeButtonEventArgs(owner, useUIOwner);
			if (newText != null)
			{
				base.Text = newText;
			}
		}
	}

	public bool ShowsData(DataSheet tooltip)
	{
		if (tooltip is EntityDataSheet entityDataSheet)
		{
			return entityDataSheet.EntityType == entityType;
		}
		if (tooltip.ProcessType != null)
		{
			return tooltip.ProcessType == processType;
		}
		return false;
	}

	private void EntityTypeButton_Click(UIComponent sender, EventArgs e)
	{
		HandleEntityTypeButtonClick(e);
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		if (The.Sim.Mode != Sim.EngineMode.Game)
		{
			return;
		}
		if (The.InGameUI.EntityTypeTooltipsStack.Count > 0)
		{
			DataSheet dataSheet = The.InGameUI.EntityTypeTooltipsStack.Find((DataSheet tt) => tt.SpawningControl == this);
			if (dataSheet != null && dataSheet.CurrentState == DataSheet.State.Collapsed && !dataSheet.MouseIsOverTooltipOrChildTooltips(args.Position, 0))
			{
				dataSheet.Hide();
			}
		}
		base.OnMouseOut(sender, args);
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			if (The.InGameUI.GetAnySpawnedTooltip(this) == null)
			{
				GetAndFillTooltip().StartCountdownToShow(this);
			}
			base.OnMouseOver(sender, args);
		}
	}

	public void HandleEntityTypeButtonClick(EventArgs e)
	{
		if (The.Sim.Mode != Sim.EngineMode.Game)
		{
			return;
		}
		DataTypeButtonEventArgs e2 = e as DataTypeButtonEventArgs;
		DataSheet anySpawnedTooltip = The.InGameUI.GetAnySpawnedTooltip(this);
		int x;
		int y;
		if (anySpawnedTooltip != null)
		{
			if (anySpawnedTooltip.CurrentState != DataSheet.State.Collapsed)
			{
				anySpawnedTooltip.Hide();
				return;
			}
			The.InGameUI.SelectAnchorPoint(this, anySpawnedTooltip.DisplayWindow, SideToAnchorOn, 320, doOverlap: true, DataSheet.GetAnchorPointYOffset(), out x, out y);
			anySpawnedTooltip.InitAndShow(this, e2.Owner, e2.UseUIOwner, x, y);
			if (entityType != null)
			{
				(anySpawnedTooltip as EntityDataSheet).Fill(entityType, EntityID);
			}
			else
			{
				(anySpawnedTooltip as ProcessTypeDataSheet).Fill(processType);
			}
			anySpawnedTooltip.Expand(InfoToShow);
		}
		else
		{
			if (IsRoot)
			{
				The.InGameUI.CloseAllEntityTooltips();
			}
			anySpawnedTooltip = GetAndFillTooltip();
			The.InGameUI.SelectAnchorPoint(this, anySpawnedTooltip.DisplayWindow, SideToAnchorOn, 320, doOverlap: true, DataSheet.GetAnchorPointYOffset(), out x, out y);
			anySpawnedTooltip.InitAndShow(this, e2.Owner, e2.UseUIOwner, x, y);
			anySpawnedTooltip.Expand(InfoToShow);
		}
	}

	private DataSheet GetAndFillTooltip()
	{
		if (entityType != null)
		{
			EntityDataSheet entityDataSheet = The.InGameUI.poolOfEntityTypeTooltips.Get();
			entityDataSheet.Fill(entityType, EntityID);
			return entityDataSheet;
		}
		ProcessTypeDataSheet processTypeDataSheet = The.InGameUI.poolOfProcessTypeTooltips.Get();
		processTypeDataSheet.Fill(processType);
		return processTypeDataSheet;
	}

	public void SetAvailableStatusColor(bool available)
	{
		base.LabelColor = GetStockStatusColor(available, GetNormalColor(), base.Type);
	}

	public static Color GetStockStatusColor(bool ownsItem, Color normalColor, TextButtonType textButtonType)
	{
		Color result = Color.White;
		if (ownsItem)
		{
			result = normalColor;
		}
		else
		{
			switch (textButtonType)
			{
			case TextButtonType.LCDToolTipBlack:
				result = notInStockColorDark;
				break;
			case TextButtonType.HUDToolTipWhite:
				result = notInStockColorLight;
				break;
			case TextButtonType.LCDAmount:
				result = notInStockColorLight;
				break;
			}
		}
		return result;
	}
}
