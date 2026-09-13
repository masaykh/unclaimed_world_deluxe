using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Substances;

public class SubstanceComponent : Component, IIDEventSubscriber
{
	private Dictionary<SubstanceType, SubstanceAmount> bulkAmounts;

	private MethodID parentBulkChangedID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Dictionary<SubstanceType, SubstanceAmount> BulkAmounts
	{
		get
		{
			return bulkAmounts;
		}
		set
		{
			bulkAmounts = value;
		}
	}

	public SubstanceComponent(Entity parent)
		: base(parent)
	{
		Parent.BulkChangedEvent.AddAndRegister((Action<float>)Parent_BulkChanged, (IIDEventSubscriber)this, out parentBulkChangedID);
	}

	public SubstanceComponent()
	{
	}

	private void Parent_BulkChanged(float oldValue)
	{
		if (BulkAmounts == null)
		{
			InitializeSubstanceAmounts();
		}
		else
		{
			UpdateSubstanceAmountsWithNewBulk(oldValue);
		}
	}

	public void ChangeSubstanceBulk(SubstanceType type, float changeInBulk)
	{
		BulkAmounts[type].Amount += changeInBulk;
		Parent.BulkChangedEvent.Remove(parentBulkChangedID);
		Parent.Bulk += changeInBulk;
		Parent.BulkChangedEvent.Add(parentBulkChangedID, this);
	}

	public void ConvertSubstance(SubstanceType from, SubstanceType to)
	{
	}

	private void InitializeSubstanceAmounts()
	{
		BulkAmounts = new Dictionary<SubstanceType, SubstanceAmount>();
		foreach (KeyValuePair<SubstanceType, float> finalSubstanceFraction in Parent.EntityType.SubstancesType.FinalSubstanceFractions)
		{
			if (BulkAmounts.TryGetValue(finalSubstanceFraction.Key, out var value))
			{
				value.Amount = finalSubstanceFraction.Value * Parent.Bulk;
			}
			else
			{
				BulkAmounts.Add(finalSubstanceFraction.Key, new SubstanceAmount(finalSubstanceFraction.Value * Parent.Bulk, finalSubstanceFraction.Key));
			}
		}
	}

	private void UpdateSubstanceAmountsWithNewBulk(float oldBulk)
	{
		float bulkPercentageIncrease = UWGame.SimSide.Entities.Body.Body.GetBulkPercentageIncrease(oldBulk, Parent.Bulk);
		foreach (SubstanceType item in BulkAmounts.Keys.ToList())
		{
			float amount = BulkAmounts[item].Amount * bulkPercentageIncrease;
			BulkAmounts[item].Amount = amount;
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		parentBulkChangedID = sn.DoMethodID(parentBulkChangedID);
		BulkAmounts = sn.DoDictionary(BulkAmounts);
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
		LoadPostProcessRegisterMethodIDs();
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		ActionLookup<float>.Add(parentBulkChangedID, Parent_BulkChanged);
	}
}
