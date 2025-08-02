#include "PerInstanceData.hlsl"

struct VsToPs
{
    float4 position : SV_Position;
    float2 uv: TEXCOORD0;
    uint instanceIndex: SV_InstanceID;
};

Texture2D<float4> textures[] : register(t1, BINDLESS_UPDATE);

SamplerState bilinear_clamp_sampler : register(s0);

float4 main(in VsToPs input) : SV_TARGET
{
    uint index = sbPerInstanceData[input.instanceIndex].mTextureIndex;

    float4 colour = textures[index].Sample(bilinear_clamp_sampler, input.uv);

    return colour;
}    