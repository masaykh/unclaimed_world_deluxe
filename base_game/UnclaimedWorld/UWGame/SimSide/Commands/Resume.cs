using UWGame.Control.Commands;

namespace UWGame.SimSide.Commands;

public class Resume : Command
{
	public override void Execute(bool giveClientFeedback)
	{
		The.Sim.ResumeGame();
		if (giveClientFeedback)
		{
			The.Client.OnResume();
		}
	}
}
