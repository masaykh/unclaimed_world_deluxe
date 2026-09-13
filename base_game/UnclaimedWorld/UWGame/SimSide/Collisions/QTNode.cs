using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Collisions;

public class QTNode<T>
{
	public delegate void MapSizeChangeDelegate(CollideShape2D newSize);

	protected CollideShape2D bounds;

	protected int maxNodeCollidablesBeforePartition;

	protected bool isPartitioned;

	protected QTNode<T> parentNode;

	protected QTNode<T> topLeftNode;

	protected QTNode<T> topRightNode;

	protected QTNode<T> bottomLeftNode;

	protected QTNode<T> bottomRightNode;

	protected List<Collidable<T>> collidables;

	protected MapSizeChangeDelegate MapResize;

	public CollideShape2D Bounds
	{
		get
		{
			return bounds;
		}
		protected set
		{
			bounds = value;
		}
	}

	public QTNode(QTNode<T> parentNode, CollideShape2D rect, int maxCollidablesPerNode)
	{
		this.parentNode = parentNode;
		Bounds = rect;
		maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
		isPartitioned = false;
		collidables = new List<Collidable<T>>();
	}

	public QTNode(CollideShape2D rect, int maxCollidablesPerNode, MapSizeChangeDelegate mapResize)
	{
		parentNode = null;
		Bounds = rect;
		maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
		MapResize = mapResize;
		isPartitioned = false;
		collidables = new List<Collidable<T>>();
	}

	public void Insert(Collidable<T> collidable)
	{
		if (!InsertInChild(collidable))
		{
			collidable.Destroy += CollidableDestroy;
			collidable.Move += CollidableMove;
			collidables.Add(collidable);
			if (!isPartitioned && collidables.Count >= maxNodeCollidablesBeforePartition)
			{
				Partition();
			}
		}
	}

	protected bool InsertInChild(Collidable<T> collidable)
	{
		if (!isPartitioned)
		{
			return false;
		}
		if (topLeftNode.ContainsRect(collidable.Bounds))
		{
			topLeftNode.Insert(collidable);
		}
		else if (topRightNode.ContainsRect(collidable.Bounds))
		{
			topRightNode.Insert(collidable);
		}
		else if (bottomLeftNode.ContainsRect(collidable.Bounds))
		{
			bottomLeftNode.Insert(collidable);
		}
		else
		{
			if (!bottomRightNode.ContainsRect(collidable.Bounds))
			{
				return false;
			}
			bottomRightNode.Insert(collidable);
		}
		return true;
	}

	public bool PushCollidableDown(int i)
	{
		if (InsertInChild(collidables[i]))
		{
			RemoveCollidable(i);
			return true;
		}
		return false;
	}

	public void PushCollidableUp(int i)
	{
		Collidable<T> collidable = collidables[i];
		RemoveCollidable(i);
		parentNode.Insert(collidable);
	}

	protected void Partition()
	{
		Vector2 vector = Vector2.Divide(Vector2.Add(Bounds.BoundsUpperLeft, Bounds.BoundsLowerRight), 2f);
		topLeftNode = new QTNode<T>(this, new CollideShape2D(Bounds.BoundsUpperLeft, vector), maxNodeCollidablesBeforePartition);
		topRightNode = new QTNode<T>(this, new CollideShape2D(new Vector2(vector.X, Bounds.BoundsTop), new Vector2(Bounds.BoundsRight, vector.Y)), maxNodeCollidablesBeforePartition);
		bottomLeftNode = new QTNode<T>(this, new CollideShape2D(new Vector2(Bounds.BoundsLeft, vector.Y), new Vector2(vector.X, Bounds.BoundsBottom)), maxNodeCollidablesBeforePartition);
		bottomRightNode = new QTNode<T>(this, new CollideShape2D(vector, Bounds.BoundsLowerRight), maxNodeCollidablesBeforePartition);
		isPartitioned = true;
		int num = 0;
		while (num < collidables.Count)
		{
			if (!PushCollidableDown(num))
			{
				num++;
			}
		}
	}

	public void GetCollidablesContainingPoint(Vector2 Point, ICollection<Collidable<T>> collidablesFound)
	{
		if (!Bounds.ContainsPoint(Point))
		{
			return;
		}
		foreach (Collidable<T> collidable in collidables)
		{
			if (collidable.Bounds.ContainsPoint(Point))
			{
				collidablesFound.Add(collidable);
			}
		}
		if (isPartitioned)
		{
			topLeftNode.GetCollidablesContainingPoint(Point, collidablesFound);
			topRightNode.GetCollidablesContainingPoint(Point, collidablesFound);
			bottomLeftNode.GetCollidablesContainingPoint(Point, collidablesFound);
			bottomRightNode.GetCollidablesContainingPoint(Point, collidablesFound);
		}
	}

	public void GetCollidablesIntersectingBounds(CollideShape2D bounds, ref List<Collidable<T>> collidablesFound)
	{
		if (!this.bounds.isBoundsOverlap(bounds))
		{
			return;
		}
		foreach (Collidable<T> collidable in collidables)
		{
			if (collidable.Bounds.isBoundsOverlap(bounds))
			{
				if (collidablesFound == null)
				{
					collidablesFound = new List<Collidable<T>>();
				}
				collidablesFound.Add(collidable);
			}
		}
		if (isPartitioned)
		{
			topLeftNode.GetCollidablesIntersectingBounds(bounds, ref collidablesFound);
			topRightNode.GetCollidablesIntersectingBounds(bounds, ref collidablesFound);
			bottomLeftNode.GetCollidablesIntersectingBounds(bounds, ref collidablesFound);
			bottomRightNode.GetCollidablesIntersectingBounds(bounds, ref collidablesFound);
		}
	}

	public void GetAllICollidablesInNode(ref List<Collidable<T>> collidablesFound)
	{
		if (collidablesFound == null && collidables.Count > 0)
		{
			collidablesFound = new List<Collidable<T>>();
		}
		collidablesFound.AddRange(collidables);
		if (isPartitioned)
		{
			topLeftNode.GetAllICollidablesInNode(ref collidablesFound);
			topRightNode.GetAllICollidablesInNode(ref collidablesFound);
			bottomLeftNode.GetAllICollidablesInNode(ref collidablesFound);
			bottomRightNode.GetAllICollidablesInNode(ref collidablesFound);
		}
	}

	public QTNode<T> FindNodeContainingCollidable(Collidable<T> collidable)
	{
		if (collidables.Contains(collidable))
		{
			return this;
		}
		if (isPartitioned)
		{
			QTNode<T> qTNode = null;
			if (topLeftNode.ContainsRect(collidable.Bounds))
			{
				qTNode = topLeftNode.FindNodeContainingCollidable(collidable);
			}
			if (qTNode == null && topRightNode.ContainsRect(collidable.Bounds))
			{
				qTNode = topRightNode.FindNodeContainingCollidable(collidable);
			}
			if (qTNode == null && bottomLeftNode.ContainsRect(collidable.Bounds))
			{
				qTNode = bottomLeftNode.FindNodeContainingCollidable(collidable);
			}
			if (qTNode == null && bottomRightNode.ContainsRect(collidable.Bounds))
			{
				qTNode = bottomRightNode.FindNodeContainingCollidable(collidable);
			}
			return qTNode;
		}
		return null;
	}

	public void Destroy()
	{
		if (isPartitioned)
		{
			topLeftNode.Destroy();
			topRightNode.Destroy();
			bottomLeftNode.Destroy();
			bottomRightNode.Destroy();
			topLeftNode = null;
			topRightNode = null;
			bottomLeftNode = null;
			bottomRightNode = null;
		}
		while (collidables.Count > 0)
		{
			RemoveCollidable(0);
		}
	}

	public void RemoveCollidable(Collidable<T> collidable)
	{
		if (collidables.Contains(collidable))
		{
			collidable.Move -= CollidableMove;
			collidable.Destroy -= CollidableDestroy;
			collidables.Remove(collidable);
		}
	}

	protected void RemoveCollidable(int idx)
	{
		if (idx < collidables.Count)
		{
			collidables[idx].Move -= CollidableMove;
			collidables[idx].Destroy -= CollidableDestroy;
			collidables.RemoveAt(idx);
		}
	}

	public void CollidableMove(Collidable<T> collidable)
	{
		if (collidables.Contains(collidable))
		{
			int i = collidables.IndexOf(collidable);
			if (!PushCollidableDown(i))
			{
				if (parentNode != null)
				{
					PushCollidableUp(i);
				}
				else if (!ContainsRect(collidable.Bounds))
				{
					MapResize(new CollideShape2D(Vector2.Min(Bounds.BoundsUpperLeft, collidable.Bounds.BoundsUpperLeft) * 2f, Vector2.Max(Bounds.BoundsLowerRight, collidable.Bounds.BoundsLowerRight) * 2f));
				}
			}
		}
		else
		{
			collidable.Move -= CollidableMove;
		}
	}

	public void CollidableDestroy(Collidable<T> collidable)
	{
		RemoveCollidable(collidable);
	}

	public bool ContainsRect(CollideShape2D bnds)
	{
		if (bnds.BoundsUpperLeft.X >= Bounds.BoundsUpperLeft.X && bnds.BoundsUpperLeft.Y >= Bounds.BoundsUpperLeft.Y && bnds.BoundsLowerRight.X <= Bounds.BoundsLowerRight.X)
		{
			return bnds.BoundsLowerRight.Y <= Bounds.BoundsLowerRight.Y;
		}
		return false;
	}
}
