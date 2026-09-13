using System.Collections.Generic;

namespace UWGame.SimSide.Entities.Body;

public class BodyType : IGameData
{
	public BodyPartType[] BodyPartTypes;

	public float? Hitpoints;

	public float? Bulk;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public BodyType()
	{
	}

	public BodyType(string keyName)
	{
		KeyName = keyName;
	}

	public override string ToString()
	{
		return KeyName;
	}

	public BodyPartType FindBodyPart(string name)
	{
		BodyPartType[] bodyPartTypes = BodyPartTypes;
		for (int i = 0; i < bodyPartTypes.Length; i++)
		{
			BodyPartType bodyPartType = bodyPartTypes[i].FindBodyPart(name);
			if (bodyPartType != null)
			{
				return bodyPartType;
			}
		}
		return null;
	}

	public void PostLoadContentInitialize()
	{
		BodyPartType[] bodyPartTypes = BodyPartTypes;
		for (int i = 0; i < bodyPartTypes.Length; i++)
		{
			bodyPartTypes[i].PostLoadContentInitialize();
		}
	}

	public void Initialize()
	{
		BodyPartType[] bodyPartTypes = BodyPartTypes;
		for (int i = 0; i < bodyPartTypes.Length; i++)
		{
			bodyPartTypes[i].Initialize();
		}
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
