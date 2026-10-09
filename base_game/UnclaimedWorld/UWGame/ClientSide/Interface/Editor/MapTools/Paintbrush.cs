using System.Collections.Generic;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.Editor.MapTools;

public class Paintbrush : MapTool, IHasRadiusOption, IHasAlphaOption
{
	public float HardCenterRadius;

	public RadiusSetting RadiusSetting { get; private set; }

	public AlphaSetting AlphaSetting { get; private set; }

	public Paintbrush()
	{
		RadiusSetting = new RadiusSetting(60f);
		AlphaSetting = new AlphaSetting(1f);
	}

	public override List<SubTileAndChange> GetAffectedSubtiles(SubtilePos tilePos)
	{
		// PORT: the soft edge fades out towards the rim, not in (MapTool.SoftDisc).
		return SoftDisc(tilePos, RadiusSetting.Value, HardCenterRadius, Common.Clamp(AlphaSetting.Value, 0f, 1f));
	}
}
