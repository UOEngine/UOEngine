struct Output
{
    float4 position : SV_Position;
    float2 uv : TEXCOORD0;
};

static const float4 cQuadVertsNDC[4] =
{
    {-0.5, -0.5, 0.0, 1.0},
    {-0.5,  0.5, 0.0, 1.0},
    { 0.5, -0.5, 0.0, 1.0},
    { 0.5,  0.5, 0.0, 1.0},
};

static const float2 cQuadUVs[4] =
{
    { 0.0,  1.0 },
    { 0.0,  0.0 },
    { 1.0,  1.0 },
    { 1.0,  0.0 },
};

Output main( uint vid : SV_VertexID )
{
    Output output;
    
    output.position = cQuadVertsNDC[vid];
    output.uv = cQuadUVs[vid];

    return output;
}