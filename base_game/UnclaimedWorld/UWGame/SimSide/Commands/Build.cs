using Microsoft.Xna.Framework;
using UWGame.Control.Commands;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class Build : Command
{
	public long EntityGroupID;

	public string EntityTypeKey;

	public Vector3 Location;

	public bool GiveClientFeedback;

	public Build()
	{
	}

	public Build(EntityType structureType, Vector3 location, bool giveClientFeedback, EntityGroupID entityGroupID)
	{
		EntityTypeKey = structureType.KeyName;
		Location = location;
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
	}

	public override void Execute(bool giveClientFeedback)
	{
		bool flag = StartBuild();
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.StartBuild();
		}
	}

	private bool StartBuild()
	{
		EntityGroup entityGroup = LookUp<EntityGroup, UWGame.SimSide.Entities.EntityGroupID>.FindByID((EntityGroupID)EntityGroupID);
		return Structure.PrepareAndStartBuildingJob(EntityTypeKey, entityGroup, (IOwner)entityGroup.Parent, Location);
	}
}
