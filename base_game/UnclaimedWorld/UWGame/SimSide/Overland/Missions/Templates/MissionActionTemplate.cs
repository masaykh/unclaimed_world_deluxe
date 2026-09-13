using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions.Templates;

[XmlInclude(typeof(BuySellActionTemplate))]
[XmlInclude(typeof(LoadActionTemplate))]
[XmlInclude(typeof(UnloadActionTemplate))]
[XmlInclude(typeof(TravelActionTemplate))]
[XmlInclude(typeof(EmbarkActionTemplate))]
[XmlInclude(typeof(DisembarkActionTemplate))]
public abstract class MissionActionTemplate : ISnapshot, ILookUp<MissionActionTemplate, MissionActionTemplateID>
{
	public bool AllowDeleting;

	[XmlIgnore]
	private MissionStopTemplateID snapshotMissionStopTemplateID;

	[XmlIgnore]
	public MissionStopTemplate MissionStopTemplate;

	private MissionActionTemplateID id = MissionActionTemplateID.Invalid;

	private static MissionActionTemplateID IDCounter = MissionActionTemplateID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public abstract ActionTypes ActionType { get; }

	public abstract string Name { get; }

	[XmlIgnore]
	public MissionActionTemplateID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	[XmlIgnore]
	public bool IsSnapshotted { get; set; }

	public MissionActionTemplate(MissionStopTemplate missionStopTemplate, bool allowDeleting)
	{
		AllowDeleting = allowDeleting;
		MissionStopTemplate = missionStopTemplate;
	}

	public MissionActionTemplate()
	{
	}

	public abstract MissionAction CreateMissionAction(Mission mission);

	public abstract bool Validate(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors);

	public virtual void Destroy()
	{
		RemoveIDEntry();
	}

	public abstract decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost);

	public abstract float ComputeTotalCargoBulk();

	public static string GetName(ActionTypes action)
	{
		return action switch
		{
			ActionTypes.Buy => "Buy", 
			ActionTypes.Load => LoadActionTemplate.TemplateName, 
			ActionTypes.Unload => UnloadActionTemplate.TemplateName, 
			ActionTypes.Embark => EmbarkActionTemplate.TemplateName, 
			ActionTypes.Disembark => DisembarkActionTemplate.TemplateName, 
			ActionTypes.Sell => "Sell", 
			_ => null, 
		};
	}

	public virtual void AssignIDs()
	{
		AddToLookup();
	}

	public void SetParentID(MissionStopTemplate parent)
	{
		MissionStopTemplate = parent;
	}

	public static bool ValidateWorkingTerminal(IKnownEntityData terminalData, ref List<string> errors)
	{
		if (!terminalData.IsCompleted() || !Entity.IsFunctional(terminalData))
		{
			Common.AddToList(ref errors, "The terminal is not in a working state.");
			return false;
		}
		return true;
	}

	public MissionActionTemplateID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= MissionActionTemplateID.Invalid)
		{
			throw new Exception("Astounding, MissionActionTemplateID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != MissionActionTemplateID.Invalid)
		{
			LookUp<MissionActionTemplate, MissionActionTemplateID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = MissionActionTemplateID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<MissionActionTemplate, MissionActionTemplateID>.Remove(this);
	}

	void ILookUp<MissionActionTemplate, MissionActionTemplateID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = MissionActionTemplateID.First;
	}

	void ILookUp<MissionActionTemplate, MissionActionTemplateID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<MissionActionTemplate, MissionActionTemplateID>.Create();
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		IDCounter = sn.DoEnum(IDCounter);
		snapshotMissionStopTemplateID = sn.SnapshotID<MissionStopTemplate, MissionStopTemplateID>(MissionStopTemplate).Value;
		AllowDeleting = sn.DoBool(AllowDeleting);
		return this;
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		MissionStopTemplate = LookUp<MissionStopTemplate, MissionStopTemplateID>.FindByID(snapshotMissionStopTemplateID);
	}
}
