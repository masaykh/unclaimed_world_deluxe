using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Renderables;

internal interface IUpdatable
{
	double? GetUpdateInterval();

	void Update(GameTime gameTime);
}
