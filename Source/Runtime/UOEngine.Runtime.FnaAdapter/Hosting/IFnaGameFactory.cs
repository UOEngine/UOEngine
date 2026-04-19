// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Xna.Framework;

namespace UOEngine.Runtime.FnaAdapter;

public interface IFnaGameFactory
{
    T Create<T>(IHostedGameHost host) where T : Game, new();
}
