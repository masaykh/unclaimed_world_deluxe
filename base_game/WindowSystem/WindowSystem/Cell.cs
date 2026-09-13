using Microsoft.Xna.Framework;

namespace WindowSystem;

public struct Cell
{
	public Rectangle? Frame;

	public Color Color;

	public Cell(Rectangle frame)
	{
		Frame = frame;
		Color = Color.White;
	}
}
