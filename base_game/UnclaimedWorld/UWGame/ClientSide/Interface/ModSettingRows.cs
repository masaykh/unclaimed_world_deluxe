using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.Mods;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

/// <summary>
/// PORT: the controls that edit <see cref="ModSetting"/>s - one row per setting, a checkbox, a
/// dropdown or a text box - and the reading back of what they show. One implementation for the two
/// places a player changes mod settings: OPTIONS -> MODS (<see cref="OptionsDialog"/>, in game and
/// in the main menu) and the MOD OPTIONS section under each mod in SELECT MODS
/// (MainMenu.Mods.SelectModsPanel). tripleacoder, "Main menu", 2026-10-11: "show the options for
/// enabled mods on the new Select mods dialog too, but without copying logic."
///
/// The rows only show values; when they become the settings' values is the caller's: OPTIONS on
/// OK (<see cref="ApplyAll"/>), SELECT MODS as each control changes (<see cref="ApplyChanged"/>).
/// Both end in the same <see cref="ModSetting.Value"/> assignment and <see cref="ModSettings.Save"/>.
/// </summary>
public sealed class ModSettingRows
{
	/// <summary>Where the value column starts, and the text box's left edge.</summary>
	public const int ValueX = 124;

	private readonly GUIManager gui;

	/// <summary>
	/// The controls built, paired with the setting each one edits. A list rather than named fields
	/// because there is no list of mod settings to name: what is here depends on which mods are
	/// installed, which is only known at run time.
	/// </summary>
	private readonly List<KeyValuePair<ModSetting, UIComponent>> controls = new List<KeyValuePair<ModSetting, UIComponent>>();

	/// <summary>
	/// The key each key button currently shows - not yet what the setting holds. Key buttons are
	/// OPTIONS' (KeybindMod's KEYS category); they are registered here with <see cref="AddKeyButton"/>
	/// so that reading, writing and applying treat them as any other row.
	/// </summary>
	public readonly Dictionary<TextButton, string> KeyButtonValues = new Dictionary<TextButton, string>();

	/// <summary>
	/// Dropdowns whose longest choice is wider than the value column: drawn under their caption,
	/// across the whole panel. Kastuk, TOOL CARE: "LOOK AFTER" and "BACK WHERE IT CAME FROM"
	/// were cut off, and should be "widened to full separated line width there to fit any
	/// contained text".
	/// </summary>
	private readonly HashSet<ModSetting> ownLineChoices = new HashSet<ModSetting>();

	/// <summary>The width of the current group's dropdowns, chosen by <see cref="ChoiceWidthFor"/>.</summary>
	private int groupChoiceWidth;

	public ModSettingRows(GUIManager gui)
	{
		this.gui = gui;
	}

	/// <summary>Every setting a row was built for, in the order built.</summary>
	public IEnumerable<ModSetting> Settings => controls.Select((KeyValuePair<ModSetting, UIComponent> pair) => pair.Key);

	public void Clear()
	{
		controls.Clear();
		KeyButtonValues.Clear();
		ownLineChoices.Clear();
	}

	/// <summary>
	/// One group's rows - an OPTIONS category, or a mod's MOD OPTIONS - from <paramref name="lineY"/>
	/// down; returns the Y below the last. The group's dropdowns share one width.
	/// </summary>
	public int AddGroup(UIComponent panel, IEnumerable<ModSetting> settings, int lineY)
	{
		List<ModSetting> list = settings.ToList();
		groupChoiceWidth = ChoiceWidthFor(list, panel.Width);
		foreach (ModSetting setting in list)
		{
			lineY = AddRow(panel, setting, lineY);
		}
		return lineY;
	}

	/// <summary>
	/// The width of a group's dropdowns: the width the dropdown's own header button asks for to
	/// show its longest choice. Kastuk: DANGEROUS FAUNA's "x1.5" sat in a box as wide as SAVE DATE
	/// FORMAT's longest pattern; then PESTS' "Normal" slid under the arrow, twice - first with widths
	/// guessed from character counts, then with the text measured but the margins guessed (36 px;
	/// the LCDCombo header keeps 17 left and 25 right, the arrow being part of its skin). So the
	/// probe is the header itself: a TextButton of the same type, sized by its own
	/// ScaleWidthToFitText, and whatever the skin needs is counted by the code that draws it.
	/// </summary>
	private int ChoiceWidthFor(List<ModSetting> settings, int panelWidth)
	{
		int longest = 0;
		TextButton probe = null;
		int full = Math.Max(80, panelWidth - ValueX - 4);
		foreach (ModSetting s in settings)
		{
			if (s.Kind != ModSettingKind.Choice || s.Choices == null)
			{
				continue;
			}
			int own = 0;
			foreach (string choice in s.Choices)
			{
				if (probe == null)
				{
					probe = new TextButton(gui);
					probe.Init(TextButton.TextButtonType.LCDCombo);
				}
				probe.Text = s.DisplayChoice(choice) ?? "";
				probe.ScaleWidthToFitText();
				// A few pixels of air, so the last letter does not touch the arrow.
				own = Math.Max(own, probe.Width + 4);
			}
			// One that cannot fit beside its caption gets a line of its own (AddRow) and does not
			// widen the rest of the group.
			if (own > full)
			{
				ownLineChoices.Add(s);
			}
			else
			{
				longest = Math.Max(longest, own);
			}
		}
		if (longest == 0)
		{
			return full;
		}
		return Common.Clamp(longest, 60, full);
	}

	/// <summary>The tooltip of a setting's row: STABLE or TESTING first, then the mod's words, then whether saves notice.</summary>
	public static string ToolTip(ModSetting setting)
	{
		// STABLE or TESTING first (ModSettings.IsStable), so a player reads it before the rest.
		string toolTip = (ModSettings.IsStable(setting) ? UWGame.Locale.Text("STABLE - confirmed in play.") + " " : UWGame.Locale.Text("TESTING - not yet confirmed in play.") + " ") + setting.DisplayToolTip;
		if (setting.AffectsSimulation)
		{
			toolTip += " " + UWGame.Locale.Text("Changes what a save contains: saves made with it on are marked MODDED.");
		}
		return toolTip;
	}

	/// <summary>One setting's row; returns the Y below it.</summary>
	public int AddRow(UIComponent panel, ModSetting setting, int lineY)
	{
		string toolTip = ToolTip(setting);
		int valueWidth = panel.Width - ValueX - 4;

		switch (setting.Kind)
		{
		case ModSettingKind.Toggle:
		{
			CheckBox checkBox = new CheckBox(gui);
			panel.Add(checkBox);
			checkBox.Init(CheckBoxType.LCD, CheckBoxFlavor.Green);
			checkBox.Text = setting.DisplayLabel;
			checkBox.FitToText();
			checkBox.X = 6;
			checkBox.Y = lineY;
			checkBox.ToolTip = toolTip;
			checkBox.TooltipWidth = 320;
			// Kastuk: the tooltip went away while the cursor was still on the switch.
			checkBox.TooltipExpires = false;
			checkBox.IsChecked = setting.On;
			controls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, checkBox));
			return checkBox.Bottom + 8;
		}
		case ModSettingKind.Choice:
		{
			Label caption = new Label(gui);
			panel.Add(caption);
			caption.Init(Label.LabelType.LCDSmallHeadingBanner);
			caption.Text = setting.DisplayLabel + ":";
			caption.FitToText();
			caption.X = 6;
			caption.Y = lineY;
			caption.ToolTip = toolTip;
			caption.TooltipWidth = 320;
			caption.TooltipExpires = false;
			ComboBox comboBox = new ComboBox(gui, ListBoxType.LCDCombo, isEditable: false);
			panel.Add(comboBox);
			comboBox.Init(ComboBoxTypes.LCD);
			// Right-aligned at the group's width; beside the caption when it fits, under it when it
			// does not ("NORTHERN BUSH DRAGON AGGRO RANGE:" is long).
			int choiceWidth = ownLineChoices.Contains(setting) ? panel.Width - 6 - 4
				: (groupChoiceWidth > 0) ? groupChoiceWidth : valueWidth;
			comboBox.Width = choiceWidth;
			comboBox.X = panel.Width - choiceWidth - 4;
			if (caption.Right + 6 <= comboBox.X)
			{
				comboBox.CenterThisVertically(caption.Y + caption.Height / 2);
			}
			else
			{
				comboBox.Y = caption.Bottom + 2;
			}
			comboBox.ToolTip = toolTip;
			comboBox.TooltipExpires = false;
			foreach (string choice in setting.Choices ?? new string[0])
			{
				comboBox.AddEntry(choice, setting.DisplayChoice(choice));
			}
			if (comboBox.EntriesByKey.ContainsKey(setting.Value))
			{
				comboBox.SelectedKey = setting.Value;
			}
			controls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, comboBox));
			return Math.Max(caption.Bottom, comboBox.Bottom) + 8;
		}
		default:
		{
			Label caption2 = new Label(gui);
			panel.Add(caption2);
			caption2.Init(Label.LabelType.LCDSmallHeadingBanner);
			caption2.Text = setting.DisplayLabel + ":";
			caption2.FitToText();
			caption2.X = 6;
			caption2.Y = lineY;
			caption2.ToolTip = toolTip;
			caption2.TooltipExpires = false;
			TextBox textBox = new TextBox(gui);
			panel.Add(textBox);
			textBox.Init(TextBox.TextBoxType.LCD);
			textBox.X = ValueX;
			textBox.Width = valueWidth;
			textBox.Height = 20;
			textBox.VMargin = 1;
			textBox.IsEditable = true;
			textBox.Text = setting.Value;
			if (caption2.Right + 6 <= ValueX)
			{
				textBox.CenterThisVertically(caption2.Y + caption2.Height / 2);
			}
			else
			{
				textBox.Y = caption2.Bottom + 2;
			}
			textBox.ToolTip = toolTip;
			controls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, textBox));
			return Math.Max(caption2.Bottom, textBox.Bottom) + 8;
		}
		}
	}

	/// <summary>A key button OPTIONS built for a key setting (KEYS category), showing <paramref name="keyName"/>.</summary>
	public void AddKeyButton(ModSetting setting, TextButton button, string keyName)
	{
		KeyButtonValues[button] = keyName;
		if (setting != null)
		{
			controls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, button));
		}
	}

	/// <summary>Calls <paramref name="handler"/> whenever the player changes a setting's control: a click, a choice, leaving a text box.</summary>
	public void OnChange(ModSetting setting, Action handler)
	{
		UIComponent control = Find(setting);
		if (control is CheckBox checkBox)
		{
			checkBox.Click += delegate
			{
				handler();
			};
		}
		else if (control is ComboBox comboBox)
		{
			comboBox.SelectionChanged += delegate
			{
				handler();
			};
		}
		else if (control is TextBox textBox)
		{
			textBox.LoseFocus += delegate
			{
				handler();
			};
		}
	}

	public UIComponent Find(ModSetting setting)
	{
		foreach (KeyValuePair<ModSetting, UIComponent> pair in controls)
		{
			if (pair.Key == setting)
			{
				return pair.Value;
			}
		}
		return null;
	}

	/// <summary>What a setting's control currently shows - which is not yet what the setting holds.</summary>
	public string Read(ModSetting setting)
	{
		UIComponent control = Find(setting);
		if (control is CheckBox checkBox)
		{
			return checkBox.IsChecked ? "true" : "false";
		}
		if (control is ComboBox comboBox)
		{
			return comboBox.SelectedKey as string ?? setting.Value;
		}
		if (control is TextBox textBox)
		{
			return textBox.Text;
		}
		if (control is TextButton keyButton && KeyButtonValues.TryGetValue(keyButton, out string keyName))
		{
			return keyName;
		}
		return setting.Value;
	}

	public void Write(ModSetting setting, string value)
	{
		UIComponent control = Find(setting);
		if (control is CheckBox checkBox)
		{
			checkBox.IsChecked = ModSetting.ParseBool(value, false);
		}
		else if (control is ComboBox comboBox && value != null && comboBox.EntriesByKey.ContainsKey(value))
		{
			comboBox.SelectedKey = value;
		}
		else if (control is TextBox textBox)
		{
			textBox.Text = value ?? "";
		}
		else if (control is TextButton keyButton && KeyButtonValues.ContainsKey(keyButton))
		{
			KeyButtonValues[keyButton] = value;
			keyButton.Text = KeybindMod.DisplayName(value);
		}
	}

	/// <summary>Every control back to what its setting holds.</summary>
	public void Refill(IEnumerable<ModSetting> settings = null)
	{
		foreach (ModSetting setting in (settings ?? Settings).ToList())
		{
			Write(setting, setting.Value);
		}
	}

	/// <summary>How many of <paramref name="settings"/> the controls show differing from the studio's game.</summary>
	public int ChangedCount(IEnumerable<ModSetting> settings)
	{
		return settings.Count((ModSetting s) => !string.Equals(Read(s), s.StockValue, StringComparison.Ordinal));
	}

	/// <summary>
	/// A group's header summary: STABLE only when every setting in it is confirmed in play, then how
	/// many differ from the studio's game.
	/// </summary>
	public string Summary(ICollection<ModSetting> settings)
	{
		int changed = ChangedCount(settings);
		string stability = ModSettings.AllStable(settings) ? UWGame.Locale.Text("STABLE") : UWGame.Locale.Text("TESTING");
		return changed > 0 ? stability + " - " + UWGame.Locale.Count(changed, "{0} CHANGED", "{0} CHANGED") : stability;
	}

	/// <summary>OPTIONS' OK: every control read back into its setting, and the file written.</summary>
	public void ApplyAll(Action<string, string> log)
	{
		if (controls.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<ModSetting, UIComponent> pair in controls)
		{
			pair.Key.Value = Read(pair.Key);
		}
		ModSettings.Save(log);
	}

	/// <summary>
	/// SELECT MODS: the controls of <paramref name="settings"/> that differ from their setting are
	/// read back into it, and the file written if any did. Returns whether any did.
	/// </summary>
	public bool ApplyChanged(IEnumerable<ModSetting> settings, Action<string, string> log)
	{
		bool any = false;
		foreach (ModSetting setting in settings)
		{
			if (Find(setting) != null && !string.Equals(Read(setting), setting.Value, StringComparison.Ordinal))
			{
				setting.Value = Read(setting);
				any = true;
			}
		}
		if (any)
		{
			ModSettings.Save(log);
		}
		return any;
	}

	/// <summary>
	/// PORT: a red warning in as many lines as the surface needs; returns its bottom. A Label is
	/// one line and is clipped at the panel edge, and a translation is often longer than the
	/// English (Kastuk, Russian: both restart warnings were cut off).
	/// </summary>
	public static int AddWarning(GUIManager gui, UIComponent parent, string text, int y)
	{
		int width = parent.Width - 12;
		Label line = AddWarningLine(gui, parent, y);
		foreach (string word in text.Split(' '))
		{
			string longer = string.IsNullOrEmpty(line.Text) ? word : line.Text + " " + word;
			if (!string.IsNullOrEmpty(line.Text) && line.GetTextWidth(longer) > width)
			{
				line.FitToText();
				line = AddWarningLine(gui, parent, line.Bottom);
				longer = word;
			}
			line.Text = longer;
		}
		line.FitToText();
		return line.Bottom;
	}

	private static Label AddWarningLine(GUIManager gui, UIComponent parent, int y)
	{
		Label line = new Label(gui);
		parent.Add(line);
		line.Init(Label.LabelType.LCDNormal);
		line.NormalColor = UIComponent.errorColor;
		line.X = 6;
		line.Y = y;
		return line;
	}

	/// <summary>
	/// "Content changes apply at the next game start!" when any of <paramref name="settings"/> only
	/// shows up when the data tables are next built (<see cref="ModSetting.TakesEffectOnNextLoad"/>);
	/// returns the Y below it, or <paramref name="y"/> when there is nothing to say.
	/// </summary>
	public static int AddReloadWarning(GUIManager gui, UIComponent parent, IEnumerable<ModSetting> settings, int y)
	{
		if (!settings.Any((ModSetting m) => m.TakesEffectOnNextLoad))
		{
			return y;
		}
		return AddWarning(gui, parent, UWGame.Locale.Text("Content changes apply at the next game start!"), y) + 6;
	}
}
