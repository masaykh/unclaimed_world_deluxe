using System;
using System.Collections.Generic;
using System.IO;
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
/// One folding entry per mod (ModCatalog; <see cref="SelectModsEntry"/>): its switch left of the
/// bar; its name; and a summary - whether it runs, who made it, how many of its settings differ
/// from the studio's game. Unfolded, the scenario picker's row: the picture, then what it does and
/// the facts; and, for a mod that is on and has settings, MOD OPTIONS - the settings, built by the
/// code OPTIONS -> MODS uses (ModSettingRows). Switching and every setting apply at once and are
/// saved. Keys stay in OPTIONS.
/// </summary>
public class SelectModsPanel : Panel
{
	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Grid surfaceGrid;

	private UIComponent surface;

	private readonly List<SelectModsEntry> entries = new List<SelectModsEntry>();

	private int entriesTop;

	/// <summary>
	/// Entries unfolded while the game runs, by mod id - and their MOD OPTIONS, by mod id and
	/// <see cref="SelectModsEntry.OptionsKeySuffix"/>. Folded is the default.
	/// </summary>
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
		foreach (SelectModsEntry e in entries)
		{
			surface.Remove(e.Bar);
			surface.Remove(e.Switch);
		}
		entries.Clear();
		surface.Controls.Clear();
		// One set of rows for every entry's MOD OPTIONS: the code OPTIONS -> MODS builds its categories with.
		var rows = new ModSettingRows(Interface.gui);
		int y = 6;
		if (ModSettings.StockOnly)
		{
			y = AddNote(UWGame.Locale.Text("Started with -nomods: every mod is off for this session. What you switch here is kept for the next start."), y);
		}
		y = AddNote(UWGame.Locale.Text("Switch a mod on or off with the box on its left. Unfold a mod that is on, then MOD OPTIONS, to change its settings - they apply and are saved at once; keys are set in OPTIONS. A mod that changes items, recipes or the map takes effect at the next game you start or load."), y);
		entriesTop = y + 4;
		foreach (ModCatalog.Entry entry in ModCatalog.Entries())
		{
			entries.Add(new SelectModsEntry(Interface.gui, surface, entry, Thumbnail(entry), rows, expanded, LayOut, GameStateManagement.UnclaimedWorld.LogError));
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
		foreach (SelectModsEntry e in entries)
		{
			y = e.LayOut(y);
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
		// A text box is read when it loses focus; whatever is still in one is kept too.
		foreach (SelectModsEntry entry in entries)
		{
			entry.ApplyPending();
		}
		Window.Hide();
		CancelClick?.Invoke(sender, e);
	}
}
