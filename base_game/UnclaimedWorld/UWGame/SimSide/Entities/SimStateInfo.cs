using UWGame.SimSide.Collisions;

namespace UWGame.SimSide.Entities;

/// <summary>
/// New footprint.
/// This cannot be in RenderableType, since it is Sim stuff
/// 
/// </summary>
public class SimStateInfo : IStateInfo
{

    public BitMask64 Conditions { get; set; }

    public BitMask64 Forbiddens { get; set; }

    // could also contain other sim stuff

    /// <summary>
    /// the geo layout to use (Sim side!)
    /// </summary>
    public GeometryLayoutType GeometryLayoutType;
}
