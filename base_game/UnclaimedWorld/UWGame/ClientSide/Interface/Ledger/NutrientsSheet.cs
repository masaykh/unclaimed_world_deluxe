using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.SimSide;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger;

public class NutrientsSheet : LedgerSheet
{
	private const int columnWidth = 90;

	private const int nameWidth = 150;

	private const int productionX = 150;

	private const int consumedX = 240;

	private const int overconsumedX = 330;

	private const int storedX = 465;

	private const int daysLeftX = 555;

	private int bottomPartHeight = 40;

	private string overconsumedTooltip;

	private string nameTooltip;

	private string producedTooltip;

	private string consumedTooltip;

	private string storedTooltip;

	private string daysLeftTooltip;

	private Grid outerGrid;

	public const int ItemHeight = 26;

	private SortingButtons<NutrientSheetSettings.SortColumns> sortingButtons;

	private int gridYPos;

	public override string DisplayName => "Nutrients";

	public override string Tooltip => "Shows food nutrients produced and consumed.";

	public override bool ShowRangeSelector => true;

	private int GridMaxHeight => Height - gridYPos - bottomPartHeight;

	public NutrientsSheet(GUIManager gui, int width, int height)
		: base(gui, width, height)
	{
		CreateTooltips();
		CreateGridHeaderButtons();
		gridYPos = sortingButtons.Bottom + 6;
		outerGrid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		Add(outerGrid);
		outerGrid.RenderType = RenderType.CRTAndLCD;
		outerGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		outerGrid.Width = Width;
		outerGrid.Height = GridMaxHeight;
		outerGrid.Position = new Point(0, gridYPos);
		outerGrid.RowSpacing = 1;
		outerGrid.FixedItemHeights = true;
		outerGrid.ItemHeight = 26;
		outerGrid.CanGrowInHeight = false;
		outerGrid.ScrollBarEnabled = true;
	}

	private void CreateTooltips()
	{
		nameTooltip = CreateTooltip("Name", "Name of food nutrient type");
		overconsumedTooltip = CreateTooltip("Overconsumed", "These nutrients were consumed but were not needed. To reduce this waste, make sure there is food with different nutrient profiles available.");
		consumedTooltip = CreateTooltip("Consumed", "Consumed by colony members");
		producedTooltip = CreateTooltip("Produced", "Produced by the colony");
		storedTooltip = CreateTooltip("Stored", "Currently stored");
		daysLeftTooltip = CreateTooltip("Days left", "How many days the current store will last");
	}

	public void LoadUserSettings(SortingSettings<NutrientSheetSettings.SortColumns> settings)
	{
		sortingButtons.Fill(settings);
	}

	private void CreateGridHeaderButtons()
	{
		sortingButtons = new SortingButtons<NutrientSheetSettings.SortColumns>(base.GUIManager);
		sortingButtons.Width = Width;
		sortingButtons.Height = 30;
		sortingButtons.Position = new Point(0, 0);
		Add(sortingButtons);
		sortingButtons.SortClicked += tbSort_Click;
		sortingButtons.AddTextButton(150, "NAME", NutrientSheetSettings.SortColumns.Name, nameTooltip);
		sortingButtons.AddTextButton(90, "PROD.", NutrientSheetSettings.SortColumns.Produced, producedTooltip);
		sortingButtons.AddTextButton(90, "CONS.", NutrientSheetSettings.SortColumns.Consumed, consumedTooltip);
		sortingButtons.AddTextButton(90, "OVER.", NutrientSheetSettings.SortColumns.Overconsumed, overconsumedTooltip);
		sortingButtons.CreateTextButton(465, 90, "STORED", NutrientSheetSettings.SortColumns.Stored, storedTooltip);
		sortingButtons.CreateTextButton(555, 90, "DAYS", NutrientSheetSettings.SortColumns.DaysLeft, daysLeftTooltip);
	}

	private void tbSort_Click()
	{
		Populate();
	}

	public override void RefreshData()
	{
		Populate();
		base.RefreshData();
	}

	private void Populate()
	{
		Expedition expedition = The.InGameUI.GetExpedition();
		if (expedition == null)
		{
			return;
		}
		_ = expedition.OwnedEntities;
		NutrientStatistics nutrientStatistics = The.InGameUI.UIAllegiance.Statistics.NutrientStatistics;
		Dictionary<FoodNutrientType, List<DataPoint<float>>> dict = nutrientStatistics.Stats[NutrientStatistics.StatTypes.Produced];
		Dictionary<FoodNutrientType, List<DataPoint<float>>> dict2 = nutrientStatistics.Stats[NutrientStatistics.StatTypes.Consumed];
		Dictionary<FoodNutrientType, List<DataPoint<float>>> dict3 = nutrientStatistics.Stats[NutrientStatistics.StatTypes.Overconsumed];
		Dictionary<FoodNutrientType, float> nutrients = GetNutrients(expedition);
		Dictionary<FoodNutrientType, float> nutrientsDaysLeft = GetNutrientsDaysLeft(expedition, nutrients);
		HashSet<FoodNutrientType> neededNutrients = GetMemberNutrients(expedition);
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		DateAndTime.TimeDateYear fromDate = base.FromDate;
		outerGrid.BeginAddingEntries();
		foreach (FoodNutrientType item2 in neededNutrients)
		{
			float produced = SumDataPoints(dict, item2, fromDate, currentTimeDateYear);
			float consumed = SumDataPoints(dict2, item2, fromDate, currentTimeDateYear);
			float overConsumed = SumDataPoints(dict3, item2, fromDate, currentTimeDateYear);
			if (!outerGrid.TryGetEntry(item2, out var item))
			{
				item = AddItemRow(item2);
			}
			UpdateItemRow(item, item2, produced, consumed, overConsumed, nutrients[item2], nutrientsDaysLeft[item2]);
		}
		outerGrid.DeleteEntries((FoodNutrientType j) => neededNutrients.Contains(j));
		outerGrid.Sort((UIComponent u) => u.OrderByTag1, The.InGameUI.NutrientSheetSettings.SortingSettings.SortOrder);
		outerGrid.EndAddingEntries();
	}

	public Dictionary<FoodNutrientType, float> GetNutrients(Expedition expedition)
	{
		Dictionary<FoodNutrientType, float> dictionary = new Dictionary<FoodNutrientType, float>();
		foreach (KeyValuePair<string, FoodNutrientType> allFoodNutrientType in GameData.Instance.AllFoodNutrientTypes)
		{
			dictionary.Add(allFoodNutrientType.Value, 0f);
		}
		OwnerID? ownerID = expedition.OwnedEntities.GetOwnerID();
		List<Entity> list = null;
		foreach (KeyValuePair<EntityType, List<EntityID>> item in expedition.OwnedEntities.Food)
		{
			if (!expedition.IsEatableByIndependentMembers(item.Key))
			{
				continue;
			}
			List<EntityID> listOfAvailableEntities = null;
			List<EntityID> listOfUnavailableEntities = null;
			foreach (EntityID item2 in item.Value)
			{
				Entity entity = Entity.FindByID(item2);
				if (entity != null)
				{
					int noOfIncompleteEntities = 0;
					int noOfAvailableEntities = 0;
					int noOfAvailableEntitiesIncludingIntrinsic = 0;
					int noOfEntitiesUsedAsParts = 0;
					int noOfItemsOnOtherSite = 0;
					int noOfItemsOwnedByOthers = 0;
					EntityGroup.CountEntity(ownerID, entity, ref noOfIncompleteEntities, ref noOfEntitiesUsedAsParts, ref noOfItemsOnOtherSite, ref noOfItemsOwnedByOthers, ref noOfAvailableEntities, ref noOfAvailableEntitiesIncludingIntrinsic, ref listOfAvailableEntities, ref listOfUnavailableEntities);
					if (noOfAvailableEntities > 0)
					{
						Common.AddToList(ref list, entity);
					}
				}
			}
		}
		if (list != null)
		{
			foreach (FoodNutrientType item3 in dictionary.Keys.ToList())
			{
				float num = 0f;
				foreach (Entity item4 in list)
				{
					if (item4.Item.Food.NutrientBulkAmounts.TryGetValue(item3, out var value))
					{
						num += value;
					}
				}
				dictionary[item3] = num;
			}
		}
		return dictionary;
	}

	private HashSet<FoodNutrientType> GetMemberNutrients(Expedition expedition)
	{
		HashSet<FoodNutrientType> nutrients = new HashSet<FoodNutrientType>();
		expedition.IterateMembers(delegate(Entity e)
		{
			if (e.BiologicalEntity != null && ProductionStatistics.CountMemberConsumption(e))
			{
				foreach (KeyValuePair<string, Need> needs in e.BiologicalEntity.Needs.NeedsList)
				{
					if (needs.Value.FoodNeed != null)
					{
						nutrients.Add(needs.Value.NeedType.FoodNeedType.FoodNutrientType);
					}
				}
			}
		});
		return nutrients;
	}

	public Dictionary<FoodNutrientType, float> GetNutrientsDaysLeft(Expedition expedition, Dictionary<FoodNutrientType, float> stored)
	{
		Dictionary<FoodNutrientType, float> dictionary = new Dictionary<FoodNutrientType, float>();
		foreach (KeyValuePair<FoodNutrientType, float> item in stored)
		{
			float totalDailyNeed = 0f;
			expedition.IterateMembers(delegate(Entity e)
			{
				if (e.BiologicalEntity != null && ProductionStatistics.CountMemberConsumption(e) && e.BiologicalEntity.Needs.NeedsList.TryGetValue(item.Key.KeyName, out var value2) && value2.FoodNeed != null)
				{
					totalDailyNeed += value2.FoodNeed.TotalNeededNutrientBulk * value2.DecreasePerDay;
				}
			});
			float value = item.Value / totalDailyNeed;
			dictionary[item.Key] = value;
		}
		return dictionary;
	}

	private void SetLabelNegativeColorAndValue(Label lbl, float value, int xPosRight)
	{
		SetLabelValue(lbl, value, xPosRight);
		if (lbl.Text != "0")
		{
			lbl.NormalColor = GameData.Instance.GUIConstants.NegativeColor;
		}
	}

	private static void SetLabelValue(Label lbl, float value, int xPosRight)
	{
		lbl.Text = Common.DecimalToStringSignificant(value, 0.0001f, "0.0001", 1E-06f);
		lbl.FitToText();
		lbl.AlignRight(xPosRight);
	}

	private void SetLabelPositiveColorAndValue(Label lbl, float value, int xPosRight)
	{
		SetLabelValue(lbl, value, xPosRight);
		if (lbl.Text != "0")
		{
			lbl.NormalColor = GameData.Instance.GUIConstants.PositiveColor;
		}
	}

	private float SumDataPoints(Dictionary<FoodNutrientType, List<DataPoint<float>>> dict, FoodNutrientType nutrientType, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to)
	{
		if (dict.TryGetValue(nutrientType, out var value))
		{
			return Statistic.GetDataPointsBetween(value, from, to).Sum((DataPoint<float> d) => d.Value);
		}
		return 0f;
	}

	private void UpdateItemRow(UIComponent itemRow, FoodNutrientType nutrientType, float produced, float consumed, float overConsumed, float stored, float daysLeft)
	{
		int num = 10;
		itemRow.FindChildById<Label>(DataControlID.Produced, out var child, firstLevelOnly: false);
		SetLabelPositiveColorAndValue(child, produced, 240 - num);
		child.ToolTip = producedTooltip;
		itemRow.FindChildById<Label>(DataControlID.Consumed, out var child2, firstLevelOnly: false);
		SetLabelValue(child2, consumed, 330 - num);
		child2.ToolTip = consumedTooltip;
		itemRow.FindChildById<Label>(DataControlID.Overconsumed, out var child3, firstLevelOnly: false);
		SetLabelNegativeColorAndValue(child3, overConsumed, 420 - num);
		child3.ToolTip = overconsumedTooltip;
		itemRow.FindChildById<Label>(DataControlID.Stored, out var child4, firstLevelOnly: false);
		SetLabelPositiveColorAndValue(child4, stored, 555 - num);
		child4.ToolTip = storedTooltip;
		itemRow.FindChildById<Label>(DataControlID.DaysLeft, out var child5, firstLevelOnly: false);
		SetLabelPositiveColorAndValue(child5, daysLeft, 645 - num);
		child5.ToolTip = daysLeftTooltip;
		object orderByTag = null;
		switch (The.InGameUI.NutrientSheetSettings.SortingSettings.SortedBy)
		{
		case NutrientSheetSettings.SortColumns.Name:
			orderByTag = nutrientType.Name;
			break;
		case NutrientSheetSettings.SortColumns.Consumed:
			orderByTag = consumed;
			break;
		case NutrientSheetSettings.SortColumns.Produced:
			orderByTag = produced;
			break;
		case NutrientSheetSettings.SortColumns.Overconsumed:
			orderByTag = overConsumed;
			break;
		case NutrientSheetSettings.SortColumns.Stored:
			orderByTag = stored;
			break;
		case NutrientSheetSettings.SortColumns.DaysLeft:
			orderByTag = daysLeft;
			break;
		}
		itemRow.OrderByTag1 = orderByTag;
	}

	private UIComponent AddItemRow(FoodNutrientType nutrientType)
	{
		GUIManager gUIManager = base.GUIManager;
		Grid grid = outerGrid;
		UIComponent uIComponent = new UIComponent(gUIManager)
		{
			DebugTag = "stocksItem"
		};
		grid.AddEntry(nutrientType, uIComponent);
		Label label = new Label(gUIManager);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.X = 0;
		label.Text = nutrientType.Name;
		label.FitToText();
		uIComponent.CenterChildVertically(label);
		Label label2 = new Label(gUIManager);
		uIComponent.Add(label2);
		label2.Init(Label.LabelType.LCDNormal);
		label2.X = 150;
		label2.ID = DataControlID.Produced;
		uIComponent.CenterChildVertically(label2);
		Label label3 = new Label(gUIManager);
		uIComponent.Add(label3);
		label3.Init(Label.LabelType.LCDNormal);
		label3.X = 240;
		label3.ID = DataControlID.Consumed;
		uIComponent.CenterChildVertically(label3);
		Label label4 = new Label(gUIManager);
		uIComponent.Add(label4);
		label4.Init(Label.LabelType.LCDNormal);
		label4.X = 330;
		label4.ID = DataControlID.Overconsumed;
		uIComponent.CenterChildVertically(label4);
		Label label5 = new Label(gUIManager);
		uIComponent.Add(label5);
		label5.Init(Label.LabelType.LCDNormal);
		label5.X = 465;
		label5.ID = DataControlID.Stored;
		uIComponent.CenterChildVertically(label5);
		Label label6 = new Label(gUIManager);
		uIComponent.Add(label6);
		label6.Init(Label.LabelType.LCDNormal);
		label6.X = 465;
		label6.ID = DataControlID.DaysLeft;
		uIComponent.CenterChildVertically(label6);
		return uIComponent;
	}

	private void AddCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, object key)
	{
		GUIManager gUIManager = base.GUIManager;
		EntityCategory entityCategory = key as EntityCategory;
		cpCategory = new CollapsablePanel(gUIManager, CollapsablePanel.PanelType.DropDownBig);
		cpCategory.HeadingYPos = 4;
		cpCategory.CollapsedHeight = 28;
		outerGrid.AddEntry(key, cpCategory);
		cpCategory.OrderByTag1 = entityCategory.Name;
		cpCategory.Init();
		cpCategory.Title = entityCategory.Name;
		cpCategory.Width = outerGrid.Width;
		categoryGrid = new Grid(gUIManager, ListBoxType.LCD, Label.LabelType.LCDNormal);
		categoryGrid.DebugTag = "categoryGrid";
		categoryGrid.FixedItemHeights = true;
		categoryGrid.Width = cpCategory.Width;
		cpCategory.AddContent(categoryGrid);
		categoryGrid.ScrollBarEnabled = false;
		categoryGrid.ItemHeight = 26;
		categoryGrid.CanGrowInHeight = true;
		categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		categoryGrid.IsOuterGrid = false;
	}
}
