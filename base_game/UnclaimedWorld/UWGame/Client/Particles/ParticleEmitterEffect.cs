using Microsoft.Xna.Framework;

namespace UWGame.Client.Particles;

public class ParticleEmitterEffect
{
	public string ParticleSystemKey;

	public float Intensity = 1f;

	public bool EmitParticlesInParentDirection;

	public bool AttachToEntity = true;

	public Vector2 Offset;

	public double? DurationInSeconds;

	public void Initialize()
	{
	}
}
