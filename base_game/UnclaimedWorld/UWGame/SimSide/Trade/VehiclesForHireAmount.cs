using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Trade;

public class VehiclesForHireAmount : ISnapshot
{
	public int StartAmount;

	public float IncreasePerDay;

	public int MaxAmount;

	public decimal Price;

	public decimal PricePerKilometer;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public VehiclesForHireAmount()
	{
	}

	public VehiclesForHireAmount(VehiclesForHireType amountType, float sizeFactor)
	{
		if (amountType.SpecificStartAmount != null)
		{
			StartAmount = amountType.SpecificStartAmount.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator);
		}
		else
		{
			StartAmount = (int)((float)amountType.StartAmount * sizeFactor);
			if (amountType.StartAmount == 1)
			{
				StartAmount = Common.ClampBottom(StartAmount, 1);
			}
		}
		if (amountType.SpecificStartAmount != null)
		{
			MaxAmount = amountType.SpecificMaxAmount.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator);
		}
		else
		{
			MaxAmount = (int)((float)amountType.MaxAmount * sizeFactor);
		}
		if (amountType.SpecificPrice != null)
		{
			Price = (decimal)amountType.SpecificPrice.GetRandomValue(The.Sim.GameplayRandomGenerator);
		}
		else
		{
			Price = (decimal)amountType.Price;
		}
		PricePerKilometer = (decimal)amountType.PricePerKilometer;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		MaxAmount = sn.DoInt32(MaxAmount);
		StartAmount = sn.DoInt32(StartAmount);
		IncreasePerDay = sn.DoFloat(IncreasePerDay);
		Price = sn.DoDecimal(Price);
		PricePerKilometer = sn.DoDecimal(PricePerKilometer);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
