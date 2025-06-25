using AutoIoTEdge.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

namespace AutoIoTEdge.Services
{
	public interface IIotEdgeService<TTwin>
		where TTwin : ModuleTwinBase
	{
		public event EventHandler<TTwin>? ModuleTwinUpdated;

		public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext);
		public Task SendEventAsync(string outputName, Message message);
		public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken);
		public Task SetMethodHandlerAsync(string methodname, MethodCallback methodCallback, object userContext);
		public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest, CancellationToken cancellationToken);
		public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest);

	}
}
