using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Resources;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class SetTileResourcesWindow : HUDWindow
{
	protected int itemHeight = 18;

	private Grid outerGrid;

	private FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

	private ClickHandler resourceClickHandler;

	private SetTileResourcesPopup popup;

	public SetTileResourcesWindow()
		: base(240, 200, hasSurface: true, hasCloseButton: false, isMovable: false, "HUD_window_base", hideWhenMouseExits: false, Level.Bottom)
	{
		popup = new SetTileResourcesPopup();
		SetSummaryDelegate = SidePanelEntity.SetSummaryAsTotal;
		resourceClickHandler = ResourceClicked;
		outerGrid = HUDWindow.CreateOuterGridForCollapsableLists(The.InGameUI.gui, DisplayWindow.ViewPort);
		TextButton textButton = new TextButton(gui);
		Add(textButton);
		textButton.Text = UWGame.Locale.Text("Clear");
		textButton.Init(TextButton.TextButtonType.HUD);
		textButton.Click += btClear_Click;
		textButton.ScaleWidthToFitText();
		// PORT: top right, over the list's heading row rather than over the resource names at the
		// bottom (Kastuk, 2026-10-09: "let the button Clear be moved into upper right corner to
		// not be drawn over names of resources"). The studio's place was X 10, Y 162.
		textButton.Y = 6;
		textButton.X = DisplayWindow.Width - textButton.Width - 10;
	}

	private void btClear_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.SelectedTiles.IterateArea(delegate(TerrainTile tile)
		{
			tile.DesignerPlacedResources = null;
			if (tile.TreesOnTile != null)
			{
				foreach (Entity item in tile.TreesOnTile)
				{
					if (item.Find<EditorData>(out var c))
					{
						c.Resources = null;
					}
				}
			}
		});
		Populate();
	}

	public void ClearResource(ResourceType resourceType)
	{
		The.InGameUI.SelectedTiles.IterateArea(delegate(TerrainTile tile)
		{
			if (resourceType.TileResourceType != null)
			{
				if (tile.DesignerPlacedResources != null)
				{
					tile.DesignerPlacedResources = tile.DesignerPlacedResources.Where((Resource r) => r.KeyName != resourceType.KeyName).ToArray();
				}
			}
			else if (tile.TreesOnTile != null)
			{
				foreach (Entity item in tile.TreesOnTile)
				{
					if (item.Find<EditorData>(out var c))
					{
						c.Resources = c.Resources?.Where((Resource r) => r.KeyName != resourceType.KeyName).ToArray();
					}
				}
			}
		});
		Populate();
	}

	public new void Hide()
	{
		DisplayWindow.Hide();
		popup.DisplayWindow.Hide();
	}

	public void ResourceClicked(UIComponent sender, EventArgs eventArgs)
	{
		string keyName = ((IGameData)((IGameDataButtonEventArgs)eventArgs).Item).KeyName;
		popup.DisplayWindow.Y = sender.AbsolutePosition.Y;
		popup.DisplayWindow.X = sender.AbsolutePosition.X + 16;
		Resource resourceData = null;
		GetDesignerResourceDataInArea(The.InGameUI.SelectedTiles, keyName, out resourceData);
		popup.Fill(GameData.Instance.AllResourceTypes[keyName], resourceData);
		popup.DisplayWindow.Show();
	}

	public void SaveResourceChanges(ResourceType resourceType, int? min, int? max, int? modifier)
	{
		bool resourceWasSet = false;
		The.InGameUI.SelectedTiles.IterateArea(delegate(TerrainTile terrainTile)
		{
			if (resourceType.TileResourceType != null)
			{
				if (SaveResourceChangesToTile(resourceType, min, max, modifier, terrainTile))
				{
					resourceWasSet = true;
				}
			}
			else if (SaveResourceChangesToTrees(resourceType, min, max, modifier, terrainTile))
			{
				resourceWasSet = true;
			}
		});
		if (resourceWasSet)
		{
			The.Sim.PlaySite.EditorResources.Add(resourceType);
		}
		Populate();
	}

	private static bool SaveResourceChangesToTrees(ResourceType resourceType, int? min, int? max, int? modifier, TerrainTile terrainTile)
	{
		if (terrainTile.TreesOnTile != null)
		{
			List<Entity> list = terrainTile.TreesOnTile.FindAll((Entity t) => t.EntityType.TreeType.CropTypes != null && t.EntityType.TreeType.CropTypes.Contains(resourceType));
			if (list != null)
			{
				foreach (Entity item in list)
				{
					if (!item.Find<EditorData>(out var c))
					{
						continue;
					}
					Resource[] resources = c.Resources;
					Resource resource;
					if (resources != null)
					{
						resource = c.Resources.FirstOrDefault((Resource r) => r.KeyName == resourceType.KeyName);
						if (resource != null)
						{
							resource.MinResourceItems = min;
							resource.MaxResourceItems = max;
							resource.Modifier = modifier;
							continue;
						}
					}
					resource = new Resource(resourceType)
					{
						MinResourceItems = min,
						MaxResourceItems = max,
						Modifier = modifier
					};
					if (resources != null)
					{
						List<Resource> list2 = c.Resources.ToList();
						list2.Add(resource);
						c.Resources = list2.ToArray();
					}
					else
					{
						c.Resources = new Resource[1] { resource };
					}
				}
				return true;
			}
		}
		return false;
	}

	private static bool SaveResourceChangesToTile(ResourceType resourceType, int? min, int? max, int? modifier, TerrainTile terrainTile)
	{
		List<Resource> list = null;
		if (terrainTile.DesignerPlacedResources != null)
		{
			Resource resource = terrainTile.DesignerPlacedResources.FirstOrDefault((Resource r) => r.KeyName == resourceType.KeyName);
			if (resource != null)
			{
				resource.MinResourceItems = min;
				resource.MaxResourceItems = max;
				resource.Modifier = modifier;
			}
			else
			{
				list = terrainTile.DesignerPlacedResources.ToList();
				list.Add(new Resource(resourceType)
				{
					MinResourceItems = min,
					MaxResourceItems = max,
					Modifier = modifier
				});
				terrainTile.DesignerPlacedResources = list.ToArray();
			}
		}
		else
		{
			terrainTile.DesignerPlacedResources = new Resource[1]
			{
				new Resource(resourceType)
				{
					MinResourceItems = min,
					MaxResourceItems = max,
					Modifier = modifier
				}
			};
		}
		return true;
	}

	private void GetDesignerResourceDataInArea(MapArea mapArea, string resourceKeyName, out Resource resourceData)
	{
		resourceData = null;
		Resource foundResourceData = null;
		if (mapArea.Count == 1)
		{
			mapArea.IterateArea(delegate(TerrainTile tile)
			{
				if (tile.DesignerPlacedResources != null)
				{
					Resource resource = tile.DesignerPlacedResources.FirstOrDefault((Resource r) => r.KeyName == resourceKeyName);
					if (resource != null)
					{
						foundResourceData = resource;
					}
				}
				else if (tile.TreesOnTile != null)
				{
					foreach (Entity item in tile.TreesOnTile)
					{
						Resource resource = GetEditorResource(item, resourceKeyName);
						if (resource != null)
						{
							foundResourceData = resource;
							break;
						}
					}
				}
			});
		}
		resourceData = foundResourceData;
	}

	private Resource GetEditorResource(Entity tree, string resourceKeyName)
	{
		Resource result = null;
		if (tree.Find<EditorData>(out var c) && c.Resources != null)
		{
			result = c.Resources.FirstOrDefault((Resource r) => r.KeyName == resourceKeyName);
		}
		return result;
	}

	private void GetApplicableCropResourcesInArea(MapArea mapArea, Dictionary<ResourceType, int> sum)
	{
		mapArea.IterateArea(delegate(TerrainTile tile)
		{
			if (tile.TreesOnTile != null)
			{
				foreach (Entity item in tile.TreesOnTile)
				{
					if (item.EntityType.TreeType.CropTypes != null)
					{
						foreach (ResourceType cropType in item.EntityType.TreeType.CropTypes)
						{
							sum[cropType] = 0;
						}
					}
				}
			}
		});
	}

	public void Populate()
	{
		Dictionary<ResourceType, int> dictionary = new Dictionary<ResourceType, int>();
		GetApplicableCropResourcesInArea(The.InGameUI.SelectedTiles, dictionary);
		foreach (KeyValuePair<string, ResourceType> allResourceType in GameData.Instance.AllResourceTypes)
		{
			if (allResourceType.Value.TileResourceType != null)
			{
				dictionary.Add(allResourceType.Value, 0);
			}
		}
		CountPlacedResourcesInArea(The.InGameUI.SelectedTiles, dictionary);
		FullLCDPanel.PopulateCategoryGrid<ResourceType, int, ResourceCategory>(gui, outerGrid, CollapsablePanel.PanelType.HUD, Label.LabelType.HUDWindow, SetSummaryDelegate, resourceClickHandler, dictionary);
	}

	/// <summary>
	/// PORT: the number beside each resource is how many of the selected tiles (or, for a crop,
	/// trees on them) have it set. The studio's was always 0, so a zone placed earlier could not
	/// be seen by selecting it again (Kastuk, 2026-10-09: "show them in this same window as Clay 1
	/// in place of Clay 0 if there's Clay resource is placed").
	/// </summary>
	private static void CountPlacedResourcesInArea(MapArea mapArea, Dictionary<ResourceType, int> counts)
	{
		void Count(Resource[] placed)
		{
			if (placed == null)
			{
				return;
			}
			foreach (Resource resource in placed)
			{
				if (resource != null && GameData.Instance.AllResourceTypes.TryGetValue(resource.KeyName, out ResourceType type) && counts.ContainsKey(type))
				{
					counts[type]++;
				}
			}
		}
		mapArea.IterateArea(delegate(TerrainTile tile)
		{
			Count(tile.DesignerPlacedResources);
			if (tile.TreesOnTile != null)
			{
				foreach (Entity tree in tile.TreesOnTile)
				{
					if (tree.Find<EditorData>(out var c))
					{
						Count(c.Resources);
					}
				}
			}
		});
	}
}
