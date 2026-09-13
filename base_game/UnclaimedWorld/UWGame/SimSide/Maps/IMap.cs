using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public interface IMap : ILookUp<IMap, IMapID>, ISnapshot
{
	TileLayer Map { get; }

	bool IsReady { get; }

	List<Dependence> Children { get; set; }

	bool DoCycle();

	IMap GetCurrent();
}
