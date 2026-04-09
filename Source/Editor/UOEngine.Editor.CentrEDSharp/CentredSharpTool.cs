// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Controls;

using UOEngine.Editor.Abstractions;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Plugin;

namespace UOEngine.Editor.CentredSharp;

[Service(UOEServiceLifetime.Singleton, typeof(IEditorTool))]
internal class CentredSharpTool : IEditorTool
{
    public string Name => "CentredSharp";

    public string Icon => Path.Combine(UOEPaths.ContentDir, "Editor/CentredSharp/CentredSharpIcon.png");

    public Func<UserControl> CreateContent => () => new CentredSharpView();
}
