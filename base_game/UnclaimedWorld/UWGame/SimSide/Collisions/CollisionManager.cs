using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Collisions;

public class CollisionManager<T> : ISnapshot
{
	protected QTNode<T> headNode;

	private Vector2 snapshotBounds;

	protected int maxNodeCollidablesBeforePartition;

	private List<Collidable<T>> listToRebuildFromPostLoad;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public CollideShape2D MapRect => headNode.Bounds;

	public bool IsSnapshotted { get; set; }

	public CollisionManager()
	{
	}

	public CollisionManager(CollideShape2D worldRect, int maxCollidablesPerNode)
	{
		headNode = new QTNode<T>(worldRect, maxCollidablesPerNode, Resize);
		maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
	}

	public CollisionManager(Vector2 size, int maxCollidablesPerNode)
		: this(new CollideShape2D(Vector2.Zero, size), maxCollidablesPerNode)
	{
	}

	public void RemoveCollidable(Collidable<T> collidable)
	{
		headNode.RemoveCollidable(collidable);
		collidable.SetDisabled();
	}

	public void AddCollidable(Collidable<T> collidable)
	{
		if (!headNode.ContainsRect(collidable.Bounds))
		{
			Resize(new CollideShape2D(Vector2.Min(headNode.Bounds.BoundsUpperLeft, collidable.Bounds.BoundsUpperLeft) * 2f, Vector2.Max(headNode.Bounds.BoundsLowerRight, collidable.Bounds.BoundsLowerRight) * 2f));
		}
		headNode.Insert(collidable);
		collidable.SetEnabled();
	}

	public Collidable<T> AddCollidable(T parent, Vector2 position, Vector2 size)
	{
		Collidable<T> collidable = new Collidable<T>(parent, position, size);
		if (!headNode.ContainsRect(collidable.Bounds))
		{
			Resize(new CollideShape2D(Vector2.Min(headNode.Bounds.BoundsUpperLeft, collidable.Bounds.BoundsUpperLeft) * 2f, Vector2.Max(headNode.Bounds.BoundsLowerRight, collidable.Bounds.BoundsLowerRight) * 2f));
		}
		headNode.Insert(collidable);
		collidable.SetEnabled();
		return collidable;
	}

	public void Resize(CollideShape2D newMapSize)
	{
		List<Collidable<T>> collidablesList = new List<Collidable<T>>();
		GetAllCollidables(ref collidablesList);
		headNode.Destroy();
		headNode = null;
		headNode = new QTNode<T>(newMapSize, maxNodeCollidablesBeforePartition, Resize);
		foreach (Collidable<T> item in collidablesList)
		{
			headNode.Insert(item);
		}
	}

	public void GetCollidablesContainingPoint(Vector2 location, ICollection<Collidable<T>> resultsList)
	{
		headNode.GetCollidablesContainingPoint(location, resultsList);
	}

	public void GetCollidablesIntersectingBounds(CollideShape2D bounds, ref List<Collidable<T>> collidablesList)
	{
		headNode.GetCollidablesIntersectingBounds(bounds, ref collidablesList);
	}

	public void GetAllCollidables(ref List<Collidable<T>> collidablesList)
	{
		headNode.GetAllICollidablesInNode(ref collidablesList);
	}

	public void GetEntitiesInRange(Vector2 center, float radius, Predicate<T> filter, ref List<Collidable<T>> resultsList)
	{
		CollideShape2D bounds = new CollideShape2D(center, radius);
		GetCollidablesIntersectingBounds(bounds, ref resultsList);
		if (resultsList == null)
		{
			return;
		}
		for (int num = resultsList.Count - 1; num >= 0; num--)
		{
			Collidable<T> collidable = resultsList[num];
			if (collidable.Parent == null || !filter(collidable.Parent))
			{
				resultsList.RemoveAt(num);
			}
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		maxNodeCollidablesBeforePartition = sn.DoInt32(maxNodeCollidablesBeforePartition);
		if (sn.mode == Snapshotter.Mode.Save)
		{
			snapshotBounds = MapRect.BoundsLowerRight - MapRect.BoundsUpperLeft;
		}
		snapshotBounds = sn.DoVector2(snapshotBounds);
		sn.Ignore(listToRebuildFromPostLoad);
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
		CollideShape2D bounds = new CollideShape2D(Vector2.Zero, snapshotBounds);
		RecreateFromList(bounds, listToRebuildFromPostLoad);
	}

	private void RecreateFromList(CollideShape2D bounds, List<Collidable<T>> listOfObjects)
	{
		if (headNode != null)
		{
			headNode.Destroy();
			headNode = null;
		}
		headNode = new QTNode<T>(bounds, maxNodeCollidablesBeforePartition, Resize);
		foreach (Collidable<T> listOfObject in listOfObjects)
		{
			AddCollidable(listOfObject);
		}
	}

	public void SetPreLoadPostProcess(List<Collidable<T>> listToRebuildFrom)
	{
		listToRebuildFromPostLoad = listToRebuildFrom;
	}

	public List<T> GetAllObjects()
	{
		List<Collidable<T>> collidablesList = new List<Collidable<T>>();
		GetAllCollidables(ref collidablesList);
		return collidablesList.Select((Collidable<T> o) => o.Parent).ToList();
	}
}
