using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide;

public interface ILocatable : IComparable
{
	Vector3 Location { get; }

	Renderable AsRenderable { get; }

	RenderAsBillboard AsRenderAsBillboard { get; }
}
