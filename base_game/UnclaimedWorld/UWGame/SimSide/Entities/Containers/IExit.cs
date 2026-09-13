using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Entities.Containers;

public interface IExit
{
	bool DockOpen { get; }

	bool UsesRallyPointAfterUndock { get; }

	bool IsDoorAvailable();

	ExitDoor ReserveDoorForEntryOrExit(Entity entity, bool exiting);

	void UseDoor(Entity entity, ExitDoor door, bool exiting);

	void UnreserveDoor(ExitDoor door);

	void SetRallyPoint(Vector3 pos, ExitDoor door);

	Vector3 GetRallyPoint(ExitDoor door = ExitDoor.NextAvailable);

	bool GetNaturalRallyPoint(ref Vector3 rallyPoint, bool offset = true);

	bool GetDoorPosition(ref Vector3 position, bool exiting, out ExitDoor doorThatWasUsed, ExitDoor door = ExitDoor.NextAvailable);

	Vector3 ComputeAccessPoint();

	bool IsClearToApproach(Entity docker);

	bool AdvanceApproachPosition(ref Entity docker, ref Vector3 position, out int index);

	void GetDebugMarkers();

	bool IsClearToEnter(Entity docker);

	bool IsClearToAdvance(Entity docker, int dockerIndex);

	void GetEnterPosition(ref Entity docker, ref Vector3 position);

	void GetDockPosition(ref Entity docker, ref Vector3 position);

	void GetDeparturePosition(ref Entity docker, ref Vector3 position);

	void OnApproachRallyReached(ref Entity docker);

	void OnDockReached(ref Entity docker);

	void OnDepartureRallyReached(ref Entity docker);

	bool Action(ref Entity docker);

	void CancelDock(ref Entity docker);

	bool IsAllowedtoDock(ref Entity dockingEntity);
}
