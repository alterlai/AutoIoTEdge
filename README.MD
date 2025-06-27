# Auto IoT Edge
This repository contains code for the IoT Edge boilerplate library. The goal of this library is to provide a simple and easy-to-use interface for IoT Edge devices, allowing developers to quickly build and deploy IoT Edge applications.

## Features
- Easy to install and use.
- Automatically update module twin properties.
- Construct module twin from a JSON if in development.
- Provides the same functionality as the IoT Edge device library.
- Provides an event subscription for module twin updates.

# Installation
1. Install this library using NuGet with
```bash
dotnet add package AutoIoTEdge
```

2. Add the following code to your `Program.cs` file to register the Iot edge library as a service:
```csharp
... Other code...
	var builder = Host.CreateApplicationBuilder();

	var isDevelopment = builder.Environment.IsDevelopment();

	if (isDevelopment)
	{
		builder.Services.Configure<ModuleTwin>(builder.Configuration.GetSection("ModuleTwin"));
		builder.Services.AddSingleton<IIotEdgeService<ModuleTwin>, DummyIotService<ModuleTwin>>();
	}
	else
	{
		builder.Services.AddSingleton<IIotEdgeService<ModuleTwin>, IotEdgeService<ModuleTwin>>();
	}
... Other code...
```

3. Automatically construct your module twin by extending the `ModuleTwinBase` class. The variable names that match module
twin settings will be set accordingly. 

```csharp
public class ModuleTwin : ModuleTwinBase
{
	public static string TestVariable { get; set; } = "DefaultString";
}
```

Your Module Twin in the azure portal should look like:
```json
{
	"properties": {
		"desired": {
			"TestVariable": "NewValue"
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
		"TestVariable": "NewValue"
	}
}
```

# Usage
Use the service in your code by injecting it into your classes:
```csharp

public class MyService
{
	private readonly IIotEdgeService<ModuleTwin> _iotEdgeService;
	public MyService(IIotEdgeService<ModuleTwin> iotEdgeService)
	{
		_iotEdgeService = iotEdgeService;
	}
	public async Task DoSomethingAsync()
	{
		var twin = await _iotEdgeService.GetModuleTwinAsync();
		// Do something with the twin
		
		// Or in case of static variables, you can access them directly
		Console.WriteLine(ModuleTwin.TestVariable);
	}
}
```

Respond to module twin updates by subscribing to the `ModuleTwinUpdated` event in your code:
```csharp
public class MyService
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




# How to update
Use the following steps to update the library:
1. Change the version number in the `AutoIoTEdge.csproj` file.
1. Run the following command to pack the library:
```bash
 dotnet pack -c Release 
```
3. Upload the package to NuGet using the following command:
```bash
 dotnet nuget push bin/Release/AutoIoTEdge.<package-version>.nupkg --api-key YOUR_NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```