// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public interface IRenderer
{
    public void FrameBegin();
    public void FrameEnd();

    public IRenderContext CreateRenderContext(string name);

    public void GetInteropContext(out RhiInteropContext interopContext);
}
