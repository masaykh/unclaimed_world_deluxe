using Microsoft.Xna.Framework;

//TODO DECOUPLE move to clientSide
namespace UWGame.SimSide.Entities;

/// <summary>
/// Use this instead of LightSourceType
/// read coords and offsets from data. Place in Renderable and use sprite state to set lights on/off
/// </summary>
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
