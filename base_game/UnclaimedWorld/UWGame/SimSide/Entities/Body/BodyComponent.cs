using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Body;

public class BodyComponent : Component
{
	public Body Body;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public BodyComponent(Entity parent)
		: base(parent)
	{
		Body = new Body(parent);
		BodyPartType[] bodyPartTypes = parent.EntityType.BodyType.BodyPartTypes;
		foreach (BodyPartType bodyPartType in bodyPartTypes)
		{
			AddBodyParts(Body, bodyPartType);
		}
	}

	public BodyComponent()
	{
	}

	private void AddBodyParts(IHasBodyParts addTo, BodyPartType bodyPartType)
	{
		BodyPart bodyPart = ((!(bodyPartType is BiologicalBodyPartType)) ? ((BodyPart)new MachineBodyPart(bodyPartType, Body)) : ((BodyPart)new BiologicalBodyPart(bodyPartType, Body)));
		if (addTo.BodyParts == null)
		{
			addTo.BodyParts = new List<BodyPart>();
		}
		addTo.BodyParts.Add(bodyPart);
		if (bodyPartType.BodyPartTypes != null)
		{
			BodyPartType[] bodyPartTypes = bodyPartType.BodyPartTypes;
			foreach (BodyPartType bodyPartType2 in bodyPartTypes)
			{
				AddBodyParts(bodyPart, bodyPartType2);
			}
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		Body = (Body)sn.DoISnapshot(Body);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		Body.LoadPostProcess(sn);
	}
}
