using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Locomotors.Stances;

public class ChanceToTakeStance
{
	public float? Chance;

	public float? AddedChanceToRemainInStance;

	public string Stance;

	[XmlIgnore]
	public StanceType StanceType;

	public void Initialize()
	{
		StanceType = GameData.Instance.AllStanceTypes[Stance];
	}
}
