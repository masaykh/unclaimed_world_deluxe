using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Entities.Containers;

public class ExitAndEntrance
{
	public static Vector3 GetRallyPoint(Container container, ExitDoor door = ExitDoor.NextAvailable)
	{
		Entity parent = container.Parent;
		Vector3 rallyPoint = parent.PlaySiteLocation;
		if (!GetNaturalRallyPoint(container, ref rallyPoint))
		{
			return rallyPoint;
		}
		Vector2[] doors = container.Parent.EntityType.ContainerType.GetDoors();
		bool hasCourtyard = container.Parent.EntityType.ContainerType.GetHasCourtyard();
		if (doors == null)
		{
			return rallyPoint;
		}
		if (door == ExitDoor.NextAvailable)
		{
			door = (ExitDoor)The.Sim.GameplayRandomGenerator.Next(doors.Length, "ExitAndEntrance");
		}
		if (door < ExitDoor.Max && doors != null && doors.Length > (int)door)
		{
			if (hasCourtyard)
			{
				Vector2 location = doors[(int)door] * 0.5f;
				if (parent.FlipHorizontally)
				{
					location.X *= -1f;
				}
				rallyPoint += location.ToVector3();
			}
			else
			{
				rallyPoint = parent.PlaySiteLocation;
				Vector2 location2 = doors[(int)door];
				location2 *= 1.4f;
				if (parent.FlipHorizontally)
				{
					location2.X *= -1f;
				}
				rallyPoint += location2.ToVector3();
			}
		}
		return rallyPoint;
	}

	public static bool GetNaturalRallyPoint(Container container, ref Vector3 rallyPoint, bool offset = true)
	{
		bool hasCourtyard = container.Parent.EntityType.ContainerType.GetHasCourtyard();
		rallyPoint = container.Parent.PlaySiteLocation;
		if (hasCourtyard)
		{
			return true;
		}
		float num = Math.Min(2, container.Parent.EntityType.StructureType.HeightInTiles) * 48;
		rallyPoint.Y += num;
		return true;
	}

	public static ExitDoor ReserveDoorForEntryOrExit(Entity container, Entity entity, bool exiting, ref ExitDoor simplifiedDoorToUseIndex)
	{
		Vector2[] doors = container.EntityType.ContainerType.GetDoors();
		if (++simplifiedDoorToUseIndex == (ExitDoor)doors.Length)
		{
			simplifiedDoorToUseIndex = ExitDoor.Door1;
		}
		return simplifiedDoorToUseIndex;
	}

	public static void GetDebugMarkers(Container container, ref bool preventRecursion)
	{
	}

	public static bool GetDoorPosition(Container container, ref Vector3 position, ref ExitDoor doorIndex, out ExitDoor doorThatWasUsed)
	{
		position = container.Parent.PlaySiteLocation;
		Vector2[] doors = container.Parent.EntityType.ContainerType.GetDoors();
		if (doorIndex == ExitDoor.NextAvailable)
		{
			doorIndex = ExitDoor.Door1;
			position = GetDoorPosition(container, doorIndex);
			doorThatWasUsed = doorIndex;
			return false;
		}
		if (doorIndex < ExitDoor.Max && doors != null && doors.Length > (int)doorIndex)
		{
			position = GetDoorPosition(container, doorIndex);
			doorThatWasUsed = doorIndex;
			return true;
		}
		doorThatWasUsed = doorIndex;
		return false;
	}

	public static Vector3 GetDoorPosition(Container container, Vector2 doorOffset)
	{
		if (container.Parent.EntityType.RenderableTypeMode.RenderAsModelType != null)
		{
			return GetRelativePointRotated(container.Parent, doorOffset).ToVector3();
		}
		Vector3 playSiteLocation = container.Parent.PlaySiteLocation;
		if (container.Parent.FlipHorizontally)
		{
			doorOffset.X *= -1f;
		}
		return playSiteLocation + doorOffset.ToVector3();
	}

	public static Vector3 GetDoorPosition(Container container, ExitDoor door)
	{
		Vector2 doorOffset = container.Parent.EntityType.ContainerType.GetDoors()[(int)door];
		return GetDoorPosition(container, doorOffset);
	}

	public static Vector2 GetRelativePointRotated(Entity container, Vector2? pointOnModel)
	{
		if (pointOnModel.HasValue)
		{
			Matrix matrix = Matrix.CreateRotationZ(container.Rotation);
			return new Vector2(container.Location.Value.X, container.Location.Value.Y) + Vector2.Transform(pointOnModel.Value, matrix);
		}
		return new Vector2(container.Location.Value.X, container.Location.Value.Y);
	}
}
