// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Collections.ObjectModel;

using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using UOEngine.Editor.Abstractions;
using UOEngine.Runtime.AvaloniaUI;

namespace UOEngine.Editor.UI;

internal class EditorMainViewModel: UOEngineViewModel
{
    internal ObservableCollection<ToolViewModel> OpenedTools { get; } = [];
    internal ToolViewModel? SelectedTool { get; set; }

    internal readonly ToolViewModel[] Tools;

    private readonly EditorLandingViewModel _editorLandingViewModel;

    internal EditorMainViewModel(IEnumerable<IEditorTool> editorTools)
    {
        Tools = new ToolViewModel[editorTools.Count() + 1];


        var tools = new List<ToolViewModel>(editorTools.Count());

        foreach(var tool in editorTools)
        {
            tools.Add(new ToolViewModel
            {
                Name = tool.Name,
                CanClose = true,
                Icon = File.Exists(tool.Icon) ? new Bitmap(tool.Icon) : null
                //Content = tool.CreateContent
            });
        }

        _editorLandingViewModel = new EditorLandingViewModel(tools); 

        Tools[0] = new ToolViewModel
        {
            Name = "",
            Content = new EditorLandingView
            {
                DataContext = _editorLandingViewModel
            },
            CanClose = false
        };

        SelectedTool = Tools[0];
        OpenedTools.Add(Tools[0]);

        int i = 1;

        foreach (var tool in tools)
        {
            Tools[i] = tool;
            i++;
        }
    }
}

internal class ToolViewModel
{
    internal string? Name { get; set; }
    internal UserControl? Content { get; set; }

    internal IImage? Icon { get; set; }

    internal bool CanClose { get; set; }

}