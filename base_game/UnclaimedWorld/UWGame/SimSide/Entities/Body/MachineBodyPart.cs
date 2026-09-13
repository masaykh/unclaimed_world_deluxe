using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Body;

public class MachineBodyPart : BodyPart, IComposite, ILookUp<IComposite, CompositeID>
{
	private bool partIsBroken;

	private List<Entity> parts = new List<Entity>();

	private List<EntityID> snapshotParts;

	private CompositeID id = CompositeID.Invalid;

	public List<Entity> Parts => parts;

	public CompositeID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public MachineBodyPart()
	{
	}

	public MachineBodyPart(BodyPartType bodyPartType, Body body)
		: base(bodyPartType, body)
	{
		if (!Snapshotter.IsSnapshotting)
		{
			AddToLookup();
		}
	}

	public MachineBodyPart(BodyPart original)
		: base(original)
	{
	}

	public void SetBrokenPart()
	{
		partIsBroken = true;
	}

	public void SetConditionDirty()
	{
		GetRoot().SetConditionDirty();
	}

	public void SetPart(Entity newPart)
	{
		Body.Parent.SetPart(newPart);
	}

	public void RemovePart(Entity part, bool setPartOfToNull = true)
	{
		Body.Parent.RemovePart(part, setPartOfToNull);
	}

	public IComposite GetRoot()
	{
		return Body.Parent;
	}

	public override void ChangeOwnershipOnParts(IOwner newOwner)
	{
		foreach (Entity part in Parts)
		{
			Entity.ChangeOwnershipOnParts(part, newOwner);
		}
		base.ChangeOwnershipOnParts(newOwner);
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		id = sn.DoEnum(id);
		partIsBroken = sn.DoBool(partIsBroken);
		if (Parts != null)
		{
			snapshotParts = Parts.Select((Entity p) => p.ID).ToList();
		}
		snapshotParts = sn.DoList(snapshotParts);
		sn.Ignore(parts);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (snapshotParts != null)
		{
			parts = snapshotParts.Select((EntityID p) => Entity.FindByID(p)).ToList();
		}
	}

	public CompositeID GetUniqueID()
	{
		return Composite.GetUniqueID();
	}

	public CompositeID SnapshotID(Snapshotter sn, CompositeID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != CompositeID.Invalid)
		{
			LookUpIComposites.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUpIComposites.Remove(this);
	}

	public void SetInvalid()
	{
		id = CompositeID.Invalid;
	}

	public void ResetIDCounter()
	{
	}

	void ILookUp<IComposite, CompositeID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUpIComposites.Create();
	}
}
