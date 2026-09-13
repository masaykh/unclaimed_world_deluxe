using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls;

public class ModalOverlay
{
	private Icon modalTintOverLCDSurface;

	private Icon modalTintOverWindow;

	private Icon modalTintOverDisplay;

	private Color color = new Color(0f, 0f, 0f, 0.6f);

	public ModalOverlay(Window Window)
	{
		modalTintOverLCDSurface = new Icon(Window.guiManager);
		modalTintOverLCDSurface.SetSkinLocation(SkinState.Normal, Window.guiManager.GUISpriteSheet.GetSourceRectangle("whiteSquare"), color, color);
		modalTintOverLCDSurface.ScaleImageToSizeOfControl = true;
		modalTintOverLCDSurface.X = 0;
		modalTintOverLCDSurface.Y = 0;
		modalTintOverLCDSurface.Width = Window.Width;
		modalTintOverLCDSurface.Height = Window.Height;
		modalTintOverLCDSurface.Visible = true;
		modalTintOverDisplay = new Icon(Window.guiManager);
		modalTintOverDisplay.SetSkinLocation(SkinState.Normal, Window.guiManager.GUISpriteSheet.GetSourceRectangle("whiteSquare"), color, color);
		modalTintOverDisplay.ScaleImageToSizeOfControl = true;
		modalTintOverDisplay.X = 0;
		modalTintOverDisplay.Y = 0;
		modalTintOverDisplay.Width = Window.Width;
		modalTintOverDisplay.Height = Window.Height;
		modalTintOverDisplay.Visible = true;
		modalTintOverWindow = new Icon(Window.guiManager);
		modalTintOverWindow.SetSkinLocation(SkinState.Normal, Window.guiManager.GUISpriteSheet.GetSourceRectangle("whiteSquare"), color, color);
		modalTintOverWindow.ScaleImageToSizeOfControl = true;
		modalTintOverWindow.X = 0;
		modalTintOverWindow.Y = 0;
		modalTintOverWindow.Width = Window.Width - 6;
		modalTintOverWindow.Height = Window.Height;
		modalTintOverWindow.Visible = true;
	}

	public void Show(Window Window, UIComponent lcdSurface, UIComponent display)
	{
		lcdSurface.Add(modalTintOverLCDSurface);
		display.Add(modalTintOverDisplay);
		Window.Add(modalTintOverWindow);
	}

	public void Remove(Window Window, UIComponent lcdSurface, UIComponent display)
	{
		lcdSurface.Remove(modalTintOverLCDSurface);
		display.Remove(modalTintOverDisplay);
		Window.Remove(modalTintOverWindow);
	}
}
