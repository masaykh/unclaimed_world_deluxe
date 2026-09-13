using GameStateManagement;
using UWGame.ClientSide.Interface;
using WindowSystem;

namespace UWGame.ClientSide.MainMenu.Credits;

public class CreditsInterface : CommonInterface
{
	private int loadPanelWidth = 600;

	private int totalHeight = 800;

	private int left;

	private int top;

	private CreditsPanel hud;

	public CreditsScreen creditsScreen;

	public CreditsInterface(CreditsScreen createGameScreen, UnclaimedWorld game)
		: base(game)
	{
		creditsScreen = createGameScreen;
		game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
		left = (game.Controller.DrawArea.Width - loadPanelWidth) / 2;
		top = (game.Controller.DrawArea.Height - totalHeight) / 2;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		hud = new CreditsPanel(this);
		hud.DisplayWindow.Close += DisplayWindow_Close;
		SetInterfaceCursor();
	}

	private void DisplayWindow_Close(UIComponent sender)
	{
		creditsScreen.ExitToMainMenu();
	}
}
