using AutoIoTEdge.Extensions;
using AutoIoTEdge.Services;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleModule.Models;


namespace SampleModule;

public class Program
{
	public static async Task Main()
	{
		var builder = Host.CreateApplicationBuilder();

		var isDevelopment = builder.Environment.IsDevelopment();

		// Register the IoT Edge service based on the environment
		// The service will automatically start when the host starts
		if (isDevelopment)
		{
			builder.Services.Configure<ModuleTwin>(builder.Configuration.GetSection("ModuleTwin"));
			builder.Services.AddDummyIotEdgeService<ModuleTwin>();
		}
		else
		{
			builder.Services.AddIotEdgeService<ModuleTwin>();
		}

		builder.Services.AddSingleton<App>();

		using var host = builder.Build();

		// Start the host - this triggers all hosted services to start (including IotEdgeService)
		await host.StartAsync();

		// Now the ModuleTwin is configured - run your app
		var app = host.Services.GetRequiredService<App>();
		await app.RunAsync();

		// Keep the host running (optional - comment out if your app logic keeps running)
		await host.WaitForShutdownAsync();
	}
}

