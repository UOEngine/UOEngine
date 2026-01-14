
struct VSInput {
    float3 position : POSITION;
    float2 uv : TEXCOORD0;
};


struct VsToPs
{
    float4 position : SV_Position;
    float2 uv : TEXCOORD0;
};

cbuffer ProjectionMatrix : register(b0, space0)
{
    float4x4 WorldViewProj;
};

VsToPs main(VSInput input)
{
    VsToPs output;

    output.position = mul(WorldViewProj, float4(input.position, 1.0f));
    output.uv = input.uv;

    return output;
}
