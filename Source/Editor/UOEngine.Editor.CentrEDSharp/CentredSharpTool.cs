// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Controls;
using CentrED;

using UOEngine.Editor.Abstractions;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Platform;
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
    private readonly IWindow _window;
    private readonly IHostedGameHostContext _hostContext;
    private readonly IFnaGameFactory _gameFactory;

    private HostedGameHandle _hostedGameHandle;

    public CentredSharpTool(IRenderResourceFactory resourceFactory, IHostedGameManager hostedGameManager, IHostedGameHostContext hostContext, IWindow window,
        IFnaGameFactory fnaGameFactory)
    {
        _resourceFactory = resourceFactory;
        _hostedGameManager = hostedGameManager;
        _window = window;
        _hostContext = hostContext;
        _gameFactory = fnaGameFactory;
    }

    private CentredSharpView CreateCentredSharpView()
    {
        var view = new CentredSharpView(_resourceFactory);

        var gameHost = new HostedCentrEDGameHost(_window.Handle);

        gameHost.SetBounds(_window.RenderTargetWidth, _window.RenderTargetHeight);

        using var gameInitialisationContext = _hostContext.Push(gameHost);

        string windowTitle = _window.WindowTitle;

        var CEDGame = _gameFactory.Create<CentrEDGame>(gameHost);

        // Restore window title as Centred will overwrite it due to SDL calls. 
        _window.WindowTitle = windowTitle;

        Application.SetFromHosted(CEDGame);

        _hostedGameHandle = _hostedGameManager.RegisterGame(new HostedCentrEDGame(Application.CEDGame, view.Surface));

        _hostedGameManager.SuspendGame(_hostedGameHandle);

        view.Surface.SurfaceVisibilityChanged += (visible) =>
        {
            if(visible)
            {
                _hostedGameManager.ResumeGame(_hostedGameHandle);
            }
            else
            {
                _hostedGameManager.SuspendGame(_hostedGameHandle);
            }
        };

        view.Surface.SurfaceRecreated += (width, height) =>
        {
            gameHost.SetBounds(width, height);
        };

        return view;
    }

}
