namespace UWGame.SimSide.Entities;

public interface IHasCategory<T> where T : ICategoryType
{
	string Name { get; set; }

	T Category { get; set; }
}
