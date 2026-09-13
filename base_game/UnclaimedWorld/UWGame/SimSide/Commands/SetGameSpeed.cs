using UWGame.Control.Commands;

namespace UWGame.SimSide.Commands;

public class SetGameSpeed : Command
{
	public Speeds Speed;

	public SetGameSpeed()
	{
	}

	public SetGameSpeed(Speeds speed)
	{
		Speed = speed;
	}

	public override void Execute(bool giveClientFeedback)
	{
		The.Sim.SetGameSpeed(Speed);
		if (giveClientFeedback)
		{
			The.Client.OnSetSpeed(Speed);
		}
	}
}
