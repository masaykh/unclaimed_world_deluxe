using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Maps.MapEditor;

public class Tree
{
	public AgeGroup? AgeGroup;

	public InSeason InSeason;

	public float? AgeInYears;

	public float ShapeFactor;

	public int Flavour;

	public bool ShouldSerializeAgeInYears()
	{
		return AgeInYears.HasValue;
	}

	public bool ShouldSerializeAgeGroup()
	{
		return AgeGroup.HasValue;
	}

	public void SetTreeComponentPreInit(UWGame.SimSide.Trees.Tree treeComponent)
	{
		if (AgeGroup.HasValue)
		{
			treeComponent.SetAgePreInit(AgeGroup.Value);
		}
		else if (AgeInYears.HasValue)
		{
			treeComponent.SetAgePreInit(AgeInYears.Value);
		}
		else
		{
			treeComponent.SetAgePreInit();
		}
		treeComponent.Flavour = Flavour;
		treeComponent.InSeason = InSeason;
		treeComponent.ShapeFactor = ShapeFactor;
	}
}
