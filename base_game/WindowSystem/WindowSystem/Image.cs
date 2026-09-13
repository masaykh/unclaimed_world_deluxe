using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class Image : Icon
{
	public Texture2D Texture
	{
		set
		{
			if (GetSkin(0) == null)
			{
				SetSkinLocation(0, new Rectangle(0, 0, value.Width, value.Height));
			}
			ComponentSkin skin = GetSkin(0);
			skin.UseCustomSkin = true;
			skin.Skin = value;
			RefreshSkins();
		}
	}

	public Image(GUIManager guiManager)
		: base(guiManager)
	{
	}

	public void Reset()
	{
		base.CurrentSkin = -1;
		Redraw();
	}
}
