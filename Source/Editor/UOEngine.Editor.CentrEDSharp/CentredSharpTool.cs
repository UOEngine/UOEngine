// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Controls;

using UOEngine.Editor.Abstractions;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Editor.CentredSharp;

[Service(UOEServiceLifetime.Singleton, typeof(IEditorTool))]
internal class CentredSharpTool : IEditorTool
{
    public string Name => "CentredSharp";

    public string Icon => Path.Combine(UOEPaths.ContentDir, "Editor/CentredSharp/CentredSharpIcon.png");

    public Func<UserControl> CreateContent => CreateCentredSharpView;

    private readonly IRenderResourceFactory _resourceFactory;
    private readonly IHostedGameManager _hostedGameManager;
    private HostedGameHandle _hostedGameHandle;

    public CentredSharpTool(IRenderResourceFactory resourceFactory, IHostedGameManager fnaPlugin)
    {
        _resourceFactory = resourceFactory;
        _hostedGameManager = fnaPlugin;
    }

    private CentredSharpView CreateCentredSharpView()
    {
        var view = new CentredSharpView(_resourceFactory);

        _hostedGameHandle = _hostedGameManager.RegisterGame(new HostedCentrEDGame(CentrEdSharpPlugin.CEDGame, view.Surface));

        _hostedGameManager.SuspendGame(_hostedGameHandle);

        view.Surface.SurfaceRecreated += () =>
        {
            _hostedGameManager.ResumeGame(_hostedGameHandle);
        };

        return view;
    }

}
