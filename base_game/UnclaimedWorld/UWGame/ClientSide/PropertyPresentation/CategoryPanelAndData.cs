using WindowSystem;

namespace UWGame.ClientSide.PropertyPresentation;

public class CategoryPanelAndData
{
	public CollapsablePanel Panel;

	public Grid Grid;

	public PresentationTypeCategory Category;

	public CategoryPanelAndData(CollapsablePanel panel, Grid grid, PresentationTypeCategory category)
	{
		Panel = panel;
		Grid = grid;
		Category = category;
		int num = 0;
		PresentationNode[] nodes = Category.Nodes;
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i].Index = num;
			num++;
		}
	}
}
