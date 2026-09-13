using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xclna.Xna.Animation;

public class BonePoseCollection : ReadOnlyCollection<BonePose>
{
	private Dictionary<string, BonePose> boneDict = new Dictionary<string, BonePose>();

	public BonePose this[string boneName]
	{
		get
		{
			if (boneName == null)
			{
				return null;
			}
			if (boneDict.TryGetValue(boneName, out var value))
			{
				return value;
			}
			return null;
		}
	}

	internal BonePoseCollection(IList<BonePose> anims)
		: base(anims)
	{
		for (int i = 0; i < anims.Count; i++)
		{
			string name = anims[i].Name;
			if (name != null && name != "" && !boneDict.ContainsKey(name))
			{
				boneDict.Add(name, anims[i]);
			}
		}
	}

	internal static BonePoseCollection FromModelBoneCollection(ModelBoneCollection bones)
	{
		BonePose[] anims = new BonePose[bones.Count];
		for (int i = 0; i < bones.Count; i++)
		{
			if (bones[i].Parent == null)
			{
				new BonePose(bones[i], bones, anims);
			}
		}
		return new BonePoseCollection(anims);
	}

	public void CopyAbsoluteTransformsTo(Matrix[] transforms)
	{
		for (int i = 0; i < transforms.Length; i++)
		{
			if (i > 0)
			{
				Matrix currentTransform = base[i].GetCurrentTransform();
				Matrix matrix = transforms[base[i].Parent.Index];
				Vector3 translation = currentTransform.Translation;
				Matrix matrix2 = Matrix.CreateFromQuaternion(Quaternion.CreateFromRotationMatrix(matrix));
				Matrix matrix3 = Matrix.CreateFromQuaternion(Quaternion.CreateFromRotationMatrix(currentTransform));
				_ = Vector3.Transform(translation, matrix2) + matrix.Translation;
				_ = matrix.Translation + currentTransform.Translation;
				transforms[i] = matrix3 * matrix2;
				transforms[i] = currentTransform * matrix;
			}
			else
			{
				transforms[i] = base[i].GetCurrentTransform();
			}
		}
	}
}
