using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace AutoIoTEdge.Models;

public abstract class ModuleTwinBase
{
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
						prop.SetValue(instance, Convert.ChangeType(value, prop.PropertyType));
					}
				}
			}
		}
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
				prop.SetValue(instance, Convert.ChangeType(value, prop.PropertyType));
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