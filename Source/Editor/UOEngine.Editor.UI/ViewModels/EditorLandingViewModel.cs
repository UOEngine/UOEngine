// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UOEngine.Editor.Abstractions;
using UOEngine.Runtime.AvaloniaUI;

namespace UOEngine.Editor.UI;

internal class EditorLandingViewModel: UOEngineViewModel
{
    internal ObservableCollection<ToolViewModel> Tools { get; } = [];

    internal EditorLandingViewModel(IEnumerable<ToolViewModel> editorTools)
    {
        foreach(var tool in editorTools)
        {
            Tools.Add(tool);
        }

    }
}
