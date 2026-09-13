using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xclna.Xna.Animation;

public class BoneKeyFrameCollection : ReadOnlyCollection<BoneKeyFrame>
{
	private string boneName;

	private uint duration;

	public uint Duration => duration;

	public string BoneName => boneName;

	internal BoneKeyFrameCollection(string boneName, IList<BoneKeyFrame> list)
		: base(list)
	{
		this.boneName = boneName;
		duration = list[list.Count - 1].Time;
	}

	public int GetIndexByTime(long ticks)
	{
		int i = (int)(ticks / 333333);
		if (i >= base.Count)
		{
			i = base.Count - 1;
		}
		for (; i < base.Count - 1 && base[i + 1].Time < ticks; i++)
		{
		}
		while (i >= 0 && base[i].Time > ticks)
		{
			i--;
		}
		return i;
	}
}
