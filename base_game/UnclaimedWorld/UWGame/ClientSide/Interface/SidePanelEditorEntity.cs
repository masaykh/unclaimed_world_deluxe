using System;
using System.Collections.Generic;
using System.Linq;
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

	private Label lblAge;

	private FillableBar fbAge;

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
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, UWGame.Locale.Text("Terrain"), value, out _, out terrainGrid);
		terrainGrid.SelectedChanged += terrainGrid_SelectedChanged;
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, UWGame.Locale.Text("Trees"), value, out _, out treeGrid);
		treeGrid.SelectedChanged += treeGrid_SelectedChanged;
		PopulateGrid();
		InitStatusContentPanel();
		InitCheckBoxes();
		AddBottomButtonInSequence(null, out var newButton, UWGame.Locale.Text("SAVE!"), UWGame.Locale.Text("Saves the map."));
		newButton.Click += saveButton_Click;
		AddBottomButtonInSequence(newButton, out var newButton2, UWGame.Locale.Text("CLEAR!"), UWGame.Locale.Text("Clears the contents of the tile you click on."));
		newButton2.Click += btClear_Click;
		buttonYPos = newButton.Y;
		int num = buttonYPos - 86;
		cbDelete = new CheckBox(Interface.gui);
		cbDelete.Text = UWGame.Locale.Text("DEL");
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
		RebuildEntityBeingPlaced();
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
		cbFlipHorizontally.Text = UWGame.Locale.Text("FLIPPED");
		cbFlipHorizontally.Init(CheckBoxType.LED, CheckBoxFlavor.Purple);
		cbFlipHorizontally.Click += cbFlipHorizontally_Click;
		cbFlipHorizontally.Label.NormalColor = Color.Black;
		cbFlipHorizontally.Width = 100;
		cbYoung = new CheckBox(Interface.gui);
		cbYoung.Text = UWGame.Locale.Text("YOUNG");
		cbYoung.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
		cbYoung.Click += cbYoungTree_Click;
		cbYoung.Label.NormalColor = Color.Black;
		cbYoung.Width = 80;
		cbGrown = new CheckBox(Interface.gui);
		cbGrown.Text = UWGame.Locale.Text("GROWN");
		cbGrown.Click += cbGrownTree_Click;
		cbGrown.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
		cbGrown.Label.NormalColor = Color.Black;
		cbGrown.Width = 80;
		cbSummer = new CheckBox(Interface.gui);
		cbSummer.Text = UWGame.Locale.Text("SUMR");
		cbSummer.Click += cbSummer_Click;
		cbSummer.Init(CheckBoxType.LED, CheckBoxFlavor.Blue);
		cbSummer.Label.NormalColor = Color.Black;
		cbSummer.Width = 80;
		cbWinter = new CheckBox(Interface.gui);
		cbWinter.Text = UWGame.Locale.Text("WITR");
		cbWinter.Click += cbWinter_Click;
		cbWinter.Init(CheckBoxType.LED, CheckBoxFlavor.Blue);
		cbWinter.Label.NormalColor = Color.Black;
		cbWinter.Width = 80;
		cbRandom = new CheckBox(Interface.gui);
		cbRandom.Text = UWGame.Locale.Text("RANDOM");
		cbRandom.Init(CheckBoxType.LED, CheckBoxFlavor.Red);
		cbRandom.Click += cbRandom_Click;
		cbRandom.Label.NormalColor = Color.Black;
		cbRandom.Width = 100;
		cbSmallBrush = new CheckBox(Interface.gui);
		cbSmallBrush.Text = UWGame.Locale.Text("S BRUSH");
		cbSmallBrush.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
		cbSmallBrush.Click += cbSmallBrush_Click;
		cbSmallBrush.Label.NormalColor = Color.Black;
		cbSmallBrush.Width = 100;
		cbBigBrush = new CheckBox(Interface.gui);
		cbBigBrush.Text = UWGame.Locale.Text("L BRUSH");
		cbBigBrush.Init(CheckBoxType.LED, CheckBoxFlavor.Green);
		cbBigBrush.Click += cbBigBrush_Click;
		cbBigBrush.Label.NormalColor = Color.Black;
		cbBigBrush.Width = 100;
		InitAgeSlider();
	}

	private void cbRandom_Click(UIComponent sender, EventArgs e)
	{
		RebuildEntityBeingPlaced();
	}

	/// <summary>
	/// PORT: what follows the pointer is made when an asset is chosen (CreateEntityForPlacement),
	/// so a change to its options reached only the one made after the next click - the asset
	/// placed first kept the old age, season or brush (Kastuk, 2026-10-11). Made again here, with
	/// the options as they are now.
	/// </summary>
	private void RebuildEntityBeingPlaced()
	{
		if (intface.InterfaceMode != InGameInterface.InterfaceState.EditorPlaceEntity || SelectedEntityType == null)
		{
			return;
		}
		The.InGameUI.DestroyEntityBeingPlaced();
		CreateEntityForPlacement(SelectedEntityType);
		The.InGameUI.UpdateEntitiesBeingPlacedPositions();
	}

	private void cbBigBrush_Click(UIComponent sender, EventArgs e)
	{
		cbSmallBrush.IsChecked = false;
		RebuildEntityBeingPlaced();
	}

	private void cbSmallBrush_Click(UIComponent sender, EventArgs e)
	{
		cbBigBrush.IsChecked = false;
		RebuildEntityBeingPlaced();
	}

	private void cbGrownTree_Click(UIComponent sender, EventArgs e)
	{
		cbYoung.IsChecked = false;
		RebuildEntityBeingPlaced();
	}

	private void cbWinter_Click(UIComponent sender, EventArgs e)
	{
		cbSummer.IsChecked = false;
		RebuildEntityBeingPlaced();
	}

	private void cbSummer_Click(UIComponent sender, EventArgs e)
	{
		cbWinter.IsChecked = false;
		RebuildEntityBeingPlaced();
	}

	/// <summary>
	/// PORT: a plant's age is chosen in tenths of a year up to its TreeType.MaxAge, at the
	/// leftmost a random age as the studio's placing gave with neither YOUNG nor GROWN on. It
	/// replaces those two for plants (Kastuk, 2026-10-11: "a slider of tree age ... instead of
	/// current switchers Young and Grown, as there's quite a lot of stages at some plants").
	/// Creatures keep YOUNG and GROWN. Beside DEL, which has that row's left end.
	/// </summary>
	private void InitAgeSlider()
	{
		lblAge = new Label(Interface.gui);
		lblAge.Init(Label.LabelType.LCDNormal);
		lblAge.NormalColor = Color.Black;
		lblAge.Text = UWGame.Locale.Text("AGE");
		lblAge.FitToText();
		fbAge = new FillableBar(Interface.gui, FillableBar.FillableBarType.LCDSlider, canGrow: false);
		fbAge.ShowNotches = false;
		fbAge.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Always;
		fbAge.DisplayValueFunction = (int tenths) => (tenths == 0) ? UWGame.Locale.Text("random") : (0.1f * (float)tenths).ToString("0.#");
		fbAge.SliderTooltip = UWGame.Locale.Text("Age in years. Leftmost: a random age.");
		fbAge.SliderMouseUp += fbAge_SliderMouseUp;
	}

	private void fbAge_SliderMouseUp(object sender, EventArgs e)
	{
		RebuildEntityBeingPlaced();
	}

	private void ShowAgeSlider(TreeType treeType)
	{
		int maxTenths = Math.Max(1, (int)Math.Round(10f * treeType.MaxAge));
		if (fbAge.MaxValue != maxTenths)
		{
			// Another plant: the same share of its life, so GROWN stays grown.
			int value = (fbAge.MaxValue > 0) ? (int)Math.Round((double)fbAge.Value * (double)maxTenths / (double)fbAge.MaxValue) : 0;
			fbAge.MaxValue = maxTenths;
			fbAge.Value = Math.Min(value, maxTenths);
		}
		int x = display.X + 90;
		int y = buttonYPos - 86;
		Window.Add(lblAge);
		lblAge.Position = new Point(x, y + 2);
		Window.Add(fbAge);
		fbAge.Position = new Point(lblAge.Right + 6, y);
		fbAge.Width = display.Right - fbAge.X - 12;
		fbAge.UpdateSliderPosition();
	}

	/// <summary>
	/// PORT: the age the slider gives a plant, in years; null for a random one.
	/// </summary>
	public static float? AgeFromSlider(int tenths)
	{
		return (tenths > 0) ? new float?(0.1f * (float)tenths) : null;
	}

	private void UpdateCheckBoxes()
	{
		foreach (KeyValuePair<string, CheckBox> option in options)
		{
			Window.Remove(option.Value);
		}
		options.Clear();
		Window.Remove(lblAge);
		Window.Remove(fbAge);
		if (intface.EntitiesBeingPlaced != null && intface.EntitiesBeingPlaced.Count > 0)
		{
			Entity entity = intface.EntitiesBeingPlaced[0].Entity;
			options.Add(cbFlipHorizontally.Text, cbFlipHorizontally);
			if (entity.EntityType.TreeType != null)
			{
				ShowAgeSlider(entity.EntityType.TreeType);
			}
			else if (entity.EntityType.BiologicalType != null)
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

	private void PopulateGrid()
	{
		outerGrid.BeginAddingEntries();
		terrainGrid.BeginAddingEntries();
		// PORT: listed in order (EditorListOrder).
		foreach (EntityType terrainFeatureType in EditorListOrder(GameData.Instance.AllTerrainFeatureTypes.Values.Where((EntityType t) => t.TerrainType.CanBeMapEditorPlaced)))
		{
			terrainGrid.AddEntry(terrainFeatureType, terrainFeatureType.Name);
		}
		terrainGrid.EndAddingEntries();
		treeGrid.BeginAddingEntries();
		foreach (EntityType treeType in EditorListOrder(GameData.Instance.AllTreeTypes.Values))
		{
			treeGrid.AddEntry(treeType, treeType.Name);
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

	/// <summary>
	/// PORT: the order of the Map Editor's asset lists - by name, numbers by value (rockwall2
	/// before rockwall10), each group of terrain together (EditorListGroup). The studio's was the
	/// order the loaders made them in (Kastuk, 2026-10-11: "sort assets ... by alphabet inside
	/// their subcategories (like all deposits must stay nearby)").
	/// </summary>
	public static List<EntityType> EditorListOrder(IEnumerable<EntityType> types)
	{
		return types.OrderBy(EditorListGroup).ThenBy((EntityType t) => t.Name ?? t.KeyName, NaturalOrder.Instance).ToList();
	}

	/// <summary>
	/// PORT: the asset types carry no category, so the groups are read off the studio's keys and
	/// names: places a job or a building needs, deposits, ambient sounds ("S: "), everything
	/// else, and ground decals (keys ending "_g").
	/// </summary>
	public static int EditorListGroup(EntityType type)
	{
		if (type.TerrainType == null)
		{
			return 0;
		}
		string key = type.KeyName;
		if (key.EndsWith("Terminal") || key.Contains("Spot"))
		{
			return 0;
		}
		if (key.EndsWith("Deposit"))
		{
			return 1;
		}
		if (type.Name != null && type.Name.StartsWith("S: "))
		{
			return 2;
		}
		if (key.EndsWith("_g"))
		{
			return 4;
		}
		return 3;
	}

	/// <summary>
	/// PORT: compares names ignoring case, a run of digits by its value.
	/// </summary>
	private sealed class NaturalOrder : IComparer<string>
	{
		public static readonly NaturalOrder Instance = new NaturalOrder();

		public int Compare(string a, string b)
		{
			int i = 0;
			int j = 0;
			while (i < a.Length && j < b.Length)
			{
				if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
				{
					int startA = i;
					int startB = j;
					while (i < a.Length && char.IsDigit(a[i]))
					{
						i++;
					}
					while (j < b.Length && char.IsDigit(b[j]))
					{
						j++;
					}
					string numberA = a.Substring(startA, i - startA).TrimStart('0');
					string numberB = b.Substring(startB, j - startB).TrimStart('0');
					int byValue = (numberA.Length != numberB.Length) ? numberA.Length.CompareTo(numberB.Length) : string.CompareOrdinal(numberA, numberB);
					if (byValue != 0)
					{
						return byValue;
					}
				}
				else
				{
					int byLetter = char.ToLowerInvariant(a[i]).CompareTo(char.ToLowerInvariant(b[j]));
					if (byLetter != 0)
					{
						return byLetter;
					}
					i++;
					j++;
				}
			}
			return (a.Length - i).CompareTo(b.Length - j);
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
				// PORT: the AGE slider's age (InitAgeSlider), saved as AgeInYears
				// (MapManager.CreateEntityDataFromEntity); random when it is at the left.
				float? age = AgeFromSlider(fbAge.Value);
				if (age.HasValue)
				{
					c.SetAgePreInit(age.Value);
				}
				else
				{
					c.SetAgePreInit();
				}
				editorData.TreeAgeGroup = null;
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
			// PORT: its crops, as a loaded map gives them (MapLoader.SetDefaultCrops).
			UWGame.SimSide.Maps.MapEditor.MapLoader.SetDefaultCrops(entity);
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

	/// <summary>
	/// PORT: nothing chosen and nothing following the pointer, for a change of editor mode
	/// (RosterAccessPanel.SwitchEditorMode). The chosen asset stayed chosen in Paint terrain
	/// properties, so a click painted and placed at once (Kastuk, 2026-10-09).
	/// </summary>
	public void ResetTools()
	{
		The.InGameUI.DestroyEntityBeingPlaced();
		terrainGrid.SelectedIndex = -1;
		treeGrid.SelectedIndex = -1;
		SelectedEntityType = null;
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
		ShowSaveMapDialog();
	}

	/// <summary>
	/// PORT: also what the menu's SAVE MAP opens in the Map Editor (InGameMenuDialog).
	/// </summary>
	public void ShowSaveMapDialog()
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
