using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Combat;

public class SplashAttack : AreaAttack
{
	public enum DamageFalloffType
	{
		None,
		Linear,
		Quadratic
	}

	private float DamageRadius;

	public DamageFalloffType DamageFalloff;

	public override List<Entity> GetEntitiesInArea(Vector3 location, Vector2 direction)
	{
		List<Pair<Entity, Vector2>> resultsList = null;
		The.AgentQuadTree.GetEntitiesInRange(location.ToVector2(), DamageRadius, null, ref resultsList);
		return resultsList?.Select((Pair<Entity, Vector2> t) => t.First).ToList();
	}

	public override void Initialize()
	{
	}
}
