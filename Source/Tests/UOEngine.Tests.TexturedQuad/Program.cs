// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Application;

namespace UOEngine.Tests.TexturedQuad;

internal class Program
{
    static int Main(string[] args)
    {
        return BuildApp().UseDefaults().Start(args);
    }

    public static UOEngineAppBuilder BuildApp() => UOEngineAppBuilder.Configure<TexturedQuadTest>();
}

