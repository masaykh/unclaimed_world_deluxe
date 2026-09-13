using InputEventSystem;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Missions;

public class ExclamationMarkInACircle : UIComponent
{
	public Icon circle;

	public Label exclamationMark;

	public override string ToolTip
	{
		get
		{
			return circle.ToolTip;
		}
		set
		{
			circle.ToolTip = value;
		}
	}

	public ExclamationMarkInACircle(GUIManager gui)
		: base(gui)
	{
		RenderType = RenderType.CRTAndLCD;
		Width = 23;
		Height = 23;
		circle = new Icon(guiManager);
		circle.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("lcd_icon_circleBG"), Color.Red, Color.Red);
		circle.Width = 22;
		circle.Height = 22;
		circle.ResizeControlToFitImage();
		Add(circle);
		circle.X = 0;
		circle.Y = 0;
		exclamationMark = new Label(guiManager);
		Add(exclamationMark);
		exclamationMark.Init(Label.LabelType.LCDError);
		exclamationMark.Text = "!";
		exclamationMark.X = 7;
		exclamationMark.Y = 1;
		exclamationMark.FitToText();
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		circle.SetMouseOutState();
		base.OnMouseOut(sender, args);
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		circle.SetMouseOverState();
		base.OnMouseOver(sender, args);
	}
}
