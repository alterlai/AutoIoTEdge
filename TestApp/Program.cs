using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestApp.Models;
using TestApp.Services.IoT;
using TestApp.Wrappers;


namespace TestApp;

public class Program
{
	public static async Task Main()
	{
		var builder = Host.CreateApplicationBuilder();

		builder.Services.AddSingleton<IModuleClient, ModuleClientWrapper>(c =>
		{
			MqttTransportSettings mqttSetting = new(TransportType.Mqtt_Tcp_Only);
			ITransportSettings[] settings = { mqttSetting };
			var moduleClient = ModuleClient.CreateFromEnvironmentAsync(settings).Result;
			Console.WriteLine("Program.cs: " + moduleClient.GetTwinAsync().Result.ToJson());
			// Open a connection to the Edge runtime
			return new ModuleClientWrapper(moduleClient);
		});

		if (builder.Environment.IsDevelopment())
		{
			Console.WriteLine("Development environment");
			builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();

			// Configure the Config class from appsettings.json
			builder.Services.Configure<ModuleTwin>(
				builder.Configuration.GetSection("ModuleTwin"));

			builder.Services.AddSingleton<IIotEdgeService, DummyIotService>();
		}
		else
		{
			Console.WriteLine("Production environment");
			// Initialize iot edge connection to setup edge module client.
			builder.Services.AddSingleton<IIotEdgeService, IotEdgeService>();
		}

		builder.Services.AddSingleton<App>();

		using var host = builder.Build();
		var app = host.Services.GetRequiredService<App>();
		await app.RunAsync();
	}
}

