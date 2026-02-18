using AutoIoTEdge.Models;
using AutoIoTEdge.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutoIoTEdge.Extensions;

/// <summary>
/// Extension methods for configuring IoT Edge services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the IoT Edge service with the specified module twin type.
	/// This allows you to inject IIotEdgeService without specifying the generic type parameter.
	/// </summary>
	/// <typeparam name="TTwin">The type of module twin that derives from ModuleTwinBase.</typeparam>
	/// <param name="services">The service collection to add the service to.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddIotEdgeService<TTwin>(this IServiceCollection services)
		where TTwin : ModuleTwinBase, new()
	{
		// Register the generic version
		services.AddSingleton<IotEdgeService<TTwin>>();
		
		// Register the non-generic version pointing to the same instance
		services.AddSingleton<IIotEdgeService>(sp => sp.GetRequiredService<IotEdgeService<TTwin>>());
		
		// Also register the generic interface for consumers that want type-safe events
		services.AddSingleton<IIotEdgeService<TTwin>>(sp => sp.GetRequiredService<IotEdgeService<TTwin>>());
		
		return services;
	}

	/// <summary>
	/// Registers the Dummy IoT Edge service for development/testing with the specified module twin type.
	/// This allows you to inject IIotEdgeService without specifying the generic type parameter.
	/// </summary>
	/// <typeparam name="TTwin">The type of module twin that derives from ModuleTwinBase.</typeparam>
	/// <param name="services">The service collection to add the service to.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddDummyIotEdgeService<TTwin>(this IServiceCollection services)
		where TTwin : ModuleTwinBase, new()
	{
		// Register the generic version
		services.AddSingleton<DummyIotService<TTwin>>();
		
		// Register the non-generic version pointing to the same instance
		services.AddSingleton<IIotEdgeService>(sp => sp.GetRequiredService<DummyIotService<TTwin>>());
		
		// Also register the generic interface for consumers that want type-safe events
		services.AddSingleton<IIotEdgeService<TTwin>>(sp => sp.GetRequiredService<DummyIotService<TTwin>>());
		
		return services;
	}
}
