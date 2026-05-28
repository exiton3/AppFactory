namespace AppFactory.Framework.DependencyInjection.AssemblyScanning;

/// <summary>
/// Defines different strategies for registering types
/// </summary>
public enum RegistrationStrategy
{
    /// <summary>
    /// Append new registrations without removing existing ones
    /// </summary>
    Append,

    /// <summary>
    /// Skip registration if the service type is already registered
    /// </summary>
    Skip,

    /// <summary>
    /// Replace existing registrations with new ones
    /// </summary>
    Replace
}
