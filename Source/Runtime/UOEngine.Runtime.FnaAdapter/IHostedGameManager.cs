// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.

namespace UOEngine.Runtime.FnaAdapter;

public struct HostedGameHandle
{
    public readonly int Id { get; init; }
}

public interface IHostedGameManager
{
    HostedGameHandle RegisterGame(IHostedGame hostedGame);

    void SuspendGame(in HostedGameHandle id);
    void ResumeGame(in HostedGameHandle id);
}
