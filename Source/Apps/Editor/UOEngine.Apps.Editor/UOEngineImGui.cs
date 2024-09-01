using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ImGuiNET;
using Silk.NET.Maths;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Rendering;
using UOEngine.Runtime.Rendering.Resources;
using Renderer = UOEngine.Runtime.Rendering.Renderer;

namespace UOEngine.Apps.Editor
{
    public class UOEngineImGui
    {
        private Input                   _input;
        private RenderDevice            _renderDevice;
        private Window                  _window;

        private IntPtr                  _fontAtlasID = 1;
        private RenderTexture2D?        _fontTexture;

        private RenderBuffer?           _vertexBuffer;
        private RenderBuffer?           _indexBuffer;

        private RenderUniformBuffer?    _mvpUniform;

        private ImDrawVert[]            _verts = [];
        private ushort[]                _indices = [];

        private ImGuiShader?            _imguiShader;

        private ImDrawDataPtr           _drawData;
        private float                   _width;
        private float                   _height;

        public UOEngineImGui(Input input, Renderer renderer, GameLoop gameLoop, Window window)
        {
            _input = input;
            _renderDevice = renderer.Device;
            _window = window;

            gameLoop.FrameStarted += OnFrameStart;
            //renderer.FrameEnd += OnFrameEnd;
        }

        public void Initialise()
        {
            var context = ImGui.CreateContext();

            ImGui.SetCurrentContext(context);

            ImGui.StyleColorsDark();

            SetupInput();

            _vertexBuffer = _renderDevice.CreateRenderBuffer(0, ERenderBufferType.Vertex);
            _indexBuffer = _renderDevice.CreateRenderBuffer(0, ERenderBufferType.Index);

            _imguiShader = _renderDevice.RegisterShader<ImGuiShader>();

            ImGuiIOPtr io = ImGui.GetIO();

            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out var width, out var height, out var bytesPerPixel);

            _fontTexture = _renderDevice.CreateTexture2D(new()
            {
                Width = (uint)width,
                Height = (uint)height,
                Format = ERenderTextureFormat.R8G8B8A8
            });

            unsafe
            {
                _fontTexture.Upload(new ReadOnlySpan<byte>(pixels.ToPointer(), width * height * bytesPerPixel));
            }

            io.Fonts.SetTexID(_fontAtlasID);

            io.Fonts.ClearTexData();

            _mvpUniform = new RenderUniformBuffer(_renderDevice, (ulong)Marshal.SizeOf(Matrix4X4<float>.Identity));

            _renderDevice.ImmediateContext!.Rendering += Draw;

        }

        private void SetupInput()
        {
        }

        private void OnFrameStart(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.DisplaySize = new Vector2(_window.Width, _window.Height);

            io.DeltaTime = deltaSeconds;

            _width = io.DisplaySize.X;
            _height = io.DisplaySize.Y;

            UpdateInput();

            ImGui.NewFrame();

            ImGui.Begin("Hello world!");
            ImGui.Text("This is some useful text.");
            ImGui.End();

            ImGui.Render();

            _drawData = ImGui.GetDrawData();

        }

        private unsafe void Draw(RenderCommandListContextImmediate immediateContext)
        {
            var drawData = _drawData;

            if ((drawData.DisplaySize.X <= 0.0f) || (drawData.DisplaySize.Y <= 0.0f))
            {
                return;
            }

            if ((drawData.TotalVtxCount) == 0 || (drawData.TotalIdxCount == 0))
            {
                return;
            }

            uint newVertexBufferSize = (uint)(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());

            if (newVertexBufferSize > _vertexBuffer!.Length)
            {
                _verts = new ImDrawVert[newVertexBufferSize];

                _vertexBuffer?.MarkForDelete();

                _vertexBuffer = _renderDevice.CreateRenderBuffer(newVertexBufferSize, ERenderBufferType.Vertex);
            }

            uint newIndexBufferSize = (uint)(drawData.TotalIdxCount * sizeof(ushort));

            if (newIndexBufferSize > _indexBuffer!.Length)
            {
                _indices = new ushort[newIndexBufferSize];

                _indexBuffer?.MarkForDelete();

                _indexBuffer = _renderDevice.CreateRenderBuffer(newIndexBufferSize, ERenderBufferType.Index);
            }

            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];

                for(int i  = 0; i < cmdList.VtxBuffer.Size; i++)
                {
                    _verts[vertexOffsetInVertices++] = new()
                    {
                        pos = cmdList.VtxBuffer[i].pos,
                        col = cmdList.VtxBuffer[i].col,
                        uv = cmdList.VtxBuffer[i].uv
                    };
                }

                for(int i = 0; i < cmdList.IdxBuffer.Size; i++)
                {
                    _indices[indexOffsetInElements++] = cmdList.IdxBuffer[i];
                }
                //_indices[vertexOffsetInVertices++] = cmdList.IdxBuffer[n];

                //vertexOffsetInVertices += (uint)cmdList.VtxBuffer.Size;
                //indexOffsetInElements += (uint)cmdList.IdxBuffer.Size;
            }

            _vertexBuffer.CopyToDevice<ImDrawVert>(_verts);
            _indexBuffer.CopyToDevice<ushort>(_indices);

            immediateContext.CommandBufferManager.SubmitUploadBuffer();

            var mvp = Matrix4X4.CreateOrthographic(_width, _height, -1.0f, 1.0f);

            _mvpUniform!.Update(mvp);

            //_renderDevice.ImmediateContext!.Rendering += (immediateContext) =>
            {
                immediateContext.BindVertexBuffer(_vertexBuffer);
                immediateContext.BindIndexBuffer(_indexBuffer);
                immediateContext.BindUniformBuffer(_mvpUniform, 0);

                int vertexOffset = 0;
                int indexOffset = 0;

                for (int n = 0; n < drawData.CmdListsCount; n++)
                {
                    ImDrawListPtr cmdList = drawData.CmdLists[n];

                    for (int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
                    {
                        ImDrawCmdPtr drawCmd = cmdList.CmdBuffer[cmdi];

                        if (drawCmd.ElemCount == 0)
                        {
                            continue;
                        }

                        if (drawCmd.TextureId != IntPtr.Zero)
                        {
                            if (drawCmd.TextureId == _fontAtlasID)
                            {
                                immediateContext.SetTexture(_fontTexture!, 1);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }

                        immediateContext.BindShader(_imguiShader!);

                        immediateContext.DrawIndexed(drawCmd.ElemCount, 1, drawCmd.IdxOffset + (uint)indexOffset, (int)drawCmd.VtxOffset + vertexOffset, 0);
                    }

                    vertexOffset += cmdList.VtxBuffer.Size;
                    indexOffset += cmdList.IdxBuffer.Size;
                }
            };
        }

        private void OnFrameEnd()
        {
        }

        private void UpdateInput()
        {
        }

   

    }
}
