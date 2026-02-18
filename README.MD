# Auto IoT Edge
This repository contains code for the IoT Edge boilerplate library. The goal of this library is to provide a simple and easy-to-use interface for IoT Edge devices, allowing developers to quickly build and deploy IoT Edge applications.

## Features
- Easy to install and use.
- Automatically update module twin properties.
- Construct module twin from a JSON if in development.
- Provides the same functionality as the IoT Edge device library.
- Provides an event subscription for module twin updates.

### Environments
Environment is determined based on the `DOTNET_ENVIRONMENT` variable. If the variable is set to `Development`, 
the library will use a dummy service that reads the module twin from a JSON file. Otherwise, 
it will use the IoT Edge service to read the module twin from the IoT Edge device.

# Installation
1. Install this library using NuGet with
```bash
dotnet add package AutoIoTEdge
```

2. Add the following code to your `Program.cs` file to register the IoT edge library as a service:
```csharp
using AutoIoTEdge.Extensions;
using Microsoft.Extensions.Hosting;
using SampleModule.Models;

var builder = Host.CreateApplicationBuilder();

// Register the IoT Edge service - specify the ModuleTwin type once here
if (builder.Environment.IsDevelopment())
{
	builder.Services.Configure<ModuleTwin>(builder.Configuration.GetSection("ModuleTwin"));
	builder.Services.AddDummyIotEdgeService<ModuleTwin>();
}
else
{
	builder.Services.AddIotEdgeService<ModuleTwin>();
}

// Register your app
builder.Services.AddSingleton<App>();

using var host = builder.Build();

// Start the host to initialize the IoT Edge service
await host.StartAsync();

// Run your application logic
var app = host.Services.GetRequiredService<App>();
await app.RunAsync();

// Keep the host running
await host.WaitForShutdownAsync();
```
> **Note:** You must call `await host.StartAsync()` to initialize the IoT Edge service and load the module twin configuration.

3. Automatically construct your module twin by extending the `ModuleTwinBase` class. The variable names that match module
twin settings will be set accordingly. 
> The property must be public and have a setter. The setter will be used to set the value, so manipulating the value in the setter is supported.

```csharp
public class ModuleTwin : ModuleTwinBase
{
	public static string TestVariable { get; set; } = "DefaultString";
	public static Position Position { get; set; } = new();
}

public class Position
{
	public double Latitude { get; set; }
	public double Longitude { get; set; }
}
```



Your Module Twin in the Azure portal should look like:
```json
{
	"properties": {
		"desired": {
			"TestVariable": "NewValue",
			"Position": {
				"Latitude": 52.3676,
				"Longitude": 4.9041
			}
		}
	}
}
```

Alternatively, when locally debugging, you can use a JSON file called `appsettings.json` or 
`appsettings.Development.json` to set the module twin properties. The `appsettings.Development.json` 
will overwrite values in `appsettings.json`. The JSON file should look like this:
```json
{
	"ModuleTwin": {
		"TestVariable": "NewValue",
		"Position": {
			"Latitude": 12.34,
			"Longitude": 56.78
		}
	}
}
```

# Usage
The IoT Edge service starts automatically when your application starts, so your ModuleTwin is already configured and ready to use.

**Option 1: Access ModuleTwin properties directly (no injection needed)**
```csharp
public class App
{
	private readonly ILogger<App> _logger;

	public App(ILogger<App> logger)
	{
		_logger = logger;
	}

	public async Task RunAsync()
	{
		// ModuleTwin is already configured - just use it!
		_logger.LogInformation($"TestVariable: {ModuleTwin.TestVariable}");
		_logger.LogInformation($"Position: ({ModuleTwin.Position.Latitude}, {ModuleTwin.Position.Longitude})");
	}
}
```

**Option 2: Inject IIotEdgeService when you need to call service methods**
```csharp
using AutoIoTEdge.Services;
using Microsoft.Azure.Devices.Client;

public class MyService
{
	private readonly IIotEdgeService _iotEdgeService;

	// No need to specify <ModuleTwin> - just inject IIotEdgeService
	public MyService(IIotEdgeService iotEdgeService)
	{
		_iotEdgeService = iotEdgeService;
	}

	public async Task SendDataAsync()
	{
		var message = new Message(Encoding.UTF8.GetBytes("Hello IoT Hub!"));
		await _iotEdgeService.SendEventAsync("output1", message);
	}
}
```

Respond to module twin updates by subscribing to the `ModuleTwinUpdated` event in your code:
```csharp
public class MyService
{
	private readonly IIotEdgeService _iotEdgeService;

	public MyService(IIotEdgeService iotEdgeService)
	{
		_iotEdgeService = iotEdgeService;

		// Subscribe to twin updates (requires casting to generic interface)
		if (iotEdgeService is IIotEdgeService<ModuleTwin> typedService)
		{
			typedService.ModuleTwinUpdated += OnModuleTwinUpdated;
		}
	}

	private void OnModuleTwinUpdated(object? sender, ModuleTwin twin)
	{
		Console.WriteLine($"Module twin updated: {twin.TestVariable}");
	}
}
```

Alternatively, inject `IIotEdgeService<ModuleTwin>` for direct access to the typed event:
```csharp
public class MyService
{
	private readonly IIotEdgeService<ModuleTwin> _iotEdgeService;

	public MyService(IIotEdgeService<ModuleTwin> iotEdgeService)
	{
		_iotEdgeService = iotEdgeService;
		// Direct access to ModuleTwinUpdated event
		_iotEdgeService.ModuleTwinUpdated += OnModuleTwinUpdated;
	}

	private void OnModuleTwinUpdated(object? sender, ModuleTwin twin)
	{
		Console.WriteLine($"Module twin updated: {twin.TestVariable}");
	}
}
```

In case you still want to use the base ModuleClient, you can call the `GetBaseModuleClient()` method on the IIotEdgeService:
```csharp
public class MyService
{
	private readonly IIotEdgeService _iotEdgeService;

	public MyService(IIotEdgeService iotEdgeService)
	{
		_iotEdgeService = iotEdgeService;
	}

	public async Task DoSomethingWithBaseClientAsync()
	{
		var client = _iotEdgeService.GetBaseModuleClient();
		// Use the base ModuleClient as needed
		await client.SendEventAsync("customOutput", new Message(...));
	}
}
```