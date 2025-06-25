using Microsoft.Azure.Devices.Shared;

namespace AutoIoTEdge.Extensions
{
	public static class TwinCollectionExtensions
	{
		public static bool GetValue<T>(this TwinCollection collection, string name, T defaultValue, out T value, string deploymentLocation = "deployment")
		{
			value = defaultValue;
			try
			{
				if (collection.Contains(name) || IsDeploymentPresent(collection, deploymentLocation))
				{
					if (collection.Contains(name))
					{
						value = ((object)collection[name]).ConvertTo<T>();
					}
					else if (!string.IsNullOrEmpty(deploymentLocation) && collection[deploymentLocation][name] != null)
					{
						value = ((object)collection[deploymentLocation][name]).ConvertTo<T>();
					}

					Console.WriteLine($"{name} changed to {value}");
					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Getting {name} from desired properties threw an exception. Searched for {name} and {deploymentLocation}.{name}. Exception: {ex.Message}");
				return false;
			}

		}

		private static bool IsDeploymentPresent(TwinCollection properties, string deploymentLocation)
		{
			return properties.Contains(deploymentLocation) && properties[deploymentLocation] != null;
		}
	}
}
