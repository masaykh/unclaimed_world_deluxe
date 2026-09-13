using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class RosterAccessPanel
{
	private Window window;

	protected InGameInterface intf = The.InGameUI;

	private const int height = 500;

	public const int Width = 51;

	private const int buttonLeft = 12;

	public ImageButton btStock;

	public ImageButton btJobs;

	public ImageButton btEventDialogs;

	public ImageButton btGraphs;

	public ImageButton btLedger;

	public ImageButton btWorld;

	public ImageButton btDiplomacy;

	public ImageButton btMissions;

	public ImageButton btZone;

	public ImageButton btEntity;

	public ImageButton btPersonnel;

	public ImageButton btPolicy;

	public ImageButton btEditorPlaceEntity;

	public ImageButton btEditorPaintTile;

	public ImageButton btEditorTerrainHeight;

	public RosterAccessPanel()
	{
		window = new Window(intf.gui);
		window.Position = new Point(The.Client.Controller.DrawArea.Width - 51, 254);
		window.WindowSize = new Vector2(51f, 500f);
		window.Level = Level.BelowBelowBelowMiddle;
		window.IsMovable = false;
		window.Resizable = false;
		window.Margin = 0;
		window.HasCloseButton = false;
		window.Skin = intf.gui.GUISpriteSheet.GetSourceRectangle("sidebar_base");
		window.CornerSize = 25;
		window.Show();
		int num = 4;
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			Image image = Panel.AddImage(The.InGameUI.gui, window, "infoswitch_bg", new Point(12, 20));
			btEntity = new ImageButton(The.InGameUI.gui);
			window.Add(btEntity);
			btEntity.Init(ImageButtonType.EntityInfo);
			btEntity.Position = new Point(image.X + 7, image.Y + 24);
			btEntity.ToolTip = "Show details about the selected entity";
			btEntity.Click += btEntity_Click;
			The.InGameUI.SidePanelEntity.AccessButton = btEntity;
			The.InGameUI.SelectedEntityChangedEvent += InGameUI_SelectedEntityChangedEvent;
			btEntity.Enabled = false;
			btZone = new ImageButton(The.InGameUI.gui);
			window.Add(btZone);
			btZone.Init(ImageButtonType.MapAreaInfo);
			btZone.Position = new Point(btEntity.X, btEntity.Bottom + 2);
			btZone.ToolTip = "Show details about the selected zone";
			btZone.Click += btZone_Click;
			The.InGameUI.SidePanelMapArea.AccessButton = btZone;
			btStock = new ImageButton(intf.gui);
			btStock.Init(ImageButtonType.InventoryButton);
			btStock.Position = new Point(12, image.Bottom + 28);
			window.Add(btStock);
			btStock.Click += stock_Click;
			The.InGameUI.InventoryPanel.AccessButton = btStock;
			btStock.ToolTip = "View the inventory and give production orders";
			btJobs = new ImageButton(intf.gui);
			btJobs.Init(ImageButtonType.TasksButton);
			btJobs.Position = new Point(12, btStock.Bottom + num);
			window.Add(btJobs);
			btJobs.Click += tbJobs_Click;
			The.InGameUI.JobsPanel.AccessButton = btJobs;
			btJobs.ToolTip = "View current tasks and set priorities";
			btEventDialogs = new ImageButton(intf.gui);
			btEventDialogs.Init(ImageButtonType.EventDialogsButton);
			btEventDialogs.Position = new Point(12, btJobs.Bottom + num);
			window.Add(btEventDialogs);
			btEventDialogs.Click += tbEventDialogs_Click;
			The.InGameUI.EventArchivePanel.AccessButton = btEventDialogs;
			btEventDialogs.ToolTip = "Display event archive";
			btGraphs = new ImageButton(intf.gui);
			btGraphs.Init(ImageButtonType.GraphButton);
			btGraphs.Position = new Point(12, btEventDialogs.Bottom + num);
			window.Add(btGraphs);
			btGraphs.Click += tbGraphs_Click;
			The.InGameUI.GraphPanel.AccessButton = btGraphs;
			btGraphs.ToolTip = "Show graphs";
			btLedger = new ImageButton(intf.gui);
			btLedger.Init(ImageButtonType.LedgerButton);
			btLedger.Position = new Point(12, btGraphs.Bottom + num);
			window.Add(btLedger);
			btLedger.Click += btLedger_Click;
			The.InGameUI.LedgerPanel.AccessButton = btLedger;
			btLedger.ToolTip = "Show ledger";
			btWorld = new ImageButton(intf.gui);
			btWorld.Init(ImageButtonType.DiplomacyButton);
			btWorld.Position = new Point(12, btLedger.Bottom + num);
			window.Add(btWorld);
			btWorld.Click += btWorld_Click;
			The.InGameUI.WorldMapPanel.AccessButton = btWorld;
			btWorld.ToolTip = "View the world map";
			btMissions = new ImageButton(intf.gui);
			btMissions.Init(ImageButtonType.MissionsButton);
			btMissions.Position = new Point(12, btWorld.Bottom + num);
			window.Add(btMissions);
			btMissions.Click += tbMissions_Click;
			The.InGameUI.MissionsPanel.AccessButton = btMissions;
			btMissions.ToolTip = "Arrange an off-map trade mission and transport";
			btPersonnel = new ImageButton(intf.gui);
			btPersonnel.Init(ImageButtonType.PersonnelButton);
			btPersonnel.Position = new Point(12, btMissions.Bottom + num);
			window.Add(btPersonnel);
			btPersonnel.Click += btPersonnel_Click;
			The.InGameUI.PersonnelRosterPanel.AccessButton = btPersonnel;
			btPersonnel.ToolTip = "View the list of colony members";
			btPolicy = new ImageButton(intf.gui);
			btPolicy.Init(ImageButtonType.PolicyButton);
			btPolicy.Position = new Point(12, btPersonnel.Bottom + num);
			window.Add(btPolicy);
			btPolicy.Click += tbPolicy_Click;
			The.InGameUI.PolicyPanel.AccessButton = btPolicy;
			btPolicy.ToolTip = "View or change the colony's policies";
		}
		else
		{
			btEditorPlaceEntity = new ImageButton(intf.gui);
			btEditorPlaceEntity.Init(ImageButtonType.InventoryButton);
			btEditorPlaceEntity.Position = new Point(12, 20);
			window.Add(btEditorPlaceEntity);
			btEditorPlaceEntity.Click += editorPlaceEntity_Click;
			The.InGameUI.SidePanelEditorEntity.AccessButton = btEditorPlaceEntity;
			btEditorPlaceEntity.ToolTip = "Place map assets";
			btEditorPaintTile = new ImageButton(intf.gui);
			btEditorPaintTile.Init(ImageButtonType.DiplomacyButton);
			btEditorPaintTile.Position = new Point(12, btEditorPlaceEntity.Bottom + num);
			window.Add(btEditorPaintTile);
			btEditorPaintTile.Click += btEditorPaintTile_Click;
			The.InGameUI.SidePanelEditorSoil.AccessButton = btEditorPaintTile;
			btEditorPaintTile.ToolTip = "Paint terrain properties";
			btEditorTerrainHeight = new ImageButton(intf.gui);
			btEditorTerrainHeight.Init(ImageButtonType.PolicyButton);
			btEditorTerrainHeight.Position = new Point(12, btEditorPaintTile.Bottom + num);
			window.Add(btEditorTerrainHeight);
			btEditorTerrainHeight.Click += btEditorTerrainHeight_Click;
			The.InGameUI.SidePanelEditorTerrainHeight.AccessButton = btEditorTerrainHeight;
			btEditorTerrainHeight.ToolTip = "Change terrain height";
		}
		Rectangle sourceRectangle = intf.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_smallsplotch");
		Panel.AddImage(intf.gui, window, sourceRectangle, new Point(window.Width - sourceRectangle.Width + 20, 82));
		sourceRectangle = intf.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
		Panel.AddImage(intf.gui, window, sourceRectangle, new Point(0, window.Height - sourceRectangle.Height));
	}

	private void InGameUI_SelectedEntityChangedEvent(EntityID? oldEntity, EntityID? newEntity)
	{
		if (!newEntity.HasValue)
		{
			btEntity.Enabled = false;
		}
		else
		{
			btEntity.Enabled = true;
		}
	}

	private static void btEntity_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ShowSelectedEntityPanel(refreshCurrentPanel: false);
	}

	private static void btZone_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ShowSelectedMapAreaPanel(refreshCurrentPanel: false);
	}

	public void DisableMissions()
	{
		window.Remove(btMissions);
	}

	public void DisableGraphs()
	{
		window.Remove(btGraphs);
	}

	public void DisableContacts()
	{
		window.Remove(btDiplomacy);
	}

	public void DisablePersonell()
	{
		window.Remove(btPersonnel);
	}

	public void DisablePolicy()
	{
		window.Remove(btPolicy);
	}

	public void DisableWorldMap()
	{
		window.Remove(btWorld);
	}

	public void DisableLedger()
	{
		window.Remove(btLedger);
	}

	public void CheckAccessButton(ImageButton bt)
	{
		bt.IsChecked = true;
		DeselectOtherRadioButtons(bt);
	}

	private void DeselectOtherRadioButtons(ImageButton bt)
	{
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			if (bt != btStock)
			{
				btStock.IsChecked = false;
			}
			if (bt != btJobs)
			{
				btJobs.IsChecked = false;
			}
			if (bt != btGraphs)
			{
				btGraphs.IsChecked = false;
			}
			if (bt != btMissions)
			{
				btMissions.IsChecked = false;
			}
			if (bt != btEventDialogs)
			{
				btEventDialogs.IsChecked = false;
			}
			if (bt != btPersonnel)
			{
				btPersonnel.IsChecked = false;
			}
			if (bt != btZone)
			{
				btZone.IsChecked = false;
			}
			if (bt != btEntity)
			{
				btEntity.IsChecked = false;
			}
		}
	}

	private void editorPlaceEntity_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.SidePanelEditorEntity);
		DeselectOtherRadioButtons(btEditorPlaceEntity);
	}

	private void btEditorPaintTile_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.SidePanelEditorSoil);
		DeselectOtherRadioButtons(btEditorPaintTile);
	}

	private void btEditorTerrainHeight_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.SidePanelEditorTerrainHeight);
		DeselectOtherRadioButtons(btEditorTerrainHeight);
	}

	private void stock_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.InventoryPanel);
		DeselectOtherRadioButtons(btStock);
	}

	private void tbEventDialogs_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.EventArchivePanel);
		DeselectOtherRadioButtons(btEventDialogs);
	}

	private void tbGraphs_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.GraphPanel);
		DeselectOtherRadioButtons(btGraphs);
	}

	private void btLedger_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.LedgerPanel);
		DeselectOtherRadioButtons(btLedger);
	}

	private void btWorld_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.WorldMapPanel);
		DeselectOtherRadioButtons(btWorld);
	}

	private void tbDiplomacy_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.DiplomacyPanel);
		DeselectOtherRadioButtons(btDiplomacy);
	}

	private void tbJobs_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.JobsPanel);
		DeselectOtherRadioButtons(btJobs);
	}

	private void tbPolicy_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.PolicyPanel);
		DeselectOtherRadioButtons(btPolicy);
	}

	public void ShowMissions()
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.MissionsPanel);
		DeselectOtherRadioButtons(btMissions);
	}

	private void tbMissions_Click(UIComponent sender, EventArgs e)
	{
		ShowMissions();
	}

	private void btPersonnel_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.PersonnelRosterPanel);
		DeselectOtherRadioButtons(btPersonnel);
	}
}
