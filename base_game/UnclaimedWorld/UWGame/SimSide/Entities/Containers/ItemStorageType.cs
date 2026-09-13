using System.Collections.Generic;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Containers;

public class ItemStorageType
{
	public SerializableDictionary<string, StorageType> StorageSpaces = new SerializableDictionary<string, StorageType>();

	public float? FullStatePercentage;

	public float? HalfFullStatePercentage;

	public ItemStorageType()
	{
	}

	public ItemStorageType(float isolatedCapacity)
	{
		StorageSpaces.Add("isolated", new StorageType
		{
			Capacity = isolatedCapacity
		});
	}

	public ItemStorageType(string condition1, float capacity1, string condition2 = null, float? capacity2 = null, string condition3 = null, float? capacity3 = null)
	{
		StorageSpaces.Add(condition1, new StorageType
		{
			Capacity = capacity1
		});
		if (condition2 != null)
		{
			StorageSpaces.Add(condition2, new StorageType
			{
				Capacity = capacity2.Value
			});
		}
		if (condition3 != null)
		{
			StorageSpaces.Add(condition3, new StorageType
			{
				Capacity = capacity3.Value
			});
		}
	}

	public void Initialize()
	{
		if (!StorageSpaces.ContainsKey("isolated"))
		{
			StorageSpaces.Add("isolated", new StorageType
			{
				Capacity = 0f
			});
		}
	}

	public float GetTotalCapacity()
	{
		float num = 0f;
		foreach (KeyValuePair<string, StorageType> storageSpace in StorageSpaces)
		{
			num += storageSpace.Value.Capacity;
		}
		return num;
	}
}
