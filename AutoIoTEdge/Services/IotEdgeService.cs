using AutoIoTEdge.Interfaces;
using AutoIoTEdge.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Text.Unicode;

namespace AutoIoTEdge.Services;
public class IotEdgeService<TTwin> : IIotEdgeService<TTwin> where TTwin : ModuleTwinBase, new()
{
    private readonly ILogger<IotEdgeService<TTwin>> _logger;
    private readonly IConfiguration _configuration;
    private ModuleClient _moduleClient;
    private TTwin _twin = new();

    public event EventHandler<TTwin>? ModuleTwinUpdated;

    public bool IsConnected { get; private set; }
    public TTwin Twin => _twin;

    public IotEdgeService(ILogger<IotEdgeService<TTwin>> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        StartInternalAsync().GetAwaiter().GetResult(); // Synchronous call to ensure the client is initialized before any method calls
	}

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _moduleClient?.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Initializes and starts the internal module client, setting up connection handlers, desired property update
    /// callbacks,  and loading the module twin configuration.
    /// </summary>
    /// <remarks>This method creates a module client using MQTT transport settings and establishes a
    /// connection to the IoT Hub.  It sets up a connection status change handler to monitor connectivity and updates
    /// the module twin configuration  either from the cloud or from local appsettings.json in a development
    /// environment.</remarks>
    /// <returns>A task that represents the asynchronous operation of starting the module client.</returns>
    private async Task StartInternalAsync()
    {
		// Create the ModuleClient here
		var mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
		ITransportSettings[] settings = { mqttSetting };
		_moduleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);

		_moduleClient.SetConnectionStatusChangesHandler((status, reason) =>
        {
            _logger.LogWarning($"{DateTime.UtcNow}: Connection changed: Status: {status} Reason: {reason}");
            IsConnected = status == ConnectionStatus.Connected;
        });

        await _moduleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);
        await _moduleClient.OpenAsync();
        await UpdateTwinFromCloud();
		OnModuleTwinUpdated();
	}

    /// <summary>
    /// Handles updates to the desired properties of the module twin.
    /// </summary>
    /// <remarks>This method processes the desired properties update by applying the changes to the internal
    /// twin representation and reporting the updated properties back to the IoT Hub. It also triggers any necessary
    /// actions based on the updated twin state.</remarks>
    /// <param name="desiredProperties">The collection of desired properties received from the IoT Hub.</param>
    /// <param name="userContext">An optional user-defined context object associated with the operation. Can be <see langword="null"/>.</param>
    /// <returns></returns>
    private async Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
    {
        _logger.LogInformation($"{DateTime.UtcNow}: Desired properties update received: {desiredProperties}");
        _twin = new TTwin();
        _twin.UpdateFromTwin(desiredProperties);

        _logger.LogInformation($"{DateTime.UtcNow}: New twin: {JsonConvert.SerializeObject(_twin)}");

        await _moduleClient.UpdateReportedPropertiesAsync(_twin.ToTwinCollection());
        OnModuleTwinUpdated();
    }

    /// <summary>
    /// Handles the event triggered when the module twin is updated.
    /// </summary>
    /// <remarks>This method invokes the <see cref="ModuleTwinUpdated"/> event, passing the current instance 
    /// and the updated module twin as arguments. Ensure that any subscribers to the event are properly  registered to
    /// handle the updated twin data.</remarks>
    private void OnModuleTwinUpdated()
    {
        ModuleTwinUpdated?.Invoke(this, _twin);
    }

    /// <summary>
    /// Updates the module's twin by retrieving the desired properties from the cloud and synchronizing the reported
    /// properties accordingly.
    /// </summary>
    /// <remarks>This method retrieves the latest desired properties from the cloud, updates the local twin
    /// representation, and reports the updated properties back to the cloud. It also triggers the <see
    /// cref="OnModuleTwinUpdated"/> event to notify that the module twin has been updated.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task UpdateTwinFromCloud()
    {
        var twin = await _moduleClient.GetTwinAsync();
        _twin = new TTwin();
        _twin.UpdateFromTwin(twin.Properties.Desired);
        await _moduleClient.UpdateReportedPropertiesAsync(_twin.ToTwinCollection());
        OnModuleTwinUpdated();
    }

	public Task SendEventAsync(string outputName, string message)
	{
		Message messageBytes = new(Encoding.UTF8.GetBytes(message));
		return SendEventAsync(outputName, messageBytes);
	}

    public ModuleClient GetBaseModuleClient()
    {
        return _moduleClient;
	}

	// Expose IoT Edge features
	public Task SendEventAsync(string outputName, Message message) => _moduleClient.SendEventAsync(outputName, message);
    public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken) => _moduleClient.SendEventAsync(outputName, message, cancellationToken);
    public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext) => _moduleClient.SetInputMessageHandlerAsync(inputName, messageHandler, userContext);
    public Task SetMethodHandlerAsync(string methodName, MethodCallback methodCallback, object userContext) => _moduleClient.SetMethodHandlerAsync(methodName, methodCallback, userContext);
    public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest, CancellationToken cancellationToken) => _moduleClient.InvokeMethodAsync(deviceId, methodRequest, cancellationToken);
    public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest) => _moduleClient.InvokeMethodAsync(deviceId, methodRequest);


}