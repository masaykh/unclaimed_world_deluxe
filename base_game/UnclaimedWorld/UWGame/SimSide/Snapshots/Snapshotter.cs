using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.Control;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Processes;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Snapshots;

public class Snapshotter
{
	public enum LogPriority
	{
		low,
		med,
		high,
		infinity
	}

	public enum Mode
	{
		Save,
		Load,
		CRC
	}

	public enum Version : uint
	{
		Original = 1u,
		/// <summary>
		/// SnapshotHeader gained its Mods field - the modded content a save was made with.
		///
		/// Every ISnapshot writes its own version before its fields (see DoVersion, called from
		/// DoISnapshot), so bumping this for ONE class costs nothing anywhere else: classes that
		/// still pass Original still write Original, and a file written by an older build still
		/// reads because the new field is guarded by a comparison against the version READ.
		/// </summary>
		ModsRecorded = 2u,
		/// <summary>
		/// RandomGenerator gained the number of internal samples it has drawn, so that loading a
		/// save RESUMES the random stream instead of restarting it. Without this the generator was
		/// rebuilt from its original seed on every load and replayed numbers it had already used -
		/// see the comment on RandomGenerator.DoSnapshot.
		/// </summary>
		RandomStreamPosition = 3u
	}

	public struct TypeInformation
	{
		public Type Type;

		public TypeCode TypeCode;

		public bool IsNullable;

		public bool IsSnapshot;

		public bool IsGameData;

		public bool IsType;

		public PropertyInfo[] TupleProperties;

		public TypeInformation[] TupleTypeArguments;
	}

	private class SnapshotClassVerification
	{
		public Type Type;

		public bool IsVerified;

		public List<FieldInfo> Fields = new List<FieldInfo>();

		public Dictionary<Type, List<FieldInfo>> FieldsByType = new Dictionary<Type, List<FieldInfo>>();

		public Dictionary<Type, int> TypesUnaccountedFor = new Dictionary<Type, int>();

		public Dictionary<Type, int> PostponedTypes = new Dictionary<Type, int>();

		public SnapshotClassVerification(Type snapshotType)
		{
			Type = snapshotType;
			BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo[] fieldInfosIncludingBaseClasses = GetFieldInfosIncludingBaseClasses(snapshotType, bindingFlags);
			for (int num = fieldInfosIncludingBaseClasses.Length - 1; num >= 0; num--)
			{
				FieldInfo fieldInfo = fieldInfosIncludingBaseClasses[num];
				int value = 0;
				if (fieldInfo.FieldType != typeof(Version) && fieldInfo.FieldType != typeof(Regulator) && fieldInfo.FieldType != typeof(Renderable) && !fieldInfo.IsLiteral && !fieldInfo.Name.Contains("IsSnapshotted") && !fieldInfo.Name.Contains("CachedAnonymousMethod"))
				{
					Type fieldType = fieldInfo.FieldType;
					_ = fieldInfo.FieldType.Name;
					Fields.Add(fieldInfo);
					if (!TypesUnaccountedFor.TryGetValue(fieldType, out value))
					{
						TypesUnaccountedFor.Add(fieldType, 1);
					}
					else
					{
						value++;
						TypesUnaccountedFor[fieldType] = value;
					}
					Common.AddToMultiList(FieldsByType, fieldType, fieldInfo);
				}
			}
		}

		public void SetTypeAsAccountedFor(Type type, bool postpone = false)
		{
			if (TypesUnaccountedFor.TryGetValue(type, out var value))
			{
				value--;
				if (value <= 0)
				{
					TypesUnaccountedFor.Remove(type);
				}
				else
				{
					TypesUnaccountedFor[type] = value;
				}
				if (postpone)
				{
					Common.AddToDictWithSums(PostponedTypes, type);
				}
			}
		}
	}

	private class FieldInfoComparer : IEqualityComparer<FieldInfo>
	{
		public bool Equals(FieldInfo x, FieldInfo y)
		{
			if (x.DeclaringType == y.DeclaringType)
			{
				return x.Name == y.Name;
			}
			return false;
		}

		public int GetHashCode(FieldInfo obj)
		{
			return obj.Name.GetHashCode() ^ obj.DeclaringType.GetHashCode();
		}
	}

	private CRC CRC = new CRC();

	private BinaryWriter m_writer;

	private BinaryReader m_reader;

	private static bool snapshotting;

	public Mode mode;

	private Dictionary<Type, SnapshotClassVerification> snapshotClasses;

	private SnapshotClassVerification snapshotClassBeingVerified;

	private Stack<SnapshotClassVerification> snapshotClassesBeingVerified = new Stack<SnapshotClassVerification>();

	private Dictionary<Type, int> allSnapshottedISnapshots = new Dictionary<Type, int>();

	private static LogPriority m_logPriority;

	private bool verifyCompleteness;

	private Dictionary<Type, Func<object, object>> TypeAndDoMappings = new Dictionary<Type, Func<object, object>>();

	public static bool IsSnapshotting
	{
		get
		{
			return snapshotting;
		}
		set
		{
			snapshotting = value;
		}
	}

	// CS0067 "the event is never used" is correct and the event stays anyway. Nothing raises it
	// and nothing subscribes; the SetPreLoadPostProcess methods on the quad trees and the
	// collision manager are a different mechanism that happens to share the name. It is the
	// studio's PUBLIC surface, though, so deleting it to satisfy a warning the port turned on
	// would be the port editing the game to suit its own build settings - which is exactly what
	// base_game is not for. Suppressed here, at the one declaration, so -warnaserror keeps
	// working everywhere else.
#pragma warning disable CS0067
	public event Action PreLoadPostProcess;
#pragma warning restore CS0067

	public Snapshotter()
	{
		Log("Snapshot ctor", LogPriority.high);
		TypeAndDoMappings.Add(typeof(Microsoft.Xna.Framework.Point), (object o) => DoPoint((Microsoft.Xna.Framework.Point)o, verifyField: false));
		TypeAndDoMappings.Add(typeof(string), (object o) => DoString((string)o, verifyField: false));
		TypeAndDoMappings.Add(typeof(Vector2), (object o) => DoVector2((Vector2)o, verifyField: false));
		TypeAndDoMappings.Add(typeof(Vector3), (object o) => DoVector3((Vector3)o, verifyField: false));
		TypeAndDoMappings.Add(typeof(WorldLocation), (object o) => DoWorldLocation((WorldLocation)o, verifyField: false));
		TypeAndDoMappings.Add(typeof(TimeSpan), (object o) => DoTimeSpan((TimeSpan)o, verifyField: false));
		TypeAndDoMappings.Add(typeof(ToolTypeCombinationID?), (object o) => DoEnumNullable((ToolTypeCombinationID?)o, verifyField: false));
		TypeAndDoMappings.Add(typeof(Microsoft.Xna.Framework.Color), (object o) => DoColor((Microsoft.Xna.Framework.Color)o, verifyField: false));
		TypeAndDoMappings.Add(typeof(EntityAndRoot), (object o) => DoEntityAndRoot((EntityAndRoot)o, verifyField: false));
	}

	public void Save(BinaryWriter writer, SnapshotHeader header, bool verifyCompleteness = true, bool snapShotHeader = false, bool snapShotBody = false)
	{
		this.verifyCompleteness = verifyCompleteness;
		if (!VerifyParameterlessConstructorsExist())
		{
			throw new Exception("Missing parameterless constructor(s). See the output log...");
		}
		mode = Mode.Save;
		m_writer = writer;
		m_reader = null;
		snapshotting = true;
		snapshotClassBeingVerified = null;
		snapshotClasses = new Dictionary<Type, SnapshotClassVerification>();
		allSnapshottedISnapshots.Clear();
		if (snapShotHeader)
		{
			DoISnapshot(header, verifyCompleteness);
		}
		if (snapShotBody)
		{
			DoISnapshot(The.Sim, verifyCompleteness);
		}
		snapshotting = false;
		m_writer = null;
	}

	public void Load(BinaryReader reader)
	{
		verifyCompleteness = false;
		mode = Mode.Load;
		m_reader = reader;
		m_writer = null;
		snapshotting = true;
		snapshotClassBeingVerified = null;
		snapshotClasses = new Dictionary<Type, SnapshotClassVerification>();
		SnapshotHeader snapshotHeader = (SnapshotHeader)DoISnapshot<SnapshotHeader>(null);
		Controller controller = The.Sim.Controller;
		int indexOfScreen = controller.GetIndexOfScreen(The.Sim);
		lock (controller.UpdateScreensLock)
		{
			controller.RemoveFromList(The.Sim);
			Sim.StartGameMode startGameMode = The.Sim.startGameMode;
			The.Sim.FreeMemoryBeforeLoad();
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			The.Sim = (Sim)DoISnapshot(The.Sim, verifyCompleteness);
			controller.AddScreen(The.Sim, indexOfScreen);
			The.Client.Controller.RecreateClientAfterLoad();
			snapshotHeader.LoadPostProcess(this);
			The.Sim.LoadPostProcess(this);
			The.Sim.StartGameParams = snapshotHeader.StartGameParams;
			The.Sim.IsSnapshotted = false;
			The.Sim.beginRunProgress = 4;
			The.Sim.startGameMode = startGameMode;
		}
		VerifyLoadPostProcessCallsComplete();
		allSnapshottedISnapshots.Clear();
		snapshotting = false;
		m_reader = null;
		Sim.LoadIsFinished = true;
	}

	public SnapshotHeader LoadHeader(BinaryReader reader)
	{
		verifyCompleteness = false;
		mode = Mode.Load;
		m_reader = reader;
		m_writer = null;
		snapshotting = true;
		snapshotClassBeingVerified = null;
		snapshotClasses = new Dictionary<Type, SnapshotClassVerification>();
		SnapshotHeader obj = (SnapshotHeader)DoISnapshot<SnapshotHeader>(null);
		obj.LoadPostProcess(this);
		snapshotting = false;
		m_reader = null;
		return obj;
	}

	public uint DoCRC()
	{
		mode = Mode.CRC;
		CRC = new CRC();
		snapshotting = true;
		The.Sim.DoSnapshot(The.Snapshotter);
		snapshotting = false;
		Log("CRC value is " + CRC.GetCurrentCRCValue + " <------------- LOOK");
		return CRC.GetCurrentCRCValue;
	}

	public object DoUnknownObject(object a, Type type = null, bool verifyField = true)
	{
		if (a is ISnapshot)
		{
			return DoISnapshot((ISnapshot)a, verifyField);
		}
		if (mode == Mode.Save)
		{
			if (a != null)
			{
				throw new Exception(string.Concat("Snapshot of type: ", a.GetType(), " not supported..."));
			}
			if (type != null)
			{
				throw new Exception("Snapshot of type: " + type.Name + " not supported...");
			}
		}
		return a;
	}

	public Version DoVersion(Version version)
	{
		if (mode == Mode.Load)
		{
			m_reader.ReadString();
			return (Version)m_reader.ReadUInt32();
		}
		if (mode == Mode.Save)
		{
			m_writer.Write("Version");
			m_writer.Write(Convert.ToUInt32(version));
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(BitConverter.GetBytes(Convert.ToUInt32(version)));
		}
		return version;
	}

	public float DoFloat(float val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(float));
		}
		if (mode == Mode.Load)
		{
			m_reader.ReadString();
			return m_reader.ReadSingle();
		}
		if (mode == Mode.Save)
		{
			m_writer.Write("float");
			m_writer.Write(val);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(BitConverter.GetBytes(val));
		}
		return val;
	}

	public T DoGameData<T>(T gameData) where T : IGameData
	{
		Type typeFromHandle = typeof(T);
		return (T)DoGameData(typeFromHandle, gameData);
	}

	public T DoGameData<T>(T gameData, bool verifyField = true) where T : IGameData
	{
		Type typeFromHandle = typeof(T);
		return (T)DoGameData(typeFromHandle, gameData, verifyField);
	}

	public object DoGameData(Type objectType, IGameData gameData, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(objectType);
		}
		string value = objectType.Name.ToLowerInvariant();
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString().Equals("null"))
			{
				return null;
			}
			string keyName = m_reader.ReadString();
			return GameData.Instance.GetGameData(objectType, keyName);
		}
		if (mode == Mode.Save)
		{
			if (gameData == null)
			{
				m_writer.Write("null");
				return null;
			}
			m_writer.Write(value);
			m_writer.Write(gameData.KeyName);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(Encoding.Unicode.GetBytes(gameData.KeyName));
		}
		return gameData;
	}

	public float? DoFloatNullable(float? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(float?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			return m_reader.ReadSingle();
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("float?");
				m_writer.Write(val.Value);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				CRC.AddData(BitConverter.GetBytes(val.Value));
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return val;
	}

	public double DoDouble(double val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(double));
		}
		if (mode == Mode.Load)
		{
			m_reader.ReadString();
			return m_reader.ReadDouble();
		}
		if (mode == Mode.Save)
		{
			m_writer.Write("double");
			m_writer.Write(val);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(BitConverter.GetBytes(val));
		}
		return val;
	}

	public decimal DoDecimal(decimal val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(decimal));
		}
		if (mode == Mode.Load)
		{
			m_reader.ReadString();
			return m_reader.ReadDecimal();
		}
		if (mode == Mode.Save)
		{
			m_writer.Write("decimal");
			m_writer.Write(val);
		}
		else
		{
			_ = mode;
			_ = 2;
		}
		return val;
	}

	public decimal? DoDecimalNullable(decimal? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(decimal?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			return m_reader.ReadDecimal();
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("decimal?");
				m_writer.Write(val.Value);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else
		{
			_ = mode;
			_ = 2;
		}
		return val;
	}

	public DateAndTime.TimeDateYear? DoTimeDateYearNullable(DateAndTime.TimeDateYear? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(DateAndTime.TimeDateYear?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			return ReadTimeDateYear();
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("TimeDateYear?");
				val = WriteTimeDateYear(val.Value);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else
		{
			_ = mode;
			_ = 2;
		}
		return val;
	}

	public DateAndTime.TimeDateYear DoTimeDateYear(DateAndTime.TimeDateYear value, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(DateAndTime.TimeDateYear));
		}
		if (mode == Mode.Load)
		{
			return ReadTimeDateYear();
		}
		if (mode == Mode.Save)
		{
			value = WriteTimeDateYear(value);
		}
		return value;
	}

	private DateAndTime.TimeDateYear WriteTimeDateYear(DateAndTime.TimeDateYear value)
	{
		DoDouble(value.TimeOfDay, verifyField: false);
		DoInt32(value.Day, verifyField: false);
		DoInt32(value.Year, verifyField: false);
		return value;
	}

	private DateAndTime.TimeDateYear ReadTimeDateYear()
	{
		double timeOfDay = DoDouble(0.0, verifyField: false);
		int day = DoInt32(0, verifyField: false);
		int year = DoInt32(0, verifyField: false);
		return new DateAndTime.TimeDateYear(timeOfDay, day, year);
	}

	public DateTime DoDateTime(DateTime val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(DateTime));
		}
		if (mode == Mode.Load)
		{
			return new DateTime(DoInt64(0L));
		}
		if (mode == Mode.Save)
		{
			DoInt64(val.Ticks);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(BitConverter.GetBytes(val.Ticks));
		}
		return val;
	}

	public TimeSpan? DoTimeSpanNullable(TimeSpan? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(byte?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			return new TimeSpan(DoInt64(0L));
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("TimeSpan?");
				DoInt64(val.Value.Ticks);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				CRC.AddData(BitConverter.GetBytes(val.Value.Ticks));
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(value: false));
			}
		}
		return val;
	}

	public TimeSpan DoTimeSpan(TimeSpan val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(TimeSpan));
		}
		if (mode == Mode.Load)
		{
			return new TimeSpan(DoInt64(0L));
		}
		if (mode == Mode.Save)
		{
			DoInt64(val.Ticks);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(BitConverter.GetBytes(val.Ticks));
		}
		return val;
	}

	public bool DoBool(bool val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(bool));
		}
		if (mode == Mode.Load)
		{
			m_reader.ReadString();
			return m_reader.ReadBoolean();
		}
		if (mode == Mode.Save)
		{
			m_writer.Write("bool");
			m_writer.Write(val);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(BitConverter.GetBytes(val));
		}
		return val;
	}

	public byte? DoByteNullable(byte? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(byte?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			return m_reader.ReadByte();
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("byte?");
				m_writer.Write(val.Value);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				// PORT DEVIATION 3 (see PORTING-NOTES.md). val is byte?, and BitConverter has no
				// GetBytes(byte) overload. On .NET Framework the compiler picked GetBytes(short);
				// .NET 8 added GetBytes(Half), which makes byte->short vs byte->Half ambiguous.
				// The original IL is `Nullable<uint8>::get_Value()` then `GetBytes(int16)`, i.e.
				// 2 CRC bytes - so the cast MUST be to short or save/replay CRCs would diverge.
				CRC.AddData(BitConverter.GetBytes((short)val.Value));
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(value: false));
			}
		}
		return val;
	}

	public byte DoByte(byte val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(byte));
		}
		if (mode == Mode.Load)
		{
			return m_reader.ReadByte();
		}
		if (mode == Mode.Save)
		{
			m_writer.Write(val);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(val);
		}
		return val;
	}

	public bool? DoBoolNullable(bool? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(bool?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			return m_reader.ReadBoolean();
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("bool?");
				m_writer.Write(val.Value);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				CRC.AddData(BitConverter.GetBytes(val.Value));
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(value: false));
			}
		}
		return val;
	}

	public double? DoDoubleNullable(double? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(double?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			return m_reader.ReadDouble();
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("double?");
				m_writer.Write(val.Value);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				CRC.AddData(BitConverter.GetBytes(val.Value));
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0.0));
			}
		}
		return val;
	}

	public ushort? DoUInt16Nullable(ushort? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(ushort?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			return m_reader.ReadUInt16();
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("ushort?");
				m_writer.Write(val.Value);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				CRC.AddData(BitConverter.GetBytes(val.Value));
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0.0));
			}
		}
		return val;
	}

	public ushort DoUInt16(ushort val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(ushort));
		}
		if (mode == Mode.Load)
		{
			m_reader.ReadString();
			return m_reader.ReadUInt16();
		}
		if (mode == Mode.Save)
		{
			m_writer.Write("ushort");
			m_writer.Write(val);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(BitConverter.GetBytes(val));
		}
		return val;
	}

	public int DoInt32(int val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(int));
		}
		if (mode == Mode.Load)
		{
			m_reader.ReadString();
			return m_reader.ReadInt32();
		}
		if (mode == Mode.Save)
		{
			m_writer.Write("int");
			m_writer.Write(val);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(BitConverter.GetBytes(val));
		}
		return val;
	}

	public int? DoInt32Nullable(int? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(int?));
		}
		if (mode == Mode.Load || mode == Mode.Save)
		{
			return DoInt32NullableStatic(mode, m_writer, m_reader, val);
		}
		if (val.HasValue)
		{
			CRC.AddData(BitConverter.GetBytes(val.Value));
		}
		else
		{
			CRC.AddData(BitConverter.GetBytes(0));
		}
		return val;
	}

	public static int? DoInt32NullableStatic(Mode mode, BinaryWriter writer, BinaryReader reader, int? value)
	{
		if (mode == Mode.Load)
		{
			if (reader.ReadString() == "null")
			{
				return null;
			}
			return reader.ReadInt32();
		}
		if (value.HasValue)
		{
			writer.Write("Int32?");
			writer.Write(value.Value);
		}
		else
		{
			writer.Write("null");
		}
		return value;
	}

	public long DoInt64(long val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(long));
		}
		if (mode == Mode.Load)
		{
			m_reader.ReadString();
			return m_reader.ReadInt64();
		}
		if (mode == Mode.Save)
		{
			m_writer.Write("long");
			m_writer.Write(val);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(BitConverter.GetBytes(val));
		}
		return val;
	}

	public ulong DoUInt64(ulong val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(ulong));
		}
		if (mode == Mode.Load)
		{
			m_reader.ReadString();
			return m_reader.ReadUInt64();
		}
		if (mode == Mode.Save)
		{
			m_writer.Write("ulong");
			m_writer.Write(val);
		}
		else if (mode == Mode.CRC)
		{
			CRC.AddData(BitConverter.GetBytes(val));
		}
		return val;
	}

	public Vector3 DoVector3(Vector3 val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Vector3));
		}
		Vector3 result = val;
		result.X = DoFloat(result.X);
		result.Y = DoFloat(result.Y);
		result.Z = DoFloat(result.Z);
		return result;
	}

	public Matrix DoMatrix(Matrix val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Matrix));
		}
		Matrix result = val;
		result.M11 = DoFloat(result.M11);
		result.M11 = DoFloat(result.M12);
		result.M11 = DoFloat(result.M13);
		result.M11 = DoFloat(result.M14);
		result.M11 = DoFloat(result.M21);
		result.M11 = DoFloat(result.M22);
		result.M11 = DoFloat(result.M23);
		result.M11 = DoFloat(result.M24);
		result.M11 = DoFloat(result.M31);
		result.M11 = DoFloat(result.M32);
		result.M11 = DoFloat(result.M33);
		result.M11 = DoFloat(result.M34);
		result.M11 = DoFloat(result.M41);
		result.M11 = DoFloat(result.M42);
		result.M11 = DoFloat(result.M43);
		result.M11 = DoFloat(result.M44);
		return result;
	}

	public WorldLocation DoWorldLocation(WorldLocation val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(WorldLocation));
		}
		WorldLocation result = val;
		result.X = DoFloat(result.X);
		result.Y = DoFloat(result.Y);
		result.Z = DoFloat(result.Z);
		return result;
	}

	public WorldLocation? DoWorldLocationNullable(WorldLocation? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(WorldLocation?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			float x = m_reader.ReadSingle();
			float y = m_reader.ReadSingle();
			float z = m_reader.ReadSingle();
			return new WorldLocation(x, y, z);
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("WorldLocation?");
				m_writer.Write(val.Value.X);
				m_writer.Write(val.Value.Y);
				m_writer.Write(val.Value.Z);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				DoFloat(val.Value.X);
				DoFloat(val.Value.Y);
				DoFloat(val.Value.Z);
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return val;
	}

	public Vector4 DoVector4(Vector4 val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Vector4));
		}
		Vector4 result = val;
		result.X = DoFloat(result.X);
		result.Y = DoFloat(result.Y);
		result.Z = DoFloat(result.Z);
		result.W = DoFloat(result.W);
		return result;
	}

	public Vector3? DoVector3Nullable(Vector3? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Vector3?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			float x = m_reader.ReadSingle();
			float y = m_reader.ReadSingle();
			float z = m_reader.ReadSingle();
			return new Vector3(x, y, z);
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("Vector3?");
				m_writer.Write(val.Value.X);
				m_writer.Write(val.Value.Y);
				m_writer.Write(val.Value.Z);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				DoFloat(val.Value.X);
				DoFloat(val.Value.Y);
				DoFloat(val.Value.Z);
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return val;
	}

	public Vector2 DoVector2(Vector2 val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Vector2));
		}
		Vector2 result = val;
		result.X = DoFloat(result.X);
		result.Y = DoFloat(result.Y);
		return result;
	}

	public Vector2? DoVector2Nullable(Vector2? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Vector2?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			float x = m_reader.ReadSingle();
			float y = m_reader.ReadSingle();
			return new Vector2(x, y);
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("Vector2?");
				m_writer.Write(val.Value.X);
				m_writer.Write(val.Value.Y);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				DoFloat(val.Value.X);
				DoFloat(val.Value.Y);
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return val;
	}

	public RectangleF DoRectangleF(RectangleF val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(RectangleF));
		}
		RectangleF result = val;
		result.Height = DoFloat(result.Height);
		result.Width = DoFloat(result.Width);
		result.X = DoFloat(result.X);
		result.Y = DoFloat(result.Y);
		return result;
	}

	public Microsoft.Xna.Framework.Rectangle? DoRectangleNullable(Microsoft.Xna.Framework.Rectangle? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Microsoft.Xna.Framework.Rectangle?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			Microsoft.Xna.Framework.Rectangle value = default(Microsoft.Xna.Framework.Rectangle);
			value.Height = DoInt32(value.Height, verifyField: false);
			value.Width = DoInt32(value.Width, verifyField: false);
			value.X = DoInt32(value.X, verifyField: false);
			value.Y = DoInt32(value.Y, verifyField: false);
			return value;
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("Rectangle?");
				DoInt32(val.Value.Height, verifyField: false);
				DoInt32(val.Value.Width, verifyField: false);
				DoInt32(val.Value.X, verifyField: false);
				DoInt32(val.Value.Y, verifyField: false);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				DoInt32(val.Value.Height, verifyField: false);
				DoInt32(val.Value.Width, verifyField: false);
				DoInt32(val.Value.X, verifyField: false);
				DoInt32(val.Value.Y, verifyField: false);
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return val;
	}

	public Microsoft.Xna.Framework.Rectangle DoRectangle(Microsoft.Xna.Framework.Rectangle val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Microsoft.Xna.Framework.Rectangle));
		}
		Microsoft.Xna.Framework.Rectangle result = val;
		result.Height = DoInt32(result.Height, verifyField: false);
		result.Width = DoInt32(result.Width, verifyField: false);
		result.X = DoInt32(result.X, verifyField: false);
		result.Y = DoInt32(result.Y, verifyField: false);
		return result;
	}

	public string DoString(string val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(string));
		}
		if (mode == Mode.Load || mode == Mode.Save)
		{
			return DoStringStatic(mode, m_writer, m_reader, val);
		}
		CRC.AddData(Encoding.Unicode.GetBytes(val));
		return val;
	}

	public static string DoStringStatic(Mode mode, BinaryWriter writer, BinaryReader reader, string val)
	{
		switch (mode)
		{
		case Mode.Load:
			if (reader.ReadString() == "null")
			{
				return null;
			}
			return reader.ReadString();
		case Mode.Save:
			if (val == null)
			{
				writer.Write("null");
				return null;
			}
			writer.Write("string");
			writer.Write(val);
			break;
		}
		return val;
	}

	public Microsoft.Xna.Framework.Point DoPoint(Microsoft.Xna.Framework.Point val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Microsoft.Xna.Framework.Point));
		}
		Microsoft.Xna.Framework.Point result = val;
		result.X = DoInt32(result.X, verifyField: false);
		result.Y = DoInt32(result.Y, verifyField: false);
		return result;
	}

	public TilePos DoTilePos(TilePos val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(TilePos));
		}
		TilePos result = val;
		result.X = DoInt32(result.X, verifyField: false);
		result.Y = DoInt32(result.Y, verifyField: false);
		return result;
	}

	public TilePos? DoTilePosNullable(TilePos? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(TilePos?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int x = m_reader.ReadInt32();
			int y = m_reader.ReadInt32();
			return new TilePos(x, y);
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("TilePos?");
				m_writer.Write(val.Value.X);
				m_writer.Write(val.Value.Y);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				DoInt32(val.Value.X);
				DoInt32(val.Value.Y);
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return val;
	}

	public Microsoft.Xna.Framework.Point? DoPointNullable(Microsoft.Xna.Framework.Point? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Microsoft.Xna.Framework.Point?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int x = m_reader.ReadInt32();
			int y = m_reader.ReadInt32();
			return new Microsoft.Xna.Framework.Point(x, y);
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("Point?");
				m_writer.Write(val.Value.X);
				m_writer.Write(val.Value.Y);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				DoFloat(val.Value.X);
				DoFloat(val.Value.Y);
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return val;
	}

	public Type DoType(Type t, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Type));
		}
		string val = null;
		if (mode != Mode.Load)
		{
			val = t.FullName;
		}
		return Type.GetType(DoString(val, verifyField: false));
	}

	public EntityAndRoot DoEntityAndRoot(EntityAndRoot val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(EntityAndRoot));
		}
		EntityAndRoot result = val;
		result.Entity = DoEnum(result.Entity, verifyField: false);
		result.Root = DoEnum(result.Root, verifyField: false);
		return result;
	}

	public EntityAndRoot? DoEntityAndRootNullable(EntityAndRoot? partAndRoot, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(EntityAndRoot?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			EntityID entity = DoEnum(EntityID.Invalid, verifyField: false);
			EntityID root = DoEnum(EntityID.Invalid, verifyField: false);
			return new EntityAndRoot(entity, root);
		}
		if (mode == Mode.Save)
		{
			if (partAndRoot.HasValue)
			{
				m_writer.Write("EntityAndRoot?");
				DoEnum(partAndRoot.Value.Entity, verifyField: false);
				DoEnum(partAndRoot.Value.Root, verifyField: false);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (partAndRoot.HasValue)
			{
				DoEnum(partAndRoot.Value.Entity, verifyField: false);
				DoEnum(partAndRoot.Value.Root, verifyField: false);
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return partAndRoot;
	}

	public TravelLocation? DoTravelLocationNullable(TravelLocation? travelLocation, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(TravelLocation?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			long? allegianceID = DoInt64Nullable(null, verifyField: false);
			long siteID = DoInt64(0L, verifyField: false);
			long? expeditionID = DoInt64Nullable(null, verifyField: false);
			long? terminalEntityID = DoInt64Nullable(null, verifyField: false);
			return new TravelLocation(siteID, allegianceID, expeditionID, terminalEntityID);
		}
		if (mode == Mode.Save)
		{
			if (travelLocation.HasValue)
			{
				m_writer.Write("TravelLocation?");
				DoInt64Nullable(travelLocation.Value.AllegianceID, verifyField: false);
				DoInt64(travelLocation.Value.SiteID, verifyField: false);
				DoInt64Nullable(travelLocation.Value.ExpeditionID, verifyField: false);
				DoInt64Nullable(travelLocation.Value.TerminalEntityID, verifyField: false);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (travelLocation.HasValue)
			{
				DoInt64Nullable(travelLocation.Value.AllegianceID, verifyField: false);
				DoInt64(travelLocation.Value.SiteID, verifyField: false);
				DoInt64Nullable(travelLocation.Value.ExpeditionID, verifyField: false);
				DoInt64Nullable(travelLocation.Value.TerminalEntityID, verifyField: false);
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return travelLocation;
	}

	public StorageTarget? DoStorageTargetNullable(StorageTarget? storageTarget, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(StorageTarget?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			EntityID storageEntity = DoEnum(EntityID.Invalid, verifyField: false);
			StorageID storageID = DoEnum(StorageID.Invalid, verifyField: false);
			return new StorageTarget(storageEntity, storageID);
		}
		if (mode == Mode.Save)
		{
			if (storageTarget.HasValue)
			{
				m_writer.Write("StorageTarget?");
				DoEnum(storageTarget.Value.StorageEntity, verifyField: false);
				DoEnum(storageTarget.Value.StorageID, verifyField: false);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (storageTarget.HasValue)
			{
				DoEnum(storageTarget.Value.StorageEntity, verifyField: false);
				DoEnum(storageTarget.Value.StorageID, verifyField: false);
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return storageTarget;
	}

	public GeodeticCoordinate? DoGeodeticCoordinateNullable(GeodeticCoordinate? coords, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(GeodeticCoordinate?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			double latitude = DoDouble(0.0, verifyField: false);
			return new GeodeticCoordinate(DoDouble(0.0, verifyField: false), latitude);
		}
		if (mode == Mode.Save)
		{
			if (coords.HasValue)
			{
				m_writer.Write("GeodeticCoordinate?");
				DoDouble(coords.Value.Latitude, verifyField: false);
				DoDouble(coords.Value.Longitude, verifyField: false);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (coords.HasValue)
			{
				DoDouble(coords.Value.Latitude, verifyField: false);
				DoDouble(coords.Value.Longitude, verifyField: false);
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0));
			}
		}
		return coords;
	}

	public GeodeticCoordinate DoGeodeticCoordinate(GeodeticCoordinate coords, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(GeodeticCoordinate));
		}
		double latitude = DoDouble(coords.Latitude, verifyField: false);
		double longitude = DoDouble(coords.Longitude, verifyField: false);
		return new GeodeticCoordinate(longitude, latitude);
	}

	public TravelLocation DoTravelLocation(TravelLocation travelLocation, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(TravelLocation));
		}
		long? allegianceID = DoInt64Nullable(travelLocation.AllegianceID, verifyField: false);
		long siteID = DoInt64(travelLocation.SiteID, verifyField: false);
		long? expeditionID = DoInt64Nullable(travelLocation.ExpeditionID, verifyField: false);
		long? terminalEntityID = DoInt64Nullable(travelLocation.TerminalEntityID, verifyField: false);
		return new TravelLocation(siteID, allegianceID, expeditionID, terminalEntityID);
	}

	public Microsoft.Xna.Framework.Color DoColor(Microsoft.Xna.Framework.Color color, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(Microsoft.Xna.Framework.Color));
		}
		return new Microsoft.Xna.Framework.Color
		{
			A = DoByte(color.A, verifyField: false),
			R = DoByte(color.R, verifyField: false),
			G = DoByte(color.G, verifyField: false),
			B = DoByte(color.B, verifyField: false)
		};
	}

	public T DoEnum<T>(T val, bool verifyField = true) where T : struct, IComparable, IFormattable, IConvertible
	{
		Type typeFromHandle = typeof(T);
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		switch (Type.GetTypeCode(Enum.GetUnderlyingType(typeFromHandle)))
		{
		case TypeCode.Int32:
		{
			int num3 = Convert.ToInt32(val);
			if (mode == Mode.CRC)
			{
				CRC.AddData(BitConverter.GetBytes(num3));
				return val;
			}
			return (T)(object)DoInt32(num3, verifyField: false);
		}
		case TypeCode.Int64:
		{
			long num2 = Convert.ToInt64(val);
			if (mode == Mode.CRC)
			{
				CRC.AddData(BitConverter.GetBytes(num2));
				return val;
			}
			return (T)(object)DoInt64(num2, verifyField: false);
		}
		case TypeCode.UInt64:
		{
			ulong num = Convert.ToUInt64(val);
			if (mode == Mode.CRC)
			{
				CRC.AddData(BitConverter.GetBytes(num));
				return val;
			}
			return (T)(object)DoUInt64(num, verifyField: false);
		}
		default:
			return val;
		}
	}

	public TEnum? DoEnumNullable<TEnum>(TEnum? val, bool verifyField = true) where TEnum : struct, IComparable, IFormattable, IConvertible
	{
		Type typeFromHandle = typeof(TEnum?);
		VerifyField(typeFromHandle);
		TypeCode typeCode = Type.GetTypeCode(Enum.GetUnderlyingType(typeof(TEnum)));
		if (val.HasValue)
		{
			switch (typeCode)
			{
			case TypeCode.UInt64:
			{
				ulong? val4 = (ulong)(object)val;
				return (TEnum)(object)DoUInt64Nullable(val4, verifyField: false);
			}
			case TypeCode.Int32:
			{
				int? val3 = (int)(object)val;
				return (TEnum)(object)DoInt32Nullable(val3, verifyField: false);
			}
			case TypeCode.Int64:
			{
				long? val2 = (long)(object)val;
				return (TEnum)(object)DoInt64Nullable(val2, verifyField: false);
			}
			}
		}
		else
		{
			switch (typeCode)
			{
			case TypeCode.UInt64:
			{
				ulong? num3 = DoUInt64Nullable(null, verifyField: false);
				if (num3.HasValue)
				{
					return (TEnum)(object)num3;
				}
				return null;
			}
			case TypeCode.Int32:
			{
				int? num2 = DoInt32Nullable(null, verifyField: false);
				if (num2.HasValue)
				{
					return (TEnum)(object)num2;
				}
				return null;
			}
			case TypeCode.Int64:
			{
				long? num = DoInt64Nullable(null, verifyField: false);
				if (num.HasValue)
				{
					return (TEnum)(object)num;
				}
				return null;
			}
			}
		}
		return val;
	}

	public EntityID? DoEntityIDNullable(EntityID? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(EntityID?));
		}
		return DoEnumNullable(val);
	}

	public EntityID DoEntityID(EntityID val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(EntityID));
		}
		return DoEnum(val, verifyField: false);
	}

	public MethodID DoMethodID(MethodID val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(MethodID));
		}
		return DoEnum(val, verifyField: false);
	}

	public MethodID? DoMethodIDNullable(MethodID? val, bool verifyField = true)
	{
		return DoEnumNullable(val);
	}

	public long? DoInt64Nullable(long? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(long?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			return m_reader.ReadInt64();
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("Int64?");
				m_writer.Write(val.Value);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				CRC.AddData(BitConverter.GetBytes(val.Value));
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0L));
			}
		}
		return val;
	}

	public ulong? DoUInt64Nullable(ulong? val, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(ulong?));
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			return m_reader.ReadUInt64();
		}
		if (mode == Mode.Save)
		{
			if (val.HasValue)
			{
				m_writer.Write("UInt64?");
				m_writer.Write(val.Value);
			}
			else
			{
				m_writer.Write("null");
			}
		}
		else if (mode == Mode.CRC)
		{
			if (val.HasValue)
			{
				CRC.AddData(BitConverter.GetBytes(val.Value));
			}
			else
			{
				CRC.AddData(BitConverter.GetBytes(0uL));
			}
		}
		return val;
	}

	public T[][] DoJaggedArray<T>(T[][] collection, bool verifyField = true)
	{
		Type typeFromHandle = typeof(T[][]);
		string name = typeFromHandle.Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			T[][] array = new T[num][];
			for (int i = 0; i < num; i++)
			{
				T[] array2 = DoArray<T>(null, verifyField: false);
				array[i] = array2;
			}
			return array;
		}
		if (mode == Mode.Save)
		{
			if (collection == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(name);
			}
		}
		if (collection == null)
		{
			return null;
		}
		int val = collection.Length;
		val = DoInt32(val, verifyField: false);
		foreach (T[] collection2 in collection)
		{
			DoArray(collection2, verifyField: false);
		}
		return collection;
	}

	public Queue<T> DoQueue<T>(Queue<T> collection, bool verifyField = true)
	{
		GetStaticTypeInfo<T>(out var typeInfo);
		Type typeFromHandle = typeof(Queue<T>);
		string name = typeFromHandle.Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			Queue<T> queue = (Queue<T>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(T));
				queue.Enqueue((T)obj);
			}
			return queue;
		}
		if (mode == Mode.Save)
		{
			if (collection == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(name);
			}
		}
		if (collection == null)
		{
			return null;
		}
		int count = collection.Count;
		count = DoInt32(count, verifyField: false);
		foreach (T item in collection)
		{
			DoElement(typeInfo, item);
		}
		return collection;
	}

	public SerializableQueue<T> DoSerializableQueue<T>(SerializableQueue<T> collection, bool verifyField = true)
	{
		GetStaticTypeInfo<T>(out var typeInfo);
		Type typeFromHandle = typeof(SerializableQueue<T>);
		string name = typeFromHandle.Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			SerializableQueue<T> serializableQueue = (SerializableQueue<T>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(T));
				serializableQueue.Enqueue((T)obj);
			}
			return serializableQueue;
		}
		if (mode == Mode.Save)
		{
			if (collection == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(name);
			}
		}
		if (collection == null)
		{
			return null;
		}
		int count = collection.Count;
		count = DoInt32(count, verifyField: false);
		foreach (T item in collection)
		{
			DoElement(typeInfo, item);
		}
		return collection;
	}

	public T[] DoArray<T>(T[] collection, bool verifyField = true)
	{
		GetStaticTypeInfo<T>(out var typeInfo);
		Type typeFromHandle = typeof(T[]);
		string name = typeFromHandle.Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			T[] array = (T[])Array.CreateInstance(typeInfo.Type, num);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(T));
				array[i] = (T)obj;
			}
			return array;
		}
		if (mode == Mode.Save)
		{
			if (collection == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(name);
			}
		}
		if (collection == null)
		{
			return null;
		}
		int val = collection.Length;
		val = DoInt32(val, verifyField: false);
		foreach (T val2 in collection)
		{
			DoElement(typeInfo, val2);
		}
		return collection;
	}

	private void VerifyField(Type type)
	{
	}

	public List<T> DoList<T>(List<T> list, bool verifyField = true)
	{
		GetStaticTypeInfo<T>(out var typeInfo);
		Type typeFromHandle = typeof(List<T>);
		string name = typeFromHandle.Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			List<T> list2 = (List<T>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(T));
				list2.Add((T)obj);
			}
			return list2;
		}
		if (mode == Mode.Save)
		{
			if (list == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(name);
			}
		}
		if (list == null)
		{
			return null;
		}
		int count = list.Count;
		count = DoInt32(count, verifyField: false);
		foreach (T item in list)
		{
			DoElement(typeInfo, item);
		}
		return list;
	}

	public HashSet<T> DoHashSet<T>(HashSet<T> set, bool verifyField = true)
	{
		GetStaticTypeInfo<T>(out var typeInfo);
		Type typeFromHandle = typeof(HashSet<T>);
		string name = typeFromHandle.Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			HashSet<T> hashSet = (HashSet<T>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(T));
				hashSet.Add((T)obj);
			}
			return hashSet;
		}
		if (mode == Mode.Save)
		{
			if (set == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(name);
			}
		}
		if (set == null)
		{
			return null;
		}
		int count = set.Count;
		count = DoInt32(count, verifyField: false);
		foreach (T item in set)
		{
			DoElement(typeInfo, item);
		}
		return set;
	}

	public static void GetStaticTypeInfo<T>(out TypeInformation typeInfo)
	{
		GetStaticTypeInfo(typeof(T), out typeInfo);
	}

	private static void GetStaticTypeInfo(Type type, out TypeInformation typeInfo)
	{
		typeInfo = default(TypeInformation);
		typeInfo.Type = type;
		typeInfo.TypeCode = Type.GetTypeCode(type);
		typeInfo.TupleTypeArguments = null;
		typeInfo.TupleProperties = null;
		typeInfo.IsNullable = false;
		typeInfo.IsSnapshot = false;
		typeInfo.IsType = false;
		typeInfo.IsGameData = false;
		if (type.IsGenericType)
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (genericTypeDefinition == typeof(Nullable<>))
			{
				typeInfo.IsNullable = true;
			}
			if (HandleGenericArguments(genericTypeDefinition))
			{
				Type[] genericArguments = type.GetGenericArguments();
				typeInfo.TupleTypeArguments = new TypeInformation[genericArguments.Length];
				typeInfo.TupleProperties = type.GetProperties();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					Type type2 = genericArguments[i];
					TypeInformation typeInfo2 = default(TypeInformation);
					GetStaticTypeInfo(type2, out typeInfo2);
					typeInfo.TupleTypeArguments[i] = typeInfo2;
				}
				return;
			}
		}
		if (type == typeof(Type))
		{
			typeInfo.IsType = true;
		}
		else if (type is ISnapshot || typeof(ISnapshot).IsAssignableFrom(type))
		{
			typeInfo.IsSnapshot = true;
		}
		else if (typeof(IGameData).IsAssignableFrom(type))
		{
			typeInfo.IsGameData = true;
		}
	}

	private static bool HandleGenericArguments(Type genericTypeDefinition)
	{
		if (!(genericTypeDefinition == typeof(Tuple<, >)) && !(genericTypeDefinition == typeof(Tuple<, , >)))
		{
			return genericTypeDefinition == typeof(Pair<, >);
		}
		return true;
	}

	public Pair<T, T> DoPair<T>(Pair<T, T> value)
	{
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString().Equals("null"))
			{
				return null;
			}
			GetStaticTypeInfo<Pair<T, T>>(out var typeInfo);
			return (Pair<T, T>)DoElement(typeInfo, value);
		}
		if (mode == Mode.Save)
		{
			if (value != null)
			{
				GetStaticTypeInfo<Pair<T, T>>(out var typeInfo2);
				return (Pair<T, T>)DoElement(typeInfo2, value);
			}
			m_writer.Write("null");
		}
		return value;
	}

	public object DoElement(TypeInformation typeInfo, object value)
	{
		value = (typeInfo.IsSnapshot ? DoISnapshot((ISnapshot)value, typeInfo.Type, verifyField: false) : (typeInfo.IsGameData ? DoGameData(typeInfo.Type, (IGameData)value, verifyField: false) : (typeInfo.IsType ? DoType((Type)value, verifyField: false) : ((typeInfo.TupleTypeArguments == null) ? DoPrimitivesAndStructs(typeInfo.Type, typeInfo.TypeCode, typeInfo.IsNullable, value) : DoGenericType(value, typeInfo.Type, typeInfo.TupleProperties, typeInfo.TupleTypeArguments, verifyField: false)))));
		return value;
	}

	public object DoObject(object value, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(typeof(object));
		}
		if (mode == Mode.Load)
		{
			string text = m_reader.ReadString();
			if (text == "null")
			{
				return null;
			}
			GetStaticTypeInfo(Type.GetType(text), out var typeInfo);
			return DoElement(typeInfo, value);
		}
		if (mode == Mode.Save)
		{
			if (value == null)
			{
				m_writer.Write("null");
				return value;
			}
			Type type = value.GetType();
			GetStaticTypeInfo(type, out var typeInfo2);
			m_writer.Write(type.FullName);
			return DoElement(typeInfo2, value);
		}
		return value;
	}

	private object GetDefaultValue(Type t)
	{
		if (t.IsValueType)
		{
			return Activator.CreateInstance(t);
		}
		return null;
	}

	private object DoGenericType(object value, Type type, PropertyInfo[] tupleProperties, TypeInformation[] tupleTypeArguments, bool verifyField = true)
	{
		if (verifyField)
		{
			VerifyField(type);
		}
		if (mode == Mode.Load)
		{
			object[] array = new object[tupleTypeArguments.Length];
			for (int i = 0; i < tupleProperties.Length; i++)
			{
				TypeInformation typeInfo = tupleTypeArguments[i];
				object obj = DoElement(typeInfo, GetDefaultValue(typeInfo.Type));
				if (typeInfo.Type.IsEnum)
				{
					array[i] = Enum.ToObject(typeInfo.Type, obj);
				}
				else
				{
					array[i] = obj;
				}
			}
			return Activator.CreateInstance(type, array);
		}
		if (mode == Mode.Save)
		{
			for (int j = 0; j < tupleProperties.Length; j++)
			{
				object value2 = tupleProperties[j].GetValue(value, null);
				TypeInformation typeInfo2 = tupleTypeArguments[j];
				DoElement(typeInfo2, value2);
			}
			return value;
		}
		return value;
	}

	private object DoPrimitivesAndStructs(Type type, TypeCode typeCode, bool isNullable, object value)
	{
		if (type.IsEnum)
		{
			Enum.GetUnderlyingType(type);
			value = DoPrimitive(typeCode, isNullable, value);
			return value;
		}
		if (typeCode == TypeCode.Object)
		{
			if (TypeAndDoMappings.TryGetValue(type, out var value2))
			{
				value = value2(value);
			}
			else if (isNullable && type.IsGenericType)
			{
				if (type.GetGenericArguments()[0].IsEnum)
				{
					try
					{
						value = DoUnknownObject(value, type, verifyField: false);
						return value;
					}
					catch (Exception innerException)
					{
						throw new Exception("Nullable enum type is not supported as a list member, add a TypeAndDoMappings entry.", innerException);
					}
				}
				value = DoUnknownObject(value, type, verifyField: false);
			}
			else
			{
				value = DoUnknownObject(value, type, verifyField: false);
			}
			return value;
		}
		return DoPrimitive(typeCode, isNullable, value);
	}

	private object DoPrimitive(TypeCode typeCode, bool isNullable, object value)
	{
		value = typeCode switch
		{
			TypeCode.Int64 => (!isNullable) ? ((object)DoInt64((long)value, verifyField: false)) : ((object)DoInt64Nullable((long?)value, verifyField: false)), 
			TypeCode.Int32 => (!isNullable) ? ((object)DoInt32((int)value, verifyField: false)) : ((object)DoInt32Nullable((int?)value, verifyField: false)), 
			TypeCode.Boolean => (!isNullable) ? ((object)DoBool((bool)value, verifyField: false)) : ((object)DoBoolNullable((bool?)value, verifyField: false)), 
			TypeCode.String => DoString((string)value, verifyField: false), 
			TypeCode.Single => DoFloat((float)value, verifyField: false), 
			TypeCode.Double => (!isNullable) ? ((object)DoDouble((double)value, verifyField: false)) : ((object)DoDoubleNullable((double?)value, verifyField: false)), 
			TypeCode.UInt16 => DoUInt16((ushort)value, verifyField: false), 
			TypeCode.UInt64 => (!isNullable) ? ((object)DoUInt64((ulong)value, verifyField: false)) : ((object)DoUInt64Nullable((ulong?)value, verifyField: false)), 
			TypeCode.Byte => DoByte((byte)value, verifyField: false), 
			_ => throw new Exception("Missing primitive type case"), 
		};
		return value;
	}

	public Dictionary<K, HashSet<V>> DoMultiMapHashSet<K, V>(Dictionary<K, HashSet<V>> dict, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		Type typeFromHandle = typeof(Dictionary<K, HashSet<V>>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeof(HashSet<V>).Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			Dictionary<K, HashSet<V>> dictionary = (Dictionary<K, HashSet<V>>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				HashSet<V> value2 = DoHashSet<V>(null, verifyField: false);
				dictionary.Add((K)obj, value2);
			}
			return dictionary;
		}
		if (mode == Mode.Save)
		{
			if (dict == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (dict == null)
		{
			return null;
		}
		int count = dict.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, HashSet<V>> item in dict)
		{
			DoElement(typeInfo, item.Key);
			DoHashSet(item.Value, verifyField: false);
		}
		return dict;
	}

	public Dictionary<K, List<V>> DoMultiMap<K, V>(Dictionary<K, List<V>> dict, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		Type typeFromHandle = typeof(Dictionary<K, List<V>>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeof(List<V>).Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			Dictionary<K, List<V>> dictionary = (Dictionary<K, List<V>>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				List<V> value2 = DoList<V>(null, verifyField: false);
				dictionary.Add((K)obj, value2);
			}
			return dictionary;
		}
		if (mode == Mode.Save)
		{
			if (dict == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (dict == null)
		{
			return null;
		}
		int count = dict.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, List<V>> item in dict)
		{
			DoElement(typeInfo, item.Key);
			DoList(item.Value, verifyField: false);
		}
		return dict;
	}

	public Dictionary<K, V[][]> DoMultiArray<K, V>(Dictionary<K, V[][]> dict, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		Type typeFromHandle = typeof(Dictionary<K, V[][]>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeof(V[][]).Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			Dictionary<K, V[][]> dictionary = (Dictionary<K, V[][]>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				V[][] value2 = DoJaggedArray<V>(null, verifyField: false);
				dictionary.Add((K)obj, value2);
			}
			return dictionary;
		}
		if (mode == Mode.Save)
		{
			if (dict == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (dict == null)
		{
			return null;
		}
		int count = dict.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, V[][]> item in dict)
		{
			DoElement(typeInfo, item.Key);
			DoJaggedArray(item.Value, verifyField: false);
		}
		return dict;
	}

	public Dictionary<K, Dictionary<V, Dictionary<U, T>>> DoDoubleNestedDictionary<K, V, U, T>(Dictionary<K, Dictionary<V, Dictionary<U, T>>> dict, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		Type typeFromHandle = typeof(Dictionary<K, Dictionary<V, Dictionary<U, T>>>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeof(IDictionary<V, Dictionary<U, T>>).Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			Dictionary<K, Dictionary<V, Dictionary<U, T>>> dictionary = (Dictionary<K, Dictionary<V, Dictionary<U, T>>>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				Dictionary<V, Dictionary<U, T>> value2 = DoNestedDictionary<V, U, T>(null, verifyField: false);
				dictionary.Add((K)obj, value2);
			}
			return dictionary;
		}
		if (mode == Mode.Save)
		{
			if (dict == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (dict == null)
		{
			return null;
		}
		int count = dict.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, Dictionary<V, Dictionary<U, T>>> item in dict)
		{
			DoElement(typeInfo, item.Key);
			DoNestedDictionary(item.Value, verifyField: false);
		}
		return dict;
	}

	public Dictionary<K, Dictionary<V, List<U>>> DoNestedMultiMap<K, V, U>(Dictionary<K, Dictionary<V, List<U>>> dict, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		Type typeFromHandle = typeof(Dictionary<K, Dictionary<V, List<U>>>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeof(Dictionary<V, List<U>>).Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			Dictionary<K, Dictionary<V, List<U>>> dictionary = (Dictionary<K, Dictionary<V, List<U>>>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				Dictionary<V, List<U>> value2 = DoMultiMap<V, U>(null, verifyField: false);
				dictionary.Add((K)obj, value2);
			}
			return dictionary;
		}
		if (mode == Mode.Save)
		{
			if (dict == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (dict == null)
		{
			return null;
		}
		int count = dict.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, Dictionary<V, List<U>>> item in dict)
		{
			DoElement(typeInfo, item.Key);
			DoMultiMap(item.Value, verifyField: false);
		}
		return dict;
	}

	public Dictionary<K, Dictionary<V, U>> DoNestedDictionary<K, V, U>(Dictionary<K, Dictionary<V, U>> dict, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		Type typeFromHandle = typeof(Dictionary<K, Dictionary<V, U>>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeof(IDictionary<V, U>).Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			Dictionary<K, Dictionary<V, U>> dictionary = (Dictionary<K, Dictionary<V, U>>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				Dictionary<V, U> value2 = DoDictionary<V, U>(null, verifyField: false);
				dictionary.Add((K)obj, value2);
			}
			return dictionary;
		}
		if (mode == Mode.Save)
		{
			if (dict == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (dict == null)
		{
			return null;
		}
		int count = dict.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, Dictionary<V, U>> item in dict)
		{
			DoElement(typeInfo, item.Key);
			DoDictionary(item.Value, verifyField: false);
		}
		return dict;
	}

	public SortedList<K, V> DoSortedList<K, V>(SortedList<K, V> sortedList, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		GetStaticTypeInfo<V>(out var typeInfo2);
		Type typeFromHandle = typeof(SortedList<K, V>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeInfo2.Type.Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			SortedList<K, V> sortedList2 = (SortedList<K, V>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				object obj2 = DoElement(typeInfo2, default(V));
				if (typeInfo.Type.IsEnum)
				{
					sortedList2.Add((K)obj, (V)obj2);
				}
				else
				{
					sortedList2.Add((K)obj, (V)obj2);
				}
			}
			return sortedList2;
		}
		if (mode == Mode.Save)
		{
			if (sortedList == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (sortedList == null)
		{
			return null;
		}
		int count = sortedList.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, V> sorted in sortedList)
		{
			DoElement(typeInfo, sorted.Key);
			DoElement(typeInfo2, sorted.Value);
		}
		return sortedList;
	}

	public Dictionary<K, V> DoDictionary<K, V>(Dictionary<K, V> dict, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		GetStaticTypeInfo<V>(out var typeInfo2);
		Type typeFromHandle = typeof(Dictionary<K, V>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeInfo2.Type.Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			Dictionary<K, V> dictionary = (Dictionary<K, V>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				object obj2 = DoElement(typeInfo2, default(V));
				if (typeInfo.Type.IsEnum)
				{
					dictionary.Add((K)obj, (V)obj2);
				}
				else
				{
					dictionary.Add((K)obj, (V)obj2);
				}
			}
			return dictionary;
		}
		if (mode == Mode.Save)
		{
			if (dict == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (dict == null)
		{
			return null;
		}
		int count = dict.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, V> item in dict)
		{
			DoElement(typeInfo, item.Key);
			DoElement(typeInfo2, item.Value);
		}
		return dict;
	}

	public SortedDictionary<K, V> DoSortedDictionary<K, V>(SortedDictionary<K, V> dict, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		GetStaticTypeInfo<V>(out var typeInfo2);
		Type typeFromHandle = typeof(SortedDictionary<K, V>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeInfo2.Type.Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			SortedDictionary<K, V> sortedDictionary = (SortedDictionary<K, V>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				object obj2 = DoElement(typeInfo2, default(V));
				if (typeInfo.Type.IsEnum)
				{
					sortedDictionary.Add((K)obj, (V)obj2);
				}
				else
				{
					sortedDictionary.Add((K)obj, (V)obj2);
				}
			}
			return sortedDictionary;
		}
		if (mode == Mode.Save)
		{
			if (dict == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (dict == null)
		{
			return null;
		}
		int count = dict.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, V> item in dict)
		{
			DoElement(typeInfo, item.Key);
			DoElement(typeInfo2, item.Value);
		}
		return dict;
	}

	public SerializableDictionary<K, V> DoSerializableDictionary<K, V>(SerializableDictionary<K, V> dict, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		GetStaticTypeInfo<V>(out var typeInfo2);
		Type typeFromHandle = typeof(SerializableDictionary<K, V>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeInfo2.Type.Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			SerializableDictionary<K, V> serializableDictionary = (SerializableDictionary<K, V>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				object obj2 = DoElement(typeInfo2, default(V));
				if (typeInfo.Type.IsEnum)
				{
					serializableDictionary.Add((K)obj, (V)obj2);
				}
				else
				{
					serializableDictionary.Add((K)obj, (V)obj2);
				}
			}
			return serializableDictionary;
		}
		if (mode == Mode.Save)
		{
			if (dict == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (dict == null)
		{
			return null;
		}
		int count = dict.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, V> item in dict)
		{
			DoElement(typeInfo, item.Key);
			DoElement(typeInfo2, item.Value);
		}
		return dict;
	}

	public SerializableDictionary<K, List<V>> DoSerializableMultiMap<K, V>(SerializableDictionary<K, List<V>> dict, bool verifyField = true)
	{
		GetStaticTypeInfo<K>(out var typeInfo);
		Type typeFromHandle = typeof(SerializableDictionary<K, List<V>>);
		string value = typeFromHandle.Name + typeInfo.Type.Name + typeof(List<V>).Name;
		if (verifyField)
		{
			VerifyField(typeFromHandle);
		}
		if (mode == Mode.Load)
		{
			if (m_reader.ReadString() == "null")
			{
				return null;
			}
			int num = DoInt32(0, verifyField: false);
			SerializableDictionary<K, List<V>> serializableDictionary = (SerializableDictionary<K, List<V>>)Activator.CreateInstance(typeFromHandle);
			for (int i = 0; i < num; i++)
			{
				object obj = DoElement(typeInfo, default(K));
				List<V> value2 = DoList<V>(null, verifyField: false);
				serializableDictionary.Add((K)obj, value2);
			}
			return serializableDictionary;
		}
		if (mode == Mode.Save)
		{
			if (dict == null)
			{
				m_writer.Write("null");
			}
			else
			{
				m_writer.Write(value);
			}
		}
		if (dict == null)
		{
			return null;
		}
		int count = dict.Count;
		count = DoInt32(count, verifyField: false);
		foreach (KeyValuePair<K, List<V>> item in dict)
		{
			DoElement(typeInfo, item.Key);
			DoList(item.Value, verifyField: false);
		}
		return dict;
	}

	public static FieldInfo[] GetFieldInfosIncludingBaseClasses(Type type, BindingFlags bindingFlags)
	{
		FieldInfo[] fields = type.GetFields(bindingFlags);
		if (type.BaseType == typeof(object))
		{
			return fields;
		}
		Type type2 = type;
		FieldInfoComparer comparer = new FieldInfoComparer();
		HashSet<FieldInfo> hashSet = new HashSet<FieldInfo>(fields, comparer);
		while (type2 != typeof(object))
		{
			fields = type2.GetFields(bindingFlags);
			hashSet.UnionWith(fields);
			type2 = type2.BaseType;
		}
		return hashSet.ToArray();
	}

	private void EndVerifyISnapshot()
	{
	}

	private bool VerifyParameterlessConstructorsExist()
	{
		return true;
	}

	private void ReturnToVerifyingPreviousClass()
	{
		if (snapshotClassesBeingVerified.Count > 0)
		{
			snapshotClassBeingVerified = snapshotClassesBeingVerified.Peek();
		}
		else
		{
			snapshotClassBeingVerified = null;
		}
	}

	public ISnapshot DoISnapshot<T>(T snap, bool verifyField = true) where T : ISnapshot
	{
		Type typeFromHandle = typeof(T);
		return DoISnapshot(snap, typeFromHandle, verifyField);
	}

	public ISnapshot DoISnapshot(ISnapshot instanceToSnapshot, Type staticOrDynamicType, bool verifyField = true)
	{
		ISnapshot snapshot = null;
		if (mode == Mode.Load)
		{
			string text = m_reader.ReadString();
			if (text == "null")
			{
				return null;
			}
			snapshot = ((!staticOrDynamicType.IsInterface && !staticOrDynamicType.IsAbstract) ? (Activator.CreateInstance(staticOrDynamicType) as ISnapshot) : (Activator.CreateInstance(Type.GetType(text)) as ISnapshot));
			snapshot.DoVersion(this);
			snapshot = snapshot.DoSnapshot(this);
		}
		else if (mode == Mode.Save)
		{
			if (verifyField)
			{
				VerifyField(staticOrDynamicType);
			}
			Type dynamicType = null;
			StartVerifyISnapshot(instanceToSnapshot, out dynamicType);
			if (instanceToSnapshot == null)
			{
				m_writer.Write("null");
				snapshot = null;
			}
			else
			{
				if (instanceToSnapshot.IsSnapshotted)
				{
					throw new Exception("calling Do(ISnapshot) on the same object twice. That's wrong.");
				}
				if (staticOrDynamicType.IsInterface || staticOrDynamicType.IsAbstract)
				{
					if (dynamicType == null)
					{
						dynamicType = instanceToSnapshot.GetType();
					}
					m_writer.Write(dynamicType.FullName);
				}
				else
				{
					m_writer.Write(staticOrDynamicType.Name);
				}
				instanceToSnapshot.DoVersion(this);
				snapshot = instanceToSnapshot.DoSnapshot(this);
				instanceToSnapshot.IsSnapshotted = true;
				snapshot.IsSnapshotted = true;
				RegisterSnapshottedISnapshot(snapshot, dynamicType ?? staticOrDynamicType);
			}
			EndVerifyISnapshot();
		}
		return snapshot;
	}

	public void RegisterLoadPostProcessCall(ISnapshot instance)
	{
	}

	private void RegisterSnapshottedISnapshot(ISnapshot instance, Type type)
	{
	}

	private void VerifyLoadPostProcessCallsComplete()
	{
	}

	private void StartVerifyISnapshot(ISnapshot instanceToSnapshot, out Type dynamicType)
	{
		dynamicType = null;
	}

	public void Postpone<T>(T a)
	{
	}

	public void Ignore<T>(T a)
	{
	}

	public static void Log(string message, LogPriority p = LogPriority.infinity)
	{
		string text = "";
		for (int i = 0; i < (int)(3 - p); i++)
		{
			text += "    ";
		}
		text += message;
		if (p >= m_logPriority)
		{
			Console.WriteLine(text);
		}
	}

	public Id? SnapshotID<T, Id>(T instance) where T : ILookUp<T, Id> where Id : struct, IComparable, IFormattable, IConvertible
	{
		VerifyField(typeof(T));
		VerifyField(typeof(Id));
		if (instance != null)
		{
			return DoEnumNullable<Id>(instance.ID, verifyField: false);
		}
		return DoEnumNullable<Id>(null, verifyField: false);
	}
}
