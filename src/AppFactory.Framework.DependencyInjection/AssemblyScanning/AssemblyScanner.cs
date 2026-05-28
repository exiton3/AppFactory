using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.DependencyInjection.AssemblyScanning;

/// <summary>
/// Builder for configuring assembly scanning
/// </summary>
internal class AssemblyScanner : IAssemblyScanner, ITypeSelector, ILifetimeSelector
{
    private readonly IServiceCollection _services;
    private readonly List<Assembly> _assemblies;
    private readonly List<TypeRegistration> _registrations = new();
    private List<Type> _currentTypes = new();
    private Func<Type, IEnumerable<Type>>? _currentServiceTypeSelector;
    private RegistrationStrategy _currentStrategy = RegistrationStrategy.Append;

    public AssemblyScanner(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        _services = services;
        _assemblies = assemblies.ToList();
    }

    public ITypeSelector AddClasses(Action<IClassFilter> filter, bool publicOnly = true)
    {
        var classFilter = new ClassFilter(_assemblies, publicOnly);
        filter(classFilter);
        _currentTypes = classFilter.GetFilteredTypes();
        return this;
    }

    public ILifetimeSelector AsImplementedInterfaces()
    {
        _currentServiceTypeSelector = implementationType =>
        {
            var interfaces = implementationType.GetInterfaces()
                .Where(i => i != typeof(IDisposable));
            return interfaces;
        };
        return this;
    }

    public ILifetimeSelector AsSelf()
    {
        _currentServiceTypeSelector = implementationType => new[] { implementationType };
        return this;
    }

    public ILifetimeSelector As(Type serviceType)
    {
        _currentServiceTypeSelector = _ => new[] { serviceType };
        return this;
    }

    public ILifetimeSelector As<TService>()
    {
        return As(typeof(TService));
    }

    public ILifetimeSelector UsingRegistrationStrategy(RegistrationStrategy registrationStrategy)
    {
        _currentStrategy = registrationStrategy;
        return this;
    }

    public IAssemblyScanner WithSingletonLifetime()
    {
        return WithLifetime(ServiceLifetime.Singleton);
    }

    public IAssemblyScanner WithScopedLifetime()
    {
        return WithLifetime(ServiceLifetime.Scoped);
    }

    public IAssemblyScanner WithTransientLifetime()
    {
        return WithLifetime(ServiceLifetime.Transient);
    }

    public IAssemblyScanner WithLifetime(ServiceLifetime lifetime)
    {
        if (_currentServiceTypeSelector == null)
        {
            throw new InvalidOperationException("No service type selector has been configured. Call AsImplementedInterfaces(), AsSelf(), or As<T>() first.");
        }

        foreach (var implementationType in _currentTypes)
        {
            var serviceTypes = _currentServiceTypeSelector(implementationType);
            foreach (var serviceType in serviceTypes)
            {
                _registrations.Add(new TypeRegistration
                {
                    ServiceType = serviceType,
                    ImplementationType = implementationType,
                    Lifetime = lifetime,
                    Strategy = _currentStrategy
                });
            }
        }

        _currentTypes = new List<Type>();
        _currentServiceTypeSelector = null;
        _currentStrategy = RegistrationStrategy.Append;

        return this;
    }

    public void Populate()
    {
        foreach (var registration in _registrations)
        {
            switch (registration.Strategy)
            {
                case RegistrationStrategy.Skip:
                    if (_services.Any(d => d.ServiceType == registration.ServiceType))
                        continue;
                    break;

                case RegistrationStrategy.Replace:
                    var existing = _services.FirstOrDefault(d => d.ServiceType == registration.ServiceType);
                    if (existing != null)
                        _services.Remove(existing);
                    break;
            }

            var descriptor = new ServiceDescriptor(
                registration.ServiceType,
                registration.ImplementationType,
                registration.Lifetime);

            _services.Add(descriptor);
        }
    }

    ITypeSelector IAssemblyScanner.AddClasses(Action<IClassFilter> filter, bool publicOnly)
    {
        return AddClasses(filter, publicOnly);
    }

    ITypeSelector ITypeSelector.AddClasses(Action<IClassFilter> filter, bool publicOnly)
    {
        return AddClasses(filter, publicOnly);
    }

    private class TypeRegistration
    {
        public Type ServiceType { get; init; } = null!;
        public Type ImplementationType { get; init; } = null!;
        public ServiceLifetime Lifetime { get; init; }
        public RegistrationStrategy Strategy { get; init; }
    }
}
