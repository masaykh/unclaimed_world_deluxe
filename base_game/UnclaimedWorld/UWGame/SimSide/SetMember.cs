namespace UWGame.SimSide;

public class SetMember
{
	private SetMember representative;

	private SetMember next;

	private int listLength = 1;

	public SetMember()
	{
		representative = this;
	}

	public SetMember FindSet()
	{
		return representative;
	}

	public void Union(SetMember setToAdd)
	{
		Union(this, setToAdd);
	}

	private void Union(SetMember set1, SetMember set2)
	{
		set1.representative.listLength = set1.representative.listLength + set2.representative.listLength;
		SetMember setMember = set1;
		while (setMember.next != null)
		{
			setMember = setMember.next;
		}
		SetMember setMember2 = set2.representative;
		do
		{
			setMember.next = setMember2;
			setMember2.representative = setMember.representative;
			setMember = setMember2;
			setMember2 = setMember2.next;
		}
		while (setMember2 != null);
	}

	private void Union(SetMember representative, SetMember set1, SetMember set2)
	{
		representative.listLength = set1.representative.listLength + set2.representative.listLength;
		SetMember setMember = set1.representative;
		while (setMember.next != null)
		{
			setMember.representative = representative;
			setMember = setMember.next;
		}
		SetMember setMember2 = set2.representative;
		do
		{
			setMember.next = setMember2;
			setMember2.representative = representative;
			setMember = setMember2;
			setMember2 = setMember2.next;
		}
		while (setMember2 != null);
	}
}
