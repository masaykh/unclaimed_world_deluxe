using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Combat;

[XmlInclude(typeof(ConeAttack))]
[XmlInclude(typeof(AreaAttack))]
public abstract class AreaAttack
{
	public bool DamageOtherAllegianceMembers;

	public abstract void Initialize();

	public abstract List<Entity> GetEntitiesInArea(Vector3 location, Vector2 direction);
}
