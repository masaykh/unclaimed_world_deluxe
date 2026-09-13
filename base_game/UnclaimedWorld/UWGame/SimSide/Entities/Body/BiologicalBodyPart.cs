using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Body;

public class BiologicalBodyPart : BodyPart
{
	public List<Wound> Wounds;

	public List<Organ> Organs;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public BiologicalBodyPart()
	{
	}

	public BiologicalBodyPart(BodyPartType bodyPartType, Body body)
		: base(bodyPartType, body)
	{
	}

	public BiologicalBodyPart(BodyPart original)
		: base(original)
	{
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
		sn.Postpone(Wounds);
		sn.Postpone(Organs);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		sn.RegisterLoadPostProcessCall(this);
	}
}
