using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Needs;

public class FoodNeed : ISnapshot
{
	public Need Parent;

	private bool bulkIsDirty = true;

	private float neededNutrientBulk;

	public float TotalNeededNutrientBulk;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float CurrentNeededNutrientBulk
	{
		get
		{
			if (bulkIsDirty)
			{
				neededNutrientBulk = TotalNeededNutrientBulk * (1f - Parent.CurrentLevel);
				bulkIsDirty = false;
			}
			return neededNutrientBulk;
		}
	}

	public bool IsSnapshotted { get; set; }

	public FoodNeed()
	{
	}

	public FoodNeed(Need parent)
	{
		Parent = parent;
	}

	public void SetNeedDirty()
	{
		bulkIsDirty = true;
	}

	public void UpdateBulk()
	{
		TotalNeededNutrientBulk = Parent.NeedType.FoodNeedType.RequiredNutrientsAsFractionOfEntityBulk * Parent.Parent.Parent.Parent.Bulk;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		neededNutrientBulk = sn.DoFloat(neededNutrientBulk);
		TotalNeededNutrientBulk = sn.DoFloat(TotalNeededNutrientBulk);
		sn.Ignore(Parent);
		sn.Ignore(bulkIsDirty);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
