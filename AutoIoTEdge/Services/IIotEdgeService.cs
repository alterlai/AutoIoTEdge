using AutoIoTEdge.Models;
using Microsoft.Azure.Devices.Client;

namespace AutoIoTEdge.Services
{
	/// <summary>
	/// Non-generic interface for IoT Edge service operations.
	/// Use this interface for dependency injection to avoid specifying the generic type parameter everywhere.
	/// </summary>
	public interface IIotEdgeService
	{
		public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext);
		public Task SendEventAsync(string outputName, Message message);
		public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken);
		public Task SetMethodHandlerAsync(string methodname, MethodCallback methodCallback, object userContext);
		public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest, CancellationToken cancellationToken);
		public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest);
		public ModuleClient GetBaseModuleClient();
	}

	/// <summary>
	/// Generic interface for IoT Edge service with typed module twin events.
	/// This extends the non-generic interface to provide type-safe twin update events.
	/// </summary>
	/// <typeparam name="TTwin">The type of module twin that derives from ModuleTwinBase.</typeparam>
	public interface IIotEdgeService<TTwin> : IIotEdgeService
		where TTwin : ModuleTwinBase
	{
		public event EventHandler<TTwin>? ModuleTwinUpdated;
	}
}
