using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Trees;

public interface IHasCrops : ILookUp<IHasCrops, HasCropsID>
{
	Vector3 Location { get; }

	Vector3 AccessPoint { get; }

	Point MapPosition { get; }
}
