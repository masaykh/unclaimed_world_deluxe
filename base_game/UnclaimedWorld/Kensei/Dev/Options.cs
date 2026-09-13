using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;

namespace Kensei.Dev;

public static class Options
{
	public enum DebugButton
	{
		MakeAButton
	}

	[Serializable]
	public class OptionNotFoundException : Exception
	{
		public OptionNotFoundException()
		{
		}

		public OptionNotFoundException(string s)
			: base(s)
		{
		}

		public OptionNotFoundException(string s, Exception e)
			: base(s, e)
		{
		}

		protected OptionNotFoundException(SerializationInfo info, StreamingContext cxt)
			: base(info, cxt)
		{
		}
	}

	public enum BehaviourIfNotPresent
	{
		ReturnFalse,
		ReturnTrue,
		Throw
	}

	public delegate void OptionChangedFunction(string option, bool? newBool, float? newFloat);

	// PORT DEVIATION 8 (see PORTING-NOTES.md).
	// The 827-line `private class OptionsForm : Form` that used to live here is replaced by
	// this inert stub. It was the WinForms/GDI+ developer tweak panel - TabControl, a TrackBar
	// per float option, owner-drawn ListBox items via System.Drawing, copy-to-Clipboard - and
	// it was UNREACHABLE in the shipped game: the only caller of Options.CreateDialog is
	// Client.InitDeveloperDialog, which itself has zero callers, so s_form was always null.
	// Every public Options member already guards on `s_form != null`, which is why all the
	// SetOption calls were pure dictionary writes and GetOption always returned false.
	//
	// Keeping the nested type with the same private member names - rather than editing the
	// outer class - means not one of the ~360 Kensei.Dev call sites changes, and s_form still
	// stays null, so behaviour is bit-identical to retail. What it buys is ~1,000 lines of
	// WinForms + GDI+ removed from the code base, which the DesktopGL build requires:
	// Options.cs alone accounted for 68 of its 160 compile errors.
	private sealed class OptionsForm
	{
		internal void AddButton(string option, EntityType type) { }

		internal void AddOption(string option) { }

		internal void AddOption(string option, bool value) { }

		internal void AddOption(string option, float value, float min, float max) { }

		internal void UpdateOption(string option, bool value) { }

		internal void UpdateOption(string option, float value) { }

		internal void RemoveOption(string option) { }

		internal void AppendEventsLogText(string text) { }

		internal void AppendPopSpawnText(string text) { }

		internal void PopulateTestEvents(List<ActionSets> actions) { }

		internal void PopulatePolledTestEvents(List<PolledEventType> actions) { }

		internal void SetAllegiancesText(string text) { }

		internal void SetEmigrateRollText(string text) { }

		internal void SetEntityGoalsText(string text) { }

		internal void SetEntityInfotext(string text) { }

		internal void SetEventsText(string text) { }

		internal void SetJobsText(string text) { }

		internal void SetPerformanceText(string text) { }

		internal void SetSoundsText(string text) { }

		internal string ShownTab() => null;

		internal void Dispose() { }
	}

	private static Dictionary<string, EntityType> s_optionsEntityTypes = new Dictionary<string, EntityType>();

	private static Dictionary<string, DebugButton> s_optionsButtons = new Dictionary<string, DebugButton>();

	private static Dictionary<string, bool> s_optionsBool = new Dictionary<string, bool>();

	private static Dictionary<string, float> s_optionsFloat = new Dictionary<string, float>();

	private static Dictionary<string, OptionChangedFunction> s_callbacks = new Dictionary<string, OptionChangedFunction>();

	private static OptionsForm s_form;

	// Was `public static Form Form`; the WinForms type is gone and nothing outside Kensei.Dev
	// ever read this. Always null, as it was in retail.
	public static object Form => s_form;

	internal static void Initialise()
	{
		s_optionsBool = new Dictionary<string, bool>();
		s_callbacks = new Dictionary<string, OptionChangedFunction>();
	}

	public static void SetOption(string optionName, DebugButton butt, bool doCallback = false)
	{
		if (s_form != null && !s_optionsButtons.ContainsKey(optionName))
		{
			s_form.AddOption(optionName);
		}
		s_optionsButtons[optionName] = butt;
		if (doCallback)
		{
			if (s_callbacks.ContainsKey(optionName))
			{
				s_callbacks[optionName](optionName, null, null);
			}
			else if (optionName.StartsWith("ItemTypes") || optionName.StartsWith("StructureTypes") || optionName.StartsWith("EntityTypes") || optionName.StartsWith("TerrainTypes"))
			{
				int num = optionName.IndexOf(".", StringComparison.Ordinal);
				string typeKey = optionName.Substring(num + 1, optionName.Length - (num + 1));
				The.InGameUI.SelectNextEntityOfType(typeKey);
			}
		}
	}

	public static void SetOption(string optionName, EntityType type, bool doCallback = false)
	{
		if (s_form != null && !s_optionsEntityTypes.ContainsKey(optionName))
		{
			s_form.AddButton(optionName, type);
		}
		s_optionsButtons[optionName] = DebugButton.MakeAButton;
		if (doCallback && s_callbacks.ContainsKey(optionName))
		{
			s_callbacks[optionName](optionName, null, null);
		}
	}

	public static void SetOption(string optionName, bool boolToSet, bool doCallback = false)
	{
		if (s_form != null)
		{
			if (!s_optionsBool.ContainsKey(optionName))
			{
				s_form.AddOption(optionName, boolToSet);
			}
			else
			{
				s_form.UpdateOption(optionName, boolToSet);
			}
		}
		s_optionsBool[optionName] = boolToSet;
		if (doCallback && s_callbacks.ContainsKey(optionName))
		{
			s_callbacks[optionName](optionName, boolToSet, null);
		}
	}

	public static void SetOption(string optionName, float val, float min, float max, bool doCallback = false)
	{
		if (s_form != null)
		{
			if (!s_optionsFloat.ContainsKey(optionName))
			{
				s_form.AddOption(optionName, val, min, max);
			}
			else
			{
				s_form.UpdateOption(optionName, val);
			}
		}
		s_optionsFloat[optionName] = val;
		if (doCallback && s_callbacks.ContainsKey(optionName))
		{
			s_callbacks[optionName](optionName, null, val);
		}
	}

	public static void SetEntityInfoText(string text)
	{
		if (s_form != null)
		{
			s_form.SetEntityInfotext(text);
		}
	}

	public static void SetEntityGoalsText(string text)
	{
		if (s_form != null)
		{
			s_form.SetEntityGoalsText(text);
		}
	}

	public static void SetEventsText(string text)
	{
		if (s_form != null)
		{
			s_form.SetEventsText(text);
		}
	}

	public static void SetJobsText(string text)
	{
		if (s_form != null)
		{
			s_form.SetJobsText(text);
		}
	}

	public static void SetPerformanceText(string text)
	{
		if (s_form != null)
		{
			s_form.SetPerformanceText(text);
		}
	}

	public static void SetEmigrateRollText(string text)
	{
		if (s_form != null)
		{
			s_form.SetEmigrateRollText(text);
		}
	}

	public static void SetAllegiancesText(string text)
	{
		if (s_form != null)
		{
			s_form.SetAllegiancesText(text);
		}
	}

	public static void PopulateTestEvents(List<ActionSets> actions)
	{
		if (s_form != null)
		{
			s_form.PopulateTestEvents(actions);
		}
	}

	public static void PopulatePolledTestEvents(List<PolledEventType> actions)
	{
		if (s_form != null)
		{
			s_form.PopulatePolledTestEvents(actions);
		}
	}

	public static bool AppendEventsLogText(string text)
	{
		if (s_form != null)
		{
			s_form.AppendEventsLogText(text);
			return true;
		}
		return false;
	}

	public static bool AppendPopSpawnText(string text)
	{
		if (s_form != null)
		{
			s_form.AppendPopSpawnText(text);
			return true;
		}
		return false;
	}

	public static void SetSoundsText(string text)
	{
		if (s_form != null)
		{
			s_form.SetSoundsText(text);
		}
	}

	public static void SetOptionCallback(string optionName, OptionChangedFunction callback)
	{
		s_callbacks[optionName] = callback;
	}

	public static string ShownTab()
	{
		if (s_form != null)
		{
			return s_form.ShownTab();
		}
		return null;
	}

	public static void RemoveOption(string name)
	{
		if (s_optionsBool.ContainsKey(name))
		{
			s_optionsBool.Remove(name);
			s_form.RemoveOption(name);
		}
		if (s_optionsButtons.ContainsKey(name))
		{
			s_optionsButtons.Remove(name);
			s_form.RemoveOption(name);
		}
		if (s_optionsEntityTypes.ContainsKey(name))
		{
			s_optionsEntityTypes.Remove(name);
			s_form.RemoveOption(name);
		}
		if (s_optionsFloat.ContainsKey(name))
		{
			s_optionsFloat.Remove(name);
			s_form.RemoveOption(name);
		}
	}

	public static void RemoveOptionsStartingWith(string startsWith)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, bool> item in s_optionsBool)
		{
			if (item.Key.StartsWith(startsWith))
			{
				list.Add(item.Key);
			}
		}
		foreach (KeyValuePair<string, DebugButton> s_optionsButton in s_optionsButtons)
		{
			if (s_optionsButton.Key.StartsWith(startsWith))
			{
				list.Add(s_optionsButton.Key);
			}
		}
		foreach (KeyValuePair<string, EntityType> s_optionsEntityType in s_optionsEntityTypes)
		{
			if (s_optionsEntityType.Key.StartsWith(startsWith))
			{
				list.Add(s_optionsEntityType.Key);
			}
		}
		foreach (KeyValuePair<string, float> item2 in s_optionsFloat)
		{
			if (item2.Key.StartsWith(startsWith))
			{
				list.Add(item2.Key);
			}
		}
		foreach (string item3 in list)
		{
			RemoveOption(item3);
		}
	}

	public static void SetOptionsStartingWith(string startsWith, bool value, bool doCallback)
	{
		new List<string>();
		foreach (KeyValuePair<string, bool> item in s_optionsBool)
		{
			if (item.Key.StartsWith(startsWith))
			{
				SetOption(item.Key, value, doCallback);
			}
		}
	}

	public static void RemoveAllOptions()
	{
		s_callbacks = null;
	}

	public static bool GetOption(string name, BehaviourIfNotPresent ifNotPresent)
	{
		bool value = false;
		if (s_optionsBool.TryGetValue(name, out value))
		{
			return value;
		}
		bool flag = false;
		flag = ifNotPresent switch
		{
			BehaviourIfNotPresent.Throw => throw new OptionNotFoundException(), 
			BehaviourIfNotPresent.ReturnTrue => true, 
			_ => false, 
		};
		SetOption(name, flag);
		return flag;
	}

	public static bool GetOption(string name)
	{
		return GetOption(name, BehaviourIfNotPresent.ReturnFalse);
	}

	// Was: construct OptionsForm, add every bool option to it, Show() it and position it
	// against GraphicsAdapter.DefaultAdapter.CurrentDisplayMode. Now a no-op - see
	// PORT DEVIATION 8. Its ONLY caller, Client.InitDeveloperDialog, has zero callers,
	// so this never ran in the shipped game and s_form was always null. Kept as a
	// method so the (unreachable) call site still compiles.
	public static void CreateDialog()
	{
	}

	public static void Destroy()
	{
		if (s_form != null)
		{
			s_form.Dispose();
			s_form = null;
			s_optionsEntityTypes.Clear();
			s_optionsButtons.Clear();
			s_optionsBool.Clear();
			s_optionsFloat.Clear();
			if (s_callbacks != null)
			{
				s_callbacks.Clear();
			}
		}
	}
}
