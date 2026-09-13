namespace UWGame;

public class Pair<T, U>
{
	public T First { get; set; }

	public U Second { get; set; }

	public Pair()
	{
	}

	public Pair(T first, U second)
	{
		First = first;
		Second = second;
	}

	public override bool Equals(object o)
	{
		Pair<T, U> pair = o as Pair<T, U>;
		if (First.Equals(pair.First))
		{
			return Second.Equals(pair.Second);
		}
		return false;
	}
}
