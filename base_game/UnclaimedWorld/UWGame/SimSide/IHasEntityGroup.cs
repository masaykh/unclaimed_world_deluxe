using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide;

public interface IHasEntityGroup : ILookUp<IHasEntityGroup, HasEntityGroupID>
{
	Vector3? Location { get; }

	int NoOfWorkers { get; }

	Allegiance Allegiance { get; }

	decimal? TradeCredits { get; set; }

	bool IsEatable(EntityType entityType);
}
