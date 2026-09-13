using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class SteerableFrontWheels : Component
{
	private const float MaxWheelAngle = 0.8f;

	private const float MaxWheelSteeringAngleChange = 2f;

	public float WheelsAngle;

	private const string leftWheelName = "wheel_front_left_joint";

	private const string rightWheelName = "wheel_front_right_joint";

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public SteerableFrontWheels(Entity parent)
		: base(parent)
	{
		_ = parent.Renderable;
	}

	public SteerableFrontWheels()
	{
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		sn.DoFloat(WheelsAngle);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void SetAngle(float headingDifference, float elapsedTime)
	{
		float num = headingDifference - WheelsAngle;
		if (!Common.IsEqual(num, 0f))
		{
			float num2 = 2f * elapsedTime;
			num = MathHelper.Clamp(num, 0f - num2, num2);
			WheelsAngle += num;
			WheelsAngle = MathHelper.Clamp(WheelsAngle, -0.8f, 0.8f);
		}
		SetWheelAngle(WheelsAngle, "wheel_front_left_joint");
		SetWheelAngle(WheelsAngle, "wheel_front_right_joint");
	}

	private void SetWheelAngle(float rotate, string wheelBoneName)
	{
		Matrix rotation = Matrix.CreateFromYawPitchRoll(0f - rotate, 0f, 0f);
		Parent.Renderable.SetModelBoneRotation(wheelBoneName, rotation);
	}
}
