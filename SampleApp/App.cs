using AutoIoTEdge;
using AutoIoTEdge.Services;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using SampleModule.Models;

namespace SampleModule;

// Approach 1: Inject the non-generic IIotEdgeService
// This is the simplest way - you specify the type only once during registration
public class App(IIotEdgeService edgeService, ILogger<App> _logger)
{
	public async Task RunAsync()
	{
		// You can use all the IoT Edge service methods without specifying the generic type
		// If you need type-safe twin update events, you can cast to the generic interface
		if (edgeService is IIotEdgeService<ModuleTwin> typedService)
		{
			typedService.ModuleTwinUpdated += OnTwinUpdated;
		}

		_logger.LogInformation($"{ModuleTwin.TestVariable}");
		_logger.LogInformation($"{DateTime.UtcNow}: {ModuleTwin.Position.Latitude}");
		_logger.LogInformation($"{DateTime.UtcNow}: {ModuleTwin.Position.Longitude}");
	}

	/// <summary>
	/// This method is called when the module twin is updated.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="twin"></param>
	private void OnTwinUpdated(object? sender, ModuleTwin twin)
	{
		// Handle twin update
		_logger.LogInformation($"{DateTime.UtcNow}: Moduletwin changed: {twin.ToTwinCollection()}");
		_logger.LogInformation($"{DateTime.UtcNow}: Variable in static: {ModuleTwin.TestVariable}");
	}
}

// Approach 2 (Alternative): If you prefer strongly-typed events without casting,
// you can still inject IIotEdgeService<ModuleTwin> - both are registered
/* 
public class App(IIotEdgeService<ModuleTwin> edgeService, ILogger<App> _logger)
{
	public async Task RunAsync()
	{
		// Direct access to ModuleTwinUpdated event with type-safety
		edgeService.ModuleTwinUpdated += OnTwinUpdated;

		_logger.LogInformation($"{ModuleTwin.TestVariable}");
		_logger.LogInformation($"{DateTime.UtcNow}: {ModuleTwin.Position.Latitude}");
		_logger.LogInformation($"{DateTime.UtcNow}: {ModuleTwin.Position.Longitude}");
	}

	private void OnTwinUpdated(object? sender, ModuleTwin twin)
	{
		_logger.LogInformation($"{DateTime.UtcNow}: Moduletwin changed: {twin.ToTwinCollection()}");
		_logger.LogInformation($"{DateTime.UtcNow}: Variable in static: {ModuleTwin.TestVariable}");
	}
}
*/
