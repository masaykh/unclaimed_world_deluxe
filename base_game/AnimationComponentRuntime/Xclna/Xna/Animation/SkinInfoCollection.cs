using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

namespace Xclna.Xna.Animation;

public class SkinInfoCollection : ReadOnlyCollection<SkinInfo>
{
	private SkinInfoCollection(Model model, SkinInfo[] info)
		: base((IList<SkinInfo>)info)
	{
	}

	internal SkinInfoCollection(IList<SkinInfo> info)
		: base(info)
	{
	}

	public static SkinInfoCollection FromModel(Model model)
	{
		return ((SkinInfoCollection[])((Dictionary<string, object>)model.Tag)["SkinInfo"])[0];
	}
}
