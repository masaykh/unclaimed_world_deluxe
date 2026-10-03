using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public class Contract : ISnapshot
{
	public ContractTemplate ContractTemplate;

	private ContractTemplateID snapshotContractTemplate;

	public decimal PaidAmount;

	public List<EntityID> TransferredEntities;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public void Revert()
	{
		IOwner owner = LookUpOwners.FindByID((OwnerID)ContractTemplate.BuyerID);
		IOwner owner2 = LookUpOwners.FindByID((OwnerID)ContractTemplate.SellerID);
		if (owner == null || owner2 == null)
		{
			return;
		}
		// PORT: the refund goes back to the buyer. The studio wrote
		// MakeTradeCreditsTransaction(buyer, seller, PaidAmount) - the direction of the original
		// sale - so reverting a contract charged the buyer a second time while also handing the
		// goods back to the seller. Mission.Abort's own notes say what was meant: "// refund -
		// penalty", "//move back the money", and RefundOrders: "we are transporting goods that have
		// been paid for, but now we are unable to deliver them".
		EntityGroup.MakeTradeCreditsTransaction(owner2, owner, PaidAmount);
		if (TransferredEntities == null)
		{
			return;
		}
		foreach (EntityID transferredEntity in TransferredEntities)
		{
			Entity entity = Entity.FindByID(transferredEntity);
			if (entity != null)
			{
				entity.ChangeOwnership(owner2);
				if (owner.OwnedEntities.TradeManager != null)
				{
					owner.OwnedEntities.TradeManager.Buy(entity.EntityType, -1);
				}
			}
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		PaidAmount = sn.DoDecimal(PaidAmount);
		snapshotContractTemplate = sn.SnapshotID<ContractTemplate, ContractTemplateID>(ContractTemplate).Value;
		TransferredEntities = sn.DoList(TransferredEntities);
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
		ContractTemplate = LookUp<ContractTemplate, ContractTemplateID>.FindByID(snapshotContractTemplate);
	}
}
