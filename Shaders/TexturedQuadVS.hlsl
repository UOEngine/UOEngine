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

struct PerInstanceData
{
    float4x4 ModelToWorld;
};

ConstantBuffer<PerFrameData> cbPerFrameData: register(b0);

StructuredBuffer<PerInstanceData> sbPerInstanceData: register(t0);

VsToPs main( uint vid : SV_VertexID, uint instance_id : SV_InstanceID )
{
    VsToPs output;

    float4x4 model = sbPerInstanceData[instance_id].ModelToWorld;

    float4 vert = float4(width * cQuadVertsNDC[vid].x, height * cQuadVertsNDC[vid].y, 0.0, 1.0);
    
    float4 vertex_world_space = mul(model, vert);

    output.position = mul(cbPerFrameData.Projection, vertex_world_space);
    output.uv = cQuadUVs[vid];
    
    return output;
}