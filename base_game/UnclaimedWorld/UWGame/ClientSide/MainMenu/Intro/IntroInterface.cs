using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using WindowSystem;

namespace UWGame.ClientSide.MainMenu.Intro;

public class IntroInterface : CommonInterface
{
	public FramedCRT framedCRT;

	private new int framedCRTWidth = 666;

	private new int framedCRTHeight = 500;

	private int frameLeft;

	private int frameTop;

	private UIComponent crtContent;

	private TextArea taText;

	public IntroInterface(UnclaimedWorld game)
		: base(game)
	{
		frameLeft = (game.GraphicsDeviceManager.PreferredBackBufferWidth - framedCRTWidth) / 2;
		frameTop = (game.GraphicsDeviceManager.PreferredBackBufferHeight - framedCRTHeight) / 2;
		framedCRT = new FramedCRT(source: new Rectangle(frameLeft, frameTop, framedCRTWidth, framedCRTHeight), intf: this, dimension: new Rectangle(frameLeft, frameTop, framedCRTWidth, framedCRTHeight), level: Level.Middle);
		framedCRT.crtTextAnimatorCharacter.TimeBetweenUpdates = 0.2f;
		crtContent = framedCRT.GetNewSurfaceContent();
		taText = new TextArea(gui, ListBoxType.Main);
		crtContent.Add(taText);
		taText.Position = new Point(16, 80);
		taText.Width = crtContent.Width;
		taText.Height = crtContent.Height - taText.Position.Y;
		taText.HMargin = 0;
		taText.AnimateOnCRTScreen = Label.AnimationMode.Line;
		taText.Text = "entity.EntityType.Description.ToUpper() entity.EntityType.Description.ToUpper() entity.EntityType.Description.ToUpper() entity.EntityType.Description.ToUpper()";
	}

	public void StartMovie()
	{
		framedCRT.ChangeContent(crtContent);
	}
}
