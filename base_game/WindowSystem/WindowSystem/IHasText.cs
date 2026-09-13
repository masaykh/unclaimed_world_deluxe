using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public interface IHasText
{
	SpriteFont Font { set; }

	Color NormalColor { set; }

	RenderType RenderType { set; }

	void Init(Label.LabelType labelType);
}
