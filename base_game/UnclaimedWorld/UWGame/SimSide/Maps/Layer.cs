using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public abstract class Layer : ISnapshot
{
	private Snapshotter.Version version;

	public ushort Width { get; private set; }

	public ushort Height { get; private set; }

	public ushort SectorsAcrossWidth { get; private set; }

	public ushort SectorsAcrossHeight { get; private set; }

	public abstract int SectorSize { get; }

	public bool IsSnapshotted { get; set; }

	public Layer(ushort width, ushort height)
	{
		Width = width;
		Height = height;
		SectorsAcrossWidth = (ushort)Math.Ceiling((double)(int)Width / (double)SectorSize);
		SectorsAcrossHeight = (ushort)Math.Ceiling((double)(int)Height / (double)SectorSize);
	}

	public Layer()
	{
	}

	public void GetSectorFromAbsoluteCoords(int x, int y, out ushort sectorX, out ushort sectorY)
	{
		GetSectorAndRelativeCoords(x, y, out sectorX, out sectorY, out var _, out var _);
	}

	public void GetSectorAndRelativeCoords(int x, int y, out ushort sectorX, out ushort sectorY, out ushort relativeX, out ushort relativeY)
	{
		sectorX = (ushort)Math.DivRem(x, SectorSize, out var result);
		sectorY = (ushort)Math.DivRem(y, SectorSize, out var result2);
		relativeX = (ushort)result;
		relativeY = (ushort)result2;
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		Width = sn.DoUInt16(Width);
		Height = sn.DoUInt16(Height);
		SectorsAcrossWidth = sn.DoUInt16(SectorsAcrossWidth);
		SectorsAcrossHeight = sn.DoUInt16(SectorsAcrossHeight);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
