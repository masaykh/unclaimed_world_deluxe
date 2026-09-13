using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface.Tasks;

public class JobsMessage
{
	public string Text;

	private double timeElapsed;

	public void Update(GameTime gameTime)
	{
		timeElapsed += gameTime.ElapsedGameTime.TotalSeconds;
	}

	public bool IsExpired()
	{
		return timeElapsed > 5.0;
	}
}
