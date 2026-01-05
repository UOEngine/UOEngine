// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Application;

public sealed class UOEngineAppBuilder
{
    public Type? ApplicationType { get; private set; }

    private Func<UOEngineApplication>? _appFactory;

    public static UOEngineAppBuilder Configure<TApp>() where TApp: UOEngineApplication, new()
    {
        if (CommandLine.HasOption("-wait_for_debugger"))
        {
            UOEDebug.WaitForDebugger();
        }

        return new UOEngineAppBuilder()
        {
            ApplicationType = typeof(TApp),
            _appFactory = () => new TApp()
        };
    }

    public int StartWithDefault(string[] args)
    {
        using var instance = _appFactory!.Invoke();

        instance.Start();

        return 0;
    }
}
