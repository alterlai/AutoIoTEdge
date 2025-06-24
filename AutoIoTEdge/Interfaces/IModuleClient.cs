using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

namespace AutoIoTEdge.Interfaces;
public interface IModuleClient : IDisposable
{
	Task OpenAsync();
	Task<Twin> GetTwinAsync();
	Task CreateFromEnvironmentAsync(ITransportSettings[] settings);
	Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext);
	Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties);
	void SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler statusChangesHandler);
	public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext);
	public Task SendEventAsync(string outputName, Message message);
	public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken);
	public Task SetMethodHandlerAsync(string methodname, MethodCallback methodCallback, object userContext);
	public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest, CancellationToken cancellationToken);
	public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest);
}