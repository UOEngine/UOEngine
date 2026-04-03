// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.ComponentModel;

using Avalonia.Controls;

using UOEngine.Runtime.Plugin;

namespace UOEngine.Runtime.AvaloniaUI;

[Service(UOEServiceLifetime.Singleton, typeof(IRootContentHost))]
internal class RootContentHost : IRootContentHost, INotifyPropertyChanged
{
    public Control? MainContent
    {
        get => _mainContent;
        private set
        {
            if (ReferenceEquals(_mainContent, value))
            {
                return;
            }

            _mainContent = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainContent)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private Control? _mainContent;

    public void SetMainContent(Control control) => MainContent = control;
}
