using UWGame.Control.Commands;

namespace UWGame.SimSide.Commands;

public class Pause : Command
{
	public override void Execute(bool giveClientFeedback)
	{
		The.Sim.PauseGame();
		if (giveClientFeedback)
		{
			The.Client.OnPause();
		}
	}
}
