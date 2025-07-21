struct VsToPs
{
    float4 position : SV_Position;
    float2 uv: TEXCOORD0;
};

static const float width = 640.0;
static const float height = 480.0;

static const float4 cQuadVertsNDC[4] =
{
    { 1.0, 0.0,  0.0, 1.0},
    { 0.0, 0.0,  0.0, 1.0},
    { 1.0, 1.0,  0.0, 1.0},
    { 0.0, 1.0f, 0.0, 1.0},
};

static const float2 cQuadUVs[4] =
{
    { 1.0,  0.0 },
    { 0.0,  0.0 },
    { 1.0,  1.0 },
    { 0.0,  1.0 },
};

struct PerFrameData
{
    float4x4 Projection;
};

ConstantBuffer<PerFrameData> cbPerFrameData: register(b0);

VsToPs main( uint vid : SV_VertexID )
{
    VsToPs output;

    float4 vert = float4(width * cQuadVertsNDC[vid].x, height * cQuadVertsNDC[vid].y, 0.0, 1.0);
    
    output.position = mul(cbPerFrameData.Projection, vert);
    output.uv = cQuadUVs[vid];
    
    return output;
}