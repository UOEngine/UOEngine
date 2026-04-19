// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.FnaAdapter;

internal static class FnaGameContextScope
{
    public static FnaGameContext Current => _current ?? throw new InvalidOperationException("Current IHostedGameHost is null.");

    private static FnaGameContext? _current;

    public static IDisposable Push(FnaGameContext context)
    {
        if (_current is not null)
        {
            throw new InvalidOperationException("A hosted game host is already active.");
        }

        _current = context;

        return new PopScope();
    }

    private sealed class PopScope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _current = null;
            _disposed = true;
        }
    }
}
