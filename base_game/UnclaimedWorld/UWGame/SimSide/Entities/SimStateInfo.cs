using UWGame.SimSide.Collisions;

namespace UWGame.SimSide.Entities;

public class SimStateInfo : IStateInfo
{
	public GeometryLayoutType GeometryLayoutType;

	public BitMask64 Conditions { get; set; }

	public BitMask64 Forbiddens { get; set; }
}
