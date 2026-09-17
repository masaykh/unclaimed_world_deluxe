using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.Control.Replays;
using WindowSystem;

namespace UWGame.Client.MainMenu.LoadReplay;

public class LoadReplayPanel : Panel
{
	private Box display;

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Grid grid;

	private TextBox tbReplayPauseTime;

	private Label lblMessages;

	private Label folderPathLabel;

	private string replayFolderPath;

	public string SelectedLoadReplayPath { get; private set; }

	public float? TimeToPauseReplay { get; private set; }

	public event EventHandler CancelClick;

	public event EventHandler LoadClick;

	public LoadReplayPanel(CommonInterface intf, Point position)
		: base(intf, "LOAD REPLAY", position, new Vector2(440f, 560f), Level.Dialogs)
	{
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 160, new Point(16, 60), out display, out lcdSurface, ref lcdScreen);
		lblMessages = new Label(intf.gui);
		lcdSurface.Add(lblMessages);
		lblMessages.Text = "Select a replay to load.";
		lblMessages.Init(Label.LabelType.LCDNormal);
		grid = FullLCDPanel.AddGridWithFixedItemHeights(intf.gui, lcdSurface, 30);
		folderPathLabel = new Label(intf.gui);
		folderPathLabel.Init(Label.LabelType.LCDNormal);
		InitButtons();
		PopulateFileList();
		Label label = new Label(intf.gui);
		Window.Add(label);
		label.Init(Label.LabelType.PlainPanelNormal);
		label.Text = "Time (s) to pause:";
		label.FitToText();
		label.X = display.X;
		label.Y = display.Bottom + 12;
		tbReplayPauseTime = new TextBox(intf.gui);
		Window.Add(tbReplayPauseTime);
		tbReplayPauseTime.Position = new Point(label.Right + 12, display.Bottom + 12);
		tbReplayPauseTime.Width = display.Width - tbReplayPauseTime.X;
		tbReplayPauseTime.Height = 27;
		tbReplayPauseTime.IsEditable = true;
		tbReplayPauseTime.DebugTag = "replayPanelPauseTime";
	}

	/// <summary>
	/// The key of the row that shows where the replays live. It is a LABEL, not a replay, and the
	/// grid does not know the difference - so it has a name and buttonLoad_Click refuses it.
	/// </summary>
	private const string FolderPathRowKey = "folderPath";

	private void PopulateFileList()
	{
		replayFolderPath = Config.GetDataFolderPath(Config.DataType.Replays);
		List<DirectoryInfo> directories;
		List<string> listOfReplayFolders = Recorder.GetListOfReplayFolders(replayFolderPath, out directories);
		grid.BeginAddingEntries();
		grid.Clear();
		grid.AddEntry(FolderPathRowKey, folderPathLabel);
		folderPathLabel.Text = replayFolderPath + " :";
		foreach (string item in listOfReplayFolders)
		{
			grid.AddEntry(item, item);
		}
		grid.EndAddingEntries();
		// PORT FIX. An empty list used to look exactly like a full one with nothing selected, and
		// the only row on it was the folder-path label - which LOAD would happily try to open.
		lblMessages.Text = listOfReplayFolders.Count == 0
			? "No replays recorded yet. Switch on RECORD SESSIONS FOR REPLAY and play a game."
			: "Select a replay to load.";
	}

	private void btClose_Click(UIComponent sender, EventArgs e)
	{
		Window.Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(sender, e);
		}
	}

	private void InitButtons()
	{
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(40, 30));
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		textButton.Text = "LOAD";
		textButton.ToolTip = "Loads a replay.";
		textButton.Click += buttonLoad_Click;
		textButton.ScaleWidthToFitText();
		PlaceRightButtonUnderLCD(textButton);
		TextButton textButton2 = new TextButton(Interface.gui);
		Window.Add(textButton2);
		textButton2.Init(TextButton.TextButtonType.White);
		PlaceLeftButtonUnderLCD(textButton2);
		textButton2.Text = "CANCEL";
		textButton2.ToolTip = "Cancels and closes the dialog.";
		textButton2.ScaleWidthToFitText();
		textButton2.Click += btCancel_Click;
	}

	private void buttonLoad_Click(UIComponent sender, EventArgs e)
	{
		if (!grid.GetSelectedKey(out var key))
		{
			lblMessages.Text = "Select a replay first.";
			return;
		}
		string text = key.ToString();

		// PORT FIX. The folder-path row is a label in the same grid as the replays, so it can be
		// selected like one - and with nothing recorded it is the ONLY row there is. LOAD then
		// built a path to a folder called "folderPath" and ReplayData.LoadReplay opened
		// Commands.xml inside it, which threw DirectoryNotFoundException and took the game down.
		// Reported as "replay is crashing / just lacks prepared replays in there".
		if (string.Equals(text, FolderPathRowKey, StringComparison.Ordinal))
		{
			lblMessages.Text = "That is the folder, not a replay. Pick one of the entries below it.";
			return;
		}

		string candidate = Path.Combine(replayFolderPath, text);
		// And a folder that is there but is not a replay - half-deleted, or copied in by hand -
		// is refused for the same reason rather than part-way through loading it.
		if (!File.Exists(Path.Combine(candidate, "Replay.UWRep"))
			|| !File.Exists(Path.Combine(candidate, "Commands.xml"))
			|| !File.Exists(Path.Combine(candidate, "GameParams.xml")))
		{
			lblMessages.Text = "That folder is missing Replay.UWRep, Commands.xml or GameParams.xml.";
			return;
		}

		SelectedLoadReplayPath = candidate;
		TimeToPauseReplay = tbReplayPauseTime.GetNumber();
		if (this.LoadClick != null)
		{
			this.LoadClick(this, null);
		}
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
		TimeToPauseReplay = null;
		SelectedLoadReplayPath = null;
		PopulateFileList();
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Window.Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(sender, e);
		}
	}

	public override void Update(GameTime elapsed)
	{
		base.Update(elapsed);
	}
}
