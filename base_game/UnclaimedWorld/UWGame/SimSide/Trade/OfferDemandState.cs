using UWGame.SimSide.Entities.Biological;

namespace UWGame.SimSide.Trade;

public class OfferDemandState : IEdge
{
	public NormalDistribution OfferDemandChange;

	public float Edge { get; set; }
}
