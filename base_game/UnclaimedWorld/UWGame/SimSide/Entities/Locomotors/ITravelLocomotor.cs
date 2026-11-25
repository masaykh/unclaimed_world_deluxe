namespace UWGame.SimSide.Entities.Locomotors
{
    public interface ITravelLocomotor
    {
        float NormalSpeed { get; }

        float GetTargetSpeed();
    }
}
