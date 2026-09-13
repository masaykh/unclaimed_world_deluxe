using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Resources;

public interface IResourceItem : ILookUp<IResourceItem, ResourceItemID>, ISnapshot
{
	ResourceContainer Container { get; }

	ProcessJob AssignedToJob { get; set; }

	void Destroy();
}
