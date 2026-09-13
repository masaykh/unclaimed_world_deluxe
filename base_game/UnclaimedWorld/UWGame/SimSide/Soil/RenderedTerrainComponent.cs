using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Soil;

public abstract class RenderedTerrainComponent : ISnapshot
{
	private bool displayAmountIsDirty;

	private float amount;

	protected Terrain Parent;

	private TerrainID snapshotParent;

	private float displayAmount;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float Amount
	{
		get
		{
			return amount;
		}
		set
		{
			amount = value;
			displayAmountIsDirty = true;
		}
	}

	public float DisplayAmount
	{
		get
		{
			if (displayAmountIsDirty)
			{
				displayAmount = ComputeDisplayAmount();
				displayAmountIsDirty = false;
			}
			return displayAmount;
		}
	}

	public bool IsSnapshotted { get; set; }

	public RenderedTerrainComponent(Terrain parent)
	{
		Parent = parent;
	}

	public RenderedTerrainComponent()
	{
	}

	protected virtual float ComputeDisplayAmount()
	{
		return amount;
	}

	public void RecomputeDisplayAmount()
	{
		displayAmount = ComputeDisplayAmount();
		displayAmountIsDirty = false;
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotParent = sn.SnapshotID<Terrain, TerrainID>(Parent).Value;
		amount = sn.DoFloat(amount);
		displayAmount = sn.DoFloat(displayAmount);
		displayAmountIsDirty = sn.DoBool(displayAmountIsDirty);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Parent = LookUpSortedDictionary<Terrain, TerrainID>.FindByID(snapshotParent);
		_ = Parent;
	}
}
