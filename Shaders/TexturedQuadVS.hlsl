#include "PerInstanceData.hlsl"

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
    { width, 0.0,  0.0, 1.0},
    { 0.0, 0.0,  0.0, 1.0},
    { width, width,  0.0, 1.0},
    { 0.0, width, 0.0, 1.0},
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
    float4x4 mWorldToCamera;
};

ConstantBuffer<PerFrameData> cbPerFrameData: register(b0, PER_FRAME_UPDATE);

VsToPs main( uint vid : SV_VertexID, uint instance_id : SV_InstanceID )
{
    VsToPs output;

    float3 tileCorner = sbPerInstanceData[instance_id].mTileCorner;

    float4 vert = cQuadVertsNDC[vid];
    float4 adjustedTileCorner = mul(cbPerFrameData.mWorldToCamera, float4(tileCorner, 1.0f));

    output.position =  mul(cbPerFrameData.Projection, adjustedTileCorner + vert);

    output.position.z = 0.0f;
	output.position.w = 1.0f;

    output.uv = cQuadUVs[vid];
    
    output.instanceIndex = instance_id;

    return output;
}