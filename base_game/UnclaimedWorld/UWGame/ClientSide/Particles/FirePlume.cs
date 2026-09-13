using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Particles;

public class FirePlume
{
	public Vector2 Position;

	public float Intensity;

	public float Scale = 1f;

	public const float TimeBetweenFlames = 2f;

	public float timeTillPuff;

	public void Initialize()
	{
		timeTillPuff = 0f;
	}
}
