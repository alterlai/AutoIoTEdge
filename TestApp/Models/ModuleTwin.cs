using Microsoft.Azure.Devices.Shared;
using TestApp.Extensions;

namespace TestApp.Models;
public record ModuleTwin
{
	// Default values
	private static string _defaultEventHubConnectionString = string.Empty;
	private static string _defaultFileStorageLocation = string.Empty;

	public static string EventHubConnectionString { get; set; } = _defaultEventHubConnectionString;
	public static string FileStorageLocation { get; set; } = _defaultFileStorageLocation;

	/// <summary>
	/// Update the settings with new values when they have been updated in the cloud.
	/// </summary>
	/// <param name="collection">new incoming settings.</param>
	/// <returns></returns>
	public TwinCollection SetSettings(TwinCollection collection)
	{
		var reportedProperties = new TwinCollection();
		string value = string.Empty;

		if (collection.GetValue("EventHubConnectionString", _defaultEventHubConnectionString, out value))
		{
			EventHubConnectionString = value;
			reportedProperties["EventHubConnectionString"] = EventHubConnectionString;
		}
		if (collection.GetValue("FileStorageLocation", _defaultFileStorageLocation, out value))
		{
			FileStorageLocation = value;
			reportedProperties["FileStorageLocation"] = FileStorageLocation;
		}

		return reportedProperties;
	}
}
