using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Xclna.Xna.Animation;

public sealed class MultiBlendController : GameComponent, IAnimationController
{
	private Dictionary<IAnimationController, float> controllerDict;

	public Dictionary<IAnimationController, float> ControllerWeightDictionary => controllerDict;

	public event EventHandler AnimationTracksChanged;

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

	private void OnAnimationTracksChanged(EventArgs e)
	{
		if (this.AnimationTracksChanged != null)
		{
			this.AnimationTracksChanged(this, e);
		}
	}
}
