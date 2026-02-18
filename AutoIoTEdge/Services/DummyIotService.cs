using AutoIoTEdge.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace AutoIoTEdge.Services
{
	/// <summary>
	/// Dummy implementation of the IIotEdgeService interface for local debugging.
	/// Implements IHostedService to ensure automatic initialization when the application starts.
	/// </summary>
	public class DummyIotService<TTwin> : IIotEdgeService<TTwin>, IHostedService
		where TTwin : ModuleTwinBase
	{
		private TTwin _twin = null!;
		private ILogger _logger = null!;

		public event EventHandler<TTwin>? ModuleTwinUpdated;

		public DummyIotService(IOptions<TTwin> twin, ILogger<DummyIotService<TTwin>> logger)
		{
			_twin = twin.Value;
			_logger = logger;
		}

		/// <summary>
		/// Starts the dummy IoT Edge service.
		/// This is called automatically by the hosting infrastructure.
		/// </summary>
		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("DummyIotService: Starting service...");
			// Trigger the ModuleTwinUpdated event to simulate twin initialization
			ModuleTwinUpdated?.Invoke(this, _twin);
			_logger.LogInformation("DummyIotService: Service started successfully.");
			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the dummy IoT Edge service.
		/// This is called automatically by the hosting infrastructure during shutdown.
		/// </summary>
		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("DummyIotService: Stopping service...");
			return Task.CompletedTask;
		}

		public ModuleClient GetBaseModuleClient()
		{
			throw new NotImplementedException("DummyIotService does not support ModuleClient operations.");
		}


		public Task SendEventAsync(string outputName, string message)
		{
			_logger.LogInformation($"DummyIotService: Sending message to output {outputName}. Content: {message}");
			return Task.CompletedTask;
		}

		public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext)
		{
			_logger.LogInformation($"DummyIotService: Setting input message handler for input {inputName}");
			return Task.CompletedTask;
		}

		public Task SendEventAsync(string outputName, Message message)
		{
			string content = message.GetBytes() != null ?
				Encoding.UTF8.GetString(message.GetBytes()) :
				"<empty>";

			_logger.LogInformation($"DummyIotService: Sending message to output {outputName}. Content: {content}");
			return Task.CompletedTask;
		}

		public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken)
		{
			string content = message.GetBytes() != null ?
				Encoding.UTF8.GetString(message.GetBytes()) :
				"<empty>";

			_logger.LogInformation($"DummyIotService: Sending message to output {outputName} with cancellation token. Content: {content}");
			return Task.CompletedTask;
		}

		public Task SetMethodHandlerAsync(string methodname, MethodCallback methodCallback, object userContext)
		{
			_logger.LogInformation($"DummyIotService: Setting method handler for method {methodname}");
			return Task.CompletedTask;
		}

		public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest, CancellationToken cancellationToken)
		{
			_logger.LogInformation($"DummyIotService: Invoking method {methodRequest.Name} on device {deviceId} with cancellation token. Payload: {methodRequest.DataAsJson}");
			// Return a dummy success response with empty payload
			return Task.FromResult(new MethodResponse(200));
		}

		public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest)
		{
			_logger.LogInformation($"DummyIotService: Invoking method {methodRequest.Name} on device {deviceId}. Payload: {methodRequest.DataAsJson}");
			// Return a dummy success response with empty payload
			return Task.FromResult(new MethodResponse(200));
		}
	}
}
