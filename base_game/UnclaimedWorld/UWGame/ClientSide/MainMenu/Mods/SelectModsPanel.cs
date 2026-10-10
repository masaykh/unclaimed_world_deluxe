using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.LCD;
using UWGame.Mods;
using WindowSystem;

namespace UWGame.ClientSide.MainMenu.Mods;

/// <summary>
/// PORT: SELECT MODS. tripleacoder, "Main menu", 2026-10-10: "SELECT MODS should be a new window,
/// same size as the scenario picker in NEW GAME. Collapsable panels are probably a good idea, since
/// the list can be very long. When expanded, each mod should be presented with an optional
/// thumbnail, author and small blurb. The scenario picker can be used as the template for the
/// expanded state."
///
/// One folding entry per mod (ModCatalog): its switch on the header's left, as the options menu's
/// categories have; its name; and a summary - whether it runs, who made it, how many of its
/// settings differ from the studio's game. Unfolded, the scenario picker's row: the picture, then
/// what it does and the facts. Switching applies at once and is saved; the settings themselves are
/// changed in OPTIONS -> MODS, as before.
/// </summary>
public class SelectModsPanel : Panel
{
	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Grid surfaceGrid;

	private UIComponent surface;

	private readonly List<CollapsablePanel> entryPanels = new List<CollapsablePanel>();

	private int entriesTop;

	/// <summary>Entries unfolded while the game runs, by mod id. Folded is the default.</summary>
	private static readonly HashSet<string> expanded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public const int ThumbnailSize = 100;

	public event EventHandler CancelClick;

	public SelectModsPanel(SelectModsInterface intf, Point position)
		: base(intf, UWGame.Locale.Text("MODS"), position, new Vector2(800f, 620f), Level.Middle)
	{
		RosterPanel.CreateRosterStyleLCDPanel(intf, Window, out _, out lcdSurface, ref lcdScreen, 55);
		CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, canHaveFocus: false);
		surface = new UIComponent(Interface.gui)
		{
			Width = surfaceGrid.SurfaceWidth,
			Height = surfaceGrid.Height
		};
		surfaceGrid.AddEntry("surface", surface);
		InitButtons();
	}

	private void Populate()
	{
		foreach (CollapsablePanel cp in entryPanels)
		{
			surface.Remove(cp);
		}
		entryPanels.Clear();
		surface.Controls.Clear();
		int y = 6;
		if (ModSettings.StockOnly)
		{
			y = AddNote(UWGame.Locale.Text("Started with -nomods: every mod is off for this session. What you switch here is kept for the next start."), y);
		}
		y = AddNote(UWGame.Locale.Text("Switch a mod on or off with the box on its left; change its settings in OPTIONS -> MODS. A mod that changes items, recipes or the map takes effect at the next game you start or load."), y);
		entriesTop = y + 4;
		foreach (ModCatalog.Entry entry in ModCatalog.Entries())
		{
			AddEntry(entry);
		}
		LayOut();
	}

	private int AddNote(string text, int y)
	{
		TextArea note = new TextArea(Interface.gui, ListBoxType.LCD);
		surface.Add(note);
		note.Init(Label.LabelType.LCDNormal);
		note.CanGrowInHeight = true;
		note.ScrollBarEnabled = false;
		note.HMargin = 0;
		note.VMargin = 0;
		note.X = 8;
		note.Y = y;
		note.Width = surface.Width - 16;
		note.Text = text;
		return note.Bottom + 4;
	}

	private void AddEntry(ModCatalog.Entry entry)
	{
		CollapsablePanel cp = new CollapsablePanel(Interface.gui, CollapsablePanel.PanelType.DropDownBig);
		surface.Add(cp);
		cp.Init();
		cp.Title = entry.Name;
		cp.X = 6;
		cp.Width = surface.Width - cp.X - 6;
		cp.CollapsedHeight = cp.ExpandedPanelYPos + 5;
		cp.Height = cp.CollapsedHeight;
		entryPanels.Add(cp);

		// The scenario picker's row: the picture, then the words.
		UIComponent content = new UIComponent(Interface.gui)
		{
			Width = cp.ExpandedPanel.Width
		};
		cp.AddContent(content);
		int textX = 8;
		Image picture = Thumbnail(entry);
		if (picture != null)
		{
			content.Add(picture);
			picture.X = 8 + (ThumbnailSize - picture.Width) / 2;
			picture.Y = 6 + Math.Max(0, (ThumbnailSize - picture.Height) / 2);
			textX = 8 + ThumbnailSize + 12;
		}
		TextArea text = new TextArea(Interface.gui, ListBoxType.LCD);
		content.Add(text);
		text.Init(Label.LabelType.LCDNormal);
		text.CanGrowInHeight = true;
		text.ScrollBarEnabled = false;
		text.HMargin = 0;
		text.VMargin = 0;
		text.X = textX;
		text.Y = 6;
		text.Width = content.Width - textX - 8;
		text.Text = Details(entry);
		// Fires Resize, which is what sizes the expanded panel (CollapsablePanel.content_Resize).
		content.Height = Math.Max(picture != null ? ThumbnailSize + 12 : 0, text.Bottom + 8);

		ImageButton modSwitch = new ImageButton(Interface.gui);
		cp.Add(modSwitch);
		modSwitch.Init(ImageButtonType.LCDCheckbox);
		modSwitch.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		modSwitch.X = 8;
		cp.CenterOnHeader(modSwitch);
		modSwitch.IsChecked = entry.SwitchedOn;
		modSwitch.Enabled = entry.CanSwitch;
		modSwitch.ToolTip = entry.CanSwitch
			? string.Format(UWGame.Locale.Text("Tick: {0} on. Untick: off - its settings act as the studio's game, and come back as you left them when it is switched on again."), entry.Name)
			: UWGame.Locale.Text("Not now: the game was started with -nomods, or without the mod loader.");
		cp.Summary = Summary(entry);
		modSwitch.Click += delegate
		{
			ModCatalog.Switch(entry, modSwitch.IsChecked, GameStateManagement.UnclaimedWorld.LogError);
			ModCatalog.Entry now = ModCatalog.Entries().FirstOrDefault((ModCatalog.Entry e) => string.Equals(e.Id, entry.Id, StringComparison.OrdinalIgnoreCase)) ?? entry;
			cp.Summary = Summary(now);
			text.Text = Details(now);
			content.Height = Math.Max(picture != null ? ThumbnailSize + 12 : 0, text.Bottom + 8);
		};

		if (expanded.Contains(entry.Id))
		{
			cp.IsExpanded = true;
		}
		cp.HeightResize += delegate
		{
			// By height, not IsExpanded: IsExpanded calls Expand() - which changes the height and
			// raises this - before it updates its own flag (as in OptionsDialog).
			if (cp.Height > cp.CollapsedHeight)
			{
				expanded.Add(entry.Id);
			}
			else
			{
				expanded.Remove(entry.Id);
			}
			LayOut();
		};
	}

	/// <summary>The header's right side: on or off, by whom, how much of it is changed.</summary>
	private static string Summary(ModCatalog.Entry entry)
	{
		var parts = new List<string> { StateText(entry) };
		if (!string.IsNullOrEmpty(entry.Author))
		{
			parts.Add(string.Format(UWGame.Locale.Text("by {0}"), entry.Author));
		}
		if (entry.Settings.Count > 0)
		{
			parts.Add(ModSettings.AllStable(entry.Settings) ? UWGame.Locale.Text("STABLE") : UWGame.Locale.Text("TESTING"));
		}
		return string.Join(" - ", parts);
	}

	private static string StateText(ModCatalog.Entry entry)
	{
		return entry.State switch
		{
			ModCatalog.ModState.On => UWGame.Locale.Text("ON"),
			ModCatalog.ModState.Off => UWGame.Locale.Text("OFF"),
			ModCatalog.ModState.OffThisSession => UWGame.Locale.Text("OFF THIS SESSION"),
			ModCatalog.ModState.Failed => UWGame.Locale.Text("FAILED"),
			_ => entry.SwitchedOn ? UWGame.Locale.Text("ON AT NEXT START") : UWGame.Locale.Text("OFF AT NEXT START"),
		};
	}

	/// <summary>The unfolded words: what it does, then the facts.</summary>
	private static string Details(ModCatalog.Entry entry)
	{
		var lines = new List<string>();
		lines.Add(string.IsNullOrEmpty(entry.Description) ? UWGame.Locale.Text("No description.") : entry.Description);
		lines.Add("");
		if (!string.IsNullOrEmpty(entry.Author))
		{
			lines.Add(string.Format(UWGame.Locale.Text("Made by: {0}"), entry.Author));
		}
		if (entry.Settings.Count > 0)
		{
			lines.Add(string.Format(UWGame.Locale.Text("Settings: {0}, {1} changed from the studio's game - in OPTIONS -> MODS."), entry.Settings.Count, entry.Changed));
		}
		if (entry.FileName != null)
		{
			lines.Add(string.Format(UWGame.Locale.Text("File: user/Mods/{0}"), entry.FileName));
		}
		if (entry.State == ModCatalog.ModState.AtNextStart)
		{
			lines.Add(UWGame.Locale.Text("Takes effect at the next start of the game."));
		}
		if (!string.IsNullOrEmpty(entry.Problem))
		{
			lines.Add(string.Format(UWGame.Locale.Text("Problem: {0}"), entry.Problem));
		}
		// " \n", as the game's own text has it: TextArea breaks lines at a space.
		return string.Join(" \n", lines);
	}

	/// <summary>
	/// The mod's picture: a sprite of the game's own (GUI sprite sheet), or a PNG beside a DLL.
	/// Shown at its own size up to <see cref="ThumbnailSize"/>, scaled down if larger. Null when
	/// there is none, or it cannot be read.
	/// </summary>
	private Image Thumbnail(ModCatalog.Entry entry)
	{
		if (string.IsNullOrEmpty(entry.Thumbnail))
		{
			return null;
		}
		Image image = new Image(Interface.gui);
		Point size;
		try
		{
			if (File.Exists(entry.Thumbnail))
			{
				Texture2D texture;
				using (FileStream stream = File.OpenRead(entry.Thumbnail))
				{
					texture = Texture2D.FromStream(Interface.Game.GraphicsDevice, stream);
				}
				image.Texture = texture;
				size = new Point(texture.Width, texture.Height);
			}
			else if (Interface.gui.GUISpriteSheet.TryGetSourceRectangle(entry.Thumbnail, out var sprite))
			{
				image.SetSkinLocation(SkinState.Normal, sprite.Value);
				image.Texture = Interface.gui.GUISpriteSheet.Texture;
				size = new Point(sprite.Value.Width, sprite.Value.Height);
			}
			else
			{
				return null;
			}
		}
		catch (Exception)
		{
			return null;
		}
		float scale = Math.Min(1f, (float)ThumbnailSize / Math.Max(size.X, size.Y));
		image.ScaleImageToSizeOfControl = true;
		image.Width = (int)(size.X * scale);
		image.Height = (int)(size.Y * scale);
		return image;
	}

	private void LayOut()
	{
		int y = entriesTop;
		foreach (CollapsablePanel cp in entryPanels)
		{
			cp.Y = y;
			y = cp.Bottom + 4;
		}
		surface.Height = Math.Max(surfaceGrid.Height, y + 12);
	}

	private void InitButtons()
	{
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		PlaceLeftButtonUnderLCD(textButton);
		textButton.Text = UWGame.Locale.Text("MAIN");
		textButton.ScaleWidthToFitText();
		textButton.ToolTip = UWGame.Locale.Text("Return to the main menu");
		textButton.Click += btCancel_Click;
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(40, 30));
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
		Populate();
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Window.Hide();
		CancelClick?.Invoke(sender, e);
	}
}
