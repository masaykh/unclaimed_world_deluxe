using Microsoft.Xna.Framework;

namespace WindowSystem;

public class TabButtonContainer
{
	public ImageButton button;

	public Label label;

	public Image markings;

	public ImageButton AddHorizontalMetalPanelButton(GUIManager gui, Window window, int markingsLeft, int markingsTop, int flavour, int markingsFlavour, string text)
	{
		markings = new Image(gui);
		window.Add(markings);
		markings.Position = new Point(markingsLeft, markingsTop);
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle($"metal_markings_{markingsFlavour}horiz");
		markings.SetSkinLocation(SkinState.Normal, sourceRectangle);
		markings.ResizeControlToFitImage();
		label = new Label(gui);
		window.Add(label);
		label.Text = text;
		label.Init(Label.LabelType.PlainPanelNormal);
		label.Position = new Point(markingsLeft + (markings.Width - label.Width) / 2, markingsTop - 5);
		int x = markingsLeft + 7;
		int y = markingsTop + 9;
		button = new ImageButton(gui);
		window.Add(button);
		button.Position = new Point(x, y);
		button.Init(ImageButtonType.MetalPanelHorizontal, flavour);
		return button;
	}

}
