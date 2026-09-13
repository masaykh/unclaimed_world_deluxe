using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xclna.Xna.Animation;

public class AnimationChannelCollection : ReadOnlyCollection<BoneKeyFrameCollection>
{
	private Dictionary<string, BoneKeyFrameCollection> dict = new Dictionary<string, BoneKeyFrameCollection>();

	private ReadOnlyCollection<string> affectedBones;

	public BoneKeyFrameCollection this[string boneName] => dict[boneName];

	internal ReadOnlyCollection<string> AffectedBones => affectedBones;

	internal AnimationChannelCollection(IList<BoneKeyFrameCollection> channels)
		: base(channels)
	{
		List<string> list = new List<string>();
		foreach (BoneKeyFrameCollection channel in channels)
		{
			dict.Add(channel.BoneName, channel);
			list.Add(channel.BoneName);
		}
		affectedBones = new ReadOnlyCollection<string>(list);
	}

	internal bool AffectsBone(string boneName)
	{
		return dict.ContainsKey(boneName);
	}
}
