// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Application;

public class UOEngineApplication: IDisposable
{
    public readonly IServiceProvider ServiceProvider;

    private readonly ApplicationLoop _applicationLoop = null!;
    private bool disposedValue;

    public UOEngineApplication(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        _applicationLoop = GetService<ApplicationLoop>();
    }

    internal void Start()
    {
        _applicationLoop.Start();  
        
        OnInitialisationCompleted();
    }

    public T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }
    public virtual void Update(float deltaSeconds) { }

    protected virtual void OnInitialisationCompleted(){}

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~UOEngineApplication()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
