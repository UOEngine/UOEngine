// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public struct RhiSemaphoreDescription
{
    public string Name;
    public bool Exportable;
}

public abstract class RhiSemaphore: IDisposable
{
    public string? Name { get; set; }

    public nint ExportedHandle { get; protected set; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

