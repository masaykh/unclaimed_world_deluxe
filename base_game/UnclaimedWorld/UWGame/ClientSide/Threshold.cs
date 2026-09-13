using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.ClientSide;

public class Threshold : IEdge
{
	public string Term;

	public string TermTooltip;

	public string Icon;

	public Color? IconTint;

	public Color? TermTint;

	public bool UseValueTextFormatting = true;

	public bool UseValueTooltipFormatting = true;

	public float Edge { get; set; }
}
