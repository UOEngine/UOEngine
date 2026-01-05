// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Application;

public static class Program
{
    public static int Main(string[] args)
    {
        return UOEngineAppBuilder.Configure<UOEngineApplication>().StartWithDefault(args);
    }
}
