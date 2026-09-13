using System;
using System.Collections.Generic;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide;

public class GameDataCollection<T> : Dictionary<string, T>, IGameDataCollection where T : IGameData
{
	private int order;

	public int Order => order;

	public IGameData Get(string key)
	{
		return base[key];
	}

	public GameDataCollection(Dictionary<Type, IGameDataCollection> allGameDataCollections, DataLoaderQueueState order)
	{
		this.order = (int)order;
		allGameDataCollections[typeof(T)] = this;
	}

	public void PreDataCompleteValidate(Dictionary<string, List<string>> allPreInitValidationErrors)
	{
		string name = typeof(T).Name;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, T> current = enumerator.Current;
			List<string> listOfErrors = null;
			current.Value.PreDataCompleteValidate(ref listOfErrors);
			if (listOfErrors != null)
			{
				allPreInitValidationErrors.Add(name + "/" + current.Key, listOfErrors);
			}
		}
	}

	public void PostDataCompleteInitialize()
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.PostDataCompleteInitialize();
		}
	}

	public void PostDataCompleteValidate(Dictionary<string, List<string>> allPostInitValidationErrors)
	{
		string name = typeof(T).Name;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, T> current = enumerator.Current;
			List<string> listOfErrors = null;
			current.Value.PostDataCompleteValidate(ref listOfErrors);
			if (listOfErrors != null)
			{
				allPostInitValidationErrors.Add(name + "/" + current.Key, listOfErrors);
			}
		}
	}
}
