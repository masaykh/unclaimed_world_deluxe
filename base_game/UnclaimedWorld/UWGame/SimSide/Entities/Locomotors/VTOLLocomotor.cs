using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Locomotors
{
    public class VTOLLocomotor : ISnapshot, ITravelLocomotor
    {
        public Locomotor Parent;

        public SteeringType SteeringType;

        public float VerticalSpeed;

        public float MaxAirSpeed => Parent.Parent.EntityType.LocomotorType.VTOLLocomotorType.MaxAirSpeed;

        public float NormalSpeed => MaxAirSpeed;

        public float GetTargetSpeed() => MaxAirSpeed;

        public VTOLLocomotor()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public VTOLLocomotor(Locomotor parent)
        {
            Parent = parent;
        }

        public bool IsSnapshotted { get; set; }

        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            return sn.DoVersion(Snapshotter.Version.Original);
        }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            SteeringType = sn.DoEnum(SteeringType);
            VerticalSpeed = sn.DoFloat(VerticalSpeed);
            sn.Ignore(Parent);
            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);
        }
    }
}
