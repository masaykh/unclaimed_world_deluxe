using System.Collections.Generic;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.Editor.MapTools;

public class Eraser : MapTool, IHasRadiusOption, IHasAlphaOption
{
	public RadiusSetting RadiusSetting { get; private set; }

	public AlphaSetting AlphaSetting { get; private set; }

	public Eraser()
	{
		RadiusSetting = new RadiusSetting(60f);
		AlphaSetting = new AlphaSetting(60f);
	}

	public override List<SubTileAndChange> GetAffectedSubtiles(SubtilePos tilePos)
	{
		List<SubTileAndChange> list = new List<SubTileAndChange>();
		float value = RadiusSetting.Value;
		float value2 = AlphaSetting.Value;
		GetSubtileBounds(tilePos, (int)(value / 16f), out var minX, out var minY, out var maxX, out var maxY);
		for (ushort num = (ushort)minX; num < maxX; num++)
		{
			for (ushort num2 = (ushort)minY; num2 < maxY; num2++)
			{
				SubtilePos subtilePos = new SubtilePos(num, num2);
				if (Common.Distance(MapManager.SubTileToWorldPos3(tilePos), MapManager.SubTileToWorldPos3(subtilePos)) <= value)
				{
					float change = 0f - value2;
					list.Add(new SubTileAndChange(subtilePos, change));
				}
			}
		}
		return list;
	}
}
