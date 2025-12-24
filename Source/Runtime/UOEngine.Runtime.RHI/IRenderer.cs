// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.RHI;

public interface IRenderer
{
    public IRenderSwapChain SwapChain { get; }

    public IRenderContext CreateRenderContext();

}
