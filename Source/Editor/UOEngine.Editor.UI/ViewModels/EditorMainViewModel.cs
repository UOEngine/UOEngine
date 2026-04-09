// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Collections.ObjectModel;
using System.Windows.Input;

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
            ToolViewModel toolViewModel = null!;

            toolViewModel = new ToolViewModel
            {
                Name = tool.Name,
                CanClose = true,
                Icon = File.Exists(tool.Icon) ? new Bitmap(tool.Icon) : null,
                OpenCommand = new DelegateCommand(() => OpenTool(toolViewModel)),
                CreateContent = tool.CreateContent
            };

            tools.Add(toolViewModel);
        }

        _editorLandingViewModel = new EditorLandingViewModel(tools); 

        Tools[0] = new ToolViewModel
        {
            Name = "Home",
            CreateContent = () => new EditorLandingView
            {
                DataContext = _editorLandingViewModel
            },
            CanClose = false,
        };

        int i = 1;

        foreach (var tool in tools)
        {
            Tools[i] = tool;
            i++;
        }

        OpenTool(Tools[0]);
    }
    private void OpenTool(ToolViewModel tool)
    {
        var existing = OpenedTools.FirstOrDefault(x => x == tool || x.Name == tool.Name);

        if (existing is not null)
        {
            SelectedTool = existing;
            OnPropertyChanged(nameof(SelectedTool));
            return;
        }

        tool.Content ??= tool.CreateContent?.Invoke();

        OpenedTools.Add(tool);
        SelectedTool = tool;
        OnPropertyChanged(nameof(SelectedTool));
    }
}

internal class ToolViewModel
{
    internal string? Name { get; set; }
    internal Func<UserControl>? CreateContent { get; set; }

    internal UserControl? Content { get; set; }

    internal IImage? Icon { get; set; }

    internal bool CanClose { get; set; }

    internal ICommand? OpenCommand { get; set; }
}