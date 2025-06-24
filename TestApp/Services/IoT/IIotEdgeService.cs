using TestApp.Models;

namespace TestApp.Services.IoT
{
	public interface IIotEdgeService
	{
		/// <summary>
		/// Get the module twin.
		/// </summary>
		/// <returns></returns>
		public ModuleTwin GetTwin();

		/// <summary>
		/// Sends a message asynchronously to the specified device output route.
		/// </summary>
		/// <remarks>The devicename is the name of the device that produced the data. This data will later be used to store
		/// the data in the correct container.</remarks>
		/// <param name="message">The message to send, serialized as JSON. Cannot be null.</param>
		/// <param name="deviceName">The name of the origin device. Cannot be null or empty.</param>
		/// <returns>A task representing the asynchronous operation. The task completes when the message is sent or an error occurs.</returns>
		public Task SendMessageAsync(string message, string deviceName);
	}
}
