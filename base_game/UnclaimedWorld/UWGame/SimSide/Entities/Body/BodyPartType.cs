using System.Linq;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Body;

[XmlInclude(typeof(MachineBodyPartType))]
[XmlInclude(typeof(BiologicalBodyPartType))]
public abstract class BodyPartType
{
	public string ModelMesh;

	public BodyPartFunction[] Functions;

	public string Name;

	public string BodyKeyName;

	public string ArmorLayer;

	[XmlIgnore]
	public BodyLayerType ArmorLayerType;

	public BodyPartType[] BodyPartTypes;

	public float HitpointsFraction;

	public float ToHitProfileFront;

	public float ToHitProfileBack;

	public float ToHitProfileLeft;

	public float ToHitProfileRight;

	public BodyPartType()
	{
	}

	public BodyPartType FindBodyPart(string name)
	{
		if (Name == name)
		{
			return this;
		}
		BodyPartType bodyPartType = BodyPartTypes.First((BodyPartType b) => b.Name == name);
		if (bodyPartType == null)
		{
			if (BodyPartTypes.Length != 0)
			{
				BodyPartType[] bodyPartTypes = BodyPartTypes;
				for (int num = 0; num < bodyPartTypes.Length; num++)
				{
					bodyPartType = bodyPartTypes[num].FindBodyPart(name);
					if (bodyPartType != null)
					{
						return bodyPartType;
					}
				}
				return null;
			}
			return null;
		}
		return bodyPartType;
	}

	public virtual bool IsVital()
	{
		return false;
	}

	public virtual void PostLoadContentInitialize()
	{
		if (ArmorLayerType != null)
		{
			ArmorLayerType.PostLoadContentInitialize();
		}
		if (BodyPartTypes != null)
		{
			BodyPartType[] bodyPartTypes = BodyPartTypes;
			for (int i = 0; i < bodyPartTypes.Length; i++)
			{
				bodyPartTypes[i].PostLoadContentInitialize();
			}
		}
	}

	public virtual void Initialize()
	{
		if (ArmorLayer != null)
		{
			ArmorLayerType = GameData.Instance.AllBodyLayerTypes[ArmorLayer];
		}
		if (BodyPartTypes != null)
		{
			BodyPartType[] bodyPartTypes = BodyPartTypes;
			for (int i = 0; i < bodyPartTypes.Length; i++)
			{
				bodyPartTypes[i].Initialize();
			}
		}
		if (ToHitProfileBack == 0f && ToHitProfileFront == 0f && ToHitProfileLeft == 0f && ToHitProfileRight == 0f)
		{
			ToHitProfileBack = (ToHitProfileFront = (ToHitProfileRight = (ToHitProfileLeft = 1f)));
		}
	}

	public override string ToString()
	{
		return Name;
	}
}
