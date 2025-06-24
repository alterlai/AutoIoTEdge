using AutoIoTEdge.Models;

namespace EventHubReceiver.Services.IoT
{
	public interface IIotEdgeService<TTwin>
		where TTwin : ModuleTwinBase
	{
		/// <summary>
		/// Get the module twin.
		/// </summary>
		/// <returns></returns>
		public TTwin GetTwin();

		/// <summary>
		/// Sends a message asynchronously to the specified device output route.
		/// </summary>
		/// <remarks>The devicename is the name of the device that produced the data. This data will later be used to store
		/// the data in the correct container.</remarks>
		/// <param name="outputName">The name of the output</param>
		/// <param name="message">The message to send, serialized as JSON. Cannot be null.</param>
		/// <returns>A task representing the asynchronous operation. The task completes when the message is sent or an error occurs.</returns>
		public Task SendEventAsync(string outputName, string message);
	}
}
