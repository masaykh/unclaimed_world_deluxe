using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class TerrainPath : Component
{
	public float Value;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		sn.DoFloat(Value);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public TerrainPath(Entity parent)
		: base(parent)
	{
	}

	public TerrainPath()
	{
	}

	public Vector4 GetColor()
	{
		return new Vector4(1f, 1f, 1f, Value);
	}
}
