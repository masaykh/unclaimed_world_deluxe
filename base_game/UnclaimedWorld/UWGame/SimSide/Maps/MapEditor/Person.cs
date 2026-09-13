using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Maps.MapEditor;

public class Person
{
	public string FirstName;

	public string LastName;

	public string PersonalityType;

	public string Portrait;

	public bool SimulateJoinedExpeditionNow;

	public void FillEntity(Entity entity)
	{
		UWGame.SimSide.Entities.Person personEntity = entity.PersonEntity;
		if (!string.IsNullOrEmpty(PersonalityType))
		{
			personEntity.Personality = new Personality(entity, GameData.Instance.AllPersonalityTypes[PersonalityType]);
		}
		if (!string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName))
		{
			entity.Intelligence.SetName(FirstName, LastName);
		}
	}
}
