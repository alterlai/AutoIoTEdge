using Microsoft.Extensions.Logging;
using TestApp.Models;
using TestApp.Services.IoT;

namespace TestApp;
public class App(IIotEdgeService iotEdgeService, ILogger<App> _logger)
{
	public async Task RunAsync()
	{
		var twin = iotEdgeService.GetTwin();

		Console.WriteLine("EventHubReceiver is running...");

	}
}
