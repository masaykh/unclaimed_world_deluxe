using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Audio;
using UWGame.Client.Particles;
using UWGame.ClientSide;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Hints;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Interface.Layout;
using UWGame.ClientSide.Particles;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.AllGameData.EventHooks;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Skills;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Trade;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.XmlCollections;
using WindowSystem;

namespace UWGame.SimSide.AllGameData;

public class BaseDataLoader : DataLoader
{
	public const float NoDefenseRating = 0f;

	public const float VeryLowDefenseRating = 0.05f;

	public const float UnderLowDefenseRating = 0.1f;

	public const float LowDefenseRating = 0.15f;

	public const float MiddleDefenseRating = 0.25f;

	public const float AboveMiddleDefenseRating = 0.3f;

	public const float HighDefenseRating = 0.55f;

	public const float HighestDefenseRating = 0.8f;

	public static readonly Color ProgressColor = "#69E590".ColorFromHex();

	public static readonly Color ConsumeProgressColor = "#FFAB26".ColorFromHex();

	public BaseDataLoader()
		: base(Config.DataType.BaseData, 1f)
	{
	}

	protected override List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
	{
		return DetectionEventHooksLoader.InitDetectEntityTypeHooks();
	}

	protected override List<RenderableType> InitAttachableRenderableTypes()
	{
		List<RenderableType> list = new List<RenderableType>();
		RenderableType item = new RenderableType("box")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "box",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("hammer")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "hammer",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("backpack")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "backpack",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("backpackHeavy")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "backpackHeavy",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("farmingHoe")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "farmingHoe",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("rifle")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "rifle",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("spear")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "spear",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("machete")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "machete",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("pickaxe")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "pickaxe",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("axe")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "axe",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("shovel")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "shovel",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("knife")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "knife",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("armsling")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "armsling",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("watergun")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "watergun",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("watergunTank")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "watergunTank",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("bow")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "bow",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("tablet")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "tablet",
				ModelScale = 1f
			}
		};
		list.Add(item);
		item = new RenderableType("bush")
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "bush",
				ModelScale = 1f
			}
		};
		list.Add(item);
		return list;
	}

	private static void InitMiscEntityTypes(List<EntityType> listOfEntityTypes)
	{
		Entrance entrance = new Entrance
		{
			Offset = new Vector2(2f, -20f),
			ExitDoor = ExitDoor.Door1
		};
		Entrance entrance2 = new Entrance
		{
			Offset = new Vector2(2f, 20f),
			ExitDoor = ExitDoor.Door2
		};
		Entrance entrance3 = new Entrance
		{
			Offset = new Vector2(-24f, 0f),
			ExitDoor = ExitDoor.Door3
		};
		EntityType entityType = new EntityType("entity:utilityvehicle");
		entityType.Name = "Mule";
		entityType.FormalName = "";
		entityType.SummaryDescription = "All-terrain utility vehicle";
		entityType.ThumbnailBig = "skimmerDark";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "utilityvehicle",
				ModelScale = 2f
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 3f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				WalkSlowSpeed = 55f,
				WalkNormalSpeed = 55f,
				WalkFastSpeed = 70f
			}
		};
		entityType.ContainerType = new VehicleContainerType(4f)
		{
			UnladenWeight = 40f,
			LoadingRadius = 60f,
			Transport = SurfaceType.TransportType.OffRoad,
			MainFunction = VehicleContainerType.Function.Hauling,
			MaxAcceleration = 30f,
			Deceleration = 0.9f,
			AverageOverlandTravelSpeed = 600f,
			RequiresReplenishType = new RequiresReplenishType(),
			PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[4]
			{
				new PassengerOrCargoSlotType
				{
					AttachPointName = "DriversSeat",
					Entrance = entrance,
					PointToFaceAtEntrance = new Vector2(4f, -12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = true,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "FrontSeat",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "CargoLeft",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, -12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType()
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "CargoRight",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType()
				}
			}
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["mulevehicle"];
		EntityType item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("entity:barge");
		entityType.Name = "Barge";
		entityType.SummaryDescription = "Slow-moving river boat";
		entityType.ThumbnailBig = "skimmerDark";
		entityType.WorldMapIcon = "barge_map_icon";
		entityType.CommunicatorType = new CommunicatorType
		{
			Method = CommunicationMethod.Radio
		};
		entityType.SensorType = new SensorType
		{
			DetectionTypeKey = "communicationSensor",
			Range = 24f,
			RangeAtNight = 24f
		};
		entityType.IntelligenceType = new IntelligenceType
		{
			StrengthRating = StrengthRating.None,
			IsMobile = false,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = false,
			CanExamine = false,
			CanPatrol = false,
			CanHaul = false,
			CanDoJobs = false,
			ServantForEntityTypeTag = "servesHumans"
		};
		entityType.RenderableType = new RenderableType();
		entityType.ContainerType = new VehicleContainerType(60f)
		{
			VehicleType = VehicleContainerType.VehicleTypes.Boat,
			CanUseTerminal = TerminalType.TypesOfTerminal.Pier,
			CanNavigateRoutes = new RouteType[1] { RouteType.CalmWater },
			CanTransactWithTags = new string[1] { "humanTransact" },
			UnladenWeight = 40f,
			LoadingRadius = 240f,
			Transport = SurfaceType.TransportType.Foot,
			MaxAcceleration = 50f,
			MainFunction = VehicleContainerType.Function.Hauling,
			AverageOverlandTravelSpeed = 270f,
			PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[13]
			{
				new PassengerOrCargoSlotType
				{
					AttachPointName = "DriversSeat",
					Entrance = entrance,
					PointToFaceAtEntrance = new Vector2(4f, -12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = true,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot1",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot2",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot3",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot4",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot5",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot6",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot7",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot8",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "CargoLeft",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, -12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType
					{
						Capacity = 15f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "CargoRight",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType
					{
						Capacity = 15f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "Cargo1",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, 12f),
					CargoSlotType = new CargoSlotType
					{
						Capacity = 15f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "Cargo2",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, 12f),
					CargoSlotType = new CargoSlotType
					{
						Capacity = 15f
					}
				}
			},
			Deceleration = 1.3f
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("entity:advancedBarge");
		entityType.Name = "Barge";
		entityType.SummaryDescription = "Slow-moving river boat";
		entityType.ThumbnailBig = "skimmerDark";
		entityType.WorldMapIcon = "barge_map_icon";
		entityType.CommunicatorType = new CommunicatorType
		{
			Method = CommunicationMethod.Satellite
		};
		entityType.SensorType = new SensorType
		{
			DetectionTypeKey = "communicationSensor",
			Range = 24f,
			RangeAtNight = 24f
		};
		entityType.IntelligenceType = new IntelligenceType
		{
			StrengthRating = StrengthRating.None,
			IsMobile = false,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanExamine = false,
			CanScout = false,
			CanPatrol = false,
			CanHaul = false,
			CanDoJobs = false,
			ServantForEntityTypeTag = "servesHumans"
		};
		entityType.RenderableType = new RenderableType();
		entityType.ContainerType = new VehicleContainerType(60f)
		{
			VehicleType = VehicleContainerType.VehicleTypes.Boat,
			CanUseTerminal = TerminalType.TypesOfTerminal.Pier,
			CanNavigateRoutes = new RouteType[1] { RouteType.CalmWater },
			CanTransactWithTags = new string[1] { "humanTransact" },
			UnladenWeight = 40f,
			LoadingRadius = 240f,
			Transport = SurfaceType.TransportType.Foot,
			MaxAcceleration = 50f,
			MainFunction = VehicleContainerType.Function.Hauling,
			AverageOverlandTravelSpeed = 270f,
			PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[13]
			{
				new PassengerOrCargoSlotType
				{
					AttachPointName = "DriversSeat",
					Entrance = entrance,
					PointToFaceAtEntrance = new Vector2(4f, -12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = true,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot1",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot2",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot3",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot4",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot5",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot6",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot7",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot8",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "CargoLeft",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, -12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType
					{
						Capacity = 15f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "CargoRight",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType
					{
						Capacity = 15f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "Cargo1",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, 12f),
					CargoSlotType = new CargoSlotType
					{
						Capacity = 15f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "Cargo2",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, 12f),
					CargoSlotType = new CargoSlotType
					{
						Capacity = 15f
					}
				}
			},
			Deceleration = 1.3f
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("entity:smallBarge");
		entityType.Name = "Small barge";
		entityType.SummaryDescription = "Slow-moving river boat";
		entityType.ThumbnailBig = "skimmerDark";
		entityType.WorldMapIcon = "barge_map_icon";
		entityType.CommunicatorType = new CommunicatorType
		{
			Method = CommunicationMethod.Radio
		};
		entityType.SensorType = new SensorType
		{
			DetectionTypeKey = "communicationSensor",
			Range = 24f,
			RangeAtNight = 24f
		};
		entityType.IntelligenceType = new IntelligenceType
		{
			StrengthRating = StrengthRating.None,
			IsMobile = false,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = false,
			CanExamine = false,
			CanPatrol = false,
			CanHaul = false,
			CanDoJobs = false,
			ServantForEntityTypeTag = "servesHumans"
		};
		entityType.RenderableType = new RenderableType();
		entityType.ContainerType = new VehicleContainerType(40f)
		{
			VehicleType = VehicleContainerType.VehicleTypes.Boat,
			CanUseTerminal = TerminalType.TypesOfTerminal.Pier,
			CanNavigateRoutes = new RouteType[1] { RouteType.CalmWater },
			CanTransactWithTags = new string[1] { "humanTransact" },
			UnladenWeight = 40f,
			LoadingRadius = 240f,
			Transport = SurfaceType.TransportType.Foot,
			MaxAcceleration = 50f,
			MainFunction = VehicleContainerType.Function.Hauling,
			AverageOverlandTravelSpeed = 200f,
			PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[13]
			{
				new PassengerOrCargoSlotType
				{
					AttachPointName = "DriversSeat",
					Entrance = entrance,
					PointToFaceAtEntrance = new Vector2(4f, -12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = true,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot1",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot2",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot3",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot4",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot5",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot6",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot7",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "PassengerSpot8",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(4f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "CargoLeft",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, -12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType
					{
						Capacity = 10f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "CargoRight",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType
					{
						Capacity = 10f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "Cargo1",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, 12f),
					CargoSlotType = new CargoSlotType
					{
						Capacity = 10f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "Cargo2",
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-12f, 12f),
					CargoSlotType = new CargoSlotType
					{
						Capacity = 10f
					}
				}
			},
			Deceleration = 1.3f
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entrance = new Entrance
		{
			Offset = new Vector2(12f, -24f),
			ExitDoor = ExitDoor.Door1
		};
		entrance2 = new Entrance
		{
			Offset = new Vector2(12f, 24f),
			ExitDoor = ExitDoor.Door2
		};
		entrance3 = new Entrance
		{
			Offset = new Vector2(-40f, 0f),
			ExitDoor = ExitDoor.Door3
		};
		entityType = new EntityType("entity:skimmer");
		entityType.Name = "Skimmer";
		entityType.FormalName = "SK-40 'Skimmer'";
		entityType.SummaryDescription = "VTOL utility aircraft";
		entityType.ThumbnailBig = "skimmerDark";
		entityType.WorldMapIcon = "skimmer_map_icon";
		entityType.CommunicatorType = new CommunicatorType
		{
			Method = CommunicationMethod.Satellite
		};
		entityType.SensorType = new SensorType
		{
			DetectionTypeKey = "communicationSensor",
			Range = 24f,
			RangeAtNight = 24f
		};
		entityType.IntelligenceType = new IntelligenceType
		{
			StrengthRating = StrengthRating.None,
			IsMobile = false,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = false,
			CanExamine = false,
			CanPatrol = false,
			CanHaul = false,
			CanDoJobs = false,
			ServantForEntityTypeTag = "servesHumans"
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "skimmer",
				ModelScale = 1.3f
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 6f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				WalkSlowSpeed = 21f,
				WalkNormalSpeed = 20f,
				WalkFastSpeed = 25f
			}
		};
		entityType.ContainerType = new VehicleContainerType(8f)
		{
			VehicleType = VehicleContainerType.VehicleTypes.Aircraft,
			UnladenWeight = 40f,
			LoadingRadius = 240f,
			Transport = SurfaceType.TransportType.Air,
			MaxAcceleration = 50f,
			MainFunction = VehicleContainerType.Function.Hauling,
			AverageOverlandTravelSpeed = 3000f,
			Aircraft = new AircraftType
			{
				MaxAirSpeed = 180f,
				MaxRollDegreeWhenTurning = 0.7f,
				MaxVerticalAcceleration = 30f,
				MaxVerticalMoveSpeed = 90f,
				MaxPitchInRadians = (float)Math.PI / 12f,
				PitchChangeSpeed = (float)Math.PI / 6f,
				DuctChangeAngleSpeed = (float)Math.PI / 2f,
				MaxPropellerSpeed = 30f,
				PropellerAcceleration = 4f
			},
			CanUseTerminal = TerminalType.TypesOfTerminal.Helipad,
			RequiresReplenishType = new RequiresReplenishType(),
			Deceleration = 1.3f,
			PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[5]
			{
				new PassengerOrCargoSlotType
				{
					AttachPointName = "DriversSeat",
					Entrance = entrance,
					PointToFaceAtEntrance = new Vector2(12f, -12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = true,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "FrontSeat",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(12f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-15f, 0f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType()
				},
				new PassengerOrCargoSlotType
				{
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-15f, 0f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType()
				},
				new PassengerOrCargoSlotType
				{
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-15f, 0f),
					CargoSlotType = new CargoSlotType
					{
						Capacity = 4f
					}
				}
			}
		};
		entityType.NonLivingType = new NonLivingType
		{
			PartKeys = new SerializableDictionary<string, int>
			{
				{ "item:skimmerHull", 1 },
				{ "item:skimmerLandingGear", 1 },
				{ "item:skimmerWing", 2 },
				{ "item:skimmerCanopy", 1 },
				{ "item:skimmerMotor", 2 },
				{ "item:skimmerRotor", 2 }
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entrance = new Entrance
		{
			Offset = new Vector2(12f, -24f),
			ExitDoor = ExitDoor.Door1
		};
		entrance2 = new Entrance
		{
			Offset = new Vector2(12f, 24f),
			ExitDoor = ExitDoor.Door2
		};
		entrance3 = new Entrance
		{
			Offset = new Vector2(-40f, 0f),
			ExitDoor = ExitDoor.Door3
		};
		entityType = new EntityType("entity:harpy");
		entityType.Name = "Harpy";
		entityType.FormalName = "ML-40 'Harpy'";
		entityType.SummaryDescription = "Medium-lift VTOL aircraft";
		entityType.ThumbnailBig = "skimmerDark";
		entityType.WorldMapIcon = "skimmer_map_icon";
		entityType.CommunicatorType = new CommunicatorType
		{
			Method = CommunicationMethod.Satellite
		};
		entityType.SensorType = new SensorType
		{
			DetectionTypeKey = "communicationSensor",
			Range = 24f,
			RangeAtNight = 24f
		};
		entityType.IntelligenceType = new IntelligenceType
		{
			StrengthRating = StrengthRating.None,
			IsMobile = false,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = false,
			CanExamine = false,
			CanPatrol = false,
			CanHaul = false,
			CanDoJobs = false,
			ServantForEntityTypeTag = "servesHumans"
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "skimmer",
				ModelScale = 1.3f
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 6f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				WalkSlowSpeed = 21f,
				WalkNormalSpeed = 20f,
				WalkFastSpeed = 25f
			}
		};
		entityType.ContainerType = new VehicleContainerType(14f)
		{
			VehicleType = VehicleContainerType.VehicleTypes.Aircraft,
			UnladenWeight = 40f,
			LoadingRadius = 240f,
			Transport = SurfaceType.TransportType.Air,
			MaxAcceleration = 50f,
			MainFunction = VehicleContainerType.Function.Hauling,
			AverageOverlandTravelSpeed = 2400f,
			Aircraft = new AircraftType
			{
				MaxAirSpeed = 180f,
				MaxRollDegreeWhenTurning = 0.7f,
				MaxVerticalAcceleration = 30f,
				MaxVerticalMoveSpeed = 90f,
				MaxPitchInRadians = (float)Math.PI / 12f,
				PitchChangeSpeed = (float)Math.PI / 6f,
				DuctChangeAngleSpeed = (float)Math.PI / 2f,
				MaxPropellerSpeed = 30f,
				PropellerAcceleration = 4f
			},
			CanUseTerminal = TerminalType.TypesOfTerminal.Helipad,
			Deceleration = 1.3f,
			PassengerOrCargoSlotTypes = new PassengerOrCargoSlotType[5]
			{
				new PassengerOrCargoSlotType
				{
					AttachPointName = "DriversSeat",
					Entrance = entrance,
					PointToFaceAtEntrance = new Vector2(12f, -12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = true,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					AttachPointName = "FrontSeat",
					Entrance = entrance2,
					PointToFaceAtEntrance = new Vector2(12f, 12f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 1f
					}
				},
				new PassengerOrCargoSlotType
				{
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-15f, 0f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType()
				},
				new PassengerOrCargoSlotType
				{
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-15f, 0f),
					PassengerSlotType = new PassengerSlotType
					{
						IsDriversSeat = false,
						Comfort = 0.5f
					},
					CargoSlotType = new CargoSlotType()
				},
				new PassengerOrCargoSlotType
				{
					Entrance = entrance3,
					PointToFaceAtEntrance = new Vector2(-15f, 0f),
					CargoSlotType = new CargoSlotType
					{
						Capacity = 10f
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
	}

	protected override UWGame.SimSide.Constants InitGameConstants()
	{
		return new UWGame.SimSide.Constants();
	}

	protected override AIConstants InitAIConstants()
	{
		return new AIConstants();
	}

	protected override GUIConstants InitGUIConstants()
	{
		return new GUIConstants();
	}

	protected override List<SoilComponentType> InitSoilComponentTypes()
	{
		List<SoilComponentType> list = new List<SoilComponentType>();
		SoilComponentType item = new SoilComponentType("soil:groundrock")
		{
			Name = "Ground rock",
			MoveFactor = 0f,
			TextureName = "earth",
			WetTint = new Color(240, 240, 240),
			RenderWithPerlinNoise = RenderPerlinNoise.None,
			IsBaseTerrain = true
		};
		list.Add(item);
		item = new SoilComponentType("soil:muckroot")
		{
			Name = "Muckroot",
			TextureName = "muckroot",
			MoveFactor = 0.2f,
			RenderWithPerlinNoise = RenderPerlinNoise.ChannelRed,
			ScaleDisplayAmountAsWithRocks = true,
			RenderAsRocksType = new RenderAsRocksType
			{
				DepthMapTextureName = "linear gradient normal map"
			}
		};
		list.Add(item);
		item = new SoilComponentType("soil:clay")
		{
			Name = "Clay",
			MoveFactor = 0f,
			TextureName = "earth",
			DryTint = new Color(255, 240, 240),
			WetTint = new Color(178, 175, 165),
			RenderWithPerlinNoise = RenderPerlinNoise.None
		};
		list.Add(item);
		item = new SoilComponentType("soil:sand")
		{
			Name = "Sand",
			MoveFactor = 0.1f,
			TextureName = "sand",
			WetTint = new Color(180, 180, 180),
			RenderWithPerlinNoise = RenderPerlinNoise.None
		};
		list.Add(item);
		item = new SoilComponentType("soil:vulcanic")
		{
			Name = "Vulcanic",
			MoveFactor = 0.1f,
			TextureName = "vulcanic",
			WetTint = new Color(180, 180, 180),
			RenderWithPerlinNoise = RenderPerlinNoise.None
		};
		list.Add(item);
		item = new SoilComponentType("soil:seabed")
		{
			Name = "Seabed",
			TextureName = "seabed",
			WetTint = new Color(180, 180, 180),
			RenderWithPerlinNoise = RenderPerlinNoise.None
		};
		list.Add(item);
		item = new SoilComponentType("soil:deepseabed")
		{
			Name = "Deep Seabed",
			TextureName = "deepseabed",
			WetTint = new Color(180, 180, 180),
			RenderWithPerlinNoise = RenderPerlinNoise.None
		};
		list.Add(item);
		item = new SoilComponentType("soil:humus")
		{
			Name = "Humus",
			MoveFactor = 0f,
			TextureName = "humus",
			RenderWithPerlinNoise = RenderPerlinNoise.None
		};
		list.Add(item);
		item = new SoilComponentType("soil:rocks")
		{
			Name = "Rocks",
			TextureName = "rocks",
			MoveFactor = 0.5f,
			WetTint = new Color(180, 180, 180),
			RenderWithPerlinNoise = RenderPerlinNoise.ChannelGreen,
			RenderPerlinNoiseSharpness = 8f,
			NoiseScaling = 1f,
			ScaleDisplayAmountAsWithRocks = true,
			RenderAsRocksType = new RenderAsRocksType
			{
				DepthMapTextureName = "linear gradient normal map"
			}
		};
		list.Add(item);
		item = new SoilComponentType("soil:rockssandstone")
		{
			Name = "SandstoneRocks",
			TextureName = "rocks",
			MoveFactor = 0.35f,
			DryTint = new Color(213, 178, 154),
			WetTint = new Color(213, 178, 154),
			InvertNoise = true,
			RenderWithPerlinNoise = RenderPerlinNoise.ChannelBlue,
			RenderPerlinNoiseSharpness = 8f,
			NoiseScaling = 1f,
			ScaleDisplayAmountAsWithRocks = true,
			RenderAsRocksType = new RenderAsRocksType
			{
				DepthMapTextureName = "linear gradient normal map"
			}
		};
		list.Add(item);
		item = new SoilComponentType("soil:limestone")
		{
			Name = "LimestoneRocks",
			TextureName = "limestone",
			MoveFactor = 0.35f,
			WetTint = new Color(198, 198, 198),
			RenderWithPerlinNoise = RenderPerlinNoise.ChannelRed,
			RenderPerlinNoiseSharpness = 8f,
			NoiseScaling = 1f,
			ScaleDisplayAmountAsWithRocks = true,
			RenderAsRocksType = new RenderAsRocksType
			{
				DepthMapTextureName = "linear gradient normal map"
			}
		};
		list.Add(item);
		return list;
	}

	protected override List<LowVegetationType> InitLowVegetationTypes()
	{
		List<LowVegetationType> list = new List<LowVegetationType>();
		LowVegetationType item = new LowVegetationType("veg:muckrootthin")
		{
			Name = "Thin muckroot",
			TextureName = "muckrootthin",
			MoveFactor = 0.1f,
			InvertNoise = true,
			RenderWithPerlinNoise = RenderPerlinNoise.ChannelGreen
		};
		list.Add(item);
		item = new LowVegetationType("veg:firegrass")
		{
			Name = "Firegrass",
			TextureName = "firegrass",
			MoveFactor = 0.07f,
			RenderWithPerlinNoise = RenderPerlinNoise.ChannelGreen
		};
		list.Add(item);
		item = new LowVegetationType("veg:greengrass")
		{
			Name = "Grass",
			TextureName = "greengrass",
			MoveFactor = 0f,
			RenderWithPerlinNoise = RenderPerlinNoise.ChannelRed
		};
		list.Add(item);
		item = new LowVegetationType("veg:billowgrass")
		{
			Name = "Billowgrass",
			CanGrowUnderWater = true,
			TextureName = "billowgrass",
			MoveFactor = 0.1f,
			RenderWithPerlinNoise = RenderPerlinNoise.ChannelRed
		};
		list.Add(item);
		foreach (KeyValuePair<string, LowVegetationType> allLowVegetationType in GameData.Instance.AllLowVegetationTypes)
		{
			allLowVegetationType.Value.RenderOrder = RenderedTerrainType.HighestOrder;
			RenderedTerrainType.HighestOrder++;
			allLowVegetationType.Value.Initialize();
		}
		return list;
	}

	protected override List<SubstanceType> InitSubstanceTypes()
	{
		return SubstanceLoader.Init();
	}

	protected override List<IconInfo> InitIconInfo()
	{
		return new List<IconInfo>
		{
			new IconInfo
			{
				KeyName = "canister",
				CenterYPos = 8
			},
			new IconInfo
			{
				KeyName = "charcoal",
				CenterYPos = 11
			},
			new IconInfo
			{
				KeyName = "blackBox",
				CenterYPos = 11
			},
			new IconInfo
			{
				KeyName = "potGoldenEmpty",
				CenterYPos = 9
			},
			new IconInfo
			{
				KeyName = "pot",
				CenterYPos = 9
			},
			new IconInfo
			{
				KeyName = "woodenPotEmpty",
				CenterYPos = 9
			},
			new IconInfo
			{
				KeyName = "potGoldenFull",
				CenterYPos = 9
			},
			new IconInfo
			{
				KeyName = "potFull",
				CenterYPos = 9
			},
			new IconInfo
			{
				KeyName = "woodenPotFull",
				CenterYPos = 9
			},
			new IconInfo
			{
				KeyName = "clayPotSmall",
				CenterYPos = 9
			},
			new IconInfo
			{
				KeyName = "clayPot",
				CenterYPos = 12
			},
			new IconInfo
			{
				KeyName = "drum",
				CenterYPos = 11
			}
		};
	}

	protected override List<ResourceType> InitResourceTypes()
	{
		return ResourceLoader.Init();
	}

	protected override List<FoodNutrientType> InitNutrientTypes()
	{
		List<FoodNutrientType> list = new List<FoodNutrientType>();
		FoodNutrientType item = new FoodNutrientType("protein")
		{
			Name = "Protein"
		};
		list.Add(item);
		item = new FoodNutrientType("foodEnergy")
		{
			Name = "Calories"
		};
		list.Add(item);
		item = new FoodNutrientType("micronutrients")
		{
			Name = "Micronutrients"
		};
		list.Add(item);
		list.Add(new FoodNutrientType("stimulants")
		{
			Name = "Stimulants"
		});
		return list;
	}

	protected override List<PolledEventType> InitGlobalConditionalEvents()
	{
		return PolledEventsLoader.Init();
	}

	protected override List<ProcessToolSet> InitProcessToolSets()
	{
		return ToolsLoader.InitProcessToolSets();
	}

	protected override List<ProcessType> InitProcessTypes()
	{
		List<ProcessType> list = ProcessLoader.InitProcessTypes();
		// UNHIDDEN MOD: the makePeatCharcoal recipe.
		if (UWGame.Mods.UnhiddenMod.Enabled)
		{
			UWGame.Mods.UnhiddenMod.AddProcesses(list);
		}
		// DISASSEMBLY MOD: a Disassemble recipe per assembled tool, weapon and spear, generated
		// from the recipe that makes it. Here rather than earlier because it reads the finished
		// process table - including anything the Unhidden Mod just added - and the entity table,
		// which the queue built several steps ago.
		UWGame.Mods.DisassemblyMod.AddDisassembly(list);
		return list;
	}

	protected override List<ParticleSystemType> InitParticleSystems()
	{
		return ParticlesLoader.Init();
	}

	public static List<Hint> InitHints()
	{
		return new List<Hint>
		{
			new Hint
			{
				KeyName = "saveHint1",
				Priority = 10,
				Text = "Remember to save the game once in a while since there is no auto-save. Saving regularly can also benefit the game's performance and frame rate."
			},
			new Hint
			{
				KeyName = "saveHint2",
				Text = "Remember to save the game once in a while since there is no auto-save. Saving regularly can also benefit the game's performance and frame rate."
			},
			new Hint
			{
				KeyName = "saveHint3",
				Text = "Remember to save the game once in a while since there is no auto-save. Saving regularly can also benefit the game's performance and frame rate."
			},
			new Hint
			{
				KeyName = "saveHint4",
				Text = "Remember to save the game once in a while since there is no auto-save. Saving regularly can also benefit the game's performance and frame rate."
			},
			new Hint
			{
				KeyName = "saveHint5",
				Text = "Remember to save the game once in a while since there is no auto-save. Saving regularly can also benefit the game's performance and frame rate."
			},
			new Hint
			{
				KeyName = "maintenanceHint",
				Text = "Colonists will automatically do maintenance on structures if the necessary time and tools are available. This will postpone the structure breaking down. However, colonists will not do maintenance on items."
			},
			new Hint
			{
				KeyName = "verminHint",
				Text = "Some structures can keep out vermin while others can be accessed by animals. The data sheet for each structure type shows the details."
			},
			new Hint
			{
				KeyName = "filterHint",
				Text = "The top part of the Production panel contains filters that can be used for example to find objects that increase the COMFORT rating."
			},
			new Hint
			{
				KeyName = "ammoPolicyHint",
				Text = "The Policy panel has settings that restrict which ammunition types can be used against vermin."
			},
			new Hint
			{
				KeyName = "nestHint",
				Text = "Field quadite nests can be attacked with varmint bombs. This will prevent more field quadites from appearing for a long time."
			},
			new Hint
			{
				KeyName = "dogHint",
				Text = "The dog can live off a dead carcass for a long time. It can also consume rotten meat."
			},
			new Hint
			{
				KeyName = "shelterHint",
				Text = "Survival structures need a lot of maintenance. Buildings of a higher tier do not decay as rapidly but often require tools for maintenance - check the maintenance tasks when they appear in the Task Manager to make sure you have the needed tools."
			},
			new Hint
			{
				KeyName = "advancedTradeHint",
				Text = "In the time where 'Fields of Tau Ceti' takes place, Advanced Tech Tier items are rare but are sometimes offered for sale. They can appear and disappear off the market again."
			},
			new Hint
			{
				KeyName = "toolsHint",
				Text = "To increase efficiency, make sure you have enough tools and weapons so colonists don't have to share. Use the Task Manager panel to identify bottlenecks."
			},
			new Hint
			{
				KeyName = "majorityHint",
				Text = "To get a majority for raising the tech tier, colonists must have sufficiently high principles. Their principles will slowly increase if the colony has a higher rating. Read more tooltips on the Policy panel."
			},
			new Hint
			{
				KeyName = "sleepingHint",
				Text = "Characters that sleep outdoors will never have their sleep need fully satisfied and it will also fill at a slower rate than for people sleeping indoors."
			},
			new Hint
			{
				KeyName = "shootingHint",
				Text = "A character with Expert shooting skill gets an extra bonus at hitting a target."
			},
			new Hint
			{
				KeyName = "examineHint",
				Text = "The EXAMINE action is needed to find many types of resources, such as mineral deposits, farm plots, fish spots and vegetables which would otherwise remain hidden."
			}
		};
	}

	protected override List<JobType> InitJobTypes()
	{
		return new List<JobType>
		{
			new StaticJobType
			{
				KeyName = "examine",
				StaticJobTypeSetting = StaticJobTypes.Examine,
				LabelType = JobLabelTypes.Red
			},
			new StaticJobType
			{
				KeyName = "scout",
				StaticJobTypeSetting = StaticJobTypes.Scout,
				LabelType = JobLabelTypes.Red
			},
			new StaticJobType
			{
				KeyName = "patrol",
				StaticJobTypeSetting = StaticJobTypes.Patrol,
				LabelType = JobLabelTypes.Red
			},
			new StaticJobType
			{
				KeyName = "attackArea",
				StaticJobTypeSetting = StaticJobTypes.AttackArea,
				LabelType = JobLabelTypes.Red
			},
			new StaticJobType
			{
				Comments = "used for both Hunting and FindPrey jobs. From the player's perspective, they are the same.",
				KeyName = "hunt",
				StaticJobTypeSetting = StaticJobTypes.Hunt,
				LabelType = JobLabelTypes.Red
			},
			new StaticJobType
			{
				KeyName = "haulToStorage",
				StaticJobTypeSetting = StaticJobTypes.HaulToStorage,
				LabelType = JobLabelTypes.Brown
			},
			new StaticJobType
			{
				KeyName = "repairingJobType",
				StaticJobTypeSetting = StaticJobTypes.Repairing,
				LabelType = JobLabelTypes.Grey
			},
			new ProcessJobType
			{
				KeyName = "weedingAndFertilizingJobType",
				Name = "Weeding/Fertilizing",
				LabelType = JobLabelTypes.Grey
			},
			new ProcessJobType
			{
				KeyName = "sowingAndHarvestingJobType",
				Name = "Sowing/Harvesting",
				LabelType = JobLabelTypes.Green
			},
			new ProcessJobType
			{
				Comments = "does not include stimulants",
				KeyName = "cookingJobType",
				Name = "Cooking",
				LabelType = JobLabelTypes.Blue
			},
			new ProcessJobType
			{
				KeyName = "sentryReloadJobType",
				Name = "Reload sentry",
				LabelType = JobLabelTypes.Blue
			},
			new ProcessJobType
			{
				KeyName = "craftingJobType",
				Name = "Producing",
				LabelType = JobLabelTypes.SteelGrey
			},
			new ProcessJobType
			{
				KeyName = "butcheringJobType",
				Name = "Butchering",
				LabelType = JobLabelTypes.Green
			},
			new ProcessJobType
			{
				KeyName = "checkTrapsJobType",
				Name = "Check traps",
				LabelType = JobLabelTypes.Grey
			},
			new ProcessJobType
			{
				Comments = "includes turnip hut and fish traps",
				KeyName = "constructionJobType",
				Name = "Construction",
				LabelType = JobLabelTypes.Brown
			},
			new ProcessJobType
			{
				KeyName = "gatherFoodJobType",
				Name = "Gather food",
				LabelType = JobLabelTypes.Green
			},
			new ProcessJobType
			{
				KeyName = "gatherMaterialsJobType",
				Name = "Gather materials",
				LabelType = JobLabelTypes.Grey
			},
			new ProcessJobType
			{
				KeyName = "gatherFuelJobType",
				Name = "Gather fuel",
				LabelType = JobLabelTypes.Brown
			},
			new ProcessJobType
			{
				KeyName = "extractFromDepositJobType",
				Name = "Extract (deposit)",
				LabelType = JobLabelTypes.Grey
			}
		};
	}

	protected override List<FilterSettingType> InitFilterSettingTypes()
	{
		return new List<FilterSettingType>
		{
			new NutrientFilterSettingType
			{
				KeyName = "highProteinForHumans",
				ConsumableByEntity = "entity:human",
				Nutrient = "protein",
				Limit = 0.047f
			},
			new NutrientFilterSettingType
			{
				KeyName = "highCaloriesForHumans",
				ConsumableByEntity = "entity:human",
				Nutrient = "foodEnergy",
				Limit = 0.7f
			},
			new NutrientFilterSettingType
			{
				KeyName = "highMicronientsForHumans",
				ConsumableByEntity = "entity:human",
				Nutrient = "micronutrients",
				Limit = 0.003f
			},
			new NutrientFilterSettingType
			{
				KeyName = "highStimulantsForHumans",
				ConsumableByEntity = "entity:human",
				Nutrient = "stimulants",
				Limit = 0.02f
			},
			new StaticFilterSettingType
			{
				KeyName = "containers",
				StaticFilterSetting = StaticFilterSettings.Containers
			},
			new StaticFilterSettingType
			{
				KeyName = "storage",
				StaticFilterSetting = StaticFilterSettings.Storage
			},
			new StaticFilterSettingType
			{
				KeyName = "fuel",
				StaticFilterSetting = StaticFilterSettings.Fuel
			},
			new StaticFilterSettingType
			{
				KeyName = "betterTools",
				StaticFilterSetting = StaticFilterSettings.BetterTools
			},
			new StaticFilterSettingType
			{
				KeyName = "structures",
				StaticFilterSetting = StaticFilterSettings.Structures
			},
			new StaticFilterSettingType
			{
				KeyName = "items",
				StaticFilterSetting = StaticFilterSettings.Items
			},
			new StaticFilterSettingType
			{
				KeyName = "usableAsWeapon",
				StaticFilterSetting = StaticFilterSettings.UsableAsWeapon
			},
			new StaticFilterSettingType
			{
				KeyName = "affectsFoodRating",
				StaticFilterSetting = StaticFilterSettings.AffectsFoodRating
			},
			new StaticFilterSettingType
			{
				KeyName = "affectsComfortRating",
				StaticFilterSetting = StaticFilterSettings.AffectsComfortRating
			},
			new StaticFilterSettingType
			{
				KeyName = "affectsSecurityRating",
				StaticFilterSetting = StaticFilterSettings.AffectsSecurityRating
			},
			new CategoryFilterSettingType
			{
				KeyName = "ammunition",
				EntityCategory = "ammunition"
			},
			new CategoryFilterSettingType
			{
				KeyName = "waste",
				EntityCategory = "waste"
			},
			new CategoryFilterSettingType
			{
				KeyName = "preparedFood",
				EntityCategory = "preparedFood"
			},
			new CategoryFilterSettingType
			{
				KeyName = "ingredients",
				EntityCategory = "ingredients"
			},
			new CategoryFilterSettingType
			{
				KeyName = "rawMaterials",
				EntityCategory = "rawMaterials"
			},
			new CategoryFilterSettingType
			{
				KeyName = "weapons",
				EntityCategory = "weapons"
			},
			new CategoryFilterSettingType
			{
				KeyName = "tools",
				EntityCategory = "tools"
			},
			new TierFilterSettingType
			{
				KeyName = "survivalTier",
				Tier = "survival"
			},
			new TierFilterSettingType
			{
				KeyName = "basicTier",
				Tier = "basic"
			},
			new TierFilterSettingType
			{
				KeyName = "mediumTier",
				Tier = "medium"
			},
			new TierFilterSettingType
			{
				KeyName = "advancedTier",
				Tier = "advanced"
			},
			new PolicyAreaFilterSettingType
			{
				KeyName = "comfortPolicy",
				RatingType = RatingTypes.Comfort
			},
			new PolicyAreaFilterSettingType
			{
				KeyName = "foodPolicy",
				RatingType = RatingTypes.Food
			},
			new PolicyAreaFilterSettingType
			{
				KeyName = "securityPolicy",
				RatingType = RatingTypes.Security
			}
		};
	}

	protected override List<PresentationTypeCategory> InitPresentationTypeCategories()
	{
		List<PresentationTypeCategory> list = new List<PresentationTypeCategory>();
		list.Add(new PresentationTypeCategory
		{
			KeyName = "occupantsCategory",
			Name = "OCCUPANTS",
			PanelSortOrder = 0,
			Nodes = new GroupNode[1]
			{
				new GroupNode
				{
					SortOrder = 0,
					SetCountAsSummary = true,
					DynamicList = new DynamicList
					{
						HasPropertiesList = "contained",
						Filter = new PropertyCondition
						{
							PropertyKey = "isAgent",
							BoolValue = true
						},
						Presentation = new Presentation
						{
							PropertyNameForValue = "name",
							MakePropertyClickable = true
						}
					}
				}
			}
		});
		list.Add(new PresentationTypeCategory
		{
			KeyName = "healthStatusCategory",
			Name = "HEALTH STATUS",
			PanelSortOrder = 5,
			Nodes = new PresentationNode[5]
			{
				new GroupNode
				{
					SortOrder = 0,
					HasBorder = true,
					ShowConnectors = true,
					GroupHeader = new GroupHeader
					{
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								StaticString = "ENERGY"
							},
							CaptionTooltip = new StringSource
							{
								StaticString = "Energy affects work speed, combat ability and other activities."
							},
							PropertyNameForValue = "energyLevel",
							PresentationTypeKey = "energyPresentationSidePanel"
						}
					},
					Nodes = new PresentationNode[2]
					{
						new GroupNode
						{
							SortOrder = 1,
							ShowConnectors = true,
							GroupHeader = new GroupHeader
							{
								Presentation = new Presentation
								{
									Caption = new StringSource
									{
										StaticString = "NUTRITION"
									},
									CaptionTooltip = new StringSource
									{
										StaticString = "Evaluation of the intake of the 3 nutrient groups: Calories, protein and micronutrients."
									},
									PropertyNameForValue = "hungerStatus",
									PresentationTypeKey = "hungerPresentationSidePanel"
								}
							},
							Nodes = new PresentationNode[3]
							{
								new LeafNode
								{
									SortOrder = 1,
									Presentation = new Presentation
									{
										Caption = new StringSource
										{
											StaticString = "Calories"
										},
										CaptionTooltip = new StringSource
										{
											StaticString = "Represents the fuel needed by living organisms."
										},
										PropertyNameForValue = "foodEnergyLevel",
										PresentationTypeKey = "foodEnergyPresentation"
									}
								},
								new LeafNode
								{
									SortOrder = 2,
									Presentation = new Presentation
									{
										Caption = new StringSource
										{
											StaticString = "Protein"
										},
										CaptionTooltip = new StringSource
										{
											StaticString = "Protein is the building blocks of living cells. Meat is a primary source of protein."
										},
										PropertyNameForValue = "proteinLevel",
										PresentationTypeKey = "proteinPresentation"
									}
								},
								new LeafNode
								{
									SortOrder = 3,
									Presentation = new Presentation
									{
										Caption = new StringSource
										{
											StaticString = "Micronutrients"
										},
										CaptionTooltip = new StringSource
										{
											StaticString = "Important vitamins and minerals that the organism needs in small quantities."
										},
										PropertyNameForValue = "micronutrientsLevel",
										PresentationTypeKey = "micronutrientsPresentation"
									}
								}
							}
						},
						new GroupNode
						{
							SortOrder = 2,
							GroupHeader = new GroupHeader
							{
								Presentation = new Presentation
								{
									Caption = new StringSource
									{
										StaticString = "SLEEP"
									},
									CaptionTooltip = new StringSource
									{
										StaticString = "Sleep is needed by most animals. Without it, death will occur."
									},
									PropertyNameForValue = "sleepStatus",
									PresentationTypeKey = "sleepNeedPresentationSidePanel"
								}
							}
						}
					}
				},
				new GroupNode
				{
					SortOrder = 1,
					HasBorder = true,
					GroupHeader = new GroupHeader
					{
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								StaticString = "STIMULANTS"
							},
							CaptionTooltip = new StringSource
							{
								StaticString = "Not an essential need, but used in moderation, stimulants can give a boost to overall efficiency. \nAccess to stimulants improves people's happiness and the colony's comfort conditions"
							},
							PropertyNameForValue = "stimulantsLevel",
							PresentationTypeKey = "stimulantsPresentation"
						}
					}
				},
				new GroupNode
				{
					SortOrder = 2,
					HasBorder = true,
					ShowConnectors = true,
					GroupHeader = new GroupHeader
					{
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								StaticString = "INJURIES"
							},
							CaptionTooltip = new StringSource
							{
								StaticString = "Diagnosis of any physical injuries suffered."
							},
							PropertyNameForValue = "hitpointLevel",
							PresentationTypeKey = "hitpointsPresentation"
						}
					},
					DynamicList = new DynamicList
					{
						HasPropertiesList = "bodyParts",
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								UseDefaultName = true
							},
							PropertyNameForValue = "Injuries",
							PresentationTypeKey = "bodyPartCondition"
						}
					}
				},
				new GroupNode
				{
					SortOrder = 3,
					HasBorder = true,
					GroupHeader = new GroupHeader
					{
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								StaticString = "STANCE"
							},
							CaptionTooltip = new StringSource
							{
								StaticString = "Represents the willingness to move near danger or threats. Injuries or low morale can make persons stay away from danger."
							},
							PropertyNameForValue = "threatStance",
							PresentationTypeKey = "threatStancePresentation"
						}
					}
				},
				new GroupNode
				{
					SortOrder = 4,
					HasBorder = true,
					GroupHeader = new GroupHeader
					{
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								StaticString = "MORALE"
							},
							CaptionTooltip = new StringSource
							{
								StaticString = "Shows the person's morale. Low morale can cause panic and fleeing. High morale is needed to take the 'Fearless' stance and move into danger."
							},
							PropertyNameForValue = "moraleLevel",
							PresentationTypeKey = "moraleLevelPresentation"
						}
					}
				}
			}
		});
		list.Add(new PresentationTypeCategory
		{
			KeyName = "cropsCategory",
			Name = "CROPS",
			StartsAsExpanded = false,
			Nodes = new LeafNode[6]
			{
				new LeafNode
				{
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "PLANTED"
						},
						PropertyNameForValue = "cropTypeToSpawn",
						PresentationTypeKey = "entityTypePresentation"
					},
					SortOrder = 0
				},
				new LeafNode
				{
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = ""
						},
						PropertyNameForValue = "cropState"
					},
					SortOrder = 5
				},
				new LeafNode
				{
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "CROP SIZE"
						},
						CaptionTooltip = new StringSource
						{
							StaticString = "The current size of crops"
						},
						PropertyNameForValue = "cropGrowthProgress",
						PresentationTypeKey = "cropGrowthPresentation"
					},
					SortOrder = 10
				},
				new LeafNode
				{
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "WEEDS SIZE"
						},
						CaptionTooltip = new StringSource
						{
							StaticString = "The field should be weeded regularly to ensure high crop yield"
						},
						PropertyNameForValue = "weedGrowthProgress",
						PresentationTypeKey = "weedGrowthPresentation"
					},
					SortOrder = 15
				},
				new LeafNode
				{
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "SOIL FERTILITY"
						},
						CaptionTooltip = new StringSource
						{
							StaticString = "With every crop cycle, the soil quality will go down if not fertilized. Depleted soil will yield fewer crops"
						},
						PropertyNameForValue = "nutrientLevel",
						PresentationTypeKey = "soilNutrientLevelPresentation"
					},
					SortOrder = 20
				},
				new LeafNode
				{
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "HARVEST ON"
						},
						CaptionTooltip = new StringSource
						{
							StaticString = "Estimated date when crops are ready for harvesting"
						},
						PropertyNameForValue = "harvestDate"
					},
					SortOrder = 25
				}
			}
		});
		list.Add(new PresentationTypeCategory
		{
			KeyName = "statusCategory",
			Name = "STATUS",
			PanelSortOrder = 15,
			Nodes = new PresentationNode[9]
			{
				new GroupNode
				{
					SortOrder = 10,
					HasBorder = true,
					GroupHeader = new GroupHeader
					{
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								StaticString = "SIZE"
							},
							PropertyNameForValue = "itemBulkForPresentation",
							PresentationTypeKey = "itemBulkPresentation"
						}
					},
					Nodes = new PresentationNode[1]
					{
						new GroupNode
						{
							DynamicList = new DynamicList
							{
								HasPropertiesList = "SubstanceTypes",
								Presentation = new Presentation
								{
									Caption = new StringSource
									{
										UseDefaultName = true
									},
									PropertyNameForValue = "substanceLevel",
									PresentationTypeKey = "substancePresentation"
								}
							}
						}
					}
				},
				new GroupNode
				{
					SortOrder = 11,
					HasBorder = true,
					GroupHeader = new GroupHeader
					{
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								StaticString = "NUTRITION"
							},
							CaptionTooltip = new StringSource
							{
								StaticString = "Shows the percentage of an adult's daily needs this item will satisfy"
							}
						}
					},
					DynamicList = new DynamicList
					{
						HasPropertiesList = "nutrition",
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								UseDefaultName = true
							},
							PropertyNameForValue = "satisfiedDailyIntake"
						}
					}
				},
				new LeafNode
				{
					SortOrder = 12,
					Presentation = new Presentation
					{
						KeyNameForTypeDependentPresentationToUse = Presentation.KeyNameForTypeDependentPresentation.Custom,
						Caption = new StringSource
						{
							StaticString = "PROGRESS"
						},
						PropertyNameForValue = "ConstructionProgress",
						PresentationTypeKey = "ConstructionProgress"
					}
				},
				new GroupNode
				{
					SortOrder = 14,
					HasBorder = true,
					ShowConnectors = true,
					GroupHeader = new GroupHeader
					{
						Presentation = new Presentation
						{
							KeyNameForTypeDependentPresentationToUse = Presentation.KeyNameForTypeDependentPresentation.Custom,
							Caption = new StringSource
							{
								StaticString = "CONDITION"
							},
							CaptionTooltip = new StringSource
							{
								StaticString = "The overall condition of the item or structure depends on its parts and on its integrity."
							},
							PropertyNameForValue = "itemPartCondition",
							ValueTooltip = new StringSource
							{
								PropertyName = "itemPartConditionTooltip"
							},
							PresentationTypeKey = "PartConditions"
						}
					},
					Nodes = new PresentationNode[2]
					{
						new GroupNode
						{
							SortOrder = 1,
							GroupHeader = new GroupHeader
							{
								Presentation = new Presentation
								{
									Caption = new StringSource
									{
										StaticString = "INTEGRITY"
									},
									CaptionTooltip = new StringSource
									{
										StaticString = "The integrity of the item or structure, or how well it is holding its parts together."
									},
									PropertyNameForValue = "integrity",
									PresentationTypeKey = "integrityPresentation"
								}
							}
						},
						new GroupNode
						{
							SortOrder = 2,
							GroupHeader = new GroupHeader
							{
								Presentation = new Presentation
								{
									Caption = new StringSource
									{
										StaticString = "COMPONENT PARTS"
									},
									CaptionTooltip = new StringSource
									{
										StaticString = "The list of parts that this item or structure is composed of. Can often be retrieved by salvaging it."
									}
								}
							},
							DynamicList = new DynamicList
							{
								HasPropertiesList = "itemParts",
								Presentation = new Presentation
								{
									KeyNameForTypeDependentPresentationToUse = Presentation.KeyNameForTypeDependentPresentation.Custom,
									Caption = new StringSource
									{
										UseDefaultName = true
									},
									PropertyNameForValue = "itemPartCondition",
									PresentationTypeKey = "PartConditions",
									ValueTooltip = new StringSource
									{
										PropertyName = "itemPartConditionTooltip"
									},
									MakePropertyClickable = true
								}
							}
						}
					}
				},
				new LeafNode
				{
					SortOrder = 15,
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "DAYS LEFT"
						},
						CaptionTooltip = new StringSource
						{
							StaticString = "How long this item will last when stored under its current conditions"
						},
						PropertyNameForValue = "daysUntilBreakDown",
						PresentationTypeKey = "timeInDaysPresentation"
					}
				},
				new LeafNode
				{
					SortOrder = 16,
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "SHOTS REMAINING"
						},
						PropertyNameForValue = "ammoLevel",
						PresentationTypeKey = null
					}
				},
				new LeafNode
				{
					SortOrder = 17,
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "STORAGE CAPACITY"
						},
						PropertyNameForValue = "itemStorageFraction",
						CaptionTooltip = new StringSource
						{
							StaticString = "Capacity for stored/carried items"
						},
						PresentationTypeKey = "storageCapacityPresentation",
						ValueTooltip = new StringSource
						{
							PropertyName = "storageFractionTooltip"
						}
					}
				},
				new LeafNode
				{
					SortOrder = 19,
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "TRADE CAPACITY"
						},
						PropertyNameForValue = "tradeStorageFraction",
						CaptionTooltip = new StringSource
						{
							StaticString = "Storage capacity for items that are set for sale"
						},
						PresentationTypeKey = "storageCapacityPresentation",
						ValueTooltip = new StringSource
						{
							PropertyName = "tradeStorageFractionTooltip"
						}
					}
				},
				new LeafNode
				{
					SortOrder = 21,
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "COMFORT VALUE"
						},
						PropertyNameForValue = "homeComfortLevel",
						CaptionTooltip = new StringSource
						{
							StaticString = "The current comfort level of the dwelling, this is affected by its condition as well as any upgrades."
						},
						PresentationTypeKey = "percentagePresentation"
					}
				}
			}
		});
		list.Add(new PresentationTypeCategory
		{
			KeyName = "residentsCategory",
			Name = "RESIDENTS",
			PanelSortOrder = 17,
			Nodes = new GroupNode[1]
			{
				new GroupNode
				{
					SortOrder = 0,
					SetCountAsSummary = true,
					DynamicList = new DynamicList
					{
						HasPropertiesList = "residents",
						Presentation = new Presentation
						{
							PropertyNameForValue = "name",
							MakePropertyClickable = true
						}
					}
				}
			}
		});
		list.Add(new PresentationTypeCategory
		{
			KeyName = "skillsCategory",
			Name = "SKILLS",
			StartsAsExpanded = false,
			PanelSortOrder = 20,
			Nodes = new GroupNode[1]
			{
				new GroupNode
				{
					SortOrder = 8,
					DynamicList = new DynamicList
					{
						HasPropertiesList = "skills",
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								UseDefaultName = true
							},
							PropertyNameForValue = "skillLevel",
							CaptionTooltip = new StringSource
							{
								PropertyName = "skillDescription"
							},
							PresentationTypeKey = "skillPresentationSidePanel"
						}
					}
				}
			}
		});
		list.Add(new PresentationTypeCategory
		{
			KeyName = "opinionsCategory",
			Name = "OPINIONS",
			StartsAsExpanded = false,
			PanelSortOrder = 30,
			Nodes = new PresentationNode[2]
			{
				new LeafNode
				{
					SortOrder = 5,
					Presentation = new Presentation
					{
						Caption = new StringSource
						{
							StaticString = "RISK OF EMIGRATING"
						},
						PropertyNameForValue = "emigrateRiskForPlaySite",
						CaptionTooltip = new StringSource
						{
							StaticString = "The daily chance that the character will decide to leave the colony"
						},
						PresentationTypeKey = "emigrationRiskPresentation",
						ValueTooltipSettings = new TooltipSettings
						{
							DisableExpiry = true,
							TooltipWidth = 260
						},
						ValueTooltip = new StringSource
						{
							PropertyName = "emigrateRiskTooltip"
						}
					}
				},
				new GroupNode
				{
					SortOrder = 10,
					HasBorder = true,
					ShowConnectors = true,
					GroupHeader = new GroupHeader
					{
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								StaticString = "HAPPINESS"
							},
							PropertyNameForValue = "happiness",
							CaptionTooltip = new StringSource
							{
								StaticString = "Happiness is the difference between the character's principles and their personal conditions. If conditions are worse than their principles, they will be unhappy. If conditions are better, they are happy"
							},
							PresentationTypeKey = "happinessPresentation"
						}
					},
					Nodes = new PresentationNode[3]
					{
						new GroupNode
						{
							SortOrder = 1,
							GroupHeader = new GroupHeader
							{
								Presentation = new Presentation
								{
									Caption = new StringSource
									{
										StaticString = "FOOD"
									},
									PropertyNameForValue = "foodPrinciplesAndRating",
									PresentationTypeKey = "foodHappinessPresentation",
									ValueTooltip = new StringSource
									{
										PropertyName = "foodHappinessTooltip"
									},
									ValueTooltipSettings = new TooltipSettings
									{
										TooltipActivationProperty = "toggleFoodRatingTooltip",
										DisableExpiry = true,
										TooltipWidth = 260
									}
								}
							}
						},
						new GroupNode
						{
							SortOrder = 2,
							GroupHeader = new GroupHeader
							{
								Presentation = new Presentation
								{
									Caption = new StringSource
									{
										StaticString = "SECURITY"
									},
									PropertyNameForValue = "securityPrinciplesAndRating",
									PresentationTypeKey = "securityHappinessPresentation",
									ValueTooltip = new StringSource
									{
										PropertyName = "securityHappinessTooltip"
									},
									ValueTooltipSettings = new TooltipSettings
									{
										TooltipActivationProperty = "toggleSecurityRatingTooltip",
										DisableExpiry = true,
										TooltipWidth = 260
									}
								}
							}
						},
						new GroupNode
						{
							SortOrder = 3,
							ShowConnectors = true,
							GroupHeader = new GroupHeader
							{
								Presentation = new Presentation
								{
									Caption = new StringSource
									{
										StaticString = "COMFORT"
									},
									PropertyNameForValue = "comfortPrinciplesAndRating",
									PresentationTypeKey = "comfortHappinessPresentation",
									ValueTooltip = new StringSource
									{
										PropertyName = "comfortHappinessTooltip"
									},
									ValueTooltipSettings = new TooltipSettings
									{
										TooltipActivationProperty = "toggleComfortRatingTooltip",
										DisableExpiry = true,
										TooltipWidth = 260
									}
								}
							},
							DynamicList = new DynamicList
							{
								HasPropertiesList = "home",
								Presentation = new Presentation
								{
									Caption = new StringSource
									{
										StaticString = "Home"
									},
									PropertyNameForValue = "name",
									MakePropertyClickable = true
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PresentationTypeCategory
		{
			KeyName = "effectsCategory",
			Name = "EFFECTS",
			StartsAsExpanded = false,
			PanelSortOrder = 35,
			Nodes = new GroupNode[1]
			{
				new GroupNode
				{
					SortOrder = 0,
					Nodes = new PresentationNode[1]
					{
						new GroupNode
						{
							DynamicList = new DynamicList
							{
								HasPropertiesList = "effectProfiles",
								Presentation = new Presentation
								{
									PropertyNameForValue = "name",
									ValueTooltip = new StringSource
									{
										PropertyName = "description"
									}
								}
							},
							SortOrder = 1
						}
					}
				}
			}
		});
		return list;
	}

	protected override List<PresentationType> InitPresentationTypes()
	{
		List<PresentationType> list = new List<PresentationType>();
		list.Add(new PresentationType
		{
			KeyName = "totalProductivity",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[3]
				{
					new Threshold
					{
						Edge = 0f
					},
					new Threshold
					{
						Edge = 0.8f
					},
					new Threshold
					{
						Edge = 1f
					}
				}
			}
		});
		Color value = "#c6000e".ColorFromHex();
		Color value2 = "#FF4F5D".ColorFromHex();
		Color value3 = "#ffffff".ColorFromHex();
		Color value4 = "#38eaff".ColorFromHex();
		Color value5 = "#C45A63".ColorFromHex();
		Color value6 = "#C3F0F5".ColorFromHex();
		Color value7 = "#3a7177".ColorFromHex();
		Color value8 = "#59C24E".ColorFromHex();
		Color value9 = "#559DBA".ColorFromHex();
		Color value10 = "#C76098".ColorFromHex();
		Color value11 = "#9FF0FD".ColorFromHex();
		Color value12 = "#AAFFD2".ColorFromHex();
		Color value13 = "#C3FEE0".ColorFromHex();
		Color value14 = "#B9E1FE".ColorFromHex();
		Color value15 = "#AAFFD2".ColorFromHex();
		Color value16 = "#C3FEE0".ColorFromHex();
		Color value17 = "#CCB390".ColorFromHex();
		list.Add(new PresentationType
		{
			KeyName = "toolProductivity",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[4]
				{
					new Threshold
					{
						Edge = 0.2f,
						Term = "Tool is not very efficient",
						Icon = "HUD_icon_productivity_tool",
						IconTint = value
					},
					new Threshold
					{
						Edge = 0.5f,
						Term = "Tool is moderately efficient",
						Icon = "HUD_icon_productivity_tool"
					},
					new Threshold
					{
						Edge = 0.75f,
						Term = "Tool is highly efficient",
						Icon = "HUD_icon_productivity_tool",
						IconTint = value3
					},
					new Threshold
					{
						Edge = 1f,
						Term = "Best tool(s) for the job!",
						Icon = "HUD_icon_productivity_tool",
						IconTint = value4
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "ConstructionProgress",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[6]
				{
					new Threshold
					{
						Edge = 0f,
						Term = "Not started"
					},
					new Threshold
					{
						Edge = 0.1f,
						Term = "Started"
					},
					new Threshold
					{
						Edge = 0.4f,
						Term = "Shaping up"
					},
					new Threshold
					{
						Edge = 0.6f,
						Term = "Halfway there"
					},
					new Threshold
					{
						Edge = 0.99f,
						Term = "Near completion"
					},
					new Threshold
					{
						Edge = 1f,
						Term = null
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "ProcessProgress",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[6]
				{
					new Threshold
					{
						Edge = 0f,
						Term = "Not started"
					},
					new Threshold
					{
						Edge = 0.01f,
						Term = "Just started"
					},
					new Threshold
					{
						Edge = 0.3f,
						Term = "Under way"
					},
					new Threshold
					{
						Edge = 0.6f,
						Term = "Halfway there"
					},
					new Threshold
					{
						Edge = 0.995f,
						Term = "Almost finished"
					},
					new Threshold
					{
						Edge = 1f,
						Term = null
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "integrityPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[4]
				{
					new Threshold
					{
						Edge = 0.001f,
						TermTooltip = "Broken",
						IconTint = value,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.1f,
						TermTooltip = "Falling apart",
						IconTint = value,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.2f,
						TermTooltip = "Damaged",
						IconTint = value,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.99f,
						TermTooltip = "Good",
						IconTint = value7,
						TermTint = value6
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f,
				SuppressIfMaximumValue = true
			},
			RightAdjustValue = true,
			ValueRightPadding = 20
		});
		list.Add(new PresentationType
		{
			KeyName = "PartConditions",
			TypeDependentPresentation = new TypeDependentPresentation
			{
				ThresholdsByType = new SerializableDictionary<string, Threshold[]>
				{
					{
						"raw seafood",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "About to spoil",
								TermTooltip = "About to spoil. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Not fresh",
								TermTooltip = "Not fresh. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Fresh",
								TermTooltip = "Fresh. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen",
								TermTint = value6
							}
						}
					},
					{
						"raw meat",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "About to spoil",
								TermTooltip = "About to spoil. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Not fresh",
								TermTooltip = "Not fresh. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Fresh",
								TermTooltip = "Fresh. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen",
								TermTint = value6
							}
						}
					},
					{
						"cooked food",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "About to spoil",
								TermTooltip = "About to spoil. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Not fresh",
								TermTooltip = "Not fresh. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Freshly cooked",
								TermTooltip = "Freshly cooked. Will decompose quickly if not stored in a cool, dry and dark place, preferably frozen",
								TermTint = value6
							}
						}
					},
					{
						"perishable",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Starting to decompose",
								TermTooltip = "Starting to decompose. To preserve its condition, should be frozen or stored in cool, dry and dark surroundings",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition. To preserve its condition, should be frozen or stored in cool, dry and dark surroundings",
								TermTint = value6
							}
						}
					},
					{
						"perishable, no freeze",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Starting to decompose",
								TermTooltip = "Starting to decompose. To preserve its condition, should be stored in cool, dry and dark surroundings but NOT frozen",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition. To preserve its condition, should be stored in cool, dry and dark surroundings but NOT frozen",
								TermTint = value6
							}
						}
					},
					{
						"biowaste",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.15f,
								Term = "Almost fully decomposed",
								TermTooltip = "Almost fully decomposed. Will decompose quickly in warm, humid conditions and exposed to surroundings.",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Decomposing",
								TermTooltip = "Decomposing. Will decompose quickly in warm, humid conditions and exposed to surroundings.",
								TermTint = value6
							}
						}
					},
					{
						"somewhatPreserved",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Starting to decompose",
								TermTooltip = "Starting to decompose. This item has an increased shelf life, but it will not keep indefinitely if stored at room temperature.",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition. This item has an increased shelf life, but it will not keep indefinitely if stored at room temperature.",
								TermTint = value6
							}
						}
					},
					{
						"stored dry",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Decaying",
								TermTooltip = "Decaying. This item lasts longest under dry and dark conditions.",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition. This item will last long under dry and dark conditions.",
								TermTint = value6
							}
						}
					},
					{
						"pickledFood",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Decaying",
								IconTint = value,
								TermTooltip = "Decaying. This item lasts longest under dark conditions.",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition. This item lasts for considerable time under dark conditions.",
								TermTint = value6
							}
						}
					},
					{
						"wetDecay",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Almost decayed",
								TermTooltip = "Almost decayed. This item lasts longest under dry conditions.",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Starting to decay",
								TermTooltip = "Starting to decay. This item lasts longest under dry conditions.",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition. This item lasts longest under dry conditions.",
								TermTint = value6
							}
						}
					},
					{
						"carcassDecomposing",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.25f,
								Term = "Heavily decomposed",
								TermTooltip = "Heavily decomposed. Decomposition happens quicker under hot/humid conditions.",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.75f,
								Term = "Somewhat decomposed",
								TermTooltip = "Somewhat decomposed. Decomposition happens quicker under hot/humid conditions.",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Fresh",
								TermTooltip = "Fresh. Decomposition happens quicker under hot/humid conditions.",
								TermTint = value6
							}
						}
					},
					{
						"decomposedFood",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.5f,
								Term = "Heavily decomposed",
								TermTint = value5,
								TermTooltip = "Heavily decomposed"
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Somewhat decomposed",
								TermTint = value5,
								TermTooltip = "Somewhat decomposed"
							}
						}
					},
					{
						"sun drying",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Almost fully dried",
								TermTooltip = "Almost fully dried. This item should be sun dried by placing it in a dry place with lots of light.",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Somewhat dried",
								TermTooltip = "Somewhat dried. This item should be sun dried by placing it in a dry place with lots of light.",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Moist",
								TermTooltip = "Moist. This item should be sun dried by placing it in a dry place with lots of light.",
								TermTint = value6
							}
						}
					},
					{
						"fastDrying",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Almost fully dried",
								TermTooltip = "Almost fully dried. This item should be sun dried by placing it in a dry place with lots of light.",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Somewhat dried",
								TermTooltip = "Somewhat dried. This item should be sun dried by placing it in a dry place with lots of light.",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Moist",
								TermTooltip = "Moist. This item should be sun dried by placing it in a dry place with lots of light.",
								TermTint = value6
							}
						}
					},
					{
						"enzyme",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.05f,
								Term = "Degraded",
								TermTooltip = "Degraded. The chemical degrades quickly and should be used immediately",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Somewhat degraded",
								TermTooltip = "Somewhat degraded. The chemical degrades quickly and should be used immediately",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Fresh",
								TermTooltip = "Fresh. The chemical degrades quickly and should be used immediately",
								TermTint = value6
							}
						}
					},
					{
						"proneToInfestation",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.17f,
								Term = "Scuttler infested",
								TermTooltip = "Scuttler infested. Leaves have limited durability because they often attract 'scuttler bugs' that will eat the leaves.",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.5f,
								Term = "Some scuttler bugs",
								TermTooltip = "Some scuttler bugs. Leaves have limited durability because they often attract 'scuttler bugs' that will eat the leaves.",
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition. Leaves have limited durability because they often attract 'scuttler bugs' that will eat the leaves.",
								TermTint = value6
							}
						}
					},
					{
						"remainsOfSpoakLeaves",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0f,
								Term = "Eaten up by scuttlers",
								TermTooltip = "Eaten up by scuttlers. Leaves have limited durability because they often attract 'scuttler bugs' that will eat the leaves.",
								Icon = "HUD_icon_status_broken",
								IconTint = value,
								TermTint = value5
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Eaten up by scuttlers",
								TermTooltip = "Eaten up by scuttlers. Leaves have limited durability because they often attract 'scuttler bugs' that will eat the leaves.",
								Icon = "HUD_icon_status_broken",
								IconTint = value,
								TermTint = value5
							}
						}
					},
					{
						"dirt",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Weathered",
								TermTint = value5,
								TermTooltip = "Weathered"
							},
							new Threshold
							{
								Edge = 1f,
								Term = "",
								TermTint = value6
							}
						}
					},
					{
						"neverDegrades",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.99f,
								Term = "Signs of decay",
								TermTooltip = "Signs of decay",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition",
								IconTint = value7,
								TermTint = value6
							}
						}
					},
					{
						"ricketyConstruction",
						new Threshold[5]
						{
							new Threshold
							{
								Edge = 0f,
								Term = "In ruins",
								IconTint = value,
								Icon = "HUD_icon_status_broken",
								TermTooltip = "In ruins. This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.1f,
								Term = "About to fall down",
								IconTint = value,
								TermTooltip = "About to fall down. It may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Just holding together",
								IconTint = value7,
								TermTooltip = "Just holding together",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Signs of decay",
								TermTooltip = "Signs of decay",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition",
								IconTint = value7,
								TermTint = value6
							}
						}
					},
					{
						"adequateConstruction",
						new Threshold[5]
						{
							new Threshold
							{
								Edge = 0f,
								Term = "In ruins",
								IconTint = value,
								Icon = "HUD_icon_status_broken",
								TermTooltip = "This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.1f,
								Term = "About to fall down",
								IconTint = value,
								TermTooltip = "About to fall down. It may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Just holding together",
								TermTooltip = "Just holding together",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Signs of decay",
								TermTooltip = "Signs of decay",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition",
								IconTint = value7,
								TermTint = value6
							}
						}
					},
					{
						"sturdyConstruction",
						new Threshold[5]
						{
							new Threshold
							{
								Edge = 0f,
								Term = "In ruins",
								IconTint = value,
								Icon = "HUD_icon_status_broken",
								TermTooltip = "In ruins. This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.1f,
								Term = "About to fall down",
								IconTint = value,
								TermTooltip = "About to fall down. It may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Just holding together",
								TermTooltip = "Just holding together",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Signs of decay",
								TermTooltip = "Signs of decay",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition",
								IconTint = value7,
								TermTint = value6
							}
						}
					},
					{
						"advancedConstruction",
						new Threshold[5]
						{
							new Threshold
							{
								Edge = 0f,
								Term = "In ruins",
								IconTint = value,
								Icon = "HUD_icon_status_broken",
								TermTooltip = "In ruins. This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.1f,
								Term = "About to fall down",
								IconTint = value,
								TermTooltip = "About to fall down. It may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Just holding together",
								TermTooltip = "Just holding together",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Signs of decay",
								TermTooltip = "Signs of decay",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition",
								IconTint = value7,
								TermTint = value6
							}
						}
					},
					{
						"improvisedEquipment",
						new Threshold[5]
						{
							new Threshold
							{
								Edge = 0f,
								Term = "Broken",
								IconTint = value,
								Icon = "HUD_icon_status_broken",
								TermTooltip = "This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.1f,
								Term = "About to break down",
								IconTint = value,
								TermTooltip = "About to break down. It may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Heavy wear and tear",
								TermTooltip = "Heavy wear and tear",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 0.5f,
								Term = "Showing signs of use",
								TermTooltip = "Showing signs of use",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition",
								IconTint = value7,
								TermTint = value6
							}
						}
					},
					{
						"equipment",
						new Threshold[5]
						{
							new Threshold
							{
								Edge = 0f,
								Term = "Broken",
								IconTint = value,
								Icon = "HUD_icon_status_broken",
								TermTooltip = "This is broken and unusable for its purpose. However, it may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.1f,
								Term = "About to break down",
								IconTint = value,
								TermTooltip = "About to break down. It may sometimes be possible to salvage and reuse its component parts",
								TermTint = value5
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Heavy wear and tear",
								TermTooltip = "Heavy wear and tear",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 0.5f,
								Term = "Showing signs of use",
								TermTooltip = "Showing signs of use",
								IconTint = value7,
								TermTint = value6
							},
							new Threshold
							{
								Edge = 1f,
								Term = "Good condition",
								TermTooltip = "Good condition",
								IconTint = value7,
								TermTint = value6
							}
						}
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f,
				SuppressIfMaximumValue = false
			},
			RightAdjustValue = true,
			ValueRightPadding = 20
		});
		list.Add(new PresentationType
		{
			KeyName = "energyUsePresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[3]
				{
					new Threshold
					{
						Edge = 0.4f,
						Term = "LOW"
					},
					new Threshold
					{
						Edge = 0.7f,
						Term = "MEDIUM"
					},
					new Threshold
					{
						Edge = 1f,
						Term = "HIGH"
					}
				}
			}
		});
		float edge = 0.2f;
		float edge2 = 0.4f;
		float edge3 = 0.8f;
		float edge4 = 0.95f;
		float edge5 = 1f;
		list.Add(new PresentationType
		{
			KeyName = "skillPresentationStatusIcons",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[5]
				{
					new Threshold
					{
						Edge = edge,
						Term = "Only basic knowledge",
						Icon = "HUD_icon_productivity_person",
						IconTint = value5
					},
					new Threshold
					{
						Edge = edge2,
						Term = "Some skill"
					},
					new Threshold
					{
						Edge = edge3,
						Term = "Competent"
					},
					new Threshold
					{
						Edge = edge4,
						Term = "Highly skilled",
						Icon = "HUD_icon_productivity_person",
						IconTint = value3
					},
					new Threshold
					{
						Edge = edge5,
						Term = "Expert",
						Icon = "HUD_icon_productivity_person",
						IconTint = value4
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "skillPresentationSidePanel",
			ValueTooltipTextFormatting = new TextFormatting
			{
				NumberFormatString = "F2",
				TextWithPlaceholders = "Skill value {1}"
			},
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[2]
				{
					new Threshold
					{
						Edge = 0.1f,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 1f,
						TermTint = value6
					}
				}
			},
			RightAdjustValue = true,
			ValueRightPadding = 20,
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "emigrationRiskPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[2]
				{
					new Threshold
					{
						Edge = -0.01f,
						Icon = "lcd_icon_noEntry",
						Term = "NONE",
						IconTint = value,
						UseValueTextFormatting = false,
						UseValueTooltipFormatting = false,
						TermTooltip = "Will not emigrate right now."
					},
					new Threshold
					{
						Edge = 1f
					}
				}
			},
			ValueTextFormatting = new TextFormatting
			{
				NumberFactor = 100f,
				NumberFormatString = "F0",
				TextWithPlaceholders = "{1} %"
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "happinessPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[2]
				{
					new Threshold
					{
						Edge = -0.01f,
						Term = "Unhappy",
						Icon = "lcd_icon_smiley_unhappy",
						IconTint = value,
						TermTint = value,
						TermTooltip = "Unhappy characters have a higher emigration risk"
					},
					new Threshold
					{
						Edge = 1f,
						Term = "Happy",
						Icon = "lcd_icon_smiley_happy",
						IconTint = value7,
						TermTooltip = "Happy characters have less emigration risk"
					}
				}
			}
		});
		float edge6 = 0.2f;
		float edge7 = 0.4f;
		float edge8 = 0.6f;
		float edge9 = 0.98f;
		float edge10 = 1f;
		list.Add(new PresentationType
		{
			KeyName = "energyPresentationSidePanel",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[5]
				{
					new Threshold
					{
						Edge = edge6,
						Term = "Low",
						TermTooltip = "The character has low energy. This is a result of bad nutrition and/or lack of sleep",
						Icon = "HUD_icon_status_sun",
						IconTint = value,
						TermTint = value5
					},
					new Threshold
					{
						Edge = edge7,
						Term = "Reduced",
						TermTooltip = "The character has reduced energy. This is a result of insufficient nutrition and/or sleep",
						Icon = "HUD_icon_status_sun",
						IconTint = value2,
						TermTint = value5
					},
					new Threshold
					{
						Edge = edge8,
						Term = "Average",
						TermTooltip = "The character has average energy. This is a result of average nutrition and sleep",
						Icon = "HUD_icon_status_sun",
						IconTint = value7,
						TermTint = value11
					},
					new Threshold
					{
						Edge = edge9,
						Term = "Average",
						TermTooltip = "The character has average energy. This is a result of average nutrition and sleep",
						Icon = "HUD_icon_status_sun",
						IconTint = value7,
						TermTint = value11
					},
					new Threshold
					{
						Edge = edge10,
						Term = "High",
						TermTooltip = "The character has high energy. This is a result of sufficient nutrition and sleep",
						Icon = "HUD_icon_status_sun",
						IconTint = value7,
						TermTint = value11
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			},
			ValueRightPadding = 55,
			RightAdjustValue = true
		});
		list.Add(new PresentationType
		{
			KeyName = "energyPresentationStatusIcons",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[5]
				{
					new Threshold
					{
						Edge = edge6,
						Term = "Low energy",
						Icon = "HUD_icon_status_sun",
						IconTint = value
					},
					new Threshold
					{
						Edge = edge7,
						Term = "Reduced energy",
						Icon = "HUD_icon_status_sun",
						IconTint = value2
					},
					new Threshold
					{
						Edge = edge8,
						Term = null
					},
					new Threshold
					{
						Edge = edge9,
						Term = null
					},
					new Threshold
					{
						Edge = edge10,
						Term = "High energy",
						Icon = "HUD_icon_status_sun",
						IconTint = value4
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "entityFunctionalStatusIcons",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[2]
				{
					new Threshold
					{
						Edge = 0f,
						Term = "This item or structure is broken",
						Icon = "HUD_icon_status_broken",
						IconTint = value
					},
					new Threshold
					{
						Edge = 1f,
						Term = null
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "progressStatusIcons",
			ValueTooltipTextFormatting = new TextFormatting
			{
				NumberFactor = 100f,
				NumberFormatString = "F0",
				TextWithPlaceholders = "Production progress {1} %"
			},
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[1]
				{
					new Threshold
					{
						Edge = 1f,
						TermTint = ProgressColor
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				SuppressIfMaximumValue = true,
				MaxValue = 1f
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "consumeProgressStatusIcons",
			ValueTooltipTextFormatting = new TextFormatting
			{
				NumberFactor = 100f,
				NumberFormatString = "F0",
				TextWithPlaceholders = "Consume progress {1} %"
			},
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[1]
				{
					new Threshold
					{
						Edge = 1f,
						TermTint = ConsumeProgressColor
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				SuppressIfMaximumValue = true,
				MaxValue = 1f
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "micronutrientsPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[4]
				{
					new Threshold
					{
						Edge = 0.05f,
						TermTooltip = "Low intake. Person has such a low intake of micronutrients that it can lead to starvation",
						IconTint = value,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.3f,
						TermTooltip = "Inadequate intake. Person must raise intake of micronutrients to remain healthy",
						IconTint = value2,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.65f,
						TermTooltip = "Adequate intake. Person has an adequate, but below recommended intake of micronutrients",
						IconTint = value7,
						TermTint = value13
					},
					new Threshold
					{
						Edge = 1f,
						TermTooltip = "High intake. Person has the recommended intake of micronutrients",
						IconTint = value7,
						TermTint = value13
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			},
			RightAdjustValue = true,
			ValueRightPadding = 10
		});
		list.Add(new PresentationType
		{
			KeyName = "proteinPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[4]
				{
					new Threshold
					{
						Edge = 0.05f,
						TermTooltip = "Low intake. Person has such a low protein intake that it can lead to starvation",
						IconTint = value,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.3f,
						TermTooltip = "Inadequate intake. Person must raise protein intake to remain healthy",
						IconTint = value2,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.65f,
						TermTooltip = "Adequate intake. Person has an adequate, but below recommended protein intake",
						IconTint = value7,
						TermTint = value13
					},
					new Threshold
					{
						Edge = 1f,
						TermTooltip = "High intake. Person has the recommended protein intake",
						IconTint = value7,
						TermTint = value13
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			},
			RightAdjustValue = true,
			ValueRightPadding = 10
		});
		list.Add(new PresentationType
		{
			KeyName = "foodEnergyPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[5]
				{
					new Threshold
					{
						Edge = 0.15f,
						TermTooltip = "Very low intake. Person has such a low calorie intake that it can lead to starvation and death",
						IconTint = value,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.3f,
						TermTooltip = "Low intake. Person has such a low calorie intake that it can lead to starvation",
						IconTint = value2,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.5f,
						TermTooltip = "Inadequate intake. Person must raise calorie intake to remain healthy",
						IconTint = value7,
						TermTint = value13
					},
					new Threshold
					{
						Edge = 0.7f,
						TermTooltip = "Adequate intake. Person has an adequate, but below recommended calorie intake",
						IconTint = value7,
						TermTint = value13
					},
					new Threshold
					{
						Edge = 1f,
						TermTooltip = "High intake. Person has the recommended calorie intake",
						IconTint = value7,
						TermTint = value13
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			},
			RightAdjustValue = true,
			ValueRightPadding = 10
		});
		string termTooltip = "The person would like to enjoy a stimulant. This would improve their happiness and the colony's comfort conditions";
		string termTooltip2 = "The person has enjoyed a stimulant. This improves their happiness and the colony's comfort conditions";
		list.Add(new PresentationType
		{
			KeyName = "stimulantsPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[2]
				{
					new Threshold
					{
						Edge = 0.3f,
						Term = "Could use one",
						TermTooltip = termTooltip,
						IconTint = value7
					},
					new Threshold
					{
						Edge = 1f,
						Term = "Satisfied",
						TermTooltip = termTooltip2,
						IconTint = value7
					}
				}
			}
		});
		string termTooltip3 = "Starving. Person has (or has recently had) insufficient intake of calories, protein or micronutrients. Starving impacts a person's energy and can lead to death. \nNOTE: The effect of starvation can linger for some time after the person ingests food again.";
		string termTooltip4 = "Severely undernourished. Person has insufficient intake of one or more of the 3 nutrient groups: calories, protein or micronutrients.";
		string termTooltip5 = "Undernourished. Person has an inadequate intake of one or more of the 3 nutrient groups: calories, protein or micronutrients.";
		string termTooltip6 = "Adequately nourished. Person has an adequate, but below recommended, intake of the 3 nutrient groups: calories, protein or micronutrients.";
		string termTooltip7 = "Starving. The person or animal is starving because of insufficient food intake. Starving has a negative impact on a character's energy and can lead to death in the long term.";
		string termTooltip8 = "Severely undernourished. The character has insufficient food intake.";
		string termTooltip9 = "Undernourished. The character has an inadequate food intake.";
		string termTooltip10 = "Adequately nourished. The character has an adequate, but below recommended, food intake.";
		string term = "Starving";
		string term2 = "Severely undernourished";
		float edge11 = 0.5f;
		float edge12 = 0.6f;
		list.Add(new PresentationType
		{
			KeyName = "hungerPresentationSidePanel",
			TypeDependentPresentation = new TypeDependentPresentation
			{
				ThresholdsByType = new SerializableDictionary<string, Threshold[]> { 
				{
					"entity:human",
					new Threshold[5]
					{
						new Threshold
						{
							Edge = edge11,
							Term = term,
							TermTooltip = termTooltip3,
							Icon = "HUD_icon_status_hunger",
							IconTint = value,
							TermTint = value5
						},
						new Threshold
						{
							Edge = edge12,
							Term = term2,
							TermTooltip = termTooltip4,
							Icon = "HUD_icon_status_hunger",
							IconTint = value2,
							TermTint = value5
						},
						new Threshold
						{
							Edge = 0.7f,
							Term = "Undernourished",
							TermTooltip = termTooltip5,
							Icon = "HUD_icon_status_hunger",
							IconTint = value7,
							TermTint = value12
						},
						new Threshold
						{
							Edge = 0.8f,
							Term = "Adequately nourished",
							TermTooltip = termTooltip6,
							Icon = "HUD_icon_status_hunger",
							IconTint = value7,
							TermTint = value12
						},
						new Threshold
						{
							Edge = 1f,
							Term = "Fully nourished",
							TermTooltip = "Fully nourished. Person has the recommended intake of the 3 nutrient groups: calories, protein or micronutrients.",
							Icon = "HUD_icon_status_hunger",
							IconTint = value7,
							TermTint = value12
						}
					}
				} }
			},
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[5]
				{
					new Threshold
					{
						Edge = edge11,
						Term = term,
						TermTooltip = termTooltip7,
						Icon = "HUD_icon_status_hunger",
						IconTint = value,
						TermTint = value5
					},
					new Threshold
					{
						Edge = edge12,
						Term = term2,
						TermTooltip = termTooltip8,
						Icon = "HUD_icon_status_hunger",
						IconTint = value2,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.7f,
						Term = "Undernourished",
						TermTooltip = termTooltip9,
						Icon = "HUD_icon_status_hunger",
						IconTint = value7,
						TermTint = value12
					},
					new Threshold
					{
						Edge = 0.8f,
						Term = "Adequately nourished",
						TermTooltip = termTooltip10,
						Icon = "HUD_icon_status_hunger",
						IconTint = value7,
						TermTint = value12
					},
					new Threshold
					{
						Edge = 1f,
						Term = "Fully nourished",
						TermTooltip = "Fully nourished. The character has the recommended food intake.",
						Icon = "HUD_icon_status_hunger",
						IconTint = value7,
						TermTint = value12
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			},
			RightAdjustValue = true,
			ValueRightPadding = 10
		});
		list.Add(new PresentationType
		{
			KeyName = "hungerPresentationStatusIcons",
			TypeDependentPresentation = new TypeDependentPresentation
			{
				ThresholdsByType = new SerializableDictionary<string, Threshold[]> { 
				{
					"entity:human",
					new Threshold[3]
					{
						new Threshold
						{
							Edge = edge11,
							Term = term,
							TermTooltip = termTooltip3,
							Icon = "HUD_icon_status_hunger",
							IconTint = value
						},
						new Threshold
						{
							Edge = edge12,
							Term = term2,
							TermTooltip = termTooltip4,
							Icon = "HUD_icon_status_hunger",
							IconTint = value2
						},
						new Threshold
						{
							Edge = 1f
						}
					}
				} }
			},
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[3]
				{
					new Threshold
					{
						Edge = edge11,
						Term = term,
						TermTooltip = termTooltip7,
						Icon = "HUD_icon_status_hunger",
						IconTint = value
					},
					new Threshold
					{
						Edge = edge12,
						Term = term2,
						TermTooltip = termTooltip8,
						Icon = "HUD_icon_status_hunger",
						IconTint = value2
					},
					new Threshold
					{
						Edge = 1f
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "moraleLevelPresentation",
			TypeDependentPresentation = new TypeDependentPresentation
			{
				ThresholdsByType = new SerializableDictionary<string, Threshold[]> { 
				{
					"entity:human",
					new Threshold[2]
					{
						new Threshold
						{
							Edge = 4f,
							Term = "LOW!",
							IconTint = value,
							TermTooltip = "Low morale makes this person anxious towards danger. In severe cases the person can panick, especially when suffering physical injuries"
						},
						new Threshold
						{
							Edge = 12f,
							Term = "High",
							IconTint = value7,
							TermTooltip = "High morale makes this person able to carry out dangerous tasks, IF not suffering from physical injuries"
						}
					}
				} }
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "threatStancePresentation",
			TypeDependentPresentation = new TypeDependentPresentation
			{
				ThresholdsByType = new SerializableDictionary<string, Threshold[]> { 
				{
					"entity:human",
					new Threshold[3]
					{
						new Threshold
						{
							Edge = 4f,
							Term = "Cautious",
							IconTint = value,
							TermTooltip = "Person will stay far away from any sort of danger. (Behavior usually caused by low morale and/or physical injuries)"
						},
						new Threshold
						{
							Edge = 12f,
							Term = "Vigilant",
							IconTint = value7,
							TermTooltip = "In this stance, a person will keep a moderate distance to danger."
						},
						new Threshold
						{
							Edge = 100f,
							Term = "Fearless",
							IconTint = value7,
							TermTooltip = "Person is willing to enter dangerous areas to fight or do necessary work."
						}
					}
				} }
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "foodItemNutritionPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[3]
				{
					new Threshold
					{
						Term = "Low",
						IconTint = value,
						Edge = 0.3f
					},
					new Threshold
					{
						Term = "Medium",
						Edge = 0.6f
					},
					new Threshold
					{
						Term = "High",
						Edge = 0.9f
					}
				}
			},
			ValueTooltipTextFormatting = new TextFormatting
			{
				NumberFormatString = "F2",
				TextWithPlaceholders = "{1}"
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "foodHappinessPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				NumberSource = NumberSource.Difference,
				Thresholds = new Threshold[3]
				{
					new Threshold
					{
						Term = "Happy",
						TermTint = value6,
						IconTint = value8,
						Icon = "lcd_icon_nutrition",
						Edge = -0.01f
					},
					new Threshold
					{
						Term = "Content",
						TermTint = value6,
						IconTint = value8,
						Icon = "lcd_icon_nutrition",
						Edge = 0f
					},
					new Threshold
					{
						Term = "Unhappy",
						TermTint = value5,
						IconTint = value8,
						Icon = "lcd_icon_nutrition",
						Edge = 2f
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f,
				SuppressIfMaximumValue = false,
				Width = 60
			},
			RightAdjustValue = true,
			ValueRightPadding = 20
		});
		list.Add(new PresentationType
		{
			KeyName = "securityHappinessPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				NumberSource = NumberSource.Difference,
				Thresholds = new Threshold[3]
				{
					new Threshold
					{
						Term = "Happy",
						TermTint = value6,
						IconTint = value9,
						Icon = "lcd_icon_security",
						Edge = -0.01f
					},
					new Threshold
					{
						Term = "Content",
						TermTint = value6,
						IconTint = value9,
						Icon = "lcd_icon_security",
						Edge = 0f
					},
					new Threshold
					{
						Term = "Unhappy",
						TermTint = value5,
						IconTint = value9,
						Icon = "lcd_icon_security",
						Edge = 2f
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f,
				SuppressIfMaximumValue = false,
				Width = 60
			},
			RightAdjustValue = true,
			ValueRightPadding = 20
		});
		list.Add(new PresentationType
		{
			KeyName = "comfortHappinessPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				NumberSource = NumberSource.Difference,
				Thresholds = new Threshold[3]
				{
					new Threshold
					{
						Term = "Happy",
						TermTint = value6,
						IconTint = value10,
						Icon = "lcd_icon_comfort",
						Edge = -0.01f
					},
					new Threshold
					{
						Term = "Content",
						TermTint = value6,
						IconTint = value10,
						Icon = "lcd_icon_comfort",
						Edge = 0f
					},
					new Threshold
					{
						Term = "Unhappy",
						TermTint = value5,
						IconTint = value10,
						Icon = "lcd_icon_comfort",
						Edge = 2f
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f,
				SuppressIfMaximumValue = false,
				Width = 60
			},
			RightAdjustValue = true,
			ValueRightPadding = 20
		});
		list.Add(new PresentationType
		{
			KeyName = "percentagePresentation",
			ValueTextFormatting = new TextFormatting
			{
				NumberFactor = 100f,
				NumberFormatString = "F0",
				TextWithPlaceholders = "{1} %"
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "timeInDaysPresentation",
			ValueTextFormatting = new TextFormatting
			{
				NumberFormatString = "F1"
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "substancePresentation",
			ValueTextFormatting = new TextFormatting
			{
				NumberFormatString = "F0",
				TextWithPlaceholders = "{1}"
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "storageCapacityPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[2]
				{
					new Threshold
					{
						Edge = 0.98f,
						TermTint = value6
					},
					new Threshold
					{
						Edge = 1f,
						TermTint = value5
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			},
			RightAdjustValue = true,
			ValueRightPadding = 10
		});
		list.Add(new PresentationType
		{
			KeyName = "itemBulkPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[1]
				{
					new Threshold
					{
						Edge = 0f,
						TermTooltip = "BULK is a measurement of mass and volume. An average human has a size of 100 BULK"
					}
				}
			},
			ValueTextFormatting = new TextFormatting
			{
				NumberFormatString = "F0",
				TextWithPlaceholders = "{1} BULK"
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "entityTypePresentation",
			RightAdjustValue = true,
			ValueRightPadding = 4,
			EntityTypePresentation = new EntityTypePresentation()
		});
		list.Add(new PresentationType
		{
			KeyName = "cropGrowthPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[7]
				{
					new Threshold
					{
						Edge = 0.01f,
						TermTooltip = "None",
						TermTint = value15
					},
					new Threshold
					{
						Edge = 0.05f,
						TermTooltip = "Germinating",
						TermTint = value15
					},
					new Threshold
					{
						Edge = 0.1f,
						TermTooltip = "Sprouting",
						TermTint = value15
					},
					new Threshold
					{
						Edge = 0.4f,
						TermTooltip = "Small",
						TermTint = value15
					},
					new Threshold
					{
						Edge = 0.6f,
						TermTooltip = "Medium",
						TermTint = value15
					},
					new Threshold
					{
						Edge = 0.95f,
						TermTooltip = "Large",
						TermTint = value15
					},
					new Threshold
					{
						Edge = 1f,
						TermTooltip = "Full size reached. The crops will be ready for harvest after ripening",
						TermTint = value15
					}
				}
			},
			RightAdjustValue = true,
			ValueRightPadding = 20,
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "weedGrowthPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[6]
				{
					new Threshold
					{
						Edge = 0.05f,
						Term = "None",
						TermTint = value16,
						TermTooltip = "We will automatically assign weeding tasks to each other to keep the field productive and free from weeds"
					},
					new Threshold
					{
						Edge = 0.1f,
						Term = "Sprouting",
						TermTint = value16,
						TermTooltip = "We will automatically assign weeding tasks to each other to keep the field productive and free from weeds"
					},
					new Threshold
					{
						Edge = 0.4f,
						Term = "Small",
						TermTint = value16,
						TermTooltip = "Small. Weeds of this size will not affect crop growth too much. We will automatically assign weeding tasks to each other to keep the field productive and free from weeds"
					},
					new Threshold
					{
						Edge = 0.6f,
						Term = "Medium",
						TermTint = value16,
						TermTooltip = "Medium. Weeds of this size will affect crop growth. We will automatically assign weeding tasks to each other to keep the field productive and free from weeds"
					},
					new Threshold
					{
						Edge = 0.95f,
						Term = "Large",
						TermTint = value16,
						TermTooltip = "Large. These weeds will severely hamper crop growth. We will automatically assign weeding tasks to each other to keep the field productive and free from weeds"
					},
					new Threshold
					{
						Edge = 1f,
						Term = "Overgrown",
						TermTint = value,
						TermTooltip = "Overgrown. These weeds will severely hamper crop growth. We will automatically assign weeding tasks to each other to keep the field productive and free from weeds"
					}
				}
			},
			RightAdjustValue = true,
			ValueRightPadding = 20,
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "soilNutrientLevelPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[5]
				{
					new Threshold
					{
						Edge = 0.05f,
						Term = "Totally depleted",
						TermTooltip = "The soil needs fertilizer to yield any crops at all",
						TermTint = value
					},
					new Threshold
					{
						Edge = 0.2f,
						Term = "Heavily depleted",
						TermTooltip = "The soil needs fertilizer to yield anything but a minimum amount of crops",
						TermTint = value2
					},
					new Threshold
					{
						Edge = 0.4f,
						Term = "Somewhat depleted",
						TermTooltip = "The soil needs fertilizer to ensure a large crop yield",
						TermTint = value17
					},
					new Threshold
					{
						Edge = 0.8f,
						Term = "Quite fertile",
						TermTooltip = "The soil is in good condition and not in need of fertilizer",
						TermTint = value17
					},
					new Threshold
					{
						Edge = 1f,
						Term = "Very fertile",
						TermTooltip = "The soil has very good quality which makes a high crop yield possible",
						TermTint = value17
					}
				}
			},
			RightAdjustValue = true,
			ValueRightPadding = 20,
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			}
		});
		string text = "About to collapse";
		string text2 = "Very sleepy";
		string text3 = "The character is in such need of sleep that it lowers the person's energy level.";
		string text4 = "The character is in need of sleep which contributes to a lowered energy level.";
		float edge13 = 0.05f;
		float edge14 = 0.2f;
		list.Add(new PresentationType
		{
			KeyName = "sleepNeedPresentationSidePanel",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[4]
				{
					new Threshold
					{
						Edge = edge13,
						TermTooltip = text + ". " + text3,
						Icon = "HUD_icon_status_sleep",
						IconTint = value,
						TermTint = value5
					},
					new Threshold
					{
						Edge = edge14,
						TermTooltip = text2 + ". " + text4,
						Icon = "HUD_icon_status_sleep",
						IconTint = value2,
						TermTint = value5
					},
					new Threshold
					{
						Edge = 0.5f,
						TermTooltip = "Tired. The character needs more sleep",
						Icon = "HUD_icon_status_sleep",
						IconTint = value7,
						TermTint = value14
					},
					new Threshold
					{
						Edge = 1f,
						TermTooltip = "Well rested. The character gets sufficient sleep",
						Icon = "HUD_icon_status_sleep",
						IconTint = value7,
						TermTint = value14
					}
				}
			},
			BarPresentation = new BarPresentation
			{
				MaxValue = 1f
			},
			RightAdjustValue = true,
			ValueRightPadding = 10
		});
		list.Add(new PresentationType
		{
			KeyName = "sleepNeedPresentationStatusIcons",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[3]
				{
					new Threshold
					{
						Edge = edge13,
						Term = text,
						TermTooltip = text3,
						Icon = "HUD_icon_status_sleep",
						IconTint = value
					},
					new Threshold
					{
						Edge = edge14,
						Term = text2,
						TermTooltip = text4,
						Icon = "HUD_icon_status_sleep",
						IconTint = value2
					},
					new Threshold
					{
						Edge = 1f
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "hitpointsPresentation",
			TypeDependentPresentation = new TypeDependentPresentation
			{
				ThresholdsByType = new SerializableDictionary<string, Threshold[]>
				{
					{
						"entity:human",
						new Threshold[8]
						{
							new Threshold
							{
								Edge = 0.2f,
								Term = "Near death",
								Icon = "HUD_icon_status_injury",
								IconTint = value
							},
							new Threshold
							{
								Edge = 0.3f,
								Term = "Critically injured",
								Icon = "HUD_icon_status_injury",
								IconTint = value2
							},
							new Threshold
							{
								Edge = 0.5f,
								Term = "Severely injured",
								Icon = "HUD_icon_status_injury",
								IconTint = value2
							},
							new Threshold
							{
								Edge = 0.6f,
								Term = "Seriously injured",
								Icon = "HUD_icon_status_injury",
								IconTint = value2
							},
							new Threshold
							{
								Edge = 0.8f,
								Term = "Moderately injured",
								Icon = "HUD_icon_status_injury",
								IconTint = value2
							},
							new Threshold
							{
								Edge = 0.95f,
								Term = "Mild injuries",
								Icon = "HUD_icon_status_injury",
								IconTint = value2
							},
							new Threshold
							{
								Edge = 0.99f,
								Term = "Superficial",
								Icon = "HUD_icon_status_injury",
								IconTint = value2
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					},
					{
						"entity:whiteThunderChicken",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.3f,
								Term = "Appears mortally wounded"
							},
							new Threshold
							{
								Edge = 0.6f,
								Term = "Appears wounded"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					},
					{
						"entity:pygmyThunderChicken",
						new Threshold[3]
						{
							new Threshold
							{
								Edge = 0.3f,
								Term = "Appears mortally wounded"
							},
							new Threshold
							{
								Edge = 0.6f,
								Term = "Appears wounded"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "bodyPartCondition",
			TypeDependentPresentation = new TypeDependentPresentation
			{
				ThresholdsByType = new SerializableDictionary<string, Threshold[]>
				{
					{
						"entity:person:Left leg",
						new Threshold[9]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Open fracture"
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Fracture"
							},
							new Threshold
							{
								Edge = 0.3f,
								Term = "Deep lacerations"
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Dislocation"
							},
							new Threshold
							{
								Edge = 0.55f,
								Term = "Muscle tear"
							},
							new Threshold
							{
								Edge = 0.65f,
								Term = "Sprain"
							},
							new Threshold
							{
								Edge = 0.75f,
								Term = "Superficial laceration"
							},
							new Threshold
							{
								Edge = 0.85f,
								Term = "Skin abrasions"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					},
					{
						"entity:person:Right leg",
						new Threshold[9]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Open fracture"
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Fracture"
							},
							new Threshold
							{
								Edge = 0.3f,
								Term = "Deep lacerations"
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Dislocation"
							},
							new Threshold
							{
								Edge = 0.55f,
								Term = "Muscle tear"
							},
							new Threshold
							{
								Edge = 0.65f,
								Term = "Sprain"
							},
							new Threshold
							{
								Edge = 0.75f,
								Term = "Superficial laceration"
							},
							new Threshold
							{
								Edge = 0.85f,
								Term = "Skin abrasions"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					},
					{
						"entity:person:Left arm",
						new Threshold[9]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Open fracture"
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Fracture"
							},
							new Threshold
							{
								Edge = 0.3f,
								Term = "Deep lacerations"
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Dislocation"
							},
							new Threshold
							{
								Edge = 0.55f,
								Term = "Muscle tear"
							},
							new Threshold
							{
								Edge = 0.65f,
								Term = "Sprain"
							},
							new Threshold
							{
								Edge = 0.75f,
								Term = "Superficial laceration"
							},
							new Threshold
							{
								Edge = 0.85f,
								Term = "Skin abrasions"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					},
					{
						"entity:person:Right arm",
						new Threshold[9]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Open fracture"
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Fracture"
							},
							new Threshold
							{
								Edge = 0.3f,
								Term = "Deep lacerations"
							},
							new Threshold
							{
								Edge = 0.4f,
								Term = "Dislocation"
							},
							new Threshold
							{
								Edge = 0.55f,
								Term = "Muscle tear"
							},
							new Threshold
							{
								Edge = 0.65f,
								Term = "Sprain"
							},
							new Threshold
							{
								Edge = 0.75f,
								Term = "Superficial laceration"
							},
							new Threshold
							{
								Edge = 0.85f,
								Term = "Skin abrasions"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					},
					{
						"entity:person:Torso",
						new Threshold[6]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Ruptured organ"
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Deep lacerations"
							},
							new Threshold
							{
								Edge = 0.45f,
								Term = "Perforation"
							},
							new Threshold
							{
								Edge = 0.75f,
								Term = "Superficial laceration"
							},
							new Threshold
							{
								Edge = 0.85f,
								Term = "Skin abrasions"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					},
					{
						"entity:person:Head",
						new Threshold[7]
						{
							new Threshold
							{
								Edge = 0.1f,
								Term = "Open fracture"
							},
							new Threshold
							{
								Edge = 0.2f,
								Term = "Fracture"
							},
							new Threshold
							{
								Edge = 0.3f,
								Term = "Deep lacerations"
							},
							new Threshold
							{
								Edge = 0.55f,
								Term = "Perforations"
							},
							new Threshold
							{
								Edge = 0.75f,
								Term = "Superficial lacerations"
							},
							new Threshold
							{
								Edge = 0.85f,
								Term = "Skin abrasions"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "MovementCondition",
			TypeDependentPresentation = new TypeDependentPresentation
			{
				ThresholdsByType = new SerializableDictionary<string, Threshold[]>
				{
					{
						"entity:pygmyThunderChicken:Left leg",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.3f,
								Term = "Limping"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					},
					{
						"entity:pygmyThunderChicken:Right leg",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.3f,
								Term = "Limping"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					},
					{
						"entity:whiteThunderChicken:Left leg",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.3f,
								Term = "Limping"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					},
					{
						"entity:whiteThunderChicken:Right leg",
						new Threshold[2]
						{
							new Threshold
							{
								Edge = 0.3f,
								Term = "Limping"
							},
							new Threshold
							{
								Edge = 1f,
								Term = null
							}
						}
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "professionPresentation",
			IconPresentation = new IconPresentation(),
			ValueTooltipTextFormatting = new TextFormatting
			{
				TextWithPlaceholders = "Field: {0}  \nThe character has the best skills in this area."
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "replenishStatusPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[2]
				{
					new Threshold
					{
						Edge = 0.5f,
						Icon = "HUD_icon_status_firewood",
						IconTint = value,
						Term = "No suitable fuel in inventory. Production orders cannot be completed before we have the correct type of fuel"
					},
					new Threshold
					{
						Edge = 1f,
						Icon = null,
						Term = null
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "ammoStatusPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[3]
				{
					new Threshold
					{
						Edge = 0.1f,
						Icon = "HUD_icon_status_ammunition",
						IconTint = value,
						Term = "Needs a reload, but there is no ammunition in inventory"
					},
					new Threshold
					{
						Edge = 0.5f,
						Icon = "HUD_icon_status_ammunition",
						IconTint = value2,
						Term = "Needs ammunition reload"
					},
					new Threshold
					{
						Edge = 1f,
						Icon = null,
						Term = null
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "inAccessiblePresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[3]
				{
					new Threshold
					{
						Edge = 0.2f,
						Icon = null,
						Term = null
					},
					new Threshold
					{
						Edge = 0.5f,
						Icon = "HUD_icon_status_exclamation",
						IconTint = value,
						Term = "A colonist needs this item but is unwilling to go near it because a dangerous area/threat is blocking the way. \nUse PATROL or ATTACK to clear the area of threats. Use the THREAT overlay next to the minimap to display danger areas."
					},
					new Threshold
					{
						Edge = 1f,
						Icon = "HUD_icon_status_noAccess",
						IconTint = value,
						Term = "Not accessible"
					}
				}
			}
		});
		list.Add(new PresentationType
		{
			KeyName = "hasThreatJobPresentation",
			NumberThresholdPresentation = new NumberThresholdPresentation
			{
				Thresholds = new Threshold[2]
				{
					new Threshold
					{
						Edge = 0.9f,
						Icon = null,
						Term = null
					},
					new Threshold
					{
						Edge = 1f,
						Icon = "HUD_icon_status_skull",
						Term = "This animal is seen as a threat and the colony members will try to eliminate it"
					}
				}
			}
		});
		return list;
	}

	protected override CustomDataPresentation InitStatusIconPresentation()
	{
		CustomDataPresentation customDataPresentation = new CustomDataPresentation();
		customDataPresentation.PresentationTypeCategories = new PresentationTypeCategory[1]
		{
			new PresentationTypeCategory
			{
				PrimarySortingOfItems = new Sorting
				{
					SortingMethod = SortingMethod.StaticSortOrder,
					SortingDirection = Grid.Sorting.Ascending
				},
				SecondarySortingOfItems = new Sorting
				{
					SortingMethod = SortingMethod.NumberResultMiddleDistance,
					SortingDirection = Grid.Sorting.Descending
				},
				Name = "Status icon presentations",
				Nodes = new LeafNode[12]
				{
					new LeafNode
					{
						SortOrder = 1,
						Presentation = new Presentation
						{
							PropertyNameForValue = "hitpointLevel",
							PresentationTypeKey = "hitpointsPresentation"
						}
					},
					new LeafNode
					{
						SortOrder = 1,
						Presentation = new Presentation
						{
							PropertyNameForValue = "hungerStatus",
							PresentationTypeKey = "hungerPresentationStatusIcons"
						}
					},
					new LeafNode
					{
						SortOrder = 1,
						Presentation = new Presentation
						{
							PropertyNameForValue = "sleepStatus",
							PresentationTypeKey = "sleepNeedPresentationStatusIcons"
						}
					},
					new LeafNode
					{
						SortOrder = 1,
						Presentation = new Presentation
						{
							PropertyNameForValue = "energyLevel",
							PresentationTypeKey = "energyPresentationStatusIcons"
						}
					},
					new LeafNode
					{
						SortOrder = 1,
						Presentation = new Presentation
						{
							PropertyNameForValue = "entityIsFunctional",
							PresentationTypeKey = "entityFunctionalStatusIcons"
						}
					},
					new LeafNode
					{
						SortOrder = 1,
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								PropertyName = "replenishTypeName"
							},
							PropertyNameForValue = "replenishStatus",
							PresentationTypeKey = "replenishStatusPresentation",
							ValueTooltip = new StringSource
							{
								PropertyName = "replenishStatusTooltip"
							}
						}
					},
					new LeafNode
					{
						SortOrder = 1,
						Presentation = new Presentation
						{
							PropertyNameForValue = "ammoStatus",
							PresentationTypeKey = "ammoStatusPresentation"
						}
					},
					new LeafNode
					{
						SortOrder = 1,
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								UseDefaultName = true
							},
							PropertyNameForValue = "inAccessible",
							PresentationTypeKey = "inAccessiblePresentation"
						}
					},
					new LeafNode
					{
						SortOrder = 1,
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								UseDefaultName = true
							},
							PropertyNameForValue = "hasThreatJob",
							PresentationTypeKey = "hasThreatJobPresentation"
						}
					},
					new LeafNode
					{
						SortOrder = 0,
						Presentation = new Presentation
						{
							PropertyNameForValue = "progress",
							PresentationTypeKey = "progressStatusIcons"
						}
					},
					new LeafNode
					{
						SortOrder = 0,
						Presentation = new Presentation
						{
							PropertyNameForValue = "consumeProgress",
							PresentationTypeKey = "consumeProgressStatusIcons"
						}
					},
					new LeafNode
					{
						SortOrder = 0,
						Presentation = new Presentation
						{
							PropertyNameForValue = "professionIcon",
							ValueTooltip = new StringSource
							{
								PropertyName = "professionDescription"
							},
							PresentationTypeKey = "professionPresentation"
						}
					}
				}
			}
		};
		return customDataPresentation;
	}

	protected override CustomDataPresentation InitActivityPresentation()
	{
		CustomDataPresentation customDataPresentation = new CustomDataPresentation();
		customDataPresentation.PresentationTypeCategories = new PresentationTypeCategory[1]
		{
			new PresentationTypeCategory
			{
				Name = "Productivity",
				Nodes = new LeafNode[3]
				{
					new LeafNode
					{
						SortOrder = 0,
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								StaticString = "Productivity"
							},
							PropertyNameForValue = "totalProductivity",
							PresentationTypeKey = "totalProductivity"
						}
					},
					new LeafNode
					{
						SortOrder = 1,
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								PropertyName = "skillInUseName"
							},
							PropertyNameForValue = "skillProductivity",
							PresentationTypeKey = "skillPresentationStatusIcons"
						}
					},
					new LeafNode
					{
						SortOrder = 2,
						Presentation = new Presentation
						{
							Caption = new StringSource
							{
								PropertyName = "toolInUseName"
							},
							PropertyNameForValue = "toolProductivity",
							PresentationTypeKey = "toolProductivity"
						}
					}
				}
			}
		};
		return customDataPresentation;
	}

	protected override CustomDataPresentation InitOtherSiteSidePanelPresentation()
	{
		CustomDataPresentation customDataPresentation = new CustomDataPresentation();
		customDataPresentation.PresentationTypeCategoryKeys = new string[2] { "skillsCategory", "opinionsCategory" };
		return customDataPresentation;
	}

	protected override CustomDataPresentation InitSidePanelPresentation()
	{
		CustomDataPresentation customDataPresentation = new CustomDataPresentation();
		customDataPresentation.PresentationTypeCategoryKeys = new string[8] { "occupantsCategory", "residentsCategory", "healthStatusCategory", "skillsCategory", "cropsCategory", "statusCategory", "opinionsCategory", "effectsCategory" };
		return customDataPresentation;
	}

	protected override List<SoundData> InitSounds()
	{
		float num = 0.06f;
		float num2 = 0.12f;
		return new List<SoundData>
		{
			new SoundData
			{
				KeyName = "aliens/alienCombat/bushdragonPoisonShot",
				Sound = "aliens/alienCombat/bushdragonPoisonShot1a",
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/bushdragonAttack",
				Sound = "aliens/alienCombat/bushdragonAttack1a",
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/bushdragonHit",
				Sound = "aliens/alienCombat/bushdragonHit1a",
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/bushdragonDeath",
				Sound = "aliens/alienCombat/bushdragonDeath1a",
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/poisonAlienImpact",
				Sound = "aliens/alienCombat/poisonAlienImpact1a",
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/wormAttackPart1",
				Sound = "aliens/alienCombat/wormAttackStart1a",
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/wormAttackPart2",
				Sound = "aliens/alienCombat/wormAttackAction1a",
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/wormHitShort",
				Sound = "aliens/alienCombat/wormHit1a",
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/wormDeathShort",
				Sound = "aliens/alienCombat/wormDeath1a",
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/snatcherAttack",
				Sound = "aliens/alienCombat/snatcherAttack1a",
				Volume = 0.22f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num
				}
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/snatcherHit",
				Sound = "aliens/alienCombat/snatcherHit1a",
				Volume = 0.22f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/snatcherDeath",
				Sound = "aliens/alienCombat/snatcherDeath1a",
				Volume = 0.22f
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/flutter_Mat43_v2",
				Sound = "aliens/alienCombat/flutter_Mat43_v2",
				Volume = 1f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.05000000074505806
				}
			},
			new SoundData
			{
				KeyName = "aliens/alienCombat/strumming_Mat25_v1",
				Sound = "aliens/alienCombat/strumming_Mat25_v1",
				Volume = 1f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.05000000074505806
				}
			},
			new SoundData
			{
				KeyName = "aliens/croaker",
				Sound = "aliens/croaker",
				Volume = 0f
			},
			new SoundData
			{
				KeyName = "aliens/binalRatDie",
				Sound = "aliens/binalRatDie1a",
				Volume = 0f
			},
			new SoundData
			{
				KeyName = "aliens/rattleWooden",
				Sound = "aliens/rattleWooden",
				Volume = 0f
			},
			new SoundData
			{
				KeyName = "aliens/rattleWoodenShort",
				Sound = "aliens/rattleWoodenShort",
				Volume = 0f
			},
			new SoundData
			{
				KeyName = "aliens/hummingClickClacking",
				Sound = "aliens/hummingClickClacking",
				Volume = 0f
			},
			new SoundData
			{
				KeyName = "aliens/bushdragonCombatIdle",
				Sound = "aliens/bushdragonCombatIdle",
				Volume = 0f
			},
			new SoundData
			{
				KeyName = "aliens/bushdragonHit",
				Sound = "aliens/bushdragonHit",
				Volume = 0f
			},
			new SoundData
			{
				KeyName = "aliens/spacechicken",
				Sound = "aliens/spacechicken",
				Volume = 0f
			},
			new SoundData
			{
				KeyName = "aliens/sliceSnapDouble",
				Sound = "aliens/sliceSnapDouble",
				Volume = 0.12f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = -0.3499999940395355,
					StandardDeviation = 0.10000000149011612
				}
			},
			new SoundData
			{
				KeyName = "aliens/sliceSnap",
				Sound = "aliens/sliceSnap",
				Volume = 0.12f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = -0.3499999940395355,
					StandardDeviation = 0.10000000149011612
				}
			},
			new SoundData
			{
				KeyName = "aliens/twinklerStab",
				Sound = "aliens/twinklerStab",
				Volume = 0.8f
			},
			new SoundData
			{
				KeyName = "aliens/bushdragonAttack_lowerVolume",
				Sound = "aliens/bushdragonAttack_lowerVolume",
				Volume = 0f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingMetalMend",
				Sound = "activities/building/buildingMetalMend",
				Volume = 0.25f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingMetalKneelDig",
				Sound = "activities/building/buildingMetalKneelDig",
				Volume = 0.25f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingMetalKneelWaterDevice",
				Sound = "activities/building/buildingMetalKneelWaterDevice",
				Volume = 0.18f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingTarpImprovisedKneelDig",
				Sound = "activities/building/buildingTarpImprovisedKneelDig",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingTarpImprovisedKneelWaterDevice",
				Sound = "activities/building/buildingTarpImprovisedKneelWaterDevice",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingTarpImprovisedMend",
				Sound = "activities/building/buildingTarpImprovisedMend",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingTarpKneelDig",
				Sound = "activities/building/buildingTarpKneelDig",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingTarpKneelWaterDevice",
				Sound = "activities/building/buildingTarpKneelWaterDevice",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingTarpMend",
				Sound = "activities/building/buildingTarpMend",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingImprovisedKneelDig",
				Sound = "activities/building/buildingImprovisedKneelDig",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingImprovisedKneelWaterDevice",
				Sound = "activities/building/buildingImprovisedKneelWaterDevice",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingImprovisedMend",
				Sound = "activities/building/buildingImprovisedMend",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingElectronic1",
				Sound = "activities/building/buildingElectronic1",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingElectronic2",
				Sound = "activities/building/buildingElectronic2",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingHammer",
				Sound = "activities/building/buildingHammer",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/building/buildingSteel",
				Sound = "activities/building/buildingSteel",
				Volume = 0.45f
			},
			new SoundData
			{
				KeyName = "activities/butcher/butcherChopLow",
				Sound = "activities/butcher/butcherChopLow",
				Volume = 0.32f
			},
			new SoundData
			{
				KeyName = "activities/butcher/butcherCutLow",
				Sound = "activities/butcher/butcherCutLow",
				Volume = 0.32f
			},
			new SoundData
			{
				KeyName = "activities/butcher/butcherConstructPull",
				Sound = "activities/butcher/butcherConstructPull",
				Volume = 0.32f
			},
			new SoundData
			{
				KeyName = "activities/butcher/butcherSharpen",
				Sound = "activities/butcher/butcherSharpen",
				Volume = 0.32f
			},
			new SoundData
			{
				KeyName = "activities/butcher/butcherGather",
				Sound = "activities/butcher/butcherGather",
				Volume = 0.32f
			},
			new SoundData
			{
				KeyName = "activities/cooking/cookingBoil",
				Sound = "activities/cooking/cookingBoil",
				Volume = 1f
			},
			new SoundData
			{
				KeyName = "activities/eating/eatingCrunchy",
				Sound = "activities/eating/eatingCrunchy",
				Volume = 0.6f
			},
			new SoundData
			{
				KeyName = "activities/farming/farmingHoe1A",
				Sound = "activities/farming/farmingHoeDirtSoil1A",
				Volume = 0.75f
			},
			new SoundData
			{
				KeyName = "activities/farming/farmingHoe1B",
				Sound = "activities/farming/farmingHoeDirtSoil1B",
				Volume = 0.75f
			},
			new SoundData
			{
				KeyName = "activities/gather/gatherChopLow",
				Sound = "activities/gather/gatherChopLow2",
				Volume = 0.55f
			},
			new SoundData
			{
				KeyName = "activities/gather/gatherHandGather",
				Sound = "activities/gather/gatherHandGather3",
				Volume = 0.3f,
				PlayMaxOneInstance = true,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.029999999329447746
				}
			},
			new SoundData
			{
				KeyName = "activities/gather/gatherHandKneelDig",
				Sound = "activities/gather/gatherHandKneelDig1",
				Volume = 0.55f,
				PlayMaxOneInstance = true,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.029999999329447746
				}
			},
			new SoundData
			{
				KeyName = "activities/gather/gatherHandGatherStand",
				Sound = "activities/gather/gatherHandGatherStand",
				PlayMaxOneInstance = true,
				Volume = 0.26f
			},
			new SoundData
			{
				KeyName = "activities/gather/gatherCutLow",
				Sound = "activities/gather/gatherCutLow",
				PlayMaxOneInstance = true,
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "melee/STAB2_24 - 4 Stabs With Blood",
				Sound = "melee/STAB2_24 - 4 Stabs With Blood",
				Volume = 0.5f
			},
			new SoundData
			{
				KeyName = "melee/swoosh",
				Sound = "melee/swoosh",
				Volume = 1f
			},
			new SoundData
			{
				KeyName = "melee/PUNCH1_02 - 9 Low Thud Punches",
				Sound = "melee/PUNCH1_02 - 9 Low Thud Punches",
				Volume = 0.21f
			},
			new SoundData
			{
				KeyName = "activities/melee/kickHard1B",
				Sound = "activities/melee/kickHard1B",
				Volume = 0.25f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num2
				}
			},
			new SoundData
			{
				KeyName = "activities/melee/kickHard2",
				Sound = "activities/melee/kickHard2B",
				Volume = 0.25f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num2
				}
			},
			new SoundData
			{
				KeyName = "activities/melee/punchLight1",
				Sound = "activities/melee/punchLight1",
				Volume = 0.25f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num2
				}
			},
			new SoundData
			{
				KeyName = "activities/melee/punchLight2",
				Sound = "activities/melee/punchLight2",
				Volume = 0.25f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num2
				}
			},
			new SoundData
			{
				KeyName = "activities/melee/punchLight3",
				Sound = "activities/melee/punchLight3",
				Volume = 0.25f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num2
				}
			},
			new SoundData
			{
				KeyName = "activities/melee/swooshClothHigh1",
				Sound = "activities/melee/swooshClothHigh1",
				Volume = 0.2f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num2
				}
			},
			new SoundData
			{
				KeyName = "activities/melee/swooshClothHigh2",
				Sound = "activities/melee/swooshClothHigh2",
				Volume = 0.2f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num2
				}
			},
			new SoundData
			{
				KeyName = "activities/melee/swooshClothHigh3",
				Sound = "activities/melee/swooshClothHigh3",
				Volume = 0.2f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num2
				}
			},
			new SoundData
			{
				KeyName = "activities/melee/swooshClothLow1",
				Sound = "activities/melee/swooshClothLow1",
				Volume = 0.2f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num2
				}
			},
			new SoundData
			{
				KeyName = "activities/melee/swooshClothLow2",
				Sound = "activities/melee/swooshClothLow2",
				Volume = 0.2f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = num2
				}
			},
			new SoundData
			{
				KeyName = "activities/salvage/salvageBreakMetal1",
				Sound = "activities/salvage/salvageBreakMetal1",
				Volume = 0.25f
			},
			new SoundData
			{
				KeyName = "activities/salvage/salvageBreakMetal2",
				Sound = "activities/salvage/salvageBreakMetal2",
				Volume = 0.25f
			},
			new SoundData
			{
				KeyName = "activities/salvage/salvagePullMetal",
				Sound = "activities/salvage/salvagePullMetal",
				Volume = 0.65f
			},
			new SoundData
			{
				KeyName = "activities/salvage/salvageMetalCutLow",
				Sound = "activities/salvage/salvageMetalCutLow",
				Volume = 0.65f
			},
			new SoundData
			{
				KeyName = "activities/salvage/salvageAll",
				Sound = "activities/salvage/salvageAllDeeperLong",
				Volume = 0.55f
			},
			new SoundData
			{
				KeyName = "activities/weapons/macheteImpaleHard",
				Sound = "activities/weapons/macheteImpaleHardD",
				Volume = 0.45f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.05000000074505806,
					StandardDeviation = num
				}
			},
			new SoundData
			{
				KeyName = "activities/weapons/knifeStabBloody1B",
				Sound = "activities/weapons/knifeStabBloody1B",
				Volume = 0.51f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.05000000074505806,
					StandardDeviation = num
				}
			},
			new SoundData
			{
				KeyName = "activities/weapons/knifeStab1A",
				Sound = "activities/weapons/knifeStab1A",
				Volume = 0.51f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.05000000074505806,
					StandardDeviation = num
				}
			},
			new SoundData
			{
				KeyName = "activities/weapons/coilrifleShot",
				Sound = "activities/weapons/coilrifleShot1b",
				Volume = 0.85f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.05000000074505806
				}
			},
			new SoundData
			{
				KeyName = "activities/weapons/coilRifle/coilrifleShotHard",
				Sound = "activities/weapons/coilRifle/coilrifleShotHard",
				Volume = 0.6f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.05000000074505806
				}
			},
			new SoundData
			{
				KeyName = "activities/weapons/coilrifleShot1a",
				Sound = "activities/weapons/coilrifleShot1a",
				Volume = 0.6f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.05000000074505806
				}
			},
			new SoundData
			{
				KeyName = "activities/weapons/coilrifleReload",
				Sound = "activities/weapons/coilrifleReload1b",
				Volume = 0.2f
			},
			new SoundData
			{
				KeyName = "activities/weapons/extinguisherBoostShot",
				Sound = "activities/weapons/extinguisherBoostShot1a",
				Volume = 0.95f
			},
			new SoundData
			{
				KeyName = "activities/weapons/bow/bowShot2A",
				Sound = "activities/weapons/bow/bowShot2A",
				Volume = 0.85f
			},
			new SoundData
			{
				KeyName = "activities/weapons/bow/arrowImpact2A",
				Sound = "activities/weapons/bow/arrowImpact2A",
				Volume = 0.5f
			},
			new SoundData
			{
				KeyName = "activities/weapons/arrowImpact1a",
				Sound = "activities/weapons/arrowImpact1a",
				Volume = 0.3f
			},
			new SoundData
			{
				KeyName = "activities/weapons/bow/bowAim2A",
				Sound = "activities/weapons/bow/bowAim2A",
				Volume = 0.65f
			},
			new SoundData
			{
				KeyName = "activities/weapons/bow/bowReload2A",
				Sound = "activities/weapons/bow/bowReload2A",
				Volume = 0.65f
			},
			new SoundData
			{
				KeyName = "activities/weapons/sentryGun/sentryMedium2Burst",
				Sound = "activities/weapons/sentryGun/sentryMedium2Burst",
				Volume = 0.85f
			},
			new SoundData
			{
				KeyName = "activities/weapons/sentryGun/sentryBurstCasingsHigh",
				Sound = "activities/weapons/sentryGun/sentryBurstCasingsHigh",
				Volume = 0.32f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.07000000029802322
				}
			},
			new SoundData
			{
				KeyName = "activities/weapons/sentryGun/sentryBurstCasingsLow",
				Sound = "activities/weapons/sentryGun/sentryBurstCasingsLow",
				Volume = 0.32f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.07000000029802322
				}
			},
			new SoundData
			{
				KeyName = "activities/crafting/craftingGeneral1",
				Sound = "activities/crafting/craftingGeneral1b",
				Volume = 0.65f
			},
			new SoundData
			{
				KeyName = "activities/crafting/craftingGeneral2",
				Sound = "activities/crafting/craftingGeneral2b",
				Volume = 0.65f
			},
			new SoundData
			{
				KeyName = "traps/mineExplosionHardwDebris",
				Sound = "traps/mineExplosionHardwDebris",
				Volume = 0.8f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.03999999910593033
				}
			},
			new SoundData
			{
				KeyName = "traps/mineDetonateBeep",
				Sound = "traps/mineDetonateBeep",
				Volume = 0.6f
			},
			new SoundData
			{
				KeyName = "domesticated/dog/dogAttackSnarl1",
				Sound = "domesticated/dog/dogAttackSnarl1",
				Volume = 0.3f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.07999999821186066
				}
			},
			new SoundData
			{
				KeyName = "domesticated/dog/dogAttackSnarl2",
				Sound = "domesticated/dog/dogAttackSnarl2",
				Volume = 0.3f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.20000000298023224,
					StandardDeviation = 0.15000000596046448
				}
			},
			new SoundData
			{
				KeyName = "domesticated/dog/dogEat",
				Sound = "domesticated/dog/dogEat",
				Volume = 0.5f
			},
			new SoundData
			{
				KeyName = "domesticated/dog/dogBark1B",
				Sound = "domesticated/dog/dogBark1B",
				Volume = 0.5f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.05999999865889549
				}
			},
			new SoundData
			{
				KeyName = "domesticated/dog/dogHitBark1",
				Sound = "domesticated/dog/dogHitBark1",
				Volume = 0.5f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.05999999865889549
				}
			},
			new SoundData
			{
				KeyName = "domesticated/dog/dogHitWhimper2",
				Sound = "domesticated/dog/dogHitWhimper2",
				Volume = 0.5f,
				RandomPitchChange = new NormalDistribution
				{
					Mean = 0.0,
					StandardDeviation = 0.05999999865889549
				}
			},
			new SoundData
			{
				KeyName = "ambient/wind/windHighFreq",
				Sound = "ambient/wind/windHighFreq2",
				Volume = 0.11f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/wind/windMidFreq",
				Sound = "ambient/wind/windMidFreq2",
				Volume = 0.08f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/wind/windHighFreqGustyLong1A",
				Sound = "ambient/wind/windHighFreqGustyLong1A",
				Volume = 0.07f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/wind/windHighFreqGustyLong1B",
				Sound = "ambient/wind/windHighFreqGustyLong1B",
				Volume = 0.07f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/wind/windHighFreqGustyLong1C",
				Sound = "ambient/wind/windHighFreqGustyLong1C",
				Volume = 0.07f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/shoreWavesCalm2A",
				Sound = "ambient/water/shoreWavesCalm2A",
				Volume = 0.1f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/shoreWavesCalm2B",
				Sound = "ambient/water/shoreWavesCalm2B",
				Volume = 0.1f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/shoreWavesCalm2C",
				Sound = "ambient/water/shoreWavesCalm2C",
				Volume = 0.1f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/shoreWavesCalmLonger3A",
				Sound = "ambient/water/shoreWavesCalmLonger3A",
				Volume = 0.1f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/shoreWavesCalmLonger3B",
				Sound = "ambient/water/shoreWavesCalmLonger3B",
				Volume = 0.1f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/shoreWavesCalmLongerLower3A",
				Sound = "ambient/water/shoreWavesCalmLonger3A",
				Volume = 0.1f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/shoreWavesCalmLongerLower3B",
				Sound = "ambient/water/shoreWavesCalmLonger3B",
				Volume = 0.1f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/waterBrook",
				Sound = "ambient/water/waterBrook",
				Volume = 0.12f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/waterBrookSmallA",
				Sound = "ambient/water/waterBrookSmallA",
				Volume = 0.04f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/waterBrookSmallB",
				Sound = "ambient/water/waterBrookSmallB",
				Volume = 0.04f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/waterBrookSmallC",
				Sound = "ambient/water/waterBrookSmallC",
				Volume = 0.04f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/waterRiverMediumStreamA",
				Sound = "ambient/water/waterRiverMediumStreamA",
				Volume = 0.08f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/waterRiverMediumStreamB",
				Sound = "ambient/water/waterRiverMediumStreamB",
				Volume = 0.08f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/waterRiverMediumStreamC",
				Sound = "ambient/water/waterRiverMediumStreamC",
				Volume = 0.08f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/mudflats1",
				Sound = "ambient/water/mudflatsCompressed1",
				Volume = 0.3f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/water/mudflats2",
				Sound = "ambient/water/mudflatsCompressed2",
				Volume = 0.3f,
				PlayMaxOneInstance = true
			},
			new SoundData
			{
				KeyName = "ambient/insects/insectFlying1A",
				Sound = "ambient/insects/insectFlying1A",
				Volume = 0.29f
			},
			new SoundData
			{
				KeyName = "ambient/insects/insectFlying1B",
				Sound = "ambient/insects/insectFlying1B",
				Volume = 0.29f
			},
			new SoundData
			{
				KeyName = "ambient/aliens/insectsSingleA",
				Sound = "ambient/aliens/insectsSingleA",
				Volume = 0.15f
			},
			new SoundData
			{
				KeyName = "ambient/aliens/insectsSingleB",
				Sound = "ambient/aliens/insectsSingleB",
				Volume = 0.15f
			},
			new SoundData
			{
				KeyName = "ambient/insects/insectGroup1A",
				Sound = "ambient/insects/insectGroup1A",
				Volume = 0.15f
			},
			new SoundData
			{
				KeyName = "ambient/insects/insectGroup1B",
				Sound = "ambient/insects/insectGroup1B",
				Volume = 0.15f
			},
			new SoundData
			{
				KeyName = "ambient/insects/insectGroup1C",
				Sound = "ambient/insects/insectGroup1C",
				Volume = 0.15f
			},
			new SoundData
			{
				KeyName = "ambient/aliens/insectsMediumGroup",
				Sound = "ambient/aliens/insectsMediumGroup",
				Volume = 0.15f
			},
			new SoundData
			{
				KeyName = "ambient/aliens/insectsLongConstantwBreak",
				Sound = "ambient/aliens/insectsLongConstantwBreak",
				Volume = 0.16f
			},
			new SoundData
			{
				KeyName = "ambient\\aliens\\rattleWoodenAmbient",
				Sound = "ambient\\aliens\\rattleWoodenAmbient",
				Volume = 0.65f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdsMediumGroup",
				Sound = "ambient/birds/birdsMediumGroup",
				Volume = 0.25f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle1A",
				Sound = "ambient/birds/birdSingle1A",
				Volume = 0.13f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle1B",
				Sound = "ambient/birds/birdSingle1B",
				Volume = 0.13f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle1C",
				Sound = "ambient/birds/birdSingle1C",
				Volume = 0.13f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle2A",
				Sound = "ambient/birds/birdSingle2A",
				Volume = 0.13f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle2B",
				Sound = "ambient/birds/birdSingle2B",
				Volume = 0.13f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle2C",
				Sound = "ambient/birds/birdSingle2C",
				Volume = 0.13f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle3A",
				Sound = "ambient/birds/birdSingle3A",
				Volume = 0.13f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle3B",
				Sound = "ambient/birds/birdSingle3B",
				Volume = 0.13f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle4A",
				Sound = "ambient/birds/birdSingle4A",
				Volume = 0.32f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle4B",
				Sound = "ambient/birds/birdSingle4B",
				Volume = 0.32f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle4C",
				Sound = "ambient/birds/birdSingle4C",
				Volume = 0.32f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle4D",
				Sound = "ambient/birds/birdSingle4D",
				Volume = 0.32f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle5A",
				Sound = "ambient/birds/birdSingle5A",
				Volume = 0.13f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle5B",
				Sound = "ambient/birds/birdSingle5B",
				Volume = 0.13f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle5C",
				Sound = "ambient/birds/birdSingle5C",
				Volume = 0.11f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle5D",
				Sound = "ambient/birds/birdSingle5D",
				Volume = 0.12f
			},
			new SoundData
			{
				KeyName = "ambient/birds/birdSingle5E",
				Sound = "ambient/birds/birdSingle5B",
				Volume = 0.06f
			},
			new SoundData
			{
				KeyName = "ambient/aliens/wormAmbienceSingle",
				Sound = "ambient/aliens/wormAmbienceSingle",
				Volume = 0.7f
			},
			new SoundData
			{
				KeyName = "ambient/aliens/swampFrogs",
				Sound = "ambient/aliens/swampFrogs",
				Volume = 0.15f
			},
			new SoundData
			{
				KeyName = "ambient/aliens/swampFrogs2",
				Sound = "ambient/aliens/swampFrogs2",
				Volume = 0.15f
			},
			new SoundData
			{
				KeyName = "ambient/aliens/swampFrogs3",
				Sound = "ambient/aliens/swampFrogs3",
				Volume = 0.15f
			},
			new SoundData
			{
				KeyName = "ambient\\aliens\\toothCricketsAmbient",
				Sound = "ambient\\aliens\\toothCricketsAmbient",
				Volume = 1f
			},
			new SoundData
			{
				KeyName = "ambient\\aliens\\angryToyAmbient",
				Sound = "ambient\\aliens\\angryToyAmbient",
				Volume = 1f
			},
			new SoundData
			{
				KeyName = "ambient\\aliens\\croakerAmbient",
				Sound = "ambient\\aliens\\croakerAmbient",
				Volume = 0.5f
			},
			new SoundData
			{
				KeyName = "ambient\\aliens\\hummingClickClackingAmbient",
				Sound = "ambient\\aliens\\hummingClickClackingAmbient",
				Volume = 1f
			},
			new SoundData
			{
				KeyName = "ambient\\aliens\\rattleBuzzAmbient",
				Sound = "ambient\\aliens\\rattleBuzzAmbient",
				Volume = 1f
			},
			new SoundData
			{
				KeyName = "ambient\\aliens\\spacechickenAmbient",
				Sound = "ambient\\aliens\\spacechickenAmbient",
				Volume = 1f
			},
			new SoundData
			{
				KeyName = "robotServoArms",
				Sound = "activities/robots/robotServoArms",
				Volume = 0.5f
			},
			new SoundData
			{
				KeyName = "robotServoArms2",
				Sound = "activities/robots/robotServoArms2",
				Volume = 0.5f
			},
			new SoundData
			{
				KeyName = "robotDriveEngineMedium",
				Sound = "activities/robots/robotDriveEngineMedium",
				Volume = 0.5f
			}
		};
	}

	protected override List<HelpTopic> InitHelpTopics()
	{
		List<HelpTopic> list = new List<HelpTopic>();
		list.Add(new HelpTopic
		{
			KeyName = "introduction",
			Name = "0: Introduction",
			FlowElements = new LayoutElement[1]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("SECTION 0: INTRODUCTION", "#COLORHEADER") + "\n \n You have been issued the Pioneer Planning Unit (PPU) to assist your group of pioneers in managing a settlement. The unit is a combination of instruments designed to coordinate your group's agreements as well as gather information about your collective, inventory and environment. \nIt is recommended that you familiarize yourself with the PPU in order to best organize your group's decisions."
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "selections",
			Name = "1: Selections",
			FlowElements = new LayoutElement[8]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("SECTION 1: SELECTIONS AND MAP", "#COLORHEADER") + "\n \nAbbreviations used: \nLMB: Left mouse button. RMB: Right mouse button. \n \nTERRAIN VIEW \nRMB-drag on terrain to move the terrain view. \nLMB-click/drag in the mini-map to center the terrain view at the chosen spot. \n \nSELECTION ZONE \nLMB-drag on terrain to make a multi-tile selection zone. \nLMB-click on terrain to make a single-tile selection zone. \nInformation about the zone will appear in the side panel. \n \nRMB-click on terrain to remove selection zone and side panel."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "help_1_zoneSidePanel"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = "\n \nSELECTING AN ENTITY \n('Entity' is the word used for an individual item, structure, creature or human.) \nTo select an entity, drag a zone around the entity on the terrain. Then, either: \n \nRepeatedly click the 'cycle entity' button at the corner of the zone:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tut_16_signalPyreCycleButton"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \nOR \n \nLook at the side panel, click the ZONE button and expand the menus until you find the name of the entity. \nIf it's a person or creature, click the underlined name. \nIf it's an item, click the number button on the right side of the name. On the appearing pop-up menu, find the item you're looking for and click 'SELECT"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "help_1_selectItemList"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = "\n \nDISPLAY INFO ABOUT A SELECTED ENTITY OR ZONE \nYou can have both a zone and an entity selected at the same time. Display information about the selected zone by clicking the ZONE button on the side menu. Display information about the selected entity by clicking the ENTITY button:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "help_1_displayEntity"
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "activityZones",
			Name = "2: Activity zones",
			FlowElements = new LayoutElement[3]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("SECTION 2: ACTIVITY ZONES", "#COLORHEADER") + "\n \nA selected zone can be designated for activities by clicking NEW at the zone corner, then choosing from the dropdown menu. A zone can have several activities assigned at a time. Once an activity zone has been created, it will stay active until all orders have been carried out or you delete it. \nRMB-clicking on the terrain will hide all zones but they can be brought to reappear by LMB-clicking in the terrain view. Each zone can be modified by clicking the Expand arrow next to the zone window, this opens the zone menu giving the option to DELETE the whole zone or modify its activities. \n \nGATHER: Opens a window where you can specify what resources to collect from the zone. LMB-drag the order control slider to the right. The numbers update according to how much is available and how much has been gathered. Reduce or cancel the order by dragging the slider left. \nStanding Gather order: Click the padlock icon that appears to the left of the order control when you move you cursor there. This allows you to automate gathering (see image below)"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_standingGather"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n \nSCOUT: Designates the area for a brief reconnaissance. \n \nEXAMINE: Designates the zone for a thorough investigation. Performing this activity can reveal previously unseen resources. \n \nPATROL: Each patrol zone will have one (armed) camp member continously patrolling the area as long as the zone exists. The patrolling person will attack any threats that appear (NOTE: The patroller needs to be in 'Fearless' stance to carry out this task) \n \nHUNT: Specifies that hunting should be carried out in the zone. \nIt is also possible to designate a specific creature to be hunted by selecting the animal or, in case it's disappeared from view, the memory image that indicates its last known position. Then click the 'Expand' arrow on its Marker window (the small 'name tag') and select HUNT. \n \nSTOCKPILE: Opens a window that allows you to define the types of  items to store in the zone. It is possible to choose specific item types or use broader item categories."
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "items",
			Name = "3: Items and production",
			FlowElements = new LayoutElement[12]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("SECTION 3: ITEMS, PRODUCTION AND INVENTORY", "#COLORHEADER") + "\n \nTo see a list of all items possessed by the colony, click the PRODUCTION MANAGER button:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tut_inventoryButton"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \nClick an item category to expand. Item types written in dark text means that at least one item of that type is owned by the colony - the amount is shown next to the type name. Yellow text means that there's no item of that type in the colony's possession (The amount will say 0) \n \nIf an item can be produced, there will be an order control bar to the right. Drag the slider to request production:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tut_9_produceMashedOilTubers"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \nStanding Production Order:  Click the padlock button as seen in the image below. This allows you to automate production - when your stock falls below the specified amount, your people will automatically resume production. Switch back to 'single order' by clicking the padlock button again."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_standingProduction"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n \nINFO WINDOWS \nEach item type has two kinds of information: Data info and production info. \nHover with the cursor on the item type to bring up a short info summary. Click the DATA icon to expand the info window with further information of the item type's properties:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "help_3_dataInfo"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \nClick the PRODUCTION icon to see what can be produced with the item type. \n \nClick any item name to open a new window and see how this item is produced. Repeat as many times you like to get an overview of long production chains. \n \nNOTE: Scroll down by hovering the cursor on the scroll arrow at the bottom of the window."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "help_3_productionInfo"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n \nTo see information about item entities owned by the colony, click the amount number and select an entity from the menu that appears. This opens the possibility to DISCARD the specific item (useful when moving camp and choosing what items to leave.) The item can be reclaimed by selecting the entity and clicking CLAIM. \nThe SALVAGE option will be available if it's possible to disassemble the item:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "help_3_salvageItem"
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "building",
			Name = "4: Building structures",
			FlowElements = new LayoutElement[6]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("SECTION 4: BUILDING STRUCTURES", "#COLORHEADER") + "\n \nIn the Production Manager, structures that can be built have a 'hammer' button next to them. Click the button, then click on the game area to place the structure."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tut_buildButton"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \nAs is the case with item production, the number of options is dependent on the materials and tools owned by the colony. \nInvestigate the structure types by clicking the name bar. Click the production info icon to get an overview of the production chain, opening as many production info windows as needed."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tut_8_buildCampfireMenu_Small"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \nSALVAGE a structure by selecting it, clicking the 'Expand' arrow on the marker window (the name tag) and select SALVAGE from the menu."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tut_3b_salvage"
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "managing",
			Name = "5: Managing tasks",
			FlowElements = new LayoutElement[4]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("SECTION 5: MANAGING TASKS", "#COLORHEADER") + "\n \nYou can monitor the progress of assignments by clicking the TASK button:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tut_taskManagerButton"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \nIf any task orders have been given, they will be listed here. Click the ARROW button at each entry to see status details of materials, tools and manpower needed to carry out the job. \nIf needed, set priorities for each task: Low, Normal or High. \nSome tasks can also be cancelled from the Task manager."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "help_5_taskManager"
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "moveCamp",
			Name = "6: Move camp",
			FlowElements = new LayoutElement[3]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("SECTION 6: MOVE CAMP", "#COLORHEADER") + "\n \nClick the 'CAMP' marker window you see in the terrain view. Click the 'Expand' arrow and select MOVE CAMP from the drop-down menu, then click somewhere on dry land to designate a new camp site. This indicates a new assembly point for your colony members and a new area for storing supplies."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tut_7a1_moveCampMenu"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \nTo avoid camp members carrying unneeded items to the new site, select the items that you want to leave at the old site and click DISCARD from the item list window (as described in SECTION 1: SELECTIONS AND MAP). The items can later be added to the colony again by selecting them and clicking CLAIM."
					}
				}
			}
		});
		return list;
	}

	protected override List<EventActionType> InitEventActionTypes()
	{
		return EventActionsLoader.Init();
	}

	protected override List<ActionSets> InitActionSets()
	{
		return ActionSetsLoader.Init();
	}

	protected override List<DamageType> InitDamageTypes()
	{
		return new List<DamageType>
		{
			new DamageType
			{
				KeyName = "sharp"
			},
			new DamageType
			{
				KeyName = "blunt"
			},
			new DamageType
			{
				KeyName = "piercing"
			},
			new DamageType
			{
				KeyName = "fire"
			},
			new DamageType
			{
				KeyName = "bite"
			},
			new DamageType
			{
				KeyName = "antiTwinkler"
			},
			new DamageType
			{
				KeyName = "smallAnimalGrapple"
			}
		};
	}

	protected override List<AttackType> InitAttackTypes()
	{
		List<AttackType> list = new List<AttackType>();
		list.Add(new AttackType
		{
			KeyName = "trapSmallBluntAttack",
			Damage = "blunt",
			DamageMean = 50f,
			DamageStandardDeviation = 1.5f,
			AccuracyFactor = 0.8f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/melee/kickHard1B"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/weapons/bow/bowAim2A"],
			DurationInSeconds = 1f,
			ActionPointInSeconds = 0f
		});
		list.Add(new AttackType
		{
			KeyName = "trapMediumPiercingAttack",
			Damage = "piercing",
			DamageMean = 100f,
			DamageStandardDeviation = 1.5f,
			AccuracyFactor = 0.9f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/arrowImpact1a"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/weapons/bow/bowAim2A"],
			DurationInSeconds = 1f,
			ActionPointInSeconds = 0f
		});
		list.Add(new AttackType
		{
			KeyName = "killAnimal",
			Damage = "blunt",
			DamageMean = 5000f,
			DamageStandardDeviation = 0f,
			AccuracyFactor = 1f,
			SoundAtStart = GameData.Instance.AllSoundData["activities/weapons/macheteImpaleHard"],
			DurationInSeconds = 1f,
			ActionPointInSeconds = 0f
		});
		list.Add(new AttackType
		{
			KeyName = "smallExplosionAttack",
			Damage = "blunt",
			DamageMean = 500f,
			DamageStandardDeviation = 1.5f,
			AccuracyFactor = 1f,
			AreaAttack = new ConeAttack
			{
				Length = 48f,
				WidthInDegrees = 360f
			},
			StartEffects = new Effects
			{
				ParticleEmitters = new ParticleEmitterEffect[3]
				{
					new ParticleEmitterEffect
					{
						DurationInSeconds = 0.1,
						AttachToEntity = false,
						ParticleSystemKey = "explosionSmokeCloud"
					},
					new ParticleEmitterEffect
					{
						DurationInSeconds = 0.1,
						AttachToEntity = false,
						ParticleSystemKey = "explosion"
					},
					new ParticleEmitterEffect
					{
						DurationInSeconds = 0.1,
						AttachToEntity = false,
						ParticleSystemKey = "mineExplosion"
					}
				}
			},
			ActionPointSound = GameData.Instance.AllSoundData["traps/mineExplosionHardwDebris"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/weapons/bow/bowAim2A"],
			DurationInSeconds = 0f,
			ActionPointInSeconds = 0f
		});
		list.Add(new AttackType
		{
			KeyName = "snatcherGrabAttack",
			Damage = "sharp",
			DamageMean = 15f,
			DamageStandardDeviation = 1.5f,
			SoundAtStart = GameData.Instance.AllSoundData["aliens/alienCombat/snatcherAttack"],
			MaxRestTimeInSeconds = 3f,
			MinRestTimeInSeconds = 1f,
			ChanceToRest = 0.4000000059604645,
			DurationInSeconds = 1.6f,
			ActionPointInSeconds = 0.56f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.High,
				AnimModifier.Extreme
			},
			RequiredSkill = "unarmedFighting"
		});
		list.Add(new AttackType
		{
			KeyName = "snatcherFastAttack",
			Damage = "sharp",
			DamageMean = 15f,
			DamageStandardDeviation = 1.5f,
			SoundAtStart = GameData.Instance.AllSoundData["aliens/alienCombat/snatcherAttack"],
			MaxRestTimeInSeconds = 3f,
			MinRestTimeInSeconds = 1f,
			ChanceToRest = 0.20000000298023224,
			DurationInSeconds = 0.8f,
			ActionPointInSeconds = 0.32f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.Low,
				AnimModifier.Right
			},
			RequiredSkill = "unarmedFighting"
		});
		list.Add(new AttackType
		{
			KeyName = "patricianHighDouble",
			Damage = "piercing",
			DamageMean = 17f,
			DamageStandardDeviation = 1.5f,
			ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnapDouble"],
			ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
			MaxRestTimeInSeconds = 0.6f,
			MinRestTimeInSeconds = 0.2f,
			ChanceToRest = 0.4000000059604645,
			DurationInSeconds = 0.92f,
			ActionPointInSeconds = 0.375f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.High,
				AnimModifier.Extreme
			},
			RequiredSkill = "unarmedFighting"
		});
		list.Add(new AttackType
		{
			KeyName = "patricianLowRight",
			Damage = "piercing",
			DamageMean = 13f,
			DamageStandardDeviation = 1.5f,
			ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnap"],
			ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
			DurationInSeconds = 1f,
			ActionPointInSeconds = 0.54f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.Low,
				AnimModifier.Right
			},
			RequiredSkill = "unarmedFighting"
		});
		list.Add(new AttackType
		{
			KeyName = "demonTreeAttack",
			Damage = "smallAnimalGrapple",
			DamageMean = 40f,
			DamageStandardDeviation = 1f,
			ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnapDouble"],
			ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
			SoundAtStart = GameData.Instance.AllSoundData["aliens/rattleWoodenShort"],
			DurationInSeconds = 3f,
			ActionPointInSeconds = 2f,
			AnimationStates = new AnimModifier[0],
			RequiredSkill = "unarmedFighting"
		});
		list.Add(new AttackType
		{
			KeyName = "spikePlantAttack",
			Damage = "piercing",
			DamageMean = 10f,
			DamageStandardDeviation = 1.5f,
			AreaAttack = new ConeAttack
			{
				Length = 48f,
				WidthInDegrees = 360f
			},
			ActionPointSound = GameData.Instance.AllSoundData["aliens/sliceSnapDouble"],
			ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
			SoundAtStart = GameData.Instance.AllSoundData["aliens/rattleWoodenShort"],
			DurationInSeconds = 0.12f,
			ActionPointInSeconds = 0.52f,
			AnimationStates = new AnimModifier[1] { AnimModifier.Low },
			RequiredSkill = "unarmedFighting"
		});
		list.Add(new AttackType
		{
			KeyName = "wormAttack",
			Damage = "piercing",
			DamageMean = 20f,
			DamageStandardDeviation = 1.5f,
			ActionPointSound = GameData.Instance.AllSoundData["aliens/alienCombat/wormAttackPart2"],
			ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
			SoundAtStart = GameData.Instance.AllSoundData["aliens/alienCombat/wormAttackPart1"],
			DurationInSeconds = 2.8f,
			ActionPointInSeconds = 1.56f,
			AnimationStates = new AnimModifier[0],
			RequiredSkill = "unarmedFighting"
		});
		list.Add(new AttackType
		{
			KeyName = "dogBiting",
			DefenseRating = 0.15f,
			Damage = "bite",
			DamageMean = 10f,
			DamageStandardDeviation = 2f,
			ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
			MaxRestTimeInSeconds = 0.8f,
			MinRestTimeInSeconds = 0f,
			DurationInSeconds = 1.36f,
			ActionPointInSeconds = 0.28f,
			AnimationStates = new AnimModifier[1] { AnimModifier.Low },
			RequiredSkill = "unarmedFighting"
		});
		list.Add(new AttackType
		{
			KeyName = "twinklerLowRight",
			Damage = "piercing",
			DamageMean = 8f,
			DamageStandardDeviation = 1.5f,
			ActionPointSound = GameData.Instance.AllSoundData["aliens/twinklerStab"],
			ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
			MaxRestTimeInSeconds = 0.8f,
			MinRestTimeInSeconds = 0f,
			DurationInSeconds = 0.68f,
			ActionPointInSeconds = 0.29f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.Low,
				AnimModifier.Right
			},
			RequiredSkill = "unarmedFighting"
		});
		list.Add(new AttackType
		{
			KeyName = "twinklerHighRight",
			Damage = "piercing",
			DamageMean = 8f,
			DamageStandardDeviation = 1.5f,
			ActionPointSound = GameData.Instance.AllSoundData["aliens/twinklerStab"],
			ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
			MaxRestTimeInSeconds = 1.1f,
			MinRestTimeInSeconds = 0.2f,
			DurationInSeconds = 1f,
			ActionPointInSeconds = 0.5f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.High,
				AnimModifier.Right
			},
			RequiredSkill = "unarmedFighting"
		});
		float num = 1f;
		float num2 = 0.4f;
		list.Add(new AttackType
		{
			KeyName = "bushDragonSpray",
			DefenseRating = 0.25f,
			Damage = "antiTwinkler",
			DamageMean = 3f,
			DamageStandardDeviation = 1.5f,
			ActionPointSound = GameData.Instance.AllSoundData["aliens/alienCombat/bushdragonPoisonShot"],
			ImpactSound = GameData.Instance.AllSoundData["aliens/alienCombat/poisonAlienImpact"],
			SoundAtStart = GameData.Instance.AllSoundData["aliens/alienCombat/bushdragonAttack"],
			DurationInSeconds = 1f,
			ActionPointInSeconds = 0.4f,
			AnimationStates = new AnimModifier[1] { AnimModifier.Near },
			RequiredSkill = "unarmedFighting",
			MaxRange = 100f,
			RangeType = AttackType.RangeTypes.Ray,
			AccuracyFactor = 2f,
			AreaAttack = new ConeAttack
			{
				Length = 120f,
				WidthInDegrees = 26f
			},
			ActionPointEffects = new Effects
			{
				ParticleEmitters = new ParticleEmitterEffect[1]
				{
					new ParticleEmitterEffect
					{
						AttachToEntity = true,
						EmitParticlesInParentDirection = true,
						Intensity = 1f,
						ParticleSystemKey = "bushDragonSpray",
						DurationInSeconds = num - num2
					}
				}
			}
		});
		BodyType bodyType = GameData.Instance.AllBodyTypes["humanoid"];
		list.Add(new AttackType
		{
			KeyName = "personPunchHighRight",
			DefenseRating = 0.05f,
			Damage = "blunt",
			DamageMean = 3f,
			DamageStandardDeviation = 1f,
			AccuracyFactor = 1f,
			ImpactSound = GameData.Instance.AllSoundData["activities/melee/punchLight1"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh1"],
			DurationInSeconds = 0.6f,
			MissDurationInSeconds = 0.72f,
			ActionPointInSeconds = 0.41f,
			AnimationStates = new AnimModifier[3]
			{
				AnimModifier.High,
				AnimModifier.Right,
				AnimModifier.Near
			},
			RequiredSkill = "unarmedFighting",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		float num3 = 7f;
		list.Add(new AttackType
		{
			KeyName = "knifeHack",
			DefenseRating = 0.05f,
			Damage = "sharp",
			DamageMean = num3,
			DamageStandardDeviation = 2.5f,
			AccuracyFactor = 1f,
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh2"],
			DurationInSeconds = 0.6f,
			ActionPointInSeconds = 0.4f,
			AnimationStates = new AnimModifier[3]
			{
				AnimModifier.Right,
				AnimModifier.Near,
				AnimModifier.Knife
			},
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "improvisedKnifeHack",
			DefenseRating = 0.05f,
			Damage = "sharp",
			DamageMean = 0.66f * num3,
			DamageStandardDeviation = 2.5f,
			AccuracyFactor = 1f,
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh2"],
			DurationInSeconds = 0.6f,
			ActionPointInSeconds = 0.4f,
			AnimationStates = new AnimModifier[3]
			{
				AnimModifier.Right,
				AnimModifier.Near,
				AnimModifier.Knife
			},
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "personPunchLowRight",
			DefenseRating = 0.05f,
			Damage = "blunt",
			DamageMean = 3f,
			DamageStandardDeviation = 1f,
			AccuracyFactor = 1f,
			ImpactSound = GameData.Instance.AllSoundData["activities/melee/punchLight2"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh2"],
			DurationInSeconds = 0.8f,
			MissDurationInSeconds = 0.72f,
			ActionPointInSeconds = 0.41f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.Right,
				AnimModifier.Near
			},
			IsDownAttack = true,
			RequiredSkill = "unarmedFighting",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "personSnapkickRight",
			DefenseRating = 0.05f,
			Damage = "blunt",
			DamageMean = 4f,
			DamageStandardDeviation = 1f,
			AccuracyFactor = 0.9f,
			ImpactSound = GameData.Instance.AllSoundData["activities/melee/kickHard1B"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothLow1"],
			DurationInSeconds = 0.52f,
			ActionPointInSeconds = 0.21f,
			MissDurationInSeconds = 0.72f,
			AnimationStates = new AnimModifier[3]
			{
				AnimModifier.Low,
				AnimModifier.Right,
				AnimModifier.Near
			},
			IsDownAttack = true,
			RequiredSkill = "unarmedFighting",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right leg") },
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.4f
		});
		list.Add(new AttackType
		{
			KeyName = "personStompRight",
			DefenseRating = 0.05f,
			Damage = "blunt",
			DamageMean = 6f,
			DamageStandardDeviation = 2f,
			AccuracyFactor = 0.8f,
			ImpactSound = GameData.Instance.AllSoundData["activities/melee/kickHard2"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothLow2"],
			DurationInSeconds = 0.64f,
			ActionPointInSeconds = 0.46f,
			MissDurationInSeconds = 0.72f,
			AnimationStates = new AnimModifier[4]
			{
				AnimModifier.Low,
				AnimModifier.Right,
				AnimModifier.Near,
				AnimModifier.Extreme
			},
			IsDownAttack = true,
			RequiredSkill = "unarmedFighting",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right leg") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "personHack",
			DefenseRating = 0.15f,
			Damage = "sharp",
			DamageMean = 20f,
			DamageStandardDeviation = 2f,
			AccuracyFactor = 1f,
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/macheteImpaleHard"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh3"],
			DurationInSeconds = 0.6f,
			ActionPointInSeconds = 0.4f,
			AnimationStates = new AnimModifier[4]
			{
				AnimModifier.Right,
				AnimModifier.Near,
				AnimModifier.Machete,
				AnimModifier.Axe
			},
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "personHammerBlow",
			DefenseRating = 0.05f,
			Damage = "blunt",
			DamageMean = 10f,
			DamageStandardDeviation = 2f,
			AccuracyFactor = 0.9f,
			ImpactSound = GameData.Instance.AllSoundData["activities/melee/kickHard2"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh3"],
			DurationInSeconds = 0.6f,
			ActionPointInSeconds = 0.4f,
			AnimationStates = new AnimModifier[3]
			{
				AnimModifier.Right,
				AnimModifier.Near,
				AnimModifier.Hammer
			},
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "personPickaxeHack",
			DefenseRating = 0.15f,
			Damage = "piercing",
			DamageMean = 20f,
			DamageStandardDeviation = 2f,
			AccuracyFactor = 1f,
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/macheteImpaleHard"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh3"],
			DurationInSeconds = 0.6f,
			ActionPointInSeconds = 0.4f,
			AnimationStates = new AnimModifier[3]
			{
				AnimModifier.Right,
				AnimModifier.Near,
				AnimModifier.Pickaxe
			},
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "personCrudeSpearThrustMid",
			DefenseRating = 0.1f,
			Damage = "piercing",
			DamageMean = 12f,
			DamageStandardDeviation = 2f,
			AccuracyFactor = 1f,
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh2"],
			DurationInSeconds = 0.84f,
			ActionPointInSeconds = 0.44f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.Near,
				AnimModifier.Spear
			},
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "personFlintSpearThrustMid",
			DefenseRating = 0.15f,
			Damage = "piercing",
			DamageMean = 23f,
			DamageStandardDeviation = 2f,
			AccuracyFactor = 1f,
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh1"],
			DurationInSeconds = 0.84f,
			ActionPointInSeconds = 0.44f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.Near,
				AnimModifier.Spear
			},
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "personMetalSpearThrustMid",
			DefenseRating = 0.15f,
			Damage = "piercing",
			DamageMean = 28f,
			DamageStandardDeviation = 2f,
			AccuracyFactor = 1f,
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothHigh2"],
			DurationInSeconds = 0.84f,
			ActionPointInSeconds = 0.44f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.Near,
				AnimModifier.Spear
			},
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "personHoeHack",
			DefenseRating = 0.05f,
			Damage = "sharp",
			DamageMean = 12f,
			DamageStandardDeviation = 2f,
			AccuracyFactor = 0.8f,
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothLow2"],
			DurationInSeconds = 0.84f,
			ActionPointInSeconds = 0.44f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.Near,
				AnimModifier.Hoe
			},
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "personShovelHack",
			DefenseRating = 0.05f,
			Damage = "sharp",
			DamageMean = 9f,
			DamageStandardDeviation = 3f,
			AccuracyFactor = 0.8f,
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/knifeStabBloody1B"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothLow2"],
			DurationInSeconds = 0.84f,
			ActionPointInSeconds = 0.44f,
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.Near,
				AnimModifier.Shovel
			},
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "hitWithRifleButt",
			Damage = "blunt",
			DamageMean = 10f,
			DamageStandardDeviation = 1f,
			AccuracyFactor = 0.8f,
			ActionPointInSeconds = 0.44f,
			DurationInSeconds = 0.88f,
			ImpactSound = GameData.Instance.AllSoundData["activities/melee/kickHard2"],
			SoundAtStart = GameData.Instance.AllSoundData["activities/melee/swooshClothLow2"],
			AnimationStates = new AnimModifier[1] { AnimModifier.Near },
			RequiredSkill = "armedMelee",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			RangeType = AttackType.RangeTypes.Melee,
			ChanceToRest = 0.3,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "shootUnrifledBullet",
			DefenseRating = 0.25f,
			Damage = "piercing",
			DamageMean = 50f,
			DamageStandardDeviation = 4f,
			AccuracyFactor = 0.85f,
			ActionPointInSeconds = 0.1f,
			DurationInSeconds = 1.24f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "shooting",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			MaxRange = 130f,
			RangeType = AttackType.RangeTypes.Ray,
			ActionPointEffects = new Effects
			{
				ParticleEmitters = new ParticleEmitterEffect[1]
				{
					new ParticleEmitterEffect
					{
						AttachToEntity = true,
						EmitParticlesInParentDirection = true,
						Intensity = 1f,
						ParticleSystemKey = "gunSmoke",
						DurationInSeconds = 12.0
					}
				}
			},
			UsesAmmo = "item:blackPowderShotAmmo",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.75f,
			MaxRestTimeInSeconds = 2f
		});
		list.Add(new AttackType
		{
			KeyName = "shootBlunderbuss",
			DefenseRating = 0.25f,
			Damage = "piercing",
			DamageMean = 26f,
			DamageStandardDeviation = 3f,
			AccuracyFactor = 1.5f,
			ActionPointInSeconds = 0.1f,
			DurationInSeconds = 1.24f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "shooting",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			MaxRange = 100f,
			RangeType = AttackType.RangeTypes.Ray,
			AreaAttack = new ConeAttack
			{
				Length = 110f,
				WidthInDegrees = 10f
			},
			ActionPointEffects = new Effects
			{
				ParticleEmitters = new ParticleEmitterEffect[2]
				{
					new ParticleEmitterEffect
					{
						AttachToEntity = true,
						EmitParticlesInParentDirection = true,
						Intensity = 1f,
						ParticleSystemKey = "buckShotCloud",
						DurationInSeconds = 1.0
					},
					new ParticleEmitterEffect
					{
						AttachToEntity = true,
						EmitParticlesInParentDirection = true,
						Intensity = 1f,
						ParticleSystemKey = "gunSmoke",
						DurationInSeconds = 12.0
					}
				}
			},
			UsesAmmo = "item:blackPowderShotAmmo",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.75f,
			MaxRestTimeInSeconds = 2f
		});
		list.Add(new AttackType
		{
			KeyName = "shootShotgun",
			DefenseRating = 0.55f,
			Damage = "piercing",
			DamageMean = 28f,
			DamageStandardDeviation = 3f,
			AccuracyFactor = 1.5f,
			ActionPointInSeconds = 0.1f,
			DurationInSeconds = 1.24f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "shooting",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			MaxRange = 180f,
			RangeType = AttackType.RangeTypes.Ray,
			AreaAttack = new ConeAttack
			{
				Length = 140f,
				WidthInDegrees = 10f
			},
			ActionPointEffects = new Effects
			{
				ParticleEmitters = new ParticleEmitterEffect[2]
				{
					new ParticleEmitterEffect
					{
						AttachToEntity = true,
						EmitParticlesInParentDirection = true,
						Intensity = 1f,
						ParticleSystemKey = "goldBuckShotCloud",
						DurationInSeconds = 1.0
					},
					new ParticleEmitterEffect
					{
						AttachToEntity = true,
						EmitParticlesInParentDirection = true,
						Intensity = 0.5f,
						ParticleSystemKey = "gunSmoke",
						DurationInSeconds = 12.0
					}
				}
			},
			UsesAmmo = "item:shotgunAmmo",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "shootCoilRifle",
			DefenseRating = 0.8f,
			Damage = "piercing",
			DamageMean = 100f,
			DamageStandardDeviation = 10f,
			AccuracyFactor = 1.5f,
			ActionPointInSeconds = 0.6f,
			DurationInSeconds = 1.24f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilrifleShot"],
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "shooting",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			MaxRange = 360f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = 20f,
				StartColor = Color.Gray,
				EndColor = Color.LightGray
			},
			UsesAmmo = "item:coilRifleAmmo",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "sentryShootGun",
			DefenseRating = 0.8f,
			Damage = "piercing",
			DamageMean = 5f,
			DamageStandardDeviation = 3f,
			AccuracyFactor = 0.8f,
			ActionPointInSeconds = 0.01f,
			DurationInSeconds = 0.02f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "shooting",
			MaxRange = 360f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = 20f,
				StartColor = Color.Yellow,
				EndColor = Color.LightYellow
			},
			RoundsToSpend = 1,
			ChanceToRest = 0.02,
			MinRestTimeInSeconds = 0.2f,
			MaxRestTimeInSeconds = 0.4f
		});
		list.Add(new AttackType
		{
			KeyName = "sentryGunBurst",
			DefenseRating = 0.8f,
			Damage = "piercing",
			DamageMean = 36f,
			DamageStandardDeviation = 3f,
			ActionPointInSeconds = 0.04f,
			DurationInSeconds = 0.2f,
			SoundAtStart = GameData.Instance.AllSoundData["activities/weapons/sentryGun/sentryBurstCasingsLow"],
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/sentryGun/sentryMedium2Burst"],
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "shooting",
			MaxRange = 240f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = 20f,
				StartColor = Color.Yellow,
				EndColor = Color.LightYellow
			},
			UsesAmmo = "item:sentryGunAmmo",
			RoundsToSpend = 3,
			ChanceToRest = 0.1,
			MinRestTimeInSeconds = 0.1f,
			MaxRestTimeInSeconds = 0.15f
		});
		list.Add(new AttackType
		{
			KeyName = "sentryLaserGunShot",
			DefenseRating = 0.8f,
			Damage = "piercing",
			DamageMean = 20f,
			DamageStandardDeviation = 3f,
			ActionPointInSeconds = 0.01f,
			DurationInSeconds = 0.1f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilrifleShot1a"],
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "shooting",
			MaxRange = 150f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = 20f,
				StartColor = Color.Red,
				EndColor = Color.Tomato
			},
			RoundsToSpend = 0,
			ChanceToRest = 1.0,
			MinRestTimeInSeconds = 3f,
			MaxRestTimeInSeconds = 3f
		});
		list.Add(new AttackType
		{
			KeyName = "shootCorditeRifledBullet",
			DefenseRating = 0.55f,
			Damage = "piercing",
			DamageMean = 75f,
			DamageStandardDeviation = 5f,
			AccuracyFactor = 1.1f,
			ActionPointInSeconds = 0.6f,
			DurationInSeconds = 1.5f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
			ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "shooting",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			MaxRange = 325f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = 20f,
				StartColor = Color.Gray,
				EndColor = Color.LightGray
			},
			UsesAmmo = "item:corditeAmmo",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "shootGunpowderRifle",
			DefenseRating = 0.3f,
			Damage = "piercing",
			DamageMean = 55f,
			DamageStandardDeviation = 5f,
			AccuracyFactor = 0.95f,
			ActionPointInSeconds = 0.6f,
			DurationInSeconds = 1.5f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/coilRifle/coilrifleShotHard"],
			ImpactSound = GameData.Instance.AllSoundData["melee/STAB2_24 - 4 Stabs With Blood"],
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "shooting",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			MaxRange = 200f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = 20f,
				StartColor = Color.Gray,
				EndColor = Color.LightGray
			},
			ActionPointEffects = new Effects
			{
				ParticleEmitters = new ParticleEmitterEffect[1]
				{
					new ParticleEmitterEffect
					{
						AttachToEntity = false,
						EmitParticlesInParentDirection = true,
						Intensity = 1f,
						ParticleSystemKey = "gunpowderSmoke",
						DurationInSeconds = 5.0
					}
				}
			},
			UsesAmmo = "item:blackPowderRifleAmmo",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		float muzzleDistance = 20f;
		list.Add(new AttackType
		{
			KeyName = "shootImprovisedBasicArrow",
			DefenseRating = 0.15f,
			Damage = "piercing",
			DamageMean = 18f,
			DamageStandardDeviation = 3f,
			AccuracyFactor = 0.85f,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/bow/bowShot2A"],
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/bow/arrowImpact2A"],
			DurationInSeconds = 2.24f,
			ActionPointInSeconds = 0.96f,
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "archery",
			DependsOn = new BodyPartType[2]
			{
				bodyType.FindBodyPart("Right arm"),
				bodyType.FindBodyPart("Left arm")
			},
			MaxRange = 100f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = muzzleDistance,
				StartColor = Color.Brown,
				EndColor = Color.SandyBrown
			},
			UsesAmmo = "item:improvisedBasicArrow",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		float accuracyFactor = 1f;
		list.Add(new AttackType
		{
			KeyName = "shootImprovisedChitinousArrow",
			DefenseRating = 0.15f,
			Damage = "piercing",
			DamageMean = 20f,
			DamageStandardDeviation = 3f,
			AccuracyFactor = accuracyFactor,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/bow/bowShot2A"],
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/bow/arrowImpact2A"],
			DurationInSeconds = 2.24f,
			ActionPointInSeconds = 0.96f,
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "archery",
			DependsOn = new BodyPartType[2]
			{
				bodyType.FindBodyPart("Right arm"),
				bodyType.FindBodyPart("Left arm")
			},
			MaxRange = 100f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = muzzleDistance,
				StartColor = Color.Brown,
				EndColor = Color.SandyBrown
			},
			UsesAmmo = "item:improvisedChitinousArrow",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "shootImprovisedMetalArrow",
			DefenseRating = 0.15f,
			Damage = "piercing",
			DamageMean = 24f,
			DamageStandardDeviation = 3f,
			AccuracyFactor = accuracyFactor,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/bow/bowShot2A"],
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/bow/arrowImpact2A"],
			DurationInSeconds = 2.24f,
			ActionPointInSeconds = 0.96f,
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "archery",
			DependsOn = new BodyPartType[2]
			{
				bodyType.FindBodyPart("Right arm"),
				bodyType.FindBodyPart("Left arm")
			},
			MaxRange = 100f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = muzzleDistance,
				StartColor = Color.Brown,
				EndColor = Color.SandyBrown
			},
			UsesAmmo = "item:improvisedMetalArrow",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		list.Add(new AttackType
		{
			KeyName = "shootIronArrow",
			DefenseRating = 0.15f,
			Damage = "piercing",
			DamageMean = 26f,
			DamageStandardDeviation = 3f,
			AccuracyFactor = accuracyFactor,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/bow/bowShot2A"],
			ImpactSound = GameData.Instance.AllSoundData["activities/weapons/bow/arrowImpact2A"],
			DurationInSeconds = 2.24f,
			ActionPointInSeconds = 0.96f,
			AnimationStates = new AnimModifier[1] { AnimModifier.Far },
			RequiredSkill = "archery",
			DependsOn = new BodyPartType[2]
			{
				bodyType.FindBodyPart("Right arm"),
				bodyType.FindBodyPart("Left arm")
			},
			MaxRange = 100f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = muzzleDistance,
				StartColor = Color.Brown,
				EndColor = Color.SandyBrown
			},
			UsesAmmo = "item:ironArrow",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		float num4 = 0.18f;
		float num5 = 1.2f;
		list.Add(new AttackType
		{
			KeyName = "shootImprovedFireExtinguisherBushDragonPoison",
			DefenseRating = 0.25f,
			Damage = "antiTwinkler",
			DamageMean = 120f,
			DamageStandardDeviation = 1.5f,
			DurationInSeconds = num5,
			ActionPointInSeconds = num4,
			ActionPointSound = GameData.Instance.AllSoundData["activities/weapons/extinguisherBoostShot"],
			ImpactSound = GameData.Instance.AllSoundData["aliens/alienCombat/poisonAlienImpact"],
			AnimationStates = new AnimModifier[2]
			{
				AnimModifier.Far,
				AnimModifier.Low
			},
			RequiredSkill = "menial",
			DependsOn = new BodyPartType[1] { bodyType.FindBodyPart("Right arm") },
			MaxRange = 144f,
			RangeType = AttackType.RangeTypes.Ray,
			AccuracyFactor = 2f,
			AreaAttack = new ConeAttack
			{
				Length = 144f,
				WidthInDegrees = 26f
			},
			ActionPointEffects = new Effects
			{
				ParticleEmitters = new ParticleEmitterEffect[1]
				{
					new ParticleEmitterEffect
					{
						AttachToEntity = true,
						EmitParticlesInParentDirection = true,
						Intensity = 1f,
						ParticleSystemKey = "fireExtinguisherPoison",
						DurationInSeconds = num5 - num4
					}
				}
			},
			UsesAmmo = "item:bushDragonCartridge",
			RoundsToSpend = 1,
			ChanceToRest = 0.8,
			MinRestTimeInSeconds = 0.4f,
			MaxRestTimeInSeconds = 1.2f
		});
		return list;
	}

	protected override List<EntityType> InitEntityTypes()
	{
		base.InitEntityTypes();
		List<EntityType> list = new List<EntityType>();
		TerrainFeatureLoader.Init(list);
		StructureLoader.Init(list);
		CreatureLoader.Init(list);
		TreeLoader.Init(list);
		ItemLoader.Init(list);
		InitMiscEntityTypes(list);
		// UNHIDDEN MOD: adds peat charcoal and retags forge fuel. Hooked here rather than at the
		// end of ItemLoader.Init / StructureLoader.Init, which is where the Harmony patch
		// postfixed, because both loaders feed this one list and the order is the same.
		if (UWGame.Mods.UnhiddenMod.Enabled)
		{
			UWGame.Mods.UnhiddenMod.AddItems(list);
			UWGame.Mods.UnhiddenModSmithies.Replace(list);
		}
		// DISASSEMBLY MOD: it needs the entity table when the PROCESS table is built, several
		// steps later. See DisassemblyMod.CaptureItems for why it cannot read GameData for it.
		UWGame.Mods.DisassemblyMod.CaptureItems(list);
		return list;
	}

	protected override List<StancesType> InitStancesTypes()
	{
		List<StancesType> list = new List<StancesType>();
		list.Add(new StancesType
		{
			KeyName = "humanoid",
			Stances = new string[4] { "lying", "standing", "kneeling", "sitting" },
			IdleStancesAwayFromHome = new ChanceToTakeStance[2]
			{
				new ChanceToTakeStance
				{
					Stance = "standing",
					Chance = 1f,
					AddedChanceToRemainInStance = 0.5f
				},
				new ChanceToTakeStance
				{
					Stance = "kneeling",
					Chance = 0.8f
				}
			},
			IdleStancesNearHome = new ChanceToTakeStance[3]
			{
				new ChanceToTakeStance
				{
					Stance = "standing",
					Chance = 1f,
					AddedChanceToRemainInStance = 0.5f
				},
				new ChanceToTakeStance
				{
					Stance = "kneeling",
					Chance = 0.8f
				},
				new ChanceToTakeStance
				{
					Stance = "sitting",
					Chance = 0.74f
				}
			},
			DefaultStanceWhenWorking = "standing",
			DefaultStance = "standing",
			MovingStance = "standing",
			IncapacitatedStance = "lying",
			SleepStances = new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "lying"
				}
			},
			PatrolStancesBriefWait = new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "standing"
				}
			},
			PatrolStancesLongerWait = new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "kneeling"
				}
			},
			SearchStancesBriefWait = new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "standing"
				}
			},
			SearchStancesLongerWait = new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "kneeling"
				}
			},
			StanceChangeDurations = new StancesType.StanceChangeDuration[8]
			{
				new StancesType.StanceChangeDuration
				{
					FromStance = "lying",
					ToStance = "sitting",
					Duration = 3.2
				},
				new StancesType.StanceChangeDuration
				{
					FromStance = "lying",
					ToStance = "standing",
					Duration = 4.56
				},
				new StancesType.StanceChangeDuration
				{
					FromStance = "sitting",
					ToStance = "standing",
					Duration = 1.44
				},
				new StancesType.StanceChangeDuration
				{
					FromStance = "kneeling",
					ToStance = "sitting",
					Duration = 1.44
				},
				new StancesType.StanceChangeDuration
				{
					FromStance = "kneeling",
					ToStance = "standing",
					Duration = 1.0
				},
				new StancesType.StanceChangeDuration
				{
					FromStance = "standing",
					ToStance = "lying",
					Duration = 5.44
				},
				new StancesType.StanceChangeDuration
				{
					FromStance = "standing",
					ToStance = "sitting",
					Duration = 1.44
				},
				new StancesType.StanceChangeDuration
				{
					FromStance = "standing",
					ToStance = "kneeling",
					Duration = 1.0
				}
			}
		});
		list.Add(new StancesType
		{
			KeyName = "dog",
			Stances = new string[2] { "lying", "standing" },
			IdleStancesAwayFromHome = new ChanceToTakeStance[2]
			{
				new ChanceToTakeStance
				{
					Stance = "standing",
					Chance = 0.5f,
					AddedChanceToRemainInStance = 0.22f
				},
				new ChanceToTakeStance
				{
					Stance = "lying",
					Chance = 0.5f,
					AddedChanceToRemainInStance = 0.22f
				}
			},
			IdleStancesNearHome = new ChanceToTakeStance[2]
			{
				new ChanceToTakeStance
				{
					Stance = "standing",
					Chance = 0.5f,
					AddedChanceToRemainInStance = 0.22f
				},
				new ChanceToTakeStance
				{
					Stance = "lying",
					Chance = 0.5f,
					AddedChanceToRemainInStance = 0.22f
				}
			},
			DefaultStanceWhenWorking = "standing",
			DefaultStance = "standing",
			MovingStance = "standing",
			IncapacitatedStance = "lying",
			SleepStances = new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "lying"
				}
			},
			StanceChangeDurations = new StancesType.StanceChangeDuration[2]
			{
				new StancesType.StanceChangeDuration
				{
					FromStance = "standing",
					ToStance = "lying",
					Duration = 1.68
				},
				new StancesType.StanceChangeDuration
				{
					FromStance = "lying",
					ToStance = "standing",
					Duration = 1.8
				}
			}
		});
		list.Add(new StancesType
		{
			KeyName = "robot",
			Stances = new string[2] { "active", "inactive" },
			IdleStancesAwayFromHome = new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "inactive",
					Chance = 1f
				}
			},
			IdleStancesNearHome = new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "inactive",
					Chance = 1f
				}
			},
			DefaultStanceWhenWorking = "active",
			DefaultStance = "inactive",
			MovingStance = "inactive",
			IncapacitatedStance = "inactive",
			SleepStances = new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "inactive"
				}
			},
			StanceChangeDurations = new StancesType.StanceChangeDuration[2]
			{
				new StancesType.StanceChangeDuration
				{
					FromStance = "inactive",
					ToStance = "active",
					Duration = 0.8
				},
				new StancesType.StanceChangeDuration
				{
					FromStance = "active",
					ToStance = "inactive",
					Duration = 0.8
				}
			}
		});
		return list;
	}

	protected override List<StanceType> InitStanceTypes()
	{
		return new List<StanceType>
		{
			new StanceType
			{
				KeyName = "lying",
				Number = 0,
				AnimModifier = AnimModifier.Lying,
				IsProne = true,
				IdleExertionLevel = 1.2f
			},
			new StanceType
			{
				KeyName = "sitting",
				Number = 1,
				AnimModifier = AnimModifier.Sitting,
				CanTurnBody = false,
				CanTurnHead = true,
				IdleExertionLevel = 1.2f
			},
			new StanceType
			{
				KeyName = "kneeling",
				Number = 2,
				AnimModifier = AnimModifier.Kneeling,
				CanTurnBody = false,
				CanTurnHead = true,
				IdleExertionLevel = 1.3f
			},
			new StanceType
			{
				KeyName = "standing",
				Number = 3,
				AnimModifier = null,
				CanStartIdleConversation = true,
				CanTurnBody = true,
				CanTurnHead = true,
				IdleExertionLevel = 1.4f
			},
			new StanceType
			{
				KeyName = "inactive",
				Number = 0,
				AnimModifier = AnimModifier.Inactive
			},
			new StanceType
			{
				KeyName = "active",
				Number = 1,
				AnimModifier = null
			}
		};
	}

	protected override List<BodyLayerType> InitBodyLayerTypes()
	{
		return new List<BodyLayerType>
		{
			new BodyLayerType
			{
				KeyName = "skinLayer",
				Name = "Skin",
				DamageReductionConstant = new Dictionary<string, float>
				{
					{ "bite", 4f },
					{ "sharp", 4f },
					{ "blunt", 2f },
					{ "fire", 3f },
					{ "piercing", 5f },
					{ "smallAnimalGrapple", 22f },
					{ "antiTwinkler", 600f }
				},
				DamageReductionFactor = new Dictionary<string, float>
				{
					{ "bite", 0.2f },
					{ "sharp", 0.2f },
					{ "blunt", 0.2f },
					{ "fire", 0.3f },
					{ "piercing", 0.3f },
					{ "smallAnimalGrapple", 0.3f },
					{ "antiTwinkler", 0f }
				}
			},
			new BodyLayerType
			{
				KeyName = "clothesLayer",
				Name = "Survival suit",
				DamageReductionConstant = new Dictionary<string, float>
				{
					{ "bite", 4f },
					{ "sharp", 4f },
					{ "blunt", 2f },
					{ "fire", 3f },
					{ "piercing", 5f },
					{ "smallAnimalGrapple", 22f },
					{ "antiTwinkler", 600f }
				},
				DamageReductionFactor = new Dictionary<string, float>
				{
					{ "bite", 0.2f },
					{ "sharp", 0.2f },
					{ "blunt", 0.2f },
					{ "fire", 0.3f },
					{ "piercing", 0.3f },
					{ "smallAnimalGrapple", 0.3f },
					{ "antiTwinkler", 0f }
				}
			},
			new BodyLayerType
			{
				KeyName = "thinExoSkeletonLayer",
				Name = "Shell",
				DamageReductionConstant = new Dictionary<string, float>
				{
					{ "bite", 4f },
					{ "sharp", 4f },
					{ "blunt", 1f },
					{ "fire", 3f },
					{ "piercing", 4f },
					{ "smallAnimalGrapple", 21f },
					{ "antiTwinkler", 600f }
				},
				DamageReductionFactor = new Dictionary<string, float>
				{
					{ "bite", 0.1f },
					{ "sharp", 0.1f },
					{ "blunt", 0.2f },
					{ "fire", 0.3f },
					{ "piercing", 0f },
					{ "smallAnimalGrapple", 0f },
					{ "antiTwinkler", 0.3f }
				}
			},
			new BodyLayerType
			{
				KeyName = "twinklerShell",
				Name = "Twinkler shell",
				DamageReductionConstant = new Dictionary<string, float>
				{
					{ "bite", 4f },
					{ "sharp", 4f },
					{ "blunt", 1f },
					{ "fire", 3f },
					{ "piercing", 4f },
					{ "smallAnimalGrapple", 21f },
					{ "antiTwinkler", 0f }
				},
				DamageReductionFactor = new Dictionary<string, float>
				{
					{ "bite", 0.2f },
					{ "sharp", 0.2f },
					{ "blunt", 0.2f },
					{ "fire", 0.3f },
					{ "piercing", 0f },
					{ "smallAnimalGrapple", 0f },
					{ "antiTwinkler", 0f }
				}
			},
			new BodyLayerType
			{
				KeyName = "mediumExoSkeletonLayer",
				Name = "Shell",
				DamageReductionConstant = new Dictionary<string, float>
				{
					{ "bite", 6f },
					{ "sharp", 6f },
					{ "blunt", 2f },
					{ "fire", 3f },
					{ "piercing", 5f },
					{ "smallAnimalGrapple", 22f },
					{ "antiTwinkler", 600f }
				},
				DamageReductionFactor = new Dictionary<string, float>
				{
					{ "bite", 0.1f },
					{ "sharp", 0.1f },
					{ "blunt", 0.2f },
					{ "fire", 0.3f },
					{ "piercing", 0f },
					{ "smallAnimalGrapple", 0f },
					{ "antiTwinkler", 0.3f }
				}
			},
			new BodyLayerType
			{
				KeyName = "turnipArmorLayer",
				Name = "Turnip shell",
				DamageReductionConstant = new Dictionary<string, float>
				{
					{ "bite", 60f },
					{ "sharp", 60f },
					{ "blunt", 50f },
					{ "fire", 90f },
					{ "piercing", 33f },
					{ "smallAnimalGrapple", 80f },
					{ "antiTwinkler", 600f }
				},
				DamageReductionFactor = new Dictionary<string, float>
				{
					{ "bite", 0.1f },
					{ "sharp", 0.1f },
					{ "blunt", 0.2f },
					{ "fire", 0.3f },
					{ "piercing", 0f },
					{ "smallAnimalGrapple", 0f },
					{ "antiTwinkler", 0.3f }
				}
			},
			new BodyLayerType
			{
				KeyName = "hideLayer",
				Name = "Hide",
				DamageReductionConstant = new Dictionary<string, float>
				{
					{ "bite", 2f },
					{ "sharp", 2f },
					{ "blunt", 2f },
					{ "fire", 2f },
					{ "piercing", 2f },
					{ "smallAnimalGrapple", 21f },
					{ "antiTwinkler", 600f }
				},
				DamageReductionFactor = new Dictionary<string, float>
				{
					{ "bite", 0.1f },
					{ "sharp", 0.1f },
					{ "blunt", 0.1f },
					{ "fire", 0.1f },
					{ "piercing", 0f },
					{ "smallAnimalGrapple", 0f },
					{ "antiTwinkler", 0.3f }
				}
			},
			new BodyLayerType
			{
				KeyName = "smallAnimalHideLayer",
				Name = "SmallAnimalHideLayer",
				DamageReductionConstant = new Dictionary<string, float>
				{
					{ "bite", 2f },
					{ "sharp", 2f },
					{ "blunt", 2f },
					{ "fire", 2f },
					{ "piercing", 2f },
					{ "smallAnimalGrapple", 0f },
					{ "antiTwinkler", 600f }
				},
				DamageReductionFactor = new Dictionary<string, float>
				{
					{ "bite", 0.1f },
					{ "sharp", 0.1f },
					{ "blunt", 0.1f },
					{ "fire", 0.1f },
					{ "piercing", 0f },
					{ "smallAnimalGrapple", 0f },
					{ "antiTwinkler", 0.3f }
				}
			},
			new BodyLayerType
			{
				KeyName = "robotShell",
				Name = "Robot shell",
				DamageReductionConstant = new Dictionary<string, float>
				{
					{ "bite", 60f },
					{ "sharp", 60f },
					{ "blunt", 50f },
					{ "fire", 70f },
					{ "piercing", 33f },
					{ "smallAnimalGrapple", 80f },
					{ "antiTwinkler", 600f }
				},
				DamageReductionFactor = new Dictionary<string, float>
				{
					{ "bite", 0.1f },
					{ "sharp", 0.1f },
					{ "blunt", 0.2f },
					{ "fire", 0.3f },
					{ "piercing", 0f },
					{ "smallAnimalGrapple", 0f },
					{ "antiTwinkler", 0f }
				}
			}
		};
	}

	protected override List<BodyType> InitBodyTypes()
	{
		List<BodyType> list = new List<BodyType>();
		string text = "humanoid";
		BodyType bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				Name = "Torso",
				BodyKeyName = text,
				ArmorLayer = "clothesLayer",
				OrganTypes = new OrganType[1]
				{
					new OrganType
					{
						Name = "Heart",
						IsVital = true
					}
				},
				ToHitProfileBack = 0.35f,
				ToHitProfileFront = 0.35f,
				ToHitProfileLeft = 0.1f,
				ToHitProfileRight = 0.1f,
				HitpointsFraction = 0.5f,
				BodyPartTypes = new BodyPartType[5]
				{
					new BiologicalBodyPartType
					{
						Name = "Head",
						BodyKeyName = text,
						ArmorLayer = "skinLayer",
						OrganTypes = new OrganType[1]
						{
							new OrganType
							{
								Name = "Brain",
								IsVital = true
							}
						},
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.1f,
						ToHitProfileLeft = 0.1f,
						ToHitProfileRight = 0.1f,
						HitpointsFraction = 0.2f
					},
					new BiologicalBodyPartType
					{
						Name = "Left arm",
						BodyKeyName = text,
						ArmorLayer = "clothesLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.25f,
						ToHitProfileRight = 0f,
						HitpointsFraction = 0.3f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Manipulation,
								Weight = 0.5f
							}
						}
					},
					new BiologicalBodyPartType
					{
						Name = "Right arm",
						BodyKeyName = text,
						ArmorLayer = "clothesLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0f,
						ToHitProfileRight = 0.25f,
						HitpointsFraction = 0.3f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Manipulation,
								Weight = 0.5f
							}
						}
					},
					new BiologicalBodyPartType
					{
						Name = "Left leg",
						BodyKeyName = text,
						ArmorLayer = "clothesLayer",
						ToHitProfileBack = 0.25f,
						ToHitProfileFront = 0.25f,
						ToHitProfileLeft = 0.3f,
						ToHitProfileRight = 0f,
						HitpointsFraction = 0.35f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.35f
							}
						}
					},
					new BiologicalBodyPartType
					{
						Name = "Right leg",
						BodyKeyName = text,
						ArmorLayer = "clothesLayer",
						ToHitProfileBack = 0.25f,
						ToHitProfileFront = 0.25f,
						ToHitProfileLeft = 0f,
						ToHitProfileRight = 0.3f,
						HitpointsFraction = 0.35f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.35f
							}
						}
					}
				}
			}
		};
		BodyType item = bodyType;
		list.Add(item);
		text = "patrician";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = "mediumExoSkeletonLayer",
				OrganTypes = new OrganType[1]
				{
					new OrganType
					{
						Name = "Heart",
						IsVital = true
					}
				},
				ToHitProfileBack = 0.2f,
				ToHitProfileFront = 0.2f,
				ToHitProfileLeft = 0.2f,
				ToHitProfileRight = 0.2f,
				HitpointsFraction = 0.3f,
				BodyPartTypes = new BodyPartType[5]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = "mediumExoSkeletonLayer",
						OrganTypes = new OrganType[1]
						{
							new OrganType
							{
								Name = "Brain",
								IsVital = true
							}
						},
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.1f,
						ToHitProfileLeft = 0.1f,
						ToHitProfileRight = 0.1f,
						HitpointsFraction = 0.1f
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 1",
						ArmorLayer = "mediumExoSkeletonLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 2",
						ArmorLayer = "mediumExoSkeletonLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 3",
						ArmorLayer = "mediumExoSkeletonLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 4",
						ArmorLayer = "mediumExoSkeletonLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "snatcher";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = "hideLayer",
				OrganTypes = new OrganType[1]
				{
					new OrganType
					{
						Name = "Heart",
						IsVital = true
					}
				},
				ToHitProfileBack = 0.2f,
				ToHitProfileFront = 0.2f,
				ToHitProfileLeft = 0.2f,
				ToHitProfileRight = 0.2f,
				HitpointsFraction = 0.3f,
				BodyPartTypes = new BodyPartType[5]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = "hideLayer",
						OrganTypes = new OrganType[1]
						{
							new OrganType
							{
								Name = "Brain",
								IsVital = true
							}
						},
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.2f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.1f
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 1",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 2",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 3",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 4",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "dog";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = "hideLayer",
				OrganTypes = new OrganType[1]
				{
					new OrganType
					{
						Name = "Heart",
						IsVital = true
					}
				},
				ToHitProfileBack = 0.2f,
				ToHitProfileFront = 0.2f,
				ToHitProfileLeft = 0.2f,
				ToHitProfileRight = 0.2f,
				HitpointsFraction = 0.3f,
				BodyPartTypes = new BodyPartType[5]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = "hideLayer",
						OrganTypes = new OrganType[1]
						{
							new OrganType
							{
								Name = "Brain",
								IsVital = true
							}
						},
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.2f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.1f
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 1",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 2",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 3",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 4",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "turnip";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = "turnipArmorLayer",
				OrganTypes = new OrganType[1]
				{
					new OrganType
					{
						Name = "Heart",
						IsVital = true
					}
				},
				ToHitProfileBack = 0.2f,
				ToHitProfileFront = 0.2f,
				ToHitProfileLeft = 0.2f,
				ToHitProfileRight = 0.2f,
				HitpointsFraction = 0.3f,
				BodyPartTypes = new BodyPartType[5]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = "turnipArmorLayer",
						OrganTypes = new OrganType[1]
						{
							new OrganType
							{
								Name = "Brain",
								IsVital = true
							}
						},
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.1f,
						ToHitProfileLeft = 0.1f,
						ToHitProfileRight = 0.1f,
						HitpointsFraction = 0.1f
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 1",
						ArmorLayer = "turnipArmorLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 2",
						ArmorLayer = "turnipArmorLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 3",
						ArmorLayer = "turnipArmorLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 4",
						ArmorLayer = "turnipArmorLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "twinkler";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = "twinklerShell",
				OrganTypes = new OrganType[1]
				{
					new OrganType
					{
						Name = "Heart",
						IsVital = true
					}
				},
				ToHitProfileBack = 0.4f,
				ToHitProfileFront = 0.4f,
				ToHitProfileLeft = 0.4f,
				ToHitProfileRight = 0.4f,
				HitpointsFraction = 0.5f,
				BodyPartTypes = new BodyPartType[5]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = "twinklerShell",
						OrganTypes = new OrganType[1]
						{
							new OrganType
							{
								Name = "Brain",
								IsVital = true
							}
						},
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.1f,
						ToHitProfileLeft = 0.1f,
						ToHitProfileRight = 0.1f,
						HitpointsFraction = 0.1f
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 1",
						ArmorLayer = "twinklerShell",
						ToHitProfileBack = 0.12f,
						ToHitProfileFront = 0.12f,
						ToHitProfileLeft = 0.12f,
						ToHitProfileRight = 0.12f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 2",
						ArmorLayer = "twinklerShell",
						ToHitProfileBack = 0.12f,
						ToHitProfileFront = 0.12f,
						ToHitProfileLeft = 0.12f,
						ToHitProfileRight = 0.12f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 3",
						ArmorLayer = "twinklerShell",
						ToHitProfileBack = 0.12f,
						ToHitProfileFront = 0.12f,
						ToHitProfileLeft = 0.12f,
						ToHitProfileRight = 0.12f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 4",
						ArmorLayer = "twinklerShell",
						ToHitProfileBack = 0.12f,
						ToHitProfileFront = 0.12f,
						ToHitProfileLeft = 0.12f,
						ToHitProfileRight = 0.12f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "forestguardian";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = "hideLayer",
				ToHitProfileBack = 0.35f,
				ToHitProfileFront = 0.35f,
				ToHitProfileLeft = 0.35f,
				ToHitProfileRight = 0.35f,
				HitpointsFraction = 0.5f,
				BodyPartTypes = new BodyPartType[5]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.35f,
						ToHitProfileFront = 0.35f,
						ToHitProfileLeft = 0.35f,
						ToHitProfileRight = 0.35f,
						HitpointsFraction = 0.5f
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 1",
						ArmorLayer = "thinExoSkeletonLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 2",
						ArmorLayer = "thinExoSkeletonLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 3",
						ArmorLayer = "thinExoSkeletonLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Leg 4",
						ArmorLayer = "thinExoSkeletonLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.25f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "bushdragon";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = "hideLayer",
				OrganTypes = new OrganType[1]
				{
					new OrganType
					{
						Name = "Heart",
						IsVital = true
					}
				},
				ToHitProfileBack = 0.6f,
				ToHitProfileFront = 0.35f,
				ToHitProfileLeft = 0.5f,
				ToHitProfileRight = 0.5f,
				HitpointsFraction = 0.5f,
				BodyPartTypes = new BodyPartType[4]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = "hideLayer",
						OrganTypes = new OrganType[1]
						{
							new OrganType
							{
								Name = "Brain",
								IsVital = true
							}
						},
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.5f,
						ToHitProfileLeft = 0.2f,
						ToHitProfileRight = 0.2f,
						HitpointsFraction = 0.1f
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Left leg",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.25f,
						ToHitProfileRight = 0f,
						HitpointsFraction = 0.2f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.5f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Right leg",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0f,
						ToHitProfileRight = 0.25f,
						HitpointsFraction = 0.2f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.5f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Front leg",
						ArmorLayer = "hideLayer",
						ToHitProfileBack = 0f,
						ToHitProfileFront = 0.25f,
						ToHitProfileLeft = 0.15f,
						ToHitProfileRight = 0.15f,
						HitpointsFraction = 0.2f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.5f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "demonTree";
		list.Add(new BodyType(text)
		{
			BodyPartTypes = new BodyPartType[1]
			{
				new BiologicalBodyPartType
				{
					BodyKeyName = text,
					Name = "Torso",
					ArmorLayer = "hideLayer",
					OrganTypes = new OrganType[1]
					{
						new OrganType
						{
							Name = "Heart",
							IsVital = true
						}
					},
					ToHitProfileBack = 0.6f,
					ToHitProfileFront = 0.35f,
					ToHitProfileLeft = 0.5f,
					ToHitProfileRight = 0.5f,
					HitpointsFraction = 0.5f,
					BodyPartTypes = new BodyPartType[4]
					{
						new BiologicalBodyPartType
						{
							BodyKeyName = text,
							Name = "Head",
							ArmorLayer = "hideLayer",
							OrganTypes = new OrganType[1]
							{
								new OrganType
								{
									Name = "Brain",
									IsVital = true
								}
							},
							ToHitProfileBack = 0.1f,
							ToHitProfileFront = 0.5f,
							ToHitProfileLeft = 0.2f,
							ToHitProfileRight = 0.2f,
							HitpointsFraction = 0.1f
						},
						new BiologicalBodyPartType
						{
							BodyKeyName = text,
							Name = "Left leg",
							ArmorLayer = "hideLayer",
							ToHitProfileBack = 0.15f,
							ToHitProfileFront = 0.15f,
							ToHitProfileLeft = 0.25f,
							ToHitProfileRight = 0f,
							HitpointsFraction = 0.2f,
							Functions = new BodyPartFunction[1]
							{
								new BodyPartFunction
								{
									Function = BodyPartFunction.FunctionType.Locomotion,
									Weight = 0.5f
								}
							}
						},
						new BiologicalBodyPartType
						{
							BodyKeyName = text,
							Name = "Right leg",
							ArmorLayer = "hideLayer",
							ToHitProfileBack = 0.15f,
							ToHitProfileFront = 0.15f,
							ToHitProfileLeft = 0f,
							ToHitProfileRight = 0.25f,
							HitpointsFraction = 0.2f,
							Functions = new BodyPartFunction[1]
							{
								new BodyPartFunction
								{
									Function = BodyPartFunction.FunctionType.Locomotion,
									Weight = 0.5f
								}
							}
						},
						new BiologicalBodyPartType
						{
							BodyKeyName = text,
							Name = "Front leg",
							ArmorLayer = "hideLayer",
							ToHitProfileBack = 0f,
							ToHitProfileFront = 0.25f,
							ToHitProfileLeft = 0.15f,
							ToHitProfileRight = 0.15f,
							HitpointsFraction = 0.2f,
							Functions = new BodyPartFunction[1]
							{
								new BodyPartFunction
								{
									Function = BodyPartFunction.FunctionType.Locomotion,
									Weight = 0.5f
								}
							}
						}
					}
				}
			}
		});
		text = "sentry";
		string armorLayer = "robotShell";
		list.Add(new BodyType(text)
		{
			Hitpoints = 40f,
			BodyPartTypes = new BodyPartType[1]
			{
				new MachineBodyPartType
				{
					BodyKeyName = text,
					Name = "Tripod",
					ArmorLayer = armorLayer,
					ToHitProfileBack = 0.5f,
					ToHitProfileFront = 0.5f,
					ToHitProfileLeft = 0.5f,
					ToHitProfileRight = 0.5f,
					HitpointsFraction = 0.5f,
					BodyPartTypes = new BodyPartType[1]
					{
						new MachineBodyPartType
						{
							BodyKeyName = text,
							Name = "Gun",
							ArmorLayer = armorLayer,
							ToHitProfileBack = 0.5f,
							ToHitProfileFront = 0.5f,
							ToHitProfileLeft = 0.5f,
							ToHitProfileRight = 0.5f,
							HitpointsFraction = 0.5f
						}
					}
				}
			}
		});
		text = "weedingRobot";
		armorLayer = "robotShell";
		bodyType = new BodyType(text);
		bodyType.Hitpoints = 30f;
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new MachineBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = armorLayer,
				ToHitProfileBack = 0.6f,
				ToHitProfileFront = 0.35f,
				ToHitProfileLeft = 0.5f,
				ToHitProfileRight = 0.5f,
				HitpointsFraction = 0.5f,
				BodyPartTypes = new BodyPartType[3]
				{
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = armorLayer,
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.5f,
						ToHitProfileLeft = 0.2f,
						ToHitProfileRight = 0.2f,
						HitpointsFraction = 0.1f
					},
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Left leg",
						ArmorLayer = armorLayer,
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.25f,
						ToHitProfileRight = 0f,
						HitpointsFraction = 0.2f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.7f
							}
						}
					},
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Right leg",
						ArmorLayer = armorLayer,
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0f,
						ToHitProfileRight = 0.25f,
						HitpointsFraction = 0.2f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.7f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "robotBody";
		armorLayer = "robotShell";
		bodyType = new BodyType(text);
		bodyType.Hitpoints = 30f;
		bodyType.Bulk = 1f;
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new MachineBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = armorLayer,
				ToHitProfileBack = 0.6f,
				ToHitProfileFront = 0.35f,
				ToHitProfileLeft = 0.5f,
				ToHitProfileRight = 0.5f,
				HitpointsFraction = 0.5f,
				BodyPartTypes = new BodyPartType[3]
				{
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = armorLayer,
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.5f,
						ToHitProfileLeft = 0.2f,
						ToHitProfileRight = 0.2f,
						HitpointsFraction = 0.1f
					},
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Left leg",
						ArmorLayer = armorLayer,
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.25f,
						ToHitProfileRight = 0f,
						HitpointsFraction = 0.2f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.7f
							}
						}
					},
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Right leg",
						ArmorLayer = armorLayer,
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0f,
						ToHitProfileRight = 0.25f,
						HitpointsFraction = 0.2f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.7f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "spikePlant";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = "hideLayer",
				OrganTypes = new OrganType[1]
				{
					new OrganType
					{
						Name = "Heart",
						IsVital = true
					}
				},
				ToHitProfileBack = 0.6f,
				ToHitProfileFront = 0.35f,
				ToHitProfileLeft = 0.5f,
				ToHitProfileRight = 0.5f,
				HitpointsFraction = 0.5f,
				BodyPartTypes = new BodyPartType[1]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = "hideLayer",
						OrganTypes = new OrganType[1]
						{
							new OrganType
							{
								Name = "Brain",
								IsVital = true
							}
						},
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.5f,
						ToHitProfileLeft = 0.2f,
						ToHitProfileRight = 0.2f,
						HitpointsFraction = 0.1f
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "worm";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = "mediumExoSkeletonLayer",
				OrganTypes = new OrganType[1]
				{
					new OrganType
					{
						Name = "Heart",
						IsVital = true
					}
				},
				ToHitProfileBack = 0.6f,
				ToHitProfileFront = 0.35f,
				ToHitProfileLeft = 0.5f,
				ToHitProfileRight = 0.5f,
				HitpointsFraction = 0.5f,
				BodyPartTypes = new BodyPartType[1]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = "mediumExoSkeletonLayer",
						OrganTypes = new OrganType[1]
						{
							new OrganType
							{
								Name = "Brain",
								IsVital = true
							}
						},
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.5f,
						ToHitProfileLeft = 0.2f,
						ToHitProfileRight = 0.2f,
						HitpointsFraction = 0.1f
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "bird";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				BodyPartTypes = new BodyPartType[5]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head"
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Wing 1",
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Wing 2",
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Wing 3",
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Wing 4",
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.4f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "thunderchicken";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new BiologicalBodyPartType
			{
				BodyKeyName = text,
				Name = "Torso",
				ArmorLayer = "smallAnimalHideLayer",
				OrganTypes = new OrganType[1]
				{
					new OrganType
					{
						Name = "Heart",
						IsVital = true
					}
				},
				ToHitProfileBack = 0.6f,
				ToHitProfileFront = 0.35f,
				ToHitProfileLeft = 0.5f,
				ToHitProfileRight = 0.5f,
				HitpointsFraction = 0.5f,
				BodyPartTypes = new BodyPartType[3]
				{
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Head",
						ArmorLayer = "smallAnimalHideLayer",
						OrganTypes = new OrganType[1]
						{
							new OrganType
							{
								Name = "Brain",
								IsVital = true
							}
						},
						ToHitProfileBack = 0.1f,
						ToHitProfileFront = 0.5f,
						ToHitProfileLeft = 0.2f,
						ToHitProfileRight = 0.2f,
						HitpointsFraction = 0.1f
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Left leg",
						ArmorLayer = "smallAnimalHideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0.25f,
						ToHitProfileRight = 0f,
						HitpointsFraction = 0.2f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.7f
							}
						}
					},
					new BiologicalBodyPartType
					{
						BodyKeyName = text,
						Name = "Right leg",
						ArmorLayer = "smallAnimalHideLayer",
						ToHitProfileBack = 0.15f,
						ToHitProfileFront = 0.15f,
						ToHitProfileLeft = 0f,
						ToHitProfileRight = 0.25f,
						HitpointsFraction = 0.2f,
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Locomotion,
								Weight = 0.7f
							}
						}
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "house";
		bodyType = new BodyType(text);
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new MachineBodyPartType
			{
				BodyKeyName = text,
				Name = "Foundation",
				Functions = new BodyPartFunction[1]
				{
					new BodyPartFunction
					{
						Function = BodyPartFunction.FunctionType.Structure,
						Weight = 1f
					}
				},
				BodyPartTypes = new BodyPartType[2]
				{
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Walls",
						Functions = new BodyPartFunction[1]
						{
							new BodyPartFunction
							{
								Function = BodyPartFunction.FunctionType.Structure,
								Weight = 1f
							}
						},
						BodyPartTypes = new BodyPartType[1]
						{
							new MachineBodyPartType
							{
								BodyKeyName = text,
								Name = "Roof",
								Functions = new BodyPartFunction[2]
								{
									new BodyPartFunction
									{
										Function = BodyPartFunction.FunctionType.Structure,
										Weight = 0.3f
									},
									new BodyPartFunction
									{
										Function = BodyPartFunction.FunctionType.UserComfort,
										Weight = 1f
									}
								}
							}
						}
					},
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Plumbing"
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		text = "mulevehicle";
		bodyType = new BodyType(text);
		bodyType.Hitpoints = 200f;
		bodyType.BodyPartTypes = new BodyPartType[1]
		{
			new MachineBodyPartType
			{
				BodyKeyName = text,
				Name = "Body",
				BodyPartTypes = new BodyPartType[5]
				{
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Front left suspension",
						BodyPartTypes = new BodyPartType[1]
						{
							new MachineBodyPartType
							{
								BodyKeyName = text,
								Name = "Front left wheel",
								BodyPartTypes = new BodyPartType[2]
								{
									new MachineBodyPartType
									{
										BodyKeyName = text,
										Name = "Front left motor"
									},
									new MachineBodyPartType
									{
										BodyKeyName = text,
										Name = "Front left tyre"
									}
								}
							}
						}
					},
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Front right suspension",
						BodyPartTypes = new BodyPartType[1]
						{
							new MachineBodyPartType
							{
								BodyKeyName = text,
								Name = "Front right wheel",
								BodyPartTypes = new BodyPartType[2]
								{
									new MachineBodyPartType
									{
										BodyKeyName = text,
										Name = "Front right motor"
									},
									new MachineBodyPartType
									{
										BodyKeyName = text,
										Name = "Front right tyre"
									}
								}
							}
						}
					},
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Control panel"
					},
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Driver's seat"
					},
					new MachineBodyPartType
					{
						BodyKeyName = text,
						Name = "Passenger seat"
					}
				}
			}
		};
		item = bodyType;
		list.Add(item);
		return list;
	}

	protected override List<SkillCategory> InitSkillCategories()
	{
		return new List<SkillCategory>
		{
			new SkillCategory
			{
				KeyName = "basic",
				Name = "Basic"
			},
			new SkillCategory
			{
				KeyName = "combat",
				Name = "Combat"
			},
			new SkillCategory
			{
				KeyName = "construction",
				Name = "Construction"
			},
			new SkillCategory
			{
				KeyName = "production",
				Name = "Production"
			},
			new SkillCategory
			{
				KeyName = "science",
				Name = "Science"
			},
			new SkillCategory
			{
				KeyName = "other",
				Name = "Other"
			}
		};
	}

	protected override List<ProfessionType> InitProfessionTypes()
	{
		return new List<ProfessionType>
		{
			new ProfessionType
			{
				KeyName = "securityPerson",
				Name = "Security",
				Icon = "lcd_icon_security"
			},
			new ProfessionType
			{
				KeyName = "hunter",
				Name = "Hunting",
				Icon = "lcd_icon_skill_rifle"
			},
			new ProfessionType
			{
				KeyName = "metalWorker",
				Name = "Blacksmithing",
				Icon = "lcd_icon_skill_anvil"
			},
			new ProfessionType
			{
				KeyName = "farmer",
				Name = "Farming",
				Icon = "lcd_icon_skill_pitchfork"
			},
			new ProfessionType
			{
				KeyName = "physician",
				Name = "Medicine",
				Icon = "lcd_icon_skill_medicineSymbol"
			},
			new ProfessionType
			{
				KeyName = "cook",
				Name = "Cooking",
				Icon = "lcd_icon_skill_cookingpot"
			},
			new ProfessionType
			{
				KeyName = "menialWorker",
				Name = "Menial labor",
				Icon = "lcd_icon_skill_wheelbarrow"
			},
			new ProfessionType
			{
				KeyName = "builder",
				Name = "Construction",
				Icon = "lcd_icon_skill_bricks"
			},
			new ProfessionType
			{
				KeyName = "electricEngineer",
				Name = "Electrical engineering",
				Icon = "lcd_icon_skill_spark"
			},
			new ProfessionType
			{
				KeyName = "mechanicalEngineer",
				Name = "Mechanical engineering",
				Icon = "lcd_icon_skill_caliper"
			},
			new ProfessionType
			{
				KeyName = "chemist",
				Name = "Chemistry",
				Icon = "lcd_icon_skill_conicalFlask"
			},
			new ProfessionType
			{
				KeyName = "scientist",
				Name = "Science",
				Icon = "lcd_icon_skill_conicalFlask"
			},
			new ProfessionType
			{
				KeyName = "bushcraft",
				Name = "Bushcraft",
				Icon = "lcd_icon_skill_axe"
			}
		};
	}

	protected override List<SkillType> InitSkillTypes()
	{
		List<SkillType> list = new List<SkillType>();
		SkillType item = new SkillType("hunting", "Hunting")
		{
			SortOrder = 20,
			Description = "Effectiveness at detecting and stalking prey. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["other"],
			ProfessionKey = "hunter"
		};
		list.Add(item);
		item = new SkillType("fishing", "Fishing")
		{
			SortOrder = 30,
			Description = "Effectiveness at catching fish. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["other"]
		};
		list.Add(item);
		item = new SkillType("foraging", "Foraging")
		{
			SortOrder = 40,
			Description = "Effectiveness at finding food and useful materials in the wild. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["other"]
		};
		list.Add(item);
		item = new SkillType("cooking", "Cooking")
		{
			SortOrder = 60,
			Description = "Productivity when making food and preparing ingredients. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"],
			ProfessionKey = "cook"
		};
		list.Add(item);
		item = new SkillType("menial", "Menial")
		{
			SortOrder = 70,
			Description = "Productivity when doing physical tasks that require no special skill. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["other"],
			ProfessionKey = "menialWorker"
		};
		list.Add(item);
		item = new SkillType("weaving", "Weaving")
		{
			SortOrder = 75,
			Description = "Productivity when making textile. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"]
		};
		list.Add(item);
		item = new SkillType("carpentry", "Carpentry")
		{
			SortOrder = 78,
			Description = "Productivity when making items from wood. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"]
		};
		list.Add(item);
		item = new SkillType("farming", "Farming")
		{
			SortOrder = 80,
			Description = "Ability to grow and harvest crops from fields. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"],
			ProfessionKey = "farmer"
		};
		list.Add(item);
		item = new SkillType("construction", "Construction")
		{
			SortOrder = 90,
			Description = "Ability to construct houses and buildings. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["construction"],
			ProfessionKey = "builder"
		};
		list.Add(item);
		item = new SkillType("smithing", "Smithing")
		{
			SortOrder = 100,
			Description = "Productivity when shaping and joining metal using a forge and hammer. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"],
			ProfessionKey = "metalWorker"
		};
		list.Add(item);
		item = new SkillType("mechanics", "Mechanics")
		{
			SortOrder = 110,
			Description = "Ability to make and operate machines and mechanical systems.\nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"],
			ProfessionKey = "mechanicalEngineer"
		};
		list.Add(item);
		item = new SkillType("electronics", "Electronics")
		{
			SortOrder = 115,
			Description = "Knowledge about electricity, electronics and electromagnetism.\nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"],
			ProfessionKey = "electricEngineer"
		};
		list.Add(item);
		item = new SkillType("chemistry", "Chemistry")
		{
			SortOrder = 120,
			Description = "Knowledge about substances and chemicals and how to produce them.\nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"],
			ProfessionKey = "chemist"
		};
		list.Add(item);
		item = new SkillType("biology", "Biology")
		{
			SortOrder = 130,
			Description = "Knowledge about plants and animals. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["science"],
			ProfessionKey = "scientist"
		};
		list.Add(item);
		item = new SkillType("medicine", "Medicine")
		{
			SortOrder = 140,
			Description = "Ability to treat wounds and diseases. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["science"],
			ProfessionKey = "physician"
		};
		list.Add(item);
		item = new SkillType("psychology", "Psychology")
		{
			SortOrder = 145,
			Description = "Knowledge about the human mental processes. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["science"],
			ProfessionKey = "scientist"
		};
		list.Add(item);
		item = new SkillType("shooting", "Shooting")
		{
			SortOrder = 150,
			GiveExpertSkillBonus = true,
			Description = "Ability to hit a target with a firearm or energy based weapon. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["combat"],
			ProfessionKey = "securityPerson"
		};
		list.Add(item);
		item = new SkillType("archery", "Archery")
		{
			SortOrder = 160,
			GiveExpertSkillBonus = true,
			Description = "Ability to hit a target with a bow and arrow. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["combat"],
			ProfessionKey = "securityPerson"
		};
		list.Add(item);
		item = new SkillType("armedMelee", "Armed melee")
		{
			SortOrder = 170,
			Description = "Ability to fight in close combat using a non-projectile weapon. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["combat"],
			ProfessionKey = "securityPerson"
		};
		list.Add(item);
		item = new SkillType("butchering", "Butchering")
		{
			SortOrder = 50,
			Description = "Productivity when retrieving meat, organs and hide from a carcass. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"]
		};
		list.Add(item);
		item = new SkillType("bushcraft", "Bushcraft")
		{
			SortOrder = 10,
			Description = "Making items and structures under primitive conditions using simple tools and materials found in the wild. The skill determines productivity. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"],
			ProfessionKey = "bushcraft"
		};
		list.Add(item);
		item = new SkillType("sneaking", "Sneaking")
		{
			SortOrder = 190,
			Description = "Ability to avoid detection while moving. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["other"]
		};
		list.Add(item);
		item = new SkillType("unarmedFighting", "Unarmed fighting")
		{
			SortOrder = 180,
			Description = "Ability to fight barehanded. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["combat"],
			ProfessionKey = "securityPerson"
		};
		list.Add(item);
		item = new SkillType("grasping", "Grasping")
		{
			SortOrder = 200,
			Description = "Ability to grab and hold objects. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["basic"],
			SuppressDisplayForPersons = true
		};
		list.Add(item);
		item = new SkillType("fruitPicking", "Fruit picking")
		{
			SortOrder = 210,
			Description = "Ability to collect small objects. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["basic"],
			SuppressDisplayForPersons = true
		};
		list.Add(item);
		item = new SkillType("weeding", "Weeding")
		{
			SortOrder = 220,
			Description = "Ability to remove unwanted weeds from fields. \nTo begin a task, a skill of 0.1 is the minimum requirement.",
			Category = GameData.Instance.AllSkillCategories["production"],
			ProfessionKey = "farmer",
			SuppressDisplayForPersons = true
		};
		list.Add(item);
		return list;
	}

	protected override List<ResourceCategory> InitResourceCategories()
	{
		List<ResourceCategory> list = new List<ResourceCategory>();
		ResourceCategory item = new ResourceCategory
		{
			KeyName = "food",
			Name = "FOOD",
			Color = "#0de700".ColorFromHex()
		};
		list.Add(item);
		item = new ResourceCategory
		{
			KeyName = "rawMaterials",
			Name = "RAW MATERIALS",
			Color = "#ffde00".ColorFromHex()
		};
		list.Add(item);
		item = new ResourceCategory
		{
			KeyName = "carcasses",
			Name = "CARCASSES"
		};
		list.Add(item);
		return list;
	}

	protected override List<DefaultStorageSettings> InitDefaultStorageSettings()
	{
		return new List<DefaultStorageSettings>
		{
			new DefaultStorageSettings
			{
				KeyName = "cooledStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", true },
					{ "ingredients", true },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int> { { "item:thunderChickenCarcass", -1 } },
				MayStockpileItemTag = new SerializableDictionary<string, int> { { "cookedMeat", -1 } }
			},
			new DefaultStorageSettings
			{
				KeyName = "workplaceStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", true },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", true }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "homeStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "rawMaterials", false },
					{ "waste", false },
					{ "bodies", false }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "firewoodstack",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int> { { "item:wetFirewood", -1 } }
			},
			new DefaultStorageSettings
			{
				KeyName = "farmToolshed",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", true }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int>
				{
					{ "item:bellows", -1 },
					{ "item:blowpipe", -1 },
					{ "item:blacksmithsToolbox", -1 },
					{ "item:metalWorkersToolbox", -1 },
					{ "item:hammer", -1 },
					{ "item:stoneHammer", -1 },
					{ "item:file", -1 },
					{ "item:tongs", -1 },
					{ "item:handDrill", -1 },
					{ "item:hacksaw", -1 },
					{ "item:tinnerSnips", -1 },
					{ "item:advancedSnips", -1 },
					{ "item:smoothSandstone", -1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "workbenchStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int>
				{
					{ "item:advancedMachete", -1 },
					{ "item:steelMachete", -1 },
					{ "item:improvisedHandAxe", -1 },
					{ "item:steelHandAxe", -1 },
					{ "item:advancedKnife", -1 },
					{ "item:improvisedKnife", -1 },
					{ "item:flintKnife", -1 },
					{ "item:steelKnife", -1 },
					{ "item:shadeleafResin", -1 },
					{ "item:advancedString", -1 },
					{ "item:metalWire", -1 },
					{ "item:rawhideString", -1 },
					{ "item:exaGlue", -1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "uncooledKitchenStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int>
				{
					{ "item:advancedKnife", -1 },
					{ "item:improvisedKnife", -1 },
					{ "item:flintKnife", -1 },
					{ "item:steelKnife", -1 },
					{ "item:vinegar", -1 },
					{ "item:advancedCookingPot", -1 },
					{ "item:improvisedCookingPot", -1 },
					{ "item:goldPot", -1 },
					{ "item:woodenCookingPot", -1 },
					{ "item:clayPotUnglazed", -1 },
					{ "item:clayJar", -1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "forgeStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int>
				{
					{ "item:bellows", -1 },
					{ "item:blowpipe", -1 },
					{ "item:blacksmithsToolbox", -1 },
					{ "item:metalWorkersToolbox", -1 },
					{ "item:hammer", -1 },
					{ "item:stoneHammer", -1 },
					{ "item:file", -1 },
					{ "item:tongs", -1 },
					{ "item:handDrill", -1 },
					{ "item:hacksaw", -1 },
					{ "item:tinnerSnips", -1 },
					{ "item:advancedSnips", -1 },
					{ "item:smoothSandstone", -1 },
					{ "item:roughBloomIron", -1 },
					{ "item:wroughtIron", -1 },
					{ "item:blisterSteel", -1 },
					{ "item:charcoal", -1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "noStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "polymerWorkshopStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int>
				{
					{ "item:marshcotSap", -1 },
					{ "item:sulfurPowder", -1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "carpentersWorkshopStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int>
				{
					{ "item:spoakBranchesTrimmed", -1 },
					{ "item:carpentersToolbox", -1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "textileWorkshopStorage",
				Comments = "tools AND materials for the weaver",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int> { { "item:cotton", -1 } }
			},
			new DefaultStorageSettings
			{
				KeyName = "metalWorkshopStorage",
				Comments = "tools AND materials for the metal workshop",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int>
				{
					{ "item:blisterSteel", -1 },
					{ "item:gunBarrelUnbored", -1 },
					{ "item:gunBarrelSmoothLong", -1 },
					{ "item:gunBarrelSmoothShort", -1 },
					{ "item:gunBarrelRifled", -1 },
					{ "item:metalWorkersToolbox", -1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "metalShopStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int>
				{
					{ "item:blisterSteel", -1 },
					{ "item:gunBarrelUnbored", -1 },
					{ "item:gunBarrelSmoothLong", -1 },
					{ "item:gunBarrelSmoothShort", -1 },
					{ "item:gunBarrelRifled", -1 },
					{ "item:metalWorkersToolbox", -1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "simplePortLocalStorage",
				Name = "",
				MayStockpileEntityType = new SerializableDictionary<string, int> { { "item:marshcotSap", -1 } },
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "compostBinSettings",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int>
				{
					{ "item:infestedLeavesRemains", -1 },
					{ "item:rottenVegetables", -1 },
					{ "item:rottenStaple", -1 },
					{ "item:twinklerGuts", -1 },
					{ "item:rottenMeat", -1 },
					{ "item:spoiledMeal", -1 },
					{ "item:degradedEnzyme", -1 },
					{ "item:degradedChemical", -1 },
					{ "item:organicMatter", -1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "stockpile",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "rawMaterials", false },
					{ "waste", false },
					{ "bodies", false }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "darkFoodStorage",
				Name = "",
				MayStockpileEntityType = new SerializableDictionary<string, int> { { "item:salt", -1 } },
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "rawMaterials", false },
					{ "waste", false },
					{ "bodies", false },
					{ "tools", false },
					{ "weapons", false },
					{ "ammunition", false }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "kitchenStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "rawMaterials", false },
					{ "waste", false },
					{ "bodies", false },
					{ "tools", false },
					{ "weapons", false },
					{ "ammunition", false }
				},
				MayStockpileItemTag = new SerializableDictionary<string, int>
				{
					{ "cookingPot", -1 },
					{ "knife", -1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "fishTrapCage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "bodies", false },
					{ "rawMaterials", false },
					{ "tools", false },
					{ "weapons", false },
					{ "ammunition", false },
					{ "waste", false }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "trapBaitStorage",
				Name = "",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				},
				MayStockpileEntityType = new SerializableDictionary<string, int>
				{
					{ "item:binalRatChunk", 1 },
					{ "item:blackpulp", 1 },
					{ "item:glassyCreeperPods", 1 }
				}
			},
			new DefaultStorageSettings
			{
				KeyName = "hideRackStorage",
				Name = "Hide Rack Storage",
				MayStockpileCategory = new SerializableDictionary<string, bool>
				{
					{ "preparedFood", false },
					{ "ingredients", false },
					{ "rawMaterials", false },
					{ "weapons", false },
					{ "bodies", false },
					{ "waste", false },
					{ "ammunition", false },
					{ "tools", false }
				}
			}
		};
	}

	protected override List<EntityCategory> InitEntityCategories()
	{
		List<EntityCategory> list = new List<EntityCategory>();
		EntityCategory item = new EntityCategory
		{
			KeyName = "preparedFood",
			Name = "PREPARED FOOD",
			CategoryColor = EntityCategory.CategoryColors.Green,
			SpriteName = "preparedFood",
			SortOrder = 5
		};
		list.Add(item);
		item = new EntityCategory
		{
			KeyName = "ingredients",
			Name = "INGREDIENTS",
			CategoryColor = EntityCategory.CategoryColors.Green,
			SpriteName = "ingredients",
			SortOrder = 10
		};
		list.Add(item);
		item = new EntityCategory
		{
			KeyName = "bodies",
			Name = "CARCASSES",
			CategoryColor = EntityCategory.CategoryColors.Green,
			SpriteName = "carcasses",
			SortOrder = 15
		};
		list.Add(item);
		item = new EntityCategory
		{
			KeyName = "rawMaterials",
			Name = "MATERIALS/COMPONENTS",
			SpriteName = "metal",
			SortOrder = 20
		};
		list.Add(item);
		item = new EntityCategory
		{
			KeyName = "tools",
			Name = "TOOLS",
			SpriteName = "equipment",
			SortOrder = 25
		};
		list.Add(item);
		item = new EntityCategory
		{
			KeyName = "weapons",
			Name = "WEAPONS",
			CategoryColor = EntityCategory.CategoryColors.Blue,
			SpriteName = "guns",
			SortOrder = 30
		};
		list.Add(item);
		item = new EntityCategory
		{
			KeyName = "ammunition",
			Name = "AMMUNITION",
			CategoryColor = EntityCategory.CategoryColors.Blue,
			SpriteName = "guns",
			SortOrder = 35
		};
		list.Add(item);
		item = new EntityCategory
		{
			KeyName = "equipment",
			Name = "EQUIPMENT",
			SpriteName = "equipment",
			SortOrder = 40
		};
		list.Add(item);
		item = new EntityCategory
		{
			KeyName = "waste",
			Name = "WASTE",
			SpriteName = "carcasses",
			IsWaste = true,
			SortOrder = 45
		};
		list.Add(item);
		item = new EntityCategory
		{
			KeyName = "upgrades",
			Name = "UPGRADES",
			SpriteName = "equipment",
			SortOrder = 55
		};
		list.Add(item);
		list.Add(new EntityCategory
		{
			KeyName = "shelter",
			Name = "SHELTER",
			CategoryColor = EntityCategory.CategoryColors.Red,
			SortOrder = 0,
			DisplayStructureIcon = true
		});
		list.Add(new EntityCategory
		{
			KeyName = "production",
			Name = "STORAGE/PRODUCTION",
			SortOrder = 1,
			DisplayStructureIcon = true
		});
		list.Add(new EntityCategory
		{
			KeyName = "defense",
			Name = "DEFENSE",
			SortOrder = 2,
			DisplayStructureIcon = true
		});
		list.Add(new EntityCategory
		{
			KeyName = "miscellaneous",
			Name = "MISCELLANEOUS",
			SortOrder = 3,
			DisplayStructureIcon = true
		});
		list.Add(new EntityCategory
		{
			KeyName = "animals",
			Name = "ANIMALS",
			SortOrder = 60
		});
		list.Add(new EntityCategory
		{
			KeyName = "robots",
			Name = "ROBOTS",
			SortOrder = 80
		});
		return list;
	}

	protected override List<StorageCondition> InitStorageConditions()
	{
		return new List<StorageCondition>
		{
			new StorageCondition
			{
				KeyName = "aquarium",
				Name = "Aquarium",
				Description = "The conditions in a water filled tank",
				FixedMoisture = 1f,
				IsolatedTemperature = true
			},
			new StorageCondition
			{
				KeyName = "exposed",
				Name = "Outside",
				Description = "Open air conditions, exposed to the elements"
			},
			new StorageCondition
			{
				KeyName = "isolated",
				Name = "Inside",
				Description = "Inside conditions with the temperature influenced by the outside temperature",
				FixedLightLevel = 0f,
				FixedMoisture = 0f,
				IsolatedTemperature = true
			},
			new StorageCondition
			{
				KeyName = "moist",
				Name = "Moist",
				Description = "Inside conditions with a high humidity",
				FixedLightLevel = 0f,
				FixedMoisture = 0.9f,
				IsolatedTemperature = true
			},
			new StorageCondition
			{
				KeyName = "earthCooled",
				Name = "Earth cooled",
				Description = "The conditions inside a dry hole, cooled by the earth. The Storage hole as well as other structures can provide this storage",
				FixedLightLevel = 0f,
				FixedMoisture = 0f,
				FixedTemperature = 284f
			},
			new StorageCondition
			{
				KeyName = "underWater",
				Name = "Under water",
				Description = "Immersed in water",
				FixedLightLevel = 0f,
				FixedMoisture = 0f,
				FixedTemperature = 284f
			},
			new StorageCondition
			{
				KeyName = "refrigerator",
				Name = "Refrigerated",
				Description = "The conditions at a fixed low temperature. This requires cooling elements and a power supply.",
				RequiresPower = true,
				FixedLightLevel = 0f,
				FixedMoisture = 0f,
				FixedTemperature = 278f
			},
			new StorageCondition
			{
				KeyName = "freezer",
				Name = "Deep freeze",
				Description = "The conditions of a deep freeze",
				RequiresPower = true,
				FixedLightLevel = 0f,
				FixedMoisture = 0f,
				FixedTemperature = 255f
			},
			new StorageCondition
			{
				KeyName = "airconditioning",
				Name = "Airconditioned",
				Description = "Indoor conditions at room temperature",
				RequiresPower = true,
				FixedLightLevel = 0f,
				FixedMoisture = 0f,
				FixedTemperature = 293f
			}
		};
	}

	protected override List<DegradeType> InitDegradeTypes()
	{
		List<DegradeType> list = new List<DegradeType>();
		string text = " \nNOTE: Colonists will automatically do maintenance on a structure if they have time and tools available. This will delay its deterioration.";
		string text2 = " \nNOTE: Unlike structures, items will NOT be provided maintenance by colonists. Only proper storage can extend an item's lifespan.";
		DegradeType degradeType = new DegradeType();
		degradeType.Name = "'RAW SEAFOOD'";
		degradeType.KeyName = "raw seafood";
		degradeType.Description = "Will decompose very quickly if not stored cool or preferably frozen.";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.02f),
			new Vector2(278f, 0.15f),
			new Vector2(293f, 2.1f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.4f)
		};
		DegradeType item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'FLESH SHREDS'";
		degradeType.KeyName = "fleshShreds";
		degradeType.Description = "Shreds of flesh from animals feeding. Will decompose very quickly";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 2f),
			new Vector2(278f, 5f),
			new Vector2(293f, 30f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0.4f),
			new Vector2(1f, 0.4f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'CAN BE SUN DRIED'";
		degradeType.KeyName = "sun drying";
		degradeType.Description = "This item can be dried in the open. Quicker options may be possible.";
		degradeType.TemperatureDamage = new Vector2[4]
		{
			new Vector2(255f, 0f),
			new Vector2(278f, 0.15f),
			new Vector2(293f, 2.7f),
			new Vector2(313f, 6f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 0f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.4f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'ENZYME'";
		degradeType.KeyName = "enzyme";
		degradeType.Description = "Enzymes should be used immediately as they have very limited shelf life";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.1f),
			new Vector2(278f, 2f),
			new Vector2(293f, 3f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.8f),
			new Vector2(1f, 1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0.4f),
			new Vector2(1f, 0.4f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'RAW MEAT'";
		degradeType.KeyName = "raw meat";
		degradeType.Description = "Will decompose quickly unless stored in a cool, dry and dark environment.";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.01f),
			new Vector2(278f, 0.15f),
			new Vector2(293f, 1.7f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.4f)
		};
		item = degradeType;
		list.Add(item);
		list.Add(new DegradeType
		{
			Name = "'CARCASS DECOMPOSITION'",
			KeyName = "carcassDecomposing",
			Description = "A carcass will decompose quickly unless stored in a cool, dry and dark environment.",
			TemperatureDamage = new Vector2[3]
			{
				new Vector2(255f, 0.005f),
				new Vector2(278f, 0.075f),
				new Vector2(293f, 0.85f)
			},
			MoistureDamage = new Vector2[2]
			{
				new Vector2(0f, 0.005f),
				new Vector2(1f, 0.5f)
			},
			LightDamage = new Vector2[2]
			{
				new Vector2(0f, 0f),
				new Vector2(1f, 0.2f)
			}
		});
		degradeType = new DegradeType();
		degradeType.Name = "'DECOMPOSED FOOD'";
		degradeType.KeyName = "decomposedFood";
		degradeType.Description = "This food item has recently spoiled and will continue to decompose, particularly quickly in the open or in humid conditions.";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.01f),
			new Vector2(278f, 0.15f),
			new Vector2(293f, 1.7f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.4f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'COOKED FOOD'";
		degradeType.KeyName = "cooked food";
		degradeType.Description = "This food item will spoil if not quickly refrigerated or frozen";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.008f),
			new Vector2(278f, 0.1f),
			new Vector2(293f, 1.3f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.4f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'HOT FOOD'";
		degradeType.KeyName = "hotFood";
		degradeType.Description = "This item is best consumed quickly, before it cools off";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 20f),
			new Vector2(278f, 6f),
			new Vector2(293f, 3f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'PERISHABLE'";
		degradeType.KeyName = "perishable";
		degradeType.Description = "This item will decay quickly unless it is kept cool, dry and dark, preferably frozen";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.005f),
			new Vector2(278f, 0.04f),
			new Vector2(293f, 0.5f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.4f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'BIOWASTE'";
		degradeType.KeyName = "biowaste";
		degradeType.Description = "Organic waste which will quickly decompose in the open in humid conditions";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.005f),
			new Vector2(278f, 0.04f),
			new Vector2(293f, 0.5f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.4f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'PERISHABLE. DO NOT FREEZE'";
		degradeType.KeyName = "perishable, no freeze";
		degradeType.Description = "This item will quickly decay outside and should be stored in a cool, dark and dry environment - but not frozen";
		degradeType.TemperatureDamage = new Vector2[4]
		{
			new Vector2(255f, 0.6f),
			new Vector2(273f, 0.15f),
			new Vector2(278f, 0.1f),
			new Vector2(293f, 0.5f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.4f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'SOMEWHAT PRESERVED'";
		degradeType.KeyName = "somewhatPreserved";
		degradeType.Description = "This food item has been preserved in a rudimentary way and will last rather long under dry and dark conditions.";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.005f),
			new Vector2(278f, 0.01f),
			new Vector2(293f, 0.1f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.1f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'BEST STORED DRY'";
		degradeType.KeyName = "stored dry";
		degradeType.Description = "This item will last very long under dry and dark conditions.";
		degradeType.TemperatureDamage = new Vector2[4]
		{
			new Vector2(255f, 0f),
			new Vector2(273f, 0f),
			new Vector2(293f, 0f),
			new Vector2(313f, 0.02f)
		};
		degradeType.MoistureDamage = new Vector2[3]
		{
			new Vector2(0f, 0.01f),
			new Vector2(0.9f, 0.4f),
			new Vector2(1f, 0.8f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.1f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'PICKLED FOOD'";
		degradeType.KeyName = "pickledFood";
		degradeType.Description = "Food immersed in an acidic liquid has a long lifespan when placed indoors.";
		degradeType.TemperatureDamage = new Vector2[4]
		{
			new Vector2(255f, 0.04f),
			new Vector2(273f, 0.01f),
			new Vector2(293f, 0f),
			new Vector2(313f, 0.02f)
		};
		degradeType.MoistureDamage = new Vector2[3]
		{
			new Vector2(0f, 0.01f),
			new Vector2(0.9f, 0.4f),
			new Vector2(1f, 0.8f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.1f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'KEEP DRY'";
		degradeType.KeyName = "wetDecay";
		degradeType.Description = "This material decays quickly in humid conditions.";
		degradeType.TemperatureDamage = new Vector2[2]
		{
			new Vector2(255f, 0f),
			new Vector2(313f, 0f)
		};
		degradeType.MoistureDamage = new Vector2[3]
		{
			new Vector2(0f, 0f),
			new Vector2(0.5f, 0.02f),
			new Vector2(1f, 2f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'PRONE TO INFESTATION'";
		degradeType.KeyName = "proneToInfestation";
		degradeType.Description = "This plant material is vulnerable to the Scuttler bug which over time, will eat it, only leaving a few remains.";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0f),
			new Vector2(273f, 0f),
			new Vector2(293f, 0.14f)
		};
		degradeType.MoistureDamage = new Vector2[3]
		{
			new Vector2(0f, 0f),
			new Vector2(0.6f, 1f),
			new Vector2(1f, 0.1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'HIGHLY RESISTANT'";
		degradeType.KeyName = "dirt";
		degradeType.Description = "This material will last indefinitely in storage and very long in the open although it will eventually erode and dissappear.";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0f),
			new Vector2(273f, 0f),
			new Vector2(293f, 0f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		item = degradeType;
		list.Add(item);
		list.Add(new DegradeType
		{
			Name = "'NEVER DEGRADES'",
			KeyName = "neverDegrades",
			Description = "This material will last for centuries, no matter the conditions it is exposed to.",
			TemperatureDamage = new Vector2[3]
			{
				new Vector2(255f, 0f),
				new Vector2(273f, 0f),
				new Vector2(293f, 0f)
			},
			MoistureDamage = new Vector2[2]
			{
				new Vector2(0f, 0f),
				new Vector2(1f, 0f)
			},
			LightDamage = new Vector2[2]
			{
				new Vector2(0f, 0f),
				new Vector2(1f, 0f)
			}
		});
		degradeType = new DegradeType();
		degradeType.Name = "'RICKETY CONSTRUCTION'";
		degradeType.KeyName = "ricketyConstruction";
		degradeType.Description = "Low quality structure which requires very frequent maintenance to prevent it breaking down." + text;
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.48f),
			new Vector2(278f, 0.48f),
			new Vector2(293f, 0.48f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'ADEQUATE CONSTRUCTION'";
		degradeType.KeyName = "adequateConstruction";
		degradeType.Description = "Mediocre quality structure which requires regular maintenance to prevent it breaking down." + text;
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.35f),
			new Vector2(278f, 0.35f),
			new Vector2(293f, 0.35f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'STURDY CONSTRUCTION'";
		degradeType.KeyName = "sturdyConstruction";
		degradeType.Description = "Good quality structure which requires little maintenance to keep it in shape." + text;
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.2f),
			new Vector2(278f, 0.2f),
			new Vector2(293f, 0.2f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'ADVANCED CONSTRUCTION'";
		degradeType.KeyName = "advancedConstruction";
		degradeType.Description = "Very high quality structure which requires minimal maintenance." + text;
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.005f),
			new Vector2(278f, 0.005f),
			new Vector2(293f, 0.005f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		item = degradeType;
		list.Add(item);
		list.Add(new DegradeType
		{
			Name = "'BRITTLE EQUIPMENT'",
			KeyName = "improvisedEquipment",
			Description = "Lower quality equipment will degrade relatively quick if left exposed to the elements." + text2,
			TemperatureDamage = new Vector2[3]
			{
				new Vector2(255f, 0.01f),
				new Vector2(273f, 0.01f),
				new Vector2(293f, 0f)
			},
			MoistureDamage = new Vector2[2]
			{
				new Vector2(0f, 0.01f),
				new Vector2(1f, 0.15f)
			},
			LightDamage = new Vector2[2]
			{
				new Vector2(0f, 0f),
				new Vector2(1f, 0.05f)
			}
		});
		degradeType = new DegradeType();
		degradeType.Name = "'EQUIPMENT'";
		degradeType.KeyName = "equipment";
		degradeType.Description = "This equipment is reasonably durable but will last longer if stored inside." + text2;
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0.01f),
			new Vector2(273f, 0.01f),
			new Vector2(293f, 0f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0.01f),
			new Vector2(1f, 0.1f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0.02f)
		};
		item = degradeType;
		list.Add(item);
		degradeType = new DegradeType();
		degradeType.Name = "'DEGRADES WITH TIME'";
		degradeType.KeyName = "timeDegrading";
		degradeType.Description = "This type of object will change or degrade with time, uninfluenced by storage conditions.";
		degradeType.TemperatureDamage = new Vector2[3]
		{
			new Vector2(255f, 0f),
			new Vector2(278f, 0f),
			new Vector2(293f, 0f)
		};
		degradeType.MoistureDamage = new Vector2[2]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		degradeType.LightDamage = new Vector2[2]
		{
			new Vector2(0f, 2f),
			new Vector2(1f, 2f)
		};
		item = degradeType;
		list.Add(item);
		return list;
	}

	protected override List<FoodNutrientProfile> InitFoodNutrientProfiles()
	{
		List<FoodNutrientProfile> list = new List<FoodNutrientProfile>();
		FoodNutrientType nutrient = GameData.Instance.AllFoodNutrientTypes["foodEnergy"];
		FoodNutrientType nutrient2 = GameData.Instance.AllFoodNutrientTypes["protein"];
		FoodNutrientType nutrient3 = GameData.Instance.AllFoodNutrientTypes["micronutrients"];
		FoodNutrientType nutrient4 = GameData.Instance.AllFoodNutrientTypes["stimulants"];
		FoodNutrientProfile foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "poorMeat";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[3]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 0.4f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient2,
				Amount = 0.02f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.0008f
			}
		};
		FoodNutrientProfile item = foodNutrientProfile;
		list.Add(item);
		foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "mediumMeat";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[3]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 0.7f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient2,
				Amount = 0.048f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.0024f
			}
		};
		item = foodNutrientProfile;
		list.Add(item);
		foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "richMeat";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[3]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 0.8f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient2,
				Amount = 0.08f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.0035f
			}
		};
		item = foodNutrientProfile;
		list.Add(item);
		foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "meatSoup";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[3]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 0.4f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient2,
				Amount = 0.02f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.0008f
			}
		};
		item = foodNutrientProfile;
		list.Add(item);
		foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "poorVegetables";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[2]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 0.1f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.0029f
			}
		};
		item = foodNutrientProfile;
		list.Add(item);
		foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "richVegetables";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[3]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 0.3f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient2,
				Amount = 0.004f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.0031f
			}
		};
		item = foodNutrientProfile;
		list.Add(item);
		foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "poorStaple";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[3]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 0.52f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient2,
				Amount = 0.011f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.0006f
			}
		};
		item = foodNutrientProfile;
		list.Add(item);
		foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "richStaple";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[3]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 0.6f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient2,
				Amount = 0.016f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.001f
			}
		};
		item = foodNutrientProfile;
		list.Add(item);
		foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "highEnergy";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[2]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 2f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.009f
			}
		};
		item = foodNutrientProfile;
		list.Add(item);
		foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "balanced meal";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[3]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 1.2f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient2,
				Amount = 0.096f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.0048f
			}
		};
		item = foodNutrientProfile;
		list.Add(item);
		foodNutrientProfile = new FoodNutrientProfile();
		foodNutrientProfile.KeyName = "lowWeightBalancedMeal";
		foodNutrientProfile.FoodNutrientTypes = new FoodNutrientAmount[3]
		{
			new FoodNutrientAmount
			{
				Nutrient = nutrient,
				Amount = 1.5f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient2,
				Amount = 0.11f
			},
			new FoodNutrientAmount
			{
				Nutrient = nutrient3,
				Amount = 0.006f
			}
		};
		item = foodNutrientProfile;
		list.Add(item);
		list.Add(new FoodNutrientProfile
		{
			KeyName = "lowStimulant",
			FoodNutrientTypes = new FoodNutrientAmount[1]
			{
				new FoodNutrientAmount
				{
					Nutrient = nutrient4,
					Amount = 0.005f
				}
			}
		});
		// MOD: raw meat, vegetables and staples are specialised so a cooked meal is worth making.
		UWGame.Mods.BalancedDietMod.AdjustProfiles(list);
		return list;
	}

	protected override List<AllegianceEventType> InitAllegianceEvents()
	{
		List<AllegianceEventType> list = new List<AllegianceEventType>();
		list.Add(new AllegianceEventType
		{
			KeyName = "cargoDeliveredToPlayer",
			Event = AllegianceEvents.CargoDeliveredToPlayer,
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType
					{
						KeyName = "sdfdfhdjghjg34",
						Actions = new EventActionType[1]
						{
							new EventActionDialog
							{
								KeyName = "ttytyyt88rtr",
								DisplayText = new DynamicText
								{
									Text = "New supplies have arrived."
								},
								DisplayImage = "Rescue"
							}
						}
					}
				}
			}
		});
		list.Add(new AllegianceEventType
		{
			KeyName = "transportAbortedContract",
			Event = AllegianceEvents.TransportToPlayerAborted,
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType
					{
						KeyName = "0cad94b3-ae09-4106-a6dd-0d97b3426972",
						Actions = new EventActionType[1]
						{
							new EventActionDialog
							{
								KeyName = "24a1cb12-11a4-4b83-bd73-2e94445990a2",
								DisplayText = new DynamicText
								{
									Text = "Mission aborted."
								},
								DisplayImage = "Rescue"
							}
						}
					}
				}
			}
		});
		return list;
	}

	protected override List<UpgradeCategory> InitUpgradeCategories()
	{
		return UpgradeCategoryLoader.Init();
	}

	protected override List<UpgradeProfile> InitUpgradeProfiles()
	{
		return UpgradeProfileLoader.Init();
	}

	protected override List<BioOrderType> InitBioOrderTypes()
	{
		return OrdersLoader.Init();
	}

	protected override List<PersonalityType> InitPersonalityTypes()
	{
		return PersonalityLoader.Init();
	}

	protected override List<RepairProfile> InitRepairProfiles()
	{
		return RepairProfileLoader.Init();
	}

	protected override List<EffectProfileType> InitEffectProfileTypes()
	{
		return EffectProfileLoader.Init();
	}

	protected override List<EffectType> InitEffectTypes()
	{
		return EffectTypeLoader.Init();
	}

	protected override List<SiteData> InitSiteData()
	{
		return SiteDataLoader.Init();
	}

	protected override List<AllegianceData> InitAllegianceData()
	{
		return AllegianceDataLoader.Init();
	}

	protected override List<SiteTemplate> InitSiteTemplates()
	{
		return SiteTemplateLoader.Init();
	}

	protected override List<ExpeditionData> InitExpeditionData()
	{
		return ExpeditionDataLoader.Init();
	}

	protected override List<AllegianceTemplate> InitAllegianceTemplates()
	{
		return AllegianceTemplateLoader.Init();
	}

	protected override List<TradeProfile> InitTradeProfiles()
	{
		return TradeProfileLoader.Init();
	}

	protected override List<PricesProfile> InitPricesProfiles()
	{
		return PricesProfileLoader.Init();
	}

	protected override List<TradeGroup> InitTradeGroups()
	{
		return TradeGroupLoader.Init();
	}

	protected override List<EntityData> InitEntityData()
	{
		return EntityDataLoader.Init();
	}

	protected override List<OfferDemandProfile> InitOfferDemandProfiles()
	{
		return OfferDemandProfileLoader.Init();
	}

	protected override List<VehiclesProfile> InitVehicleProfiles()
	{
		return VehiclesProfileLoader.Init();
	}

	protected override List<StructuresProfile> InitStructureProfiles()
	{
		return StructuresProfileLoader.Init();
	}

	protected override List<TraitTemplate> InitTraitTemplates()
	{
		return TraitTemplateLoader.Init();
	}

	protected override List<CultureTemplate> InitCultureTemplates()
	{
		return CultureTemplateLoader.Init();
	}

	protected override List<TierArea> InitTierAreas()
	{
		return new List<TierArea>
		{
			new TierArea
			{
				KeyName = "comfortSurvival",
				Icon = "lcd_icon_tier_comfort1",
				Tier = "survival",
				Area = RatingTypes.Comfort,
				Description = "At this tech level, we only use the barest minimum of shelter against the elements. Our shelters are made quickly with simple materials and require frequent maintenance to remain standing."
			},
			new TierArea
			{
				KeyName = "comfortBasic",
				Icon = "lcd_icon_tier_comfort2",
				Tier = "basic",
				Area = RatingTypes.Comfort,
				Description = "At this tech level, we build sturdy buildings with sparse amenities. We allow ourselves to consume some simple stimulants."
			},
			new TierArea
			{
				KeyName = "comfortMedium",
				Icon = "lcd_icon_tier_comfort3",
				Tier = "medium",
				Area = RatingTypes.Comfort,
				Description = "At this tech level, we have buildings which are quite comfortable and offer a good range of amenities. We enjoy a variety of good quality stimulants."
			},
			new TierArea
			{
				KeyName = "comfortAdvanced",
				Icon = "lcd_icon_tier_comfort4",
				Tier = "advanced",
				Area = RatingTypes.Comfort,
				Description = "At this tech level, we have advanced habitats manufactured with precision technology, offering us the ultimate in creature comforts."
			},
			new TierArea
			{
				KeyName = "foodSurvival",
				Icon = "lcd_icon_tier_food1",
				Tier = "survival",
				Area = RatingTypes.Food,
				Description = "At this tech level, we gather or hunt food directly from the environment, and only use very simple food preparation and storage."
			},
			new TierArea
			{
				KeyName = "foodBasic",
				Icon = "lcd_icon_tier_food2",
				Tier = "basic",
				Area = RatingTypes.Food,
				Description = "At this tech level, we can feed a small community by basic plant farming and we maintain reliable food stores by using better food storage and preparation methods."
			},
			new TierArea
			{
				KeyName = "foodMedium",
				Icon = "lcd_icon_tier_food3",
				Tier = "medium",
				Area = RatingTypes.Food,
				Description = "At this tech level, we employ efficient food production and storage methods which make it possible to feed larger colonies."
			},
			new TierArea
			{
				KeyName = "foodAdvanced",
				Icon = "lcd_icon_tier_food4",
				Tier = "advanced",
				Area = RatingTypes.Food,
				Description = "At this tech level, we use technologies to synthesize food which provide the greatest variety and production capacity available."
			},
			new TierArea
			{
				KeyName = "securitySurvival",
				Icon = "lcd_icon_tier_security1",
				Tier = "survival",
				Area = RatingTypes.Security,
				Description = "At this tech level, we make do with simple weapons like spears and bows which provide a minimum of protection against threats."
			},
			new TierArea
			{
				KeyName = "securityBasic",
				Icon = "lcd_icon_tier_security2",
				Tier = "basic",
				Area = RatingTypes.Security,
				Description = "At this tech level, we use simple firearms which give us basic protection and stand-off range."
			},
			new TierArea
			{
				KeyName = "securityMedium",
				Icon = "lcd_icon_tier_security3",
				Tier = "medium",
				Area = RatingTypes.Security,
				Description = "At this tech level, we use higher-precision firearms like rifles which give a good chance to eliminate threats in a safe and efficient manner."
			},
			new TierArea
			{
				KeyName = "securityAdvanced",
				Icon = "lcd_icon_tier_security4",
				Tier = "advanced",
				Area = RatingTypes.Security,
				Description = "At this tech level, we get the best possible protection using ultra-high precision kinetic weapons, lasers and robotic systems."
			}
		};
	}

	protected override List<TierType> InitTiers()
	{
		return new List<TierType>
		{
			new TierType
			{
				KeyName = "survival",
				Name = "Survival",
				Description = "This policy tier represents the minimum requirements to stay alive.",
				Icon = "lcd_icon_tier_optional1",
				UpperEdge = 0.16f
			},
			new TierType
			{
				KeyName = "basic",
				Name = "Basic",
				Description = "Moving to this tier requires use of technology, planning and effort beyond the pure survival level. But the rewards are a higher standard of living and reduced uncertainty.",
				Icon = "lcd_icon_tier_optional2",
				UpperEdge = 0.32f
			},
			new TierType
			{
				KeyName = "medium",
				Name = "Medium",
				Description = "A colony at this development tier has expended even more organizational effort and planning. Resources are used in a sophisticated manner and the colony will experience more safety and comfort as a result.",
				Icon = "lcd_icon_tier_optional3",
				UpperEdge = 0.64f
			},
			new TierType
			{
				KeyName = "advanced",
				Name = "Advanced",
				Description = "Highly advanced technologies are needed to reach this tier. It is only reachable using tools or materials brought from Earth and sustained or replicated on Antheia. It cannot be reached using Antheia's resources alone.",
				Icon = "lcd_icon_tier_optional4",
				UpperEdge = 1f
			}
		};
	}

	protected override List<TriggerType> InitTriggerTypes()
	{
		return new List<TriggerType>
		{
			new TriggerType
			{
				KeyName = "prey",
				Range = 200f,
				DurationBetweenTriggerUpdatesInSeconds = 1.0,
				IsPrey = true
			},
			new TriggerType
			{
				KeyName = "smallImprovisedTrapTrigger",
				Range = 20f,
				DurationBetweenTriggerUpdatesInSeconds = 1.0,
				CanTriggerWhenUndetected = true,
				ActionSetsKey = "springSnareTriggered"
			},
			new TriggerType
			{
				KeyName = "spikeTrapTrigger",
				Range = 15f,
				DurationBetweenTriggerUpdatesInSeconds = 1.0,
				CanTriggerWhenUndetected = true,
				ActionSetsKey = "springSnareTriggered"
			},
			new TriggerType
			{
				KeyName = "mineTrigger",
				Range = 28f,
				DurationBetweenTriggerUpdatesInSeconds = 1.0,
				CanTriggerWhenUndetected = true,
				MaxTimesToTriggerBeforeExpiring = 1,
				ActionSetsKey = "landMineTriggered"
			},
			new TriggerType
			{
				KeyName = "animalMigrateTrigger",
				DurationBetweenTriggerUpdatesInSeconds = 1.0,
				CanTriggerWhenUndetected = true,
				ActionSetsKey = "animalLeavesMapEdge"
			},
			new TriggerType
			{
				KeyName = "creature",
				Range = 300f,
				Interest = new Interest
				{
					InterestLevelMean = 40.0,
					InterestLevelStdDeviation = 6.0
				},
				Priority = TriggerPriority.Lowest,
				DurationBetweenTriggerUpdatesInSeconds = 4.0
			},
			new TriggerType
			{
				KeyName = "drivenVehicle",
				Range = 200f,
				DurationBetweenTriggerUpdatesInSeconds = 1.0,
				IsDrivenVehicle = true,
				Interest = new Interest
				{
					InterestLevelMean = 4.0
				}
			},
			new TriggerType
			{
				KeyName = "entityDied",
				Range = 300f,
				LifetimeInSeconds = 2f,
				IsEntityDied = true,
				Priority = TriggerPriority.Normal,
				DurationBetweenTriggerUpdatesInSeconds = 1.0,
				CooldownInSeconds = 10f,
				Interest = new Interest
				{
					InterestLevelMean = 50.0,
					InterestLevelStdDeviation = 5.0
				}
			}
		};
	}

	protected override List<DetectionType> InitDetectionTypes()
	{
		List<DetectionType> list = new List<DetectionType>();
		DetectionFactor detectionFactor = new DetectionFactor
		{
			TypeTag = "wellHiddenAnimal",
			Value = 0.05f,
			RequiresExamineAction = true,
			DistanceToAlwaysDetect = 0f,
			AddLogMessageWhenDetected = true,
			SkillToUse = GameData.Instance.AllSkillTypes["hunting"]
		};
		DetectionFactor detectionFactor2 = new DetectionFactor
		{
			TypeTag = "huge",
			Value = 2f,
			DistanceToAlwaysDetect = 200f
		};
		DetectionFactor detectionFactor3 = new DetectionFactor
		{
			TypeTag = "hardToSpot",
			Value = 0.01f,
			DistanceToAlwaysDetect = 48f
		};
		DetectionFactor detectionFactor4 = new DetectionFactor
		{
			TypeTag = "kindaHardToSpot",
			Value = 0.04f,
			DistanceToAlwaysDetect = 120f
		};
		list.Add(new DetectionType
		{
			KeyName = "human",
			DetectionFactors = new DetectionFactor[19]
			{
				new DetectionFactor
				{
					TypeTag = "largeAboveGround",
					RequiresExamineAction = false,
					DistanceToAlwaysDetect = 140f,
					InterestLevelForSpottedResourceMean = 0f,
					Value = 0.9f,
					AddLogMessageWhenDetected = false
				},
				new DetectionFactor
				{
					TypeTag = "largeOnGround",
					RequiresExamineAction = false,
					DistanceToAlwaysDetect = 90f,
					InterestLevelForSpottedResourceMean = 0f,
					Value = 0.8f,
					AddLogMessageWhenDetected = false
				},
				new DetectionFactor
				{
					TypeTag = "smallAboveGround",
					RequiresExamineAction = false,
					DistanceToAlwaysDetect = 0f,
					Value = 0.3f,
					AddLogMessageWhenDetected = false,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "inShallowWater",
					RequiresExamineAction = false,
					DistanceToAlwaysDetect = 0f,
					Value = 0.05f,
					AddLogMessageWhenDetected = true
				},
				new DetectionFactor
				{
					TypeTag = "smallAnimalAboveGround",
					RequiresExamineAction = false,
					DistanceToAlwaysDetect = 0f,
					Value = 0.6f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "smallAnimalAboveGroundHardToSee",
					RequiresExamineAction = false,
					DistanceToAlwaysDetect = 0f,
					Value = 0.05f,
					AddLogMessageWhenDetected = true
				},
				new DetectionFactor
				{
					TypeTag = "smallAnimalOnGround",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.15f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "smallAnimalBelowGround",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.1f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "aboveGroundHardToSee",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.15f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "farmSpotAndResourceDeposit",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.95f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "inDeeperWaterFishingSpot",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.2f,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "inDeeperWater",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.1f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "fruitOnGroundHardToFind",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.15f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "smallHidden",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.1f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeKey = "entity:bird",
					Value = 1f,
					DistanceToAlwaysDetect = 600f
				},
				detectionFactor,
				detectionFactor2,
				detectionFactor3,
				detectionFactor4
			}
		});
		list.Add(new DetectionType
		{
			KeyName = "defaultDetection",
			DetectionFactors = new DetectionFactor[4] { detectionFactor, detectionFactor2, detectionFactor3, detectionFactor4 }
		});
		list.Add(new DetectionType
		{
			Comments = "This sensor should not have any detecting capabilites other than the ability to report its parent's status.",
			KeyName = "communicationSensor",
			DetectionDisabled = true
		});
		list.Add(new DetectionType
		{
			KeyName = "motionSensor",
			DetectionFactors = new DetectionFactor[13]
			{
				new DetectionFactor
				{
					TypeTag = "smallAnimalAboveGround",
					RequiresExamineAction = false,
					DistanceToAlwaysDetect = 0f,
					Value = 0.6f,
					AddLogMessageWhenDetected = true
				},
				new DetectionFactor
				{
					TypeTag = "smallAnimalAboveGroundHardToSee",
					RequiresExamineAction = false,
					DistanceToAlwaysDetect = 0f,
					Value = 0.05f,
					AddLogMessageWhenDetected = true
				},
				new DetectionFactor
				{
					TypeTag = "smallAnimalOnGround",
					RequiresExamineAction = false,
					DistanceToAlwaysDetect = 0f,
					Value = 0.05f,
					AddLogMessageWhenDetected = false
				},
				new DetectionFactor
				{
					TypeTag = "smallAnimalBelowGround",
					RequiresExamineAction = false,
					DistanceToAlwaysDetect = 0f,
					Value = 0.02f,
					AddLogMessageWhenDetected = true
				},
				new DetectionFactor
				{
					TypeTag = "fruitOnGroundHardToFind",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.05f,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "inDeeperWater",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.028f,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "inDeeperWaterFishingSpot",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.06f,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "largeAboveGround",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 140f,
					InterestLevelForSpottedResourceMean = 0f,
					Value = 0.9f,
					AddLogMessageWhenDetected = false,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "largeOnGround",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 90f,
					InterestLevelForSpottedResourceMean = 0f,
					Value = 0.8f,
					AddLogMessageWhenDetected = false,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "smallAboveGround",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.3f,
					AddLogMessageWhenDetected = false,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "inShallowWater",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.05f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "aboveGroundHardToSee",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.05f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				},
				new DetectionFactor
				{
					TypeTag = "smallHidden",
					RequiresExamineAction = true,
					DistanceToAlwaysDetect = 0f,
					Value = 0.014f,
					AddLogMessageWhenDetected = true,
					SkillToUse = GameData.Instance.AllSkillTypes["foraging"]
				}
			}
		});
		return list;
	}
}
