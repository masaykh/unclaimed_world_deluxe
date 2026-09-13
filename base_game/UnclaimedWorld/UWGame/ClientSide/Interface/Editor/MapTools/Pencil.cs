using System.Collections.Generic;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.Editor.MapTools;

public class Pencil : MapTool, IHasAlphaOption
{
	public AlphaSetting AlphaSetting { get; private set; }

	public Pencil()
	{
		AlphaSetting = new AlphaSetting(1f);
	}

	public override List<SubTileAndChange> GetAffectedSubtiles(SubtilePos tilePos)
	{
		List<SubTileAndChange> list = new List<SubTileAndChange>();
		float value = AlphaSetting.Value;
		list.Add(new SubTileAndChange(tilePos, value));
		return list;
	}
}
