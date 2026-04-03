// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Controls;

namespace UOEngine.Runtime.AvaloniaUI;

public interface IRootContentHost
{
    Control? MainContent { get; }

    void SetMainContent(Control control);
}
