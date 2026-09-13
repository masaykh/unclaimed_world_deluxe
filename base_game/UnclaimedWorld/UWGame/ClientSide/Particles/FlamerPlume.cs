using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Particles;

public class FlamerPlume
{
	public Vector2 Position;

	public float Intensity;

	public const float TimeBetweenFlamerPuffs = 0.05f;

	public float timeTillFlamerPuff;
}
