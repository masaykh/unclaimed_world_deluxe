using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Systems;

namespace UWGame.ClientSide.Particles;

public class ParticleManager
{
	private RenderableType emitterType;

	public Dictionary<ParticleSystemType, ParticleSystem> ParticleSystems = new Dictionary<ParticleSystemType, ParticleSystem>();

	public ParticleManager()
	{
		emitterType = new RenderableType
		{
			FadeOutWhenDestroyed = true
		};
	}

	public void Init()
	{
		ParticleSystems = new Dictionary<ParticleSystemType, ParticleSystem>();
		foreach (KeyValuePair<string, ParticleSystemType> allParticleSystem in GameData.Instance.AllParticleSystems)
		{
			ParticleSystem particleSystem = new ParticleSystem(allParticleSystem.Value);
			ParticleSystems.Add(allParticleSystem.Value, particleSystem);
			particleSystem.Initialize();
		}
	}

	public void AddEmitter(string systemKey, Vector2 worldPosition, float? scale = null, float? timeBetweenPuffs = null, double? durationInSeconds = null, Vector2? offset = null)
	{
		Renderable renderable = RenderableFactory.Produce(null, emitterType, null, durationInSeconds);
		renderable.Location = worldPosition.ToVector3();
		ParticleEmitter emitter = ParticleSystems[GameData.Instance.AllParticleSystems[systemKey]].AddEmitter(renderable, worldPosition, offset ?? Vector2.Zero, scale, timeBetweenPuffs);
		renderable.AddParticleEmitter(emitter);
		Point pos = MapManager.WorldPosToTile(worldPosition);
		The.Map.GetTile(pos).AddRenderable(renderable);
	}

	public void AddEmitter(string systemKey, Renderable renderable, float? scale = null, float? timeBetweenPuffs = null, bool emitParticlesInParentsDirection = false, double? lifetimeInSeconds = null, Vector2? offset = null)
	{
		ParticleEmitter particleEmitter = ParticleSystems[GameData.Instance.AllParticleSystems[systemKey]].AddEmitter(renderable, scale, timeBetweenPuffs, offset ?? Vector2.Zero, emitParticlesInParentsDirection);
		if (lifetimeInSeconds.HasValue)
		{
			particleEmitter.ExpiryTimePointInSeconds = UpdateTimePoints.ComputeTimePointFromInterval(lifetimeInSeconds.Value);
		}
		renderable.AddParticleEmitter(particleEmitter);
	}

	public void RemoveEmitter(ParticleEmitter emitter)
	{
		ParticleSystems[emitter.System.ParticleSystemType].RemoveEmitter(emitter);
	}

	public void AddDust(Entity vehicle, Vector3 worldPosition, float intensity, float scale)
	{
		if (The.MapUI.WorldPositionIsOnScreen(worldPosition, 10f))
		{
			ParticleSystems[GameData.Instance.AllParticleSystems["dust"]].AddDust(vehicle, new Vector2(worldPosition.X, worldPosition.Y), intensity, scale);
		}
	}

	public void Update()
	{
		foreach (KeyValuePair<ParticleSystemType, ParticleSystem> particleSystem in ParticleSystems)
		{
			particleSystem.Value.Update(The.Sim.GameTime);
		}
	}
}
