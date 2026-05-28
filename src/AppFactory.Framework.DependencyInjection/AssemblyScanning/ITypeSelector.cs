using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.DependencyInjection.AssemblyScanning;

/// <summary>
/// Defines the contract for selecting types during assembly scanning
/// </summary>
public interface ITypeSelector
{
    /// <summary>
    /// Registers the selected types as their implemented interfaces
    /// </summary>
    /// <returns>The lifetime selector for further configuration</returns>
    ILifetimeSelector AsImplementedInterfaces();

    /// <summary>
    /// Registers the selected types as themselves
    /// </summary>
    /// <returns>The lifetime selector for further configuration</returns>
    ILifetimeSelector AsSelf();

    /// <summary>
    /// Registers the selected types as the specified service type
    /// </summary>
    /// <param name="serviceType">The service type to register as</param>
    /// <returns>The lifetime selector for further configuration</returns>
    ILifetimeSelector As(Type serviceType);

    /// <summary>
    /// Registers the selected types as the specified service type
    /// </summary>
    /// <typeparam name="TService">The service type to register as</typeparam>
    /// <returns>The lifetime selector for further configuration</returns>
    ILifetimeSelector As<TService>();

    /// <summary>
    /// Uses a custom registration strategy
    /// </summary>
    /// <param name="registrationStrategy">The registration strategy to use</param>
    /// <returns>The lifetime selector for further configuration</returns>
    ILifetimeSelector UsingRegistrationStrategy(RegistrationStrategy registrationStrategy);

    /// <summary>
    /// Adds additional classes matching the specified criteria
    /// </summary>
    /// <param name="filter">Filter to apply to classes</param>
    /// <param name="publicOnly">Whether to include only public types</param>
    /// <returns>The type selector for further configuration</returns>
    ITypeSelector AddClasses(Action<IClassFilter> filter, bool publicOnly = true);
}
