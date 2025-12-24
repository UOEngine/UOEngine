// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using static SDL3.SDL;

namespace UOEngine.Runtime.SDL3GPU;


internal readonly struct Sdl3GpuResourceProperty
{
    public readonly uint Id;
    public readonly string Value;

    public Sdl3GpuResourceProperty(uint id, string value)
    {
        Id = id;
        Value = value;
    }
}

internal abstract class Sdl3GpuResource : IDisposable
{
    public IntPtr Handle { get; protected set; }
   
    public string Name
    {
        get => _name;
        set
        {
            _name = value;

            (_setNameFunc ?? throw new Exception("_setNameFunc not set"))(Handle, Handle, _name);
        }
    }

    public readonly Sdl3GpuDevice Device;

    private readonly Action<IntPtr, IntPtr, string>? _setNameFunc;

    private bool _disposed;
    private string _name = "";

    private Dictionary<string, Sdl3GpuResourceProperty> _properties = [];

    protected Sdl3GpuResource(Sdl3GpuDevice device, Action<IntPtr, IntPtr, string>? setNameFunc = null, string? debugName = null)
    {
        Device = device;

        _setNameFunc = setNameFunc;

        if (debugName != null)
        {
            Name = debugName;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected abstract void FreeResource();

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        foreach (var property in _properties)
        {
            string name = property.Key;
            Sdl3GpuResourceProperty prop = property.Value;

            SDL_ClearProperty(prop.Id, name);
        }

        if (Handle != IntPtr.Zero)
        {
            FreeResource();

            Handle = IntPtr.Zero;
        }

        _disposed = true;
    }

    protected uint CreateProperty(string name, string value)
    {
        uint prop = SDL_CreateProperties();

        SDL_SetStringProperty(prop, name, value);

        _properties.Add(name, new Sdl3GpuResourceProperty(prop, value));

        return prop;
    }

    ~Sdl3GpuResource()
    {
        Dispose(false);
    }
}
