using System;
using Microsoft.Xna.Framework;

namespace Xclna.Xna.Animation;

public interface IAnimationController
{
	event EventHandler AnimationTracksChanged;

	Matrix GetCurrentBoneTransform(BonePose pose);

	bool ContainsAnimationTrack(BonePose pose);
}
