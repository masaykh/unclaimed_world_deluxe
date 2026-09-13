using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances;

public class FoodExtraction : ISnapshot
{
	private Dictionary<EntityType, HashSet<ProcessType>> FoodExtractionProcesses = new Dictionary<EntityType, HashSet<ProcessType>>();

	private Dictionary<EntityType, ProcessType> ConsumeProcesses = new Dictionary<EntityType, ProcessType>();

	private Dictionary<EntityType, HashSet<ProcessType>> ExtractionResultsInConsumable = new Dictionary<EntityType, HashSet<ProcessType>>();

	private bool extractionProcessesAreDirty = true;

	private ICanIterateEntities parent;

	private CanIterateEntitiesID parentID;

	private EntityGroupID? foodItemsGroup;

	public bool IncludeNonIndependentMembers = true;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public FoodExtraction()
	{
	}

	public FoodExtraction(ICanIterateEntities parent, EntityGroupID? foodItemsGroup, bool includeNonIndependentMembers = true)
	{
		this.parent = parent;
		parentID = parent.ID;
		IncludeNonIndependentMembers = includeNonIndependentMembers;
		this.foodItemsGroup = foodItemsGroup;
	}

	public void SetIsDirty()
	{
		extractionProcessesAreDirty = true;
	}

	private void UpdateEatingProcessesByMembers()
	{
		extractionProcessesAreDirty = false;
		Dictionary<EntityType, ProcessType> oldConsumeProcesses = new Dictionary<EntityType, ProcessType>(ConsumeProcesses);
		Dictionary<EntityType, HashSet<ProcessType>> dictionary = new Dictionary<EntityType, HashSet<ProcessType>>();
		foreach (KeyValuePair<EntityType, HashSet<ProcessType>> foodExtractionProcess in FoodExtractionProcesses)
		{
			foreach (ProcessType item in foodExtractionProcess.Value)
			{
				Common.AddToMultiList(dictionary, foodExtractionProcess.Key, item);
			}
		}
		FoodExtractionProcesses.Clear();
		ConsumeProcesses.Clear();
		ExtractionResultsInConsumable.Clear();
		parent.IterateMembers(GetFoodProcesses);
		if (FoodProcessesHaveChanged(FoodExtractionProcesses, dictionary, ConsumeProcesses, oldConsumeProcesses) && foodItemsGroup.HasValue)
		{
			LookUp<EntityGroup, EntityGroupID>.FindByID(foodItemsGroup.Value)?.SetFoodDirty();
		}
	}

	public static bool FoodProcessesHaveChanged(Dictionary<EntityType, HashSet<ProcessType>> newFoodExtractionProcesses, Dictionary<EntityType, HashSet<ProcessType>> oldFoodExtractionProcesses, Dictionary<EntityType, ProcessType> newConsumeProcesses, Dictionary<EntityType, ProcessType> oldConsumeProcesses)
	{
		foreach (KeyValuePair<EntityType, HashSet<ProcessType>> oldFoodExtractionProcess in oldFoodExtractionProcesses)
		{
			HashSet<ProcessType> value = oldFoodExtractionProcess.Value;
			newFoodExtractionProcesses.TryGetValue(oldFoodExtractionProcess.Key, out var value2);
			int num = value?.Count ?? 0;
			int num2 = value2?.Count ?? 0;
			if (num != num2)
			{
				return true;
			}
			foreach (ProcessType item in value)
			{
				if (!value2.Contains(item))
				{
					return true;
				}
			}
		}
		if (oldConsumeProcesses.Count != newConsumeProcesses.Count)
		{
			return true;
		}
		foreach (KeyValuePair<EntityType, ProcessType> oldConsumeProcess in oldConsumeProcesses)
		{
			if (!newConsumeProcesses.TryGetValue(oldConsumeProcess.Key, out var value3))
			{
				return true;
			}
			if (value3 != oldConsumeProcess.Value)
			{
				return true;
			}
		}
		return false;
	}

	private void GetFoodProcesses(Entity member)
	{
		if (member.EntityType.BiologicalType == null || (!IncludeNonIndependentMembers && !member.Intelligence.IsIndependent()))
		{
			return;
		}
		member.Find<BiologicalEntity>(out var c);
		if (c.FoodExtractionProcesses != null)
		{
			foreach (KeyValuePair<EntityType, HashSet<ProcessType>> foodExtractionProcess in c.FoodExtractionProcesses)
			{
				Common.AddToMultiList(FoodExtractionProcesses, foodExtractionProcess.Key, foodExtractionProcess.Value);
			}
		}
		if (c.ConsumeProcesses != null)
		{
			foreach (KeyValuePair<EntityType, ProcessType> consumeProcess in c.ConsumeProcesses)
			{
				Common.AddToDictionary(ref ConsumeProcesses, consumeProcess.Key, consumeProcess.Value);
			}
		}
		if (c.ExtractionResultsInConsumable == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, HashSet<ProcessType>> item in c.ExtractionResultsInConsumable)
		{
			Common.AddToDictionary(ref ExtractionResultsInConsumable, item.Key, item.Value);
		}
	}

	public bool IsEatable(EntityType food)
	{
		if (extractionProcessesAreDirty)
		{
			UpdateEatingProcessesByMembers();
		}
		return food.IsEatable(ExtractionResultsInConsumable, ConsumeProcesses);
	}

	public void Destroy()
	{
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		ConsumeProcesses = sn.DoDictionary(ConsumeProcesses);
		extractionProcessesAreDirty = sn.DoBool(extractionProcessesAreDirty);
		ExtractionResultsInConsumable = sn.DoMultiMapHashSet(ExtractionResultsInConsumable);
		FoodExtractionProcesses = sn.DoMultiMapHashSet(FoodExtractionProcesses);
		parentID = sn.DoEnum(parentID);
		foodItemsGroup = sn.DoEnumNullable(foodItemsGroup);
		IncludeNonIndependentMembers = sn.DoBool(IncludeNonIndependentMembers);
		sn.Ignore(parent);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		parent = LookUpICanIterateEntities.FindByID(parentID);
	}
}
