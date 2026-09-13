using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Systems;

namespace UWGame.ClientSide.Particles;

[DebuggerDisplay("{ParticleSystem.ParticleSystemType.KeyName} {activeParticles.Count}")]
public class ParticleEmitter : ISleepingUpdatable
{
	public ParticleSystem System;

	private List<Particle> activeParticles = new List<Particle>();

	private double? timePointInSeconds;

	private double? updateInterval;

	public Vector2 Offset;

	private Vector2 position;

	public float? EmitDirection;

	public float Intensity;

	public float EmitterScale = 1f;

	public float? TimeBetweenEmitting;

	public double? NextEmissionTimePoint;

	public Renderable Parent { get; private set; }

	public double? ExpiryTimePointInSeconds { get; set; }

	public double? TimePointInSeconds => timePointInSeconds;

	public double? UpdateInterval
	{
		get
		{
			return updateInterval;
		}
		private set
		{
			if (!Common.IsEqual(updateInterval, value))
			{
				updateInterval = value;
				LookUpSleepyUpdater<ParticleEmitter>.FindByID(SleepyUpdater)?.NotifyUpdateIntervalChanged(this);
			}
		}
	}

	public SleepyUpdaterID SleepyUpdater { get; set; }

	public Vector2 Position
	{
		get
		{
			if (Parent != null)
			{
				if (Parent.RenderAsModel != null)
				{
					return Parent.RenderAsModel.Location.ToVector2() + Offset;
				}
				return Parent.Location.Value.ToVector2() + Offset;
			}
			return position;
		}
		set
		{
			position = value;
		}
	}

	public bool EmitInRenderablesFacingDirection { get; private set; }

	public float? ParentEmitDirection
	{
		get
		{
			if (Parent != null)
			{
				if (Parent.Entity != null)
				{
					return Parent.Entity.FacingAngleWithRotator;
				}
				return null;
			}
			return null;
		}
	}

	private bool EmitsParticlesContinually
	{
		get
		{
			if (!System.ParticleSystemType.ParticlesNeverExpire)
			{
				return !System.ParticleSystemType.EmitParticlesManually;
			}
			return false;
		}
	}

	public void SetNextTimepoint(double? timepoint)
	{
		timePointInSeconds = timepoint;
	}

	void ISleepingUpdatable.CreateSleepyLookupCollection()
	{
	}

	public static void CreateSleepyLookupCollection()
	{
		LookUpSleepyUpdater<ParticleEmitter>.Create();
	}

	public ParticleEmitter(ParticleSystem system, Renderable parent, Vector2 worldPosition, Vector2 offset, float? timeBetweenEmitting, bool emitParticlesInParentsDirection)
	{
		System = system;
		Offset = offset;
		Position = worldPosition + Offset;
		TimeBetweenEmitting = timeBetweenEmitting;
		EmitInRenderablesFacingDirection = emitParticlesInParentsDirection;
		Parent = parent;
		if (!System.ParticleSystemType.EmitParticlesManually)
		{
			AddParticles(EmitterScale, Position);
		}
		SetNextEmissionTimepoint();
		RecomputeUpdateInterval();
	}

	public void AddParticles(float scale, Vector2 where, float lifetimeFactor = 1f)
	{
		int num = The.Client.ClientRandomGenerator.Next(System.ParticleSystemType.MinNumParticles, System.ParticleSystemType.MaxNumParticles + 1, "ParticleEmitter", saveMessage: false);
		for (int i = 0; i < num; i++)
		{
			if (System.FreeParticles.Count <= 0)
			{
				break;
			}
			Particle particle = System.FreeParticles.Dequeue();
			activeParticles.Add(particle);
			float? emitterMeanDirectionInRadians = ((!EmitInRenderablesFacingDirection) ? EmitDirection : new float?(ParentEmitDirection.Value));
			System.InitializeParticle(particle, scale, where, lifetimeFactor, emitterMeanDirectionInRadians, null);
		}
	}

	public void UpdateParticlesInParallel(GameTime gameTime)
	{
		foreach (Particle activeParticle in activeParticles)
		{
			activeParticle.Update(gameTime);
		}
	}

	public void Update(GameTime gameTime, out bool wasDestroyed)
	{
		UpdateEmission();
		UpdateParticleExpiry(out wasDestroyed);
		if (!wasDestroyed)
		{
			UpdateExpiry(out wasDestroyed);
		}
	}

	private void UpdateParticleExpiry(out bool wasDestroyed)
	{
		wasDestroyed = false;
		if (System.ParticleSystemType.ParticlesNeverExpire)
		{
			return;
		}
		for (int num = activeParticles.Count - 1; num >= 0; num--)
		{
			Particle particle = activeParticles[num];
			if (!particle.IsActive)
			{
				activeParticles.RemoveAt(num);
				System.FreeParticles.Enqueue(particle);
			}
		}
		if (activeParticles.Count == 0 && System.ParticleSystemType.RemoveEmittersAfterLastParticleExpires)
		{
			Destroy();
			wasDestroyed = true;
		}
	}

	private void UpdateEmission()
	{
		if (EmitsParticlesContinually && NextEmissionTimePoint.HasValue && The.Sim.TimepointReached(NextEmissionTimePoint.Value))
		{
			AddParticles(EmitterScale, Position);
			SetNextEmissionTimepoint();
		}
	}

	private void SetNextEmissionTimepoint()
	{
		if (!System.ParticleSystemType.EmitParticlesManually && TimeBetweenEmitting.HasValue)
		{
			NextEmissionTimePoint = UpdateTimePoints.ComputeTimePointFromInterval(TimeBetweenEmitting.Value);
		}
		else
		{
			NextEmissionTimePoint = null;
		}
	}

	private void UpdateExpiry(out bool wasDestroyed)
	{
		wasDestroyed = false;
		if (ExpiryTimePointInSeconds.HasValue && The.Sim.TimepointReached(ExpiryTimePointInSeconds.Value))
		{
			Destroy();
			wasDestroyed = true;
			ExpiryTimePointInSeconds = null;
		}
	}

	private void RecomputeUpdateInterval()
	{
		RecomputeUpdateInterval(out var _);
	}

	public void RecomputeUpdateInterval(out bool intervalWasChanged)
	{
		intervalWasChanged = false;
		double? currentInterval = null;
		UpdateTimePoints.GetSoonestInterval(GetParticleExpiryUpdateInterval(), ref currentInterval);
		UpdateTimePoints.GetSoonestInterval(GetEmissionUpdateInterval(), ref currentInterval);
		UpdateTimePoints.GetSoonestInterval(GetEmitterExpiryUpdateInterval(), ref currentInterval);
		if (!Common.IsEqual(UpdateInterval, currentInterval))
		{
			UpdateInterval = currentInterval;
			intervalWasChanged = true;
		}
	}

	private double? GetParticleExpiryUpdateInterval()
	{
		if (System.ParticleSystemType.ParticlesNeverExpire)
		{
			return null;
		}
		return 0.0;
	}

	private double? GetEmitterExpiryUpdateInterval()
	{
		return UpdateTimePoints.ComputeIntervalFromTimepoint(ExpiryTimePointInSeconds);
	}

	private double? GetEmissionUpdateInterval()
	{
		if (System.ParticleSystemType.ParticlesNeverExpire || System.ParticleSystemType.EmitParticlesManually)
		{
			return null;
		}
		return UpdateTimePoints.ComputeIntervalFromTimepoint(NextEmissionTimePoint);
	}

	public void Destroy()
	{
		System.RemoveEmitter(this);
		for (int num = activeParticles.Count - 1; num >= 0; num--)
		{
			Particle item = activeParticles[num];
			activeParticles.RemoveAt(num);
			System.FreeParticles.Enqueue(item);
		}
	}

	public void Draw(Color tint)
	{
		if (The.Client.spriteBatch == null)
		{
			return;
		}
		The.Client.spriteBatch.Begin(SpriteSortMode.Deferred, System.ParticleSystemType.spriteBlendState);
		Vector2 mapWindowWorldPosition = The.MapUI.MapWindowWorldPosition;
		foreach (Particle activeParticle in activeParticles)
		{
			Vector2 vector = activeParticle.Position - mapWindowWorldPosition;
			float scale;
			Color color;
			if (activeParticle.Lifetime.HasValue)
			{
				float num = activeParticle.TimeSinceStart / activeParticle.Lifetime.Value;
				float num2 = 4f * num * (1f - num);
				color = System.ParticleSystemType.StartColor * num2;
				scale = activeParticle.Scale * (0.75f + 0.25f * num);
			}
			else
			{
				float num2 = 1f;
				color = System.ParticleSystemType.StartColor;
				scale = 1f;
			}
			Rectangle? sourceRectangle;
			Texture2D texture;
			Vector2 origin;
			if (activeParticle.Player == null)
			{
				sourceRectangle = null;
				texture = System.ParticleSystemType.Texture;
				origin = System.ParticleSystemType.Origin;
			}
			else
			{
				sourceRectangle = activeParticle.Player.GetFrame();
				texture = activeParticle.Player.Animation.Texture;
				color = activeParticle.Player.GetCurrentColor(null);
				origin = activeParticle.Player.Animation.Origin;
			}
			color = new Color(color.ToVector4() * tint.ToVector4());
			The.Client.spriteBatch.Draw(texture, vector, sourceRectangle, color, activeParticle.Rotation, origin, scale, SpriteEffects.None, 0f);
		}
		The.Client.spriteBatch.End();
	}
}
