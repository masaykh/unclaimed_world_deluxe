using UWGame.ClientSide;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Screens;
using UWGame.SimSide;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame;

public static class The
{
	public static Sim Sim;

	public static MapManager Map;

	public static CollisionManager<Entity> CollisionManager;

	public static PointQuadTree<Entity> AgentQuadTree;

	public static UWGame.ClientSide.Client Client;

	public static InGameInterface InGameUI;

	public static MapClient MapUI;

	public static LoadingScreen LoadScreen;

	public static IngameLoadGameScreen IngameLoadScreen;

	public static Snapshotter Snapshotter;
}
