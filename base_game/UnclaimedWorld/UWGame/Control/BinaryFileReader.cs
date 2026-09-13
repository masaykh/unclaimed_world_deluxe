using System;
using System.IO;

namespace UWGame.Control;

public class BinaryFileReader
{
	private BinaryReader reader;

	private long readPosition;

	private long documentLength;

	private FileStream inStream;

	public BinaryFileReader(string filePath)
	{
		inStream = File.OpenRead(filePath);
		reader = new BinaryReader(inStream);
		documentLength = inStream.Length;
		readPosition = 0L;
	}

	public bool HasReachedEndOfFile()
	{
		if (readPosition < documentLength)
		{
			return false;
		}
		return true;
	}

	public bool ReadBool()
	{
		AdvanceReadPosition(1L);
		return reader.ReadBoolean();
	}

	public int ReadInt()
	{
		AdvanceReadPosition(4L);
		return reader.ReadInt32();
	}

	public long ReadLong()
	{
		AdvanceReadPosition(8L);
		return reader.ReadInt64();
	}

	public float ReadFloat()
	{
		AdvanceReadPosition(4L);
		return reader.ReadSingle();
	}

	public short ReadShort()
	{
		AdvanceReadPosition(2L);
		return reader.ReadInt16();
	}

	public char ReadChar()
	{
		AdvanceReadPosition(2L);
		return reader.ReadChar();
	}

	private void AdvanceReadPosition(long amountToAdvance)
	{
		readPosition += amountToAdvance;
		if (readPosition > documentLength)
		{
			throw new Exception("Tried to read outside file!");
		}
	}

	public void Close()
	{
		reader.Close();
	}
}
