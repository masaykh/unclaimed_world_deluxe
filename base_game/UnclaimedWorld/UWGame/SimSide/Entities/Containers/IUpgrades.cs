using System.Collections.Generic;

namespace UWGame.SimSide.Entities.Containers;

internal interface IUpgrades
{
	Dictionary<UpgradeCategory, EntityID> ContainedUpgrades { get; }

	bool IsUpgrade(EntityID entityID);
}
