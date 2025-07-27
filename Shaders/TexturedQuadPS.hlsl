struct VsToPs
{
    float4 position : SV_Position;
    float2 uv: TEXCOORD0;
};

Texture2D<float4> texture : register(t1);
SamplerState bilinear_clamp_sampler : register(s0);

float4 main(in VsToPs input) : SV_TARGET
{
    float4 colour = texture.Sample(bilinear_clamp_sampler, input.uv);

    return colour;
}    