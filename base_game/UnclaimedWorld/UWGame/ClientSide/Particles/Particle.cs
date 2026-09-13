using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Particles;

public class Particle
{
	public Vector2 Position;

	public Vector2 Velocity;

	public Vector2 Acceleration;

	private float? lifetime;

	private float timeSinceStart;

	private float scale;

	private float rotation;

	private float rotationSpeed;

	public Animation2DPlayer Player;

	public Animation2D Animation2D;

	public float? Lifetime
	{
		get
		{
			return lifetime;
		}
		set
		{
			lifetime = value;
		}
	}

	public float TimeSinceStart
	{
		get
		{
			return timeSinceStart;
		}
		set
		{
			timeSinceStart = value;
		}
	}

	public float Scale
	{
		get
		{
			return scale;
		}
		set
		{
			scale = value;
		}
	}

	public float Rotation
	{
		get
		{
			return rotation;
		}
		set
		{
			rotation = value;
		}
	}

	public float RotationSpeed
	{
		get
		{
			return rotationSpeed;
		}
		set
		{
			rotationSpeed = value;
		}
	}

	public bool IsActive
	{
		get
		{
			if (Lifetime.HasValue)
			{
				return TimeSinceStart < Lifetime;
			}
			return true;
		}
	}

	public void Initialize(Vector2 position, float rotation, Vector2 velocity, Vector2 acceleration, float? lifetime, float scale, float rotationSpeed)
	{
		Position = position;
		Velocity = velocity;
		Acceleration = acceleration;
		Lifetime = lifetime;
		Scale = scale;
		RotationSpeed = rotationSpeed;
		TimeSinceStart = 0f;
		Rotation = rotation;
		if (Player != null && Animation2D != null)
		{
			Player.StartAnimation(Animation2D);
		}
	}

	public void Update(GameTime gameTime)
	{
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		Velocity += Acceleration * num;
		Position += Velocity * num;
		Rotation += RotationSpeed * num;
		TimeSinceStart += num;
		if (Player != null)
		{
			Player.Update(gameTime);
		}
	}
}
