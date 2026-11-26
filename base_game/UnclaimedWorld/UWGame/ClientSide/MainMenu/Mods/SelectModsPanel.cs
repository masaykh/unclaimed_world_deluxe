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
/// One folding entry per mod (ModCatalog; <see cref="SelectModsEntry"/>): its switch left of the
/// bar; its name; and a summary - whether it runs, who made it, how many of its settings differ
/// from the studio's game. Unfolded, the scenario picker's row: the picture, then what it does and
/// the facts; and, for a mod that is on and has settings, MOD OPTIONS - the settings, keys included,
/// built by the code OPTIONS builds its rows with (ModSettingRows). Switching and every setting
/// apply at once and are saved. First comes PORT: the port's own settings, always on.
///
/// THE ONE PLACE FOR MODS. The user, 2026-10-18: "all mods should be explicitly under modding".
/// Every registered setting is under an entry here, except LANGUAGE, which is OPTIONS'
/// (ModCatalog.InOptions); the studio's own keys stay in OPTIONS too. OPTIONS opens this panel with
/// its MOD OPTIONS... button (<see cref="Mode"/>): from the main menu as it is here, and in game
/// titled MOD OPTIONS, without the switches - a mod is not switched on or off mid-game.
/// </summary>
public class SelectModsPanel : Panel
{
	/// <summary>Where the panel is shown, which decides its title, its switches and its button.</summary>
	public enum Mode
	{
		/// <summary>Main menu -> MODDING -> SELECT MODS (SelectModsScreen): MAIN returns to the main menu.</summary>
		SelectMods,

		/// <summary>OPTIONS -> MOD OPTIONS... in the main menu: the same, over OPTIONS, which BACK returns to.</summary>
		FromOptions,

		/// <summary>OPTIONS -> MOD OPTIONS... in game: titled MOD OPTIONS, no switches; BACK returns to OPTIONS.</summary>
		InGame,
	}

	private readonly Mode mode;

	/// <summary>The panel's title: MOD OPTIONS in game, where it only changes settings; MODS elsewhere.</summary>
	public static string TitleFor(Mode mode) => (mode == Mode.InGame) ? UWGame.Locale.Text("MOD OPTIONS") : UWGame.Locale.Text("MODS");

	/// <summary>Whether the panel shows the entries' on/off switches: not in game.</summary>
	public static bool ShowsSwitches(Mode mode) => mode != Mode.InGame;

	/// <summary>The panel most recently shown; open while its window is visible (<see cref="HandleKeys"/>).</summary>
	private static SelectModsPanel shown;

	/// <summary>The rows of every entry's MOD OPTIONS.</summary>
	private ModSettingRows rows;

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Grid surfaceGrid;

	private UIComponent surface;

	private readonly List<SelectModsEntry> entries = new List<SelectModsEntry>();

	private int entriesTop;

	/// <summary>PRESET's list, its name box for SAVE, and what the last preset button did (ModPresets).</summary>
	private ComboBox presetCombo;

	private TextBox presetName;

	private static string presetMessage;

	/// <summary>The preset last applied or saved, chosen in the list when the panel opens again.</summary>
	private static string lastPreset;

	/// <summary>
	/// Set by a preset button: the entries are built again on the next frame (<see cref="HandleKeys"/>),
	/// not inside the click, which would empty the surface the button sits on while it is being clicked.
	/// </summary>
	private bool repopulate;

	/// <summary>
	/// Entries unfolded while the game runs, by mod id - and their MOD OPTIONS, by mod id and
	/// <see cref="SelectModsEntry.OptionsKeySuffix"/>. Folded is the default.
	/// </summary>
	private static readonly HashSet<string> expanded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public const int ThumbnailSize = 100;

	public event EventHandler CancelClick;

	public SelectModsPanel(CommonInterface intf, Mode mode = Mode.SelectMods)
		: base(intf, TitleFor(mode), Point.Zero, new Vector2(800f, 620f),
			// Over OPTIONS, which is Level.Menu; on its own screen, as the scenario picker.
			(mode == Mode.SelectMods) ? Level.Middle : Level.Menu)
	{
		this.mode = mode;
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
			if (e.Switch.Parent != null)
			{
				surface.Remove(e.Switch);
			}
		}
		entries.Clear();
		surface.Controls.Clear();
		rows?.Clear();
		// One set of rows for every entry's MOD OPTIONS: the code OPTIONS builds its rows with.
		rows = new ModSettingRows(Interface.gui);
		// A mod's key that one of the studio's keys (OPTIONS -> KEYS) also uses turns red.
		Options options = Interface.Game.Controller.Options;
		rows.KeysElsewhere = () => KeybindMod.VanillaFields().Select(f => f.Field.GetValue(options).ToString());
		bool switches = ShowsSwitches(mode);
		int y = 6;
		if (ModSettings.StockOnly)
		{
			y = AddNote(UWGame.Locale.Text("Started with -nomods: every mod is off for this session. What you switch here is kept for the next start."), y);
		}
		y = AddNote(switches
			? UWGame.Locale.Text("Switch a mod on or off with the box on its left. Unfold a mod that is on, then MOD OPTIONS, to change its settings and keys - they apply and are saved at once. The language and the studio's own keys are set in OPTIONS. A mod that changes items, recipes or the map takes effect at the next game you start or load.")
			: UWGame.Locale.Text("Unfold a mod that is on, then MOD OPTIONS, to change its settings and keys - they apply and are saved at once. Mods are switched on and off in the main menu, under MODDING -> SELECT MODS. A mod that changes items, recipes or the map takes effect at the next game you start or load."), y);
		if (switches && !ModSettings.StockOnly)
		{
			y = AddPresets(y);
		}
		entriesTop = y + 4;
		foreach (ModCatalog.Entry entry in ModCatalog.Entries())
		{
			entries.Add(new SelectModsEntry(Interface.gui, surface, entry, Thumbnail(entry), rows, expanded, LayOut, GameStateManagement.UnclaimedWorld.LogError, switches));
		}
		rows.MarkDuplicateKeys();
		LayOut();
	}

	/// <summary>
	/// NEW (Kastuk, "Mod settings", 2026-11-14: "saveable presets of activated mods. Especially to
	/// disable all mods beside debug ones"): PRESET - a list, APPLY and DELETE - and SAVE AS - a
	/// name and SAVE. See ModPresets. Returns the y below them.
	/// </summary>
	private int AddPresets(int y)
	{
		Label caption = AddCaption(UWGame.Locale.Text("PRESET") + ":", y);
		presetCombo = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		surface.Add(presetCombo);
		presetCombo.Init(ComboBoxTypes.LCD);
		presetCombo.Width = 260;
		presetCombo.X = 130;
		presetCombo.CenterThisVertically(caption.Y + caption.Height / 2);
		presetCombo.ToolTip = UWGame.Locale.Text("A preset is a list of mods switched on. APPLY switches those on and every other mod off; their settings stay as you left them. STOCK + DEBUG is the studio's game with only the debugging tools.");
		presetCombo.TooltipExpires = false;
		foreach (ModPresets.Preset preset in ModPresets.All(GameStateManagement.UnclaimedWorld.LogError))
		{
			presetCombo.AddEntry(preset.Name, preset.Name);
		}
		presetCombo.SelectedKey = (lastPreset != null && presetCombo.EntriesByKey.ContainsKey(lastPreset)) ? lastPreset : ModPresets.StockAndDebugName;
		TextButton apply = AddPresetButton(UWGame.Locale.Text("APPLY"), presetCombo.Right + 8, presetCombo);
		apply.Click += delegate
		{
			ApplyPreset();
		};
		TextButton delete = AddPresetButton(UWGame.Locale.Text("DELETE"), apply.Right + 8, presetCombo);
		delete.Click += delegate
		{
			DeletePreset();
		};
		y = Math.Max(caption.Bottom, presetCombo.Bottom) + 8;

		Label saveCaption = AddCaption(UWGame.Locale.Text("SAVE AS") + ":", y);
		presetName = new TextBox(Interface.gui);
		surface.Add(presetName);
		presetName.Init(TextBox.TextBoxType.LCD);
		presetName.X = 130;
		presetName.Width = 260;
		presetName.Height = 20;
		presetName.VMargin = 1;
		presetName.IsEditable = true;
		presetName.CenterThisVertically(saveCaption.Y + saveCaption.Height / 2);
		presetName.ToolTip = UWGame.Locale.Text("A name for the mods switched on now. A preset of that name is replaced.");
		TextButton save = AddPresetButton(UWGame.Locale.Text("SAVE"), presetName.Right + 8, presetName);
		save.Click += delegate
		{
			SavePreset();
		};
		y = Math.Max(saveCaption.Bottom, presetName.Bottom) + 6;
		if (!string.IsNullOrEmpty(presetMessage))
		{
			y = AddNote(presetMessage, y);
		}
		return y;
	}

	private Label AddCaption(string text, int y)
	{
		Label caption = new Label(Interface.gui);
		surface.Add(caption);
		caption.Init(Label.LabelType.LCDSmallHeadingBanner);
		caption.Text = text;
		caption.FitToText();
		caption.X = 8;
		caption.Y = y;
		return caption;
	}

	private TextButton AddPresetButton(string text, int x, UIComponent beside)
	{
		TextButton button = new TextButton(Interface.gui);
		surface.Add(button);
		button.Init(TextButton.TextButtonType.LCDToolTipBlack);
		button.CheckedMode = CheckedModes.CannotBeChecked;
		button.Text = text;
		button.ScaleWidthToFitText();
		button.X = x;
		button.CenterThisVertically(beside.Y + beside.Height / 2);
		return button;
	}

	private string SelectedPreset => presetCombo?.SelectedKey as string;

	private void ApplyPreset()
	{
		string name = SelectedPreset;
		ModPresets.Preset preset = ModPresets.All(GameStateManagement.UnclaimedWorld.LogError).FirstOrDefault((ModPresets.Preset p) => p.Name == name);
		if (preset == null)
		{
			return;
		}
		int switched = ModPresets.Apply(preset, ModCatalog.Entries(), GameStateManagement.UnclaimedWorld.LogError);
		lastPreset = preset.Name;
		presetMessage = string.Format(UWGame.Locale.Text("{0} applied: {1} mods switched."), preset.Name, switched);
		repopulate = true;
	}

	private void SavePreset()
	{
		string name = ModPresets.NormalizeName(presetName?.Text);
		if (!ModPresets.CanSaveAs(name))
		{
			presetMessage = (name == null)
				? UWGame.Locale.Text("Type a name for the preset first.")
				: string.Format(UWGame.Locale.Text("{0} comes with the game and cannot be replaced. Choose another name."), name);
		}
		else if (ModPresets.Save(name, ModCatalog.Entries(), GameStateManagement.UnclaimedWorld.LogError))
		{
			lastPreset = name;
			presetMessage = string.Format(UWGame.Locale.Text("Saved {0}."), name);
		}
		else
		{
			presetMessage = UWGame.Locale.Text("The preset could not be saved - see Errors.txt.");
		}
		repopulate = true;
	}

	private void DeletePreset()
	{
		string name = SelectedPreset;
		if (name == null)
		{
			return;
		}
		if (ModPresets.BuiltIns().Any((ModPresets.Preset p) => p.Name == name))
		{
			presetMessage = string.Format(UWGame.Locale.Text("{0} comes with the game and cannot be deleted."), name);
		}
		else if (ModPresets.Delete(name, GameStateManagement.UnclaimedWorld.LogError))
		{
			presetMessage = string.Format(UWGame.Locale.Text("Deleted {0}."), name);
		}
		repopulate = true;
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
		textButton.Text = (mode == Mode.SelectMods) ? UWGame.Locale.Text("MAIN") : UWGame.Locale.Text("BACK");
		textButton.ScaleWidthToFitText();
		textButton.ToolTip = (mode == Mode.SelectMods) ? UWGame.Locale.Text("Return to the main menu") : UWGame.Locale.Text("Return to OPTIONS");
		textButton.Click += btCancel_Click;
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(40, 30));
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
		Populate();
		shown = this;
	}

	/// <summary>
	/// Escape is MAIN or BACK while the panel is open. Called from SelectModsScreen.HandleInput, and
	/// - for the panel shown over OPTIONS - from OptionsDialog.HandleKeys, after a key button
	/// waiting for its key (ModSettingRows.CaptureKey) has had the key. Returns whether it took it.
	/// </summary>
	public static bool HandleKeys(InputEventSystem.InputData input)
	{
		SelectModsPanel panel = shown;
		if (panel == null || input == null || panel.Window == null || !panel.Window.IsVisibleAndActive)
		{
			return false;
		}
		if (panel.repopulate)
		{
			panel.repopulate = false;
			panel.Populate();
		}
		if (input.IsKeyTapped(Microsoft.Xna.Framework.Input.Keys.Escape))
		{
			panel.btCancel_Click(null, null);
			return true;
		}
		return false;
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		rows?.CancelKeyCapture();
		// A text box is read when it loses focus; whatever is still in one is kept too.
		foreach (SelectModsEntry entry in entries)
		{
			entry.ApplyPending();
		}
		Window.Hide();
		CancelClick?.Invoke(sender, e);
	}
}
