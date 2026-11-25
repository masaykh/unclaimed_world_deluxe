using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
    public class GoalTakeoff : CompositeGoal
    {
        public Entity Aircraft;
        private EntityID aircraftID;
        VehicleContainer vehicleComponent;

        /*  public Point Destination
          {
              get { return destination; }
              set
              {
                  destination = value;
                  destinationPoint = new Vector2(UWGame.SimSide.Instance.map.tileWidth * (0.5f + destination.X),
                      UWGame.SimSide.Instance.map.tileHeight * (0.5f + destination.Y));
              }

          }
          private Point destination;*/
        private Vector3 destinationPoint;

        /// <summary>
        /// The "close enough" limit, if the tank is inside this many pixel 
        /// to it's destination it's considered at it's destination
        /// // Remember: distance is squared!
        /// </summary>
        const float atDestinationLimit = 16f;

        // TEASER XXX:
        // public const float TargetAltitude = 300f; //200f;



        public GoalTakeoff(Entity owner)
            : base(owner)
        {

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.GoalWaitAnimationTarget);
            return version;
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            // BASE: GoalMoveToPosition queues takeoff, flight and landing together, every goal is saved,
            // and Aircraft is set only in Activate - so a save while the pilot walked to the skimmer
            // or waited threw NullReferenceException here (Lars's flight audit, 2026-11-17). A goal
            // that has not started saves no aircraft: Invalid, not the default 0, which is a real
            // entity that LoadPostProcess would find. Activate looks the aircraft up again anyway.
            if (sn.mode != Snapshotter.Mode.Load)
            {
                aircraftID = Aircraft != null ? Aircraft.EntityID : EntityID.Invalid;
            }

            aircraftID = sn.DoEntityID(aircraftID);
            this.destinationPoint = sn.DoVector3(destinationPoint);
            if (version == Snapshotter.Version.VehicleAnimationRequests)
                sn.DoBool(false);

            sn.Ignore(Aircraft);
            sn.Ignore(vehicleComponent);

            return this;
        }

        public GoalTakeoff()
        {
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);
            base.LoadPostProcess(sn);

            Aircraft = Entity.FindByID(aircraftID);
            vehicleComponent = Aircraft?.Vehicle;
        }



        protected override void Activate()
        {
            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            if (!entity.GetDrivenVehicle(out Aircraft))
            {
                Status = Goals.Status.Failed;
                return;
            }
            vehicleComponent = Aircraft.Vehicle;

            // NEW (issue #33): leaving the spot it stood on frees it for the others. Only that one -
            // when the flight's landing spot was found before the takeoff, the lock already moved there.
            if (StoppingSpot.TryGetReserved(entity, Aircraft, out Vector3 heldSpot)
                && Aircraft.MapPosition.HasValue && Maps.MapManager.WorldPosToTile(heldSpot) == Aircraft.MapPosition.Value)
            {
                StoppingSpot.Release(entity, Aircraft);
            }

            Status = Status.Active;
            /*
            destinationPoint = new Vector3(MapManager.tileSize * (0.5f + entity.MapPosition.X),
                    MapManager.tileSize * (0.5f + entity.MapPosition.Y), TargetAltitude);
            */ // NEW: use the aircraft position instead of the pilot's:
            destinationPoint = new Vector3(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y, ((VehicleContainerType)Aircraft.EntityType.ContainerType).Aircraft.CruiseAltitude);


            /**
            Aircraft.Renderable.RenderAsModel.TryStartAnimation("driver_entry", Playback.Backwards, StartingPoint.Current, RenderAsModel.Mode.Additive);
            Aircraft.Renderable.RenderAsModel.TryStartAnimation("passenger_entry", Playback.Backwards, StartingPoint.Current, RenderAsModel.Mode.Additive);
            Aircraft.Renderable.RenderAsModel.TryStartAnimation("open_for_loading", Playback.Backwards, StartingPoint.Current, RenderAsModel.Mode.Additive);
            */

            /*  AddSubgoal(new GoalStartAnimationAndWaitForEnd(entity, Aircraft, "propeller_right_start", Playback.Forwards, StartingPoint.Current,
                   RenderAsModel.Mode.Additive, false));
              */


            // PORT FIX. Nothing finishes a flight that is cut short - GoalFlyToPosition has no
            // cleanup - so a pilot whose task was removed mid-air left the skimmer hanging there, and
            // his next flight began here with the whole ground sequence: fifteen seconds of engine and
            // door waits, in the air, before it moved (Kastuk, 2026-11-14: "pausing during flight
            // maneuvers too, wait some, then switching to another task"). Already in the air, the
            // engines are running and the doors shut: climb at once, from a standstill rather than
            // the speed the cut-short flight left behind.
            if (StartsInTheAir(Aircraft.PlaySiteLocation.Z))
            {
                Aircraft.Locomotor.MoveSpeed = 0f;
                // BASE: the vertical speed too. A landing cut short left it descending, and this climb
                // began at that downward speed, with 30 px/s per second to turn it round (Lars's flight audit,
                // 2026-11-17). A speed, not a position: nothing jumps.
                vehicleComponent.Aircraft.VerticalSpeed = 0f;
                return;
            }

            // Request both engine starts and both door closures concurrently. The three-second
            // simulation delay is fixed; takeoff does not depend on client animation completion.
            AddSubgoal(new GoalWait(entity, new List<AnimConditions>(TakeoffAnimationConditions()), 3d, animationTargetEntity: Aircraft));



            //// close all doors and start the engines:
            //AddSubgoal(new GoalStartAnimationAndWaitForEnd(entity, new List<AnimationToRun>
            //{
            //    new AnimationToRun(){ 
            //         targetEntity = Aircraft,
            //         animationKey = "propeller_right_start", 
            //        AnimationStates = new AnimState[] { AnimState.Pre, AnimState.Right, AnimState.Slow },

            //         playback = Playback.Forwards, //TODO DECOUPLE
            //         startingPoint = StartingPoint.FromBeginning,
            //         mode = BlendMode.Additive, runToEndIfInterrupted = false
            //    },
            //    new AnimationToRun(){ 
            //         targetEntity = Aircraft,
            //         animationKey = "propeller_left_start", 
            //        AnimationStates = new AnimState[] { AnimState.Pre, AnimState.Left, AnimState.Slow },
            //         playback = Playback.Forwards, 
            //         startingPoint = StartingPoint.FromBeginning,
            //         mode = BlendMode.Additive, runToEndIfInterrupted = false
            //    },
            //    new AnimationToRun(){ 
            //         targetEntity = Aircraft,
            //         animationKey = "driver_entry", 
            //        AnimationStates = new AnimState[] { AnimState.Containing, AnimState.Crew },
            //         playback = Playback.Backwards, //TODO not sure why this is backwards
            //         startingPoint = StartingPoint.Current,
            //         mode = BlendMode.Additive, runToEndIfInterrupted = false
            //    },
            //    new AnimationToRun(){ 
            //         targetEntity = Aircraft,
            //         animationKey = "passenger_entry", 
            //        AnimationStates = new AnimState[] { AnimState.Containing, AnimState.Passenger },
            //         playback = Playback.Backwards, //TODO not sure why this is backwards
            //         startingPoint = StartingPoint.Current,
            //         mode = BlendMode.Additive, runToEndIfInterrupted = false
            //    },
            //    new AnimationToRun(){ 
            //         targetEntity = Aircraft,
            //         animationKey = "open_for_loading", 
            //        AnimationStates = new AnimState[] { AnimState.Open },
            //         playback = Playback.Backwards, //TODO not sure why this is backwards
            //         startingPoint = StartingPoint.Current,
            //         mode = BlendMode.Additive, runToEndIfInterrupted = false
            //    }
            //}
            //));
        }

        private static IEnumerable<AnimConditions> TakeoffAnimationConditions()
        {
            yield return new AnimConditions { Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre, (int)AnimModifier.Right, (int)AnimModifier.Slow) };
            yield return new AnimConditions { Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre, (int)AnimModifier.Left, (int)AnimModifier.Slow) };
            yield return new AnimConditions { Action = AnimAction.Containing, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Crew) };
            yield return new AnimConditions { Action = AnimAction.Containing, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Passenger) };
        }

        // PORT: GoalLand puts an aircraft down at altitude 0; anything above a pixel is still flying.
        public static bool StartsInTheAir(float altitude)
        {
            return altitude > 1f;
        }

        public static bool UpdateAircraftUpDown(float desiredMoveSpeed, Microsoft.Xna.Framework.GameTime elapsed, Entity pilot, Entity aircraft, VehicleContainer vehicleComponent, Vector3 destinationPoint)
        {
            float elapsedTime = (float)elapsed.ElapsedGameTime.TotalSeconds;

            Vector2 planeAdjustment = Vector2.Zero;
            float previousMoveSpeed = vehicleComponent.Aircraft.VerticalSpeed;

            // do vertical accel/decel:
            double distToTarget = Vector3.Distance(destinationPoint, aircraft.PlaySiteLocation);
            if (distToTarget > 0)
            {
                VehicleContainerType vehicleContainerType = (VehicleContainerType)aircraft.EntityType.ContainerType;
                double speed = MathHelper.Clamp(desiredMoveSpeed,
                    previousMoveSpeed - vehicleContainerType.Aircraft.MaxVerticalAcceleration * elapsedTime,
                    previousMoveSpeed + vehicleContainerType.Aircraft.MaxVerticalAcceleration * elapsedTime);

                // decelerate:
                speed = (Math.Sign(speed)) * Math.Min(distToTarget / vehicleContainerType.Deceleration, Math.Abs(speed));

                vehicleComponent.Aircraft.VerticalSpeed = (float)speed;

            }

            // make a small adjustment to x and y pos if needed:
            float maxAdjustment = 10f * elapsedTime;
            planeAdjustment.X = MathHelper.Clamp(destinationPoint.X - aircraft.PlaySiteLocation.X, -maxAdjustment, maxAdjustment);
            planeAdjustment.Y = MathHelper.Clamp(destinationPoint.Y - aircraft.PlaySiteLocation.Y, -maxAdjustment, maxAdjustment);

            // move the aircraft:  
            ////pilot.Location = aircraft.Location + new Vector3(planeAdjustment, vehicleComponent.Aircraft.VerticalSpeed * elapsedTime);
            aircraft.Location = aircraft.Location + new Vector3(planeAdjustment, vehicleComponent.Aircraft.VerticalSpeed * elapsedTime);


            if (aircraft.Renderable.RenderAsModel.ModelData.HasAircraftDucts)
            {
                GoalFlyToPosition.SetLeftDuctAngle(aircraft, vehicleComponent, 0f, elapsedTime);
                GoalFlyToPosition.SetRightDuctAngle(aircraft, vehicleComponent, 0f, elapsedTime);

                // use anims instead...
                //GoalFlyToPosition.UpdatePropellers(Aircraft, elapsedTime, GoalFlyToPosition.PropDirection.Start);
            }

            if (Vector3.DistanceSquared(aircraft.PlaySiteLocation, destinationPoint)
                      < atDestinationLimit)
            {
                return true;     // target reached           
            }

            return false;

        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()           
            /*   ActivateIfInactive();

               if (Status == Goals.Status.Active)
               {*/
            Status = ProcessSubgoals(elapsed);

            if (Status == Status.Completed)
            {
                Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_left", false);
                Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_right", false);

                float desiredMoveSpeed = ((VehicleContainerType)Aircraft.EntityType.ContainerType).Aircraft.MaxVerticalMoveSpeed;


                if (UpdateAircraftUpDown(desiredMoveSpeed, elapsed, entity, Aircraft, vehicleComponent, destinationPoint))
                {
                    Aircraft.Locomotor.MoveSpeed = 0f;
                    vehicleComponent.Aircraft.VerticalSpeed = 0f;
                    Status = Status.Completed;
                }
                else
                {
                    AddSkimmerDust(Aircraft);

                    Status = Status.Active;
                }
            }
            /* }

             ExitIfFailedOrCompleted();

             return Status;*/
        }

        public static void AddSkimmerDust(Entity aircraft)
        {
            float altitudeForDust = ((VehicleContainerType)aircraft.EntityType.ContainerType).Aircraft.AltitudeForDust;
            float dustIntensity = Math.Max((altitudeForDust - aircraft.PlaySiteLocation.Z) / altitudeForDust, 0f);
            dustIntensity *= The.Map.GetTile(aircraft.MapPosition.Value).GetDustFactor();

            The.Client.ParticleManager.AddDust(aircraft, aircraft.PlaySiteLocation, dustIntensity, GameData.Instance.Constants.SkimmerDustScale);
        }
    }
}
