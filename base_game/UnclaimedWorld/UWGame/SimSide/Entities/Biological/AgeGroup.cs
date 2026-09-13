using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Biological;

public class AgeGroup : ISnapshot
{
	public AgeGroupType AgeGroupType;

	private float age;

	public BiologicalEntity Parent;

	private EntityID snapshotParent;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float Age => age;

	public bool IsSnapshotted { get; set; }

	public AgeGroup()
	{
	}

	public AgeGroup(BiologicalEntity parent)
	{
		Parent = parent;
	}

	public void UpdateAge(double deltaTimeInSeconds)
	{
		float num = (float)(deltaTimeInSeconds * The.Sim.DateAndTime.YearsPerSecond);
		age += num;
		UpdateAgeGroup();
	}

	public void SetAge(float age)
	{
		this.age = age;
		UpdateAgeGroup();
	}

	public void SetRandomAge(AIAgeGroup ageGroup)
	{
		int num = Parent.CasteType.AgeGroupTypes.FindIndex((AgeGroupType a) => a.AIAgeGroup == ageGroup);
		_ = Parent.CasteType.AgeGroupTypes[num];
		float num2 = ((num > 0) ? Parent.CasteType.AgeGroupTypes[num - 1].Edge : 0f);
		float num3 = Parent.CasteType.AgeGroupTypes[num].Edge - num2;
		SetAge((float)(The.Sim.GameplayRandomGenerator.NextDouble("AgeGroup") * (double)num3) + num2);
	}

	public static float GetAdultAge(List<AgeGroupType> AgeGroupTypes)
	{
		float num = 0f;
		foreach (AgeGroupType AgeGroupType in AgeGroupTypes)
		{
			if (AgeGroupType.AIAgeGroup == AIAgeGroup.Adult)
			{
				return num;
			}
			num = AgeGroupType.Edge;
		}
		return Math.Max(0.1f, num);
	}

	private void UpdateAgeGroup()
	{
		AgeGroupType ageGroupType = AgeGroupType;
		AgeGroupType = Common.GetStairStepIndex(Age, Parent.CasteType.AgeGroupTypes, out var _);
		if (ageGroupType != AgeGroupType)
		{
			if (Parent.Parent.PersonEntity != null)
			{
				Parent.Parent.PersonEntity.UpdateAgeGroup();
			}
			Parent.Needs.UpdateSetOfNeeds();
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		age = sn.DoFloat(age);
		snapshotParent = sn.SnapshotID<Entity, EntityID>((Parent != null) ? Parent.Parent : null).Value;
		sn.Ignore(Parent);
		sn.Ignore(AgeGroupType);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Parent = Entity.FindByID(snapshotParent).BiologicalEntity;
		AgeGroupType = Common.GetStairStepIndex(Age, Parent.CasteType.AgeGroupTypes, out var _);
	}
}
