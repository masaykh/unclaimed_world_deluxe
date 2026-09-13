using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Inventory;

public class TrackTarget : ISnapshot
{
	public EntityType EntityType;

	public Color Color;

	public HashSet<EntityType> InputForTrackTarget;

	public HashSet<EntityType> OutputForTrackTarget;

	public HashSet<EntityType> ToolsForTrackTarget;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool ShowInputs { get; private set; }

	public bool ShowOutputs { get; private set; }

	public bool ShowTools { get; private set; }

	public bool IsSnapshotted { get; set; }

	public TrackTarget()
	{
	}

	public TrackTarget(EntityType entityType, bool showInputs, bool showOutputs, bool showTools)
	{
		EntityType = entityType;
		ShowInputs = showInputs;
		ShowOutputs = showOutputs;
		ShowTools = showTools;
		ComputeRelatedEntityTypes();
	}

	public void RecomputeRelatedEntityTypes()
	{
		InputForTrackTarget = null;
		OutputForTrackTarget = null;
		ToolsForTrackTarget = null;
		ComputeRelatedEntityTypes();
	}

	public void SetTrackingOptions(bool trackInputs, bool trackOutputs, bool trackTools)
	{
		bool flag = false;
		if (ShowOutputs != trackOutputs)
		{
			ShowOutputs = trackOutputs;
			flag = true;
		}
		if (ShowInputs != trackInputs)
		{
			ShowInputs = trackInputs;
			flag = true;
		}
		if (ShowTools != trackTools)
		{
			ShowTools = trackTools;
			flag = true;
		}
		if (flag)
		{
			The.InGameUI.InventorySettings.TrackTargetSettingsChanged();
		}
	}

	private void ComputeRelatedEntityTypes()
	{
		ComputeInputs();
		ComputeOutputs();
		ComputeTools();
	}

	private void ComputeInputs()
	{
		if (InputForTrackTarget == null)
		{
			InputForTrackTarget = new HashSet<EntityType>();
			HandleInput(EntityType);
		}
	}

	private void HandleInput(EntityType entityType)
	{
		if (!GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out var value))
		{
			return;
		}
		foreach (ProcessType item in value)
		{
			HandleProcessInput(item);
		}
	}

	private void HandleProcessInput(ProcessType process)
	{
		if (process.InputsByType == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, Input> item in process.InputsByType)
		{
			if (!InputForTrackTarget.Contains(item.Key))
			{
				InputForTrackTarget.Add(item.Key);
				HandleInput(item.Key);
			}
		}
	}

	private void ComputeOutputs()
	{
		if (OutputForTrackTarget == null)
		{
			OutputForTrackTarget = new HashSet<EntityType>();
			HandleOutput(EntityType);
			HandleAsToolOutput(EntityType);
		}
	}

	private void HandleOutput(EntityType entityType)
	{
		if (!GameData.Instance.ProcessesUsingThisInput.TryGetValue(entityType, out var value))
		{
			return;
		}
		foreach (ProcessType item in value)
		{
			HandleProcessOutput(item);
		}
	}

	private void HandleProcessOutput(ProcessType process)
	{
		if (process.Outputs == null)
		{
			return;
		}
		Output[] outputs = process.Outputs;
		foreach (Output output in outputs)
		{
			if (!OutputForTrackTarget.Contains(output.FinalEntityTypeToCreate))
			{
				OutputForTrackTarget.Add(output.FinalEntityTypeToCreate);
				HandleOutput(output.FinalEntityTypeToCreate);
			}
		}
	}

	private void HandleAsToolOutput(EntityType entityType)
	{
		foreach (KeyValuePair<string, ProcessType> allProcessType in GameData.Instance.AllProcessTypes)
		{
			if (allProcessType.Value.ProcessToolSet != null)
			{
				HandleProcessForOutputsUsingTool(entityType, allProcessType.Value);
			}
		}
	}

	private void HandleProcessForOutputsUsingTool(EntityType entityType, ProcessType process)
	{
		ToolAlternatives[] tools = process.ProcessToolSet.Tools;
		for (int i = 0; i < tools.Length; i++)
		{
			Tool[] tools2 = tools[i].Tools;
			for (int j = 0; j < tools2.Length; j++)
			{
				foreach (EntityType toolEntityType in tools2[j].ToolEntityTypes)
				{
					if (toolEntityType != entityType)
					{
						continue;
					}
					if (process.Outputs == null)
					{
						return;
					}
					Output[] outputs = process.Outputs;
					foreach (Output output in outputs)
					{
						if (!OutputForTrackTarget.Contains(output.FinalEntityTypeToCreate))
						{
							OutputForTrackTarget.Add(output.FinalEntityTypeToCreate);
						}
					}
					return;
				}
			}
		}
	}

	private void ComputeTools()
	{
		if (ToolsForTrackTarget == null)
		{
			ToolsForTrackTarget = new HashSet<EntityType>();
			HandleTools(EntityType);
		}
	}

	private void HandleTools(EntityType entityType)
	{
		if (!GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out var value))
		{
			return;
		}
		foreach (ProcessType item in value)
		{
			if (item.ProcessToolSet == null)
			{
				continue;
			}
			ToolAlternatives[] tools = item.ProcessToolSet.Tools;
			for (int i = 0; i < tools.Length; i++)
			{
				Tool[] tools2 = tools[i].Tools;
				for (int j = 0; j < tools2.Length; j++)
				{
					foreach (EntityType toolEntityType in tools2[j].ToolEntityTypes)
					{
						if (!ToolsForTrackTarget.Contains(toolEntityType))
						{
							ToolsForTrackTarget.Add(toolEntityType);
							HandleTools(toolEntityType);
						}
					}
				}
			}
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		ShowInputs = sn.DoBool(ShowInputs);
		ShowOutputs = sn.DoBool(ShowOutputs);
		ShowTools = sn.DoBool(ShowTools);
		Color = sn.DoColor(Color);
		EntityType = sn.DoGameData(EntityType);
		InputForTrackTarget = sn.DoHashSet(InputForTrackTarget);
		OutputForTrackTarget = sn.DoHashSet(OutputForTrackTarget);
		ToolsForTrackTarget = sn.DoHashSet(ToolsForTrackTarget);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
