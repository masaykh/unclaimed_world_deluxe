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

	private bool m_affect;

	public void update()
	{
	}

	public void play(Color peak, uint atackFrames = 8u, uint decayFrames = 8u, uint sustainAtPeak = 8u)
	{
	}

	public void sustain()
	{
	}

	public void release()
	{
	}

	public void rest()
	{
	}

	public bool isEffective()
	{
		return m_affect;
	}

	public void saturate(Color color)
	{
	}

	public void setVibrato(float amplitude, float frequency)
	{
	}

	public Color GetColor()
	{
		return Color.White;
	}

	public void setSustain(uint x)
	{
	}
}
