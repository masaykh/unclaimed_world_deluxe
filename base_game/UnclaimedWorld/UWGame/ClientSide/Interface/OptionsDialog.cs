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

	/// <summary>PORT: the LANGUAGE row and the studio's key buttons (ModSettingRows, as SELECT MODS' MOD OPTIONS uses).</summary>
	private readonly ModSettingRows modRows;

	/// <summary>PORT: opened from the in-game menu - MOD OPTIONS then cannot switch mods on or off.</summary>
	private readonly bool inGame;

	/// <summary>PORT: SELECT MODS, shown over this dialog by MOD OPTIONS...; made on first use.</summary>
	private UWGame.ClientSide.MainMenu.Mods.SelectModsPanel modsPanel;

	public event EventHandler CancelClick;

	public event EventHandler OKClick;

	public OptionsDialog(CommonInterface intf, bool inGame = false)
		: base(intf, UWGame.Locale.Text("OPTIONS"), new Point(420, 120), new Vector2(400f, 600f), Level.Menu, PanelType.RegularEdges)
	{
		GUIManager gui = Interface.gui;
		this.inGame = inGame;
		modRows = new ModSettingRows(gui);
		int x = 124;
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 52, Panel.RosterMargin, out display, out lcdSurface, ref lcdScreen);
		CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, canHaveFocus: false, 0, 40);
		UIComponent uIComponent = new UIComponent(Interface.gui)
		{
			Width = surfaceGrid.SurfaceWidth,
			Height = 600
		};
		surfaceGrid.AddEntry("surfaceKey", uIComponent);
		Label label = AddSectionHeader(6, uIComponent, UWGame.Locale.Text("GRAPHICS"));
		int warningBottom = ModSettingRows.AddWarning(Interface.gui, uIComponent, UWGame.Locale.Text("Restart the game to apply Graphics changes!"), label.Bottom + 6);
		cbFullscreen = new CheckBox(Interface.gui);
		uIComponent.Add(cbFullscreen);
		cbFullscreen.Init(CheckBoxType.LCD, CheckBoxFlavor.Blue);
		cbFullscreen.Text = UWGame.Locale.Text("FULL SCREEN");
		cbFullscreen.FitToText();
		cbFullscreen.X = 6;
		cbFullscreen.Y = warningBottom + 6;
		cbFullscreen.Click += cbFullscreen_Click;
		int y = cbFullscreen.Bottom + 6;
		cbHardwareModeSwitch = new CheckBox(Interface.gui);
		uIComponent.Add(cbHardwareModeSwitch);
		cbHardwareModeSwitch.Init(CheckBoxType.LCD, CheckBoxFlavor.Green);
		cbHardwareModeSwitch.Text = UWGame.Locale.Text("HARDWARE MODE SWITCH");
		cbHardwareModeSwitch.FitToText();
		cbHardwareModeSwitch.X = x;
		cbHardwareModeSwitch.Y = y;
		cbHardwareModeSwitch.ToolTip = UWGame.Locale.Text("If selected, the game attempts to switch the screen resolution when in fullscreen. If not selected, only the desktop resolution is available for fullscreen. Is selected by default.");
		cbHardwareModeSwitch.Click += cbHardwareModeSwitch_Click;
		y = cbHardwareModeSwitch.Bottom + 12;
		Label label3 = new Label(Interface.gui);
		uIComponent.Add(label3);
		label3.Init(Label.LabelType.LCDHeadingBlue);
		label3.Text = UWGame.Locale.Text("RESOLUTION");
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
		rbFixed.Text = UWGame.Locale.Text("FIXED:");
		rbFixed.FitToText();
		rbFixed.CenterThisVertically(y);
		rbFixed.X = 6;
		rbFixed.ToolTip = UWGame.Locale.Text("Select this option to select a fixed resolution among those supported by the video card.");
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
		rbCustom.Text = UWGame.Locale.Text("CUSTOM:");
		rbCustom.FitToText();
		rbCustom.CenterThisVertically(yPosToCenterTo);
		rbCustom.X = 6;
		// PORT: the column starts after the longer of FIXED: and CUSTOM: in whatever language they
		// are in, and the list takes the rest of the line - "Width: 1366 Height: 768" is longer in
		// most languages than in English (Kastuk, Russian: the height was cut off).
		int column = System.Math.Max(x, System.Math.Max(rbFixed.Right, rbCustom.Right) + 6);
		cbResolution.X = column;
		cbResolution.Width = System.Math.Max(200, uIComponent.Width - column - 6);
		rbCustom.ToolTip = UWGame.Locale.Text("Select this option to enter a custom window size. Not available in fullscreen.");
		Label label4 = new Label(Interface.gui);
		uIComponent.Add(label4);
		label4.Init(Label.LabelType.LCDHeadingBlue);
		label4.Text = UWGame.Locale.Text("W:");
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
		label5.Text = UWGame.Locale.Text("H:");
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
		cbBorder.Text = UWGame.Locale.Text("BORDER");
		cbBorder.FitToText();
		cbBorder.ToolTip = UWGame.Locale.Text("Select whether the application window should have a border.");
		cbBorder.Position = new Point(6, tbHeight.Bottom + 24);
		string toolTip = UWGame.Locale.Text("Set the magnification (pixel zoom) level. Restart the game to see the effect. WARNING: Setting a high magnification level may cause the interface to be truncated. In case of problems, use the ZoomFactor property in Options.xml to revert.");
		Label label6 = new Label(Interface.gui);
		uIComponent.Add(label6);
		label6.Init(Label.LabelType.LCDSmallHeadingBanner);
		label6.Text = UWGame.Locale.Text("MAGNIFICATION:");
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
		y = AddSectionHeader(280, uIComponent, UWGame.Locale.Text("SOUND")).Bottom + 6;
		cbMusic = new CheckBox(Interface.gui);
		uIComponent.Add(cbMusic);
		cbMusic.Text = UWGame.Locale.Text("MUSIC");
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
		fbMusicVolume.ToolTip = UWGame.Locale.Text("Set the music volume.");
		fbMusicVolume.SliderMouseUp += fbMusicVolume_SliderMouseUp;
		fbMusicVolume.MaxValue = 100;
		fbMusicVolume.ShowMaxValueLabelAtEnd = false;
		fbMusicVolume.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Never;
		fbMusicVolume.KnobWidth = 16;
		y = cbMusic.Y + 24;
		cbSound = new CheckBox(Interface.gui);
		uIComponent.Add(cbSound);
		cbSound.Text = UWGame.Locale.Text("SOUND");
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
		fbSoundVolume.ToolTip = UWGame.Locale.Text("Set the sound effects volume.");
		fbSoundVolume.SliderMouseUp += fbSoundVolume_SliderMouseUp;
		fbSoundVolume.MaxValue = 100;
		fbSoundVolume.ShowMaxValueLabelAtEnd = false;
		fbSoundVolume.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Never;
		fbSoundVolume.KnobWidth = 16;
		BuildPortSections(uIComponent, fbSoundVolume.Bottom + 24);
		errorsAndMessages = new ErrorsAndMessages(lcdSurface, Window.guiManager, 6, lcdSurface.Height - 24);
		AddLowerButton(UWGame.Locale.Text("OK"), UWGame.Locale.Text("Accepts the changes and closes the dialog."), Align.Left).Click += btOK_Click;
		AddLowerButton(UWGame.Locale.Text("CANCEL"), UWGame.Locale.Text("Closes the dialog without applying the changes."), Align.Right).Click += btCancel_Click;
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
	/// PORT: what OPTIONS has beyond the studio's GRAPHICS and SOUND (original_src's OptionsDialog
	/// has those two and nothing else). The user, 2026-10-18: "all mods should be explicitly under
	/// modding", then "language selection belong to options; key binding that not mod related
	/// too". So three sections, and no mod's setting:
	///
	/// LANGUAGE - the port's LANGUAGE (<see cref="ModCatalog.InOptions"/>; Kastuk, "Translation").
	///
	/// KEYS (KeybindMod) - the studio's own bindings: the public Keys fields of Options, which
	/// Options.xml keeps (KeybindMod.VanillaFields). A key a mod registers (ModSettings.Key) is the
	/// mod's, and is rebound under that mod's MOD OPTIONS in SELECT MODS - the port's shadows key
	/// under PORT's.
	///
	/// MODS - one button, MOD OPTIONS..., which shows SELECT MODS over this dialog (tripleacoder:
	/// "replace the long mod options section with a button 'Mod options' that opens the Select
	/// mods dialog, and ... show / hide the Enable mod checkbox and change the dialog title to
	/// 'Mod options'"). Everything there applies and is saved as it is changed; OK and CANCEL here
	/// are about this dialog's own controls.
	/// </summary>
	private void BuildPortSections(UIComponent panel, int y)
	{
		modRows.Clear();
		vanillaKeyButtons.Clear();
		// PORT: a translation dropped into data/BaseData/Strings while the game runs is offered too.
		PortSettings.RefreshLanguageChoices();
		List<ModSetting> here = OptionsSettings();
		if (here.Count > 0)
		{
			y = AddSectionHeader(y, panel, UWGame.Locale.Text("LANGUAGE")).Bottom + 6;
			y = ModSettingRows.AddReloadWarning(Interface.gui, panel, here, y);
			y = modRows.AddGroup(panel, here, y) + 6;
		}
		if (KeybindMod.Enabled)
		{
			y = AddSectionHeader(y, panel, UWGame.Locale.Text("KEYS")).Bottom + 6;
			y = BuildKeys(panel, y) + 6;
		}
		y = AddSectionHeader(y, panel, UWGame.Locale.Text("MODS")).Bottom + 6;
		TextButton modOptions = new TextButton(Interface.gui);
		panel.Add(modOptions);
		modOptions.Init(TextButton.TextButtonType.LCDToolTipBlack);
		modOptions.CheckedMode = CheckedModes.CannotBeChecked;
		modOptions.Text = UWGame.Locale.Text("MOD OPTIONS...");
		modOptions.Width = 160;
		modOptions.X = 6;
		modOptions.Y = y;
		modOptions.ToolTip = inGame
			? UWGame.Locale.Text("Every mod's settings and keys, and the port's own: changed and saved at once. Mods are switched on and off in the main menu, under MODDING -> SELECT MODS.")
			: UWGame.Locale.Text("SELECT MODS: switch mods on and off, and change their settings and keys, and the port's own. Changes are saved at once.");
		modOptions.TooltipWidth = 320;
		modOptions.Click += btModOptions_Click;
		panel.Height = Math.Max(panel.Height, modOptions.Bottom + 12);
	}

	/// <summary>
	/// The registered settings OPTIONS shows - LANGUAGE (<see cref="ModCatalog.InOptions"/>). Every
	/// other one is under an entry of SELECT MODS.
	/// </summary>
	public static List<ModSetting> OptionsSettings() => ModSettings.All.Where(ModCatalog.InOptions).ToList();

	/// <summary>MOD OPTIONS...: SELECT MODS over this dialog, which comes back as it was left when that closes.</summary>
	private void btModOptions_Click(UIComponent sender, EventArgs e)
	{
		modRows.CancelKeyCapture();
		if (modsPanel == null)
		{
			modsPanel = new UWGame.ClientSide.MainMenu.Mods.SelectModsPanel(Interface, inGame
				? UWGame.ClientSide.MainMenu.Mods.SelectModsPanel.Mode.InGame
				: UWGame.ClientSide.MainMenu.Mods.SelectModsPanel.Mode.FromOptions);
			modsPanel.CancelClick += modsPanel_CancelClick;
		}
		Window.Hide();
		modsPanel.ShowDialog(modal: true);
	}

	private void modsPanel_CancelClick(object sender, EventArgs e)
	{
		// Panel's, not this class's ShowDialog: that would put back every control from what is
		// stored, and lose what was changed here before MOD OPTIONS was opened.
		base.ShowDialog(modal: true);
	}

	// ---- KEYS (KeybindMod): the studio's own ------------------------------------------------

	/// <summary>The studio's bindings: the Options field each button edits.</summary>
	private readonly List<KeyValuePair<System.Reflection.FieldInfo, TextButton>> vanillaKeyButtons =
		new List<KeyValuePair<System.Reflection.FieldInfo, TextButton>>();

	/// <summary>The studio's key rows and DEFAULT KEYS, from <paramref name="lineY"/>; returns the Y below them.</summary>
	private int BuildKeys(UIComponent panel, int lineY)
	{
		Options options = Interface.Game.Controller.Options;
		foreach ((System.Reflection.FieldInfo field, string label) in KeybindMod.VanillaFields())
		{
			TextButton button = modRows.AddKeyRow(panel, null, label, field.GetValue(options).ToString(), UWGame.Locale.Text("The studio's binding, saved in Options.xml."), ref lineY);
			vanillaKeyButtons.Add(new KeyValuePair<System.Reflection.FieldInfo, TextButton>(field, button));
		}
		// A studio key that a mod's key also uses turns red, as two studio keys do.
		modRows.KeysElsewhere = ModKeys;
		// Kastuk: GAME MENU rebound away from Escape could never be put back, since Escape cancels
		// a rebinding. This puts every key here back to the studio's default - pending, like
		// everything else here, until OK.
		TextButton defaults = new TextButton(Interface.gui);
		panel.Add(defaults);
		defaults.Init(TextButton.TextButtonType.LCDToolTipBlack);
		defaults.CheckedMode = CheckedModes.CannotBeChecked;
		defaults.Text = UWGame.Locale.Text("DEFAULT KEYS");
		defaults.Width = 120;
		defaults.X = panel.Width - defaults.Width - 4;
		defaults.Y = lineY;
		defaults.ToolTip = UWGame.Locale.Text("Every key here back to the studio's default. Nothing is applied until OK.");
		defaults.Click += delegate
		{
			RestoreDefaultKeys();
		};
		modRows.MarkDuplicateKeys();
		return defaults.Bottom;
	}

	/// <summary>The mods' keys, as their settings hold them (rebound in SELECT MODS).</summary>
	private static IEnumerable<string> ModKeys() =>
		ModSettings.All.Where((ModSetting s) => s.Kind == ModSettingKind.Key).Select((ModSetting s) => s.Value);

	/// <summary>DEFAULT KEYS: the studio's defaults, from a fresh Options.</summary>
	private void RestoreDefaultKeys()
	{
		modRows.CancelKeyCapture();
		Options fresh = new Options();
		foreach (KeyValuePair<System.Reflection.FieldInfo, TextButton> pair in vanillaKeyButtons)
		{
			modRows.KeyButtonValues[pair.Value] = pair.Key.GetValue(fresh).ToString();
			pair.Value.Text = KeybindMod.DisplayName(modRows.KeyButtonValues[pair.Value]);
		}
		modRows.MarkDuplicateKeys();
	}

	/// <summary>On OK: the studio's bindings back into Options, which OK then writes to Options.xml.</summary>
	private void ApplyKeyBindings(Options options)
	{
		foreach (KeyValuePair<System.Reflection.FieldInfo, TextButton> pair in vanillaKeyButtons)
		{
			if (Enum.TryParse(modRows.KeyButtonValues[pair.Value], out Microsoft.Xna.Framework.Input.Keys key))
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
		modRows.CancelKeyCapture();
		modRows.Refill();
		Options options = Interface.Game.Controller.Options;
		foreach (KeyValuePair<System.Reflection.FieldInfo, TextButton> pair in vanillaKeyButtons)
		{
			modRows.KeyButtonValues[pair.Value] = pair.Key.GetValue(options).ToString();
			pair.Value.Text = KeybindMod.DisplayName(modRows.KeyButtonValues[pair.Value]);
		}
		modRows.MarkDuplicateKeys();
	}

	/// <summary>
	/// Reads LANGUAGE back into its setting and writes the file. Called from OK, with the rest of
	/// the dialog - a setting the player changed and then cancelled must not have taken effect,
	/// which is why nothing here happens as the controls are clicked. (The studio's keys go back
	/// through ApplyKeyBindings; the mods' settings are SELECT MODS', which saves them at once.)
	/// </summary>
	private void ApplyModSettings()
	{
		modRows.CancelKeyCapture();
		modRows.ApplyAll(GameStateManagement.UnclaimedWorld.LogError);
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
				string text = string.Format(UWGame.Locale.Text("Width: {0} Height: {1}"), item.Width, item.Height);
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
		modRows.CancelKeyCapture();
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
					errorsAndMessages.ShowError(UWGame.Locale.Text("Select a resolution from the list."));
					return false;
				}
			}
			else
			{
				if (tbWidth.Text == null || tbHeight.Text == null)
				{
					errorsAndMessages.ShowError(UWGame.Locale.Text("Both width and height is required."));
					return false;
				}
				if (!int.TryParse(tbWidth.Text, out var result))
				{
					errorsAndMessages.ShowError(UWGame.Locale.Text("Illegal width entered."));
					return false;
				}
				if (!int.TryParse(tbHeight.Text, out var result2))
				{
					errorsAndMessages.ShowError(UWGame.Locale.Text("Illegal height entered."));
					return false;
				}
				if (result > UnclaimedWorld.MaxScreenDimensions.X)
				{
					errorsAndMessages.ShowError(string.Format(UWGame.Locale.Text("Illegal width entered. {0} is maximum."), UnclaimedWorld.MaxScreenDimensions.X));
					return false;
				}
				if (result2 > UnclaimedWorld.MaxScreenDimensions.Y)
				{
					errorsAndMessages.ShowError(string.Format(UWGame.Locale.Text("Illegal height entered. {0} is maximum."), UnclaimedWorld.MaxScreenDimensions.Y));
					return false;
				}
			}
		}
		// MOD: the floor here was a hard 100, which refused every magnification below 1 no matter
		// what the rest of the game allowed - so the slider could not reach what Options.xml could.
		// With the mod off this is 100 and the message is the studio's, character for character.
		//
		// The floor is taken for the switch as it is set - in SELECT MODS, which saves it at once -
		// and a slider below it is lifted to it rather than refused. Refusing trapped Kastuk: with
		// 0.8 set and the mod switched off, every OK failed until the slider was moved by hand,
		// and he could not tell why.
		ModSetting allowBelowOne = ModSettings.Find(UWGame.Mods.MagnificationMod.ModId + ".allowBelowOne");
		bool pendingAllowBelowOne = allowBelowOne != null && allowBelowOne.On;
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
		// A key button waiting for its key - here or in MOD OPTIONS - takes every key first, Enter
		// included; then MOD OPTIONS, shown over this dialog, takes Escape.
		if (ModSettingRows.CaptureKey(input) || UWGame.ClientSide.MainMenu.Mods.SelectModsPanel.HandleKeys(input))
		{
			return true;
		}
		OptionsDialog dialog = shown;
		if (dialog == null || input == null || dialog.Window == null || !dialog.Window.IsVisibleAndActive)
		{
			return false;
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
			tbWidth.Text = UWGame.Locale.Text("");
			tbHeight.Text = UWGame.Locale.Text("");
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
