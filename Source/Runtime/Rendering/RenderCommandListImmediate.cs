namespace UOEngine.Runtime.Rendering
{
    public class RenderCommandListImmediate: RenderCommandList
    {
        public RenderCommandListImmediate(RenderDevice renderDevice): base(renderDevice)
        {
            IsImmediate = true;
        }
    }
}
