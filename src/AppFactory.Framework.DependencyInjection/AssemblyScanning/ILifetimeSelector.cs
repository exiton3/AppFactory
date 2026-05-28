using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.DependencyInjection.AssemblyScanning;

/// <summary>
/// Defines the contract for selecting service lifetime during assembly scanning
/// </summary>
public interface ILifetimeSelector
{
    /// <summary>
    /// Registers the selected types with a singleton lifetime
    /// </summary>
    /// <returns>The assembly scanner for further configuration</returns>
    IAssemblyScanner WithSingletonLifetime();

    /// <summary>
    /// Registers the selected types with a scoped lifetime
    /// </summary>
    /// <returns>The assembly scanner for further configuration</returns>
    IAssemblyScanner WithScopedLifetime();

    /// <summary>
    /// Registers the selected types with a transient lifetime
    /// </summary>
    /// <returns>The assembly scanner for further configuration</returns>
    IAssemblyScanner WithTransientLifetime();

    /// <summary>
    /// Registers the selected types with the specified lifetime
    /// </summary>
    /// <param name="lifetime">The service lifetime</param>
    /// <returns>The assembly scanner for further configuration</returns>
    IAssemblyScanner WithLifetime(ServiceLifetime lifetime);
}
