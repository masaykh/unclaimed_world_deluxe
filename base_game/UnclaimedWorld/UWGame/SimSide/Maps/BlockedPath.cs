using System;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Maps;

public struct BlockedPath
{
	public Vector3 From;

	public Vector3 To;

	public SurfaceType.TransportType Transport;

	public DateTime Timestamp;

	public BlockedPath(SurfaceType.TransportType transportType, Vector3 f, Vector3 t, DateTime time)
	{
		From = f;
		To = t;
		Timestamp = time;
		Transport = transportType;
	}
}
