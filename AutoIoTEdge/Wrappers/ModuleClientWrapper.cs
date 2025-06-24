using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using AutoIoTEdge.Interfaces;

namespace AutoIoTEdge.Wrappers;
/// <summary>
/// Wrapper voor de module client. Deze is nodig om de module client te kunnen mocken in de tests.
/// </summary>
public class ModuleClientWrapper : IModuleClient
{
	private ModuleClient _moduleClient;

	public ModuleClientWrapper(ModuleClient moduleClient)
	{
		_moduleClient = moduleClient;
	}

	public Task OpenAsync() => _moduleClient.OpenAsync();
	public Task<Twin> GetTwinAsync() => _moduleClient.GetTwinAsync();
	public Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext)
		=> _moduleClient.SetDesiredPropertyUpdateCallbackAsync(callback, userContext);
	public async Task CreateFromEnvironmentAsync(ITransportSettings[] settings)
	{
		_moduleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
	}
	public Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties)
		=> _moduleClient.UpdateReportedPropertiesAsync(reportedProperties);
	public void SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler statusChangesHandler)
		=> _moduleClient.SetConnectionStatusChangesHandler(statusChangesHandler);
	public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext)
		=> _moduleClient.SetInputMessageHandlerAsync(inputName, messageHandler, userContext);
	public Task SendEventAsync(string outputName, Message message)
		=> _moduleClient.SendEventAsync(outputName, message);
	public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken)
		=> _moduleClient.SendEventAsync(outputName, message, cancellationToken);
	public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest, CancellationToken cancellationToken)
	=> _moduleClient.InvokeMethodAsync(deviceId, methodRequest, cancellationToken);
	public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest)
		=> _moduleClient.InvokeMethodAsync(deviceId, methodRequest);
	public Task SetMethodHandlerAsync(string methodname, MethodCallback methodCallback, object userContext)
		=> _moduleClient.SetMethodHandlerAsync(methodname, methodCallback, userContext);
	public void Dispose() => _moduleClient.Dispose();
}