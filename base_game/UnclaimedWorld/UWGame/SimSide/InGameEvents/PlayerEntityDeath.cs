using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Skills;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.InGameEvents;

public class PlayerEntityDeath : ISnapshot
{
	public string EntityName;

	public EntityID Corpse;

	public CauseOfDeath? CauseOfDeath;

	public ProfessionType Profession;

	public string DisplayImageName;

	public string HisHerIts;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		EntityName = sn.DoString(EntityName);
		Corpse = sn.DoEnum(Corpse);
		CauseOfDeath = sn.DoEnumNullable(CauseOfDeath);
		HisHerIts = sn.DoString(HisHerIts);
		Profession = sn.DoGameData(Profession);
		DisplayImageName = sn.DoString(DisplayImageName);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
