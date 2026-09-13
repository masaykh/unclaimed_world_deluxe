using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors;

public class Rotator : ISnapshot
{
	private float relativeRotation;

	public Locomotor Parent;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float RelativeRotation
	{
		get
		{
			return relativeRotation;
		}
		set
		{
			if (relativeRotation != value)
			{
				relativeRotation = value;
				SetBoneAngle();
			}
		}
	}

	public float AbsoluteRotation
	{
		get
		{
			return Parent.Parent.Rotation + relativeRotation;
		}
		set
		{
			relativeRotation = value - Parent.Parent.Rotation;
			SetBoneAngle();
		}
	}

	public bool IsSnapshotted { get; set; }

	public Rotator()
	{
	}

	public Rotator(Locomotor parent)
	{
		Parent = parent;
	}

	private void SetBoneAngle()
	{
		Matrix rotation = Matrix.CreateFromYawPitchRoll(0f - relativeRotation, 0f, 0f);
		Parent.Parent.Renderable.SetModelBoneRotation(Parent.Parent.EntityType.LocomotorType.RotatorType.BoneKeyName, rotation);
	}

	public void DoneRotating()
	{
		Parent.Parent.Renderable.StopOverridingAnimTransforms(Parent.Parent.EntityType.LocomotorType.RotatorType.BoneKeyName);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		relativeRotation = sn.DoFloat(relativeRotation);
		sn.Ignore(Parent);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
