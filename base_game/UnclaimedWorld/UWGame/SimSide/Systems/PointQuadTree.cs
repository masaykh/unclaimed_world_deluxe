using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems;

public class PointQuadTree<T> : ISnapshot
{
	protected PointQuadTreeNode<T> headNode;

	private RectangleF snapshotBounds;

	protected int maxNodeEntitiesBeforePartition;

	private int maxDepth;

	private Dictionary<T, PointTreeDweller<T>> containedObjects = new Dictionary<T, PointTreeDweller<T>>();

	private List<Pair<T, Vector2>> listToRebuildFromPostLoad = new List<Pair<T, Vector2>>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public RectangleF MapRect => headNode.Bounds;

	public bool IsSnapshotted { get; set; }

	public PointQuadTree()
	{
	}

	public PointQuadTree(RectangleF worldRect, int maxCollidablesPerNode, int maxDepth)
	{
		headNode = new PointQuadTreeNode<T>(worldRect, maxCollidablesPerNode, 0, maxDepth);
		maxNodeEntitiesBeforePartition = maxCollidablesPerNode;
		this.maxDepth = maxDepth;
	}

	public PointQuadTree(Vector2 size, int maxCollidablesPerNode, int maxDepth)
		: this(new RectangleF(0f, 0f, size.X, size.Y), maxCollidablesPerNode, maxDepth)
	{
	}

	public bool RemoveObject(T objectToRemove)
	{
		if (containedObjects.ContainsKey(objectToRemove))
		{
			headNode.RemoveObject(containedObjects[objectToRemove]);
			containedObjects.Remove(objectToRemove);
			return true;
		}
		return false;
	}

	public bool Contains(T objectToCheck)
	{
		return containedObjects.ContainsKey(objectToCheck);
	}

	public void AddObject(T objectToAdd, Vector2 point)
	{
		if (!headNode.EnvelopsPoint(point))
		{
			throw new Exception("pointObject wanted to be placed outside pointObject tree, currently this case is not handled!");
		}
		PointTreeDweller<T> pointTreeDweller = new PointTreeDweller<T>();
		Pair<T, Vector2> objectAndPosition = new Pair<T, Vector2>(objectToAdd, point);
		pointTreeDweller.ObjectAndPosition = objectAndPosition;
		containedObjects.Add(objectToAdd, pointTreeDweller);
		headNode.Insert(pointTreeDweller);
	}

	public bool UpdateObject(T objectToUpdate, Vector2 point)
	{
		if (containedObjects.TryGetValue(objectToUpdate, out var value))
		{
			if (!Common.IsLocationEqual(value.ObjectAndPosition.Second, point))
			{
				value.ObjectAndPosition.Second = point;
				if (!value.ContainingNode.EnvelopsPoint(value.ObjectAndPosition.Second))
				{
					value.ContainingNode.PushObjectUp(value);
				}
			}
			return true;
		}
		return false;
	}

	public void Resize(RectangleF newMapSize)
	{
		List<Pair<T, Vector2>> allObjectsAndPositions = GetAllObjectsAndPositions();
		RecreateFromList(newMapSize, allObjectsAndPositions);
	}

	private void RecreateFromList(RectangleF newMapSize, List<Pair<T, Vector2>> listOfObjects)
	{
		if (headNode != null)
		{
			headNode.Destroy();
			headNode = null;
		}
		containedObjects.Clear();
		headNode = new PointQuadTreeNode<T>(newMapSize, maxNodeEntitiesBeforePartition, 0, maxDepth);
		foreach (Pair<T, Vector2> listOfObject in listOfObjects)
		{
			AddObject(listOfObject.First, listOfObject.Second);
		}
	}

	public void GetObjectsIntersectingBounds(CollideShape2D bounds, Predicate<T> filter, ref List<Pair<T, Vector2>> resultsList)
	{
		headNode.GetObjectsIntersectingBounds(bounds, ref resultsList);
		if (resultsList == null)
		{
			return;
		}
		for (int num = resultsList.Count - 1; num >= 0; num--)
		{
			T first = resultsList[num].First;
			if (first == null || (filter != null && !filter(first)))
			{
				resultsList.RemoveAt(num);
			}
		}
	}

	public void GetAllObjects(ref List<PointTreeDweller<T>> pointObjectList)
	{
		headNode.GetAllObjectsInNode(ref pointObjectList);
	}

	public List<Pair<T, Vector2>> GetAllObjectsAndPositions()
	{
		return containedObjects.Select((KeyValuePair<T, PointTreeDweller<T>> o) => new Pair<T, Vector2>(o.Key, o.Value.ObjectAndPosition.Second)).ToList();
	}

	public void GetEntitiesInRange(Vector2 center, float radius, Predicate<T> filter, ref List<Pair<T, Vector2>> resultsList)
	{
		CollideShape2D bounds = new CollideShape2D(center, radius);
		GetObjectsIntersectingBounds(bounds, filter, ref resultsList);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		maxDepth = sn.DoInt32(maxDepth);
		maxNodeEntitiesBeforePartition = sn.DoInt32(maxNodeEntitiesBeforePartition);
		if (sn.mode == Snapshotter.Mode.Save)
		{
			snapshotBounds = MapRect;
		}
		snapshotBounds = sn.DoRectangleF(snapshotBounds);
		sn.Ignore(listToRebuildFromPostLoad);
		sn.Ignore(containedObjects);
		sn.Ignore(headNode);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		RecreateFromList(snapshotBounds, listToRebuildFromPostLoad);
	}

	public void SetPreLoadPostProcess(List<Pair<T, Vector2>> listToRebuildFrom)
	{
		listToRebuildFromPostLoad = listToRebuildFrom;
	}
}
