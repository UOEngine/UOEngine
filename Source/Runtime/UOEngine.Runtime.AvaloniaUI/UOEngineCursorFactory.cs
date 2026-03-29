// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineCursorFactory : ICursorFactory
{
    public ICursorImpl CreateCursor(Bitmap cursor, PixelPoint hotSpot) => new CursorStub(StandardCursorType.Arrow);

    public ICursorImpl GetCursor(StandardCursorType cursorType) => new CursorStub(cursorType);

    private sealed class CursorStub : ICursorImpl
    {
        public StandardCursorType Type { get; }

        public CursorStub(StandardCursorType type)
        {
            Type = type;
        }

        public void Dispose()
        {
            UOEDebug.NotImplemented();
        }
    }
}
