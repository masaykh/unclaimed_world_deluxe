namespace UWGame.ClientSide.Interface.Overlays;

public enum EntityGrouping
{
	Structures,
	Animals,
	ColonyMembers,
	Interest,
	// MOD: HudMod's item layers. Appended, so the values a save already holds keep their meaning.
	Tools,
	Weapons,
	PreparedFood,
	Ingredients,
	Materials
}
