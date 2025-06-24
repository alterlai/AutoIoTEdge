namespace ExampleApp.Models;
/// <summary>
/// General settings that are not in the module twin.
/// </summary>
public class Settings
{
	public static readonly string EventHubConsumerGroup = "hil"; // Consumer group name for Event Hub
	public static readonly string PartitionId = "0"; // Default partition ID to read from
	public static readonly string OutputName = "output"; // Default output name for messages
}
