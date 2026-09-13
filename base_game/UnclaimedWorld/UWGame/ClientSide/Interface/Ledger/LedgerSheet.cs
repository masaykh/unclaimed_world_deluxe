using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger;

public abstract class LedgerSheet : UIComponent
{
	public abstract string DisplayName { get; }

	public abstract string Tooltip { get; }

	public abstract bool ShowRangeSelector { get; }

	public DateAndTime.TimeDateYear FromDate { get; set; }

	protected string CreateTooltip(string header, string blob)
	{
		return Common.ComposeHeadingAndBlobText(header, blob);
	}

	public LedgerSheet(GUIManager gui, int width, int height)
		: base(gui)
	{
		base.Width = width;
		base.Height = height;
	}

	public virtual void RefreshData()
	{
	}
}
