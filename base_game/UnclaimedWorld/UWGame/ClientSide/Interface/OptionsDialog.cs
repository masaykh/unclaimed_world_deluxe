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
		fbZoom.StepSize = 25;
		fbZoom.KnobWidth = 16;
		fbZoom.ShowNotches = true;
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
	/// The MODS section: one control per registered <see cref="ModSetting"/>, in registration
	/// order.
	///
	/// It is built from the registry rather than from a list of fields because a third-party mod
	/// cannot add a field to this class - which is the whole reason mod settings do not live in
	/// Options.xml. A build with no mods still gets the section, because the port registers two
	/// entries of its own (see PortSettings), so this code is exercised everywhere rather than
	/// only where someone has installed something.
	///
	/// The surface grows to fit; it is inside a scrolling grid, so a long list scrolls rather than
	/// being cut off.
	/// </summary>
	private void BuildModsSection(UIComponent panel, int y)
	{
		modControls.Clear();
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

		foreach (ModSetting setting in ModSettings.All)
		{
			string toolTip = setting.ToolTip;
			if (setting.AffectsSimulation)
			{
				toolTip += " Changes what a save contains: saves made with it on are marked MODDED.";
			}

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
				checkBox.IsChecked = setting.On;
				modControls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, checkBox));
				lineY = checkBox.Bottom + 8;
				break;
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
				ComboBox comboBox = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
				panel.Add(comboBox);
				comboBox.Init(ComboBoxTypes.LCD);
				comboBox.X = 128;
				comboBox.Width = 200;
				comboBox.CenterThisVertically(caption.Y + caption.Height / 2);
				comboBox.ToolTip = toolTip;
				foreach (string choice in setting.Choices ?? new string[0])
				{
					comboBox.AddEntry(choice, choice);
				}
				if (comboBox.EntriesByKey.ContainsKey(setting.Value))
				{
					comboBox.SelectedKey = setting.Value;
				}
				modControls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, comboBox));
				lineY = Math.Max(caption.Bottom, comboBox.Bottom) + 8;
				break;
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
				TextBox textBox = new TextBox(Interface.gui);
				panel.Add(textBox);
				textBox.Init(TextBox.TextBoxType.LCD);
				textBox.X = 128;
				textBox.Width = 200;
				textBox.Height = 20;
				textBox.VMargin = 1;
				textBox.IsEditable = true;
				textBox.Text = setting.Value;
				textBox.CenterThisVertically(caption2.Y + caption2.Height / 2);
				textBox.ToolTip = toolTip;
				modControls.Add(new KeyValuePair<ModSetting, UIComponent>(setting, textBox));
				lineY = Math.Max(caption2.Bottom, textBox.Bottom) + 8;
				break;
			}
			}
		}

		if (lineY + 12 > panel.Height)
		{
			panel.Height = lineY + 12;
		}
	}

	/// <summary>
	/// Reads the MODS controls back into the settings and writes the file. Called from OK, with
	/// the rest of the dialog - a setting the player changed and then cancelled must not have
	/// taken effect, which is why nothing here happens as the controls are clicked.
	/// </summary>
	private void ApplyModSettings()
	{
		if (modControls.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<ModSetting, UIComponent> pair in modControls)
		{
			if (pair.Value is CheckBox checkBox)
			{
				pair.Key.Value = checkBox.IsChecked ? "true" : "false";
			}
			else if (pair.Value is ComboBox comboBox)
			{
				pair.Key.Value = comboBox.SelectedKey as string ?? pair.Key.Value;
			}
			else if (pair.Value is TextBox textBox)
			{
				pair.Key.Value = textBox.Text;
			}
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
		int minZoomPercent = UWGame.Mods.MagnificationMod.OptionsFloorPercent();
		if ((float)fbZoom.Value < (float)minZoomPercent)
		{
			errorsAndMessages.ShowError("Magnification must be at least "
				+ (0.01f * (float)minZoomPercent).ToString("0.##") + ".");
			return false;
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
