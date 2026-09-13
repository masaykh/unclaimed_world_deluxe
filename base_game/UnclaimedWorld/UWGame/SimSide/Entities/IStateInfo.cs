namespace UWGame.SimSide.Entities;

public interface IStateInfo
{
	BitMask64 Conditions { get; }

	BitMask64 Forbiddens { get; }
}
