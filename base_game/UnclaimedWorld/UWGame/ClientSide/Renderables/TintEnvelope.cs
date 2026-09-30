using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Renderables;

public class TintEnvelope
{
	public enum State
	{
		Rest,
		Attack,
		Decay,
		Sustain
	}

	public void update()
	{
	}

	public void play(Color peak, uint atackFrames = 8u, uint decayFrames = 8u, uint sustainAtPeak = 8u)
	{
	}

	public void release()
	{
	}

	public void rest()
	{
	}

	public Color GetColor()
	{
		return Color.White;
	}

}
