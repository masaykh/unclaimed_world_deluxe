using UWGame.Control.Commands;
using UWGame.Mods;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

/// <summary>
/// Adds trade credits to an allegiance: DebugMod's credits key, for testing what the colony
/// cannot yet afford (Kastuk, "Dogs and robots", 2026-10-09: "To Test GOPHER I need to buy it,
/// but it so costly ... Need to bring debug tool to change credits quantity").
///
/// The studio had the same tool - "Dev.Add credits" on its developer panel (Client.AddCredits,
/// +500), which wrote TradeCredits straight from the interface. A command instead, so a replay
/// carries it. In a build without the mod it does nothing - DebugMod.Absent's AddCredits is empty.
/// </summary>
public class AddCredits : Command
{
	public long AllegianceID;

	public decimal Amount;

	public AddCredits()
	{
	}

	public AddCredits(AllegianceID allegianceID, decimal amount)
	{
		AllegianceID = (long)allegianceID;
		Amount = amount;
	}

	public override void Execute(bool giveClientFeedback)
	{
		Allegiance allegiance = LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID((AllegianceID)AllegianceID);
		if (allegiance != null)
		{
			DebugMod.AddCredits(allegiance, Amount);
		}
	}
}
