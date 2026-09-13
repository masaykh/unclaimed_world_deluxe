using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Systems;

public class PointTreeDweller<T>
{
	public PointQuadTreeNode<T> ContainingNode;

	public Pair<T, Vector2> ObjectAndPosition;
}
