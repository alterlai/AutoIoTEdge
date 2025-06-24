using AutoIoTEdge.Interfaces;
using AutoIoTEdge.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AutoIoTEdge;
public class IotEdgeService<TTwin> where TTwin : ModelTwinBase, new()
{
    private readonly ILogger _logger;
    private readonly IModuleClient _moduleClient;
    private readonly IConfiguration? _configuration;
    private TTwin _twin = new();
    private bool _isDevelopment;

    public event EventHandler<TTwin>? ModuleTwinUpdated;

    public bool IsConnected { get; private set; }
    public TTwin Twin => _twin;

    public IotEdgeService(ILogger logger, IModuleClient moduleClient, IConfiguration? configuration = null)
    {
        _logger = logger;
        _moduleClient = moduleClient;
        _configuration = configuration;
        _isDevelopment = configuration?["ASPNETCORE_ENVIRONMENT"] == "Development";
    }

    public async Task StartAsync()
    {
        _moduleClient.SetConnectionStatusChangesHandler((status, reason) =>
        {
            _logger.LogWarning($"{DateTime.UtcNow}: Connection changed: Status: {status} Reason: {reason}");
            IsConnected = status == ConnectionStatus.Connected;
        });

        await _moduleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, _moduleClient);
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

    // Expose IoT Edge features
    public Task SendEventAsync(string outputName, Message message) => _moduleClient.SendEventAsync(outputName, message);
    public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken) => _moduleClient.SendEventAsync(outputName, message, cancellationToken);
    public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext) => _moduleClient.SetInputMessageHandlerAsync(inputName, messageHandler, userContext);
    public Task SetMethodHandlerAsync(string methodName, MethodCallback methodCallback, object userContext) => _moduleClient.SetMethodHandlerAsync(methodName, methodCallback, userContext);
    public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest, CancellationToken cancellationToken) => _moduleClient.InvokeMethodAsync(deviceId, methodRequest, cancellationToken);
    public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest) => _moduleClient.InvokeMethodAsync(deviceId, methodRequest);

}