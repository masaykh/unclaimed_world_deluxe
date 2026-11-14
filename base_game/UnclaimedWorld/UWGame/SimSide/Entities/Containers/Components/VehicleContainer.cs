using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameEngine.Sim.Sim.Entities.Container;
using UWGame.SimSide.Vehicles;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;
using UWGame.ClientSide;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.Entities.Containers.Components
{
    /*
 IGarrison and IStorage interfaces, which the rest of the game classes use to relate to a Container component in different ways.
[02:29:41] Mark Lorenzen: Container as IGarrison
[02:29:52] Mark Lorenzen: Container as IEquipment
[02:30:05] Mark Lorenzen: Container as IStorage
[02:31:26] Mark Lorenzen: each of these interfaces would allow other classes to enter, exit, store, remember, inhabit, transport, crew... the container, if the interface is present.
[02:31:50] Mark Lorenzen: in the case of an encampment building...
[02:32:01] Mark Lorenzen: it would need to do at least two things:
[02:32:12] Mark Lorenzen: 1) be a place to leave your stuff
[02:32:34] Mark Lorenzen: 2) be a place to go into and do homey stuff like sleep
[02:32:59] Mark Lorenzen: so write a class ContainerHome
[02:33:19] Mark Lorenzen: inherit IStorage and IGarrison
[02:33:57] Mark Lorenzen: IStorage exposes the CombinedStorage style functions.
[02:34:19] Mark Lorenzen: IGarrison would be new, exposing the functions for people entering and Exiting
[02:34:42] Mark Lorenzen: also inherit IExit, which handles the physical transitioning into and out of the building.     
     
 other buildings might be IGarrison but not IStorage... like a watchtower
[02:35:45] Mark Lorenzen: or the other way around... a pup tent is a home but not a storage
[02:36:16] Mark Lorenzen: a vehicle would inherit ITransport and ICrew
[02:36:27] Mark Lorenzen: the crew is the driver
[02:36:40] Mark Lorenzen: passengers and items are contained via ITransport
 */

    public class VehicleContainer : Container, ICrew, ITransport, IGarrison, IStorage, IExit, IHasReplenishItems // IReplenishes
    {
        /// <summary>
        /// The roll angle about the direction vector.
        /// Positive = roll to the right.
        /// Negative = roll to the left.
        /// </summary>
        public float Roll = 0f;


        /// <summary>
        /// The pitch angle.
        /// </summary>
        public float Pitch = 0f;

        // these are used in AI evaluators:
        public const float CommonLowestHaulingSpeed = 0.8f;
        public const float CommonCarryLimit = 6f;

        public Entity DrivenBy;
        public Aircraft Aircraft;

        public List<Entity> Passengers = new List<Entity>();
        public List<Passenger> PassengerItinerary = new List<Passenger>();
        private EntityID drivenByID;
        private List<EntityID> snapshotPassengers;

        public float CargoCapacityTakenUpByPassengers;
        public List<PassengerOrCargoSlot> Slots = new List<PassengerOrCargoSlot>();

        /// <summary>
        /// Used when we are accepting passengers and waiting for the passengers to take their seat.
        /// </summary>
        public int WaitingForPassengers = 0;

        ItemStorage storage;

        Residence residence;


        /// <summary>
        /// contains agents - crew + passengers... it doesn't seem we need a new class..?
        /// </summary>
        Garrison garrison;


        /// <summary>
        /// fuel, power etc.
        /// </summary>
        ReplenishItems replenishItems;
        public ReplenishItems ReplenishItems
        {
            get
            {
                return replenishItems;
            }
        }


        public VehicleContainer(Entity parent)
            : base(parent)
        {
            VehicleContainerType vehicleType = (VehicleContainerType)parent.EntityType.ContainerType;

            if (vehicleType.Aircraft != null)
            {
                Aircraft = new Aircraft();
            }

            if (vehicleType.PassengerOrCargoSlotTypes != null)
            {
                foreach (PassengerOrCargoSlotType slot in vehicleType.PassengerOrCargoSlotTypes)
                {
                    PassengerOrCargoSlot thisSlot = new PassengerOrCargoSlot(this) { PassengerOrCargoSlotType = slot };
                    if (slot.CargoSlotType != null)
                    {
                        thisSlot.CargoSlot = new CargoSlot(thisSlot);
                    }

                    if (slot.PassengerSlotType != null)
                    {
                        thisSlot.PassengerSlot = new PassengerSlot();

                        if (slot.PassengerSlotType.IsDriversSeat)
                        {
                            thisSlot.DriversSlot = new DriversSlot();
                        }
                    }

                    Slots.Add(thisSlot);
                }
            }

            storage = new ItemStorage(parent, true, vehicleType.ItemStorageType);

            if (vehicleType.RequiresReplenishType != null)
            {
                replenishItems = new ReplenishItems(parent);
            }

            if (vehicleType.MaxPassengers > 0)
            {
                garrison = new Garrison(parent); // unmanned vehicles..?
            }

            if (parent.EntityType.ContainerType.ResidenceType != null)
            {
                residence = new Residence(parent);
            }
        }

        public VehicleContainer()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }

        public override void Destroy()
        {
            // eject all items:
            storage.UncontainAllEntities(
                GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

            if (replenishItems != null)
            {
                replenishItems.UncontainAllEntities(
                    GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                    GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

                replenishItems.Destroy();
            }

            if (garrison != null)
            {
                garrison.UncontainAllEntities();
            }

            if (Slots != null)
            {
                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    slot.Destroy();
                }
            }

        }


        public Storage FindStorage(StorageID storageID)
        {
            Storage foundStorage = storage.FindStorage(storageID);
            
            return foundStorage;
        }

        public override void IterateContained(Action<Entity> del) //Container.IterateMethod del)
        {
            if (storage != null)
            {
                storage.IterateContained(del);
            }

            if (garrison != null)
            {
                garrison.IterateContained(del);
            }
        }

        public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
        {
            List<Entity> items = new List<Entity>();

            if (storage != null)
            {
                storage.GetContainedItemsList(rule, items);
            }

            if (garrison != null)
            {
                garrison.GetContainedItemsList(rule, items);
            }

            return items;
        }

        public float TotalStored
        {
            get
            {
                return storage.TotalStored;
            }
        }

        public float TotalItemStorageCapacity
        {
            get
            {
                return storage.TotalCapacity;
            }
        }
        public Storage GetStoredIn(Entity entity)
        {
            return storage.GetStoredIn(entity.EntityID);
        }

        public override bool Contains(EntityID entityID)
        {            
            // TODO
            return storage.Contains(entityID) || (garrison != null && garrison.Contains(entityID));
        }

        public Dictionary<StorageCondition, Storage> GetStorageSpaces()
        {
            return storage.StorageSpaces;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public EntityID? Driver
        {
            get
            {
                return DrivenBy?.EntityID;
            }
            set
            {
                if (DrivenBy != null)
                {
                    DrivenBy.DrivingVehicle = null;
                }

                if (value.HasValue)
                {
                    DrivenBy = Entity.FindByID(value.Value);
                    if (DrivenBy != null)
                    {
                        DrivenBy.DrivingVehicle = Parent.EntityID;
                    }
                }
                else
                {
                    DrivenBy = null;
                }
            }
        }

        public bool IsDriver(Entity entity)
        {
            return DrivenBy == entity;
        }

        public bool IsPassenger(Entity entity)
        {
            return Passengers.Contains(entity);
        }

        public StorageCompartment GetCompartment(StorageID storageID)
        {
            return StorageCompartment.NormalStorage;
        }

        public void TellPassengerToGetOff(Entity sendingEntity, Passenger passenger)
        {
            passenger.Entity.SendMessage(new Message(sendingEntity, Message.MessageTypes.GetOff, null));
            if (PassengerItinerary.Contains(passenger))
            {
                PassengerItinerary.Remove(passenger);
            }
        }

        public void TellPassengerToGetOff(Entity sendingEntity, Entity passenger)
        {
            passenger.SendMessage(new Message(sendingEntity, Message.MessageTypes.GetOff, null));

            int passengerToRemove = -1;
            for (int i = 0; i < PassengerItinerary.Count; i++)
            {
                if (PassengerItinerary[i].Entity == passenger)
                {
                    passengerToRemove = i;
                    break;
                }
            }
            if (passengerToRemove != -1)
            {
                PassengerItinerary.RemoveAt(passengerToRemove);
            }
        }

        private void RecalculateCargoCapacity()
        {
            CargoCapacityTakenUpByPassengers = 0f;

            foreach (PassengerOrCargoSlot slot in Slots)
            {
                if (slot.CargoSlot != null
                    && ((slot.PassengerSlot != null && slot.PassengerSlot.Passenger != null)
                        || (slot.DriversSlot != null && slot.DriversSlot.Driver != null)))
                {
                    CargoCapacityTakenUpByPassengers += slot.PassengerOrCargoSlotType.CargoSlotType.Capacity;
                }
            }

            if (this.storage != null)
            {
                this.storage.RecalculateTotalCapacity();
            }
        }
        /*
        public bool EnterVehicleAsPassenger(Entity entity, PassengerOrCargoSlot slot)
        {
            Passengers.Add(entity);
            entity.PassengerInVehicle = Parent.EntityID;

            slot.PassengerSlot.Passenger = entity;
            slot.PassengerSlot.TargetedBy = null;

            RecalculateCargoCapacity();

            return false;
        }

        public bool EnterVehicleAsDriver(Entity entity, PassengerOrCargoSlot slot)
        {
            DrivenBy = entity;
            entity.DrivingVehicle = Parent.EntityID;

            slot.DriversSlot.Driver = entity;
            slot.DriversSlot.TargetedBy = null;

            this.AddToContain(entity);

            RecalculateCargoCapacity();

            return false;
        }

        public bool ExitVehicle(Entity entity)
        {
            bool isOnBoard = false;
            if (DrivenBy == entity)
            {
                DrivenBy = null;
                entity.DrivingVehicle = null;
                isOnBoard = true;
            }
            else if (Passengers != null && Passengers.Contains(entity))
            {
                Passengers.Remove(entity);
                entity.PassengerInVehicle = null;
                isOnBoard = true;
            }

            if (isOnBoard)
            {
                PassengerOrCargoSlot slot = GetEntityPlaceInVehicle(entity);

                if (slot.PassengerSlot != null)
                {
                    slot.PassengerSlot.Passenger = null;
                    slot.PassengerSlot.TargetedBy = null;
                }

                if (slot.DriversSlot != null)
                {
                    slot.DriversSlot.Driver = null;
                    slot.DriversSlot.TargetedBy = null;
                }

                RecalculateCargoCapacity();
            }

            return isOnBoard;
        }
        */

        public PassengerOrCargoSlot GetEntityPlaceInVehicle(Entity entity)
        {
            foreach (PassengerOrCargoSlot slot in Slots)
            {
                if (slot.PassengerSlot != null && slot.PassengerSlot.Passenger != null && slot.PassengerSlot.Passenger == entity)
                {
                    return slot;
                }
                else if (slot.DriversSlot != null && slot.DriversSlot.Driver != null && slot.DriversSlot.Driver == entity)
                {
                    return slot;
                }
            }

            return null;
        }

        public void GetRendezvousPoint(Vector3 vehicleLocation, Vector3 entityLocation, out Vector3 rendezvousLocation)
        {
            Common.GetLocationAtDistance(vehicleLocation, entityLocation, 60f, out rendezvousLocation);
        }

        public bool HasIntelligenceOnboard()
        {
            if (DrivenBy != null)
            {
                return true;
            }

            foreach (Entity entity in Passengers)
            {
                if (entity.Intelligence != null)
                {
                    return true;
                }
            }

            return false;
        }

        public List<PassengerOrCargoSlot> GetCargoSlotsForLoading(float bulkToLoad)
        {
            if (Slots != null)
            {
                List<PassengerOrCargoSlot> listOfCargoSlots = null;

                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    if (slot.CargoSlot != null
                        && !slot.CargoSlot.IsFull()
                        && (slot.PassengerSlot == null || (slot.PassengerSlot.Passenger == null && slot.PassengerSlot.TargetedBy == null))
                        && (slot.DriversSlot == null || (slot.DriversSlot.Driver == null && slot.DriversSlot.TargetedBy == null)))
                    {
                        if (listOfCargoSlots == null)
                        {
                            listOfCargoSlots = new List<PassengerOrCargoSlot>();
                        }

                        listOfCargoSlots.Add(slot);
                    }
                }

                if (listOfCargoSlots != null)
                {
                    listOfCargoSlots.Sort((a, b) => b.CargoSlot.BulkCarried.CompareTo(a.CargoSlot.BulkCarried));

                    float bulkStillToLoad = bulkToLoad;

                    List<PassengerOrCargoSlot> cargoSlotsToLoadInto = new List<PassengerOrCargoSlot>();
                    foreach (PassengerOrCargoSlot slot in listOfCargoSlots)
                    {
                        float roomInSlot = slot.CargoSlot.GetRemainingRoom();

                        bulkStillToLoad -= roomInSlot;

                        cargoSlotsToLoadInto.Add(slot);

                        if (bulkStillToLoad <= 0f)
                        {
                            break;
                        }
                    }

                    return cargoSlotsToLoadInto;
                }
            }

            return null;
        }

        public List<PassengerOrCargoSlot> GetCargoSlotsForUnloading(float bulkToUnload)
        {
            if (Slots != null)
            {
                List<PassengerOrCargoSlot> listOfCargoSlots = null;

                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    if (slot.CargoSlot != null && slot.CargoSlot.BulkCarried > 0f)
                    {
                        if (listOfCargoSlots == null)
                        {
                            listOfCargoSlots = new List<PassengerOrCargoSlot>();
                        }

                        listOfCargoSlots.Add(slot);
                    }
                }

                if (listOfCargoSlots != null)
                {
                    listOfCargoSlots.Sort((a, b) => a.CargoSlot.BulkCarried.CompareTo(b.CargoSlot.BulkCarried));

                    float bulkStillToUnload = bulkToUnload;

                    List<PassengerOrCargoSlot> cargoSlotsToUnloadFrom = new List<PassengerOrCargoSlot>();
                    foreach (PassengerOrCargoSlot slot in listOfCargoSlots)
                    {
                        float bulkInSlot = slot.CargoSlot.BulkCarried;

                        bulkStillToUnload -= bulkInSlot;

                        cargoSlotsToUnloadFrom.Add(slot);

                        if (bulkStillToUnload <= 0f)
                        {
                            break;
                        }
                    }

                    return cargoSlotsToUnloadFrom;
                }
            }

            return null;
        }

        // Vehicle, inventory, storage, and visitor should all share a common contain parent.
        public void UpdateCargoSlotsWithAttachedModels()
        {
            foreach (PassengerOrCargoSlot slot in Slots)
            {
                if (slot.CargoSlot != null)
                {
                    AttachPoint attachPoint = Parent.Renderable.RenderAsModel.ModelData.GetAttachPointFromKeyName(slot.PassengerOrCargoSlotType.AttachPointName);

                    if (attachPoint != null)
                    {
                        if (slot.CargoSlot.BulkCarried > 0f)
                        {
                            Parent.Renderable.AttachPooledObjectIfPossible("box", attachPoint, AttacheePoint.Bottom, true);
                        }
                        else
                        {
                            Parent.Renderable.RemoveAttachedBoxModelsFromHand();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clean up procedure.
        /// </summary>
        public void EmptyAllCargoSlots()
        {
            foreach (PassengerOrCargoSlot slot in Slots)
            {
                if (slot.CargoSlot != null)
                {
                    slot.CargoSlot.BulkCarried = 0f;
                }
            }
        }

        public PassengerOrCargoSlot GetFreeDriversSlot()
        {
            if (Slots != null)
            {
                float bestComfort = 0;
                PassengerOrCargoSlot bestSlot = null;
                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    if (slot.DriversSlot != null && slot.DriversSlot.Driver == null && slot.DriversSlot.TargetedBy == null)
                        && (slot.CargoSlot == null || (slot.CargoSlot.BulkCarried == 0f && slot.CargoSlot.TargetedByHauler == null)))
                    {
                        if (slot.PassengerOrCargoSlotType.PassengerSlotType.Comfort > bestComfort)
                        {
                            bestSlot = slot;
                        }
                    }
                }

                return bestSlot;
            }

            return null;
        }

        public PassengerOrCargoSlot GetPlaceForNewPassenger()
        {
            if (Slots != null)
            {
                float bestComfort = 0;
                PassengerOrCargoSlot bestSlot = null;
                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    if (slot.PassengerSlot != null && slot.PassengerSlot.Passenger == null && slot.PassengerSlot.TargetedBy == null
                        && (slot.DriversSlot == null || (slot.DriversSlot.Driver == null && slot.DriversSlot.TargetedBy == null))
                        && (slot.CargoSlot == null || (slot.CargoSlot.BulkCarried == 0f && slot.CargoSlot.TargetedByHauler == null)))
                    {
                        if (slot.PassengerOrCargoSlotType.PassengerSlotType.Comfort > bestComfort)
                        {
                            bestSlot = slot;
                        }
                    }
                }

                return bestSlot;
            }

            return null;
        }

        public Vector2 GetRelativePointRotated(Vector2? pointOnModel)
        {
            if (pointOnModel.HasValue)
            {
                Matrix rotationMatrix = Matrix.CreateRotationZ(Parent.Rotation);

                return new Vector2(Parent.PlaySiteLocation.X, Parent.PlaySiteLocation.Y) + Vector2.Transform(pointOnModel.Value, rotationMatrix);
            }

            return new Vector2(Parent.PlaySiteLocation.X, Parent.PlaySiteLocation.Y);
        }

        public void ComputeRelativePointInWorld(Vector2 pointOnModel, out Vector3 transformedLocation)
        {
            Matrix rotationMatrix =
                   Matrix.CreateRotationX(Roll) *
                   Matrix.CreateRotationZ(Parent.Rotation);

            Vector2 vehicleLocation = Parent.PlaySiteLocation.ToVector2();

            Vector2 transformedPoint = vehicleLocation + Vector2.Transform(pointOnModel, rotationMatrix);

            if (MapManager.IsPointReachableInStraightLine(Parent.PlaySiteLocation, transformedPoint.ToVector3()))
            {
                transformedLocation = transformedPoint.ToVector3();
            }
            else
            {
                transformedLocation = Parent.PlaySiteLocation;
            }
        }

        public static bool FindParkingSpot(Point from, Allegiances.Allegiance allegiance, IKnownEntityData vehicle, Rectangle tileArea, SurfaceType.TransportType transport, ProtectionLevel protectionLevel,
            EntityType driverType, ThreatStance approach,
            Point targetTile,
            out Vector3 foundLocation)
        {
            Point targetTile1;
            Point? targetTile2 = null;

            foundLocation = Vector3.Zero;

            targetTile1 = targetTile;

            if (The.Map.IsNoParkingSpot(vehicle.EntityType, protectionLevel, driverType, approach, targetTile))
            {
                // Bail out if this was previously registered as a no parking spot area.
                return false;
            }

            MovementMap moveMap = allegiance.SharedKnowledge.GetMovementMap(protectionLevel, driverType, approach);
            SurfaceType.TransportType transportToUse = transport;
            if (transportToUse == SurfaceType.TransportType.Air)
            {
                transportToUse = SurfaceType.TransportType.OffRoad;
            }
            bool[][] isBlockedMap = Structure.CreateIsBlockedMapAvoidConstructions(tileArea, The.Map.TerrainCosts[transportToUse]);

            byte[][] subtileMap = null;
            Common.InitJaggedArray(ref subtileMap, Common.GetJaggedArrayWidth(isBlockedMap), Common.GetJaggedArrayHeight(isBlockedMap));

            byte passableValue = 40;
            byte unpassableValue = 10;
            int targetCenterValue = 40;
            int targetRadius = 18;

            int width = Common.GetJaggedArrayWidth(subtileMap);
            int height = Common.GetJaggedArrayHeight(subtileMap);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    subtileMap[x][y] = passableValue;
                }
            }

            CreatePassageMap(vehicle, tileArea, subtileMap, vehicle.BoundingRadius3D, isBlockedMap, unpassableValue);

            byte[][] passageMap = null;
            if (targetTile2.HasValue)
            {
                passageMap = Common.CloneJaggedArray(subtileMap);
            }

            Point targetSubtile2;
            Point targetSubtile1 = new Point((targetTile1.X - tileArea.X) * 3, (targetTile1.Y - tileArea.Y) * 3);
            InfluenceMap.DrawLinearInfluenceCircle(subtileMap, targetSubtile1, targetCenterValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, targetRadius);

            Point bestSubtilePoint = new Point(-1, -1);
            Point bestSubtilePoint1;
            Point bestSubtilePoint2;
            int bestTarget1Value = InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subtileMap, isBlockedMap, out bestSubtilePoint1);
            if (targetTile2.HasValue)
            {
                targetSubtile2 = new Point((targetTile2.Value.X - tileArea.X) * 3, (targetTile2.Value.Y - tileArea.Y) * 3);
                InfluenceMap.DrawLinearInfluenceCircle(passageMap, targetSubtile2, targetCenterValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, targetRadius);
                int bestTarget2Value = InfluenceMap.GetBestSubtileLocationThatIsntBlocked(passageMap, isBlockedMap, out bestSubtilePoint2);

                if (bestTarget2Value > bestTarget1Value)
                {
                    bestSubtilePoint = bestSubtilePoint2;
                }
                else if (bestTarget1Value > bestTarget2Value)
                {
                    bestSubtilePoint = bestSubtilePoint1;
                }
                else if (bestTarget1Value != -1)
                {
                    if (Common.DistanceOctile(from, bestSubtilePoint1) < Common.DistanceOctile(from, bestSubtilePoint2))
                    {
                        bestSubtilePoint = bestSubtilePoint1;
                    }
                    else
                    {
                        bestSubtilePoint = bestSubtilePoint2;
                    }
                }
                else
                {
                    The.Map.RegisterNoParkingSpot(vehicle.EntityType, protectionLevel, driverType, approach, targetTile);
                    return false;
                }
            }
            else
            {
                bestSubtilePoint = bestSubtilePoint1;
            }

            if (bestSubtilePoint.X != -1)
            {
                MapManager.SubtileAndTilePosToWorldPos(bestSubtilePoint, new Point(tileArea.Left, tileArea.Top), out foundLocation);

                if (foundLocation.X < 0 || foundLocation.Y < 0)
                {
                    throw new Exception();
                }

                return true;
            }

            return false;
        }

        public static Rectangle GetSurroundingAreaUsingEntityRadius(IKnownEntityData entity, Point destination)
        {
            int tileRadius = (int)(GameData.Instance.AIConstants.AreaSizeRadiusForFindingParkingSpot * entity.BoundingRadius3D / MapManager.tileSize);

            Entity targetBuilding = The.Map.TileMap[destination.X][destination.Y].GetMainBuildingOnTile();
            Point topLeft;
            Point bottomRight;
            if (targetBuilding == null)
            {
                topLeft = The.Map.ClampTileMapPosition(new Point(destination.X - tileRadius, destination.Y - tileRadius));
                bottomRight = The.Map.ClampTileMapPosition(new Point(destination.X + tileRadius, destination.Y + tileRadius));
            }
            else
            {
                topLeft = The.Map.ClampTileMapPosition(new Point(targetBuilding.MapPosition.Value.X - tileRadius, targetBuilding.MapPosition.Value.Y - tileRadius));
                bottomRight = The.Map.ClampTileMapPosition(new Point(targetBuilding.MapPosition.Value.X + targetBuilding.EntityType.StructureType.WidthInTiles + tileRadius,
                    targetBuilding.MapPosition.Value.Y + targetBuilding.EntityType.StructureType.HeightInTiles + tileRadius));
            }

            return new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }

        /// <summary>
        /// "Grows" an isBlockedMap with negative influence. SubtileMap must be pre-filled.
        /// Also adds negative influence from vehicles and entities according to the radius.
        /// </summary>
        public static void CreatePassageMap(IKnownEntityData entity, Rectangle tileArea, byte[][] subtileMap, float agentRadius, bool[][] isBlockedMap, byte notPassableValue)
        {
            int agentSubtileRadius = (int)(agentRadius / MapManager.subTileSize);

            int width = Common.GetJaggedArrayWidth(subtileMap);
            int height = Common.GetJaggedArrayHeight(subtileMap);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (isBlockedMap[x][y])
                    {
                        InfluenceMap.DrawLinearInfluenceCircle(subtileMap, new Point(x, y),
                                    notPassableValue, InfluenceMap.Operation.SetValue, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, agentSubtileRadius);
                    }
                }
            }

            TerrainTile[][] map = The.Map.TileMap;

            Vector3 topLeftOfArea = MapManager.EdgeOfTileToWorldPos(tileArea.Left, tileArea.Top);

            for (int x = tileArea.X; x < tileArea.X + tileArea.Width; x++)
            {
                for (int y = tileArea.Y; y < tileArea.Y + tileArea.Height; y++)
                {
                    TerrainTile tile = map[x][y];
                    if (tile.EntitiesOnTile != null)
                    {
                        foreach (Entity tileEntity in tile.EntitiesOnTile)
                        {
                            if (tileEntity != entity && tileEntity.ContainedBy == null
                                && !tileEntity.IsInsideVehicle()
                                && tileEntity.Renderable.RenderAsModel != null)
                            {
                                int combinedRadius = (int)((agentRadius + tileEntity.BoundingRadius3D) / MapManager.subTileSize);
                                Point subTilePosOfEntity = MapManager.WorldPosToRelativeSubtile(tileEntity.PlaySiteLocation, topLeftOfArea);
                                InfluenceMap.DrawLinearInfluenceCircle(subtileMap, subTilePosOfEntity,
                                        notPassableValue, InfluenceMap.Operation.SetValue, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, combinedRadius);
                            }
                        }
                    }
                }
            }
        }

        protected override bool AddToContainList(Entity entity, StorageCompartment? compartment, StorageCondition placeInStorage,
            List<PassengerOrCargoSlot> slotsToUse, bool ignoreCapacity, bool replenish, bool isProductionOutput,
            UpgradeCategory upgradeCategory, PassengerOrCargoSlot seat, bool asDriver)
        {
            if (seat == null)
            {
                return !asDriver && AddToContainList(entity, compartment, placeInStorage, slotsToUse, ignoreCapacity, replenish, isProductionOutput, upgradeCategory);
            }

            if (!Slots.Contains(seat) || entity.ContainedBy.HasValue || DrivenBy == entity || Passengers.Contains(entity)
                || seat.PassengerSlot == null || seat.PassengerSlot.Passenger != null
                || (seat.PassengerSlot.TargetedBy != null && seat.PassengerSlot.TargetedBy != entity)
                || (seat.DriversSlot != null && (seat.DriversSlot.Driver != null
                    || (seat.DriversSlot.TargetedBy != null && seat.DriversSlot.TargetedBy != entity)))
                || (seat.CargoSlot != null && (seat.CargoSlot.BulkCarried > 0f || seat.CargoSlot.TargetedByHauler != null))
                || (asDriver && (seat.DriversSlot == null || DrivenBy != null)))
            {
                return false;
            }

            if (garrison == null || !garrison.Add(entity.EntityID))
            {
                return false;
            }

            if (asDriver)
            {
                DrivenBy = entity;
                entity.DrivingVehicle = Parent.EntityID;
                seat.DriversSlot.Driver = entity;
                seat.DriversSlot.TargetedBy = null;
            }
            else
            {
                Passengers.Add(entity);
                entity.PassengerInVehicle = Parent.EntityID;
                seat.PassengerSlot.Passenger = entity;
                seat.PassengerSlot.TargetedBy = null;
            }

            RecalculateCargoCapacity();
            return true;
        }

        protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
        {
            bool isDriver = DrivenBy == entity;
            bool isPassenger = Passengers != null && Passengers.Contains(entity);
            if (isDriver || isPassenger)
            {
                PassengerOrCargoSlot seat = GetEntityPlaceInVehicle(entity);
                if (isDriver)
                {
                    DrivenBy = null;
                    entity.DrivingVehicle = null;
                    if (seat?.DriversSlot != null)
                    {
                        seat.DriversSlot.Driver = null;
                        seat.DriversSlot.TargetedBy = null;
                    }
                }
                else
                {
                    Passengers.Remove(entity);
                    entity.PassengerInVehicle = null;
                    if (seat?.PassengerSlot != null)
                    {
                        seat.PassengerSlot.Passenger = null;
                        seat.PassengerSlot.TargetedBy = null;
                    }
                }

                bool removed = garrison != null && garrison.Remove(entity.EntityID);
                RecalculateCargoCapacity();
                if (Parent.Locomotor != null)
                {
                    Parent.Locomotor.CurrentMaximumSpeedIsDirty = true;
                    Parent.Locomotor.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
                }
                return removed;
            }

            if (slots != null)
            {
                foreach (PassengerOrCargoSlot slot in slots)
                {
                    slot.CargoSlot.TargetedByHauler = null;
                }
            }

            bool wasRemoved = storage.Remove(entity, true);

            if (!wasRemoved && garrison != null)
            {
                wasRemoved = garrison.Remove(entity.EntityID);
            }

            if (wasRemoved)
            {


                // handle the slots (if present):
                float storedInSlot;
                float bulkStillToUnload = entity.Bulk;

                if (slots != null)
                {
                    foreach (PassengerOrCargoSlot slot in slots)
                    {
                        storedInSlot = slot.CargoSlot.BulkCarried;

                        // roomInSlot = slot.CargoSlot.GetRemainingRoom();

                        if (storedInSlot >= bulkStillToUnload)
                        {
                            slot.CargoSlot.BulkCarried -= bulkStillToUnload;

                            break;
                        }
                        else
                        {
                            slot.CargoSlot.BulkCarried -= storedInSlot;

                            bulkStillToUnload -= storedInSlot;
                        }

                    }
                }

                if (TotalStored == 0f)
                {
                    // clean up!!!
                    EmptyAllCargoSlots();
                }

                UpdateCargoSlotsWithAttachedModels();
            }




            if (wasRemoved)
            {
                if (Parent.Locomotor != null)
                {
                    Parent.Locomotor.CurrentMaximumSpeedIsDirty = true;
                    Parent.Locomotor.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
                    // must not affect evaluator speed.
                }

                //RecalculateAndSetCurrentSpeed();

                //HandleAttachedBoxAnimation(parent);
            }

            //   return wasRemoved;

            return true;

        }


        public int GetNoOfAgentsInside()
        {
            if (garrison != null)
            {
                return garrison.GetNoOfAgentsInside();
            }
            else return 0;
        }

        /// <summary>
        /// TODO: clean this up
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="compartment"></param>
        /// <param name="placeInStorage"></param>
        /// <param name="slotsToUse"></param>
        /// <returns></returns>
        protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, 
            bool ignoreCapacity = false,
            bool replenish = false,
            bool isProductionOutput = false,
            UpgradeCategory upgradeCategory = null) //bool isUpgrade = false)
        {
            if (Garrison.EntityBelongs(entity))
            {
                if (garrison != null)
                {
                    return garrison.Add(entity.EntityID);
                }
                else return false;
            }
            else
            {
                return storage.Add(entity, placeInStorage, ignoreCapacity);
            }

           /* if (!storage.Add(entity, null, ignoreCapacity))
            {
                return false;
            }*/

            return true;

            // TODO: enable this code again:

            foreach (PassengerOrCargoSlot slotToLoad in slotsToUse)
            {
                slotToLoad.CargoSlot.TargetedByHauler = null;
            }
                      

            entity.Item.OKToTakeThisItemFromCarrier = null;


            Parent.Locomotor.CurrentMaximumSpeedIsDirty = true;
            Parent.Locomotor.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
            // must not affect evaluator speed.

            float roomInSlot;
            float bulkStillToLoad = entity.Bulk;
            //float bulkToLoadInThisSlot;

            // fill 'em up:
            foreach (PassengerOrCargoSlot slotToLoad in slotsToUse)
            {
                roomInSlot = slotToLoad.CargoSlot.GetRemainingRoom();

                if (roomInSlot >= bulkStillToLoad)
                {
                    slotToLoad.CargoSlot.BulkCarried += bulkStillToLoad;

                    break;
                }
                else
                {
                    slotToLoad.CargoSlot.BulkCarried += roomInSlot;

                    bulkStillToLoad -= roomInSlot;
                }
            }



            UpdateCargoSlotsWithAttachedModels();


            return true;
           
        }

        public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
        {
            // TODO: add crew...
            StorageCondition conditions = storage.GetStorageConditions(itemToRemove.EntityID);

            if (Remove(itemToRemove, null))
            {
                AddToContain(exchangeWithItem, null, conditions, ignoreCapacity: true);
            }

          /*  if (storage.Contains(itemToRemove))
            {
                storage.SwitchEntities(itemToRemove, exchangeWithItem);
            }*/

          /*  if (garrison.Contains(itemToRemove.ID))
            {
                garrison.Remove(itemToRemove.ID);

                storage.Add(exchangeWithItem, null);

                exchangeWithItem.ContainedBy = parent.ID;

            }*/
        }

      /*  public void UnloadFromVehicle(Entity vehicle, Vector3 location)// Point tilePos, Vector2 tileCenterOffset)
        {
            //OnBoard = null;
            vehicle.AgentStorage.ItemStorage.Remove(parent, true);

            parent.Location = location;
            parent.MapPosition = MapManager.WorldPosToTile(location); // tilePos;
            The.Map.TileMap[parent.MapPosition.X][parent.MapPosition.Y].AddEntity(parent);

        }*/

/*
        public bool LoadCarriedItem(Entity bearerOfItem, Entity item, List<PassengerOrCargoSlot> slots)
        {
            foreach (PassengerOrCargoSlot slotToLoad in slots)
            {
                slotToLoad.CargoSlot.TargetedByHauler = null;
            }

            if (item.Bulk + TotalStored <= TotalCapacity)
            {
                //StoredItems.Add(item);
                if (!Add(item, null))
                {
                    return false;
                }

                bearerOfItem.AgentStorage.ItemStorage.Remove(item, true);

               
                bearerOfItem.Locomotor.currentMaximumSpeedIsDirty = true;

                item.Item.EquippedBy = null;
                item.Item.OKToTakeThisItemFromCarrier = null;

                //item.OnBoard = Parent;

                CalculateTotalStored();
                Parent.Locomotor.currentMaximumSpeedIsDirty = true;


                float roomInSlot;
                float bulkStillToLoad = item.Bulk;
                //float bulkToLoadInThisSlot;

                // fill 'em up:
                foreach (PassengerOrCargoSlot slotToLoad in slots)
                {
                    roomInSlot = slotToLoad.CargoSlot.GetRemainingRoom();

                    if (roomInSlot >= bulkStillToLoad)
                    {
                        slotToLoad.CargoSlot.BulkCarried += bulkStillToLoad;

                        break;
                    }
                    else
                    {
                        slotToLoad.CargoSlot.BulkCarried += roomInSlot;

                        bulkStillToLoad -= roomInSlot;
                    }
                }

                             

                UpdateCargoSlotsWithAttachedModels();


                return true;
            }
            else
            {
                return false;
            }
        }*/


        #region IExit Methods
        
        private bool preventRecursion = false;
        public void GetDebugMarkers()
        {
            ExitAndEntrance.GetDebugMarkers(this, ref preventRecursion);
        }

        public bool IsDoorAvailable()
        {
            return false;
        }
         public ExitDoor ReserveDoorForEntryOrExit(Entity entity, bool exiting)
        {
            return ExitDoor.NoneNeeded;
        }
        public void UseDoor(Entity entity, ExitDoor door, bool exiting)
        {
        }
        public void UnreserveDoor(ExitDoor door)
        {
        }
        public void SetRallyPoint(Vector3 pos, ExitDoor door)
        {
        }
        public Vector3 GetRallyPoint(ExitDoor door = ExitDoor.NextAvailable)
        {
            return Parent.PlaySiteLocation;
        }
        public bool GetNaturalRallyPoint(ref Vector3 rallyPoint, bool offset = true)
        {
            rallyPoint.X = rallyPoint.X = rallyPoint.X = 0f;
            return false;
        }

        public bool GetDoorPosition(ref Vector3 position, bool exiting, out ExitDoor doorThatWasUsed, ExitDoor door = ExitDoor.NextAvailable)
        {
            // TODO: use the passenger's slot...
            PassengerOrCargoSlotType passengerSlotType =
                ((VehicleContainerType)Parent.EntityType.ContainerType).PassengerOrCargoSlotTypes.FirstOrDefault(s => s.PassengerSlotType != null && s.PassengerSlotType.IsDriversSeat == false);

            // no queuing..?
            position = ExitAndEntrance.GetDoorPosition(this, 
                /*passengerSlotType.Entrance.ExitDoor,*/ passengerSlotType.Entrance.Offset ?? Vector2.Zero);

            doorThatWasUsed = passengerSlotType.Entrance.ExitDoor;

            return true;

           // return Garrison.GetDoorPosition(parent, parent.EntityType.ContainerType.VehicleContainerType.Doors, ref position, ref door, out doorThatWasUsed);
        }


        public Vector3 ComputeAccessPoint()
        {
            //since this is a vehicle, it should be one of the several doors, but which one? the dirver door or the main hatch?
            return GetRallyPoint();
        }


        public bool IsClearToApproach(Entity docker)
        {
            return IsDoorAvailable();
        }

        public bool ReserveApproachPosition(ref Entity docker, ref Vector3 position, out int index)
        {
            ExitDoor door = ReserveDoorForEntryOrExit(docker, false);

            if (door != ExitDoor.NoneAvailable) //valid door reserved
            {
                index = (int)door;
                ExitDoor doorThatWasUsed;
                GetDoorPosition(ref position, false, out doorThatWasUsed, door);
            }

            index = -1;
            return false;
        }


        public bool AdvanceApproachPosition(ref Entity docker, ref Vector3 position, out int index)
        {
            /// Give Entity the next Queue point to move to, and record that that point is taken.
            //In impl, this will consult with IExit ReserveDoor()

            index = -1;
            return false; //TODO impl
        }

        public bool IsClearToEnter(Entity docker)
        {
            return IsDoorAvailable();
        }

        public bool IsClearToAdvance(Entity docker, int dockerIndex)
        {
            ///this fn should examine the area between the door at dockerIndex and its rally point
            ///to make sure it is clear of interlopers, rapscallions or other ne'erdowells

            return true;//for now, everyone is clear to advance all the time, oops
        }

        public void GetEnterPosition(ref Entity docker, ref Vector3 position)
        {
            /// Give Entity the point that is the start of his docking path
            /// Returning null means there is none free
            /// All functions take docker as arg so we could have multiple docks on a building.  
            /// Docker is not assumed, it is recorded and checked.    

            ///In impl, this will consult with IExit.GetRallyPoint()
        }

        public void GetDockPosition(ref Entity docker, ref Vector3 position)
        {
            /// Give Entity the middle point of the dock process where the action() happens 	
            ///In impl, this will consult with IExit.GetDoorPosition()
        }

        public void GetDeparturePosition(ref Entity docker, ref Vector3 position)
        {
            /// Give Entity the point to move to when he is done  
            ///In impl, this will consult with IExit.GetRallyPoint()
        }

        public void OnApproachRallyReached(ref Entity docker)
        {
            /// Entity has reached the Enter rally Point.
        }

        public void OnDockReached(ref Entity docker)
        {
            /// Entity has reached the Dock point        
        }

        public void OnDepartureRallyReached(ref Entity docker)
        {
            /// Entity has reached the rally point on his way out.  He is no longer busy
        }

        public bool Action(ref Entity docker)
        {
            /// Perform our specific action on visiting entity.
            /// examples, fill his basket with apples, his tank with fuel, his belly with food...
            /// Returning FALSE means there is nothing for you to do so entity should leave
            return true;
        }


        public void CancelDock(ref Entity docker)
        {
            /// Clear entity from any reserved points, and if entity was the reason we were Busy, we aren't anymore.
        }

        public bool DockOpen
        {
            /// Is the dock open to accepting dockers?
            get
            {
                return true;
            }
        }

        public bool IsAllowedtoDock(ref Entity dockingEntity)
        {
            ///can entity dock here?
            ///this should be a combination of DockOpen, and also applying an entityFilter on dockingEntity
            ///this filter should be defined in COntainerTYpe, but might be hard coded in the interim
            return true;
        }

        public bool UsesRallyPointAfterUndock
        {
            /// A minority of docks want to give you a final command to their rally point. 
            /// this should refer to data in ContainType in many cases
            get
            {
                return true; // for now
            }
        }

        #endregion

        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.VehicleSeatState); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            Aircraft = (Aircraft)sn.DoISnapshot(Aircraft);
            sn.DoFloat(CargoCapacityTakenUpByPassengers);
            if (sn.mode != Snapshotter.Mode.Load)
            {
                drivenByID = DrivenBy?.EntityID ?? default;
                snapshotPassengers = Passengers.Select(passenger => passenger.EntityID).ToList();
            }
            drivenByID = sn.DoEntityID(drivenByID);
            snapshotPassengers = sn.DoList(snapshotPassengers);
            PassengerItinerary = sn.DoList(PassengerItinerary);
            sn.DoFloat(Pitch);
            sn.DoFloat(Roll);
            Slots = sn.DoList(Slots);
            sn.DoInt32(WaitingForPassengers);

            this.storage = (ItemStorage)sn.DoISnapshot(storage);
            this.replenishItems = (ReplenishItems)sn.DoISnapshot(replenishItems);
            this.residence = (Residence)sn.DoISnapshot(residence);
            this.garrison = (Garrison)sn.DoISnapshot(garrison);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (storage != null)
            {
                storage.LoadPostProcess(sn);
            }

            DrivenBy = Entity.FindByID(drivenByID);
            Passengers = snapshotPassengers?
                .Select(Entity.FindByID)
                .Where(passenger => passenger != null)
                .ToList() ?? new List<Entity>();

            if (PassengerItinerary != null)
            {
                foreach (Passenger passenger in PassengerItinerary)
                {
                    passenger.LoadPostProcess(sn);
                }
            }

            if (Slots != null)
            {
                foreach (PassengerOrCargoSlot slot in Slots)
                {
                    slot.LoadPostProcess(sn);
                }
                RecalculateCargoCapacity();
            }

            foreach (Entity passenger in Passengers)
            {
                passenger.PassengerInVehicle = Parent.EntityID;
            }

            if (residence != null)
            {
                residence.LoadPostProcess(sn);
            }

            if (garrison != null)
            {
                garrison.LoadPostProcess(sn);
            }

            if (replenishItems != null)
            {
                replenishItems.LoadPostProcess(sn);
            }
        }



        #endregion
    }
}
