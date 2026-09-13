using UWGame.Control.Commands;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Tiers;

namespace UWGame.SimSide.Commands;

public class AdoptTierPolicy : Command
{
	public long ExpeditionID;

	public string Tier;

	public RatingTypes Rating;

	public AdoptTierPolicy()
	{
	}

	public AdoptTierPolicy(ExpeditionID expeditionID, TierType tier, RatingTypes rating, bool giveClientFeedback)
	{
		ExpeditionID = (long)expeditionID;
		Tier = tier.KeyName;
		Rating = rating;
	}

	public override void Execute(bool giveClientFeedback)
	{
		Expedition expedition = LookUp<Expedition, UWGame.SimSide.Expeditions.ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);
		GameData.Instance.AllTierTypes.TryGetValue(Tier, out var value);
		if (expedition != null && value != null)
		{
			expedition.AdoptTierPolicy(value, Rating);
		}
	}
}
