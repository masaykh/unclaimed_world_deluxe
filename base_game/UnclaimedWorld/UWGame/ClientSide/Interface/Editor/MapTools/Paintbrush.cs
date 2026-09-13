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
		List<SubTileAndChange> list = new List<SubTileAndChange>();
		float value = RadiusSetting.Value;
		float value2 = AlphaSetting.Value;
		GetSubtileBounds(tilePos, (int)(value / 16f), out var minX, out var minY, out var maxX, out var maxY);
		float num = value - HardCenterRadius;
		for (ushort num2 = (ushort)minX; num2 < maxX; num2++)
		{
			for (ushort num3 = (ushort)minY; num3 < maxY; num3++)
			{
				SubtilePos subtilePos = new SubtilePos(num2, num3);
				float num4 = Common.Distance(MapManager.SubTileToWorldPos3(tilePos), MapManager.SubTileToWorldPos3(subtilePos));
				if (num4 <= value)
				{
					float num5;
					if (num4 > HardCenterRadius)
					{
						num5 = (num4 - HardCenterRadius) / num;
						num5 *= value2;
						num5 = Common.Clamp(num5, 0f, 1f);
					}
					else
					{
						num5 = value2;
					}
					list.Add(new SubTileAndChange(subtilePos, num5));
				}
			}
		}
		return list;
	}
}
