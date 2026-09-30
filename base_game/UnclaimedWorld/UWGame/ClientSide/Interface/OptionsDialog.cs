using System;
using System.Collections.Generic;
using System.Linq;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Interface.LCD;
using UWGame.Mods;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class OptionsDialog : Panel
{
	protected Box display;

	protected LCDScreen lcdScreen;

	protected UIComponent lcdSurface;

	private Grid surfaceGrid;

	private CheckBox cbMusic;

	private CheckBox cbSound;

	private CheckBox cbFullscreen;

	private CheckBox cbBorder;

	private CheckBox cbHardwareModeSwitch;

	private ComboBox cbResolution;

	private TextBox tbWidth;

	private TextBox tbHeight;

	private FillableBar fbMusicVolume;

	private FillableBar fbSoundVolume;

	private FillableBar fbZoom;

	private RadioButton rbCustom;

	private RadioButton rbFixed;

	private RadioGroup rgResolution;

	private ErrorsAndMessages errorsAndMessages;

	/// <summary>
	/// The controls built for the MODS section, paired with the setting each one edits.
	///
	/// A list rather than named fields because there is no list of mod settings to name: what is
	/// here depends on which mods are installed, which is only known at run time. Everything else
	/// in this dialog is a field because everything else is fixed at compile time.
	/// </summary>
	private readonly List<KeyValuePair<ModSetting, UIComponent>> modControls =
		new List<KeyValuePair<ModSetting, UIComponent>>();

	private const int xPos = 6;

	private const string emptyKey = "";

	public event EventHandler CancelClick;

	public event EventHandler OKClick;

	public OptionsDialog(CommonInterface intf)
		: base(intf, "OPTIONS", new Point(420, 120), new Vector2(400f, 600f), Level.Menu, PanelType.RegularEdges)
	{
		GUIManager gui = Interface.gui;
		int x = 124;
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 52, Panel.RosterMargin, out display, out lcdSurface, ref lcdScreen);
		CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, canHaveFocus: false, 0, 40);
		UIComponent uIComponent = new UIComponent(Interface.gui)
		{
			Width = surfaceGrid.SurfaceWidth,
			Height = 600
		};
		surfaceGrid.AddEntry("surfaceKey", uIComponent);
		Label label = AddSectionHeader(6, uIComponent, "GRAPHICS");
		Label label2 = new Label(Interface.gui);
		uIComponent.Add(label2);
		label2.Init(Label.LabelType.LCDNormal);
		label2.NormalColor = UIComponent.errorColor;
		label2.Text = "Restart the game to apply Graphics changes!";
		label2.FitToText();
		label2.X = 6;
		label2.Y = label.Bottom + 6;
		cbFullscreen = new CheckBox(Interface.gui);
		uIComponent.Add(cbFullscreen);
		cbFullscreen.Init(CheckBoxType.LCD, CheckBoxFlavor.Blue);
		cbFullscreen.Text = "FULL SCREEN";
		cbFullscreen.FitToText();
		cbFullscreen.X = 6;
		cbFullscreen.Y = label2.Bottom + 6;
		cbFullscreen.Click += cbFullscreen_Click;
		int y = cbFullscreen.Bottom + 6;
		cbHardwareModeSwitch = new CheckBox(Interface.gui);
		uIComponent.Add(cbHardwareModeSwitch);
		cbHardwareModeSwitch.Init(CheckBoxType.LCD, CheckBoxFlavor.Green);
		cbHardwareModeSwitch.Text = "HARDWARE MODE SWITCH";
		cbHardwareModeSwitch.FitToText();
		cbHardwareModeSwitch.X = x;
		cbHardwareModeSwitch.Y = y;
		cbHardwareModeSwitch.ToolTip = "If selected, the game attempts to switch the screen resolution when in fullscreen. If not selected, only the desktop resolution is available for fullscreen. Is selected by default.";
		cbHardwareModeSwitch.Click += cbHardwareModeSwitch_Click;
		y = cbHardwareModeSwitch.Bottom + 12;
		Label label3 = new Label(Interface.gui);
		uIComponent.Add(label3);
		label3.Init(Label.LabelType.LCDHeadingBlue);
		label3.Text = "RESOLUTION";
		label3.FitToText();
		label3.CenterThisVertically(y);
		label3.X = 6;
		y = label3.Bottom + 12;
		rgResolution = new RadioGroup(gui);
		rgResolution.NewMemberChecked += rgResolution_NewMemberChecked;
		rbFixed = new RadioButton(gui);
		uIComponent.Add(rbFixed);
		rgResolution.Add(rbFixed, addAsControl: false);
		rbFixed.Init(CheckBoxType.LCDRadioBanner);
		rbFixed.Text = "FIXED:";
		rbFixed.FitToText();
		rbFixed.CenterThisVertically(y);
		rbFixed.X = 6;
		rbFixed.ToolTip = "Select this option to select a fixed resolution among those supported by the video card.";
		cbResolution = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		uIComponent.Add(cbResolution);
		cbResolution.Init(ComboBoxTypes.LCD);
		cbResolution.X = x;
		cbResolution.Width = 200;
		cbResolution.CenterThisVertically(y);
		cbResolution.SelectionChanged += cbResolution_SelectionChanged;
		int yPosToCenterTo = cbResolution.Bottom + 12;
		rbCustom = new RadioButton(gui);
		uIComponent.Add(rbCustom);
		rgResolution.Add(rbCustom, addAsControl: false);
		rbCustom.Init(CheckBoxType.LCDRadioBanner);
		rbCustom.Text = "CUSTOM:";
		rbCustom.FitToText();
		rbCustom.CenterThisVertically(yPosToCenterTo);
		rbCustom.X = 6;
		rbCustom.ToolTip = "Select this option to enter a custom window size. Not available in fullscreen.";
		Label label4 = new Label(Interface.gui);
		uIComponent.Add(label4);
		label4.Init(Label.LabelType.LCDHeadingBlue);
		label4.Text = "W:";
		label4.FitToText();
		label4.CenterThisVertically(yPosToCenterTo);
		label4.X = cbResolution.X;
		tbWidth = new TextBox(Interface.gui);
		uIComponent.Add(tbWidth);
		tbWidth.Init(TextBox.TextBoxType.LCD);
		tbWidth.X = label4.Right + 6;
		tbWidth.Width = 46;
		tbWidth.Height = 20;
		tbWidth.VMargin = 1;
		tbWidth.IsEditable = true;
		tbWidth.IsNumericBox = true;
		tbWidth.Y = label4.Y;
		Label label5 = new Label(Interface.gui);
		uIComponent.Add(label5);
		label5.Init(Label.LabelType.LCDHeadingBlue);
		label5.Text = "H:";
		label5.FitToText();
		label5.CenterThisVertically(yPosToCenterTo);
		label5.X = tbWidth.Right + 6;
		tbHeight = new TextBox(Interface.gui);
		uIComponent.Add(tbHeight);
		tbHeight.Init(TextBox.TextBoxType.LCD);
		tbHeight.X = label5.Right + 6;
		tbHeight.Width = tbWidth.Width;
		tbHeight.Height = tbWidth.Height;
		tbHeight.VMargin = tbWidth.VMargin;
		tbHeight.IsEditable = true;
		tbHeight.IsNumericBox = true;
		tbHeight.CenterThisVertically(yPosToCenterTo);
		cbBorder = new CheckBox(Interface.gui);
		uIComponent.Add(cbBorder);
		cbBorder.Init(CheckBoxType.LCD, CheckBoxFlavor.Purple);
		cbBorder.Text = "BORDER";
		cbBorder.FitToText();
		cbBorder.ToolTip = "Select whether the application window should have a border.";
		cbBorder.Position = new Point(6, tbHeight.Bottom + 24);
		string toolTip = "Set the magnification (pixel zoom) level. Restart the game to see the effect. WARNING: Setting a high magnification level may cause the interface to be truncated. In case of problems, use the ZoomFactor property in Options.xml to revert.";
		Label label6 = new Label(Interface.gui);
		uIComponent.Add(label6);
		label6.Init(Label.LabelType.LCDSmallHeadingBanner);
		label6.Text = "MAGNIFICATION:";
		label6.Position = new Point(6, cbBorder.Bottom + 6);
		label6.ToolTip = toolTip;
		label6.TooltipExpires = false;
		label6.TooltipWidth = 300;
		fbZoom = new FillableBar(Interface.gui, FillableBar.FillableBarType.LCDSlider, canGrow: false);
		uIComponent.Add(fbZoom);
		fbZoom.Width = 220;
		fbZoom.X = 128;
		fbZoom.Y = label6.Y;
		fbZoom.ToolTip = toolTip;
		fbZoom.MaxValue = 400;
		fbZoom.ShowMaxValueLabelAtEnd = true;
		fbZoom.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Always;
		fbZoom.StepSize = MagnificationMod.OptionsStepSize();
		fbZoom.KnobWidth = 16;
		// 80 notches at a step of 5 would be a solid bar; the studio's 25 keeps them.
		fbZoom.ShowNotches = fbZoom.StepSize >= 25;
		fbZoom.MaxSliderValueSymbol = null;
		fbZoom.MaxSliderValueTooltip = null;
		fbZoom.DisplayValueFunction = (int v) => (0.01f * (float)v).ToString("N2");
		y = AddSectionHeader(280, uIComponent, "SOUND").Bottom + 6;
		cbMusic = new CheckBox(Interface.gui);
		uIComponent.Add(cbMusic);
		cbMusic.Text = "MUSIC";
		cbMusic.Init(CheckBoxType.LCD);
		cbMusic.X = 6;
		cbMusic.Y = y;
		cbMusic.FitToText();
		cbMusic.Click += cbMusic_Click;
		fbMusicVolume = new FillableBar(Interface.gui, FillableBar.FillableBarType.LCDSlider, canGrow: false);
		uIComponent.Add(fbMusicVolume);
		fbMusicVolume.Width = 150;
		fbMusicVolume.X = 128;
		fbMusicVolume.Y = y + 5;
		fbMusicVolume.ToolTip = "Set the music volume.";
		fbMusicVolume.SliderMouseUp += fbMusicVolume_SliderMouseUp;
		fbMusicVolume.MaxValue = 100;
		fbMusicVolume.ShowMaxValueLabelAtEnd = false;
		fbMusicVolume.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Never;
		fbMusicVolume.KnobWidth = 16;
		y = cbMusic.Y + 24;
		cbSound = new CheckBox(Interface.gui);
		uIComponent.Add(cbSound);
		cbSound.Text = "SOUND";
		cbSound.Init(CheckBoxType.LCD);
		cbSound.X = 6;
		cbSound.Y = y;
		cbSound.FitToText();
		cbSound.Click += cbSound_Click;
		fbSoundVolume = new FillableBar(Interface.gui, FillableBar.FillableBarType.LCDSlider, canGrow: false);
		uIComponent.Add(fbSoundVolume);
		fbSoundVolume.Width = 150;
		fbSoundVolume.X = 128;
		fbSoundVolume.Y = y + 5;
		fbSoundVolume.ToolTip = "Set the sound effects volume.";
		fbSoundVolume.SliderMouseUp += fbSoundVolume_SliderMouseUp;
		fbSoundVolume.MaxValue = 100;
		fbSoundVolume.ShowMaxValueLabelAtEnd = false;
		fbSoundVolume.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Never;
		fbSoundVolume.KnobWidth = 16;
		BuildModsSection(uIComponent, fbSoundVolume.Bottom + 24);
		errorsAndMessages = new ErrorsAndMessages(lcdSurface, Window.guiManager, 6, lcdSurface.Height - 24);
		AddLowerButton("OK", "Accepts the changes and closes the dialog.", Align.Left).Click += btOK_Click;
		AddLowerButton("CANCEL", "Closes the dialog without applying the changes.", Align.Right).Click += btCancel_Click;
		AddDirtOnStraightEdges(excludeBottomDirt: true);
	}

	private void cbHardwareModeSwitch_Click(UIComponent sender, EventArgs e)
	{
		SetGraphicsEnabledStates();
	}

	private void rgResolution_NewMemberChecked(ICanBeChecked arg1, EventArgs arg2)
	{
		if (arg1 == rbFixed)
		{
			EnableFixedResolution();
		}
		else
		{
			EnableCustomResolution();
		}
	}

	private void DisableResolution()
	{
		rbFixed.Enabled = false;
		rbCustom.Enabled = false;
		cbResolution.Enabled = false;
		tbWidth.Enabled = false;
		tbHeight.Enabled = false;
	}

	private void EnableFixedResolution()
	{
		rbFixed.Enabled = true;
		cbResolution.Enabled = true;
		tbWidth.Enabled = false;
		tbHeight.Enabled = false;
	}

	private void EnableCustomResolution()
	{
		rbCustom.Enabled = true;
		cbResolution.Enabled = false;
		tbWidth.Enabled = true;
		tbHeight.Enabled = true;
	}

	private int VolumeToSliderIncrementsQuad(float volume)
	{
		return (int)MathHelper.Clamp((float)(100.0 * Math.Pow(volume, 0.25)), 0f, 100f);
	}

	private float SliderValueToVolumeQuad(int value)
	{
		return MathHelper.Clamp((float)(Math.Pow(MathHelper.Clamp(value, 0f, 100f), 4.0) * 9.99999993922529E-09), 0.001f, 1f);
	}

	private int VolumeToSliderIncrementsLog(float volume)
	{
		return (int)MathHelper.Clamp((float)(Math.Pow(101.0, volume) - 1.0), 0f, 100f);
	}

	private float SliderValueToVolumeLog(int value)
	{
		return SliderValueToVolumeQuad(value);
	}

	private void fbMusicVolume_SliderMouseUp(object sender, EventArgs e)
	{
		Interface.Game.Controller.AudioManager.MusicVolume = SliderValueToVolumeQuad(fbMusicVolume.Value);
	}

	private void fbSoundVolume_SliderMouseUp(object sender, EventArgs e)
	{
		if (The.Client != null)
		{
			The.Client.Controller.AudioManager.SoundVolume = SliderValueToVolumeQuad(fbSoundVolume.Value);
		}
		GUIManager.BeepBasicPanel.Play();
	}

	/// <summary>
	/// The MODS section: the registered <see cref="ModSetting"/>s, grouped into folding categories.
	///
	/// It is built from the registry rather than from a list of fields because a third-party mod
	/// cannot add a field to this class - which is the whole reason mod settings do not live in
	/// Options.xml. A build with no mods still gets the section, because the port registers
	/// entries of its own (see PortSettings), so this code is exercised everywhere rather than
	/// only where someone has installed something.
	///
	/// CATEGORIES. Kastuk: "Currently all mods going into same list of Settings. But its too big
	/// and chaotic list." One category per mod, named by the first part of the setting id
	/// (debug.testScenario -> DEBUG, or the label the mod registered with
	/// ModSettings.SetCategoryLabel); a mod with a single setting goes under OTHER. Each is the
	/// game's own folding header - CollapsablePanel.DropDownBig, as the trade window's categories
	/// use - and folded by default; the fold state is remembered while the game runs. The header
	/// carries a checkbox, like the stockpile window's category headers: unticking puts the whole
	/// category back to the studio's game, ticking switches every switch in it on. And a count of
	/// what differs from the stock game, so a folded category still says whether it is doing
	/// anything.
	///
	/// The surface grows and shrinks with the categories; it is inside a scrolling grid, which
	/// re-lays itself out when an entry changes height (Grid.item_Resize), so a long list scrolls.
	/// </summary>
	private void BuildModsSection(UIComponent panel, int y)
	{
		modControls.Clear();
		modCategoryPanels.Clear();
		if (ModSettings.All.Count == 0)
		{
			return;
		}

		Label header = AddSectionHeader(y, panel, "MODS");
		int lineY = header.Bottom + 6;

		bool anyNeedsReload = ModSettings.All.Any((ModSetting m) => m.TakesEffectOnNextLoad);
		if (anyNeedsReload)
		{
			Label note = new Label(Interface.gui);
			panel.Add(note);
			note.Init(Label.LabelType.LCDNormal);
			note.NormalColor = UIComponent.errorColor;
			// Kept to the width of the studio's own warning above ("Restart the game to apply
			// Graphics changes!"): the surface is about 340px and a longer line is simply clipped
			// at the panel edge, which is how the first version shipped.
			note.Text = "Content changes apply at the next game start!";
			note.FitToText();
			note.X = 6;
			note.Y = lineY;
			lineY = note.Bottom + 6;
		}

		// Categories in the order their first setting was registered, so the menu keeps the order
		// the mods were written down in; OTHER last.
		// KeybindMod: every key - the studio's and the mods' - in one KEYS category, first.
		if (KeybindMod.Enabled)
		{
			BuildKeysCategory(panel);
		}
		var byMod = new List<KeyValuePair<string, List<ModSetting>>>();
		foreach (ModSetting setting in ModSettings.All)
		{
			if (KeybindMod.Enabled && setting.Kind == ModSettingKind.Key)
			{
				continue;
			}
			int index = byMod.FindIndex((KeyValuePair<string, List<ModSetting>> g) => g.Key == setting.ModId);
			if (index < 0)
			{
				byMod.Add(new KeyValuePair<string, List<ModSetting>>(setting.ModId, new List<ModSetting> { setting }));
			}
			else
			{
				byMod[index].Value.Add(setting);
			}
		}
		var other = new List<ModSetting>();
		modsSurface = panel;
		modsTop = lineY;
		modsSurfaceMinHeight = panel.Height;
		foreach (KeyValuePair<string, List<ModSetting>> group in byMod)
		{
			if (group.Value.Count == 1)
			{
				other.AddRange(group.Value);
			}
			else
			{
				BuildModCategory(panel, group.Key, ModSettings.CategoryLabel(group.Key), group.Value);
			}
		}
		if (other.Count > 0)
		{
			BuildModCategory(panel, "\u0001other", "OTHER", other);
		}
		LayOutModCategories();
	}

	/// <summary>Categories folded open while the game runs, by key. Folded is the default.</summary>
	private static readonly HashSet<string> expandedModCategories = new HashSet<string>();

	private readonly List<CollapsablePanel> modCategoryPanels = new List<CollapsablePanel>();

	private UIComponent modsSurface;

	private int modsTop;

	private int modsSurfaceMinHeight;

	private void BuildModCategory(UIComponent panel, string key, string label, List<ModSetting> settings)
	{
		CollapsablePanel cp = new CollapsablePanel(Interface.gui, CollapsablePanel.PanelType.DropDownBig);
		panel.Add(cp);
		cp.Init();
		cp.Title = label;
		cp.X = 6;
		cp.Width = surfaceGrid.SurfaceWidth - cp.X;
		cp.CollapsedHeight = cp.ExpandedPanelYPos + 5;
		cp.Height = cp.CollapsedHeight;
		modCategoryPanels.Add(cp);

		UIComponent content = new UIComponent(Interface.gui)
		{
			Width = cp.ExpandedPanel.Width
		};
		cp.AddContent(content);
		int lineY = 0;
		categoryChoiceWidth = ChoiceWidthFor(settings, content.Width);
		foreach (ModSetting setting in settings)
		{
			lineY = AddModSettingControl(content, setting, lineY);
		}
		// Fires Resize, which is what sizes the expanded panel (CollapsablePanel.content_Resize).
		content.Height = lineY + 4;

		// The category switch, on the header's left where DropDownBig leaves room before the title.
		ImageButton categorySwitch = new ImageButton(Interface.gui);
		cp.Add(categorySwitch);
		categorySwitch.Init(ImageButtonType.LCDCheckbox);
		categorySwitch.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		categorySwitch.X = 8;
		cp.CenterOnHeader(categorySwitch);
		categorySwitch.ToolTip = "Untick: everything in " + label + " back to the studio's game. " +
			"Tick: every switch in it on. Nothing is applied until OK.";

		void Refresh()
		{
			int changed = settings.Count((ModSetting s) => !string.Equals(ReadModControl(s), s.StockValue, StringComparison.Ordinal));
			categorySwitch.IsChecked = changed > 0;
			// The category's state first: STABLE only when every switch in it is confirmed in play.
			string stability = ModSettings.AllStable(settings) ? "STABLE" : "TESTING";
			cp.Summary = changed > 0 ? stability + " - " + changed + " CHANGED" : stability;
		}

		categorySwitch.Click += delegate
		{
			bool on = categorySwitch.IsChecked;
			foreach (ModSetting s in settings)
			{
				string value;
				if (!on)
				{
					value = s.StockValue;
				}
				else if (s.Kind == ModSettingKind.Toggle)
				{
					value = "true";
				}
				else
				{
					// A choice has no "on"; the mod's own default is the nearest thing, and where
					// that is the stock value too (DangerousFaunaMod's x1) it is left as it is.
					value = string.Equals(s.DefaultValue, s.StockValue, StringComparison.Ordinal) ? ReadModControl(s) : s.DefaultValue;
				}
				WriteModControl(s, value);
			}
			Refresh();
		};
		foreach (ModSetting s in settings)
		{
			UIComponent control = FindModControl(s);
			if (control is CheckBox checkBox)
			{
				checkBox.Click += delegate
				{
					Refresh();
				};
			}
			else if (control is ComboBox comboBox)
			{
				comboBox.SelectionChanged += delegate
				{
					Refresh();
				};
			}
		}
		Refresh();

		if (expandedModCategories.Contains(key))
		{
			cp.IsExpanded = true;
		}
		cp.HeightResize += delegate
		{
			// By height, not IsExpanded: CollapsablePanel.IsExpanded calls Expand() - which is
			// what changes the height and raises this - before it updates its own flag.
			if (cp.Height > cp.CollapsedHeight)
			{
				expandedModCategories.Add(key);
			}
			else
			{
				expandedModCategories.Remove(key);
			}
			LayOutModCategories();
		};
	}

	// ---- KEYS (KeybindMod) ------------------------------------------------------------------

	/// <summary>The key each button currently shows - not yet what Options or the setting holds.</summary>
	private readonly Dictionary<TextButton, string> keyButtonValues = new Dictionary<TextButton, string>();

	/// <summary>The studio's bindings: the Options field each button edits.</summary>
	private readonly List<KeyValuePair<System.Reflection.FieldInfo, TextButton>> vanillaKeyButtons =
		new List<KeyValuePair<System.Reflection.FieldInfo, TextButton>>();

	/// <summary>The button waiting for a key press, or null.</summary>
	private TextButton capturingKeyButton;

	private Color keyButtonColor;

	private void BuildKeysCategory(UIComponent panel)
	{
		keyButtonValues.Clear();
		vanillaKeyButtons.Clear();
		capturingKeyButton = null;
		CollapsablePanel cp = new CollapsablePanel(Interface.gui, CollapsablePanel.PanelType.DropDownBig);
		panel.Add(cp);
		cp.Init();
		cp.Title = "KEYS";
		cp.X = 6;
		cp.Width = surfaceGrid.SurfaceWidth - cp.X;
		cp.CollapsedHeight = cp.ExpandedPanelYPos + 5;
		cp.Height = cp.CollapsedHeight;
		modCategoryPanels.Add(cp);
		UIComponent content = new UIComponent(Interface.gui)
		{
			Width = cp.ExpandedPanel.Width
		};
		cp.AddContent(content);
		int lineY = 0;
		Options options = Interface.Game.Controller.Options;
		foreach ((System.Reflection.FieldInfo field, string label) in KeybindMod.VanillaFields())
		{
			TextButton button = AddKeyRow(content, label, field.GetValue(options).ToString(), "The studio's binding, saved in Options.xml.", ref lineY);
			vanillaKeyButtons.Add(new KeyValuePair<System.Reflection.FieldInfo, TextButton>(field, button));
		}
		foreach (ModSetting setting in ModSettings.All)
		{
			if (setting.Kind == ModSettingKind.Key)
			{
				string category = ModSettings.CategoryLabel(setting.ModId);
				TextButton button = AddKeyRow(content, setting.Label, setting.Value, (category != null ? category + ": " : "") + setting.ToolTip, ref lineY);
				modControls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, button));
			}
		}
		// Kastuk: GAME MENU rebound away from Escape could never be put back, since Escape cancels
		// a rebinding. This puts every key back to the studio's and the mods' defaults - pending,
		// like everything else here, until OK.
		TextButton defaults = new TextButton(Interface.gui);
		content.Add(defaults);
		defaults.Init(TextButton.TextButtonType.LCDToolTipBlack);
		defaults.CheckedMode = CheckedModes.CannotBeChecked;
		defaults.Text = "DEFAULT KEYS";
		defaults.Width = 120;
		defaults.X = content.Width - defaults.Width - 4;
		defaults.Y = lineY;
		defaults.ToolTip = "Every key back to its default. Nothing is applied until OK.";
		defaults.Click += delegate
		{
			RestoreDefaultKeys();
		};
		lineY = defaults.Bottom + 6;
		content.Height = lineY + 4;
		MarkDuplicateKeys();
		const string key = "\u0002keys";
		if (expandedModCategories.Contains(key))
		{
			cp.IsExpanded = true;
		}
		cp.HeightResize += delegate
		{
			if (cp.Height > cp.CollapsedHeight)
			{
				expandedModCategories.Add(key);
			}
			else
			{
				expandedModCategories.Remove(key);
			}
			LayOutModCategories();
		};
	}

	private TextButton AddKeyRow(UIComponent panel, string label, string keyName, string toolTip, ref int lineY)
	{
		Label caption = new Label(Interface.gui);
		panel.Add(caption);
		caption.Init(Label.LabelType.LCDSmallHeadingBanner);
		caption.Text = label + ":";
		caption.FitToText();
		caption.X = 6;
		caption.Y = lineY;
		caption.ToolTip = toolTip;
		caption.TooltipWidth = 320;
		caption.TooltipExpires = false;
		TextButton button = new TextButton(Interface.gui);
		panel.Add(button);
		button.Init(TextButton.TextButtonType.LCDToolTipBlack);
		button.CheckedMode = CheckedModes.CannotBeChecked;
		// A fixed key column, wide enough for LEFT SHIFT; a caption too long for the space beside
		// it puts the key on the next line instead of pushing it off the panel (Kastuk, screenshot).
		button.Width = 104;
		button.X = panel.Width - button.Width - 4;
		if (caption.Right + 6 <= button.X)
		{
			button.CenterThisVertically(caption.Y + caption.Height / 2);
		}
		else
		{
			button.Y = caption.Bottom + 2;
		}
		button.ToolTip = "Click, then press the new key. Escape cancels.";
		keyButtonColor = button.LabelColor;
		keyButtonValues[button] = keyName;
		button.Text = KeybindMod.DisplayName(keyName);
		button.Click += delegate
		{
			StartKeyCapture(button);
		};
		lineY = Math.Max(caption.Bottom, button.Bottom) + 6;
		return button;
	}

	private void StartKeyCapture(TextButton button)
	{
		if (capturingKeyButton != null)
		{
			capturingKeyButton.Text = KeybindMod.DisplayName(keyButtonValues[capturingKeyButton]);
		}
		capturingKeyButton = button;
		button.Text = "PRESS A KEY...";
	}

	/// <summary>While a key button waits: the first key pressed is its new key, Escape cancels. Returns whether it waited.</summary>
	private bool CaptureKey(InputEventSystem.InputData input)
	{
		TextButton button = capturingKeyButton;
		if (button == null)
		{
			return false;
		}
		if (input.IsKeyTapped(Microsoft.Xna.Framework.Input.Keys.Escape))
		{
			capturingKeyButton = null;
			button.Text = KeybindMod.DisplayName(keyButtonValues[button]);
			return true;
		}
		foreach (Microsoft.Xna.Framework.Input.Keys key in KeybindMod.Bindable)
		{
			if (input.IsKeyTapped(key))
			{
				capturingKeyButton = null;
				keyButtonValues[button] = key.ToString();
				button.Text = KeybindMod.DisplayName(key.ToString());
				MarkDuplicateKeys();
				break;
			}
		}
		return true;
	}

	/// <summary>A key on two buttons turns both red; nothing stops it, since two keys for PAUSE is the studio's own design.</summary>
	private void MarkDuplicateKeys()
	{
		var counts = keyButtonValues.Values.GroupBy((string k) => k).ToDictionary((IGrouping<string, string> g) => g.Key, (IGrouping<string, string> g) => g.Count());
		foreach (KeyValuePair<TextButton, string> pair in keyButtonValues)
		{
			pair.Key.LabelColor = (counts[pair.Value] > 1) ? UIComponent.errorColor : keyButtonColor;
		}
	}

	/// <summary>DEFAULT KEYS: the studio's defaults from a fresh Options, the mods' from each setting's default.</summary>
	private void RestoreDefaultKeys()
	{
		capturingKeyButton = null;
		Options fresh = new Options();
		foreach (KeyValuePair<System.Reflection.FieldInfo, TextButton> pair in vanillaKeyButtons)
		{
			keyButtonValues[pair.Value] = pair.Key.GetValue(fresh).ToString();
			pair.Value.Text = KeybindMod.DisplayName(keyButtonValues[pair.Value]);
		}
		foreach (KeyValuePair<ModSetting, UIComponent> pair in modControls)
		{
			if (pair.Key.Kind == ModSettingKind.Key)
			{
				WriteModControl(pair.Key, pair.Key.DefaultValue);
			}
		}
		MarkDuplicateKeys();
	}

	/// <summary>On OK: the studio's bindings back into Options, which OK then writes to Options.xml.</summary>
	private void ApplyKeyBindings(Options options)
	{
		foreach (KeyValuePair<System.Reflection.FieldInfo, TextButton> pair in vanillaKeyButtons)
		{
			if (Enum.TryParse(keyButtonValues[pair.Value], out Microsoft.Xna.Framework.Input.Keys key))
			{
				pair.Key.SetValue(options, key);
			}
		}
	}

	/// <summary>
	/// On showing the dialog: every control back to what is stored. Without it, a change made and
	/// then CANCELLED was still showing the next time the dialog opened.
	/// </summary>
	private void RefillModControls()
	{
		capturingKeyButton = null;
		foreach (KeyValuePair<ModSetting, UIComponent> pair in modControls)
		{
			WriteModControl(pair.Key, pair.Key.Value);
		}
		Options options = Interface.Game.Controller.Options;
		foreach (KeyValuePair<System.Reflection.FieldInfo, TextButton> pair in vanillaKeyButtons)
		{
			keyButtonValues[pair.Value] = pair.Key.GetValue(options).ToString();
			pair.Value.Text = KeybindMod.DisplayName(keyButtonValues[pair.Value]);
		}
		MarkDuplicateKeys();
	}

	/// <summary>Stacks the categories under the MODS header and sizes the surface to fit them.</summary>
	private void LayOutModCategories()
	{
		if (modsSurface == null)
		{
			return;
		}
		int y = modsTop;
		foreach (CollapsablePanel cp in modCategoryPanels)
		{
			cp.Y = y;
			y = cp.Bottom + 4;
		}
		modsSurface.Height = Math.Max(modsSurfaceMinHeight, y + 12);
	}

	/// <summary>The width of this category's dropdowns, chosen by <see cref="ChoiceWidthFor"/>.</summary>
	private int categoryChoiceWidth;

	/// <summary>
	/// The width of a category's dropdowns: its longest choice as the LCD font draws it, plus the
	/// arrow button (as wide as the box is tall) and the text box's margins. Kastuk: DANGEROUS
	/// FAUNA's "x1.5" sat in a box as wide as SAVE DATE FORMAT's longest pattern; then, with widths
	/// guessed from character counts, PESTS' "Normal" slid under the arrow. Measured, both fit.
	/// </summary>
	private int ChoiceWidthFor(List<ModSetting> settings, int panelWidth)
	{
		int longest = 0;
		Label probe = null;
		foreach (ModSetting s in settings)
		{
			if (s.Kind != ModSettingKind.Choice || s.Choices == null)
			{
				continue;
			}
			foreach (string choice in s.Choices)
			{
				if (probe == null)
				{
					probe = new Label(Interface.gui);
					probe.Init(Label.LabelType.LCDNormal);
				}
				probe.Text = choice ?? "";
				probe.FitToText();
				longest = Math.Max(longest, probe.Width);
			}
		}
		int full = Math.Max(80, panelWidth - 124 - 4);
		if (longest == 0)
		{
			return full;
		}
		const int arrowAndMargins = 22 + 14;
		return Common.Clamp(longest + arrowAndMargins, 60, full);
	}

	/// <summary>One setting's row inside a category; returns the Y below it.</summary>
	private int AddModSettingControl(UIComponent panel, ModSetting setting, int lineY)
	{
		// STABLE or TESTING first (ModSettings.IsStable), so a player reads it before the rest.
		string toolTip = (ModSettings.IsStable(setting) ? "STABLE - confirmed in play. " : "TESTING - not yet confirmed in play. ") + setting.ToolTip;
		if (setting.AffectsSimulation)
		{
			toolTip += " Changes what a save contains: saves made with it on are marked MODDED.";
		}
		int valueX = 124;
		int valueWidth = panel.Width - valueX - 4;

		switch (setting.Kind)
		{
		case ModSettingKind.Toggle:
		{
			CheckBox checkBox = new CheckBox(Interface.gui);
			panel.Add(checkBox);
			checkBox.Init(CheckBoxType.LCD, CheckBoxFlavor.Green);
			checkBox.Text = setting.Label;
			checkBox.FitToText();
			checkBox.X = 6;
			checkBox.Y = lineY;
			checkBox.ToolTip = toolTip;
			checkBox.TooltipWidth = 320;
			// Kastuk: the tooltip went away while the cursor was still on the switch.
			checkBox.TooltipExpires = false;
			checkBox.IsChecked = setting.On;
			modControls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, checkBox));
			return checkBox.Bottom + 8;
		}
		case ModSettingKind.Choice:
		{
			Label caption = new Label(Interface.gui);
			panel.Add(caption);
			caption.Init(Label.LabelType.LCDSmallHeadingBanner);
			caption.Text = setting.Label + ":";
			caption.FitToText();
			caption.X = 6;
			caption.Y = lineY;
			caption.ToolTip = toolTip;
			caption.TooltipWidth = 320;
			caption.TooltipExpires = false;
			ComboBox comboBox = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
			panel.Add(comboBox);
			comboBox.Init(ComboBoxTypes.LCD);
			// Right-aligned at the category's width; beside the caption when it fits, under it
			// when it does not ("NORTHERN BUSH DRAGON AGGRO RANGE:" is long).
			int choiceWidth = (categoryChoiceWidth > 0) ? categoryChoiceWidth : valueWidth;
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
				comboBox.AddEntry(choice, choice);
			}
			if (comboBox.EntriesByKey.ContainsKey(setting.Value))
			{
				comboBox.SelectedKey = setting.Value;
			}
			modControls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, comboBox));
			return Math.Max(caption.Bottom, comboBox.Bottom) + 8;
		}
		default:
		{
			Label caption2 = new Label(Interface.gui);
			panel.Add(caption2);
			caption2.Init(Label.LabelType.LCDSmallHeadingBanner);
			caption2.Text = setting.Label + ":";
			caption2.FitToText();
			caption2.X = 6;
			caption2.Y = lineY;
			caption2.ToolTip = toolTip;
			caption2.TooltipExpires = false;
			TextBox textBox = new TextBox(Interface.gui);
			panel.Add(textBox);
			textBox.Init(TextBox.TextBoxType.LCD);
			textBox.X = valueX;
			textBox.Width = valueWidth;
			textBox.Height = 20;
			textBox.VMargin = 1;
			textBox.IsEditable = true;
			textBox.Text = setting.Value;
			if (caption2.Right + 6 <= valueX)
			{
				textBox.CenterThisVertically(caption2.Y + caption2.Height / 2);
			}
			else
			{
				textBox.Y = caption2.Bottom + 2;
			}
			textBox.ToolTip = toolTip;
			modControls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, textBox));
			return Math.Max(caption2.Bottom, textBox.Bottom) + 8;
		}
		}
	}

	private UIComponent FindModControl(ModSetting setting)
	{
		foreach (KeyValuePair<ModSetting, UIComponent> pair in modControls)
		{
			if (pair.Key == setting)
			{
				return pair.Value;
			}
		}
		return null;
	}

	/// <summary>What a setting's control currently shows - which is not yet what the setting holds.</summary>
	private string ReadModControl(ModSetting setting)
	{
		UIComponent control = FindModControl(setting);
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
		if (control is TextButton keyButton && keyButtonValues.TryGetValue(keyButton, out string keyName))
		{
			return keyName;
		}
		return setting.Value;
	}

	private void WriteModControl(ModSetting setting, string value)
	{
		UIComponent control = FindModControl(setting);
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
		else if (control is TextButton keyButton && keyButtonValues.ContainsKey(keyButton))
		{
			keyButtonValues[keyButton] = value;
			keyButton.Text = KeybindMod.DisplayName(value);
		}
	}

	/// <summary>
	/// Reads the MODS controls back into the settings and writes the file. Called from OK, with
	/// the rest of the dialog - a setting the player changed and then cancelled must not have
	/// taken effect, which is why nothing here happens as the controls are clicked. That includes
	/// the category switches: they only move the controls.
	/// </summary>
	private void ApplyModSettings()
	{
		if (modControls.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<ModSetting, UIComponent> pair in modControls)
		{
			pair.Key.Value = ReadModControl(pair.Key);
		}
		ModSettings.Save(GameStateManagement.UnclaimedWorld.LogError);
	}

	private Label AddSectionHeader(int xYpos, UIComponent panel, string text)
	{
		Label label = new Label(Interface.gui);
		panel.Add(label);
		label.Init(Label.LabelType.LCDBigHeaderBanner);
		label.Text = text;
		label.X = 6;
		label.Y = xYpos;
		label.Width = surfaceGrid.SurfaceWidth - label.X;
		return label;
	}

	private void cbResolution_SelectionChanged(UIComponent sender)
	{
	}

	private void PopulateResolutionsCombo()
	{
		cbResolution.Clear();
		DisplayModeCollection supportedDisplayModes = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes;
		cbResolution.AddEntry("", "");
		foreach (DisplayMode item in supportedDisplayModes)
		{
			if (item.Width >= 800 && item.Format == SurfaceFormat.Color)
			{
				string text = $"Width: {item.Width} Height: {item.Height}";
				cbResolution.AddEntry(item, text);
			}
		}
		cbResolution.SelectionChanged -= cbResolution_SelectionChanged;
		cbResolution.SelectedIndex = 0;
		cbResolution.SelectionChanged += cbResolution_SelectionChanged;
	}

	private void cbFullscreen_Click(UIComponent sender, EventArgs e)
	{
		SetGraphicsEnabledStates();
	}

	private void cbMusic_Click(UIComponent sender, EventArgs e)
	{
		if (cbMusic.IsChecked)
		{
			Interface.Game.Controller.AudioManager.MusicVolume = Interface.Game.Controller.Options.MusicVolume;
		}
		else
		{
			Interface.Game.Controller.AudioManager.MusicVolume = 0f;
		}
		SetSoundEnabledStates();
	}

	private void cbSound_Click(UIComponent sender, EventArgs e)
	{
		if (The.Client != null)
		{
			if (cbSound.IsChecked)
			{
				The.Client.AudioManager.SoundVolume = Interface.Game.Controller.Options.SoundFXVolume;
			}
			else
			{
				The.Client.AudioManager.SoundVolume = 0f;
			}
		}
		SetSoundEnabledStates();
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		ApplySettings();
		Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(this, null);
		}
	}

	private bool CanSetResolution()
	{
		if (cbFullscreen.IsChecked)
		{
			return cbHardwareModeSwitch.IsChecked;
		}
		return true;
	}

	private bool ValidateInput()
	{
		if (CanSetResolution())
		{
			if (rbFixed.IsChecked)
			{
				if (cbResolution.SelectedKey.Equals(""))
				{
					errorsAndMessages.ShowError("Select a resolution from the list.");
					return false;
				}
			}
			else
			{
				if (tbWidth.Text == null || tbHeight.Text == null)
				{
					errorsAndMessages.ShowError("Both width and height is required.");
					return false;
				}
				if (!int.TryParse(tbWidth.Text, out var result))
				{
					errorsAndMessages.ShowError("Illegal width entered.");
					return false;
				}
				if (!int.TryParse(tbHeight.Text, out var result2))
				{
					errorsAndMessages.ShowError("Illegal height entered.");
					return false;
				}
				if (result > UnclaimedWorld.MaxScreenDimensions.X)
				{
					errorsAndMessages.ShowError($"Illegal width entered. {UnclaimedWorld.MaxScreenDimensions.X} is maximum.");
					return false;
				}
				if (result2 > UnclaimedWorld.MaxScreenDimensions.Y)
				{
					errorsAndMessages.ShowError($"Illegal height entered. {UnclaimedWorld.MaxScreenDimensions.Y} is maximum.");
					return false;
				}
			}
		}
		// MOD: the floor here was a hard 100, which refused every magnification below 1 no matter
		// what the rest of the game allowed - so the slider could not reach what Options.xml could.
		// With the mod off this is 100 and the message is the studio's, character for character.
		//
		// The floor is taken for the switch as its checkbox SHOWS - the state OK is about to save -
		// and a slider below it is lifted to it rather than refused. Refusing trapped Kastuk: with
		// 0.8 set and the mod switched off, every OK failed until the slider was moved by hand,
		// and he could not tell why.
		ModSetting allowBelowOne = ModSettings.Find(UWGame.Mods.MagnificationMod.ModId + ".allowBelowOne");
		bool pendingAllowBelowOne = allowBelowOne != null && ModSetting.ParseBool(ReadModControl(allowBelowOne), false);
		int minZoomPercent = UWGame.Mods.MagnificationMod.OptionsFloorPercent(pendingAllowBelowOne);
		if ((float)fbZoom.Value < (float)minZoomPercent)
		{
			fbZoom.Value = minZoomPercent;
			fbZoom.UpdateSliderPosition();
		}
		errorsAndMessages.Hide();
		return true;
	}

	private void btOK_Click(UIComponent sender, EventArgs e)
	{
		if (ValidateInput())
		{
			Options options = Interface.Game.Controller.Options;
			options.MusicEnabled = cbMusic.IsChecked;
			options.SoundEnabled = cbSound.IsChecked;
			options.FullScreen = cbFullscreen.IsChecked;
			options.HardwareModeSwitch = cbHardwareModeSwitch.IsChecked;
			if (!CanSetResolution())
			{
				options.ResolutionWidth = 0;
				options.ResolutionHeight = 0;
			}
			else if (rbFixed.IsChecked)
			{
				DisplayMode displayMode = (DisplayMode)cbResolution.SelectedKey;
				options.ResolutionWidth = displayMode.Width;
				options.ResolutionHeight = displayMode.Height;
			}
			else
			{
				options.ResolutionWidth = int.Parse(tbWidth.Text);
				options.ResolutionHeight = int.Parse(tbHeight.Text);
			}
			if (!cbFullscreen.IsChecked)
			{
				options.Borderless = !cbBorder.IsChecked;
			}
			if (cbMusic.IsChecked)
			{
				options.MusicVolume = SliderValueToVolumeQuad(fbMusicVolume.Value);
			}
			if (cbSound.IsChecked)
			{
				options.SoundFXVolume = SliderValueToVolumeQuad(fbSoundVolume.Value);
			}
			options.ZoomFactor = 0.01f * (float)fbZoom.Value;
			ApplyKeyBindings(options);
			ApplyModSettings();
			ApplyAndSaveOptionsToFile();
			Hide();
			if (this.OKClick != null)
			{
				this.OKClick(this, null);
			}
		}
	}

	private void ApplyAndSaveOptionsToFile()
	{
		ApplySettings();
		Interface.Game.Controller.Options.Write();
	}

	private void ApplySettings()
	{
		Options options = Interface.Game.Controller.Options;
		if (The.Client != null)
		{
			The.Client.AudioManager.Init(options);
		}
		Interface.Game.Controller.AudioManager.Init(options, useFading: false);
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
		Fill();
		RefillModControls();
		shown = this;
	}

	/// <summary>The dialog most recently shown; open while its window is visible.</summary>
	private static OptionsDialog shown;

	/// <summary>
	/// Enter is OK and Escape is CANCEL while the dialog is open. Kastuk: at a small resolution
	/// or magnification the OK and CANCEL buttons can end up off the screen, and the only way out
	/// was editing Options.xml by hand. Called from Client.HandleInput in game and from
	/// MainMenuScreen.HandleInput; returns whether it took the key.
	/// </summary>
	public static bool HandleKeys(InputEventSystem.InputData input)
	{
		OptionsDialog dialog = shown;
		if (dialog == null || input == null || dialog.Window == null || !dialog.Window.IsVisibleAndActive)
		{
			return false;
		}
		// A key button waiting for its key takes every key first - Enter included.
		if (dialog.CaptureKey(input))
		{
			return true;
		}
		if (input.IsKeyTapped(Microsoft.Xna.Framework.Input.Keys.Enter))
		{
			dialog.btOK_Click(null, null);
			return true;
		}
		if (input.IsKeyTapped(Microsoft.Xna.Framework.Input.Keys.Escape))
		{
			dialog.btCancel_Click(null, null);
			return true;
		}
		return false;
	}

	private void Fill()
	{
		Options options = Interface.Game.Controller.Options;
		cbMusic.IsChecked = options.MusicEnabled;
		cbSound.IsChecked = options.SoundEnabled;
		cbFullscreen.IsChecked = options.FullScreen;
		cbHardwareModeSwitch.IsChecked = options.HardwareModeSwitch;
		cbBorder.IsChecked = !options.Borderless;
		fbSoundVolume.Value = VolumeToSliderIncrementsQuad(options.SoundFXVolume);
		fbMusicVolume.Value = VolumeToSliderIncrementsQuad(options.MusicVolume);
		fbSoundVolume.UpdateSliderPosition();
		fbMusicVolume.UpdateSliderPosition();
		fbZoom.Value = (int)(options.ZoomFactor * 100f);
		fbZoom.UpdateSliderPosition();
		PopulateResolutionsCombo();
		if (CanSetResolution())
		{
			object obj = cbResolution.EntriesByKey.Keys.FirstOrDefault((object key) => key is DisplayMode && ((DisplayMode)key).Width == options.ResolutionWidth && ((DisplayMode)key).Height == options.ResolutionHeight);
			if (obj != null)
			{
				cbResolution.SelectedKey = obj;
				EnableFixedResolution();
				rgResolution.SelectMember(rbFixed);
			}
			else
			{
				cbResolution.SelectedKey = "";
				tbWidth.Text = options.ResolutionWidth.ToString();
				tbHeight.Text = options.ResolutionHeight.ToString();
				EnableCustomResolution();
				rgResolution.SelectMember(rbCustom);
			}
		}
		else
		{
			cbResolution.SelectedKey = "";
			tbWidth.Text = "";
			tbHeight.Text = "";
			DisableResolution();
		}
		if (cbFullscreen.IsChecked)
		{
			cbBorder.Enabled = false;
			rbCustom.Enabled = false;
		}
		else
		{
			cbBorder.Enabled = true;
			rbCustom.Enabled = true;
		}
		SetSoundEnabledStates();
	}

	private void SetGraphicsEnabledStates()
	{
		if (cbFullscreen.IsChecked)
		{
			cbBorder.Enabled = false;
			cbHardwareModeSwitch.Enabled = true;
			if (cbHardwareModeSwitch.IsChecked)
			{
				EnableFixedResolution();
				rbCustom.Enabled = false;
				rgResolution.SelectMember(rbFixed);
			}
			else
			{
				DisableResolution();
			}
		}
		else
		{
			cbBorder.Enabled = true;
			cbHardwareModeSwitch.Enabled = false;
			rbFixed.Enabled = true;
			cbResolution.Enabled = true;
			rbCustom.Enabled = true;
		}
	}

	private void SetSoundEnabledStates()
	{
		if (cbMusic.IsChecked)
		{
			fbMusicVolume.Enabled = true;
		}
		else
		{
			fbMusicVolume.Enabled = false;
		}
		if (cbSound.IsChecked)
		{
			fbSoundVolume.Enabled = true;
		}
		else
		{
			fbSoundVolume.Enabled = false;
		}
	}
}
