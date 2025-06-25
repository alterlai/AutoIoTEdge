using AutoIoTEdge.Interfaces;
using AutoIoTEdge.Services;
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

		var isDevelopment = builder.Environment.IsDevelopment();

		if (isDevelopment)
		{
			builder.Services.Configure<ModuleTwin>(builder.Configuration.GetSection("ModuleTwin"));
			builder.Services.AddSingleton<IIotEdgeService<ModuleTwin>, DummyIotService<ModuleTwin>>();
		}
		else
		{
			builder.Services.AddHostedService<IotEdgeService<ModuleTwin>>();
			builder.Services.AddSingleton<IIotEdgeService<ModuleTwin>>(sp => sp.GetRequiredService<IotEdgeService<ModuleTwin>>());
		}

		builder.Services.AddSingleton<App>();

		using var host = builder.Build();
		var app = host.Services.GetRequiredService<App>();
		await app.RunAsync();
	}
}

