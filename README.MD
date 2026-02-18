# Auto IoT Edge
This repository contains code for the IoT Edge boilerplate library. The goal of this library is to provide a simple and easy-to-use interface for IoT Edge devices, allowing developers to quickly build and deploy IoT Edge applications.

## Features
- Easy to install and use.
- Automatically update module twin properties.
- Construct module twin from a JSON if in development.
- Provides the same functionality as the IoT Edge device library.
- Provides an event subscription for module twin updates.
- **Automatic service initialization** - module twin is configured even if you don't inject the service anywhere.
- **Simplified dependency injection** - specify the module twin type only once during registration.

### Environments
Environment is determined based on the `DOTNET_ENVIRONMENT` variable. If the variable is set to `Development`, 
the library will use a dummy service that reads the module twin from a JSON file. Otherwise, 
it will use the IoT Edge service to read the module twin from the IoT Edge device.

# Installation
1. Install this library using NuGet with
```bash
dotnet add package AutoIoTEdge
```

2. Add the following code to your `Program.cs` file to register the IoT Edge library as a service:
```csharp
using AutoIoTEdge.Extensions;
using SampleModule.Models;

var builder = Host.CreateApplicationBuilder();

// Register the IoT Edge service - it will start automatically!
// You only need to specify the ModuleTwin type once here
if (builder.Environment.IsDevelopment())
{
	builder.Services.Configure<ModuleTwin>(builder.Configuration.GetSection("ModuleTwin"));
	builder.Services.AddDummyIotEdgeService<ModuleTwin>();
}
else
{
	builder.Services.AddIotEdgeService<ModuleTwin>();
}

// The service is now registered and will start automatically when the host starts
// Your ModuleTwin will be configured even if you never inject the service!
```

3. Automatically construct your module twin by extending the `ModuleTwinBase` class. The variable names that match module
twin settings will be set accordingly. 
> [!NOTE]
> The property must be public and have a setter. The setter will be used to set the value, so manupulating the value in the setter is supported.

```csharp
public class ModuleTwin : ModuleTwinBase
{
	public static string TestVariable { get; set; } = "DefaultString";
	public static Position Position { get; set; } = new();
}
```

Your Module Twin in the Azure portal should look like:
```json
{
	"properties": {
		"desired": {
			"TestVariable": "NewValue",
			"Position": {
				"Latitude": 12.34,
				"Longitude": 56.78
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

> [!IMPORTANT]
> In Visual studio, right-click on appsettings.json -> properties -> Copy to output directory = "Copy always". This option will copy your appsettings to the build directory when building the project. If you forget this step, your appsettings will not apply.

# Usage

## Option 1: Access ModuleTwin properties directly (No injection required!)
Since the service starts automatically, your ModuleTwin is already configured. You can access static properties directly:

```csharp
public class MyApp
{
	private readonly ILogger<MyApp> _logger;

	public MyApp(ILogger<MyApp> logger)
	{
		_logger = logger;
	}

	public async Task RunAsync()
	{
		// ModuleTwin is already configured - just use it!
		_logger.LogInformation($"TestVariable: {ModuleTwin.TestVariable}");
		_logger.LogInformation($"Position: ({ModuleTwin.Position.Latitude}, {ModuleTwin.Position.Longitude})");

		// Your application logic here...
	}
}
```

## Option 2: Inject IIotEdgeService (when you need to call service methods)
Use the non-generic interface - no need to specify the type parameter every time you inject it:

```csharp
public class MyService
{
	private readonly IIotEdgeService _iotEdgeService;
	private readonly ILogger<MyService> _logger;

	public MyService(IIotEdgeService iotEdgeService, ILogger<MyService> logger)
	{
		_iotEdgeService = iotEdgeService;
		_logger = logger;

		// Subscribe to twin updates if needed (requires casting to generic interface)
		if (iotEdgeService is IIotEdgeService<ModuleTwin> typedService)
		{
			typedService.ModuleTwinUpdated += OnTwinUpdated;
		}
	}

	public async Task SendDataAsync()
	{
		// Use any IoT Edge service method
		var message = new Message(Encoding.UTF8.GetBytes("Hello IoT Hub!"));
		await _iotEdgeService.SendEventAsync("output1", message);
	}

	private void OnTwinUpdated(object? sender, ModuleTwin twin)
	{
		_logger.LogInformation($"Twin updated: {twin.TestVariable}");
	}
}
```

## Option 3: Inject IIotEdgeService<ModuleTwin> (for strongly-typed events)
If you prefer compile-time type safety for the ModuleTwinUpdated event:

```csharp
public class MyService
{
	private readonly IIotEdgeService<ModuleTwin> _iotEdgeService;

	public MyService(IIotEdgeService<ModuleTwin> iotEdgeService)
	{
		_iotEdgeService = iotEdgeService;
		// Direct access to ModuleTwinUpdated event with type safety
		_iotEdgeService.ModuleTwinUpdated += OnTwinUpdated;
	}

	private void OnTwinUpdated(object? sender, ModuleTwin twin)
	{
		Console.WriteLine($"Twin updated: {twin.TestVariable}");
	}
}
```

# Key Benefits

✅ **Automatic Initialization**: The service starts automatically when your application starts. No manual initialization required!

✅ **Specify Type Once**: You only specify the `<ModuleTwin>` type parameter once during registration in `Program.cs`

✅ **No Injection Required**: If you only need the ModuleTwin configured, you don't need to inject anything. Just access the static properties!

✅ **Flexible Injection**: Inject `IIotEdgeService` (no generics) or `IIotEdgeService<ModuleTwin>` (type-safe events) based on your needs

✅ **Same Instance**: All injection options resolve to the same singleton instance
{
	private readonly IIotEdgeService<ModuleTwin> _iotEdgeService;
	public MyService(IIotEdgeService<ModuleTwin> iotEdgeService)
	{
		_iotEdgeService = iotEdgeService;
		_iotEdgeService.ModuleTwinUpdated += OnModuleTwinUpdated;
	}
	private void OnModuleTwinUpdated(object sender, ModuleTwin twin)
	{
		Console.WriteLine($"Module twin updated: {twin.TestVariable}");
	}
}

```

In case you still want to use the base ModuleClient, you can call the `GetBaseModuleClient()` method on the IIoTEdgeService:
```csharp
public class MyService
{
	private readonly IIotEdgeService<ModuleTwin> _iotEdgeService;

	public MyService(IIotEdgeService<ModuleTwin> iotEdgeService)
	{
		_iotEdgeService = iotEdgeService;
	}
	public async Task DoSomethingWithBaseClientAsync()
	{
		var client = await _iotEdgeService.GetBaseModuleClient();
		// Use the base ModuleClient as needed
	}
}
```
