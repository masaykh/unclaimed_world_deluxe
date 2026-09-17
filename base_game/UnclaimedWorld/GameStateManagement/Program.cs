using System;
using UWGame.Port;
using UWGame.SimSide;

namespace GameStateManagement;

internal static class Program
{
	/// <summary>
	/// PORT DEVIATION 13 (see PORTING-NOTES.md). Was <c>Main()</c>, with no arguments at all.
	///
	/// The game already contains a complete data export/import path: DataLoader wraps every one
	/// of its ~80 hard-coded tables in <c>HandleDataTypeList</c> / <c>HandleDataTypeObject</c>,
	/// which consult <see cref="Sim.CurrentSerializeMode"/> and will write each table out to
	/// <c>data/BaseData/&lt;name&gt;.xml</c> and read it back. But
	/// <c>Sim.CurrentSerializeMode</c> is initialised to <c>NoSerialize</c> and **nothing in
	/// the shipped game ever assigns it**, so none of it runs and the folders stay empty. That
	/// is why the retail install has a <c>data/BaseData</c> containing only Strings.
	///
	/// These two flags are the switch the studio never wired up. Default behaviour is
	/// unchanged - no flags means <c>NoSerialize</c>, exactly as retail.
	/// </summary>
	private static bool ApplyDataFlags(string[] args)
	{
		foreach (string arg in args)
		{
			switch (arg)
			{
				case "--export-data":
					// Writes every hard-coded table to data/BaseData/ (relative to the game
					// folder - see Config.GetDataFolderPath) and then loads from those files,
					// so what you get is the data the game actually ran with.
					Sim.CurrentSerializeMode = Sim.SerializeMode.WriteAndRead;
					Console.WriteLine("--export-data: writing the built-in data tables to data/BaseData/ and loading from them.");
					break;

				case "--data-from-xml":
					// Skips the hard-coded initialisers and loads only what is on disk. This is
					// the mode a data mod wants: edit the exported XML, then run with this.
					Sim.CurrentSerializeMode = Sim.SerializeMode.Read;
					Console.WriteLine("--data-from-xml: loading data tables from data/ only; built-in defaults skipped.");
					break;

				case "--help":
				case "-h":
				case "/?":
					Console.WriteLine("UnclaimedWorld - .NET 8 / MonoGame port");
					Console.WriteLine();
					Console.WriteLine("  --export-data     Write the built-in data tables to data/BaseData/*.xml,");
					Console.WriteLine("                    then load from them. Run this once to get editable copies.");
					Console.WriteLine("  --data-from-xml   Load data tables from data/ only, ignoring the built-in");
					Console.WriteLine("                    defaults. Use after editing the exported XML.");
					Console.WriteLine("  -nomods           Run the stock game: the bundled Unhidden Mod off AND no");
					Console.WriteLine("                    mods loaded from user/Mods. Saves made with mods on may");
					Console.WriteLine("                    not load without them.");
					Console.WriteLine();
					Console.WriteLine("Data loading with no flags is exactly as retail: the built-in tables are");
					Console.WriteLine("used and nothing under data/ is written or read.");
					Console.WriteLine();
					Console.WriteLine("This build BUNDLES the community Unhidden Mod, ON by default. For retail");
					Console.WriteLine("behaviour in full, run with -nomods. See MODDING.md.");
					return false;

				// Anything else is ignored - Steam passes arguments of its own.
			}
		}
		return true;
	}

	[STAThread]
	private static void Main(string[] args)
	{
		if (!ApplyDataFlags(args))
		{
			return;
		}
		// -nomods gives the stock game: it switches off both the bundled Unhidden Mod and the
		// third-party mod loader. Parsed before anything else because content hooks and Harmony
		// patches on the data loaders run during data loading, which starts inside the
		// UnclaimedWorld constructor below - after that it is too late to switch off.
		foreach (string arg in args)
		{
			if (string.Equals(arg, "-nomods", StringComparison.OrdinalIgnoreCase))
			{
				UWGame.Mods.UnhiddenMod.Disable();
				UWGame.Mods.ModLoader.Disable();
			}
		}
		// Mod configuration, from user/ModSettings.xml. Read BEFORE anything registers a setting
		// and long before the first data table is built, because a switch that shapes the tables -
		// the bundled mod's peat recipe is one - is read while they are being assembled. The
		// port's own entries are registered either way; the bundled mod's only when it is present
		// and on, so -nomods leaves no trace of it in the file or the menu.
		UWGame.Mods.ModSettings.Load(UnclaimedWorld.LogError);
		UWGame.Mods.PortSettings.RegisterSettings();
		// The mods that ship as their own file, each answering one request. Always registered:
		// unlike the bundled Unhidden Mod they are not a build-time feature, so their switches are
		// in the file and the menu whatever the build, and each is off with one click.
		UWGame.Mods.MapEdgeMod.RegisterSettings();
		UWGame.Mods.HealingMod.RegisterSettings();
		UWGame.Mods.SelfPreservationMod.RegisterSettings();
		UWGame.Mods.MagnificationMod.RegisterSettings();
		UWGame.Mods.BalancedDietMod.RegisterSettings();
		UWGame.Mods.DisassemblyMod.RegisterSettings();
		UWGame.Mods.DebugMod.RegisterSettings();
		UWGame.Mods.StateDumpMod.RegisterSettings();
		UWGame.Mods.AgentMod.RegisterSettings();
		if (UWGame.Mods.UnhiddenMod.Enabled)
		{
			UWGame.Mods.UnhiddenMod.RegisterSettings();
			// AFTER the settings are loaded, because the culture is now a choice with the studio's
			// own default rather than something the mod imposes. Nothing between here and the
			// startup above reads Config.Culture - it is display formatting, and the first thing
			// to use it is the window this method goes on to create.
			UWGame.Mods.UnhiddenMod.ApplyCulture();
		}

		// Third-party mods from user/Mods. Before the game object is constructed, so a mod can
		// patch anything the game does on the way up - including the data loaders, which run
		// during the first scenario load. Failures are per-mod and reported to Errors.txt.
		UWGame.Mods.ModLoader.LoadAll(UnclaimedWorld.LogError);
		// PORT DEVIATION 10 (see PORTING-NOTES.md). Was an inline HKEY_LOCAL_MACHINE probe for
		// Windows Media Player followed by a WinForms MessageBox listing per-Windows-version
		// Media Feature Pack download links. Moved behind PlatformWindow so the OpenGL backend,
		// which does not use MediaFoundation at all, does not gate startup on a Windows
		// registry key. The original's five download URLs are all long dead, so the message
		// now points at Microsoft's current Media Feature Pack support page instead.
		string codecProblem = PlatformWindow.CheckMediaCodecsAvailable();
		if (codecProblem != null)
		{
			PlatformWindow.ShowErrorDialog(IntPtr.Zero, codecProblem, "Missing Windows Media Player");
			return;
		}

		// PORT DEVIATION 18 (see PORTING-NOTES.md). Reports up front, and in words that say what
		// to do, when the shaders this installation would load are still the shipped MGFX v8 -
		// which happens when the binaries were copied by hand without port-content\.
		//
		// Otherwise MonoGame raises "This MGFX effect is for an older release of MonoGame" from
		// inside content loading, naming no file and offering no remedy, several screens in.
		//
		// It resolves the effect the way UwContentManager does, override first, so a correct
		// installation - whose Content\ shaders are deliberately still v8 - does not trip it.
		string effectProblem = EffectVersionCheck.Check();
		if (effectProblem != null)
		{
			PlatformWindow.ShowErrorDialog(IntPtr.Zero, effectProblem, "Shaders not set up");
			return;
		}
		using UnclaimedWorld unclaimedWorld = new UnclaimedWorld();
		unclaimedWorld.Exiting += game_Exiting;
		// NOTE: game 1.0.4.8 DELETED the target-monitor placement that used to be here, so the
		// port follows it and no longer calls PlatformWindow.PlaceWindowOnMonitor. The helper is
		// kept (PORT DEVIATION 10 still covers the codec dialog above), so restoring the
		// behaviour is one line if Options.TargetMonitorNumber is wanted back.
		try
		{
			unclaimedWorld.Run();
		}
		catch (Exception e)
		{
			unclaimedWorld.HandleExceptionInReleaseMode(e, isMainThread: true);
		}
	}

	// NOTE: `windowForm_Load` used to sit here - 13 lines of WinForms that positioned the
	// window on `Screen.AllScreens[1]` with a hardcoded monitor index of 2. It had no
	// subscribers at all, so it never ran. Removed with the rest of the WinForms surface.

	private static void game_Exiting(object sender, EventArgs e)
	{
		(sender as UnclaimedWorld).Controller.Destroy();
	}

	private static void OnProcessExit(object sender, EventArgs e)
	{
	}
}
