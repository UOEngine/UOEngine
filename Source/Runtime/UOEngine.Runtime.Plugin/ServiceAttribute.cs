// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace UOEngine.Runtime.Plugin;

public enum UOEServiceLifetime
{
    Singleton
}

public sealed class ServiceAttribute: Attribute
{
    public ServiceLifetime Lifetime { get; }
    public Type? ServiceType { get; }

    public ServiceAttribute(UOEServiceLifetime lifetime, Type? serviceType = null)
    {
        Lifetime = lifetime switch
        {
            UOEServiceLifetime.Singleton => ServiceLifetime.Singleton,
            _ => throw new SwitchExpressionException("Unhandled UOEServiceLifetime")
        };

        ServiceType = serviceType;
    }
}
