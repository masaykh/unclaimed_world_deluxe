using Microsoft.Xna.Framework;

namespace Xclna.Xna.Animation;

public interface IAttachable
{
	Matrix LocalTransform { get; }

	Matrix CombinedTransform { get; set; }

	ModelAnimator ModelAnimator { get; set; }

	AttacheePoint? AttacheePointValue { get; set; }

	BonePose AttacheeBone { get; set; }

	BonePose AttachorBone { get; set; }

	object AttachedTo { get; set; }

	float Scale { get; }

	AttachPoint AttachorPoint { get; set; }
}
