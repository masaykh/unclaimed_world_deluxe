using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Trees;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class SidePanelEditorEntity : RosterPanel
{
	private Grid outerGrid;

	public EntityType SelectedEntityType;

	private Label lblStatusInfo1;

	private Label lblStatusInfo2;

	private CollapsablePanel cpTerrain;

	private CollapsablePanel cpTrees;

	private Grid terrainGrid;

	private Grid treeGrid;

	private MapEditorSaveLoadPanel saveDialog;

	private CheckBox cbDelete;

	private CheckBox cbFlipHorizontally;

	private CheckBox cbYoung;

	private CheckBox cbGrown;

	private CheckBox cbSummer;

	private CheckBox cbWinter;

	private CheckBox cbRandom;

	private CheckBox cbSmallBrush;

	private CheckBox cbBigBrush;

	private int buttonYPos;

	private Dictionary<string, CheckBox> options = new Dictionary<string, CheckBox>();

	private static char[] stopChars = new char[11]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'_'
	};

	public SidePanelEditorEntity()
		: base(The.InGameUI.sidePanelFullHeight, isInfoPanel: true, 150)
	{
		int value = 20;
		outerGrid = RosterPanel.CreateOuterGridForCollapsableLists(The.InGameUI.gui, lcdSurface);
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Terrain", value, out cpTerrain, out terrainGrid);
		terrainGrid.SelectedChanged += terrainGrid_SelectedChanged;
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Trees", value, out cpTrees, out treeGrid);
		treeGrid.SelectedChanged += treeGrid_SelectedChanged;
		PopulateGrid();
		InitStatusContentPanel();
		InitCheckBoxes();
		AddBottomButtonInSequence(null, out var newButton, "SAVE!", "Saves the map.");
		newButton.Click += saveButton_Click;
		AddBottomButtonInSequence(newButton, out var newButton2, "CLEAR!", "Clears the contents of the tile you click on.");
		newButton2.Click += btClear_Click;
		buttonYPos = newButton.Y;
		int num = buttonYPos - 86;
		cbDelete = new CheckBox(Interface.gui);
		cbDelete.Text = "DEL";
		cbDelete.Init(CheckBoxType.LED, CheckBoxFlavor.Red);
		cbDelete.Label.NormalColor = Color.Black;
		cbDelete.Width = 80;
		cbDelete.Click += cbDelete_Click;
		int x = display.X;
		int y = num;
		Window.Add(cbDelete);
		cbDelete.Position = new Point(x, y);
		saveDialog = new MapEditorSaveLoadPanel(MapEditorSaveLoadPanel.SaveOrLoad.Save, Interface, Point.Zero);
		saveDialog.SaveOrLoadClick += saveDialog_SaveOrLoadClick;
	}

	private void cbDrawCoords_Click(UIComponent sender, EventArgs e)
	{
	}

	private void cbDrawResources_Click(UIComponent sender, EventArgs e)
	{
	}

	private void cbDelete_Click(UIComponent sender, EventArgs e)
	{
		if (cbDelete.IsChecked)
		{
			The.InGameUI.DestroyEntityBeingPlaced();
			intface.InterfaceMode = InGameInterface.InterfaceState.EditorDeleteEntities;
		}
		else if (SelectedEntityType != null)
		{
			intface.InterfaceMode = InGameInterface.InterfaceState.EditorPlaceEntity;
			CreateEntityForPlacement(SelectedEntityType);
		}
		else
		{
			intface.InterfaceMode = InGameInterface.InterfaceState.None;
		}
	}

	private void saveDialog_SaveOrLoadClick(object sender, EventArgs e)
	{
	}

	private void cbYoungTree_Click(UIComponent sender, EventArgs e)
	{
		cbGrown.IsChecked = false;
	}

	private void btClear_Click(UIComponent sender, EventArgs e)
	{
		intface.InterfaceMode = InGameInterface.InterfaceState.EditorClearTile;
	}

	private void cbFlipHorizontally_Click(UIComponent sender, EventArgs e)
	{
		if (intface.EntitiesBeingPlaced == null)
		{
			return;
		}
		foreach (InGameInterface.EntityPosition item in intface.EntitiesBeingPlaced)
		{
			FlipEntityIfChosen(item.Entity);
		}
	}

	private void InitCheckBoxes()
	{
		cbFlipHorizontally = new CheckBox(Interface.gui);
		cbFlipHorizontally.Text = "FLIPPED";
		cbFlipHorizontally.Init(CheckBoxType.LED, CheckBoxFlavor.Purple);
		cbFlipHorizontally.Click += cbFlipHorizontally_Click;
		cbFlipHorizontally.Label.NormalColor = Color.Black;
		cbFlipHorizontally.Width = 100;
		cbYoung = new CheckBox(Interface.gui);
		cbYoung.Text = "YOUNG";
		cbYoung.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
		cbYoung.Click += cbYoungTree_Click;
		cbYoung.Label.NormalColor = Color.Black;
		cbYoung.Width = 80;
		cbGrown = new CheckBox(Interface.gui);
		cbGrown.Text = "GROWN";
		cbGrown.Click += cbGrownTree_Click;
		cbGrown.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
		cbGrown.Label.NormalColor = Color.Black;
		cbGrown.Width = 80;
		cbSummer = new CheckBox(Interface.gui);
		cbSummer.Text = "SUMR";
		cbSummer.Click += cbSummer_Click;
		cbSummer.Init(CheckBoxType.LED, CheckBoxFlavor.Blue);
		cbSummer.Label.NormalColor = Color.Black;
		cbSummer.Width = 80;
		cbWinter = new CheckBox(Interface.gui);
		cbWinter.Text = "WITR";
		cbWinter.Click += cbWinter_Click;
		cbWinter.Init(CheckBoxType.LED, CheckBoxFlavor.Blue);
		cbWinter.Label.NormalColor = Color.Black;
		cbWinter.Width = 80;
		cbRandom = new CheckBox(Interface.gui);
		cbRandom.Text = "RANDOM";
		cbRandom.Init(CheckBoxType.LED, CheckBoxFlavor.Red);
		cbRandom.Label.NormalColor = Color.Black;
		cbRandom.Width = 100;
		cbSmallBrush = new CheckBox(Interface.gui);
		cbSmallBrush.Text = "S BRUSH";
		cbSmallBrush.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
		cbSmallBrush.Click += cbSmallBrush_Click;
		cbSmallBrush.Label.NormalColor = Color.Black;
		cbSmallBrush.Width = 100;
		cbBigBrush = new CheckBox(Interface.gui);
		cbBigBrush.Text = "L BRUSH";
		cbBigBrush.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
		cbBigBrush.Click += cbBigBrush_Click;
		cbBigBrush.Label.NormalColor = Color.Black;
		cbBigBrush.Width = 100;
	}

	private void cbBigBrush_Click(UIComponent sender, EventArgs e)
	{
		cbSmallBrush.IsChecked = false;
	}

	private void cbSmallBrush_Click(UIComponent sender, EventArgs e)
	{
		cbBigBrush.IsChecked = false;
	}

	private void cbGrownTree_Click(UIComponent sender, EventArgs e)
	{
		cbYoung.IsChecked = false;
	}

	private void cbWinter_Click(UIComponent sender, EventArgs e)
	{
		cbSummer.IsChecked = false;
	}

	private void cbSummer_Click(UIComponent sender, EventArgs e)
	{
		cbWinter.IsChecked = false;
	}

	private void UpdateCheckBoxes()
	{
		foreach (KeyValuePair<string, CheckBox> option in options)
		{
			Window.Remove(option.Value);
		}
		options.Clear();
		if (intface.EntitiesBeingPlaced != null && intface.EntitiesBeingPlaced.Count > 0)
		{
			Entity entity = intface.EntitiesBeingPlaced[0].Entity;
			options.Add(cbFlipHorizontally.Text, cbFlipHorizontally);
			if (entity.EntityType.TreeType != null || entity.EntityType.BiologicalType != null)
			{
				options.Add(cbYoung.Text, cbYoung);
				options.Add(cbGrown.Text, cbGrown);
			}
			if (entity.EntityType.TreeType != null && entity.EntityType.TreeType.HasSummerWinterCycle)
			{
				options.Add(cbSummer.Text, cbSummer);
				options.Add(cbWinter.Text, cbWinter);
			}
			if (entity.EntityType.TerrainType != null)
			{
				options.Add(cbRandom.Text, cbRandom);
			}
			options.Add(cbSmallBrush.Text, cbSmallBrush);
			options.Add(cbBigBrush.Text, cbBigBrush);
		}
		int num = display.X;
		int num2 = buttonYPos - 40;
		foreach (KeyValuePair<string, CheckBox> option2 in options)
		{
			Window.Add(option2.Value);
			option2.Value.Position = new Point(num, num2);
			num += option2.Value.Width - 15;
			if (num > display.Right - 60)
			{
				num = display.X;
				num2 -= 22;
			}
		}
	}

	private void InitStatusContentPanel()
	{
		statusContent = The.InGameUI.StatusScreen.GetNewSurfaceContent();
		_ = The.Sim.Controller.Game;
		GUIManager gui = The.InGameUI.gui;
		InitStatusImage();
		InitBillboardPanel();
		RosterPanel.InitStatusCRTHeader(gui, statusContent, 8, out lblStatusHeading, out crtUnderline);
		lblStatusInfo1 = new Label(gui);
		statusContent.Add(lblStatusInfo1);
		lblStatusInfo1.Position = new Point(8, 80);
		lblStatusInfo1.Init(Label.LabelType.CRTSmall);
		lblStatusInfo1.Width = 200;
		lblStatusInfo2 = new Label(gui);
		statusContent.Add(lblStatusInfo2);
		lblStatusInfo2.Position = new Point(8, 100);
		lblStatusInfo2.Init(Label.LabelType.CRTSmall);
		lblStatusInfo2.Width = 200;
	}

	private void RefreshStatusScreen()
	{
		if (SelectedEntityType != null && SelectedEntityType.RenderableTypeMode != null && SelectedEntityType.RenderableTypeMode.DefaultClientState != null && SelectedEntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType != null)
		{
			ShowBillboardPanel();
			RosterPanel.CreateAndPlaceBillboards(intface.gui, pnBillboards, SelectedEntityType, statusBillboardPanelCenter, 130f, doScaling: true);
			SetHeaderText(SelectedEntityType.Name.ToUpper(Config.Culture));
		}
	}

	private void PopulateGrid()
	{
		outerGrid.BeginAddingEntries();
		terrainGrid.BeginAddingEntries();
		foreach (KeyValuePair<string, EntityType> allTerrainFeatureType in GameData.Instance.AllTerrainFeatureTypes)
		{
			if (allTerrainFeatureType.Value.TerrainType.CanBeMapEditorPlaced)
			{
				terrainGrid.AddEntry(allTerrainFeatureType.Value, allTerrainFeatureType.Value.Name);
			}
		}
		terrainGrid.EndAddingEntries();
		treeGrid.BeginAddingEntries();
		foreach (KeyValuePair<string, EntityType> allTreeType in GameData.Instance.AllTreeTypes)
		{
			treeGrid.AddEntry(allTreeType.Value, allTreeType.Value.Name);
		}
		treeGrid.EndAddingEntries();
		outerGrid.EndAddingEntries();
	}

	public void CreateEntityForPlacement(EntityType entityType)
	{
		Vector2 vector = default(Vector2);
		List<Vector2> list = new List<Vector2>();
		if (cbSmallBrush.IsChecked)
		{
			list = UniformPoissonDiskSampler.SampleCircle(vector, 60f, 40f);
		}
		else if (cbBigBrush.IsChecked)
		{
			list = UniformPoissonDiskSampler.SampleCircle(vector, 120f, 40f);
		}
		else
		{
			list.Add(vector);
		}
		foreach (Vector2 item in list)
		{
			if (entityType.TerrainType != null && (cbRandom.IsChecked || cbSmallBrush.IsChecked || cbBigBrush.IsChecked))
			{
				string prefix = GetPrefix(entityType.KeyName);
				entityType = PickRandomTerrainFeatureWithPrefix(prefix);
			}
			Entity entity = CreateThisEntityForPlacement(entityType);
			intface.EntitiesBeingPlaced.Add(new InGameInterface.EntityPosition
			{
				Entity = entity,
				Position = item
			});
		}
	}

	public static string GetPrefix(string key)
	{
		int num = key.IndexOfAny(stopChars);
		if (num > 0)
		{
			return key.Substring(0, num);
		}
		return key;
	}

	private Entity CreateThisEntityForPlacement(EntityType entityType)
	{
		Entity entity = new Entity(entityType);
		EditorData editorData = new EditorData(entity);
		entity.Add(editorData);
		Allegiance allegiance = null;
		if (entity.Intelligence != null)
		{
			editorData.AllegianceKey = "Allegiance #" + entity.EntityID;
		}
		if (entity.BiologicalEntity != null)
		{
			if (The.Sim.Mode == Sim.EngineMode.Edit)
			{
				entity.Intelligence.DisableAI = true;
			}
			AIAgeGroup? ageGroup = null;
			if (cbYoung.IsChecked)
			{
				ageGroup = AIAgeGroup.YoungAdult;
			}
			else if (cbGrown.IsChecked)
			{
				ageGroup = AIAgeGroup.Adult;
			}
			The.Sim.InitializeBioEntityToPlace(Sim.PersonSex.Male, null, ageGroup, allegiance, entity);
		}
		else
		{
			if (entity.Find<Tree>(out var c))
			{
				float? num = null;
				if (cbYoung.IsChecked)
				{
					num = 0.5f * entity.EntityType.TreeType.MatureAge;
					c.SetAgePreInit(num.Value);
					editorData.TreeAgeGroup = UWGame.SimSide.Trees.AgeGroup.Young;
				}
				else if (cbGrown.IsChecked)
				{
					num = 1.5f * entity.EntityType.TreeType.MatureAge;
					c.SetAgePreInit(num.Value);
					editorData.TreeAgeGroup = UWGame.SimSide.Trees.AgeGroup.Grown;
				}
				else
				{
					c.SetAgePreInit();
					editorData.TreeAgeGroup = null;
				}
				if (cbSummer.IsChecked)
				{
					c.InSeason = InSeason.Summer;
				}
				else if (cbWinter.IsChecked)
				{
					c.InSeason = InSeason.Winter;
				}
				else if (entityType.TreeType.HasSummerWinterCycle)
				{
					if (The.Client.ClientRandomGenerator.Next(2, "SmallEditorPanel", saveMessage: false) == 0)
					{
						c.InSeason = InSeason.Summer;
					}
					else
					{
						c.InSeason = InSeason.Winter;
					}
				}
			}
			entity.Initialize(The.Sim.PlaySite, allegiance);
			entity.InitializeModelAndOnScreenFunctionality();
		}
		entity.FlipHorizontally = The.Client.ClientRandomGenerator.Next(2, "SmallEditorPanel", saveMessage: false) == 0;
		FlipEntityIfChosen(entity);
		return entity;
	}

	private void terrainGrid_SelectedChanged(UIComponent sender)
	{
		HandleUserSelectedAnEntityType(sender);
	}

	private EntityType PickRandomTerrainFeatureWithPrefix(string prefix)
	{
		List<EntityType> matchingTerrainFeatureTypes = GetMatchingTerrainFeatureTypes(prefix);
		int index = The.Client.ClientRandomGenerator.Next(matchingTerrainFeatureTypes.Count, "SmallEditorPanel", saveMessage: false);
		return matchingTerrainFeatureTypes[index];
	}

	public static List<EntityType> GetMatchingTerrainFeatureTypes(string prefix)
	{
		List<EntityType> list = new List<EntityType>();
		foreach (KeyValuePair<string, EntityType> allTerrainFeatureType in GameData.Instance.AllTerrainFeatureTypes)
		{
			if (MatchesPrefix(allTerrainFeatureType.Value, prefix))
			{
				list.Add(allTerrainFeatureType.Value);
			}
		}
		return list;
	}

	public static bool MatchesPrefix(EntityType entityType, string prefix)
	{
		if (entityType.KeyName == prefix)
		{
			return true;
		}
		if (entityType.KeyName.StartsWith(prefix))
		{
			string keyName = entityType.KeyName;
			int num = keyName.IndexOfAny(stopChars);
			if (num > 0 && keyName.Substring(0, num) == prefix)
			{
				return true;
			}
		}
		return false;
	}

	private void critterGrid_SelectedChanged(UIComponent sender)
	{
		HandleUserSelectedAnEntityType(sender);
	}

	private void treeGrid_SelectedChanged(UIComponent sender)
	{
		HandleUserSelectedAnEntityType(sender);
	}

	private void HandleUserSelectedAnEntityType(UIComponent sender)
	{
		SelectedEntityType = null;
		if (sender is Grid grid && grid.GetKey(grid.SelectedItem, out var key))
		{
			SelectedEntityType = (EntityType)key;
		}
		if (SelectedEntityType != null)
		{
			if (cbDelete.IsChecked)
			{
				intface.InterfaceMode = InGameInterface.InterfaceState.EditorDeleteEntities;
			}
			else
			{
				intface.InterfaceMode = InGameInterface.InterfaceState.EditorPlaceEntity;
				CreateEntityForPlacement(SelectedEntityType);
			}
		}
		else
		{
			intface.InterfaceMode = InGameInterface.InterfaceState.None;
		}
		UpdateCheckBoxes();
	}

	private void FlipEntityIfChosen(Entity entity)
	{
		if (cbFlipHorizontally.IsChecked)
		{
			entity.FlipHorizontally = true;
		}
	}

	private void saveButton_Click(UIComponent sender, EventArgs e)
	{
		saveDialog.ShowDialog(modal: true);
	}

	public override void Hide()
	{
		base.Hide();
	}

	public override void Refresh()
	{
	}
}
