// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
struct VsToPs
{
    float4 position : SV_Position;
    float2 uv: TEXCOORD0;
};

Texture2D Texture : register(t0, space0);
SamplerState Sampler : register(s1, space0);

float4 main(in VsToPs input) : SV_TARGET
{
    float4 colour = Texture.Sample(Sampler, input.uv);
    
    return colour;
}    