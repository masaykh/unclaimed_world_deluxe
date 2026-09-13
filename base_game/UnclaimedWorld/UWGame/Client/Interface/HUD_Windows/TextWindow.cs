using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.HUD_Windows;
using WindowSystem;

namespace UWGame.Client.Interface.HUD_Windows;

public class TextWindow : HUDWindow
{
	private Label text;

	private int borderMargin = 4;

	public string Text
	{
		get
		{
			return text.Text;
		}
		set
		{
			text.Text = value;
			DisplayWindow.Width = text.Width + borderMargin * 2;
			DisplayWindow.CenterChildHorizontally(text);
		}
	}

	public TextWindow(bool hasSurface)
		: base(0, 0, hasSurface)
	{
		text = new Label(gui);
		text.Init(Label.LabelType.HUDWindow);
		DisplayWindow.Height = text.Height + borderMargin * 2;
		DisplayWindow.CenterChildVertically(text);
		Add(text);
	}

	public override void Update(GameTime elapsed)
	{
		base.Update(elapsed);
	}

	public override void Hide()
	{
	}
}
