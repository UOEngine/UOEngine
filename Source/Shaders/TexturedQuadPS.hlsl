
struct VsToPs
{
    float4 position : SV_Position;
    float2 uv: TEXCOORD0;
};

Texture2D Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

float4 main(in VsToPs input) : SV_TARGET
{
    float4 colour = Texture.Sample(Sampler, input.uv);
    
    return colour;
}    