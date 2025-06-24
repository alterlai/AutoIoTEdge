using AutoIoTEdge;
using AutoIoTEdge.Interfaces;
using ExampleApp.Models;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;

namespace ExampleApp;
public class App(IotEdgeService<ModuleTwin> edgeService, ILogger<App> _logger)
{
	public async Task RunAsync()
	{
		var twin = edgeService.Twin;
		edgeService.ModuleTwinUpdated += OnTwinUpdated;


		Console.WriteLine("EventHubReceiver is running...");

	}
	private void OnTwinUpdated(object sender, ModuleTwin twin)
	{
		// Handle twin update

	}
}
