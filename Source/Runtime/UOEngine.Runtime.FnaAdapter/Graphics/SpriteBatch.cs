using System.Runtime.InteropServices;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class SpriteBatch
{
    private const int MAX_SPRITES = 2048;
    private const int MAX_VERTICES = MAX_SPRITES * 4;
    private const int MAX_INDICES = MAX_SPRITES * 6;

    private readonly GraphicsDevice _graphicsDevice;

    private bool _beginCalled;

    private SpriteSortMode _sortMode;

    private BlendState _blendState;
    private SamplerState _samplerState;
    private DepthStencilState _depthStencilState;
    private RasterizerState _rasterizerState;
    private Matrix _transformMatrix;

    private Effect _customEffect;

    private DynamicVertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    private VertexPositionColorTexture4[] _vertexInfo;

    private Texture2D[] _textureInfo = new Texture2D[MAX_SPRITES];

    private int _numSprites = 0;

    private static readonly short[] indexData = GenerateIndexArray();

    private static readonly float[] CornerOffsetX = new float[]
    {
        0.0f,
        1.0f,
        0.0f,
        1.0f
    };

    private static readonly float[] CornerOffsetY = new float[]
    {
        0.0f,
        0.0f,
        1.0f,
        1.0f
    };

    public SpriteBatch(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

        _vertexBuffer = new DynamicVertexBuffer(_graphicsDevice, typeof(VertexPositionColorTexture), MAX_VERTICES, BufferUsage.WriteOnly);

        _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, MAX_INDICES, BufferUsage.WriteOnly);

        _indexBuffer.SetData(indexData);

        _vertexInfo = new VertexPositionColorTexture4[MAX_SPRITES];

    }

    public void Begin()
    {
        Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            Matrix.Identity
        );
    }

    public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState,
                    RasterizerState rasterizerState, Effect? effect, Matrix transformMatrix)
    {
        if (_beginCalled)
        {
            throw new InvalidOperationException(
                "Begin has been called before calling End" +
                " after the last call to Begin." +
                " Begin cannot be called again until" +
                " End has been successfully called."
            );
        }

        _beginCalled = true;

        _sortMode = sortMode;

        _blendState = blendState ?? BlendState.AlphaBlend;
        _samplerState = samplerState ?? SamplerState.LinearClamp;
        _depthStencilState = depthStencilState ?? DepthStencilState.None;
        _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;

        _customEffect = effect;
        _transformMatrix = transformMatrix;
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
    {
        PushSprite(
            texture,
            0.0f,
            0.0f,
            1.0f,
            1.0f,
            destinationRectangle.X,
            destinationRectangle.Y,
            destinationRectangle.Width,
            destinationRectangle.Height,
            color,
            0.0f,
            0.0f,
            0.0f,
            1.0f,
            0.0f,
            0
        );
    }

    public void Begin(SpriteSortMode sortMode, BlendState blendState)
    {
        throw new NotImplementedException();
    }

    public void Draw(Texture2D texture, Vector2 position, Color color)
    {
        throw new NotImplementedException();
    }

    public void Draw(RenderTarget2D texture, Vector2 position, Color color)
    {
        throw new NotImplementedException();
    }

    public void End()
    {
        FlushBatch();
    }

    private void FlushBatch()
    {
        PrepRenderState();

        if (_numSprites == 0)
        {
            return;
        }

        _numSprites = 0;
    }

    private void PrepRenderState()
    {
        _graphicsDevice.BlendState = _blendState;
        _graphicsDevice.SamplerStates[0] = _samplerState;
        _graphicsDevice.DepthStencilState = _depthStencilState;
        _graphicsDevice.RasterizerState = _rasterizerState;

        _graphicsDevice.SetVertexBuffer(_vertexBuffer);
    }

    private unsafe void PushSprite(
        Texture2D texture,
        float sourceX,
        float sourceY,
        float sourceW,
        float sourceH,
        float destinationX,
        float destinationY,
        float destinationW,
        float destinationH,
        Color color,
        float originX,
        float originY,
        float rotationSin,
        float rotationCos,
        float depth,
        byte effects
        )
    {
        if(_sortMode != SpriteSortMode.Deferred)
        {
            throw new NotImplementedException();
        }

        fixed (VertexPositionColorTexture4* sprite = &_vertexInfo[_numSprites])
        {
            GenerateVertexInfo(
                sprite,
                sourceX,
                sourceY,
                sourceW,
                sourceH,
                destinationX,
                destinationY,
                destinationW,
                destinationH,
                color,
                originX,
                originY,
                rotationSin,
                rotationCos,
                depth,
                effects
            );
        }

        _textureInfo[_numSprites] = texture;
        _numSprites++;
    }

    private void DrawPrimitive(Texture texture, int baseSprit, int batchSize)
    {
        throw new NotImplementedException();
    }

    private static short[] GenerateIndexArray()
    {
        short[] result = new short[MAX_INDICES];
        for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
        {
            result[i] = (short)(j);
            result[i + 1] = (short)(j + 1);
            result[i + 2] = (short)(j + 2);
            result[i + 3] = (short)(j + 3);
            result[i + 4] = (short)(j + 2);
            result[i + 5] = (short)(j + 1);
        }
        return result;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct VertexPositionColorTexture4 : IVertexType
    {
        public const int RealStride = 96;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Vector3 Position0;
        public Color Color0;
        public Vector2 TextureCoordinate0;
        public Vector3 Position1;
        public Color Color1;
        public Vector2 TextureCoordinate1;
        public Vector3 Position2;
        public Color Color2;
        public Vector2 TextureCoordinate2;
        public Vector3 Position3;
        public Color Color3;
        public Vector2 TextureCoordinate3;
    }

    private static unsafe void GenerateVertexInfo(
    VertexPositionColorTexture4* sprite,
    float sourceX,
    float sourceY,
    float sourceW,
    float sourceH,
    float destinationX,
    float destinationY,
    float destinationW,
    float destinationH,
    Color color,
    float originX,
    float originY,
    float rotationSin,
    float rotationCos,
    float depth,
    byte effects
    )
    {
        float cornerX = -originX * destinationW;
        float cornerY = -originY * destinationH;
        sprite->Position0.X = (
            (-rotationSin * cornerY) +
            (rotationCos * cornerX) +
            destinationX
        );
        sprite->Position0.Y = (
            (rotationCos * cornerY) +
            (rotationSin * cornerX) +
            destinationY
        );
        cornerX = (1.0f - originX) * destinationW;
        cornerY = -originY * destinationH;
        sprite->Position1.X = (
            (-rotationSin * cornerY) +
            (rotationCos * cornerX) +
            destinationX
        );
        sprite->Position1.Y = (
            (rotationCos * cornerY) +
            (rotationSin * cornerX) +
            destinationY
        );
        cornerX = -originX * destinationW;
        cornerY = (1.0f - originY) * destinationH;
        sprite->Position2.X = (
            (-rotationSin * cornerY) +
            (rotationCos * cornerX) +
            destinationX
        );
        sprite->Position2.Y = (
            (rotationCos * cornerY) +
            (rotationSin * cornerX) +
            destinationY
        );
        cornerX = (1.0f - originX) * destinationW;
        cornerY = (1.0f - originY) * destinationH;
        sprite->Position3.X = (
            (-rotationSin * cornerY) +
            (rotationCos * cornerX) +
            destinationX
        );
        sprite->Position3.Y = (
            (rotationCos * cornerY) +
            (rotationSin * cornerX) +
            destinationY
        );
        fixed (float* flipX = &CornerOffsetX[0])
        {
            fixed (float* flipY = &CornerOffsetY[0])
            {
                sprite->TextureCoordinate0.X = (flipX[0 ^ effects] * sourceW) + sourceX;
                sprite->TextureCoordinate0.Y = (flipY[0 ^ effects] * sourceH) + sourceY;
                sprite->TextureCoordinate1.X = (flipX[1 ^ effects] * sourceW) + sourceX;
                sprite->TextureCoordinate1.Y = (flipY[1 ^ effects] * sourceH) + sourceY;
                sprite->TextureCoordinate2.X = (flipX[2 ^ effects] * sourceW) + sourceX;
                sprite->TextureCoordinate2.Y = (flipY[2 ^ effects] * sourceH) + sourceY;
                sprite->TextureCoordinate3.X = (flipX[3 ^ effects] * sourceW) + sourceX;
                sprite->TextureCoordinate3.Y = (flipY[3 ^ effects] * sourceH) + sourceY;
            }
        }
        sprite->Position0.Z = depth;
        sprite->Position1.Z = depth;
        sprite->Position2.Z = depth;
        sprite->Position3.Z = depth;
        sprite->Color0 = color;
        sprite->Color1 = color;
        sprite->Color2 = color;
        sprite->Color3 = color;
    }
}
