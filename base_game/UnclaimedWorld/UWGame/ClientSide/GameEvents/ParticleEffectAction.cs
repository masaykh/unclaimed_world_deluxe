using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.ClientSide.GameEvents;

public class ParticleEffectAction : EventActionType
{
	public ParticleEmitterEffect[] ParticleEmitters;

	public EvalNode Location;

	public TargetObject UseLocationOfEntity;

	public float? Scale;

	public float? TimeBetweenEmissions;

	public double? DurationInSeconds;

	public ParticleEffectAction(string keyName)
		: base(keyName)
	{
	}

	public ParticleEffectAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		Entity entity = null;
		Vector2 worldPosition = Vector2.Zero;
		if (Location != null)
		{
			PropertyResult? propertyResult = Location.Evaluate(action);
			if (!propertyResult.HasValue || !propertyResult.Value.LocationResult.HasValue)
			{
				failReason = "Location did not evaluate to a result";
				return false;
			}
			worldPosition = propertyResult.Value.LocationResult.Value;
		}
		else
		{
			if (UseLocationOfEntity == null)
			{
				failReason = "Neither EntityToAttachTo or Location was specified.";
				return false;
			}
			bool flag = false;
			List<IHasExposedProperties> result = UseLocationOfEntity.GetResult(action);
			if (result != null && result.Count > 0)
			{
				entity = result[0] as Entity;
				if (entity != null)
				{
					worldPosition = entity.PlaySiteLocation.ToVector2();
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				failReason = "EntityToAttachTo did not evaluate to an Entity result";
				return false;
			}
		}
		ParticleEmitterEffect[] particleEmitters = ParticleEmitters;
		foreach (ParticleEmitterEffect particleEmitterEffect in particleEmitters)
		{
			if (entity == null || !particleEmitterEffect.AttachToEntity)
			{
				The.Client.ParticleManager.AddEmitter(particleEmitterEffect.ParticleSystemKey, worldPosition, null, null, DurationInSeconds, particleEmitterEffect.Offset);
			}
			else if (particleEmitterEffect.AttachToEntity)
			{
				The.Client.ParticleManager.AddEmitter(particleEmitterEffect.ParticleSystemKey, entity.Renderable, Scale, TimeBetweenEmissions, particleEmitterEffect.EmitParticlesInParentDirection, DurationInSeconds, particleEmitterEffect.Offset);
			}
			else
			{
				The.Client.ParticleManager.AddEmitter(particleEmitterEffect.ParticleSystemKey, worldPosition, Scale, TimeBetweenEmissions, DurationInSeconds, particleEmitterEffect.Offset);
			}
		}
		return true;
	}

	public override void PreInitValidate(ref List<string> errors)
	{
		base.PreInitValidate(ref errors);
		if (Location == null && UseLocationOfEntity == null)
		{
			EntityType.CreateValidationError(ref errors, "Either Location or UseLocationOfEntity must be specified to place the emitter on the map");
		}
	}
}
