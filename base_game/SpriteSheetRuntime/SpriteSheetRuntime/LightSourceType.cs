using Microsoft.Xna.Framework;

namespace SpriteSheetRuntime;

public class LightSourceType
{
	public string SpriteName;

	public Point Offset;

	public Point OffsetFlipped;

	public bool IsIndoor;

	public Point GetOffset(bool flipHorizontally)
	{
		if (flipHorizontally)
		{
			return OffsetFlipped;
		}
		return Offset;
	}
}
