using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Entities;

public class LightingType
{
	public string SpriteName;

	public Point Offset;

	public Point OffsetFlipped;

	public Point GetOffset(bool flipHorizontally)
	{
		if (flipHorizontally)
		{
			return OffsetFlipped;
		}
		return Offset;
	}
}
