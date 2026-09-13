using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class MapEditorSaveLoadPanel : Panel
{
	public enum SaveOrLoad
	{
		Save,
		Load
	}

	private SaveOrLoad saveOrLoad;

	private Box display;

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Grid grid;

	private TextBox tbMapName;

	private Label lblFolderPath;

	private TextArea taMessages;

	private string folderPath;

	public DirectoryInfo SelectedMapFolder;

	public event EventHandler SaveOrLoadClick;

	public event EventHandler CancelClick;

	public MapEditorSaveLoadPanel(SaveOrLoad saveOrLoad, CommonInterface intf, Point position)
		: base(intf, (saveOrLoad == SaveOrLoad.Save) ? "SAVE MAP" : "LOAD MAP", position, new Vector2(440f, 560f), Level.Dialogs)
	{
		this.saveOrLoad = saveOrLoad;
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 80, new Point(16, MarginTop), out display, out lcdSurface, ref lcdScreen);
		taMessages = new TextArea(intf.gui, ListBoxType.LCD);
		taMessages.RenderType = RenderType.CRTAndLCD;
		lcdSurface.Add(taMessages);
		taMessages.Init(Label.LabelType.LCDNormal);
		taMessages.Width = lcdSurface.Width;
		taMessages.CanGrowInHeight = false;
		taMessages.ScrollBarEnabled = false;
		taMessages.Height = 42;
		DisplayPrompt();
		lblFolderPath = new Label(intf.gui);
		lcdSurface.Add(lblFolderPath);
		lblFolderPath.Init(Label.LabelType.LCDNormal);
		lblFolderPath.Y = taMessages.Bottom;
		lblFolderPath.TooltipWidth = 300;
		lblFolderPath.TooltipExpires = false;
		int gridTopMargin = lblFolderPath.Bottom + 6;
		grid = FullLCDPanel.AddGridWithFixedItemHeights(intf.gui, lcdSurface, gridTopMargin);
		if (saveOrLoad == SaveOrLoad.Save)
		{
			grid.SelectedChanged += terrainGrid_SelectedChanged;
		}
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		textButton.Text = ((saveOrLoad == SaveOrLoad.Save) ? "SAVE" : "LOAD");
		textButton.ToolTip = ((saveOrLoad == SaveOrLoad.Save) ? "Saves the map data." : "Loads a new map.");
		textButton.Click += btSaveLoad_Click;
		textButton.ScaleWidthToFitText();
		PlaceRightButtonUnderLCD(textButton);
		TextButton textButton2 = new TextButton(Interface.gui);
		Window.Add(textButton2);
		textButton2.Init(TextButton.TextButtonType.White);
		textButton2.Text = "CANCEL";
		textButton2.ToolTip = "Cancels and closes the dialog.";
		textButton2.ScaleWidthToFitText();
		textButton2.Click += btCancel_Click;
		PlaceLeftButtonUnderLCD(textButton2);
		if (saveOrLoad == SaveOrLoad.Save)
		{
			tbMapName = new TextBox(Interface.gui);
			Window.Add(tbMapName);
			tbMapName.Position = new Point(display.X, display.Bottom + 6);
			tbMapName.Width = display.Width;
			tbMapName.Height = 27;
			tbMapName.IsEditable = true;
			tbMapName.DebugTag = "editorMapName";
		}
		AddDefaultDirt();
	}

	private void DisplayPrompt()
	{
		taMessages.Text = ((saveOrLoad == SaveOrLoad.Save) ? "Select an existing map file to overwrite or enter a new name in the box below." : "Select a map to load.");
	}

	private string ShortenPath(string path, int maxLength)
	{
		_ = path.Length;
		string[] array = path.Split('\\');
		int num = (array.Length - 1) / 2;
		int num2 = num;
		string text = "";
		text = string.Join("\\", array, 0, array.Length);
		decimal num3 = default(decimal);
		int num4 = 1;
		while (text.Length >= maxLength && num2 != 0 && num2 != -1)
		{
			array[num2] = "...";
			text = string.Join("\\", array, 0, array.Length);
			num3 += 0.5m;
			num4 *= -1;
			num2 = num + (int)num3 * num4;
		}
		return text;
	}

	private void DisplayFolderPath(string path)
	{
		string text = ShortenPath(path, 55);
		string text2 = "Path: " + text;
		lblFolderPath.Text = text2;
		lblFolderPath.ToolTip = path;
	}

	public static List<DirectoryInfo> GetListOfMapFolders()
	{
		return GetListOfMapFolders(GetDefaultPath());
	}

	public static List<DirectoryInfo> GetListOfMapFolders(string baseFolderPath)
	{
		List<DirectoryInfo> list = new List<DirectoryInfo>();
		string[] directories = Directory.GetDirectories(baseFolderPath, "*", SearchOption.TopDirectoryOnly);
		foreach (string path in directories)
		{
			if (Directory.GetFiles(path, "MapData.xml").Length != 0)
			{
				DirectoryInfo item = new DirectoryInfo(path);
				list.Add(item);
			}
		}
		return list;
	}

	private static string GetDefaultPath()
	{
		string path = ((!Debugger.IsAttached) ? Config.GetDataFolderPath(Config.DataType.RGMap) : "..//..//..//data/Maps");
		return Path.GetFullPath(path);
	}

	private void PopulateFileList()
	{
		List<DirectoryInfo> listOfMapFolders = GetListOfMapFolders(folderPath);
		grid.BeginAddingEntries();
		grid.Clear();
		foreach (DirectoryInfo item in listOfMapFolders)
		{
			grid.AddEntry(item, item.Name);
		}
		grid.EndAddingEntries();
	}

	private void btClose_Click(UIComponent sender, EventArgs e)
	{
		Window.Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(sender, e);
		}
	}

	private void btSaveLoad_Click(UIComponent sender, EventArgs e)
	{
		object key;
		if (saveOrLoad == SaveOrLoad.Save)
		{
			try
			{
				string fullFolderPath = Path.Combine(folderPath, tbMapName.Text);
				string text = MapManager.ComposeMapDataXmlFilePathFromFolderPath(fullFolderPath);
				if (File.Exists(text))
				{
					MapData mapData = MapLoader.LoadMapData(text, tbMapName.Text);
					The.Map.SaveMap(fullFolderPath, mapData, createNewFolders: false);
				}
				else
				{
					MapData mapData2 = new MapData();
					mapData2.Name = tbMapName.Text;
					mapData2.FolderName = tbMapName.Text;
					The.Map.SaveMap(fullFolderPath, mapData2, createNewFolders: true);
				}
				taMessages.Text = "The map was saved.";
				PopulateFileList();
			}
			catch (Exception ex)
			{
				taMessages.Text = "An error occurred: " + ex.Message;
			}
		}
		else if (grid.GetSelectedKey(out key))
		{
			SelectedMapFolder = (DirectoryInfo)key;
		}
		else
		{
			SelectedMapFolder = null;
		}
		if (this.SaveOrLoadClick != null)
		{
			this.SaveOrLoadClick(sender, e);
		}
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
		DisplayPrompt();
		folderPath = GetDefaultPath();
		DisplayFolderPath(folderPath);
		PopulateFileList();
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(sender, e);
		}
	}

	private void terrainGrid_SelectedChanged(UIComponent sender)
	{
		if (sender is Grid grid && grid.GetSelectedKey(out var key))
		{
			string name = ((DirectoryInfo)key).Name;
			tbMapName.Text = name;
		}
	}
}
