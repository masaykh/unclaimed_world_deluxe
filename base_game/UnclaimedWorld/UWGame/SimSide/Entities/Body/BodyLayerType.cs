using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Entities.Body;

[DebuggerDisplay("{KeyName}")]
public class BodyLayerType : IGameData
{
	/// <summary>
	/// The relative thickness of this layer compared with other layers.
	/// </summary>
	public float Thickness;

	/// <summary>
	/// Resistance to damage types per unit of thickness. Higher values model resistance to non-penetrating attacks.
	/// </summary>
	public Dictionary<string, float> DamageReductionFactor;

	[XmlIgnore]
	public Dictionary<DamageType, float> DamageReductionFactorFinal;

	/// <summary>
	/// Damage always deducted from an attack. It can nullify attacks below this value; higher values model resistance to penetrating attacks.
	/// </summary>
	public Dictionary<string, float> DamageReductionConstant;

	[XmlIgnore]
	public Dictionary<DamageType, float> DamageReductionConstantFinal;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public override string ToString()
	{
		return Name;
	}

	public void PostLoadContentInitialize()
	{
		DamageReductionConstantFinal = new Dictionary<DamageType, float>();
		foreach (KeyValuePair<string, float> item in DamageReductionConstant)
		{
			DamageReductionConstantFinal.Add(GameData.Instance.AllDamageTypes[item.Key], item.Value);
		}
		DamageReductionFactorFinal = new Dictionary<DamageType, float>();
		foreach (KeyValuePair<string, float> item2 in DamageReductionFactor)
		{
			DamageReductionFactorFinal.Add(GameData.Instance.AllDamageTypes[item2.Key], item2.Value);
		}
	}

	public void Initialize()
	{
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
