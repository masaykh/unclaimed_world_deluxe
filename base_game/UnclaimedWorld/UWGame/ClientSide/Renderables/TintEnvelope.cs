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

	private Vector3 m_attackRate;

	private Vector3 m_decayRate;

	private Color m_peakColor;

	private Vector3 m_currentColor;

	private uint m_sustainCounter;

	private State m_envState;

	private bool m_affect;

	private float m_vibratoAmplitude;

	private float m_vibratoFrequency;

	private Vector3 m_vibratoColor;

	public void update()
	{
	}

	public void play(Color peak, uint atackFrames = 8u, uint decayFrames = 8u, uint sustainAtPeak = 8u)
	{
	}

	public void sustain()
	{
		m_envState = State.Sustain;
	}

	public void release()
	{
		m_envState = State.Decay;
	}

	public void rest()
	{
		m_envState = State.Rest;
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
		m_sustainCounter = x;
	}

	private void setAttackFrames(uint frames)
	{
	}

	private void setDecayFrames(uint frames)
	{
	}

	private void setPeakColor(Color peak)
	{
		m_peakColor = new Color(peak.R, peak.G, peak.B);
	}

	private void setPeakColor(float r, float g, float b)
	{
		m_peakColor = new Color(r, g, b);
	}
}
