using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items;

public interface IComposite : ILookUp<IComposite, CompositeID>
{
	List<Entity> Parts { get; }

	void SetPart(Entity newPart);

	void RemovePart(Entity part, bool setPartOfToNull = true);

	void SetBrokenPart();

	void SetConditionDirty();

	IComposite GetRoot();
}
