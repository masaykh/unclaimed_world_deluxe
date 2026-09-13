using System.IO;

namespace UWGame.SimSide.Scenarios;

public class StartGameEditorParams
{
	public string MapToLoadPath;

	public Sim.EngineMode EngineMode;

	public override string ToString()
	{
		return Path.GetDirectoryName(MapToLoadPath);
	}
}
