namespace UWGame.Mods;

/// <summary>
/// The main menu mod, compiled OUT. See PeatMod.Absent.cs for the pattern. MODDING's MAP EDITOR
/// and TEST SCENE stay greyed, and no test map is revealed.
/// </summary>
public static class MainMenuMod
{
    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "mainmenu";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>Always false: the editor buttons come with the mod.</summary>
    public const bool EditorButtons = false;

    /// <summary>Always false: without the mod there is no TEST SCENE to open one.</summary>
    public const bool RevealsTestMap = false;
}
