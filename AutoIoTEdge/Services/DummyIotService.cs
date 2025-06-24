using AutoIoTEdge;
using AutoIoTEdge.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventHubReceiver.Services.IoT
{
	/// <summary>
	/// Dummy implementation of the IIotEdgeService interface voor local debugging.
	/// </summary>
	public class DummyIotService<TTwin> : IIotEdgeService<TTwin> 
		where TTwin :ModuleTwinBase 
	{
		private TTwin _twin = null!;
		private ILogger _logger = null!;

		public DummyIotService(IOptions<TTwin> twin, ILogger<DummyIotService<TTwin>> logger)
		{
			_twin = twin.Value;
			_logger = logger;
		}

		public TTwin GetTwin()
		{
			return _twin;
		}

		public Task SendEventAsync(string outputName, string message)
		{
			_logger.LogInformation($"DummyIotService: Sending message to output {outputName}. Content: {message}");
			return Task.CompletedTask;
		}
	}
}
