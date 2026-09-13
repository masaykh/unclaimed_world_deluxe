using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class TalkPanel
{
	public HUDTalkPanel HUDTalkPanel;

	private Window plasticPanel;

	private Window crtWindow;

	private Image faceImage;

	private CRTAnimator crtAnimator;

	private UIComponent crtContent;

	private const double timeToShowFaceMean = 6.0;

	private double timeLeftToShowFace;

	public TalkPanel()
	{
		int num = 248;
		int num2 = 13;
		int num3 = 160;
		plasticPanel = new Window(The.InGameUI.gui);
		plasticPanel.Skin = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("twitterpanel_frame");
		plasticPanel.CornerSize = 31;
		plasticPanel.Margin = 0;
		plasticPanel.IsMovable = false;
		plasticPanel.Resizable = false;
		plasticPanel.HasCloseButton = false;
		plasticPanel.Position = new Point(0, The.Client.ScreenHeight / 2 - num3 - 50);
		plasticPanel.WindowSize = new Vector2(32f, 370f);
		plasticPanel.Level = Level.Bottom;
		plasticPanel.Show();
		crtWindow = new Window(The.InGameUI.gui);
		crtWindow.CornerSize = 7;
		crtWindow.Margin = 0;
		crtWindow.IsMovable = false;
		crtWindow.Resizable = false;
		crtWindow.HasCloseButton = false;
		crtWindow.Position = new Point(num2 + 3, plasticPanel.Y + 28);
		crtWindow.WindowSize = new Vector2(num - 6, num3);
		crtWindow.Level = Level.Bottom;
		crtWindow.HasCRTOrLCDComponents = true;
		crtWindow.Show();
		HUDTalkPanel = new HUDTalkPanel(num2, plasticPanel.Y + 184, num, num3);
		crtAnimator = new CRTAnimator(crtWindow, crtWindow.AbsolutePosition, Point.Zero, crtWindow.Width, crtWindow.Height, ReflectionToUse.None, 0.8f);
		_ = The.InGameUI.gui;
		crtContent = new UIComponent(The.InGameUI.gui);
		crtContent.Width = crtAnimator.SurfacePanel.Width;
		crtContent.Height = crtAnimator.SurfacePanel.Height;
		crtContent.RenderType = RenderType.CRTAndLCD;
		faceImage = new Image(The.InGameUI.gui);
		faceImage.Texture = The.InGameUI.gui.GUI_CRT_SpriteSheet.Texture;
		faceImage.RenderType = RenderType.CRTAndLCD;
		faceImage.ScaleImageToSizeOfControl = true;
		faceImage.Width = num;
		faceImage.Height = num3;
		faceImage.X = 0;
		faceImage.Y = 0;
		crtAnimator.ChangeContent(crtContent);
	}

	public void Update(GameTime gameTime)
	{
		crtAnimator.Update(gameTime);
		if (timeLeftToShowFace > 0.0)
		{
			timeLeftToShowFace -= gameTime.ElapsedGameTime.TotalSeconds;
			if (timeLeftToShowFace <= 0.0)
			{
				timeLeftToShowFace = 0.0;
				crtAnimator.Switch();
				crtContent.Remove(faceImage);
			}
		}
	}

	public void ShowSpeaker(TalkEvent talkEvent)
	{
		Entity entity = Entity.FindByID(talkEvent.SpokenBy);
		if (entity != null)
		{
			crtAnimator.Switch();
			faceImage.SetSkinLocation(SkinState.Normal, entity.PersonEntity.GetPortraitForTalkDisplay(The.InGameUI.gui), null, null, flipHorizontally: true);
			crtContent.Add(faceImage);
			timeLeftToShowFace = The.Client.ClientRandomGenerator.RandomNormalDistribution(6.0, 0.4);
		}
	}

	public void Hide()
	{
		plasticPanel.Hide();
		HUDTalkPanel.Hide();
		crtWindow.Hide();
	}
}
