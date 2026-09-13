using Microsoft.Xna.Framework;

namespace Xclna.Xna.Animation;

public struct SkinInfo
{
	public readonly string BoneName;

	public readonly Matrix InverseBindPoseTransform;

	public readonly int PaletteIndex;

	public readonly int BoneIndex;

	public SkinInfo(string name, Matrix inverseBindPoseTransform, int paletteIndex, int boneIndex)
	{
		BoneName = name;
		InverseBindPoseTransform = inverseBindPoseTransform;
		PaletteIndex = paletteIndex;
		BoneIndex = boneIndex;
	}
}
