using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items;

public class Tool : Component
{
	private bool? isPrepared;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool? IsPrepared
	{
		get
		{
			return isPrepared;
		}
		set
		{
			if (value == isPrepared)
			{
				return;
			}
			bool? flag = value;
			bool flag2 = true;
			if (flag == true == flag2 && flag.HasValue && Parent.EntityType.ContainerType != null && Parent.EntityType.ContainerType.GetRequiresReplenishType() != null && !((IHasReplenishItems)Parent.Contains).ReplenishItems.Start())
			{
				return;
			}
			isPrepared = value;
			if (Parent.EntityType.ToolType.PrepareProcess == null)
			{
				return;
			}
			StateModifier stateModifier = Parent.EntityType.ToolType.PrepareProcessType.PreparedToolModifier ?? StateModifier.PreparedTool;
			if (isPrepared == true)
			{
				Parent.SetSpriteStateFlag(stateModifier);
				if (Parent.GetContainedBy(out Entity container))
				{
					container?.SetSpriteStateFlag(stateModifier);
				}
			}
			else if (isPrepared == false)
			{
				Parent.ClearSpriteStateFlag(stateModifier);
				if (Parent.GetContainedBy(out Entity container2))
				{
					container2?.ClearSpriteStateFlag(stateModifier);
				}
			}
		}
	}

	public Tool(Entity parent)
		: base(parent)
	{
		if (Parent.EntityType.ToolType.PrepareProcess != null)
		{
			isPrepared = false;
		}
		else
		{
			isPrepared = null;
		}
	}

	public Tool()
	{
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		isPrepared = sn.DoBoolNullable(isPrepared);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
