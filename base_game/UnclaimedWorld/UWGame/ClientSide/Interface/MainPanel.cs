using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class MainPanel
{
	private class SpeedArg : EventArgs
	{
		public Speeds Speed;
	}

	public Window DisplayWindow;

	protected InGameInterface intf = The.InGameUI;

	private const int height = 52;

	public ImageButton btHelp;

	public TextButton tbMain;

	public TextButton btPause;

	public TextButton btNormalSpeed;

	public TextButton bt2GameSpeed;

	public TextButton bt4GameSpeed;

	private RadioGroup speedGroup;

	private Rectangle pauseIcon;

	private Rectangle pausePosition;

	private const int buttonHeight = 32;

	public MainPanel()
	{
		int num = ((The.Sim.Mode != Sim.EngineMode.Game) ? 160 : 250);
		DisplayWindow = new Window(intf.gui);
		DisplayWindow.Position = new Point(0, 0);
		DisplayWindow.WindowSize = new Vector2(num, 52f);
		DisplayWindow.Level = Level.Bottom;
		DisplayWindow.IsMovable = false;
		DisplayWindow.Resizable = false;
		DisplayWindow.Margin = 0;
		DisplayWindow.HasCloseButton = false;
		DisplayWindow.Skin = intf.gui.GUISpriteSheet.GetSourceRectangle("optionspanel");
		DisplayWindow.CornerSize = 52;
		DisplayWindow.Show();
		int buttonLeft = 22;
		int num2 = 6;
		tbMain = AddBlackTextButton(buttonLeft, num2, "MENU", "Show the game menu");
		tbMain.ScaleWidthToFitText();
		tbMain.Click += main_Click;
		tbMain.DebugTag = "tbMain";
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			btHelp = new ImageButton(intf.gui);
			DisplayWindow.Add(btHelp);
			btHelp.InitWithIcon(ImageButtonType.Black, "help_button_icon", hasCheckedState: true);
			btHelp.X = tbMain.Right + 5;
			btHelp.Y = num2;
			btHelp.ToolTip = "Show various help topics";
			btHelp.Click += tbHelp_Click;
			btHelp.Height = 32;
			speedGroup = new RadioGroup(The.InGameUI.gui);
			DisplayWindow.Add(speedGroup);
			speedGroup.X = btHelp.Right + 4;
			speedGroup.Y = num2;
			speedGroup.Height = 32;
			speedGroup.Width = 128;
			speedGroup.NewMemberChecked += speedGroup_NewMemberChecked;
			int num3 = -3;
			btPause = AddSpeedButton(speedGroup, 0, Speeds.Pause, "II", "Pause the game");
			btNormalSpeed = AddSpeedButton(speedGroup, btPause.Right + num3, Speeds.Normal, "1X", "Set to normal game speed.");
			bt2GameSpeed = AddSpeedButton(speedGroup, btNormalSpeed.Right + num3, Speeds.TwiceNormal, "2X", "Set to 2 times the normal game speed.");
			bt4GameSpeed = AddSpeedButton(speedGroup, bt2GameSpeed.Right + num3, Speeds.FourTimesNormal, "4X", "Set to 4 times the normal game speed.");
			pausePosition = default(Rectangle);
			pauseIcon = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_pause_big");
			pausePosition.Width = pauseIcon.Width;
			pausePosition.Height = pauseIcon.Height;
			pausePosition.X = btPause.Right - 12;
			pausePosition.Y = btPause.Bottom + 25;
			SetSpeedButton(The.Sim.Speed);
		}
	}

	private void speedGroup_NewMemberChecked(ICanBeChecked arg1, EventArgs arg2)
	{
		Speeds speed = ((SpeedArg)((UIComponent)arg1).EventArgs).Speed;
		switch (speed)
		{
		case Speeds.Pause:
			if (!The.Sim.IsPaused)
			{
				The.Client.PauseGame();
			}
			break;
		case Speeds.Normal:
		case Speeds.TwiceNormal:
		case Speeds.FourTimesNormal:
			if (The.Sim.IsPaused)
			{
				The.Client.ResumeGame();
			}
			The.Client.SetGameSpeed(speed);
			break;
		}
	}

	private void tbHelp_Click(UIComponent sender, EventArgs e)
	{
		if (!intf.HUDHelpPanel.DisplayWindow.IsVisibleAndActive)
		{
			intf.HUDHelpPanel.ShowInScreenSpace(sender.AbsolutePosition.X, sender.AbsolutePosition.Y + sender.Height);
		}
		else
		{
			intf.HUDHelpPanel.Hide();
		}
	}

	public void OnSetSpeed(Speeds speed)
	{
		SetSpeedButton(speed);
	}

	private void SetSpeedButton(Speeds speed)
	{
		switch (speed)
		{
		case Speeds.Pause:
			speedGroup.SelectMember(btPause);
			break;
		case Speeds.Normal:
			speedGroup.SelectMember(btNormalSpeed);
			break;
		case Speeds.TwiceNormal:
			speedGroup.SelectMember(bt2GameSpeed);
			break;
		case Speeds.FourTimesNormal:
			speedGroup.SelectMember(bt4GameSpeed);
			break;
		}
	}

	public void OnPause()
	{
		SetSpeedButton(Speeds.Pause);
	}

	public void OnResume()
	{
		SetSpeedButton(Speeds.Normal);
	}

	private void main_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ShowInGameMenu();
	}

	private TextButton AddBlackTextButton(int buttonLeft, int buttonTop, string text, string tooltip)
	{
		TextButton textButton = new TextButton(intf.gui);
		DisplayWindow.Add(textButton);
		textButton.Text = text;
		textButton.Position = new Point(buttonLeft, buttonTop);
		textButton.Init(TextButton.TextButtonType.Black);
		textButton.Height = 32;
		textButton.ToolTip = tooltip;
		return textButton;
	}

	private TextButton AddSpeedButton(RadioGroup rgSpeed, int buttonLeft, Speeds speed, string text, string tooltip)
	{
		TextButton textButton = new TextButton(intf.gui);
		rgSpeed.Add(textButton);
		textButton.Text = text;
		textButton.Position = new Point(buttonLeft, 0);
		textButton.Init(TextButton.TextButtonType.BlackSlim);
		textButton.CheckedMode = CheckedModes.CanBeChecked;
		textButton.Height = 32;
		textButton.ScaleWidthToFitText();
		textButton.ToolTip = tooltip;
		textButton.EventArgs = new SpeedArg
		{
			Speed = speed
		};
		return textButton;
	}

	public void DrawPauseIcon()
	{
		The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		The.Client.spriteBatch.Draw(The.InGameUI.gui.GUISpriteSheet.Texture, pausePosition, pauseIcon, The.InGameUI.SelectedCyclePlayer.GetCurrentColor(Color.White));
		The.Client.spriteBatch.End();
	}
}
