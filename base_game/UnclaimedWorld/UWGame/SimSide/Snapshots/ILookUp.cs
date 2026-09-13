namespace UWGame.SimSide.Snapshots;

public interface ILookUp<T, Id> where T : ILookUp<T, Id> where Id : struct
{
	Id ID { get; }

	int LoadPostProcessOrder { get; }

	Id GetUniqueID();

	void AddToLookup();

	void RemoveIDEntry();

	void ResetIDCounter();

	void CreateLookupCollection();

	void SetInvalid();
}
