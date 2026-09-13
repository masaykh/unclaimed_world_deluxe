using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using WindowSystem;

namespace UWGame.ClientSide.Screens;

public class IngameLoadGameInterface : CommonInterface
{
	public SaveLoadMessageBox SaveLoadMessageBox;

	public IngameLoadGameInterface(IngameLoadGameScreen screen, UnclaimedWorld game)
		: base(game)
	{
	}

	public override void LoadContent()
	{
		base.LoadContent();
		SaveLoadMessageBox = new SaveLoadMessageBox(this);
		SaveLoadMessageBox.ShowMessage("Loading game, please wait...", null, modal: true, UWGame.ClientSide.Interface.MessageBox.ButtonOptions.None);
		SetInterfaceCursor();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (SaveLoadMessageBox.Window.IsVisibleAndActive)
		{
			SaveLoadMessageBox.Update(gameTime);
		}
	}

	private void Form_Close(UIComponent sender)
	{
	}
}
