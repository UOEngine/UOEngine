using System.Numerics;
using System.Runtime.CompilerServices;

using ImGuiNET;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.Rendering;
using Renderer = UOEngine.Runtime.Rendering.Renderer;

namespace UOEngine.Apps.Editor
{
    public class UOEngineImGui
    {
        public UOEngineImGui(Input input, Renderer renderer, GameLoop gameLoop, Window window) 
        {
            _input = input;
            _renderDeviceContext = renderer.Context;
            _renderDevice = renderer.Device;
            _window = window;

            gameLoop.FrameStarted += OnFrameStart;
            renderer.FrameEnd += OnFrameEnd;
        }

        public void Initialise()
        {
            var context = ImGui.CreateContext();

            ImGui.SetCurrentContext(context);

            SetupInput();

            _vertexBuffer = _renderDevice.CreateRenderBuffer(0, ERenderBufferType.Vertex);
            _indexBuffer = _renderDevice.CreateRenderBuffer(0, ERenderBufferType.Index);

            _imguiShaderId = _renderDevice.RegisterShader<ImGuiShader>();

            ImGuiIOPtr io = ImGui.GetIO();

            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out var width, out var height, out var bytesPerPixel);

            _fontTexture = _renderDevice.CreateTexture2D(new()
            {
                Width = (uint)width,
                Height = (uint)height,
                Format = ERenderTextureFormat.R8G8B8A8
            });

            _fontTexture.Upload(pixels);

            io.Fonts.SetTexID(_fontAtlasID);

            io.Fonts.ClearTexData();

        }

        private void SetupInput()
        {
        }

        private void OnFrameStart(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.DisplaySize = new Vector2(_window.Width, _window.Height);

            io.DeltaTime = deltaSeconds;

            UpdateInput();

            ImGui.NewFrame();
        }

        private void OnFrameEnd()
        {
            ImGui.Begin("Hello world!");
            ImGui.Text("This is some useful text.");
            ImGui.End();

            ImGui.Render();

            var drawData = ImGui.GetDrawData();

            if((drawData.DisplaySize.X <= 0.0f) || (drawData.DisplaySize.Y <= 0.0f))
            {
                return;
            }

            uint newVertexBufferSize = (uint)(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());

            // TODO - free the buffers

            if (newVertexBufferSize > _vertexBuffer!.Length)
            {
                _verts = new ImDrawVert[newVertexBufferSize];
                _vertexBuffer = _renderDevice.CreateRenderBuffer(newVertexBufferSize, ERenderBufferType.Vertex);
            }

            uint newIndexBufferSize = (uint)(drawData.TotalIdxCount * sizeof(ushort));

            if (newIndexBufferSize > _indexBuffer!.Length)
            {
                _indices = new ushort[newIndexBufferSize];
                _indexBuffer = _renderDevice.CreateRenderBuffer(newIndexBufferSize, ERenderBufferType.Index);
            }

            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];

                _verts[vertexOffsetInVertices++] = new()
                {
                    pos = cmdList.VtxBuffer[n].pos,
                    col = cmdList.VtxBuffer[n].col,
                    uv = cmdList.VtxBuffer[n].uv
                };

                _indices[vertexOffsetInVertices++] = cmdList.IdxBuffer[n];

                vertexOffsetInVertices += (uint)cmdList.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmdList.IdxBuffer.Size;
            }

            _vertexBuffer.CopyToDevice<ImDrawVert>(_verts);
            _indexBuffer.CopyToDevice<ushort>(_indices);

            var commandList = _renderDeviceContext.ImmediateCommandList;

            commandList!.BindVertexBuffer(_vertexBuffer);
            commandList.BindIndexBuffer(_indexBuffer);
            commandList.BindShader(_imguiShaderId);

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

                    commandList.DrawIndexed(0, 0);
                }
            }
        }

        private void UpdateInput()
        {
        }

        private Input                   _input;
        private RenderDeviceContext     _renderDeviceContext;
        private RenderDevice            _renderDevice;
        private Window                  _window;

        private IntPtr                  _fontAtlasID = 1;
        private RenderTexture2D?        _fontTexture;

        private RenderBuffer?           _vertexBuffer;
        private RenderBuffer?           _indexBuffer;

        private ImDrawVert[]            _verts = [];
        private ushort[]                _indices = [];

        private int                     _imguiShaderId = 0xFF;       

    }
}
