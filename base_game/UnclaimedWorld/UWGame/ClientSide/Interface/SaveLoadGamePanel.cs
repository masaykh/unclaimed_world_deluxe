using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Interface.LCD;
using UWGame.Mods;
using UWGame.SimSide;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class SaveLoadGamePanel : Panel
{
	public enum SaveOrLoad
	{
		Save,
		Load
	}

	public SaveOrLoad SaveOrLoadValue;

	private Box display;

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Grid grid;

	private TextBox tbFileName;

	private TextArea taNote;

	private ErrorsAndMessages output;

	private const int itemHeight = 112;

	private const int selectButtonWidth = 80;

	private const int horizPadding = 8;

	private const int vertPadding = 5;

	private int itemPadding = 6;

	private string saveFileFullPath;

	/// <summary>The "this save needs different mods" box, created on first use and then reused.</summary>
	private MessageBox modMessageBox;

	/// <summary>What the save being opened was made with, while that box is up.</summary>
	private string pendingSaveMods;

	public string SelectedSaveGamePath { get; private set; }

	public event EventHandler SaveOrLoadClick;

	public event EventHandler CancelClick;

	public SaveLoadGamePanel(SaveOrLoad saveOrLoad, CommonInterface intf, Point position)
		: base(intf, "SAVE / LOAD", position, new Vector2(822f, 620f), Level.Menu)
	{
		SaveOrLoadValue = saveOrLoad;
		RosterPanel.CreateRosterStyleLCDPanel(intf, Window, out display, out lcdSurface, ref lcdScreen);
		int x = 1;
		grid = FullLCDPanel.AddGridWithFixedItemHeights(intf.gui, lcdSurface, 58);
		grid.ItemHeight = 112;
		grid.Selectability = Grid.SelectabilityOptions.None;
		taNote = new TextArea(intf.gui, ListBoxType.LCD);
		taNote.RenderType = RenderType.CRTAndLCD;
		taNote.Init(Label.LabelType.LCDNormal);
		taNote.CanGrowInHeight = true;
		lcdSurface.Add(taNote);
		taNote.X = x;
		taNote.Y = 2;
		taNote.Width = lcdSurface.Width - 4;
		if (SaveOrLoadValue == SaveOrLoad.Save)
		{
			taNote.Text = $"Save files may become obsolete when they are older than the current version {UnclaimedWorld.GetVersionAsString()} of the game. To load those, you must change to the corresponding version of the game using the Steam library list. For more info, go to the Unclaimed World forum on Steam.";
		}
		else
		{
			taNote.Text = $"Select a game to load. \nSave files with a version number lower than the current version {UnclaimedWorld.GetVersionAsString()} of the game may not work. To load those, you must change to the corresponding version of the game using the Steam library list. For more info, go to the Unclaimed World forum on Steam.";
		}
		// The same warning the version note above gives, for the other thing that can make a save
		// unopenable: the modded content it was written with. Stated once, where a player is about
		// to write or open one, rather than as a dialog on every save - a confirmation seen every
		// time is a confirmation nobody reads.
		//
		// ONE LINE, and the detail lives in each row's MODDED tooltip. The first version appended a
		// sentence and the mod list to the note above, which was written to fit the 58px the grid
		// below it starts at - so it grew to four lines and ran over the first save in the list.
		// Reported by Kastuk, with a screenshot of exactly that.
		string moddedNow = ModSettings.Signature();
		if (!string.IsNullOrEmpty(moddedNow))
		{
			taNote.Text += " Modded content is on; saves made with it are marked MODDED.";
		}

		// Whatever the note ended up being, the list starts below it. The studio's fixed 58px
		// margin was right for the studio's text and is not a promise about anyone else's.
		int noteBottom = taNote.Bottom + 4;
		if (noteBottom > grid.Y)
		{
			grid.Height -= noteBottom - grid.Y;
			grid.Y = noteBottom;
		}
		output = new ErrorsAndMessages(lcdSurface, intf.gui, x, taNote.Bottom);
		InitButtons();
	}

	private void InitButtons()
	{
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(40, 30));
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		PlaceLeftButtonUnderLCD(textButton);
		textButton.Text = "CANCEL";
		textButton.ToolTip = "Cancels and closes the dialog.";
		textButton.Click += btCancel_Click;
		textButton.ScaleWidthToFitText();
		if (SaveOrLoadValue == SaveOrLoad.Save)
		{
			TextButton textButton2 = new TextButton(Interface.gui);
			Window.Add(textButton2);
			textButton2.Init(TextButton.TextButtonType.White);
			textButton2.Position = new Point(300, textButton.Y);
			textButton2.Text = "NEW SAVE";
			textButton2.ToolTip = "Saves the game in a new file.";
			textButton2.Click += btSaveNew_Click;
			textButton2.ScaleWidthToFitText();
			tbFileName = new TextBox(Interface.gui);
			Window.Add(tbFileName);
			tbFileName.Position = new Point(textButton2.Right + 12, display.Y + display.Height + 10);
			tbFileName.Width = display.Width - tbFileName.X;
			tbFileName.Height = 27;
			tbFileName.IsEditable = true;
			tbFileName.CenterThisVertically(textButton2.Y + textButton2.Height / 2);
		}
	}

	private void AddItemRow(string fullPath, long fileLength, SnapshotHeader header)
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		LCDInnerPanel lCDInnerPanel = new LCDInnerPanel(Interface.gui, 112, includeDecor: false);
		uIComponent.Add(lCDInnerPanel.Panel);
		Scenario scenario = null;
		if (header.StartGameParams.StartScenarioParams != null)
		{
			scenario = header.StartGameParams.StartScenarioParams.Scenario;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);
		int num = 112 - 2 * itemPadding;
		if (scenario != null && !string.IsNullOrEmpty(scenario.ThumbnailImage))
		{
			Image image = new Image(Interface.gui);
			lCDInnerPanel.AddContentSetFullWidth(image);
			if (!Interface.gui.GUISpriteSheet.TryGetSourceRectangle(scenario.ThumbnailImage, out var spriteRect))
			{
				image.Texture = Interface.gui.ContentManager.Load<Texture2D>(scenario.ThumbnailImage);
			}
			else
			{
				image.SetSkinLocation(SkinState.Normal, spriteRect.Value);
				image.Texture = Interface.gui.GUISpriteSheet.Texture;
			}
			image.Position = new Point(itemPadding, itemPadding);
			image.Height = num;
			image.Width = num;
		}
		LCDInnerPanel lCDInnerPanel2 = new LCDInnerPanel(Interface.gui, 469, includeDecor: false);
		uIComponent.Add(lCDInnerPanel2.Panel);
		lCDInnerPanel2.Panel.X = lCDInnerPanel.Panel.Right - 2;
		lCDInnerPanel2.HorizontalContentPadding = 8;
		lCDInnerPanel2.VerticalContentPadding = 5;
		int num2 = 4;
		Label label = new Label(Interface.gui);
		lCDInnerPanel2.AddContent(label);
		label.Init(Label.LabelType.LCDHeadingBlue);
		label.Text = fileNameWithoutExtension;
		Label label2 = new Label(Interface.gui);
		lCDInnerPanel2.Panel.Add(label2);
		lCDInnerPanel2.AddContentSetFullWidth(label2);
		label2.Init(Label.LabelType.LCDNormal);
		label2.Y = label.Bottom + num2;
		label2.Text = ((scenario != null) ? ("Scenario: " + scenario.DisplayName) : "(Missing)");
		Label label3 = new Label(Interface.gui);
		lCDInnerPanel2.Panel.Add(label3);
		lCDInnerPanel2.AddContentSetFullWidth(label3);
		label3.Init(Label.LabelType.LCDNormal);
		label3.Y = label2.Bottom + num2;
		// Was header.Timestamp.ToString(Config.Culture). Config.Culture is not a display setting -
		// it is also ToUpper and the "N" number formats, and it sits beside the number handling on
		// the XML data path - so a player choosing a culture here to get day-month-year would also
		// be choosing a decimal separator. The format is its own setting now, applied with the
		// invariant culture. See PortSettings.SaveDateFormat.
		label3.Text = "Date: " + ModSettings.FormatDate(header.Timestamp, PortSettings.SaveDateFormat.Value);
		Label label4 = new Label(Interface.gui);
		lCDInnerPanel2.Panel.Add(label4);
		lCDInnerPanel2.AddContentSetFullWidth(label4);
		label4.Init(Label.LabelType.LCDNormal);
		label4.Y = label3.Bottom + num2;
		label4.Text = "Version: " + header.ProgramVersion.ToString();
		Label label5 = new Label(Interface.gui);
		lCDInnerPanel2.Panel.Add(label5);
		lCDInnerPanel2.AddContentSetFullWidth(label5);
		label5.Init(Label.LabelType.LCDNormal);
		label5.Y = label4.Bottom + num2;
		label5.Text = "File size: " + fileLength / 1000000 + " mb";
		LCDInnerPanel lCDInnerPanel3 = new LCDInnerPanel(Interface.gui, 148, includeDecor: false);
		uIComponent.Add(lCDInnerPanel3.Panel);
		lCDInnerPanel3.Panel.X = lCDInnerPanel2.Panel.Right - 2;
		// A save carrying modded content says so, in the list, before it is opened. The header is
		// all this needs - the save list reads headers only - so marking every save costs one
		// string comparison per row and no additional file reading.
		if (!string.IsNullOrEmpty(header.Mods))
		{
			Label lblModded = new Label(Interface.gui);
			lCDInnerPanel3.Panel.Add(lblModded);
			lblModded.Init(Label.LabelType.LCDHeadingRed);
			lblModded.Text = "MODDED";
			lblModded.FitToText();
			lblModded.X = lCDInnerPanel3.Panel.Width - lblModded.Width - 8;
			lblModded.Y = 5;
			lblModded.ToolTip = Common.ComposeHeadingAndBlobText(
				"Made with modded content",
				DescribeSaveMods(header.Mods) +
				Environment.NewLine +
				"Loading it without the same content may fail, because a save names the recipes and " +
				"items it contains.");
		}
		TextButton textButton = new TextButton(Interface.gui);
		lCDInnerPanel3.Panel.Add(textButton);
		textButton.Init(TextButton.TextButtonType.LCD);
		textButton.Text = ((SaveOrLoadValue == SaveOrLoad.Save) ? "SAVE" : "LOAD");
		textButton.ToolTip = ((SaveOrLoadValue == SaveOrLoad.Save) ? "Saves the current game and overwrites this file." : "Loads the game.");
		textButton.ScaleWidthToFitText();
		textButton.X = lCDInnerPanel3.Panel.Width - textButton.Width - lCDInnerPanel3.HorizontalContentPadding;
		textButton.Y = lCDInnerPanel3.Panel.Height - textButton.Height;
		textButton.Tag1 = fullPath;
		// The stamp travels with the button so the click handler does not have to open the file
		// again to find out what the save needs.
		textButton.Tag2 = header.Mods ?? "";
		if (SaveOrLoadValue == SaveOrLoad.Save)
		{
			textButton.Click += btSave_Click;
		}
		else
		{
			textButton.Click += btLoad_Click;
		}
		TextButton textButton2 = new TextButton(Interface.gui);
		lCDInnerPanel3.Panel.Add(textButton2);
		textButton2.Init(TextButton.TextButtonType.LCD);
		textButton2.Text = "DELETE";
		textButton2.ToolTip = "Deletes this saved game file";
		textButton2.ScaleWidthToFitText();
		textButton2.X = 0;
		textButton2.Y = textButton.Y;
		textButton2.Tag1 = fullPath;
		textButton2.Click += tbDelete_Click;
		grid.AddEntry(header, uIComponent);
	}

	private void tbDelete_Click(UIComponent sender, EventArgs e)
	{
		string path = (string)sender.Tag1;
		try
		{
			File.Delete(path);
		}
		catch (Exception ex)
		{
			output.ShowError("Could not delete the file. Message: " + ex.Message);
		}
		PopulateFileList();
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
		SelectedSaveGamePath = null;
		PopulateFileList();
	}

	public static List<string> GetListOfSavedGamePaths()
	{
		string dataFolderPath = Config.GetDataFolderPath(Config.DataType.SaveGames);
		if (!Directory.Exists(dataFolderPath))
		{
			Directory.CreateDirectory(dataFolderPath);
		}
		List<string> list = new List<string>();
		string[] files = Directory.GetFiles(dataFolderPath, "*.*");
		foreach (string item in files)
		{
			list.Add(item);
		}
		return list;
	}

	private List<Tuple<string, long, SnapshotHeader>> GetSnapshotHeaders(List<string> savegamePaths)
	{
		List<Tuple<string, long, SnapshotHeader>> list = new List<Tuple<string, long, SnapshotHeader>>();
		foreach (string savegamePath in savegamePaths)
		{
			long length = new FileInfo(savegamePath).Length;
			using BinaryReader reader = new BinaryReader(new BufferedStream(new GZipStream(File.Open(savegamePath, FileMode.Open), CompressionMode.Decompress), 65536));
			try
			{
				SnapshotHeader item = The.Snapshotter.LoadHeader(reader);
				list.Add(new Tuple<string, long, SnapshotHeader>(savegamePath, length, item));
			}
			catch (Exception)
			{
				Snapshotter.IsSnapshotting = false;
			}
		}
		return list;
	}

	private void PopulateFileList()
	{
		List<string> listOfSavedGamePaths = GetListOfSavedGamePaths();
		List<Tuple<string, long, SnapshotHeader>> snapshotHeaders = GetSnapshotHeaders(listOfSavedGamePaths);
		// UNHIDDEN MOD: newest save first. The Harmony patch had to replace this whole method
		// with a reflection-driven copy to get one sort in; from inside the assembly it is a
		// sort.
		if (UWGame.Mods.UnhiddenMod.Enabled)
		{
			snapshotHeaders = snapshotHeaders.OrderByDescending((Tuple<string, long, SnapshotHeader> x) => x.Item3.Timestamp).ToList();
		}
		grid.BeginAddingEntries();
		grid.Clear();
		foreach (Tuple<string, long, SnapshotHeader> item in snapshotHeaders)
		{
			AddItemRow(item.Item1, item.Item2, item.Item3);
		}
		grid.EndAddingEntries();
	}

	private void btLoad_Click(UIComponent sender, EventArgs e)
	{
		string text = sender.Tag1.ToString();
		Console.WriteLine(sender.Tag1.ToString());
		if (!File.Exists(text))
		{
			output.ShowError("File not found.");
			return;
		}
		SelectedSaveGamePath = text;
		// A save names the recipes and items it contains, so one made with different modded
		// content than this session has may not load at all. Offer to load it with what it was
		// made with - which works, because the data tables are rebuilt on the way in.
		string saveMods = (sender.Tag2 as string) ?? "";
		if (PortSettings.AskAboutModsOnLoad.On
			&& !string.Equals(saveMods, ModSettings.EffectiveSignature, StringComparison.Ordinal))
		{
			AskAboutModsThenLoad(saveMods);
			return;
		}
		StartLoad();
	}

	/// <summary>
	/// What a save was made with, one line each, coloured by what this session has: GREEN for a mod
	/// that is loaded or a setting already at that value, RED for one that is switched off, GREY
	/// for one that is not installed at all.
	///
	/// The colour is the whole value of the tooltip. "This save wants three things" is a fact the
	/// MODDED tag already conveyed; "and you are missing the second one" is the part that tells a
	/// player whether to expect it to open.
	/// </summary>
	private static string DescribeSaveMods(string signature)
	{
		StringBuilder text = new StringBuilder();
		foreach (KeyValuePair<string, ModContentState> part in ModSettings.Explain(signature))
		{
			switch (part.Value)
			{
			case ModContentState.Present:
				Common.AppendPossibleActionText(text, part.Key);
				break;
			case ModContentState.Disabled:
				Common.AppendImpossibleActionText(text, part.Key);
				break;
			default:
				// Grey rather than red, because nothing here can switch this on. Red would send a
				// player through the options menu looking for something that is not in it.
				text.Append(Label.ToLabel(part.Key + " - not installed", UIComponent.lcdDisabledColor));
				break;
			}
			text.Append(Environment.NewLine);
		}
		return text.ToString();
	}

	/// <summary>The load the buttons ultimately arrive at, once the mod question is settled.</summary>
	private void StartLoad()
	{
		if (this.SaveOrLoadClick != null)
		{
			this.SaveOrLoadClick(this, null);
		}
	}

	/// <summary>
	/// The save was made with content this session is not running. Say what the difference is and
	/// let the player choose; OK applies the save's settings FOR THIS SESSION, which is enough
	/// because every path from here rebuilds the data tables before the save is read.
	///
	/// It cannot load or unload a third-party DLL from user/Mods - that is a restart - so a
	/// difference of those is reported and no more.
	/// </summary>
	private void AskAboutModsThenLoad(string saveMods)
	{
		string had = ModSettings.Describe(saveMods) ?? "nothing - a stock save";
		string have = ModSettings.Describe(ModSettings.EffectiveSignature) ?? "nothing - a stock game";
		// One box, created on first use and reused. A new one per click would leave a Window behind
		// in the GUI manager every time somebody changed their mind.
		pendingSaveMods = saveMods;
		if (modMessageBox == null)
		{
			// Wider and taller than the default box. Two lists and a sentence do not fit in
			// 330x250: the first version came out clipped on both sides.
			modMessageBox = new MessageBox(Interface, "DIFFERENT MODS", new Vector2(560f, 420f));
			modMessageBox.OKClick += delegate
			{
				// In memory only. The file is not rewritten: this is "open that save", not "change
				// my settings permanently", and the options menu is where the second one belongs.
				ModSettings.ApplySignature(pendingSaveMods);
				StartLoad();
			};
		}
		modMessageBox.ShowMessage(
			"MADE WITH:" + Environment.NewLine + had + Environment.NewLine + Environment.NewLine +
			"YOU ARE RUNNING:" + Environment.NewLine + have + Environment.NewLine + Environment.NewLine +
			"OK loads it with the save's settings, for this" + Environment.NewLine +
			"session only. CANCEL leaves it alone." + Environment.NewLine + Environment.NewLine +
			"ASK ABOUT MODS WHEN LOADING, in the options," + Environment.NewLine +
			"turns this off.",
			"DIFFERENT MODS", modal: true, MessageBox.ButtonOptions.OKAndCancel);
	}

	private void btSave_Click(UIComponent sender, EventArgs e)
	{
		string fullFilePath = sender.Tag1.ToString();
		Save(fullFilePath);
	}

	private void btSaveNew_Click(UIComponent sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(tbFileName.Text))
		{
			output.ShowError("Please enter a name for the new save file.");
			return;
		}
		string dataFolderPath = Config.GetDataFolderPath(Config.DataType.SaveGames);
		if (!Directory.Exists(dataFolderPath))
		{
			Directory.CreateDirectory(dataFolderPath);
		}
		saveFileFullPath = null;
		try
		{
			saveFileFullPath = Config.GetDataFolderPath(Config.DataType.SaveGames, dataFolderPath, tbFileName.Text);
			saveFileFullPath += ".sav";
		}
		catch (Exception ex)
		{
			output.ShowError("Error occurred: " + ex.Message);
			return;
		}
		if (File.Exists(saveFileFullPath))
		{
			HandleFileAlreadyExists();
		}
		else
		{
			Save(saveFileFullPath);
		}
	}

	private void HandleFileAlreadyExists()
	{
		The.InGameUI.MessageBox.ShowMessage("There is already a save file with that name. Overwrite?", "FILE EXISTS", modal: true, MessageBox.ButtonOptions.OKAndCancel);
		The.InGameUI.MessageBox.OKClick += MessageBoxOverwriteSaveFile_OKClick;
	}

	private void MessageBoxOverwriteSaveFile_OKClick(object sender, EventArgs e)
	{
		The.InGameUI.MessageBox.OKClick -= MessageBoxOverwriteSaveFile_OKClick;
		Save(saveFileFullPath);
	}

	private void Save(string fullFilePath)
	{
		output.ShowMessage("Saving. Please wait");
		SelectedSaveGamePath = fullFilePath;
		Window.Hide();
		if (this.SaveOrLoadClick != null)
		{
			try
			{
				this.SaveOrLoadClick(this, null);
			}
			catch (Sim.FileOpenException ex)
			{
				HandleSaveException(ex);
			}
		}
	}

	private void HandleSaveException(Exception ex)
	{
		Window.Show();
		output.ShowError(ex.Message);
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Window.Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(this, null);
		}
	}

	private void Load(string loadFileName)
	{
	}
}
