using System.Collections.Generic;
using UWGame.ClientSide.HelpTopics;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class HUDHelpPanel : HUDWindow
{
	private Grid grid;

	public HUDHelpPanel()
		: base(204, 230, hasSurface: true, hasCloseButton: false, isMovable: false, "HUD_window_base", hideWhenMouseExits: true, Level.Menu)
	{
		CreateMenuGrid(out grid);
		grid.Selectability = Grid.SelectabilityOptions.Single;
		Populate();
		grid.SelectedChanged += grid_SelectedChanged;
	}

	private void grid_SelectedChanged(UIComponent sender)
	{
		HelpTopic key = null;
		if (sender is Grid grid && grid.GetKey(grid.SelectedItem, out var key2))
		{
			key = (HelpTopic)key2;
		}
		HelpTopicDialog helpTopicDialog = The.InGameUI.HelpTopicDialogs[key];
		helpTopicDialog.ShowInScreenSpace(0, 200);
		helpTopicDialog.DisplayWindow.CenterWindow();
	}

	public void Populate()
	{
		grid.BeginAddingEntries();
		foreach (KeyValuePair<string, HelpTopic> allHelpTopic in GameData.Instance.AllHelpTopics)
		{
			grid.AddEntry(allHelpTopic.Value, allHelpTopic.Value.Name.ToUpper(Config.Culture)).OrderByTag1 = allHelpTopic.Value.Name;
		}
		grid.Sort((UIComponent t) => t.OrderByTag1, Grid.Sorting.Ascending);
		grid.EndAddingEntries();
	}

	public override void Hide()
	{
		base.Hide();
		if (The.Sim.Mode != Sim.EngineMode.Edit)
		{
			The.InGameUI.MainPanel.btHelp.IsChecked = false;
		}
	}
}
