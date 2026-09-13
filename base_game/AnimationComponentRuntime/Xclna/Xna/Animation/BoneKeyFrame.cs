using Microsoft.Xna.Framework;

namespace Xclna.Xna.Animation;

public struct BoneKeyFrame
{
	public readonly Quaternion Rotation;

	public readonly Vector3 Translation;

	public readonly Vector3 Scaling;

	public readonly uint Time;

	public BoneKeyFrame(Quaternion rotation, Vector3 translation, Vector3 scaling, uint time)
	{
		Rotation = rotation;
		Scaling = scaling;
		Translation = translation;
		Time = time;
	}

	public Matrix ComputeMatrix()
	{
		return Matrix.CreateScale(Scaling) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Translation);
	}
}
