using System.Collections.Generic;

namespace UWGame.SimSide.Entities;

public class EntityTypeDescription : IGameData
{
	public string EntityType;

	public string EntityName;

	public string SummaryDescription;

	public string Description;

	public string KeyName { get; set; }

	public bool DeleteRecord { get; set; }

	public string Name { get; set; }

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void Initialize()
	{
	}

	public void ApplyDescriptions(EntityType entityType)
	{
		if (EntityName != null)
		{
			entityType.Name = EntityName;
		}
		if (SummaryDescription != null)
		{
			entityType.SummaryDescription = SummaryDescription;
		}
		if (Description != null)
		{
			entityType.Description = Description;
		}
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
