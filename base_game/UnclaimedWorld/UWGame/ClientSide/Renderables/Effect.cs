using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Renderables;

public class Effect<T> : IUpdatable
{
	private List<Envelope<T>> activeEnvelopes = new List<Envelope<T>>();

	private T defaultElement;

	private Renderable parent;

	public Effect(Renderable parent, T elementAsDefault)
	{
		this.parent = parent;
		defaultElement = elementAsDefault;
	}

	public void SetDefaultValue(T element)
	{
		defaultElement = element;
	}

	public int GetNumberOfEnvelopes()
	{
		return activeEnvelopes.Count;
	}

	public T GetValue()
	{
		if (activeEnvelopes.Count > 0)
		{
			return activeEnvelopes[activeEnvelopes.Count - 1].GetValue();
		}
		return defaultElement;
	}

	public void AddNewEnvelope(float duration, T element)
	{
		Envelope<T> item = new Envelope<T>(duration, element);
		activeEnvelopes.Add(item);
	}

	public void Update(GameTime gameTime)
	{
		for (int num = activeEnvelopes.Count - 1; num >= 0; num--)
		{
			if (!activeEnvelopes[num].Update(gameTime))
			{
				activeEnvelopes.RemoveAt(num);
			}
		}
	}

	public double? GetUpdateInterval()
	{
		if (activeEnvelopes.Count > 0)
		{
			return 0.0;
		}
		if (typeof(T) == typeof(Animation2DPlayer) && parent.IsOnScreen && ((Animation2DPlayer)(object)defaultElement).RequiresUpdate)
		{
			if (!The.MapUI.TileIsOnScreen(parent.MapPosition.X, parent.MapPosition.Y, 2))
			{
				parent.IsOnScreen = false;
				return null;
			}
			return 0.0;
		}
		return null;
	}
}
