using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public interface IDetectable : ILookUp<IDetectable, DetectableID>
{
	EntityType EntityType { get; }

	ResourceType ResourceType { get; }

	Vector3 Location { get; }

	bool IsIntelligent { get; }

	bool UsesMemory(SharedKnowledge sharedKnowledge);

	bool RequiresRollToDetect();

	string ToLink(bool useUpperCase = false);
}
