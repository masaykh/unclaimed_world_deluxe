using System.Collections.Generic;
using UWGame.SimSide.Collisions;

namespace UWGame.SimSide.Entities.Locomotors;

public interface ICollisionResponder
{
	void BeginCollisionHandling();

	void HandleSingleCollision(Collidable<Entity> collidee);

	void EndCollisionHandling(List<Collidable<Entity>> collidees);
}
