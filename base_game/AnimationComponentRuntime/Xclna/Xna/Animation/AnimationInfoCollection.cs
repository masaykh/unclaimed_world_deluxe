using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Xclna.Xna.Animation;

public class AnimationInfoCollection : SortedList<string, AnimationInfo>
{
	public AnimationInfo this[int index] => base.Values[index];

	internal AnimationInfoCollection()
	{
	}

	public static AnimationInfoCollection FromModel(Model model)
	{
		Dictionary<string, object> dictionary = (Dictionary<string, object>)model.Tag;
		if (dictionary == null || !dictionary.ContainsKey("Animations"))
		{
			return new AnimationInfoCollection();
		}
		return (AnimationInfoCollection)dictionary["Animations"];
	}
}
