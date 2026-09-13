using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class LogAlert : HUDWindow
{
	private Label label;

	public static Color AlertColor = Color.GreenYellow;

	private double timeElapsed;

	public const int AlertWindowHeight = 28;

	public string Text => label.Text;

	public LogAlert()
		: base(404, 28, hasSurface: true, hasCloseButton: true)
	{
		HideOnRightClick = false;
		base.CloseButtonYPos = 2;
		DisplayWindow.Level = Level.Bottom;
		label = new Label(DisplayWindow.guiManager);
		Add(label);
		label.Init(Label.LabelType.HUDWindow);
		label.NormalColor = AlertColor;
		label.X = 8;
		label.Y = 6;
		SetLabelSize();
		DisplayWindow.Resize += DisplayWindow_Resize;
	}

	private void SetLabelSize()
	{
		label.Width = btClose.X - 2 - label.X;
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		SetLabelSize();
	}

	public void Show(string text)
	{
		label.Text = text;
		timeElapsed = 0.0;
		DisplayWindow.X = The.InGameUI.LogPanel.HUDEventLog.DisplayWindow.X;
		DisplayWindow.Y = The.InGameUI.LogPanel.GetTopOfAlertWindows() - 4;
		DisplayWindow.Width = The.InGameUI.LogPanel.HUDEventLog.DisplayWindow.Width;
		DisplayWindow.Show();
		DisplayWindow.BringToTop();
	}

	public override void Update(GameTime gameTime)
	{
		timeElapsed += gameTime.ElapsedGameTime.TotalSeconds;
	}

	public bool IsExpired()
	{
		return timeElapsed > (double)The.Client.Controller.Options.AlertLifetime;
	}

	public override void Hide()
	{
		base.Hide();
		The.InGameUI.LogPanel.Retire(this);
	}
}
