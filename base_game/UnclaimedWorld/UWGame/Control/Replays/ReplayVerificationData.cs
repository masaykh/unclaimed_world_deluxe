using System.IO;
using Microsoft.Xna.Framework;

namespace UWGame.Control.Replays;

public class ReplayVerificationData
{
	public Vector3 RepresentativeEntityLocation;

	public Vector2 MapWindowLocation;

	public void Load(BinaryFileReader replayReader)
	{
		RepresentativeEntityLocation.X = replayReader.ReadFloat();
		RepresentativeEntityLocation.Y = replayReader.ReadFloat();
		MapWindowLocation.X = replayReader.ReadFloat();
		MapWindowLocation.Y = replayReader.ReadFloat();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(RepresentativeEntityLocation.X);
		writer.Write(RepresentativeEntityLocation.Y);
		writer.Write(MapWindowLocation.X);
		writer.Write(MapWindowLocation.Y);
	}

	public bool Verify(ReplayVerificationData compareTo, Replayer.ReplayingMode mode)
	{
		Vector2 mapWindowLocation = compareTo.MapWindowLocation;
		Vector3 representativeEntityLocation = compareTo.RepresentativeEntityLocation;
		if (!Common.IsEqual(representativeEntityLocation.X, RepresentativeEntityLocation.X))
		{
			return false;
		}
		if (!Common.IsEqual(representativeEntityLocation.Y, RepresentativeEntityLocation.Y))
		{
			return false;
		}
		if (mode != Replayer.ReplayingMode.CommandMode)
		{
			if (!Common.IsEqual(mapWindowLocation.X, MapWindowLocation.X))
			{
				return false;
			}
			if (!Common.IsEqual(mapWindowLocation.Y, MapWindowLocation.Y))
			{
				return false;
			}
		}
		return true;
	}
}
