// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Application;

namespace UOEngine.Tests.Triangle;

internal class Program
{
    static int Main(string[] args)
    {
        return BuildApp().StartWithDefault(args);
    }

    public static UOEngineAppBuilder BuildApp() => UOEngineAppBuilder.Configure<TriangleTest>();
}
