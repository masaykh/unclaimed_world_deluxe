using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Jobs.JobTypes;

[XmlInclude(typeof(ProcessJobType))]
[XmlInclude(typeof(StaticJobType))]
public abstract class JobType : IGameData
{
	public string Comments;

	public JobLabelTypes? LabelType;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public abstract bool IsType(Job job);

	public abstract string GetDefaultDisplayName();

	public abstract void IterateJobs(EntityGroup entityGroup, Action<Job> iterateFunction);

	public virtual void Initialize()
	{
	}

	public virtual void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public virtual void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public virtual void PostDataCompleteInitialize()
	{
	}

	public virtual void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public virtual void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
