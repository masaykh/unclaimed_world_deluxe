using Microsoft.Xna.Framework;

namespace Xclna.Xna.Animation;

public class InterpolationController : AnimationController
{
	private static Matrix curTransform;

	private static Matrix nextTransform;

	private static Matrix transform;

	public InterpolationController(AnimationInfo source)
		: base(source)
	{
	}

	public override Matrix GetCurrentBoneTransform(BonePose pose)
	{
		BoneKeyFrameCollection boneKeyFrameCollection = base.AnimationInfo.AnimationChannels[pose.Name];
		int indexByTime = boneKeyFrameCollection.GetIndexByTime(base.ElapsedTime);
		int num = indexByTime + 1;
		if (num >= boneKeyFrameCollection.Count)
		{
			return boneKeyFrameCollection[indexByTime].ComputeMatrix();
		}
		double num2 = base.ElapsedTime - boneKeyFrameCollection[indexByTime].Time;
		double num3 = boneKeyFrameCollection[num].Time - boneKeyFrameCollection[indexByTime].Time;
		double num4 = num2 / num3;
		BoneKeyFrame boneKeyFrame = boneKeyFrameCollection[indexByTime];
		BoneKeyFrame boneKeyFrame2 = boneKeyFrameCollection[num];
		Util.SlerpDecomposedMatrixValues(boneKeyFrame.Rotation, boneKeyFrame2.Rotation, boneKeyFrame.Translation, boneKeyFrame2.Translation, boneKeyFrame.Scaling, boneKeyFrame2.Scaling, (float)num4, out transform);
		return transform;
	}
}
