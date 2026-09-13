using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface.DateAndWeatherPanel;

public class DateAndWeather
{
	public HUDDateAndWeather HUDPanel;

	public Window PlasticPanel;

	private Bar frame;

	private const int hudWidth = 385;

	public const int Width = 445;

	private const int frameEdgeWidth = 33;

	private const int hudWindowXPos = 30;

	public DateAndWeather(int xPos)
	{
		HUDPanel = new HUDDateAndWeather(xPos + 30, 385);
		HUDPanel.DisplayWindow.Y = 14;
		HUDPanel.DisplayWindow.Resize += DisplayWindow_Resize;
		PlasticPanel = new Window(The.InGameUI.gui);
		PlasticPanel.Margin = 0;
		PlasticPanel.IsMovable = false;
		PlasticPanel.Resizable = false;
		PlasticPanel.HasCloseButton = false;
		PlasticPanel.Position = new Point(xPos, 0);
		PlasticPanel.WindowSize = new Vector2(30f, 43f);
		PlasticPanel.Level = Level.Bottom;
		PlasticPanel.Show();
		frame = new Bar(The.InGameUI.gui);
		frame.SetSkinLocation(SkinState.Normal, The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("date_weather_panel_frame"));
		frame.EdgeSize = 33;
		PlasticPanel.Add(frame);
		frame.Height = PlasticPanel.Height;
		UpdateFrameSize();
	}

	public void SetPosition(int xPos)
	{
		PlasticPanel.X = xPos;
		HUDPanel.DisplayWindow.X = xPos + 30;
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		UpdateFrameSize();
	}

	private void UpdateFrameSize()
	{
		PlasticPanel.Width = HUDPanel.DisplayWindow.Width + 60;
		frame.Width = PlasticPanel.Width;
	}
}
