// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Text;

namespace UOEngine.Runtime.Core;

public static class UOEThread
{
    public static Thread MainThread { get; private set; } = null!;

    public static bool IsMainThread => MainThread == Thread.CurrentThread;

    public static void Init()
    {
        MainThread = Thread.CurrentThread;
    }
}
