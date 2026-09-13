using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Snapshots;

namespace UWGame;

[DebuggerDisplay("{Type.Name} {StateNames}")]
public class BitMask64 : IXmlSerializable, ISnapshot
{
	public string StateNames = "<zero>";

	public ulong Bits;

	public Type Type;

	private static ulong[] B = new ulong[6] { 6148914691236517205uL, 3689348814741910323uL, 1085102592571150095uL, 71777214294589695uL, 281470681808895uL, 4294967295uL };

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(BitMask64))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings()
	};

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	private ulong CountBits(ulong v)
	{
		ulong num = v - ((v >> 1) & B[0]);
		num = ((num >> 2) & B[1]) + (num & B[1]);
		num = ((num >> 4) + num) & B[2];
		num = ((num >> 8) + num) & B[3];
		num = ((num >> 16) + num) & B[4];
		return ((num >> 32) + num) & B[5];
	}

	public BitMask64()
	{
	}

	public BitMask64(BitMask64 original)
	{
		Type = original.Type;
		Bits = original.Bits;
		UpdateStateNames();
	}

	public BitMask64(Type t)
	{
		Type = t;
		Bits = 0uL;
		UpdateStateNames();
	}

	public BitMask64(Type t, int bit)
		: this(t)
	{
		SetInternal(bit);
		UpdateStateNames();
	}

	public BitMask64(Type t, int bit1, int bit2)
		: this(t)
	{
		SetInternal(bit1);
		SetInternal(bit2);
		UpdateStateNames();
	}

	public BitMask64(Type t, int bit1, int bit2, int bit3)
		: this(t)
	{
		SetInternal(bit1);
		SetInternal(bit2);
		SetInternal(bit3);
		UpdateStateNames();
	}

	public BitMask64(Type t, int bit1, int bit2, int bit3, int bit4)
		: this(t)
	{
		SetInternal(bit1);
		SetInternal(bit2);
		SetInternal(bit3);
		SetInternal(bit4);
		UpdateStateNames();
	}

	public BitMask64(Type t, int bit1, int bit2, int bit3, int bit4, int bit5)
		: this(t)
	{
		SetInternal(bit1);
		SetInternal(bit2);
		SetInternal(bit3);
		SetInternal(bit4);
		SetInternal(bit5);
		UpdateStateNames();
	}

	private void SetInternal(int bit)
	{
		Bits |= (ulong)(1L << bit);
	}

	public void UpdateStateNames()
	{
	}

	public void Set<T>(T flag) where T : struct
	{
		if (typeof(T) != Type)
		{
			throw new Exception("Use enum type used when constructing BitMask64");
		}
		int num = (int)(object)flag;
		Bits |= (ulong)(1L << num);
		UpdateStateNames();
	}

	public void SetOrClear<T>(T flag, bool doSet) where T : struct
	{
		if (typeof(T) != Type)
		{
			throw new Exception("Use enum type used when constructing BitMask64");
		}
		if (doSet)
		{
			Set(flag);
		}
		else
		{
			Clear(flag);
		}
		UpdateStateNames();
	}

	public bool Equals(BitMask64 other)
	{
		if (other == null)
		{
			return false;
		}
		if (other.Type != Type)
		{
			throw new Exception("attempt to compare two BitMask64s of different enum types");
		}
		return Equals(other.Bits);
	}

	public void Clear<T>(T flag) where T : struct
	{
		if (typeof(T) != Type)
		{
			throw new Exception("Use enum type used when constructing BitMask64");
		}
		int num = (int)(object)flag;
		Bits &= (ulong)(~(1L << num));
		UpdateStateNames();
	}

	public bool Test<T>(T flag) where T : struct
	{
		if (typeof(T) != Type)
		{
			throw new Exception("Use enum type used when constructing BitMask64");
		}
		int num = (int)(object)flag;
		return (Bits & (ulong)(1L << num)) != 0;
	}

	public string GetNameFromEnumValue(int value)
	{
		return Enum.GetName(Type, value);
	}

	public bool GetEnumValueFromName<T>(string name, out T value) where T : struct
	{
		if (typeof(T) != Type)
		{
			throw new Exception("Use enum type used when constructing BitMask64");
		}
		return Enum.TryParse<T>(name, ignoreCase: true, out value);
	}

	public bool TestForAny(ulong mask)
	{
		return (Bits & mask) != 0;
	}

	public bool TestForAll(ulong mask)
	{
		return (Bits & mask) == Bits;
	}

	public bool TestForNone(ulong mask)
	{
		return (Bits & mask) == 0;
	}

	public bool Equals(ulong mask)
	{
		return Bits == mask;
	}

	public bool Any()
	{
		return Bits != 0;
	}

	public uint CountBits()
	{
		return (uint)CountBits(Bits);
	}

	public uint CountIntersection(ulong mask)
	{
		return (uint)CountBits(Bits | mask);
	}

	public uint CountInverseIntersection(ulong mask)
	{
		return (uint)CountBits(Bits | ~mask);
	}

	public bool TestForAny(BitMask64 other)
	{
		return (Bits & other.Bits) != 0;
	}

	public bool TestForAll(BitMask64 other)
	{
		return (Bits & other.Bits) == Bits;
	}

	public bool TestForNone(BitMask64 other)
	{
		return (Bits & other.Bits) == 0;
	}

	public uint CountIntersection(BitMask64 other)
	{
		return (uint)CountBits(Bits & other.Bits);
	}

	public uint CountInverseIntersection(BitMask64 other)
	{
		if (CountBits(Bits) > CountBits(other.Bits))
		{
			return (uint)CountBits(Bits & ~other.Bits);
		}
		return (uint)CountBits(other.Bits & ~Bits);
	}

	public void Clear(BitMask64 bitsToClear)
	{
		Bits &= ~bitsToClear.Bits;
		UpdateStateNames();
	}

	public void ClearAndSet(BitMask64 clr, BitMask64 set)
	{
		Bits = (Bits & ~clr.Bits) | set.Bits;
		UpdateStateNames();
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
	}

	public void WriteXml(XmlWriter writer)
	{
		CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Bits = sn.DoUInt64(Bits);
		StateNames = sn.DoString(StateNames);
		Type = sn.DoType(Type);
		sn.Ignore(B);
		sn.Ignore(_proxyData);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
