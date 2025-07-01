using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace AutoIoTEdge.Extensions
{
	public static class ObjectExtensions
	{
		/// <summary>
		/// The culture used for numeric parsing (Dutch culture).
		/// </summary>
		private static readonly CultureInfo DutchCulture = new CultureInfo("nl-NL");

		public static T ConvertTo<T>(this object value)
		{
			if (value is T variable) return variable;

			// Handling Nullable types i.e, int?, double?, bool? ..etc
			if (Nullable.GetUnderlyingType(typeof(T)) != null)
			{
				return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value)!;
			}

			// Handling IEnumerable types
			if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
			{
				var elementType = typeof(T).GetGenericArguments()[0];
				var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
				foreach (var item in (IEnumerable)value)
				{
					list.Add(ConvertWithCulture(item, elementType));
				}
				return (T)list;
			}

			return (T)ConvertWithCulture(value, typeof(T));
		}

		/// <summary>
		/// Converts a value to the specified type using culture-aware conversion for numeric types.
		/// </summary>
		private static object ConvertWithCulture(object value, Type targetType)
		{
			// Handle nullable types
			var underlyingType = Nullable.GetUnderlyingType(targetType);
			if (underlyingType != null)
			{
				targetType = underlyingType;
			}

			// For numeric types, use culture-aware conversion
			if (IsNumericType(targetType) && value is string stringValue)
			{
				return ConvertNumericString(stringValue, targetType);
			}

			// For non-numeric types or non-string values, use default conversion
			return Convert.ChangeType(value, targetType);
		}

		/// <summary>
		/// Checks if the specified type is a numeric type.
		/// </summary>
		private static bool IsNumericType(Type type)
		{
			return type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
				   type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte) ||
				   type == typeof(double) || type == typeof(float) || type == typeof(decimal);
		}

		/// <summary>
		/// Converts a string to a numeric type using Dutch culture.
		/// </summary>
		private static object ConvertNumericString(string value, Type targetType)
		{
			return targetType.Name switch
			{
				nameof(Int32) => int.Parse(value, DutchCulture),
				nameof(Int64) => long.Parse(value, DutchCulture),
				nameof(Int16) => short.Parse(value, DutchCulture),
				nameof(Byte) => byte.Parse(value, DutchCulture),
				nameof(UInt32) => uint.Parse(value, DutchCulture),
				nameof(UInt64) => ulong.Parse(value, DutchCulture),
				nameof(UInt16) => ushort.Parse(value, DutchCulture),
				nameof(SByte) => sbyte.Parse(value, DutchCulture),
				nameof(Double) => double.Parse(value, DutchCulture),
				nameof(Single) => float.Parse(value, DutchCulture),
				nameof(Decimal) => decimal.Parse(value, DutchCulture),
				_ => Convert.ChangeType(value, targetType, DutchCulture)
			};
		}
	}
}
