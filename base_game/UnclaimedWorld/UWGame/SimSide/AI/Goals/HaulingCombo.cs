using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;

namespace UWGame.SimSide.AI.Goals;

public struct HaulingCombo
{
	public HaulingJob Job;

	public EntityID Item;

	public EntityID? Vehicle;

	public double Score;
}
