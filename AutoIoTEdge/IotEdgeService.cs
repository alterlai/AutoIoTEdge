using AutoIoTEdge.Interfaces;
using AutoIoTEdge.Models;
using EventHubReceiver.Services.IoT;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Text.Unicode;

namespace AutoIoTEdge;
public class IotEdgeService<TTwin> : IIotEdgeService<TTwin>, IHostedService where TTwin : ModuleTwinBase, new()
{
    private readonly ILogger _logger;
    private readonly IModuleClient _moduleClient;
    private readonly IConfiguration? _configuration;
    private TTwin _twin = new();
    private bool _isDevelopment;

    public event EventHandler<TTwin>? ModuleTwinUpdated;

    public bool IsConnected { get; private set; }
    public TTwin Twin => _twin;

    public IotEdgeService(ILogger<IotEdgeService<TTwin>> logger, IModuleClient moduleClient, IConfiguration? configuration = null)
    {
        _logger = logger;
        _moduleClient = moduleClient;
        _configuration = configuration;
        _isDevelopment = configuration?["ASPNETCORE_ENVIRONMENT"] == "Development";
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StartInternalAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Cleanup if needed
        return Task.CompletedTask;
    }

    private async Task StartInternalAsync()
    {
        _moduleClient.SetConnectionStatusChangesHandler((status, reason) =>
        {
            _logger.LogWarning($"{DateTime.UtcNow}: Connection changed: Status: {status} Reason: {reason}");
            IsConnected = status == ConnectionStatus.Connected;
        });

        await _moduleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);
        await _moduleClient.OpenAsync();

        if (_isDevelopment && _configuration != null)
        {
            _logger.LogInformation("Development environment detected. Loading twin from appsettings.json.");
            _twin = new TTwin();
            _twin.UpdateFromConfiguration(_configuration.GetSection("ModuleTwin"));
            OnModuleTwinUpdated();
        }
        else
        {
            await UpdateTwinFromCloud();
        }
    }

    private async Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
    {
        _logger.LogInformation($"{DateTime.UtcNow}: Desired properties update received.");
        _twin = new TTwin();
        _twin.UpdateFromTwin(desiredProperties);

        _logger.LogInformation($"{DateTime.UtcNow}: New twin: {JsonConvert.SerializeObject(_twin)}");

        await _moduleClient.UpdateReportedPropertiesAsync(_twin.ToTwinCollection());
        OnModuleTwinUpdated();
    }

    private void OnModuleTwinUpdated()
    {
        ModuleTwinUpdated?.Invoke(this, _twin);
    }

    private async Task UpdateTwinFromCloud()
    {
        var twin = await _moduleClient.GetTwinAsync();
        _twin = new TTwin();
        _twin.UpdateFromTwin(twin.Properties.Desired);
        await _moduleClient.UpdateReportedPropertiesAsync(_twin.ToTwinCollection());
        OnModuleTwinUpdated();
    }

	public TTwin GetTwin()
	{
        return _twin;
	}

	public Task SendEventAsync(string outputName, string message)
	{
		Message messageBytes = new(Encoding.UTF8.GetBytes(message));
		return SendEventAsync(outputName, messageBytes);
	}

	// Expose IoT Edge features
	public Task SendEventAsync(string outputName, Message message) => _moduleClient.SendEventAsync(outputName, message);
    public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken) => _moduleClient.SendEventAsync(outputName, message, cancellationToken);
    public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext) => _moduleClient.SetInputMessageHandlerAsync(inputName, messageHandler, userContext);
    public Task SetMethodHandlerAsync(string methodName, MethodCallback methodCallback, object userContext) => _moduleClient.SetMethodHandlerAsync(methodName, methodCallback, userContext);
    public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest, CancellationToken cancellationToken) => _moduleClient.InvokeMethodAsync(deviceId, methodRequest, cancellationToken);
    public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest) => _moduleClient.InvokeMethodAsync(deviceId, methodRequest);


}