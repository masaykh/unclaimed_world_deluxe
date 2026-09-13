using System;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Renderables;

public class AnimConditions : ISnapshot
{
	public AnimAction? Action;

	public BitMask64 Modifiers;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public AnimConditions()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			Modifiers = new BitMask64(typeof(AnimModifier));
		}
	}

	public AnimConditions(AnimConditions original)
	{
		Action = original.Action;
		Modifiers = new BitMask64(original.Modifiers);
	}

	public bool Equals(AnimConditions other)
	{
		if (other != null)
		{
			if (Action == other.Action)
			{
				return Modifiers.Equals(other.Modifiers);
			}
			return false;
		}
		return false;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Action.HasValue)
		{
			stringBuilder.Append(Enum.GetName(typeof(AnimAction), Action.Value));
		}
		stringBuilder.Append(" - ");
		if (Modifiers != null)
		{
			stringBuilder.Append(Modifiers.StateNames);
		}
		return stringBuilder.ToString();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Action = sn.DoEnumNullable(Action);
		Modifiers = (BitMask64)sn.DoISnapshot(Modifiers);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Modifiers.LoadPostProcess(sn);
	}
}
