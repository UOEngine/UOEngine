// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;

public static class Program
{
    public static void Main(string[] args)
    {
        if(CommandLine.HasOption("-wait_for_debugger"))
        {
            UOEDebug.WaitForDebugger();
        }

        using (var app = new Application())
        {
            app.Start();
        }
    }
}
