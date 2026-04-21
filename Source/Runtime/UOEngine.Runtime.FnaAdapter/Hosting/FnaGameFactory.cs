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
    private readonly IRenderResourceFactory _renderResourceFactory;

    public FnaGameFactory(
        IServiceProvider services,
        IRenderResourceFactory renderResourceFactory,
        InputManager inputManager)
    {
        _services = services;
        _renderResourceFactory = renderResourceFactory;
    }

    public T Create<T>(IHostedGameHost host) where T : Game, new()
    {
        var context = new FnaGameContext
        {
            Services = _services,
            Host = host,
            RenderResourceFactory = _renderResourceFactory,
        };

        using (FnaGameContextScope.Push(context))
        {
            return new T();
        }
    }
}
