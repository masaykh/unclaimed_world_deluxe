using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.StrategicDecisions;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Skills;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Personnel;

public class PersonnelList : UIComponent
{
	private Grid outerGrid;

	private bool isRoster;

	private const int itemHeight = 25;

	private const int horizPadding = 6;

	private const int vertPadding = 2;

	private const int ratingColumnX = 65;

	private const int professionColumnX = 106;

	private const int nameColumnX = 128;

	private const int missionStatusColumnX = 267;

	private const int ageColumnX = 297;

	private const int sexColumnX = 325;

	private const int statusColumn = 425;

	private List<IKnownEntityData> entities;

	private Func<List<IKnownEntityData>> getEntities;

	private bool showSelectors;

	private bool showMigrationRisk;

	private Dictionary<EntityID, List<CategoryPanelAndData>> customPanels = new Dictionary<EntityID, List<CategoryPanelAndData>>();

	public PersonnelList(CommonInterface intf, UIComponent lcdSurface, bool showSelectors, int bottomMargin, bool isRoster)
		: base(intf.gui)
	{
		this.showSelectors = showSelectors;
		this.isRoster = isRoster;
		if (isRoster)
		{
			Panel.CreateGridWithColumnHeadingsWithFixedLength(lcdSurface, 0, 25, out outerGrid, bottomMargin, new Tuple<string, int, int>("OPINION", 0, 98), new Tuple<string, int, int>("PERSON DATA", 106, 312), new Tuple<string, int, int>("STATUS", 410, 123));
		}
		else
		{
			Panel.CreateGridWithColumnHeadingsWithFixedLength(lcdSurface, 0, 25, out outerGrid, bottomMargin, new Tuple<string, int, int>("OPINION", 0, 98), new Tuple<string, int, int>("PERSON DATA", 106, 312));
		}
		outerGrid.FixedItemHeights = false;
	}

	public void Fill(Func<List<IKnownEntityData>> getEntities, bool showSelectors, bool showMigrationRisk)
	{
		this.getEntities = getEntities;
		this.showSelectors = showSelectors;
		this.showMigrationRisk = showMigrationRisk;
	}

	private UIComponent AddRow(IKnownEntityData entityData)
	{
		UIComponent uIComponent = new UIComponent(guiManager);
		uIComponent.Height = 25;
		uIComponent.Tag1 = entityData.EntityID;
		AddControlsToCollapsedPart(uIComponent, entityData);
		outerGrid.AddEntry(entityData.EntityID, uIComponent);
		return uIComponent;
	}

	private void AddExpandedPart(UIComponent item, EntityID entityID)
	{
		Box box = new Box(guiManager);
		box.ID = DataControlID.PanelBox;
		box.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"), Color.LightYellow, Color.LightYellow);
		box.Height = 0;
		box.Width = outerGrid.Width - 20;
		box.Y = item.FindChildById(DataControlID.Name).Bottom + 2;
		box.ID = DataControlID.PanelBox;
		box.Visible = true;
		box.DebugTag = "panelBox";
		box.Tag1 = entityID;
		int y = 4;
		Grid grid = new Grid(guiManager, ListBoxType.LCD, Label.LabelType.CRTBigGlow);
		grid.FixedItemHeights = false;
		grid.RenderType = RenderType.CRTAndLCD;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Width = 250;
		grid.ID = DataControlID.GridInItemRow;
		grid.Height = 50;
		grid.ItemHeight = 26;
		grid.Position = new Point(0, y);
		grid.CanGrowInHeight = true;
		grid.ScrollBarEnabled = false;
		grid.HeightResize += grid_HeightResize;
		grid.DebugTag = "innerGrid";
		box.Add(grid);
		box.CenterChildHorizontally(grid);
		item.Add(box);
		box.Parent.Height = 25;
		AddCustomPresentationPanel(entityID, grid);
	}

	private void grid_HeightResize(UIComponent sender)
	{
		SetExpandedItemHeight((Grid)sender);
	}

	private static void SetExpandedItemHeight(Grid innerGrid)
	{
		UIComponent uIComponent = innerGrid.Parent;
		uIComponent.Height = innerGrid.Height + 8;
		uIComponent.Parent.Height = uIComponent.Height + 25;
	}

	private void AddCustomPresentationPanel(EntityID entityID, Grid grid)
	{
		List<CategoryPanelAndData> value = new List<CategoryPanelAndData>();
		if (LookUp<Entity, EntityID>.FindByID(entityID).IsOnPlaySite())
		{
			SidePanelEntity.AddPresentationPanel(grid, value, GameData.Instance.CustomSidePanelData);
		}
		else
		{
			SidePanelEntity.AddPresentationPanel(grid, value, GameData.Instance.CustomOtherSiteSidePanelData);
		}
		grid.Tag1 = Entity.FindByID(entityID).AllegianceID;
		customPanels.Add(entityID, value);
	}

	private void AddControlsToCollapsedPart(UIComponent item, IKnownEntityData entityData)
	{
		CheckBox checkBox = new CheckBox(guiManager);
		checkBox.Init(CheckBoxType.LCDNoLabel);
		item.Add(checkBox);
		checkBox.X = 0;
		checkBox.Width = 28;
		checkBox.ID = DataControlID.Selector;
		item.CenterChildVertically(checkBox);
		Icon icon = new Icon(guiManager);
		icon.ScaleImageToSizeOfControl = false;
		item.Add(icon);
		icon.X = 28;
		icon.ID = DataControlID.RatingIcon;
		item.CenterChildVertically(icon);
		icon.TooltipWidth = 280;
		icon.TooltipExpires = false;
		Icon icon2 = new Icon(guiManager);
		icon2.ScaleImageToSizeOfControl = false;
		item.Add(icon2);
		icon2.X = 44;
		icon2.ID = DataControlID.BiggestConcernIcon;
		item.CenterChildVertically(icon2);
		icon2.TooltipWidth = 280;
		icon2.TooltipExpires = false;
		icon2.DebugTag = "imBiggestConcern";
		Icon icon3 = new Icon(guiManager);
		icon3.ScaleImageToSizeOfControl = false;
		item.Add(icon3);
		icon3.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle("lcd_icon_noEntry"), GameData.Instance.GUIConstants.NegativeTint, Hyperlink.HoverColor);
		icon3.ResizeControlToFitImage();
		icon3.X = 75;
		icon3.ID = DataControlID.CannotEmigrate;
		item.CenterChildVertically(icon3);
		icon3.TooltipWidth = 280;
		icon3.TooltipExpires = false;
		icon3.Visible = false;
		Label label = new Label(guiManager);
		item.Add(label);
		label.X = 65;
		label.Width = 120;
		label.Init(Label.LabelType.LCDNormal);
		label.ID = DataControlID.Rating;
		item.CenterChildVertically(label);
		label.TooltipWidth = 280;
		label.TooltipExpires = false;
		Icon icon4 = new Icon(guiManager);
		icon4.ScaleImageToSizeOfControl = false;
		item.Add(icon4);
		icon4.X = 106;
		icon4.Y = 4;
		icon4.ID = DataControlID.Profession;
		Hyperlink hyperlink = new Hyperlink(guiManager);
		item.Add(hyperlink);
		hyperlink.X = 128;
		hyperlink.Width = 120;
		hyperlink.NormalColor = UIComponent.LCDNormal;
		hyperlink.ID = DataControlID.Name;
		hyperlink.TargetEntityID = (uint)entityData.EntityID;
		hyperlink.RenderType = base.RenderType;
		item.CenterChildVertically(hyperlink);
		Icon icon5 = new Icon(guiManager);
		icon5.ScaleImageToSizeOfControl = false;
		item.Add(icon5);
		icon5.X = 267;
		icon5.Y = 0;
		icon5.ID = DataControlID.TravelStatus;
		Label label2 = new Label(guiManager);
		item.Add(label2);
		label2.Width = 40;
		label2.X = 297;
		label2.Y = 1;
		label2.Init(Label.LabelType.LCDNormal);
		label2.ID = DataControlID.Age;
		label2.ToolTip = "The age of the person";
		item.CenterChildVertically(label2);
		Icon icon6 = new Icon(guiManager);
		icon6.ScaleImageToSizeOfControl = false;
		item.Add(icon6);
		icon6.X = 325;
		icon6.Y = 2;
		icon6.ID = DataControlID.Sex;
		TextButton textButton = new TextButton(guiManager);
		item.Add(textButton);
		textButton.Init(TextButton.TextButtonType.LCD);
		textButton.Text = "MORE";
		textButton.ID = DataControlID.Expand;
		textButton.X = 345;
		textButton.Click += tbExpand_Click;
		textButton.ScaleToFitText();
		textButton.MinHeight = 25;
		textButton.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		if (isRoster)
		{
			Label label3 = new Label(guiManager);
			item.Add(label3);
			label3.X = 425;
			label3.Width = 120;
			label3.Init(Label.LabelType.LCDNormal);
			label3.ID = DataControlID.Status;
			item.CenterChildVertically(label3);
			label3.Width = 122;
		}
	}

	private void tbExpand_Click(UIComponent sender, EventArgs e)
	{
		TextButton textButton = sender as TextButton;
		EntityID entityID = (EntityID)sender.Parent.Tag1;
		outerGrid.TryGetEntry(entityID, out var item);
		if (textButton.IsChecked)
		{
			if (!customPanels.TryGetValue(entityID, out var _))
			{
				AddExpandedPart(item, entityID);
			}
			IKnownEntityData knownEntityData = entities.FirstOrDefault((IKnownEntityData n) => n.EntityID == entityID);
			if (knownEntityData != null)
			{
				UpdateExpandedPart(knownEntityData, item);
			}
		}
		Box box = (Box)item.FindChildById(DataControlID.PanelBox);
		if (textButton.IsChecked)
		{
			box.Visible = true;
			textButton.Text = "LESS";
			SetExpandedItemHeight((Grid)box.FindChildById(DataControlID.GridInItemRow));
		}
		else
		{
			box.Visible = false;
			textButton.Text = "MORE";
			item.Height = 25;
		}
	}

	private void UpdateRow(IKnownEntityData entity, UIComponent itemRow)
	{
		TextButton obj = (TextButton)itemRow.FindChildById(DataControlID.Expand);
		UpdateCollapsedPart(itemRow, entity);
		if (obj.IsChecked)
		{
			UpdateExpandedPart(entity, itemRow);
		}
	}

	private void UpdateExpandedPart(IKnownEntityData entityData, UIComponent itemRow)
	{
		Grid grid = (Grid)itemRow.FindChildById(DataControlID.GridInItemRow);
		SidePanelEntity.PopulateCustomPanels(entityData, customPanels[entityData.EntityID], grid);
	}

	private void CancelDialog()
	{
	}

	private void UpdateCollapsedPart(UIComponent itemRow, IKnownEntityData entityData)
	{
		Entity entity = entityData as Entity;
		UIComponent uIComponent = itemRow.FindChildById(DataControlID.Selector);
		if (showSelectors)
		{
			uIComponent.Visible = true;
		}
		else
		{
			uIComponent.Visible = false;
		}
		Icon icon = itemRow.FindChildById(DataControlID.Profession) as Icon;
		ProfessionType professionType = null;
		if (entity != null)
		{
			professionType = entity.Intelligence.Profession;
		}
		if (professionType != null && !string.IsNullOrEmpty(professionType.Icon))
		{
			icon.DebugTag = "profession";
			icon.Visible = true;
			icon.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(professionType.Icon), UIComponent.LCDNormal, Hyperlink.HoverColor);
			icon.ResizeControlToFitImage();
			icon.ToolTip = "Field: " + professionType.Name;
			itemRow.OrderByTag1 = professionType.Name;
		}
		else
		{
			icon.Visible = false;
			icon.ToolTip = null;
			itemRow.OrderByTag1 = "";
		}
		Hyperlink hyperlink = itemRow.FindChildById(DataControlID.Name) as Hyperlink;
		hyperlink.Text = entityData.Name ?? entityData.EntityType.Name;
		itemRow.OrderByTag2 = hyperlink.Text;
		if (entityData.Location.HasValue)
		{
			hyperlink.Enabled = true;
		}
		else
		{
			hyperlink.Enabled = false;
		}
		Reproduction? reproduction = null;
		if (entityData.CasteType != null)
		{
			reproduction = entityData.CasteType.Reproduction;
		}
		string text = null;
		string text2 = null;
		if (reproduction == Reproduction.Male)
		{
			text = "lcd_icon_male";
			text2 = "Male";
		}
		else if (reproduction == Reproduction.Female)
		{
			text = "lcd_icon_female";
			text2 = "Female";
		}
		if (text != null)
		{
			Icon obj = itemRow.FindChildById(DataControlID.Sex) as Icon;
			obj.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(text), UIComponent.LCDNormal, Hyperlink.HoverColor);
			obj.ToolTip = text2;
			obj.ResizeControlToFitImage();
		}
		string text3 = "";
		if (entity != null && entity.EntityType.BiologicalType != null)
		{
			entity.Find<BiologicalEntity>(out var c);
			text3 = c.AgeGroup.Age.ToString("N0");
		}
		(itemRow.FindChildById(DataControlID.Age) as Label).Text = text3;
		Icon icon2 = itemRow.FindChildById(DataControlID.RatingIcon) as Icon;
		Icon icon3 = itemRow.FindChildById(DataControlID.BiggestConcernIcon) as Icon;
		Icon cannotEmigrate = itemRow.FindChildById(DataControlID.CannotEmigrate) as Icon;
		Label label = itemRow.FindChildById(DataControlID.Rating) as Label;
		if (entity != null && entity.EntityType.Person != null)
		{
			icon2.Visible = true;
			if (!showMigrationRisk)
			{
				UpdateOtherSiteEntity(itemRow, entity, icon2, label);
			}
			else
			{
				UpdatePlaySiteEntity(itemRow, entity, icon2, cannotEmigrate, label, icon3);
			}
		}
		else
		{
			icon2.Visible = false;
			label.Visible = false;
			icon3.Visible = false;
		}
		label.FitToText();
		label.AlignRight(97);
		Icon icon4 = itemRow.FindChildById(DataControlID.TravelStatus) as Icon;
		string text4 = "";
		string text5 = "";
		if (entityData.Site.HasValue)
		{
			text4 = "On site";
			text5 = "HUD_icon_structure";
		}
		else
		{
			text4 = "Travelling";
			text5 = "hiker_map_icon";
		}
		if (isRoster)
		{
			Label label2 = itemRow.FindChildById(DataControlID.Status) as Label;
			if (entity != null)
			{
				string text6 = entity.Intelligence.Brain.GetStatus().ToUpper(Config.Culture);
				if (string.IsNullOrEmpty(text6))
				{
					text6 = "Idling".ToUpper(Config.Culture);
				}
				label2.Text = text6;
				label2.ToolTip = text6;
			}
			else
			{
				label2.Text = "";
				label2.ToolTip = null;
			}
		}
		icon4.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(text5), UIComponent.LCDNormal, Hyperlink.HoverColor);
		icon4.ToolTip = text4;
		icon4.ResizeControlToFitImage();
		itemRow.CenterChildVertically(icon4);
		icon4.CenterThisHorizontally(267);
	}

	private void UpdatePlaySiteEntity(UIComponent itemRow, Entity entity, Icon imRating, Icon cannotEmigrate, Label lblEmigrateRisk, Icon imBiggestConcern)
	{
		float happiness = entity.PersonEntity.Personality.Happiness;
		string spriteName;
		Color? color;
		if (Common.IsGreaterThan(happiness, 0f))
		{
			spriteName = "lcd_icon_smiley_happy";
			color = null;
			imBiggestConcern.Visible = false;
		}
		else if (Common.IsZero(happiness))
		{
			spriteName = "lcd_icon_smiley_content";
			color = null;
			imBiggestConcern.Visible = false;
		}
		else
		{
			spriteName = "lcd_icon_smiley_unhappy";
			color = GameData.Instance.GUIConstants.NegativeTint;
			if (entity.PersonEntity.Personality.MostUnhappyRating.HasValue)
			{
				RatingTypes value = entity.PersonEntity.Personality.MostUnhappyRating.Value;
				imBiggestConcern.Visible = true;
				imBiggestConcern.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(Statistic.RatingsTypeToIcon(value)), GameData.Instance.GUIConstants.NegativeTint, Hyperlink.HoverColor);
				imBiggestConcern.ResizeControlToFitImage();
				itemRow.CenterChildVertically(imBiggestConcern);
				StringBuilder stringBuilder = new StringBuilder();
				Common.Append(stringBuilder, "The area causing the most unhappiness for the character: ");
				Statistic.AppendRatingsTypeToStringAndIcon(stringBuilder, value);
				imBiggestConcern.ToolTip = stringBuilder.ToString();
			}
			else
			{
				imBiggestConcern.Visible = false;
			}
		}
		imRating.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(spriteName), color ?? UIComponent.LCDNormal, Hyperlink.HoverColor);
		imRating.ResizeControlToFitImage();
		imRating.Y = 3;
		imRating.ToolTip = entity.PersonEntity.Personality.HappinessBreakdown;
		if (entity.Intelligence.EmigrateDecider.CanEmigrateToAnyTarget())
		{
			cannotEmigrate.Visible = false;
			lblEmigrateRisk.Visible = true;
			string text = null;
			lblEmigrateRisk.Text = Common.PercentageToString(entity.Intelligence.EmigrateDecider.MigrationRisk);
			text = entity.Intelligence.EmigrateDecider.GetMigrateRiskTooltip();
			lblEmigrateRisk.ToolTip = text;
		}
		else
		{
			cannotEmigrate.Visible = true;
			lblEmigrateRisk.Visible = false;
			cannotEmigrate.ToolTip = entity.Intelligence.EmigrateDecider.GetCanEmigrateToTargetTooltip(null);
		}
	}

	private void SetCannotEmigrateIcon()
	{
	}

	private void UpdateOtherSiteEntity(UIComponent itemRow, Entity entity, Icon imRating, Label lblRating)
	{
		string text = "The person's rating of our colony.";
		string text2 = "";
		Color? color = null;
		string text3 = null;
		if (!entity.Intelligence.EmigrateDecider.CanEmigrateToTarget(The.InGameUI.UIAllegiance))
		{
			text3 = "lcd_icon_noEntry";
			color = GameData.Instance.GUIConstants.NegativeTint;
			imRating.ToolTip = entity.Intelligence.EmigrateDecider.GetCanEmigrateToTargetTooltip(The.InGameUI.UIAllegiance);
			lblRating.Visible = false;
		}
		else
		{
			lblRating.Visible = true;
			AllegianceRatings ratingsForAllegiance = entity.Intelligence.GetRatingsForAllegiance(The.InGameUI.UIAllegiance.ID);
			if (ratingsForAllegiance != null)
			{
				text = "The person's willingness to join our colony. \n";
				text += ratingsForAllegiance.Breakdown;
				text2 = Common.PercentageToString(ratingsForAllegiance.Desirability);
				if (entity.Intelligence.IsReadyForEmbark(The.InGameUI.UIAllegiance))
				{
					text3 = "lcd_icon_thumbsUp";
					color = GameData.Instance.GUIConstants.PositiveTint;
					imRating.ToolTip = "The person is willing to join our colony right now.";
				}
				else
				{
					text3 = "lcd_icon_thumbsDown";
					color = GameData.Instance.GUIConstants.NegativeTint;
					imRating.ToolTip = "The person is not willing to join our colony at this time.";
				}
				lblRating.Text = text2;
				lblRating.ToolTip = text;
			}
		}
		if (text3 != null)
		{
			imRating.Visible = true;
			imRating.SetSkinLocations(guiManager.GUISpriteSheet.GetSourceRectangle(text3), color ?? UIComponent.LCDNormal, Hyperlink.HoverColor);
			imRating.Y = itemRow.FindChildById(DataControlID.Sex).Y;
			imRating.ResizeControlToFitImage();
		}
		else
		{
			imRating.Visible = false;
		}
	}

	public List<EntityID> GetSelectedEntities()
	{
		List<EntityID> list = null;
		foreach (KeyValuePair<object, UIComponent> item in outerGrid.EntriesByKey)
		{
			if (((CheckBox)item.Value.FindChildById(DataControlID.Selector)).IsChecked)
			{
				Common.AddToList(ref list, (EntityID)item.Key);
			}
		}
		return list;
	}

	private void HandleDestroyedEntityGroupOrDataSource()
	{
		outerGrid.Clear();
		CancelDialog();
	}

	private bool GetData()
	{
		entities = getEntities();
		if (entities == null)
		{
			HandleDestroyedEntityGroupOrDataSource();
			return false;
		}
		return true;
	}

	public void Populate()
	{
		if (!GetData())
		{
			return;
		}
		outerGrid.BeginAddingEntries();
		if (entities != null)
		{
			for (int num = entities.Count - 1; num >= 0; num--)
			{
				IKnownEntityData knownEntityData = entities[num];
				if (knownEntityData.EntityType.Person != null)
				{
					if (!outerGrid.TryGetEntry(knownEntityData.EntityID, out var item))
					{
						item = AddRow(knownEntityData);
					}
					UpdateRow(knownEntityData, item);
				}
			}
		}
		DeleteRows();
		outerGrid.Sort((UIComponent r) => r.OrderByTag1, Grid.Sorting.Descending, (UIComponent r) => r.OrderByTag2, Grid.Sorting.Descending);
		outerGrid.EndAddingEntries();
	}

	private void DeleteRows()
	{
		for (int num = outerGrid.Entries.Count - 1; num >= 0; num--)
		{
			UIComponent uIComponent = outerGrid.Entries[num];
			EntityID key = (EntityID)uIComponent.Tag1;
			if (!entities.Exists((IKnownEntityData e) => e.EntityID == key))
			{
				DeleteRow(key);
			}
		}
	}

	private void DeleteRow(EntityID key)
	{
		outerGrid.RemoveEntry(key);
		customPanels.Remove(key);
	}
}
