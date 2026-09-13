using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Renderables;

public class Envelope<T>
{
	private float duration;

	private T value;

	public Envelope(float duration, T value)
	{
		this.duration = duration;
		this.value = value;
	}

	public bool Update(GameTime gameTime)
	{
		duration -= gameTime.ElapsedGameTime.Milliseconds;
		if (duration < 0f)
		{
			return false;
		}
		return true;
	}

	public T GetValue()
	{
		return value;
	}
}
