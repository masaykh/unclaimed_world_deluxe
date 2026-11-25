using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Locomotors
{
    /// <summary>
    /// Locomotor type for things like the skimmer, blimps, drones, weird aliens...
    /// Skimmer ducted fan animation, banking etc. are in the Aircraft class.
    /// </summary>
    public class VTOLLocomotorType
    {
        /// <summary>
        /// Maximum horizontal air speed in pixels per second.
        /// </summary>
        public float MaxAirSpeed;

        public float MaxVerticalMoveSpeed;

        public float CruiseAltitude = 200f;

        [XmlIgnore]
        public float EstimatedTakeOffLandingTime => CruiseAltitude <= 0f
            ? 0f
            : MaxVerticalMoveSpeed <= 0f ? float.PositiveInfinity : 1.5f * CruiseAltitude / MaxVerticalMoveSpeed;
    }
}
