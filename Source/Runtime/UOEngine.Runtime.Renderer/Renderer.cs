using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.Renderer;

public class RenderSystem
{
    public event Action<IRenderContext>? OnFrameBegin;
    public event Action<IRenderContext>? OnFrameEnd;

    public RenderTarget GBufferDiffuse;
    public RenderTarget UIOverlay;

    private IRenderContext _context = null!;
    private readonly IRenderer _rhiRenderer;
    private readonly IRenderResourceFactory _resourceFactory;

    private RenderPassInfo _mainPass;

    private IRhiIndexBuffer _indexBuffer;

    private uint _frameNumber = 0;

    public RenderSystem(IRenderer rhiRenderer, IRenderResourceFactory resourceFactory)
    {
        _rhiRenderer = rhiRenderer;
        _resourceFactory = resourceFactory;

        _mainPass = new RenderPassInfo
        {
            RenderTarget = GBufferDiffuse,
            Name = "MainPass"
        };
    }

    public void Startup()
    {
        _context = _rhiRenderer.CreateRenderContext();
        _indexBuffer = _resourceFactory.CreateIndexBuffer(6, "MainIndexBuffer");

        _indexBuffer.SetData([0, 1, 2, 0, 2, 3]);

        _indexBuffer.Upload();
    }

    public void FrameBegin()
    {
        _context.BeginRecording();

        GBufferDiffuse = _rhiRenderer.SwapChain.Acquire(_context);

        _mainPass.RenderTarget = GBufferDiffuse;

        _context.IndexBuffer = _indexBuffer;
        _context.BeginRenderPass(_mainPass);

        OnFrameBegin?.Invoke(_context);
    }

    public void FrameEnd()
    {
        OnFrameEnd?.Invoke(_context);

        _context.EndRenderPass();
        _context.EndRecording();

        _frameNumber++;
    }

    public void ResizeSwapchain(uint width,  uint height)
    {

    }
}
