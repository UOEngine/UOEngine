Texture2D Texture : register(t0, space2);
SamplerState TextureSampler : register(s0, space2);

cbuffer Parameters : register(b0, space1)
{
    float4 DiffuseColor = float4(1.0f, 1.0f, 1.0f, 1.0f);  
}

cbuffer ProjectionMatrix : register(b1, space1)
{
    float4x4 WorldViewProj;
};


//float ComputeFogFactor(float4 position)
//{
//    return saturate(dot(position, FogVector));
//}


//void ApplyFog(inout float4 color, float fogFactor)
//{
//    color.rgb = lerp(color.rgb, FogColor * color.a, fogFactor);
//}


void AddSpecular(inout float4 color, float3 specular)
{
    color.rgb += specular * color.a;
}


struct CommonVSOutput
{
    float4 Pos_ps;
    float4 Diffuse;
    float3 Specular;
    float FogFactor;
};


CommonVSOutput ComputeCommonVSOutput(float4 position)
{
    CommonVSOutput vout;
    
    //vout.Pos_ps = mul(position, WorldViewProj);
    vout.Pos_ps = mul(WorldViewProj, position);
    vout.Diffuse = DiffuseColor;
    vout.Specular = 0;
    //vout.FogFactor = ComputeFogFactor(position);
    
    return vout;
}


#define SetCommonVSOutputParams \
    vout.PositionPS = cout.Pos_ps; \
    vout.Diffuse = cout.Diffuse; \
    vout.Specular = float4(cout.Specular, cout.FogFactor);


#define SetCommonVSOutputParamsNoFog \
    vout.PositionPS = cout.Pos_ps; \
    vout.Diffuse = cout.Diffuse;




struct VSInputTxVc
{
    float2 Position : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float4 Color : TEXCOORD2;
};

struct VSOutputTxNoFog
{
    float4 Diffuse : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float4 PositionPS : SV_Position;
};

struct PSInputTxNoFog
{
    float4 Diffuse : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

// Vertex shader: texture + vertex color, no fog.
VSOutputTxNoFog VSBasicTxVcNoFog(VSInputTxVc vin)
{
    VSOutputTxNoFog vout;
    
    float4 p = float4(vin.Position.xy, 0.5f, 1.0f);
    
    CommonVSOutput cout = ComputeCommonVSOutput(p);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}

float4 PSBasicTxNoFog(PSInputTxNoFog input) : SV_Target
{
    float4 tex = Texture.Sample(TextureSampler, input.TexCoord);
    return tex * input.Diffuse;
}
