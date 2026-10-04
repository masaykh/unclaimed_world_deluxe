namespace UWGame.SimSide.Entities.Containers;

/// <summary>
/// Any container that agents can enter must implement this.
/// </summary>
internal interface IGarrison
{
	int GetNoOfAgentsInside();
}
