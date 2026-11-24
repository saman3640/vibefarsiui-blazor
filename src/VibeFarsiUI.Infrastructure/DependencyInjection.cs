using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VibeFarsiUI.Core.Registry;
using VibeFarsiUI.Infrastructure.Registry;

namespace VibeFarsiUI.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the JSON-backed registry as a singleton.</summary>
    public static IServiceCollection AddVibeFarsiInfrastructure(
        this IServiceCollection services,
        Action<RegistryOptions> configure)
    {
        services.Configure(configure);
        // Minimal API handlers inject the concrete options instance.
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RegistryOptions>>().Value);
        services.AddSingleton<IRegistryProvider, JsonRegistryProvider>();
        return services;
    }
}
