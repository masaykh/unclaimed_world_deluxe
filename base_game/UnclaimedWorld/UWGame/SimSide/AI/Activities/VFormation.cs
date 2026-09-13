using Microsoft.Xna.Framework;

namespace UWGame.SimSide.AI.Activities;

public class VFormation : Formation
{
	private static Vector2 vLine;

	private float memberSpacing = 256f;

	public override int MaxMembers => -1;

	static VFormation()
	{
		vLine = new Vector2(-0.4f, -1f);
		vLine.Normalize();
	}

	public override Vector2 ComputePosition()
	{
		int num = 1 - FormationPositions.Count % 2 * 2;
		return memberSpacing * (float)(FormationPositions.Count / 2 + 1) * new Vector2(vLine.X, (float)num * vLine.Y);
	}
}
