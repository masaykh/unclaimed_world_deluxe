using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UWGame.SimSide;

public sealed class EnumComparer<TEnum> : IEqualityComparer<TEnum> where TEnum : struct, IComparable, IConvertible, IFormattable
{
	private static readonly Func<TEnum, TEnum, bool> equals;

	private static readonly Func<TEnum, int> getHashCode;

	public static readonly EnumComparer<TEnum> Instance;

	static EnumComparer()
	{
		getHashCode = generateGetHashCode();
		equals = generateEquals();
		Instance = new EnumComparer<TEnum>();
	}

	private EnumComparer()
	{
		assertTypeIsEnum();
		assertUnderlyingTypeIsSupported();
	}

	public bool Equals(TEnum x, TEnum y)
	{
		return equals(x, y);
	}

	public int GetHashCode(TEnum obj)
	{
		return getHashCode(obj);
	}

	private static void assertTypeIsEnum()
	{
		if (typeof(TEnum).IsEnum)
		{
			return;
		}
		throw new NotSupportedException($"The type parameter {typeof(TEnum)} is not an Enum. LcgEnumComparer supports Enums only.");
	}

	private static void assertUnderlyingTypeIsSupported()
	{
		Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
		if (((ICollection<Type>)new Type[8]
		{
			typeof(byte),
			typeof(sbyte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong)
		}).Contains(underlyingType))
		{
			return;
		}
		throw new NotSupportedException($"The underlying type of the type parameter {typeof(TEnum)} is {underlyingType}. LcgEnumComparer only supports Enums with underlying type of byte, sbyte, short, ushort, int, uint, long, or ulong.");
	}

	private static Func<TEnum, TEnum, bool> generateEquals()
	{
		return ((Expression<Func<TEnum, TEnum, bool>>)((TEnum x, TEnum y) => (object)x == (object)y)).Compile();
	}

	private static Func<TEnum, int> generateGetHashCode()
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(TEnum), "obj");
		Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
		UnaryExpression instance = Expression.Convert(parameterExpression, underlyingType);
		MethodInfo method = underlyingType.GetMethod("GetHashCode");
		return Expression.Lambda<Func<TEnum, int>>(Expression.Call(instance, method), new ParameterExpression[1] { parameterExpression }).Compile();
	}
}
