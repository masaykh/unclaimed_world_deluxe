using System.Collections.Generic;

namespace SpriteSheetRuntime;

/// <summary>
/// PORT DEVIATION 16 (see PORTING-NOTES.md). RETIRED but deliberately kept.
///
/// Game 1.0.4.8 deleted this class and now stores LightSources as a plain SpriteSheet. Nothing
/// in the port references it any more - GameData types the field as SpriteSheet, as the game
/// itself does. It is retained solely so that an OLDER LightSources.xnb still loads: those
/// files name this type in their ReflectiveReader, and with the type absent the content reader
/// cannot resolve it at all. Keeping it costs one class and makes the port work against either
/// game version's assets.
/// </summary>
public class LightSourceSpriteSheet : SpriteSheet
{
	public Dictionary<string, LightSourceType> AllLightSourceData;
}
