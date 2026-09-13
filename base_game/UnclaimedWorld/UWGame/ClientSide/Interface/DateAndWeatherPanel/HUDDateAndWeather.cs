using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.HUD_Windows;
using WindowSystem;

namespace UWGame.ClientSide.Interface.DateAndWeatherPanel;

public class HUDDateAndWeather : HUDWindow
{
	private Label time;

	private Label season;

	private Label year;

	private Label tempNow;

	private Label tempAhead;

	private Label weatherNow;

	private Label weatherAhead;

	private Color headerColor = new Color(226, 226, 226);

	private Color weatherNowColor = "4BE7CF".ColorFromHex();

	private Color weatherLaterColor = "22B89D".ColorFromHex();

	private UIComponent surface;

	public void SetWeatherNow(string cloudCover, string wind)
	{
		weatherNow.Text = $"{cloudCover}/{wind}";
	}

	public void SetWeatherAhead(string cloudCover, string wind)
	{
		weatherAhead.Text = $"{cloudCover}/{wind}";
	}

	public void SetTemp(int temp)
	{
		tempNow.Text = $"{temp.ToString()}° C";
	}

	public void SetTime(string timePhrase)
	{
		time.Text = timePhrase;
	}

	public void SetDate(string date)
	{
		year.Text = date;
	}

	public void SetSeason(string seasonPhrase)
	{
		season.Text = seasonPhrase;
	}

	public HUDDateAndWeather(int xPos, int width)
		: base(width, 72)
	{
		GUIManager gUIManager = The.InGameUI.gui;
		HideOnRightClick = false;
		DisplayWindow.X = xPos;
		DisplayWindow.Y = 4;
		DisplayWindow.Resizable = false;
		DisplayWindow.SetResizableArea(ResizeAreas.Bottom, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.Right, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.BottomRight, isResizable: true);
		DisplayWindow.ResizableBorderSize = 12;
		DisplayWindow.Resize += DisplayWindow_Resize;
		DisplayWindow.Level = Level.Bottom;
		DisplayWindow.MinHeight = 34;
		DisplayWindow.MinWidth = 60;
		DisplayWindow.MaxWidth = width;
		DisplayWindow.MaxHeight = DisplayWindow.Height;
		DisplayWindow.Show();
		surface = new UIComponent(gUIManager);
		Add(surface);
		surface.X = 0;
		surface.Y = 0;
		DisplayWindow.Height = 34;
		time = new Label(gUIManager);
		surface.Add(time);
		time.Text = "Noon";
		time.Init(Label.LabelType.HUDWindow);
		time.Position = new Point(9, 9);
		season = new Label(gUIManager);
		surface.Add(season);
		season.Text = "Start of spring";
		season.Init(Label.LabelType.HUDWindow);
		season.Position = new Point(112, time.Y);
		year = new Label(gUIManager);
		surface.Add(year);
		year.Init(Label.LabelType.HUDWindow);
		year.Position = new Point(220, time.Y);
		Bar bar = new Bar(gUIManager);
		surface.Add(bar);
		bar.EdgeSize = 1;
		bar.SetSkinLocation(SkinState.Normal, gUIManager.GUISpriteSheet.GetSourceRectangle("HUD_line_horizontal"));
		bar.Height = 1;
		bar.Width = 352;
		bar.X = 9;
		bar.Y = time.Bottom + 1;
		Label label = new Label(gUIManager);
		surface.Add(label);
		label.Text = "WEATHER NOW:";
		label.Init(Label.LabelType.HUDWindow);
		label.NormalColor = weatherNowColor;
		label.Position = new Point(9, 28);
		Label label2 = new Label(gUIManager);
		surface.Add(label2);
		label2.Text = "WEATHER AHEAD:";
		label2.Init(Label.LabelType.HUDWindow);
		label2.NormalColor = weatherLaterColor;
		label2.Position = new Point(9, 46);
		tempNow = new Label(gUIManager);
		surface.Add(tempNow);
		tempNow.Text = "17 C";
		tempNow.Init(Label.LabelType.HUDWindow);
		tempNow.Position = new Point(142, label.Y);
		tempNow.NormalColor = weatherNowColor;
		tempAhead = new Label(gUIManager);
		surface.Add(tempAhead);
		tempAhead.Text = "13 C";
		tempAhead.Init(Label.LabelType.HUDWindow);
		tempAhead.Position = new Point(tempNow.X, label2.Y);
		tempAhead.NormalColor = weatherLaterColor;
		weatherNow = new Label(gUIManager);
		surface.Add(weatherNow);
		weatherNow.Text = "Cloudy/Light breeze";
		weatherNow.Init(Label.LabelType.HUDWindow);
		weatherNow.Position = new Point(177, label.Y);
		weatherNow.NormalColor = weatherNowColor;
		weatherAhead = new Label(gUIManager);
		surface.Add(weatherAhead);
		weatherAhead.Text = "Cloudy/Windy";
		weatherAhead.Init(Label.LabelType.HUDWindow);
		weatherAhead.Position = new Point(weatherNow.X, label2.Y);
		weatherAhead.NormalColor = weatherLaterColor;
		UpdateSurface();
		Show();
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		UpdateSurface();
	}

	private void UpdateSurface()
	{
		surface.Width = DisplayWindow.Width - 6;
		surface.Height = DisplayWindow.Height - 9;
	}

	public void Show()
	{
	}

	private void closeButton_Click(UIComponent sender, EventArgs e)
	{
	}
}
