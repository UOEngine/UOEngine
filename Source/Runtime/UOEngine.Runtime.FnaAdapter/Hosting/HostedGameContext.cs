// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Plugin;

namespace UOEngine.Runtime.FnaAdapter;

[Service(UOEServiceLifetime.Singleton, typeof(IHostedGameHostContext))]
internal sealed class HostedGameHostContext : IHostedGameHostContext
{
    public IHostedGameHost Current => _current ?? throw new InvalidOperationException("Current IHostedGameHost is null.");

    private IHostedGameHost? _current;

    public IDisposable Push(IHostedGameHost host)
    {
        if (_current is not null)
        {
            throw new InvalidOperationException("A hosted game host is already active.");
        }

        _current = host;

        return new PopScope(this);
    }

    private sealed class PopScope : IDisposable
    {
        private readonly HostedGameHostContext _owner;
        private bool _disposed;

        public PopScope(HostedGameHostContext owner) => _owner = owner;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _owner._current = null;
            _disposed = true;
        }
    }
}
