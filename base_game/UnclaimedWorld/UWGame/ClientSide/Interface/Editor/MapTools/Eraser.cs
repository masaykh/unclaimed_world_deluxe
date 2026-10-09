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
		// PORT: was AlphaSetting(60f). ALPHA is 0..1 (AlphaOption: slider / 100), so the Eraser
		// started 60 times stronger than its own slider can go, and dropped to at most 1 the first
		// time the slider was touched. It starts at the slider's full strength, like the Brush.
		AlphaSetting = new AlphaSetting(1f);
	}

	public override List<SubTileAndChange> GetAffectedSubtiles(SubtilePos tilePos)
	{
		// PORT: was a hard-edged disc of -alpha. It takes away with the Paintbrush's soft edge
		// (MapTool.SoftDisc).
		return SoftDisc(tilePos, RadiusSetting.Value, 0f, 0f - AlphaSetting.Value);
	}
}
