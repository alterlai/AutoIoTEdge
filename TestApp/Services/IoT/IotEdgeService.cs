using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using TestApp.Models;

namespace TestApp.Services.IoT
{
	public class IotEdgeService : IIotEdgeService
	{
		private ILogger<IotEdgeService> _logger;
		private IModuleClient _moduleClient;
		private readonly Lazy<Task> _setupTask;
		private ModuleTwin _twin = null!;

		public bool IsConnected { get; private set; }

		public IotEdgeService(ILogger<IotEdgeService> logger, IModuleClient moduleClient)
		{
			_logger = logger;
			_moduleClient = moduleClient;
			_setupTask = new Lazy<Task>(() => Setup());
		}



		// <inheritdoc/>
		public ModuleTwin GetTwin()
		{
			_setupTask.Value.Wait();

			return _twin;
		}

		private async Task Setup()
		{
			try
			{
				// Setup listener for commands.
				/*				await _moduleClient.SetMethodHandlerAsync("SetNodeValue", SetNodeValue, _moduleClient);*/

				_logger.LogInformation("Setting desired properties callback...");

				// Setup callback for desired properties
				await _moduleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, _moduleClient);

				// Reconnect is not implented because we'll let docker restart the process when the connection is lost
				_moduleClient.SetConnectionStatusChangesHandler((status, reason) =>
				{
					_logger.LogWarning($"{DateTime.UtcNow}: Connection changed: Status: {status} Reason: {reason}", status, reason);
					IsConnected = status == ConnectionStatus.Connected;
				});

				await _moduleClient.OpenAsync();

				IsConnected = true;

				_logger.LogInformation("IoT Hub module client initialized.");

				await UpdateTwin();

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
			}
		}

		/// <summary>
		/// Callback voor het afhandelen van een property update van uit de IotHub. De twin wordt geupdate met de nieuwe properties.
		/// </summary>
		/// <param name="desiredProperties">desired properties ontvangen vanuit de iot hub.</param>
		/// <param name="userContext"></param>
		/// <returns></returns>
		private async Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
		{
			try
			{
				_logger.LogInformation($"{DateTime.UtcNow}: Getting desired properties...");
				var twin = await _moduleClient!.GetTwinAsync();

				_logger.LogInformation($"{DateTime.UtcNow}: Twin values: {JsonConvert.SerializeObject(twin)}");

				desiredProperties = twin.Properties.Desired;

				_twin = new();
				var reportedProperties = _twin.SetSettings(desiredProperties);

				// Log the new settings
				string desiredPropertiesJson = JsonConvert.SerializeObject(desiredProperties);
				_logger.LogInformation($"{DateTime.UtcNow}: Processing incoming twin collection: {desiredPropertiesJson}");

				// Report back
				_logger.LogInformation($"{DateTime.UtcNow}: New settings are: {JsonConvert.SerializeObject(_twin)}");
				await _moduleClient!.UpdateReportedPropertiesAsync(new TwinCollection(JsonConvert.SerializeObject(reportedProperties)));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{DateTime.UtcNow}: Error updating desired properties.");
			}
		}

		/// <inheritdoc />
		public async Task SendMessageAsync(string message, string deviceName)
		{
			try
			{
				if (_moduleClient == null)
				{
					_logger.LogError($"{DateTime.UtcNow}: IoT Hub device client NOT available when sending message");
					return;
				}
				// Determine output route.
				string outputRoute = Settings.OutputName;

				// Construct message to send.
				string jsonMessage = JsonConvert.SerializeObject(message);
				var messageobj = new Message(Encoding.UTF8.GetBytes(jsonMessage));

				// Add the devicename as a property to the message.
				messageobj.Properties.Add("DeviceName", deviceName);

				// Send message to output.
				await _moduleClient.SendEventAsync(outputRoute, messageobj);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{DateTime.UtcNow}: Error sending message {message}");
			}
		}

		private async Task UpdateTwin()
		{
			if (_moduleClient == null)
			{
				_logger.LogError($"{DateTime.UtcNow}: IoT Hub module client NOT available when reading moduletwin");
				return;
			}

			await OnDesiredPropertiesUpdate(new TwinCollection(), _moduleClient);
		}
	}
}
