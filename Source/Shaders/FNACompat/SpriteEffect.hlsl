Texture2D SpriteTexture : register(t0, space2);
SamplerState SpriteSampler : register(s0, space2);

cbuffer SpriteBatchBuffer : register(b0, space1)
{
    float4x4 MatrixTransform;
};

struct VSInput
{
    float4 Position : TEXCOORD0; // xy = vertex position, z = layer depth
    float4 Color : TEXCOORD01;
    float2 TexCoord: TEXCOORD2;
};

struct VSOutput
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VSOutput SpriteVertexShader(VSInput input)
{
    VSOutput output;

    float4 pos = float4(input.Position.xy, input.Position.z, 1.0f);
    output.Position = mul(pos, MatrixTransform);

    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    return output;
}

float4 SpritePixelShader(VSOutput input) : SV_Target
{
    float4 tex = SpriteTexture.Sample(SpriteSampler, input.TexCoord);
    return tex * input.Color;
}