using System;
using InputEventSystem;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class MinimapAccessPanel
{
	public Window DisplayWindow;

	protected InGameInterface intf = The.InGameUI;
	public ImageButton btAccessMinimap;

	private InputData frameInput;

	public MinimapAccessPanel(int screenX)
	{
	}
}
