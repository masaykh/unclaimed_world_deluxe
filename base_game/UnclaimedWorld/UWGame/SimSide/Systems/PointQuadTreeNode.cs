using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;

namespace UWGame.SimSide.Systems;

public class PointQuadTreeNode<T>
{
	public delegate void MapSizeChangeDelegate(CollideShape2D newSize);

	protected RectangleF bounds;

	protected int maxNodeCollidablesBeforePartition;

	protected bool isPartitioned;

	protected PointQuadTreeNode<T> parentNode;

	protected PointQuadTreeNode<T> topLeftNode;

	protected PointQuadTreeNode<T> topRightNode;

	protected PointQuadTreeNode<T> bottomLeftNode;

	protected PointQuadTreeNode<T> bottomRightNode;

	protected List<PointTreeDweller<T>> objectsInThisNode;

	private int depth;

	private int maxDepth;

	public RectangleF Bounds
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

	public PointQuadTreeNode(PointQuadTreeNode<T> parentNode, RectangleF rect, int maxCollidablesPerNode, int depth, int maxDepth)
	{
		this.parentNode = parentNode;
		Bounds = rect;
		maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
		isPartitioned = false;
		objectsInThisNode = new List<PointTreeDweller<T>>();
		this.depth = depth;
		this.maxDepth = maxDepth;
	}

	public PointQuadTreeNode(RectangleF rect, int maxCollidablesPerNode, int depth, int maxDepth)
	{
		parentNode = null;
		Bounds = rect;
		maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
		isPartitioned = false;
		objectsInThisNode = new List<PointTreeDweller<T>>();
		this.depth = depth;
		this.maxDepth = maxDepth;
	}

	public void Insert(PointTreeDweller<T> objectToInsert)
	{
		if (!InsertInChild(objectToInsert))
		{
			objectsInThisNode.Add(objectToInsert);
			objectToInsert.ContainingNode = this;
			if (!isPartitioned && objectsInThisNode.Count >= maxNodeCollidablesBeforePartition)
			{
				Partition();
			}
		}
	}

	protected bool InsertInChild(PointTreeDweller<T> pointObject)
	{
		if (!isPartitioned)
		{
			return false;
		}
		if (topLeftNode.EnvelopsPoint(pointObject.ObjectAndPosition.Second))
		{
			topLeftNode.Insert(pointObject);
		}
		else if (topRightNode.EnvelopsPoint(pointObject.ObjectAndPosition.Second))
		{
			topRightNode.Insert(pointObject);
		}
		else if (bottomLeftNode.EnvelopsPoint(pointObject.ObjectAndPosition.Second))
		{
			bottomLeftNode.Insert(pointObject);
		}
		else
		{
			if (!bottomRightNode.EnvelopsPoint(pointObject.ObjectAndPosition.Second))
			{
				return false;
			}
			bottomRightNode.Insert(pointObject);
		}
		return true;
	}

	public bool EnvelopsPoint(Vector2 point)
	{
		return bounds.Contains(point.X, point.Y);
	}

	public bool PushEntityDown(int i)
	{
		if (InsertInChild(objectsInThisNode[i]))
		{
			RemovePointObjectAtIndex(i);
			return true;
		}
		return false;
	}

	public bool PushObjectUp(PointTreeDweller<T> objectToPushUp)
	{
		bool num = RemoveObject(objectToPushUp);
		if (num)
		{
			parentNode.InsertOrPassUp(objectToPushUp, this);
		}
		return num;
	}

	private void InsertOrPassUp(PointTreeDweller<T> objectToInsert, PointQuadTreeNode<T> sender)
	{
		if (topLeftNode != sender && topLeftNode.EnvelopsPoint(objectToInsert.ObjectAndPosition.Second))
		{
			topLeftNode.Insert(objectToInsert);
			return;
		}
		if (topRightNode != sender && topRightNode.EnvelopsPoint(objectToInsert.ObjectAndPosition.Second))
		{
			topRightNode.Insert(objectToInsert);
			return;
		}
		if (bottomLeftNode != sender && bottomLeftNode.EnvelopsPoint(objectToInsert.ObjectAndPosition.Second))
		{
			bottomLeftNode.Insert(objectToInsert);
			return;
		}
		if (bottomRightNode != sender && bottomRightNode.EnvelopsPoint(objectToInsert.ObjectAndPosition.Second))
		{
			bottomRightNode.Insert(objectToInsert);
			return;
		}
		if (parentNode != null)
		{
			parentNode.InsertOrPassUp(objectToInsert, this);
			return;
		}
		throw new Exception("Object did not fit anywhere in the grid!");
	}

	protected void Partition()
	{
		if (depth >= maxDepth)
		{
			return;
		}
		Vector2 value = new Vector2(Bounds.Left, Bounds.Top);
		Vector2 value2 = new Vector2(Bounds.Right, Bounds.Bottom);
		Vector2 vector = Vector2.Divide(Vector2.Add(value, value2), 2f);
		float width = bounds.Width / 2f;
		float height = bounds.Height / 2f;
		topLeftNode = new PointQuadTreeNode<T>(this, new RectangleF(Bounds.X, Bounds.Y, width, height), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
		topRightNode = new PointQuadTreeNode<T>(this, new RectangleF(vector.X, Bounds.Y, width, height), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
		bottomLeftNode = new PointQuadTreeNode<T>(this, new RectangleF(Bounds.X, vector.Y, width, height), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
		bottomRightNode = new PointQuadTreeNode<T>(this, new RectangleF(vector.X, vector.Y, width, height), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
		isPartitioned = true;
		int num = 0;
		while (num < objectsInThisNode.Count)
		{
			if (!PushEntityDown(num))
			{
				num++;
			}
		}
	}

	public void GetObjectsIntersectingBounds(CollideShape2D bounds, ref List<Pair<T, Vector2>> foundObjects)
	{
		RectangleF rect = new RectangleF(bounds.BoundsLeft, bounds.BoundsTop, bounds.BoundsWidth, bounds.BoundsHeight);
		if (!this.bounds.IntersectsWith(rect))
		{
			return;
		}
		foreach (PointTreeDweller<T> item in objectsInThisNode)
		{
			if (bounds.ContainsPoint(item.ObjectAndPosition.Second))
			{
				if (foundObjects == null)
				{
					foundObjects = new List<Pair<T, Vector2>>();
				}
				foundObjects.Add(item.ObjectAndPosition);
			}
		}
		if (isPartitioned)
		{
			topLeftNode.GetObjectsIntersectingBounds(bounds, ref foundObjects);
			topRightNode.GetObjectsIntersectingBounds(bounds, ref foundObjects);
			bottomLeftNode.GetObjectsIntersectingBounds(bounds, ref foundObjects);
			bottomRightNode.GetObjectsIntersectingBounds(bounds, ref foundObjects);
		}
	}

	public void GetAllObjectsInNode(ref List<PointTreeDweller<T>> foundObjects)
	{
		if (objectsInThisNode.Count > 0)
		{
			if (foundObjects == null && objectsInThisNode.Count > 0)
			{
				foundObjects = new List<PointTreeDweller<T>>();
			}
			foundObjects.AddRange(objectsInThisNode);
		}
		if (isPartitioned)
		{
			topLeftNode.GetAllObjectsInNode(ref foundObjects);
			topRightNode.GetAllObjectsInNode(ref foundObjects);
			bottomLeftNode.GetAllObjectsInNode(ref foundObjects);
			bottomRightNode.GetAllObjectsInNode(ref foundObjects);
		}
	}

	public PointQuadTreeNode<T> FindNodeContainingEntity(PointTreeDweller<T> objectToFindNodeWith)
	{
		if (objectsInThisNode.Contains(objectToFindNodeWith))
		{
			return this;
		}
		if (isPartitioned)
		{
			PointQuadTreeNode<T> pointQuadTreeNode = null;
			if (topLeftNode.EnvelopsPoint(objectToFindNodeWith.ObjectAndPosition.Second))
			{
				pointQuadTreeNode = topLeftNode.FindNodeContainingEntity(objectToFindNodeWith);
			}
			if (pointQuadTreeNode == null && topRightNode.EnvelopsPoint(objectToFindNodeWith.ObjectAndPosition.Second))
			{
				pointQuadTreeNode = topRightNode.FindNodeContainingEntity(objectToFindNodeWith);
			}
			if (pointQuadTreeNode == null && bottomLeftNode.EnvelopsPoint(objectToFindNodeWith.ObjectAndPosition.Second))
			{
				pointQuadTreeNode = bottomLeftNode.FindNodeContainingEntity(objectToFindNodeWith);
			}
			if (pointQuadTreeNode == null && bottomRightNode.EnvelopsPoint(objectToFindNodeWith.ObjectAndPosition.Second))
			{
				pointQuadTreeNode = bottomRightNode.FindNodeContainingEntity(objectToFindNodeWith);
			}
			return pointQuadTreeNode;
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
		while (objectsInThisNode.Count > 0)
		{
			RemovePointObjectAtIndex(0);
		}
	}

	public bool RemoveObject(PointTreeDweller<T> objectToRemove)
	{
		if (objectsInThisNode.Contains(objectToRemove))
		{
			objectsInThisNode.Remove(objectToRemove);
			return true;
		}
		if (isPartitioned)
		{
			if (topLeftNode.RemoveObject(objectToRemove))
			{
				return true;
			}
			if (topRightNode.RemoveObject(objectToRemove))
			{
				return true;
			}
			if (bottomLeftNode.RemoveObject(objectToRemove))
			{
				return true;
			}
			if (bottomRightNode.RemoveObject(objectToRemove))
			{
				return true;
			}
		}
		return false;
	}

	protected void RemovePointObjectAtIndex(int index)
	{
		if (index < objectsInThisNode.Count)
		{
			objectsInThisNode.RemoveAt(index);
		}
	}
}
