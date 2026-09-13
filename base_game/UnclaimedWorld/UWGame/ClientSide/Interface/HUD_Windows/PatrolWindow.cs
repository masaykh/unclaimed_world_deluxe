using System;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class PatrolWindow : HUDWindow
{
	private MapArea mapArea;

	private CheckBox cbAttackVermin;

	private CheckBox cbAttackTargetsOutsideZone;

	private TextButton btOK;

	private Label lblName;

	private Label lblHeader;

	private Image headerIcon;

	private FillableBar fbNoOfPatrollers;

	public PatrolWindow()
		: base(239, 240, hasSurface: true, hasCloseButton: false, isMovable: true, "HUD_window_base", hideWhenMouseExits: false, Level.Bottom)
	{
		AddZoneNameAndHeader("", "PATROL", "HUD_icon_patrol", 18, out lblName, out lblHeader, out headerIcon);
		Label label = new Label(gui);
		label.Init(Label.LabelType.HUDWindow);
		Add(label);
		label.Text = "No. of patrollers:";
		label.X = 18;
		label.Y = 52;
		fbNoOfPatrollers = new FillableBar(gui, FillableBar.FillableBarType.HUDSlider, canGrow: false, includeButtons: true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);
		Add(fbNoOfPatrollers);
		fbNoOfPatrollers.X = 120;
		fbNoOfPatrollers.Y = label.Y;
		fbNoOfPatrollers.Width = 120;
		fbNoOfPatrollers.MaxValue = 10;
		fbNoOfPatrollers.Value = 1;
		fbNoOfPatrollers.UpdateSliderPosition();
		cbAttackVermin = new CheckBox(gui);
		Add(cbAttackVermin);
		cbAttackVermin.Init(CheckBoxType.HUDCheckBox);
		cbAttackVermin.Text = "Attack vermin";
		cbAttackVermin.ToolTip = "Select whether vermin should be attacked by the patroller in addition to dangerous animals.";
		cbAttackVermin.FitToText();
		cbAttackVermin.X = 18;
		cbAttackVermin.Y = label.Bottom + 12;
		cbAttackTargetsOutsideZone = new CheckBox(gui);
		Add(cbAttackTargetsOutsideZone);
		cbAttackTargetsOutsideZone.Init(CheckBoxType.HUDCheckBox);
		cbAttackTargetsOutsideZone.Text = "May leave zone in pursuit";
		cbAttackTargetsOutsideZone.ToolTip = "Select whether the patroller is permitted to pursue targets far outside the patrol zone.";
		cbAttackTargetsOutsideZone.FitToText();
		cbAttackTargetsOutsideZone.X = 18;
		cbAttackTargetsOutsideZone.Y = cbAttackVermin.Bottom + 12;
		TextButton textButton = new TextButton(gui);
		Add(textButton);
		textButton.Text = "CANCEL";
		textButton.Init(TextButton.TextButtonType.HUD);
		textButton.Click += btCancel_Click;
		textButton.Width = 72;
		textButton.Y = DisplayWindow.Height - textButton.Height - 18;
		textButton.X = DisplayWindow.Width - 18 - textButton.Width;
		btOK = new TextButton(gui);
		Add(btOK);
		btOK.Text = "OK";
		btOK.Init(TextButton.TextButtonType.HUD);
		btOK.Click += btOK_Click;
		btOK.Width = 72;
		btOK.Y = DisplayWindow.Height - btOK.Height - 18;
		btOK.X = textButton.X - 2 - btOK.Width;
	}

	public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = true)
	{
		base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);
		mapArea = TileSelectionContextMenu.GetMapArea();
		string zoneName = "";
		if (mapArea.Zone != null)
		{
			zoneName = mapArea.Zone.GetDisplayName();
		}
		SetDisplayName(zoneName, lblName, lblHeader, headerIcon);
		if (mapArea.Zone != null && mapArea.Zone.PatrolJob != null)
		{
			cbAttackVermin.IsChecked = mapArea.Zone.PatrolJob.AttackVermin;
			cbAttackTargetsOutsideZone.IsChecked = mapArea.Zone.PatrolJob.AttackTargetsOutsideZone;
			fbNoOfPatrollers.Value = mapArea.Zone.PatrolJob.MaxJobPositions;
			fbNoOfPatrollers.UpdateSliderPosition();
			cbAttackVermin.Enabled = false;
			cbAttackTargetsOutsideZone.Enabled = false;
		}
		else
		{
			cbAttackVermin.IsChecked = false;
			cbAttackTargetsOutsideZone.IsChecked = false;
			cbAttackVermin.Enabled = true;
			cbAttackTargetsOutsideZone.Enabled = true;
		}
	}

	public override void Hide()
	{
		base.Hide();
	}

	private void btOK_Click(UIComponent sender, EventArgs e)
	{
		if (TileSelectionContextMenu.CanCreateAndSelectZone())
		{
			if (!TileSelectionContextMenu.GetExpedition(out var expeditionGroupID))
			{
				Hide();
				return;
			}
			bool isChecked = cbAttackVermin.IsChecked;
			bool isChecked2 = cbAttackTargetsOutsideZone.IsChecked;
			int value = fbNoOfPatrollers.Value;
			Command command = ((mapArea.Zone != null && mapArea.Zone.PatrolJob != null) ? ((Command)new PatrolAreaUpdateJob(mapArea.Zone.PatrolJob.ID, giveClientFeedback: true, value)) : ((Command)((The.InGameUI.SelectedZone == null) ? new PatrolArea(The.InGameUI.SelectedTiles, giveClientFeedback: true, expeditionGroupID, isChecked, isChecked2, value) : new PatrolArea(The.InGameUI.SelectedZone.ID, giveClientFeedback: true, expeditionGroupID, isChecked, isChecked2, value))));
			The.Client.Controller.StoreAndExecuteCommand(command);
		}
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Hide();
	}
}
