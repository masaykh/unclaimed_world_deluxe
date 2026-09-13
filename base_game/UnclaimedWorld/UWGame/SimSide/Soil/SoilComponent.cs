using System;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Soil;

public class SoilComponent : RenderedTerrainComponent
{
	public SoilComponentType SoilComponentType;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public SoilComponent(Terrain parent, SoilComponentType type)
		: base(parent)
	{
		SoilComponentType = type;
	}

	public SoilComponent()
	{
	}

	protected override float ComputeDisplayAmount()
	{
		float num = 1f;
		if (Parent.IsSubtileTerrain())
		{
			num = 9f;
		}
		if (SoilComponentType.ScaleDisplayAmountAsWithRocks)
		{
			return GetRockAlpha(num);
		}
		return Common.ClampTop(num * base.Amount, 1f);
	}

	public float GetRockAlpha(float factor)
	{
		return Common.ClampTop((float)(2.0 / (1.0 + Math.Pow(Math.E, -10.0 * (double)factor * (double)base.Amount)) - 1.0), 1f);
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		SoilComponentType = sn.DoGameData(SoilComponentType);
		return this;
	}
}
