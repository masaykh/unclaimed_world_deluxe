using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Xclna.Xna.Animation;

public sealed class MultiBlendController : GameComponent, IAnimationController
{
	private Dictionary<IAnimationController, float> controllerDict;

	public Dictionary<IAnimationController, float> ControllerWeightDictionary => controllerDict;

	/// <summary>
	/// Required by IAnimationController; this controller never raises it (its raiser was never called,
	/// and went with the unused-code sweep). Explicit no-op accessors say so instead of a field that
	/// only looks live.
	/// </summary>
	public event EventHandler AnimationTracksChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	public MultiBlendController(Game game)
		: base(game)
	{
		game.Components.Add(this);
		controllerDict = new Dictionary<IAnimationController, float>();
	}

	public Matrix GetCurrentBoneTransform(BonePose pose)
	{
		if (controllerDict.Count == 0)
		{
			return pose.DefaultTransform;
		}
		Matrix result = default(Matrix);
		foreach (KeyValuePair<IAnimationController, float> item in controllerDict)
		{
			if (item.Key.ContainsAnimationTrack(pose))
			{
				Matrix currentBoneTransform = item.Key.GetCurrentBoneTransform(pose);
				result += Matrix.Multiply(currentBoneTransform, item.Value);
			}
			else
			{
				result += Matrix.Multiply(pose.DefaultTransform, item.Value);
			}
		}
		return result;
	}

	public bool ContainsAnimationTrack(BonePose pose)
	{
		return true;
	}
}
