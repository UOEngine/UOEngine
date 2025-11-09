using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.RHI;

public interface IRenderer
{
    public IRenderSwapChain SwapChain { get; }

    public IRenderContext CreateRenderContext();

}
