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
	private FillableBar fbNoOfAttackers;

	private Label lblName;

	private Label lblHeader;

	private Image headerIcon;

	private TextButton btCancel;

	private TextButton btOK;
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
}
