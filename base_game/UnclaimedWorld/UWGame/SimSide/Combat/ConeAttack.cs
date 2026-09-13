using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Combat;

public class ConeAttack : AreaAttack
{
	public float Length;

	public float WidthInDegrees;

	[XmlIgnore]
	private float lengthSquared;

	[XmlIgnore]
	private float angleDistanceFromCenterLineInRadians;

	public override void Initialize()
	{
		lengthSquared = Length * Length;
		angleDistanceFromCenterLineInRadians = MathHelper.ToRadians(WidthInDegrees) / 2f;
	}

	public override List<Entity> GetEntitiesInArea(Vector3 location, Vector2 direction)
	{
		List<Pair<Entity, Vector2>> resultsList = null;
		The.AgentQuadTree.GetEntitiesInRange(location.ToVector2(), Length, (Entity e) => PointIsWithinCone(location, direction, e.Location.Value), ref resultsList);
		return resultsList?.Select((Pair<Entity, Vector2> t) => t.First).ToList();
	}

	private bool PointIsWithinCone(Vector3 coneLocation, Vector2 coneDirection, Vector3 point)
	{
		Vector2 b = point.ToVector2() - coneLocation.ToVector2();
		if (b.Length() > Length)
		{
			return false;
		}
		return Common.GetAngleBetweenVectors(coneDirection, b) < (double)angleDistanceFromCenterLineInRadians;
	}
}
