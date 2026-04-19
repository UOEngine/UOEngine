// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Xna.Framework;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.FnaAdapter;

[Service(UOEServiceLifetime.Singleton, typeof(IFnaGameFactory))]
internal sealed class FnaGameFactory : IFnaGameFactory
{
    private readonly IServiceProvider _services;
    private readonly IWindow _window;
    private readonly IRenderResourceFactory _renderResourceFactory;
    private readonly InputManager _inputManager;

    public FnaGameFactory(
        IServiceProvider services,
        IWindow window,
        IRenderResourceFactory renderResourceFactory,
        InputManager inputManager)
    {
        _services = services;
        _window = window;
        _renderResourceFactory = renderResourceFactory;
        _inputManager = inputManager;
    }

    public T Create<T>(IHostedGameHost host) where T : Game, new()
    {
        var context = new FnaGameContext
        {
            Services = _services,
            Host = host,
            PlatformWindow = _window,
            RenderResourceFactory = _renderResourceFactory,
            InputManager = _inputManager
        };

        using (FnaGameContextScope.Push(context))
        {
            return new T();
        }
    }
}
