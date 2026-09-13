using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Entities;

public class EditorData : Component
{
	public string AllegianceKey;

	public string ExpeditionName;

	public string ThreatGroupName;

	public AgeGroup? TreeAgeGroup;

	public Resource[] Resources;

	public EditorData(Entity parent)
		: base(parent)
	{
	}

	public EditorData()
	{
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		return Snapshotter.Version.Original;
	}
}
