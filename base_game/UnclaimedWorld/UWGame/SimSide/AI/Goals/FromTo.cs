using Microsoft.Xna.Framework;

namespace UWGame.SimSide.AI.Goals;

internal struct FromTo
{
	public Vector3 From;

	public Vector3 To;

	public FromTo(Vector3 f, Vector3 to)
	{
		From = f;
		To = to;
	}
}
