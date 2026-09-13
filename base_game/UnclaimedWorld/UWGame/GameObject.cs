using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;

namespace UWGame;

public class GameObject : ILocatable, IComparable
{
	protected Vector3? location;

	public virtual Renderable AsRenderable => null;

	public virtual RenderAsBillboard AsRenderAsBillboard => null;

	Vector3 ILocatable.Location => Location.Value;

	public virtual Vector3? Location
	{
		get
		{
			return location;
		}
		set
		{
			if (value.HasValue)
			{
				location = The.Map.ClampWorldPosition(value.Value);
			}
			else
			{
				location = null;
			}
		}
	}

	public int CompareTo(object obj)
	{
		ILocatable locatable = obj as ILocatable;
		if (Location.HasValue)
		{
			return (int)(Location.Value.Y - locatable.Location.Y);
		}
		return -1;
	}
}
