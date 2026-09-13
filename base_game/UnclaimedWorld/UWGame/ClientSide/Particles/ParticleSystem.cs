using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Systems;
using WindowSystem;

namespace UWGame.ClientSide.Particles;

[DebuggerDisplay("{ParticleSystemType.KeyName} {AllEmitters.Count}")]
public class ParticleSystem
{
	public ParticleSystemType ParticleSystemType;

	private SleepyUpdater<ParticleEmitter> AllEmitters = new SleepyUpdater<ParticleEmitter>(Module.Client);

	private List<ParticleEmitter> allEmittersForParticleUpdates = new List<ParticleEmitter>();

	public Queue<Particle> FreeParticles;

	public int FreeParticleCount => FreeParticles.Count;

	public ParticleSystem(ParticleSystemType type)
	{
		ParticleSystemType = type;
	}

	public void RemoveEmitter(ParticleEmitter emitter)
	{
		emitter.Parent.RemoveParticleEmitter(emitter);
		AllEmitters.Remove(emitter);
		allEmittersForParticleUpdates.Remove(emitter);
	}

	protected Vector2 PickRandomDirection(float? meanDirectionInRadiansToUse, float? maxDirectionDifferenceInRadiansToUse)
	{
		float num;
		if (meanDirectionInRadiansToUse.HasValue)
		{
			num = meanDirectionInRadiansToUse.Value;
			if (maxDirectionDifferenceInRadiansToUse.HasValue)
			{
				num += maxDirectionDifferenceInRadiansToUse.Value - 2f * The.Client.ClientRandomGenerator.RandomBetween(0f, maxDirectionDifferenceInRadiansToUse.Value);
			}
		}
		else
		{
			num = 0f;
		}
		return Common.AngleToVector(num);
	}

	public void Initialize()
	{
		int num = ParticleSystemType.NoOfEmitters * ParticleSystemType.MaxNumParticles;
		FreeParticles = new Queue<Particle>(num);
		for (int i = 0; i < num; i++)
		{
			Particle particle = new Particle();
			if (ParticleSystemType.Animation != null)
			{
				particle.Animation2D = ParticleSystemType.Animation;
				particle.Player = new Animation2DPlayer();
			}
			FreeParticles.Enqueue(particle);
		}
	}

	public void InitializeParticle(Particle p, float scale, Vector2 where, float lifetimeFactor, float? emitterMeanDirectionInRadians, float? emitterMaxDirectionDifferenceInRadians)
	{
		float? meanDirectionInRadiansToUse = emitterMeanDirectionInRadians ?? ParticleSystemType.MeanDirectionInRadians;
		float? maxDirectionDifferenceInRadiansToUse = emitterMaxDirectionDifferenceInRadians ?? ParticleSystemType.MaxDirectionDifferenceInRadians;
		Vector2 vector = PickRandomDirection(meanDirectionInRadiansToUse, maxDirectionDifferenceInRadiansToUse);
		float num = ParticleSystemType.SystemScale * scale;
		float num2 = num * The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.minInitialSpeed, ParticleSystemType.maxInitialSpeed);
		float? lifetime = ((!ParticleSystemType.ParticlesNeverExpire) ? new float?(lifetimeFactor * The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.minLifetime, ParticleSystemType.maxLifetime)) : ((float?)null));
		float num3 = ((ParticleSystemType.ParticlesNeverExpire || !ParticleSystemType.DecelerateParticlesToZeroBeforeTheyDie) ? (num * The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.minAcceleration, ParticleSystemType.maxAcceleration)) : ((0f - num2) / lifetime.Value));
		float scale2 = num * The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.minScale, ParticleSystemType.maxScale);
		float rotationSpeed = The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.minRotationSpeed, ParticleSystemType.maxRotationSpeed);
		float rotation = 0f;
		if (ParticleSystemType.rotateRandomlyAtStart)
		{
			rotation = The.Client.ClientRandomGenerator.RandomBetween(0f, (float)Math.PI * 2f);
		}
		p.Initialize(where, rotation, vector * num2, num3 * vector, lifetime, scale2, rotationSpeed);
		if (ParticleSystemType.IsAffectedByWind)
		{
			p.Acceleration += The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.MinWindAccelerationFactor, ParticleSystemType.MaxWindAccelerationFactor) * The.Sim.PlaySite.PlaySite.Weather.WindSpeed * The.Sim.PlaySite.PlaySite.Weather.WindDirection;
		}
	}

	public void AddDust(Entity vehicle, Vector2 worldPosition, float intensity, float scale)
	{
		ParticleEmitter particleEmitter = vehicle.Renderable.ParticleEmitters.Find((ParticleEmitter e) => e.System.ParticleSystemType.KeyName == "dust");
		if (particleEmitter == null)
		{
			particleEmitter = AddEmitter(vehicle.Renderable, scale, null, Vector2.Zero);
		}
		particleEmitter.Intensity = intensity;
		if (!particleEmitter.NextEmissionTimePoint.HasValue || The.Sim.TimepointReached(particleEmitter.NextEmissionTimePoint.Value))
		{
			particleEmitter.AddParticles(scale, worldPosition, 0.3f + 1.4f * particleEmitter.Intensity);
			particleEmitter.NextEmissionTimePoint = UpdateTimePoints.ComputeTimePointFromInterval(particleEmitter.TimeBetweenEmitting.Value);
		}
	}

	public ParticleEmitter AddEmitter(Renderable renderable, float? scale, float? timeBetweenEmission, Vector2 offset, bool emitParticlesInParentsDirection = false)
	{
		Vector2 location = renderable.Location.Value.ToVector2();
		return AddEmitter(renderable, location, offset, scale, timeBetweenEmission, emitParticlesInParentsDirection);
	}

	public ParticleEmitter AddEmitter(Renderable renderable, Vector2 location, Vector2 offset, float? scale, float? timeBetweenEmission, bool emitParticlesInParentsDirection = false)
	{
		ParticleEmitter particleEmitter = new ParticleEmitter(this, renderable, location, offset, timeBetweenEmission ?? ParticleSystemType.TimeBetweenEmitting, emitParticlesInParentsDirection);
		if (scale.HasValue)
		{
			particleEmitter.EmitterScale = scale.Value;
		}
		AllEmitters.Add(particleEmitter);
		allEmittersForParticleUpdates.Add(particleEmitter);
		return particleEmitter;
	}

	public void Update(GameTime gameTime)
	{
		Parallel.ForEach(allEmittersForParticleUpdates, delegate(ParticleEmitter e)
		{
			e.UpdateParticlesInParallel(gameTime);
		});
		AllEmitters.Update(gameTime);
	}
}
