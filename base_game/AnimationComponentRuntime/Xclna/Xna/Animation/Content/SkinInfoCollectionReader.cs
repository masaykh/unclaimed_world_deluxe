using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Xclna.Xna.Animation.Content;

internal class SkinInfoCollectionReader : ContentTypeReader<SkinInfoCollection>
{
	protected override SkinInfoCollection Read(ContentReader input, SkinInfoCollection existingInstance)
	{
		int num = input.ReadInt32();
		SkinInfo[] array = new SkinInfo[num];
		for (int i = 0; i < num; i++)
		{
			int boneIndex = input.ReadInt32();
			string name = input.ReadString();
			Matrix inverseBindPoseTransform = input.ReadMatrix();
			int paletteIndex = input.ReadInt32();
			array[i] = new SkinInfo(name, inverseBindPoseTransform, paletteIndex, boneIndex);
		}
		return new SkinInfoCollection(array);
	}
}
