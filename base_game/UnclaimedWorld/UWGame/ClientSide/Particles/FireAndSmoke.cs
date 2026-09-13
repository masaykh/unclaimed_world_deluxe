using UWGame.ClientSide.Map;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Particles;

public class FireAndSmoke
{
	public FirePlume FirePlume;

	public ParticleEmitter SmokePlume;

	public LightSource LightSource;

	public Entity AttachedToEntity;

	public float Size
	{
		set
		{
			FirePlume.Scale = value;
			FirePlume.Intensity = value;
			SmokePlume.EmitterScale = value;
		}
	}

	public float SmokeAmount
	{
		set
		{
			SmokePlume.Intensity = value;
		}
	}

	public void Remove()
	{
	}
}
