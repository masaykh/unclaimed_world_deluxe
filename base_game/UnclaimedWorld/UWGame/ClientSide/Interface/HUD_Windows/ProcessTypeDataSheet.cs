using System.Linq;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class ProcessTypeDataSheet : DataSheet
{
	private ProcessType processType;

	private const float outputsIndex = 200f;

	private Grid grdOutputs;

	private UIComponent outputsHeader;

	private Label lblOutputs;

	public void Fill(ProcessType processType)
	{
		this.processType = processType;
		Fill();
	}

	protected override string GetDescription()
	{
		return processType.Description;
	}

	protected override void CreateGeneralPanelContents()
	{
	}

	protected override void CreateProductionPanelContents()
	{
		CreateGridAndHeader(grdProductionOuter, out outputsHeader, "OUTPUT:", "Creates these output types and amounts", 200f, out grdOutputs, out lblOutputs);
	}

	protected override void Retire()
	{
		The.InGameUI.poolOfProcessTypeTooltips.Retire(this);
	}

	protected override void PopulateCollapsedFieldsContents()
	{
		if (processTypeToShowProductionFor != null)
		{
			lblName.Text = processTypeToShowProductionFor.Name;
			summaryDescription.Text = processTypeToShowProductionFor.SummaryDescription;
		}
	}

	protected override void PopulateGeneralDataContent()
	{
	}

	protected override ProcessType GetProcessToShow()
	{
		return processType;
	}

	protected override void PopulateProductionContentRefresh()
	{
		ResolveOwner(out var resolvedOwner);
		PopulateOutputsList(resolvedOwner);
	}

	protected override void SetProductionHeading(Label lbl)
	{
		lbl.Text = "REQUIREMENTS";
		PadHeader(lbl);
	}

	private UIComponent AddOutputItemRow(EntityType outputEntityType, bool ownsItem, int amount)
	{
		UIComponent uIComponent = new UIComponent(gui);
		grdOutputs.AddEntry(outputEntityType, uIComponent);
		CreateItemGridRow(outputEntityType, uIComponent, out var _);
		return uIComponent;
	}

	private void PopulateOutputsList(EntityGroup resolvedOwner)
	{
		grdOutputs.BeginAddingEntries();
		if (processType.Outputs != null)
		{
			Output[] outputs = processType.Outputs;
			foreach (Output output in outputs)
			{
				EntityType finalEntityTypeToCreate = output.FinalEntityTypeToCreate;
				if (!output.IsWasteProduct)
				{
					InventoryPanel.OwnsProductOrHasProcessInputsAndTools(output.FinalEntityTypeToCreate, resolvedOwner, out var ownsItem, out var _, out var _, out var _, null, countPartsOfEntities: true);
					if (!grdOutputs.TryGetEntry(finalEntityTypeToCreate, out var item))
					{
						item = AddEntityAmountRow(grdOutputs, finalEntityTypeToCreate, ownsItem, "The amount that will be produced");
					}
					UpdateEntityAmountRow(item, finalEntityTypeToCreate, ownsItem, output.Amount.NoOfItems ?? 0);
				}
			}
			grdOutputs.DeleteEntries((EntityType e) => processType.Outputs.Any((Output o) => o.FinalEntityTypeToCreate == e));
		}
		else
		{
			grdOutputs.Clear();
		}
		grdOutputs.EndAddingEntries();
		grdProductionOuter.TryRemoveEntry(outputsHeader);
		if (grdOutputs.Entries.Count > 0)
		{
			grdProductionOuter.AddEntry(outputsHeader, outputsHeader);
			lblOutputs.Text = "OUTPUT:";
			lblOutputs.ToolTip = "Shows the outputs for the process";
		}
	}

	protected override string GetInputHeading()
	{
		return "MATERIALS";
	}
}
