using System.Collections;
using System.ComponentModel;

namespace TestApp.Extensions
{
	public static class ObjectExtensions
	{
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
					list.Add(Convert.ChangeType(item, elementType));
				}
				return (T)list;
			}

			return (T)Convert.ChangeType(value, typeof(T));
		}
	}
}
