using System;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.AI.Activities;

public class LooseGroupFormation : Formation
{
	private float spacingBetweenTurns = 12f / (float)Math.PI;

	private float angleSpacingFactor = (float)Math.PI / 4f;

	private double anglePosition = 1.5707963705062866;

	public override int MaxMembers => -1;

	public override Vector2 ComputePosition()
	{
		float num = (float)anglePosition * spacingBetweenTurns + 48f;
		float num2 = (float)FormationPositions.Count * angleSpacingFactor;
		anglePosition += num2;
		return num * new Vector2((float)Math.Sin(anglePosition), (float)(0.0 - Math.Cos(anglePosition)));
	}
}
