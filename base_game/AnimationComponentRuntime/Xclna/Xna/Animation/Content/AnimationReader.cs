using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Xclna.Xna.Animation.Content;

internal sealed class AnimationReader : ContentTypeReader<AnimationInfoCollection>
{
	protected override AnimationInfoCollection Read(ContentReader input, AnimationInfoCollection existingInstance)
	{
		AnimationInfoCollection animationInfoCollection = new AnimationInfoCollection();
		int num = input.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string text = input.ReadString();
			int num2 = input.ReadInt32();
			List<BoneKeyFrameCollection> list = new List<BoneKeyFrameCollection>();
			for (int j = 0; j < num2; j++)
			{
				string boneName = input.ReadString();
				int num3 = input.ReadInt32();
				List<BoneKeyFrame> list2 = new List<BoneKeyFrame>();
				for (int k = 0; k < num3; k++)
				{
					BoneKeyFrame item = new BoneKeyFrame(input.ReadQuaternion(), input.ReadVector3(), input.ReadVector3(), input.ReadUInt32());
					list2.Add(item);
				}
				BoneKeyFrameCollection item2 = new BoneKeyFrameCollection(boneName, list2);
				list.Add(item2);
			}
			AnimationInfo animationInfo = new AnimationInfo(text, new AnimationChannelCollection(list));
			animationInfo.ActionPointInSeconds = input.ReadSingle();
			animationInfo.StartOffset = input.ReadInt64();
			animationInfo.HideHandAttachments = input.ReadBoolean();
			animationInfo.IsGaitAnim = input.ReadBoolean();
			animationInfoCollection.Add(text, animationInfo);
		}
		return animationInfoCollection;
	}
}
