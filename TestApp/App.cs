using AutoIoTEdge;
using AutoIoTEdge.Interfaces;
using AutoIoTEdge.Services;
using ExampleApp.Models;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;

namespace ExampleApp;
public class App(IIotEdgeService<ModuleTwin> edgeService, ILogger<App> _logger)
{
	public async Task RunAsync()
	{
		edgeService.ModuleTwinUpdated += OnTwinUpdated;

		Console.WriteLine("EventHubReceiver is running...");
		_logger.LogInformation($"{ModuleTwin.TestVariable}");

	}
	private void OnTwinUpdated(object? sender, ModuleTwin twin)
	{
		// Handle twin update
		_logger.LogInformation($"{DateTime.UtcNow}: Moduletwin changed: {twin.ToTwinCollection()}");
		_logger.LogInformation($"{DateTime.UtcNow}: Variable in static: {ModuleTwin.TestVariable}");
	}
}
