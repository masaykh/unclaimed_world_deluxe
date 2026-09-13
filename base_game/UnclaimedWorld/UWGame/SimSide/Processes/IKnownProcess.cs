using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Processes;

public interface IKnownProcess
{
	SimProcessID ProcessID { get; }

	Point? MapPosition { get; set; }

	ProcessType ProcessType { get; }

	Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> AssignedInputs { get; set; }

	EntityID? ImmovableTool { get; set; }

	EntityID? ImmovableInput { get; set; }

	Vector3? GroundLocation { get; set; }

	EntityID? ContainerToPlaceOutputsIn { get; set; }

	List<EntityID> StationaryTools { get; set; }

	EntityAndRoot? ActingOnEntity { get; set; }

	UpgradeCategory UpgradeCategory { get; }

	Productivity Productivity { get; }

	bool IsStarted { get; }

	float ProgressSpeed { get; }

	List<EntityID> OutputEntities { get; }

	bool GetKnownProgress(SharedKnowledge sharedKnowledge, out float progress);

	bool GetCurrentLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false);

	bool GetFixedJobLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false);

	bool GetProductionSiteLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false);

	bool HasFixedLocation();

	bool IsCompleted(out bool isCompleted, SharedKnowledge sharedKnowledge);

	bool OutputExists(out bool outputExists, SharedKnowledge sharedKnowledge);
}
