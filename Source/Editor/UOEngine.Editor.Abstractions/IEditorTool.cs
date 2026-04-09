// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Controls;

namespace UOEngine.Editor.Abstractions;

public interface IEditorTool
{
    public string Name { get; }

    public string Icon { get; }

    public Func<UserControl> CreateContent { get; }
}
