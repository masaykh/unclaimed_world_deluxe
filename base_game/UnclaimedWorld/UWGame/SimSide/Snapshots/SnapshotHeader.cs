using System;
using GameStateManagement;
using UWGame.Mods;
using UWGame.SimSide.Scenarios;

namespace UWGame.SimSide.Snapshots;

public class SnapshotHeader : ISnapshot
{
	public StartGameParams StartGameParams;

	public Version ProgramVersion;

	public DateTime Timestamp;

	/// <summary>
	/// What modded content this save was made with - <see cref="ModSettings.Signature"/>'s output,
	/// or "" for a stock game. Written since <see cref="Snapshotter.Version.ModsRecorded"/>; a save
	/// from before that reads back as "" and is treated as stock, which is what it almost always
	/// is and, either way, is the only thing that can be said about it.
	///
	/// It is in the HEADER rather than the body on purpose: the save list reads headers only, so
	/// every save can be marked as modded without loading a single game.
	/// </summary>
	public string Mods = "";

	/// <summary>
	/// The version WRITTEN is the newest; the version READ is whatever the file says. Every field
	/// added after Original has to be guarded by a comparison against this, which is what lets a
	/// save written by an older build still load.
	/// </summary>
	private Snapshotter.Version version = Snapshotter.Version.ModsRecorded;

	public bool IsSnapshotted { get; set; }

	public SnapshotHeader(StartGameParams startGameParams)
	{
		StartGameParams = startGameParams;
		ProgramVersion = UnclaimedWorld.GetVersion();
		Timestamp = DateTime.Now;
		// The one place a save is written from, so the one place the stamp has to be taken.
		Mods = ModSettings.Signature();
	}

	public SnapshotHeader()
	{
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		StartGameParams = (StartGameParams)sn.DoISnapshot(StartGameParams);
		string val = null;
		if (sn.mode != Snapshotter.Mode.Load)
		{
			val = ProgramVersion.ToString();
		}
		ProgramVersion = new Version(sn.DoString(val));
		Timestamp = sn.DoDateTime(Timestamp);
		if (version >= Snapshotter.Version.ModsRecorded)
		{
			Mods = sn.DoString(Mods ?? "");
		}
		sn.Ignore(ProgramVersion);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.ModsRecorded);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		StartGameParams.LoadPostProcess(sn);
	}
}
