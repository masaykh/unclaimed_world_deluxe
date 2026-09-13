using System.Collections.Generic;
using UWGame.ClientSide.Interface.Editor.MapTools;

namespace UWGame.ClientSide.Interface.Editor;

public interface IEditorPanel
{
	void AffectMap(List<MapTool.SubTileAndChange> affectedSubtiles);

	void AffectMap(List<MapTool.TileAndChange> affectedTiles);
}
