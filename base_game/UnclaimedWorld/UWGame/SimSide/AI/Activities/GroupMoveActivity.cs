using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Activities;

public class GroupMoveActivity : Activity
{
	public Entity Leader;

	private Matrix? leadersRotationMatrix;

	public Formation MoveFormation = new VFormation();

	public Vector3 Destination;

	private const float fullSpeedModifier = 2.2f;

	private const float minimumSpeedModifier = 0.5f;

	public Matrix LeadersRotationMatrix
	{
		get
		{
			if (leadersRotationMatrix.HasValue)
			{
				return leadersRotationMatrix.Value;
			}
			ComputeLeadersRotationMatrix();
			return leadersRotationMatrix.Value;
		}
	}

	public void ComputeLeadersRotationMatrix()
	{
		leadersRotationMatrix = Matrix.CreateRotationZ(Leader.Rotation);
	}

	public GroupMoveActivity(List<Activity> addToList, Vector3 destination)
		: base(addToList)
	{
		Destination = destination;
	}

	public bool IsLeader(Entity entity)
	{
		return Leader == entity;
	}

	public bool IsFollower(Entity entity)
	{
		if (Leader != entity)
		{
			return Members.Contains(entity);
		}
		return false;
	}

	public override void AddMember(Entity member)
	{
		base.AddMember(member);
		MoveFormation.FormationPositions.Add(member, MoveFormation.ComputePosition());
		MoveFormation.FormationPositionDrifts.Add(member, Vector2.Zero);
	}

	public override void LeaveActivity(Entity entity)
	{
		base.LeaveActivity(entity);
		Intelligence intelligence = entity.Intelligence;
		intelligence.GroupMoveAssignedWaypoint = null;
		intelligence.IsAtGroupMoveDestination = null;
		intelligence.FollowerStatus = FollowerStatus.Normal;
		MoveFormation.LeaveFormation(entity);
		if (Members.Count > 0 && Leader == entity)
		{
			Leader = Members[0];
		}
	}

	public bool AllAreReady()
	{
		foreach (Entity member in Members)
		{
			if (member.Intelligence.IsAtGroupMoveDestination != true)
			{
				return false;
			}
		}
		return true;
	}

	public bool EveryoneElseIsReady(Entity me)
	{
		foreach (Entity member in Members)
		{
			if (member != me && member.Intelligence.IsAtGroupMoveDestination != true)
			{
				return false;
			}
		}
		return true;
	}

	public Vector3 ComputePositionFromLeader(Entity member, Vector3 leadersPosition, Matrix rotMatrix)
	{
		Vector2 vector = MoveFormation.FormationPositions[member];
		Vector2 vector2 = MoveFormation.FormationPositionDrifts[member] + new Vector2(The.Sim.GameplayRandomGenerator.RandomBetween(-2f, 2f), The.Sim.GameplayRandomGenerator.RandomBetween(-2f, 2f));
		vector2.X = MathHelper.Clamp(vector2.X, -8f, 8f);
		vector2.Y = MathHelper.Clamp(vector2.Y, -8f, 8f);
		MoveFormation.FormationPositionDrifts[member] = vector2;
		vector = Vector2.TransformNormal(vector + vector2, rotMatrix);
		return The.Map.ClampWorldPosition(new Vector3(leadersPosition.X + vector.X, leadersPosition.Y + vector.Y, 0f));
	}

	public float ComputeSpeedModifier(Entity member)
	{
		Intelligence intelligence = member.Intelligence;
		if (intelligence.FollowerStatus == FollowerStatus.IsFollowingPathToWaypoint || intelligence.FollowerStatus == FollowerStatus.IsCatchingUp)
		{
			return 2.2f;
		}
		Vector2 vector = Leader.Location.Value.ToVector2();
		Vector2 vector2 = MoveFormation.FormationPositions[member];
		Vector2 vector3 = MoveFormation.FormationPositionDrifts[member];
		Vector2 value = Vector2.TransformNormal(vector2 + vector3, LeadersRotationMatrix) + vector - member.Location.Value.ToVector2();
		float num = value.Length();
		value /= num;
		float value2 = Vector2.Dot(value, new Vector2(Leader.FacingNormal.X, Leader.FacingNormal.Y));
		if (Math.Abs(value2) < 0.5f)
		{
			return 2.2f;
		}
		float num2;
		if (Math.Sign(value2) < 0)
		{
			num2 = MathHelper.Clamp(num / 40f, 0f, 0.5f);
			return 1f - num2;
		}
		num2 = MathHelper.Clamp(num / 40f, 0f, 2.2f);
		return 1f + num2;
	}
}
