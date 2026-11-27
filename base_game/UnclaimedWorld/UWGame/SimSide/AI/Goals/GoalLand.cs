using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Maps;
using Xclna.Xna.Animation;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
    public class GoalLand : Goal
    {
        public Entity Aircraft;
        private EntityID aircraftID;
        private VehicleContainer vehicleComponent;

        private Vector3 destinationPoint;

        public GoalLand(Entity owner)
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

            version = sn.DoVersion(Snapshotter.Version.VehicleSnapshotReferences); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            // BASE: as in GoalTakeoff.DoSnapshot - queued behind the takeoff and the flight, this goal
            // has no Aircraft until Activate, and saving it threw NullReferenceException (Lars's
            // flight audit, 2026-11-17). Not started, it saves Invalid, which loads as no aircraft.
            if (sn.mode != Snapshotter.Mode.Load)
            {
                aircraftID = Aircraft != null ? Aircraft.EntityID : EntityID.Invalid;
            }

            aircraftID = sn.DoEntityID(aircraftID);
            this.destinationPoint = sn.DoVector3(destinationPoint);

            sn.Ignore(Aircraft);
            sn.Ignore(vehicleComponent);

            return this;
        }

        public GoalLand()
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
            if (!entity.GetDrivenVehicle(out Aircraft))
            {
                Status = Goals.Status.Failed;
                return;
            }
            vehicleComponent = Aircraft.Vehicle;

            // NEW (issue #33): one last look at the reserved spot before coming down on it. If it
            // went bad after GoalFlyToPosition's last check, fail while still in the air: GoalThink
            // chooses again, and an idle pilot flies home (GoalTakeFive.TryFlyHome). A landing with
            // no reservation here - a put-down where nothing better was found - goes ahead.
            if (StoppingSpot.TryGetReserved(entity, Aircraft, out Vector3 heldSpot)
                && Aircraft.MapPosition.HasValue && MapManager.WorldPosToTile(heldSpot) == Aircraft.MapPosition.Value
                && !StoppingSpot.IsSuitable(entity, Aircraft, heldSpot))
            {
                StoppingSpot.Release(entity, Aircraft);
                StoppingSpot.SayNoSpot(entity);
                Status = Goals.Status.Failed;
                return;
            }

            // BASE: only a finished takeoff or landing zeroes the climb speed. After a task cancelled
            // during the climb this landing began at the takeoff's upward speed, with
            // MaxVerticalAcceleration (30 px/s per second) to turn it round - the skimmer kept climbing for a
            // while after it was told to come down (Lars's flight audit, 2026-11-17). Start from a
            // standstill instead. A speed, not a position: the skimmer stops climbing, it does not jump.
            Aircraft.Locomotor.VTOLLocomotor.VerticalSpeed = 0f;

            Status = Status.Active;
            destinationPoint = new Vector3(MapManager.tileSize * (0.5f + entity.MapPosition.Value.X),
                   MapManager.tileSize * (0.5f + entity.MapPosition.Value.Y), 0f);

            /*       if (UWGame.SimSide.Instance.map.TileMap[entity.MapPosition.X, entity.MapPosition.Y].ContainsItem(meal))
                   {
                       Status = Status.Active;
                       secondsToEat = Common.RandomSpread(Globals.Instance.Random, baseSecondsToEat, secondsToEatSpread);

                   }
                   else
                   {
                       // cannot get the item. Fail.
                       Status = Status.Failed;
                   }*/

        }
        public override bool IsSame(Job job)
        {
            return false;
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()           
            /*    ActivateIfInactive();

                if (Status == Goals.Status.Active)
                {*/

            float desiredMoveSpeed = -Aircraft.EntityType.LocomotorType.VTOLLocomotorType.MaxVerticalMoveSpeed;

            if (GoalTakeoff.UpdateAircraftUpDown(desiredMoveSpeed, elapsed, entity, Aircraft, vehicleComponent, destinationPoint))
            {
                Aircraft.Locomotor.MoveSpeed = 0f;
                //  Aircraft.Renderable.RenderAsModel.RotationSpeed = 0f;
                Aircraft.Locomotor.VTOLLocomotor.VerticalSpeed = 0f;

                // Put the aircraft on the ground; the pilot is contained by it.
                Aircraft.Location = new Vector3(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y, 0f);

                // stop engines. don't wait for them to stop before getting out...
                Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_left", true);
                Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_right", true);

                Aircraft.Renderable.StartAdditionalAnimation("propeller_right_stop", Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal); //TODO DECOUPLE
                Aircraft.Renderable.StartAdditionalAnimation("propeller_left_stop", Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal); //TODO DECOUPLE

                Status = Status.Completed;
            }
            else
            {
                // still in the air, kick up some dust:
                GoalTakeoff.AddSkimmerDust(Aircraft);

                /* float dustIntensity = (GoalTakeoff.TargetAltitude - Aircraft.Location.Z) / GoalTakeoff.TargetAltitude;
                 dustIntensity *= UWGame.SimSide.Instance.Map.TileMap[Aircraft.MapPosition.X, Aircraft.MapPosition.Y].GetDustFactor();
                 UWGame.SimSide.Instance.particleManager.AddDust(Aircraft, Aircraft.Location, dustIntensity, GameConstants.SkimmerDustScale);*/
            }
            /* }

             ExitIfFailedOrCompleted();

             return Status;*/

        }

    }
}
