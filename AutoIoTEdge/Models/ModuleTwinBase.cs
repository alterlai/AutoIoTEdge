using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Reflection;

namespace AutoIoTEdge.Models;

public abstract class ModuleTwinBase
{
	/// <summary>
	/// The culture used for numeric parsing (Dutch culture).
	/// </summary>
	private static readonly CultureInfo DutchCulture = new CultureInfo("nl-NL");

	/// <summary>
	/// Checks if the specified type is a numeric type.
	/// </summary>
	private static readonly HashSet<Type> NumericTypes = new()
	{
		typeof(int), typeof(long), typeof(short), typeof(byte),
		typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte),
		typeof(double), typeof(float), typeof(decimal)
	};

	private bool IsNumericType(Type type)
	{
		return NumericTypes.Contains(type);
	}

	/// <summary>
	/// Populates the twin properties from a TwinCollection using reflection.
	/// </summary>
	public void UpdateFromTwin(TwinCollection collection)
	{
		foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
		{
			if (collection.Contains(prop.Name) && prop.CanWrite)
			{
				var value = collection[prop.Name];
				if (value != null)
				{
					// Check if the property is static by examining its getter method
					var isStatic = prop.GetMethod?.IsStatic == true;
					var instance = isStatic ? null : this;
					
					// Handle special case for List<string> and other complex types
					if (prop.PropertyType == typeof(List<string>) || prop.PropertyType == typeof(IList<string>))
					{
						var listValue = ParseStringList(value);
						prop.SetValue(instance, listValue);
					}
					else
					{
						var convertedValue = ParseWithCulture(value, prop.PropertyType);
						prop.SetValue(instance, convertedValue);
					}
				}
			}
		}
	}

	/// <summary>
	/// Converts a value to the specified type using culture-aware conversion for numeric types.
	/// </summary>
	private object ParseWithCulture(object value, Type targetType)
	{
		string stringValue = value as string ?? value?.ToString() ?? "";

		// Handle nullable types
		var underlyingType = Nullable.GetUnderlyingType(targetType);
		if (underlyingType != null)
		{
			targetType = underlyingType;
		}

		 // Handle JValue case - extract the actual value
		if (value is JValue jValue)
		{
			value = jValue.Value ?? "";
		}
		else if (value is JObject jObject)
		{
			// If the target type is a class or interface, deserialize the JObject
			if (!targetType.IsPrimitive && targetType != typeof(string))
			{
				return jObject.ToObject(targetType);
			}
		}
		else if (value is JArray jArray)
		{
			// If the target type is a collection, deserialize the JArray
			if (typeof(System.Collections.IEnumerable).IsAssignableFrom(targetType) && targetType != typeof(string))
			{
				return jArray.ToObject(targetType);
			}
		}

		// Handle Enum types
		if (targetType.IsEnum)
		{
			if (value is string && !int.TryParse(stringValue, out _))
			{
				return Enum.Parse(targetType, stringValue, true);
			}
			return Enum.ToObject(targetType, Convert.ChangeType(value, Enum.GetUnderlyingType(targetType)));
		}

		// For numeric types, use culture-aware conversion
		if (IsNumericType(targetType) && value is string)
		{
			return ConvertNumericString(stringValue, targetType);
		}

		// For non-numeric types or non-string values, use default conversion
		return Convert.ChangeType(value, targetType);
	}

	/// <summary>
	/// Converts a string to a numeric type using Dutch culture.
	/// </summary>
	private object ConvertNumericString(string value, Type targetType)
	{
		return Convert.ChangeType(value, targetType, DutchCulture);
	}

	/// <summary>
	/// Parses a value into a List<string>, handling various input formats.
	/// </summary>
	private List<string> ParseStringList(object value)
	{
		Console.WriteLine($"ParseStringList called with value: {value} (Type: {value.GetType()})");
		
		// Handle JArray case first
		if (value is JArray jArray)
		{
			Console.WriteLine("Value is JArray");
			return jArray.ToObject<List<string>>() ?? new List<string>();
		}

		/// If the value is packed in a jValue, extract the string.
		if(value is JValue jValue)
		{
			value = jValue.Value ?? "";
		}
		
		// Handle string case (JSON string)
		if (value is string stringValue)
		{
			Console.WriteLine($"Value is string: '{stringValue}'");
			// Try to parse as JSON array first
			if (stringValue.Trim().StartsWith("[") && stringValue.Trim().EndsWith("]"))
			{
				Console.WriteLine("String looks like JSON array, attempting to parse");
				try
				{
					var result = JsonConvert.DeserializeObject<List<string>>(stringValue) ?? new List<string>();
					Console.WriteLine($"JSON parsing successful, result count: {result.Count}");
					return result;
				}
				catch (JsonException ex)
				{
					Console.WriteLine($"JSON parsing failed: {ex.Message}");
					// If JSON parsing fails, treat as single string item
					return new List<string> { stringValue };
				}
			}
			else
			{
				Console.WriteLine("String does not look like JSON array, treating as single item");
				// Not a JSON array format, treat as single string item
				return new List<string> { stringValue };
			}
		}
		
		// Handle IEnumerable<object> case
		if (value is IEnumerable<object> enumerable)
		{
			Console.WriteLine("Value is IEnumerable<object>");
			return enumerable.Select(x => x?.ToString() ?? string.Empty).ToList();
		}
		
		Console.WriteLine("Using default case");
		// Default case: convert to string and create single-item list
		return new List<string> { value.ToString() ?? string.Empty };
	}

	/// <summary>
	/// Populates the twin properties from a configuration section (e.g., appsettings.json).
	/// </summary>
	public void UpdateFromConfiguration(IConfiguration config)
	{
		foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
		{
			var value = config[prop.Name];
			if (value != null && prop.CanWrite)
			{
				// Check if the property is static by examining its getter method
				var isStatic = prop.GetMethod?.IsStatic == true;
				var instance = isStatic ? null : this;
				var convertedValue = ParseWithCulture(value, prop.PropertyType);
				prop.SetValue(instance, convertedValue);
			}
		}
	}

	/// <summary>
	/// Converts the twin properties to a TwinCollection.
	/// </summary>
	public TwinCollection ToTwinCollection()
	{
		var collection = new TwinCollection();
		foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
		{
			// Check if the property is static by examining its getter method
			var isStatic = prop.GetMethod?.IsStatic == true;
			var instance = isStatic ? null : this;
			collection[prop.Name] = prop.GetValue(instance);
		}
		return collection;
	}
}