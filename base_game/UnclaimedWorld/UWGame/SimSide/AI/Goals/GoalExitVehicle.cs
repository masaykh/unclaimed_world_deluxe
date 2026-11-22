using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using UWGame.ClientSide;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;//TODO DECOUPLE
using UWGame.SimSide.Systems;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Maps;
using Xclna.Xna.Animation;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// exit goal for both driver and passengers.
    /// </summary>
    internal class GoalExitVehicle : CompositeGoal
    {
        EntityID vehicle;
        // Vehicle vehicleComponent;

        //  DrivingVehicle drivingVehicleTrigger;
        public GoalExitVehicle(Entity owner, EntityID vehicle) //, DrivingVehicle trigger)
            : base(owner)
        {
            this.vehicle = vehicle;

            //    this.drivingVehicleTrigger = trigger;
        }


        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.vehicle = (EntityID)sn.DoEnum(vehicle);

            return this;
        }

        public GoalExitVehicle()
        {
        }

        // PORT: how far a pilot looks for ground to land on before he lands where he is.
        public const int LandingSearchRadius = 12;

        public static bool IsAirborne(Entity vehicleEntity)
        {
            return vehicleEntity.Vehicle?.Aircraft != null && vehicleEntity.PlaySiteLocation.Z > 1f;
        }

        internal static bool CanStandOn(Point tile) // PORT: shared with GoalTakeFive.FindHome
        {
            if (tile.X < 0 || tile.Y < 0 || tile.X >= The.Map.mapTileWidth || tile.Y >= The.Map.mapTileHeight)
            {
                return false;
            }
            Point centre = MapManager.TileToUpperLeftSubtile(tile) + new Point(1, 1);
            return !The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], centre);
        }

        /// <summary>
        /// PORT: the nearest tile, ring by ring, that canStandOn accepts; the tile itself first. Within
        /// a ring the closest by straight-line distance wins, ties in scan order (rows, then columns),
        /// so a replay picks the same one. Null when there is none within maxRadius.
        /// </summary>
        public static Point? FindLandingTile(Point from, Func<Point, bool> canStandOn, int maxRadius)
        {
            if (canStandOn(from))
            {
                return from;
            }
            for (int r = 1; r <= maxRadius; r++)
            {
                Point? best = null;
                int bestDistance = int.MaxValue;
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r)
                        {
                            continue;
                        }
                        Point p = new Point(from.X + dx, from.Y + dy);
                        int distance = dx * dx + dy * dy;
                        if (distance < bestDistance && canStandOn(p))
                        {
                            best = p;
                            bestDistance = distance;
                        }
                    }
                }
                if (best.HasValue)
                {
                    return best;
                }
            }
            return null;
        }

        protected override void Activate()
        {
            Status = Status.Active;
            Entity vehicleEntity;

            if (!entityIntelligence.GetEntitySeenDirectly(vehicle, out vehicleEntity))
            {
                // something wrong has happened!
                Status = Goals.Status.Failed;
                return;
            }

            if (entity.DrivingVehicle == vehicle || entity.PassengerInVehicle == vehicle)
            {
                // PORT FIX. Nothing here asked whether the aircraft was on the ground: a task removed
                // during the climb, or a pilot told to walk somewhere, put him out at the exit point
                // in mid-air or over the river (Kastuk, 2026-11-14). The driver now brings it down
                // first - where he is if he could stand there, else on the nearest tile he could -
                // and a passenger waits for that rather than step out.
                if (IsAirborne(vehicleEntity))
                {
                    if (entity.DrivingVehicle != vehicle)
                    {
                        Status = Status.Failed;
                        return;
                    }
                    Point here = vehicleEntity.MapPosition.Value;
                    Point? landing = FindLandingTile(here, CanStandOn, LandingSearchRadius);
                    if (landing.HasValue && landing.Value != here)
                    {
                        AddSubgoal(new GoalFlyToPosition(entity, landing.Value));
                    }
                    AddSubgoal(new GoalLand(entity));
                }

                // TODO: change this:
                AddSubgoal(new GoalWait(entity, 3, AnimAction.Containing, AnimModifier.Crew)); //, vehicle));


                //AddSubgoal(new GoalStartAnimationAndWaitForEnd(entity, vehicleEntity.EntityID, "driver_entry", Playback.Forwards, StartingPoint.Current, //TODO DECOUPLE
                //     BlendMode.Additive, false));

                /*  AnimationController anim = vehicle.Renderable.RenderAsModel.TryStartAnimation("driver_entry", Playback.Forwards, StartingPoint.Current, RenderAsModel.Mode.Additive, 1f);
                  timeToWait = anim.Duration - anim.ElapsedTime;
                  StartDelay(anim.Duration - anim.ElapsedTime);*/
            }
            else
            {

                Status = Status.Completed;
            }


        }



        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
            /* ActivateIfInactive();

             if (Status == Goals.Status.Active)
             {*/
            if (!IsDelayed(elapsed))
            {
                Status = ProcessSubgoals(elapsed);

                if (Status == Status.Completed)
                {
                    Entity vehicleEntity;


                    // code this defensively, so an additional GoalExitVehicle doesn't throw an error.
                    if (entity.DrivingVehicle != null || entity.PassengerInVehicle != null)
                    {
                        if (!entityIntelligence.GetEntitySeenDirectly(vehicle, out vehicleEntity))
                        {
                            // something wrong has happened!
                            Status = Goals.Status.Failed;

                            // ExitIfFailedOrCompleted();

                            return; // Status;
                        }


                        bool isDriving = entity.DrivingVehicle == vehicle;

                        Vector3 exitLocation, seatLocation;
                        VehicleContainer vehicleComponent = vehicleEntity.Vehicle;

                        PassengerOrCargoSlot place = vehicleComponent.GetEntityPlaceInVehicle(entity);
                        // Place? place = vehicleComponent.GetEntityPlaceInVehicle(entity);

                        place.GetEntryPoints(out exitLocation, out seatLocation);
                        //vehicleComponent.GetEntryPoint(place, out exitLocation, out seatLocation); 

                        if (vehicleComponent.Remove(entity))
                        {
                            if (isDriving)
                            {
                                entity.DeleteTriggerOfType(GameData.Instance.AllTriggerTypes["drivenVehicle"]);
                            }
                            entity.Location = exitLocation;

                            entity.FacingNormal = vehicleEntity.FacingNormal;
                            entity.Rotation = vehicleEntity.Rotation;

                            AttachPoint attachPoint;
                            if (isDriving)
                            {
                                // attachPoint = vehicle.EntityType.Renderable.RenderAsModelType.ModelData.DriverAttachPoint;
                                attachPoint = vehicleEntity.Renderable.RenderAsModel.ModelData.DriverAttachor;
                            }
                            else
                            {
                                // attachPoint = vehicle.EntityType.Renderable.RenderAsModelType.ModelData.GetAttachPointFromKeyName(place.PassengerOrCargoSlotType.AttachPointName); //.Value);
                                attachPoint = vehicleEntity.Renderable.RenderAsModel.ModelData.GetAttachPointFromKeyName(place.PassengerOrCargoSlotType.AttachPointName); //.Value);
                            }

                            if (attachPoint != null) // !string.IsNullOrEmpty(vehicle.EntityType.Renderable.RenderAsModelType.ModelData.DriverAttachPoint.AttachBoneName))
                            {
                                vehicleEntity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.DeattachObject(entity.Renderable.RenderAsModel, attachPoint.BoneName);
                            }

                        }
                        else
                        {
                            Status = Status.Failed;
                            return;
                        }

                        // tell all passengers to Get Off:
                        if (isDriving && vehicleComponent.Passengers.Count > 0)
                        {
                            for (int i = 0; i < vehicleComponent.Passengers.Count; i++)
                            {

                                vehicleComponent.TellPassengerToGetOff(entity, vehicleComponent.Passengers[i]);

                            }
                        }

                        // close the door...
                        vehicleEntity.Renderable.StartAdditionalAnimation("driver_entry", Playback.Backwards, StartingPoint.Current, BlendMode.Normal);


                        Status = Status.Completed;
                        // it is OK if we are already out of the vehicle...


                    }
                }
            }
            /* }

             ExitIfFailedOrCompleted();

             return Status;*/

        }
    }
}
