using System.Linq;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners;

internal class PhysicalNeedsMotivation : Motivation
{
	private int maxNrOfHuntingJobs;

	private int maxNrOfScoutingJobs;

	private Allegiance allegiance;

	private AllegianceID snapshotAllegiance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public PhysicalNeedsMotivation()
	{
	}

	public PhysicalNeedsMotivation(Allegiance allegiance)
	{
		this.allegiance = allegiance;
		maxNrOfHuntingJobs = allegiance.Members.Count;
		maxNrOfScoutingJobs = allegiance.Members.Count;
	}

	public bool GetActionsWeAreMotivatedToDo(out int numberOfScoutingActionsNeeded, out int numberOfHuntingActionsNeeded)
	{
		return NeedsMoreResources(out numberOfScoutingActionsNeeded, out numberOfHuntingActionsNeeded);
	}

	private bool NeedsMoreResources(out int numberOfScoutingActionsNeeded, out int numberOfHuntingActionsNeeded)
	{
		float num = allegiance.Members.Count;
		maxNrOfScoutingJobs = (int)(num * allegiance.RepresentativeEntityType.IntelligenceType.MembersScoutingFraction);
		maxNrOfHuntingJobs = (int)(num * allegiance.RepresentativeEntityType.IntelligenceType.MembersScoutingFraction);
		numberOfScoutingActionsNeeded = 0;
		numberOfHuntingActionsNeeded = 0;
		DoesThisAllegianceNeedMoreHuntingJobs();
		if (DoesThisAllegianceNeedMoreScoutingJobs())
		{
			numberOfScoutingActionsNeeded = maxNrOfScoutingJobs - allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs.Count();
		}
		return numberOfScoutingActionsNeeded + numberOfHuntingActionsNeeded > 0;
	}

	private bool DoesThisAllegianceNeedMoreHuntingJobs()
	{
		if (allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs.Count > maxNrOfHuntingJobs)
		{
			return false;
		}
		return true;
	}

	private bool DoesThisAllegianceNeedMoreScoutingJobs()
	{
		if (allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs.Count > maxNrOfScoutingJobs)
		{
			return false;
		}
		return true;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		snapshotAllegiance = sn.SnapshotID<Allegiance, AllegianceID>(allegiance).Value;
		maxNrOfHuntingJobs = sn.DoInt32(maxNrOfHuntingJobs);
		maxNrOfScoutingJobs = sn.DoInt32(maxNrOfScoutingJobs);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
	}
}
