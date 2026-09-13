using UWGame.ClientSide.Map;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Entities;

internal interface IDrawnAsGroundSprite
{
	void CopyQuadToVertexBuffer(VertexGroundFeature[] featureVertices, ref int index, Renderable.AdditionalEffect? overridingEffectID = null);
}
