using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Resources;

namespace UWGame.ClientSide.Interface.Inventory;

public class AttainableInfo
{
	public int DistanceToRoot;

	public bool IsOwned;

	public bool IsProducable;

	public List<EntityType> UnavailableInputs;

	public List<EntityType> UnavailableTools;

	public ResourceType UnavailableResource;

	public TierOrAreaType UnavailablePolicy;

	public EntityType UnavailableSpecialSite;

	public SkillType UnavailableSkill;

	private const string tooltipHeader = "Missing/unattainable: \n \n";

	public bool IsAttainable => DistanceToRoot > -1;

	public AttainableInfo(int distanceToRoot)
	{
		DistanceToRoot = distanceToRoot;
	}
}
