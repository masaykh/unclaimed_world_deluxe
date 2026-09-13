using System;
using System.IO;
using System.Text;
using GameStateManagement;
using Steamworks;

namespace UWGame.Steam;

public class SteamManager
{
	private static bool s_EverInialized;

	private bool isInitialized;

	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	public bool IsInitialized => isInitialized;

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Console.WriteLine(pchDebugText);
	}

	public bool Initialize()
	{
		// PORT DEVIATION 4 (see PORTING-NOTES.md).
		// Controller.Initialize calls this unconditionally and unguarded. The original was safe
		// because Steamworks.NET 7.0.0.0 and the shipped CSteamworks/steam_api64.dll were built
		// together. We now use current Steamworks.NET, which P/Invokes flat-API exports that
		// only exist in newer steam_api64.dll builds - so a native/managed version mismatch
		// surfaces as EntryPointNotFoundException rather than as SteamAPI.Init() returning
		// false, and would take out game startup.
		//
		// The game already degrades correctly when Steam is unavailable ("Achievements will not
		// be available"), so funnel native failures into that same path instead. This also
		// makes the game runnable with no Steam client at all, which the port needs for testing.
		try
		{
			return InitializeCore();
		}
		catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
		{
			isInitialized = false;
			Console.WriteLine("[Steamworks.NET] Steam native library unavailable or incompatible: " + ex.Message);
			UnclaimedWorld.LogError(
				ex.GetType().Name + ": " + ex.Message + Environment.NewLine + Environment.NewLine + Report(),
				"Steam: the native library could not be loaded - achievements are off");
			return false;
		}
	}

	/// <summary>
	/// Everything known about this installation's Steam setup, as one block.
	///
	/// WHY. The failure a player sees is one line at the bottom of the main menu - "Steam has not
	/// initialized correctly" - and until now that was also all the game knew. Three quite
	/// different things produce it: the Steam client is not running, steam_api64.dll is missing or
	/// is a build too old for the flat exports Steamworks.NET P/Invokes, or the appid is not
	/// visible to the API. They need three different answers, and the difference is cheap to
	/// record but impossible to guess afterwards.
	///
	/// Written to Errors.txt on failure only. It is diagnostic detail, not news, and a line in
	/// Errors.txt on a healthy launch trains people to ignore the file.
	/// </summary>
	private static string Report()
	{
		StringBuilder text = new StringBuilder();
		string dir = Directory.GetCurrentDirectory();
		text.AppendLine("  working directory : " + dir);
		text.AppendLine("  steam running     : " + SafeIsSteamRunning());

		// The native library, which is the half of the pair that goes wrong. Steamworks.NET
		// P/Invokes flat-API exports that only exist in newer builds, so a steam_api64.dll from
		// the shipped 2016 game loads and then fails to bind - the size and timestamp are what
		// tell an old one from a current one at a glance.
		AppendFile(text, "  steam_api64.dll   : ", Path.Combine(dir, "steam_api64.dll"));
		AppendFile(text, "  steam_api.dll     : ", Path.Combine(dir, "steam_api.dll"));

		// steam_appid.txt is how the API knows which game it is when the exe was not launched by
		// Steam. Without it, and outside Steam, Init fails with no obvious cause.
		string appid = Path.Combine(dir, "steam_appid.txt");
		if (File.Exists(appid))
		{
			string content;
			try
			{
				content = File.ReadAllText(appid).Trim();
			}
			catch (Exception ex)
			{
				content = "unreadable: " + ex.Message;
			}
			text.AppendLine("  steam_appid.txt   : " + content);
		}
		else
		{
			text.AppendLine("  steam_appid.txt   : ABSENT - required when the game is not started by Steam");
		}

		text.AppendLine("  Steamworks.NET    : " + SafeAssemblyVersion());
		text.AppendLine("  Packsize.Test()   : " + SafeTest(() => Packsize.Test()));
		text.AppendLine("  DllCheck.Test()   : " + SafeTest(() => DllCheck.Test()));
		text.Append("  If Steam is not running, that is the whole answer. If it is, and the DLL is ");
		text.Append("the one that shipped with the game in 2016, it is too old for this build - ");
		text.Append("build/12-fetch-steam-natives.sh fetches a matching one.");
		return text.ToString();
	}

	private static void AppendFile(StringBuilder text, string label, string path)
	{
		if (!File.Exists(path))
		{
			text.AppendLine(label + "absent");
			return;
		}
		try
		{
			FileInfo info = new FileInfo(path);
			text.AppendLine(label + info.Length + " bytes, written " +
				info.LastWriteTimeUtc.ToString("yyyy-MM-dd") + " UTC");
		}
		catch (Exception ex)
		{
			text.AppendLine(label + "present, but unreadable: " + ex.Message);
		}
	}

	/// <summary>
	/// IsSteamRunning goes through the same native library that may be the thing that is broken,
	/// so it gets the same treatment as everything else here: a failure is an answer, not an
	/// exception thrown out of the diagnostics.
	/// </summary>
	private static string SafeIsSteamRunning()
	{
		try
		{
			return SteamAPI.IsSteamRunning() ? "yes" : "NO - start Steam, or accept that achievements are off";
		}
		catch (Exception ex)
		{
			return "could not ask (" + ex.GetType().Name + ")";
		}
	}

	private static string SafeTest(Func<bool> test)
	{
		try
		{
			return test() ? "pass" : "FAIL";
		}
		catch (Exception ex)
		{
			return "threw " + ex.GetType().Name;
		}
	}

	private static string SafeAssemblyVersion()
	{
		try
		{
			return typeof(SteamAPI).Assembly.GetName().Version?.ToString() ?? "unknown";
		}
		catch (Exception ex)
		{
			return "unknown (" + ex.GetType().Name + ")";
		}
	}

	private bool InitializeCore()
	{
		if (s_EverInialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		if (!Packsize.Test())
		{
			Console.WriteLine("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Console.WriteLine("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}

		// InitEx rather than Init. Both do the same thing; InitEx also hands back Valve's own
		// reason - k_ESteamAPIInitResult_NoSteamClient, VersionMismatch, FailedGeneric - and a
		// message string written by the API itself. Init() collapses all of that into false,
		// which is how "Steam has not initialized correctly" came to be everything the game knew.
		ESteamAPIInitResult result = SteamAPI.InitEx(out string steamErrMsg);
		isInitialized = result == ESteamAPIInitResult.k_ESteamAPIInitResult_OK;
		if (!isInitialized)
		{
			Console.WriteLine("[Steamworks.NET] SteamAPI_InitEx() failed: " + result + " - " + steamErrMsg, this);
			UnclaimedWorld.LogError(
				result + ": " + (string.IsNullOrWhiteSpace(steamErrMsg) ? "(no message)" : steamErrMsg.Trim())
					+ Environment.NewLine + Environment.NewLine + Report(),
				"Steam did not initialise - achievements are off");
			return false;
		}
		s_EverInialized = true;
		m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
		SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		return true;
	}

	public void Destroy()
	{
		if (isInitialized)
		{
			SteamAPI.Shutdown();
		}
	}

	public void Update()
	{
		if (isInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}
}
