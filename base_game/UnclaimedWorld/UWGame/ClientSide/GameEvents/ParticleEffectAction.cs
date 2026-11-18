using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Scenarios;

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

	/// <summary>
	/// PORT FIX. A scenario's lasting map effects - Twinkler Island's sulphur lake smoke, its
	/// fog and haze - are ParticleEffectActions in the scenario's start actions. They run once,
	/// in Sim.StartGamePostLoadMapExecuteEvents, and only for ScenarioNewGame
	/// (StartGamePostLoadMapPrepareEventActions has no case for a loaded game, in the studio's
	/// source as well). What they make is client-only: a Renderable on a TerrainTile
	/// (RenderablesOnTile is sn.Ignore'd) holding a ParticleEmitter, and neither is in a save.
	/// So every load - including the reload that follows every in-game SAVE - came back without
	/// them. Kastuk, 2026-11-17: "Sulfur lake at Twinkler island no more smoking after loading
	/// the savegame." A vanilla bug, not a port regression.
	///
	/// Snapshotter.Load calls this once the loaded Sim has its StartGameParams. It runs again
	/// exactly the start actions that leave something lasting behind: ParticleEffectActions with
	/// no DurationInSeconds. A timed one (Twinkler Island's blackSmoke over the wreck, 20 s) has
	/// long burned out in any game worth saving, so it stays gone. The new client and the
	/// freshly read map have no emitters of their own, so nothing is doubled; a new game never
	/// comes here. Returns how many emitter actions ran.
	/// </summary>
	public static int RestoreLastingStartEmittersAfterLoad(StartGameParams startGameParams)
	{
		int restored = 0;
		foreach (ParticleEffectAction action in LastingStartEmitters(startGameParams?.StartScenarioParams))
		{
			try
			{
				string failReason = "";
				if (action.Execute(new EventAction(action, null), ref failReason))
				{
					restored++;
				}
			}
			catch (System.Exception ex)
			{
				// Smoke is scenery. A scenario whose effect cannot be rebuilt must not stop the load.
				GameStateManagement.UnclaimedWorld.LogError(action.KeyName + ": " + ex.Message, "Scenario emitter not restored after load");
			}
		}
		return restored;
	}

	/// <summary>
	/// PORT FIX. The start actions Sim.StartGamePostLoadMapPrepareEventActions collects for a new
	/// game - the scenario's own, then each chosen option's - narrowed to the lasting emitters.
	/// </summary>
	public static List<ParticleEffectAction> LastingStartEmitters(StartScenarioParams scenarioParams)
	{
		List<ParticleEffectAction> list = new List<ParticleEffectAction>();
		if (scenarioParams?.Scenario?.ScenarioData == null)
		{
			return list;
		}
		List<EventActionType> actions = new List<EventActionType>();
		if (scenarioParams.Scenario.ScenarioData.Actions != null)
		{
			foreach (string key in scenarioParams.Scenario.ScenarioData.Actions)
			{
				if (key != null && GameData.Instance.AllEventActionTypes.TryGetValue(key, out EventActionType action))
				{
					actions.Add(action);
				}
			}
		}
		if (scenarioParams.Options != null)
		{
			foreach (KeyValuePair<string, Option> option in scenarioParams.Options)
			{
				actions.AddRange(option.Value.GetStartActions());
			}
		}
		foreach (EventActionType action in actions)
		{
			if (action is ParticleEffectAction particleAction && !particleAction.DurationInSeconds.HasValue && particleAction.ParticleEmitters != null)
			{
				list.Add(particleAction);
			}
		}
		return list;
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
