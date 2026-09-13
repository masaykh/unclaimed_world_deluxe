using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland;

public struct TravelLocation
{
	private readonly long? allegianceID;

	private readonly long siteID;

	private readonly long? expeditionID;

	private readonly long? terminalEntityID;

	public long? AllegianceID => allegianceID;

	public long SiteID => siteID;

	public long? ExpeditionID => expeditionID;

	public long? TerminalEntityID => terminalEntityID;

	public TravelLocation(TravelLocation original)
	{
		allegianceID = original.AllegianceID;
		siteID = original.SiteID;
		expeditionID = original.ExpeditionID;
		terminalEntityID = original.TerminalEntityID;
	}

	public TravelLocation(long SiteID, long? AllegianceID, long? ExpeditionID, long? TerminalEntityID)
	{
		allegianceID = AllegianceID;
		siteID = SiteID;
		expeditionID = ExpeditionID;
		terminalEntityID = TerminalEntityID;
	}

	public TravelLocation(Allegiance allegiance, long? expeditionID, long? terminalEntityID)
	{
		this.expeditionID = expeditionID;
		this.terminalEntityID = terminalEntityID;
		allegianceID = (long)allegiance.ID;
		siteID = (long)allegiance.Site.ID;
	}

	public bool ResolveLocation(SharedKnowledge sharedKnowledge, out Site site, out Allegiance allegiance, out Expedition expedition, out IKnownEntityData terminalData)
	{
		site = null;
		allegiance = null;
		expedition = null;
		terminalData = null;
		bool result = true;
		if (AllegianceID.HasValue)
		{
			allegiance = LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID((AllegianceID)AllegianceID.Value);
			if (allegiance == null)
			{
				result = false;
			}
		}
		site = LookUp<Site, UWGame.SimSide.Overland.SiteID>.FindByID((SiteID)SiteID);
		if (site == null)
		{
			result = false;
		}
		if (ExpeditionID.HasValue)
		{
			expedition = LookUp<Expedition, UWGame.SimSide.Expeditions.ExpeditionID>.FindByID((ExpeditionID)ExpeditionID.Value);
			if (expedition == null)
			{
				result = false;
			}
		}
		if (TerminalEntityID.HasValue && GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData((EntityID)TerminalEntityID.Value, out terminalData)))
		{
			result = false;
		}
		return result;
	}

	public override bool Equals(object obj)
	{
		if (obj is TravelLocation)
		{
			return this == (TravelLocation)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return AllegianceID.GetHashCode() ^ SiteID.GetHashCode() ^ expeditionID.GetHashCode() ^ terminalEntityID.GetHashCode();
	}

	public static bool operator ==(TravelLocation x, TravelLocation y)
	{
		if (x.SiteID == y.SiteID)
		{
			long? num = x.AllegianceID;
			long? num2 = y.AllegianceID;
			if (num.GetValueOrDefault() == num2.GetValueOrDefault() && num.HasValue == num2.HasValue && x.ExpeditionID == y.ExpeditionID)
			{
				return x.TerminalEntityID == y.TerminalEntityID;
			}
		}
		return false;
	}

	public static bool operator !=(TravelLocation x, TravelLocation y)
	{
		return !(x == y);
	}

	public static string GetKey(SiteID siteID, AllegianceID? allegianceID, ExpeditionID? expeditionID, EntityID? terminal)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Site:");
		stringBuilder.Append(siteID.ToString());
		stringBuilder.Append("All:");
		if (allegianceID.HasValue)
		{
			stringBuilder.Append(((long)allegianceID.Value).ToString());
		}
		stringBuilder.Append("Exp:");
		if (expeditionID.HasValue)
		{
			stringBuilder.Append(((long)expeditionID.Value).ToString());
		}
		stringBuilder.Append("Term:");
		if (terminal.HasValue)
		{
			stringBuilder.Append(((long)terminal.Value).ToString());
		}
		return stringBuilder.ToString();
	}
}
