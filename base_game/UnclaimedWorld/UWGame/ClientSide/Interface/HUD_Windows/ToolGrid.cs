using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class ToolGrid
{
	public UIComponent HeaderRow;

	public Grid Grid;

	public int ScrollPosition;

	public ImageButton btScrollDown;

	public ImageButton btScrollUp;

	public bool ScrollButtonsAreShown = true;
}
