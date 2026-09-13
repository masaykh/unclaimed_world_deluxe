using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Maps;

public struct FromTo
{
	private Vector3 From;

	private Vector3 To;

	private SurfaceType.TransportType Transport;

	public FromTo(SurfaceType.TransportType transportType, Vector3 f, Vector3 t)
	{
		From = f;
		To = t;
		Transport = transportType;
	}
}
