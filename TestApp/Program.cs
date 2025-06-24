using AutoIoTEdge;
using AutoIoTEdge.Interfaces;
using AutoIoTEdge.Wrappers;
using EventHubReceiver.Services.IoT;
using ExampleApp.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace ExampleApp;

public class Program
{
	public static async Task Main()
	{
		var builder = Host.CreateApplicationBuilder();
		
		// Register ModuleClient and your twin type
		builder.Services.AddSingleton<IModuleClient, ModuleClientWrapper>(sp =>
		{
			// Create your ModuleClient here, e.g.:
			MqttTransportSettings mqttSetting = new(TransportType.Mqtt_Tcp_Only);
			ITransportSettings[] settings = { mqttSetting };
			var client = ModuleClient.CreateFromEnvironmentAsync(settings).Result;
			return new ModuleClientWrapper(client);
		});

		var isDevelopment = builder.Environment.IsDevelopment();

		if (isDevelopment)
		{
			// Register dummy service for local development
			builder.Services.Configure<ModuleTwin>(builder.Configuration.GetSection("ModuleTwin"));
			builder.Services.AddSingleton<IIotEdgeService<ModuleTwin>, DummyIotService<ModuleTwin>>();
		}
		else
		{
			// Register real service for production/edge
			builder.Services.AddHostedService<IotEdgeService<ModuleTwin>>();
			builder.Services.AddSingleton<IIotEdgeService<ModuleTwin>>(sp => sp.GetRequiredService<IotEdgeService<ModuleTwin>>());
		}

		builder.Services.AddSingleton<App>();

		using var host = builder.Build();
		var app = host.Services.GetRequiredService<App>();
		await app.RunAsync();
	}
}

