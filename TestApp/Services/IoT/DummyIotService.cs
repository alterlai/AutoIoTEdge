using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestApp.Models;

namespace TestApp.Services.IoT
{
	/// <summary>
	/// Dummy implementation of the IIotEdgeService interface voor local debugging.
	/// </summary>
	public class DummyIotService : IIotEdgeService
	{
		private ModuleTwin _twin = null!;
		private ILogger _logger = null!;

		public DummyIotService(IOptions<ModuleTwin> twin, ILogger<DummyIotService> logger)
		{
			_twin = twin.Value;
			_logger = logger;
		}

		public ModuleTwin GetTwin()
		{
			return _twin;
		}

		public Task SendMessageAsync(string message, string deviceName)
		{
			_logger.LogInformation($"DummyIotService: Sending message to output {Settings.OutputName} with property DeviceName: {deviceName}.");
			return Task.CompletedTask;
		}
	}
}
