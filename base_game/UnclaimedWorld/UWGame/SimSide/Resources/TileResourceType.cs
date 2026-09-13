using System.Linq;
using System.Xml.Serialization;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Resources;

public class TileResourceType
{
	public int MaxFlavours;

	public float MoreSpriteLimit;

	public RenderableType RenderableType;

	public RenderableType EditorRenderableType;

	public RenderableType RenderableTypeMode
	{
		get
		{
			if (The.Sim.Mode == Sim.EngineMode.Game)
			{
				return RenderableType;
			}
			return EditorRenderableType ?? RenderableType;
		}
	}

	[XmlIgnore]
	public bool IsRenderedWithSprites { get; private set; }

	public void Initialize()
	{
		IsRenderedWithSprites = IsRenderedWithSprite();
	}

	public bool IsRenderedWithSprite()
	{
		RenderableType renderableTypeMode = RenderableTypeMode;
		if (renderableTypeMode == null)
		{
			return false;
		}
		if (renderableTypeMode.DefaultClientState == null || (renderableTypeMode.DefaultClientState.RenderAsGroundSpriteType == null && renderableTypeMode.DefaultClientState.RenderAsBillboardType == null))
		{
			if (renderableTypeMode.ClientStateConditions != null)
			{
				return renderableTypeMode.ClientStateConditions.Any((ClientStateInfo s) => s.RenderAsGroundSpriteType != null || s.RenderAsBillboardType != null);
			}
			return false;
		}
		return true;
	}
}
