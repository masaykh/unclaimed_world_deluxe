using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class AttackWindow : HUDWindow
{
	private MapArea mapArea;

	private Expedition expedition;

	private CheckBox cbAttackVermin;

	private Grid grid;

	private FillableBar fbNoOfAttackers;

	private Label lblName;

	private Label lblHeader;

	private Image headerIcon;

	private TextButton btCancel;

	private TextButton btOK;

	private int itemTypeIconColumnX = 10;

	private bool isFirstUpdate;

	public AttackWindow()
		: base(239, 240, hasSurface: true, hasCloseButton: false, isMovable: true, "HUD_window_base", hideWhenMouseExits: false, Level.Bottom)
	{
		AddZoneNameAndHeader("", "ATTACK", "HUD_icon_sword", 12, out lblName, out lblHeader, out headerIcon);
		Label label = new Label(gui);
		label.Init(Label.LabelType.HUDWindow);
		Add(label);
		label.Text = "No. of attackers:";
		label.ToolTip = "Select how many armed colony members should participate in the attack. \nWhen all threats are eliminated, the task will be canceled automatically";
		label.X = 18;
		label.Y = 52;
		fbNoOfAttackers = new FillableBar(gui, FillableBar.FillableBarType.HUDSlider, canGrow: false, includeButtons: true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);
		Add(fbNoOfAttackers);
		fbNoOfAttackers.X = 120;
		fbNoOfAttackers.Y = label.Y;
		fbNoOfAttackers.Width = 120;
		fbNoOfAttackers.MaxValue = 10;
		fbNoOfAttackers.Value = 1;
		fbNoOfAttackers.UpdateSliderPosition();
		cbAttackVermin = new CheckBox(gui);
		Add(cbAttackVermin);
		cbAttackVermin.Init(CheckBoxType.HUDCheckBox);
		cbAttackVermin.Text = "Also attack vermin";
		cbAttackVermin.ToolTip = "Select whether vermin should be attacked by the patrollers in addition to dangerous animals.";
		cbAttackVermin.FitToText();
		cbAttackVermin.X = 18;
		cbAttackVermin.Y = fbNoOfAttackers.Bottom + 12;
		cbAttackVermin.button.DebugTag = "cbAttackVermin";
		btCancel = new TextButton(gui);
		Add(btCancel);
		btCancel.Text = "CANCEL";
		btCancel.Init(TextButton.TextButtonType.HUD);
		btCancel.Click += btCancel_Click;
		btCancel.Width = 72;
		btCancel.X = DisplayWindow.Width - 12 - btCancel.Width;
		btOK = new TextButton(gui);
		Add(btOK);
		btOK.Text = "OK";
		btOK.Init(TextButton.TextButtonType.HUD);
		btOK.Click += btOk_Click;
		btOK.Width = 72;
		btOK.X = btCancel.X - 2 - btOK.Width;
		SetVerticalPositions();
	}

	private void SetVerticalPositions()
	{
		btCancel.Y = DisplayWindow.Height - btCancel.Height - 12;
		btOK.Y = btCancel.Y;
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Hide();
	}

	private void btOk_Click(UIComponent sender, EventArgs e)
	{
		EntityGroupID iD = expedition.OwnedEntities.ID;
		Zone zone = mapArea.Zone;
		int value = fbNoOfAttackers.Value;
		bool isChecked = cbAttackVermin.IsChecked;
		Command command = ((zone != null && zone.AttackAreaJob != null) ? ((Command)new AttackAreaUpdateJob(zone.AttackAreaJob.ID, giveClientFeedback: true, value)) : ((Command)((The.InGameUI.SelectedZone == null) ? new AttackArea(The.InGameUI.SelectedTiles, giveClientFeedback: true, iD, isChecked, attackThreats: true, value) : new AttackArea(The.InGameUI.SelectedZone.ID, giveClientFeedback: true, iD, isChecked, attackThreats: true, value))));
		The.Client.Controller.StoreAndExecuteCommand(command);
		zone?.RemoveZoneOrFireOrdersChangedEvent();
		Hide();
	}

	public override void Hide()
	{
		DisplayWindow.Hide();
	}

	public override void Refresh()
	{
	}

	private List<Tuple<EntityType, bool>> GetPreyToDisplay()
	{
		List<Tuple<EntityType, bool>> list = new List<Tuple<EntityType, bool>>();
		HashSet<EntityType> habitats = GetHabitats();
		if (habitats != null)
		{
			foreach (EntityType item in habitats)
			{
				if (The.InGameUI.UIAllegiance.RepresentativeEntityType.IntelligenceType.PreyTypes.Contains(item))
				{
					list.Add(new Tuple<EntityType, bool>(item, item2: true));
				}
			}
		}
		foreach (EntityType item2 in The.InGameUI.UIAllegiance.SharedKnowledge.PlaySiteKnowledge.SpottedPrey)
		{
			if (habitats == null || !habitats.Contains(item2))
			{
				list.Add(new Tuple<EntityType, bool>(item2, item2: false));
			}
		}
		return list;
	}

	private HashSet<EntityType> GetHabitats()
	{
		HashSet<Collidable<Expedition>> expeditions = new HashSet<Collidable<Expedition>>();
		mapArea.IterateArea(delegate(TerrainTile tile)
		{
			GetPreyHabitatsOnTile(tile, expeditions);
		});
		HashSet<EntityType> set = null;
		foreach (Collidable<Expedition> item in expeditions)
		{
			Common.AddToSet(ref set, item.Parent.Allegiance.RepresentativeEntityType);
		}
		return set;
	}

	private void GetPreyHabitatsOnTile(TerrainTile tile, HashSet<Collidable<Expedition>> expeditions)
	{
		if (tile.HasEverBeenSeenByPlayer)
		{
			Vector2 location = MapManager.TileToWorldPosVector2(tile.TilePos.ToPoint());
			The.Sim.PlaySite.PlaySite.ExpeditionRadiusQuadTree.GetCollidablesContainingPoint(location, expeditions);
		}
	}

	private void Populate()
	{
		UIComponent item = null;
		EntityGroup owner = mapArea.GetOwner();
		if (owner == null)
		{
			grid.Clear();
			return;
		}
		List<Tuple<EntityType, bool>> data = GetPreyToDisplay();
		_ = grid.Entries.Count;
		grid.BeginAddingEntries();
		foreach (Tuple<EntityType, bool> item2 in data)
		{
			if (!grid.TryGetEntry(item2.Item1, out item))
			{
				item = AddRow(item2.Item1, owner);
			}
		}
		grid.DeleteEntries((EntityType e) => data.Any((Tuple<EntityType, bool> t) => t.Item1 == e));
		grid.Sort((UIComponent i) => i.OrderByTag1, Grid.Sorting.Ascending);
		grid.EndAddingEntries();
		isFirstUpdate = false;
	}

	public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = true)
	{
		isFirstUpdate = true;
		base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);
		mapArea = TileSelectionContextMenu.GetMapArea();
		expedition = mapArea.GetOwner().Parent as Expedition;
		string zoneName = "";
		if (mapArea.Zone != null)
		{
			zoneName = mapArea.Zone.GetDisplayName();
		}
		if (mapArea.Zone != null && mapArea.Zone.AttackAreaJob != null)
		{
			cbAttackVermin.IsChecked = mapArea.Zone.AttackAreaJob.AttackVermin;
			fbNoOfAttackers.Value = mapArea.Zone.AttackAreaJob.MaxJobPositions;
			fbNoOfAttackers.UpdateSliderPosition();
			cbAttackVermin.Enabled = false;
		}
		else
		{
			cbAttackVermin.IsChecked = false;
			cbAttackVermin.Enabled = true;
		}
		SetDisplayName(zoneName, lblName, lblHeader, headerIcon);
	}

	private UIComponent AddRow(EntityType entityType, EntityGroup owner)
	{
		UIComponent uIComponent = new UIComponent(gui);
		grid.AddEntry(entityType, uIComponent);
		uIComponent.OrderByTag1 = entityType.PluralName;
		InventoryPanel.AddEntityTypeIcon(entityType, uIComponent, itemTypeIconColumnX);
		DataTypeButton dataTypeButton = new DataTypeButton(gui, DataSheet.InfoToShow.Data, entityType, owner.ID, useUIOwner: false);
		dataTypeButton.Init(TextButton.TextButtonType.HUDToolTipWhite);
		dataTypeButton.ID = UIComponent.DataControlID.Caption;
		dataTypeButton.IsRoot = true;
		dataTypeButton.Text = entityType.PluralName;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = 125;
		dataTypeButton.X = 22;
		dataTypeButton.DebugTag = "entityTypeButton";
		CheckBox checkBox = new CheckBox(gui);
		checkBox.Init(CheckBoxType.HUDCheckBox);
		checkBox.ID = UIComponent.DataControlID.Selector;
		checkBox.ToolTip = "If checked, any entities of this type will be attacked";
		uIComponent.AlignVertically(checkBox);
		checkBox.IsChecked = false;
		checkBox.Tag1 = entityType;
		uIComponent.OrderByTag2 = entityType.PluralName;
		return uIComponent;
	}
}
