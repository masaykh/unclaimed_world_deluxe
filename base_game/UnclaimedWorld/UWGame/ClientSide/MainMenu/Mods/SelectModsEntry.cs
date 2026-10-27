using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.ClientSide.Interface;
using UWGame.Mods;
using WindowSystem;

namespace UWGame.ClientSide.MainMenu.Mods;

/// <summary>
/// PORT: one mod in SELECT MODS (<see cref="SelectModsPanel"/>): its switch, its folding bar, and
/// - unfolded - the scenario picker's row (the picture, what it does, the facts) and, when the mod
/// is on and has settings, a second folding bar MOD OPTIONS with them.
///
/// THE SWITCH sits left of the bar, outside it. tripleacoder, "Main menu", 2026-10-12: "The
/// checkbox that enables the mod is not really designed to appear on top of the expander bar... it
/// goes outside the borders of the bar. Maybe give the expander panel a left margin and show the
/// checkbox to the left of the expander panel." So the bar starts at <see cref="BarX"/> and the
/// switch is the surface's, at <see cref="SwitchX"/>, centred on the bar's header.
///
/// MOD OPTIONS. tripleacoder: "show the options for enabled mods on the new Select mods dialog too,
/// but without copying logic ... Maybe there should be an extra expander button MOD OPTIONS inside
/// each mod panel so the dialog stays fairly clean." Kastuk: "Settings of a mod may come under mod
/// header. Keys binding can stay in game settings as part of vanilla settings." / "Expand under
/// expand is my favorite." The rows are <see cref="ModSettingRows"/>' - the code OPTIONS builds
/// its rows with - and a change is applied and saved as it is made (there is no OK here), through
/// <see cref="ModSettingRows.ApplyChanged"/> and ModSettings.Save. Every setting of the mod is
/// here, its keys too (a key button, as OPTIONS -> KEYS has; the user, 2026-10-18: "all mods should
/// be explicitly under modding"). Shown only while the mod is on: a switched-off mod's settings
/// read stock and the file keeps the player's values (ModSettings.SetModEnabled), so a change made
/// to them here would not be kept. PORT's (<see cref="ModCatalog.Entry.AlwaysOn"/>) always shows,
/// and has no switch. A mod whose one setting is a single switch has no MOD OPTIONS beside its own
/// switch, which is that setting (<see cref="ModCatalog.Entry.SwitchIsItsSetting"/>) - except in
/// game, where its own switch is not shown.
/// </summary>
public sealed class SelectModsEntry
{
	/// <summary>The switch's left edge on the surface.</summary>
	public const int SwitchX = 6;

	/// <summary>Between the switch's right edge and the bar.</summary>
	public const int SwitchGap = 6;

	/// <summary>Right of the bar, before the surface's edge.</summary>
	public const int RightMargin = 6;

	/// <summary>Left and right of MOD OPTIONS inside the unfolded mod.</summary>
	public const int OptionsInset = 8;

	/// <summary>What <see cref="SelectModsPanel"/>'s remembered set holds for an unfolded MOD OPTIONS: the mod id and this.</summary>
	public const string OptionsKeySuffix = "\u0001options";

	private readonly GUIManager gui;

	private readonly ModCatalog.Entry entry;

	private readonly ModSettingRows rows;

	private readonly Action<string, string> log;

	private readonly UIComponent content;

	private readonly Image picture;

	private readonly TextArea text;

	private readonly int switchOffset;

	/// <summary>The mod's folding bar.</summary>
	public CollapsablePanel Bar { get; }

	/// <summary>The mod's on/off switch, on the surface left of <see cref="Bar"/>.</summary>
	public ImageButton Switch { get; }

	/// <summary>MOD OPTIONS, or null for a mod with no settings to show here.</summary>
	public CollapsablePanel Options { get; }

	/// <summary>The settings MOD OPTIONS shows: all of the mod's (<see cref="ModCatalog.Entry.Settings"/>).</summary>
	public List<ModSetting> OptionSettings { get; }

	/// <summary>Whether the switches are shown: not in game (SelectModsPanel.Mode.InGame).</summary>
	private readonly bool switchesShown;

	/// <summary>
	/// Builds the entry onto <paramref name="surface"/>. <paramref name="expanded"/> is the set of
	/// what is unfolded, kept while the game runs; <paramref name="relayout"/> is called whenever
	/// the entry's height changes. Without <paramref name="switchesShown"/> (in game) the switch is
	/// built but not put on the surface, and the bar starts where it would have; PORT never has one.
	/// </summary>
	public SelectModsEntry(GUIManager gui, UIComponent surface, ModCatalog.Entry entry, Image picture, ModSettingRows rows, ISet<string> expanded, Action relayout, Action<string, string> log, bool switchesShown = true)
	{
		this.gui = gui;
		this.entry = entry;
		this.rows = rows;
		this.log = log;
		this.picture = picture;
		this.switchesShown = switchesShown;

		Switch = new ImageButton(gui);
		bool hasSwitch = switchesShown && !entry.AlwaysOn;
		if (hasSwitch)
		{
			surface.Add(Switch);
		}
		Switch.Init(ImageButtonType.LCDCheckbox);
		Switch.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		Switch.X = SwitchX;

		Bar = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownBig);
		surface.Add(Bar);
		Bar.Init();
		Bar.Title = entry.Name;
		Bar.X = hasSwitch ? BarX(Switch.Width) : SwitchX;
		Bar.Width = surface.Width - Bar.X - RightMargin;
		Bar.CollapsedHeight = Bar.ExpandedPanelYPos + 5;
		Bar.Height = Bar.CollapsedHeight;
		// Centred on the bar's header; the header's offset, kept for LayOut since the switch is not the bar's.
		Bar.CenterOnHeader(Switch);
		switchOffset = Switch.Y;

		// The scenario picker's row: the picture, then the words.
		content = new UIComponent(gui)
		{
			Width = Bar.ExpandedPanel.Width
		};
		Bar.AddContent(content);
		int textX = 8;
		if (picture != null)
		{
			content.Add(picture);
			picture.X = 8 + (SelectModsPanel.ThumbnailSize - picture.Width) / 2;
			picture.Y = 6 + Math.Max(0, (SelectModsPanel.ThumbnailSize - picture.Height) / 2);
			textX = 8 + SelectModsPanel.ThumbnailSize + 12;
		}
		text = new TextArea(gui, ListBoxType.LCD);
		content.Add(text);
		text.Init(Label.LabelType.LCDNormal);
		text.CanGrowInHeight = true;
		text.ScrollBarEnabled = false;
		text.HMargin = 0;
		text.VMargin = 0;
		text.X = textX;
		text.Y = 6;
		text.Width = content.Width - textX - 8;

		OptionSettings = entry.Settings.ToList();
		if (OptionSettings.Count > 0 && !(switchesShown && entry.SwitchIsItsSetting))
		{
			Options = BuildOptions(expanded);
		}

		Switch.IsChecked = entry.ShownOn;
		Switch.Enabled = entry.CanSwitch;
		Switch.ToolTip = entry.CanSwitch
			? string.Format(UWGame.Locale.Text("Tick: {0} on - and its switches, if none of them is. Untick: off - its settings act as the studio's game, and come back as you left them when it is switched on again."), entry.Name)
			: UWGame.Locale.Text("Not now: the game was started with -nomods, or without the mod loader.");
		Switch.Click += delegate
		{
			ModCatalog.Switch(entry, Switch.IsChecked, log);
			// Switched back on, its settings are the file's again (ModSettings.SetModEnabled).
			rows.Refill(OptionSettings);
			Refresh();
		};
		Refresh();

		if (expanded.Contains(entry.Id))
		{
			Bar.IsExpanded = true;
		}
		Bar.HeightResize += delegate
		{
			// By height, not IsExpanded: IsExpanded calls Expand() - which changes the height and
			// raises this - before it updates its own flag (as in OptionsDialog).
			Remember(expanded, entry.Id, Bar);
			relayout();
		};
	}

	/// <summary>Where the bar starts: right of the switch, with <see cref="SwitchGap"/> between.</summary>
	public static int BarX(int switchWidth) => SwitchX + switchWidth + SwitchGap;

	/// <summary>
	/// Whether MOD OPTIONS is shown: the mod is on, and the game was not started with -nomods. Its
	/// settings read stock otherwise, and a change would not be kept. PORT's always is: -nomods
	/// leaves the port's settings as the file has them.
	/// </summary>
	public static bool ShowsOptions(ModCatalog.Entry entry) => entry.AlwaysOn || (!ModSettings.StockOnly && ModSettings.IsModEnabled(entry.Id));

	private CollapsablePanel BuildOptions(ISet<string> expanded)
	{
		CollapsablePanel options = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownBig);
		options.Init();
		options.Title = UWGame.Locale.Text("MOD OPTIONS");
		options.X = OptionsInset;
		options.Width = content.Width - 2 * OptionsInset;
		options.CollapsedHeight = options.ExpandedPanelYPos + 5;
		options.Height = options.CollapsedHeight;
		options.ToolTip = UWGame.Locale.Text("This mod's settings and keys. A change is applied and saved at once.");

		UIComponent rowsPanel = new UIComponent(gui)
		{
			Width = options.ExpandedPanel.Width
		};
		options.AddContent(rowsPanel);
		int lineY = ModSettingRows.AddReloadWarning(gui, rowsPanel, OptionSettings, 0);
		lineY = rows.AddGroup(rowsPanel, OptionSettings, lineY);
		// Fires Resize, which is what sizes the expanded panel (CollapsablePanel.content_Resize).
		rowsPanel.Height = lineY + 4;
		foreach (ModSetting setting in OptionSettings)
		{
			rows.OnChange(setting, ApplyPending);
		}

		string key = entry.Id + OptionsKeySuffix;
		if (expanded.Contains(key))
		{
			options.IsExpanded = true;
		}
		options.HeightResize += delegate
		{
			Remember(expanded, key, options);
			SizeContent();
		};
		return options;
	}

	private static void Remember(ISet<string> expanded, string key, CollapsablePanel cp)
	{
		if (cp.Height > cp.CollapsedHeight)
		{
			expanded.Add(key);
		}
		else
		{
			expanded.Remove(key);
		}
	}

	/// <summary>
	/// Applies and saves what MOD OPTIONS' controls show and their settings do not hold yet. Called
	/// as each control changes, and by <see cref="SelectModsPanel"/> on leaving - a text box is only
	/// read when it loses focus.
	/// </summary>
	public void ApplyPending()
	{
		if (Options == null || !ShowsOptions(entry))
		{
			return;
		}
		if (rows.ApplyChanged(OptionSettings, log))
		{
			Refresh();
		}
	}

	/// <summary>The header, the words and MOD OPTIONS, from what the mod is now.</summary>
	private void Refresh()
	{
		ModCatalog.Entry now = ModCatalog.Entries().FirstOrDefault((ModCatalog.Entry e) => string.Equals(e.Id, entry.Id, StringComparison.OrdinalIgnoreCase)) ?? entry;
		Bar.Summary = Summary(now);
		text.Text = Details(now, switchesShown);
		// Off while none of its switches is on, so a change under MOD OPTIONS can tick or untick it.
		Switch.IsChecked = now.ShownOn;
		if (Options != null)
		{
			Options.Summary = rows.Summary(OptionSettings);
		}
		SizeContent();
	}

	/// <summary>The unfolded part's height: the picture or the words, and MOD OPTIONS under them while the mod is on.</summary>
	private void SizeContent()
	{
		int y = Math.Max(picture != null ? SelectModsPanel.ThumbnailSize + 12 : 0, text.Bottom + 8);
		if (Options != null)
		{
			bool show = ShowsOptions(entry);
			if (show && Options.Parent == null)
			{
				content.Add(Options);
			}
			else if (!show && Options.Parent != null)
			{
				content.Remove(Options);
			}
			if (show)
			{
				Options.Y = y;
				y = Options.Bottom + 8;
			}
		}
		// Fires Resize, which is what sizes the expanded panel (CollapsablePanel.content_Resize).
		content.Height = y;
	}

	/// <summary>Places the entry at <paramref name="y"/>; returns the Y for the next.</summary>
	public int LayOut(int y)
	{
		Bar.Y = y;
		Switch.Y = y + switchOffset;
		return Bar.Bottom + 4;
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
	private static string Details(ModCatalog.Entry entry, bool switchesShown)
	{
		var lines = new List<string>();
		lines.Add(string.IsNullOrEmpty(entry.Description) ? UWGame.Locale.Text("No description.") : entry.Description);
		lines.Add("");
		if (!string.IsNullOrEmpty(entry.Author))
		{
			lines.Add(string.Format(UWGame.Locale.Text("Made by: {0}"), entry.Author));
		}
		if (entry.Settings.Count > 0 && !(switchesShown && entry.SwitchIsItsSetting))
		{
			string where = ShowsOptions(entry) ? UWGame.Locale.Text("Settings: {0}, {1} changed from the studio's game - under MOD OPTIONS below.")
				: switchesShown ? UWGame.Locale.Text("Settings: {0}, {1} changed from the studio's game - switch the mod on to change them.")
				: UWGame.Locale.Text("Settings: {0}, {1} changed from the studio's game - switch the mod on in the main menu, under MODDING -> SELECT MODS, to change them.");
			lines.Add(string.Format(where, entry.Settings.Count, entry.Changed));
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
}
