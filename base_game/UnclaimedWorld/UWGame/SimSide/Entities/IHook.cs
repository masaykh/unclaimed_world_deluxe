namespace UWGame.SimSide.Entities;

public interface IHook
{
	string TypeKey { get; }

	int ExecutionOrder { get; }
}
