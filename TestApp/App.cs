using AutoIoTEdge;
using AutoIoTEdge.Interfaces;
using AutoIoTEdge.Services;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using SampleModule.Models;

namespace SampleModule;
public class App(IIotEdgeService<ModuleTwin> edgeService, ILogger<App> _logger)
{
	public async Task RunAsync()
	{
		// egister the desired property update callback. This will be called when the module twin is updated from the cloud.
		edgeService.ModuleTwinUpdated += OnTwinUpdated;

		_logger.LogInformation($"{ModuleTwin.TestVariable}");

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
