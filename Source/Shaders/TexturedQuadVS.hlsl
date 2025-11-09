
struct VsToPs
{
    float4 position : SV_Position;
    float2 uv: TEXCOORD0;
    uint instanceIndex: SV_InstanceID;
};

static const float width = 44.0;
static const float height = 44.0;

static const float4 cQuadVertsNDC[4] =
{
    {   0.0,   0.0,  0.0, 1.0},
    {   0.0, width,  0.0, 1.0},
    { width, width,  0.0, 1.0},
    { width,   0.0,  0.0, 1.0},
};

static const float2 cQuadUVs[4] =
{
    { 0.0,  0.0 },
    { 0.0,  1.0 },
    { 1.0,  1.0 },
    { 0.0,  1.0 },
};

cbuffer PerViewData : register(b0, space1)
{
    float4x4 View;
    float4x4 Projection;
};

VsToPs main(uint vid : SV_VertexID, uint instance_id : SV_InstanceID)
{
    VsToPs output;

    float4 vert = cQuadVertsNDC[vid];

    output.position = mul(Projection, mul(View, vert));
    output.position.w = 1.0f;
    output.uv = cQuadUVs[vid];
    output.instanceIndex = instance_id;

    return output;
}
