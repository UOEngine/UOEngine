struct VsToPs
{
    float4 colour : COLOR0;
    float4 position : SV_Position;
};

static const float4 cQuadVertsNDC[4] =
{
    { 0.5,  0.5, 0.0, 1.0},
    {-0.5,  0.5, 0.0, 1.0},
    { 0.5, -0.5, 0.0, 1.0},
    {-0.5, -0.5, 0.0, 1.0},
};

static const float2 cQuadUVs[4] =
{
    { 1.0,  1.0 },
    { 0.0,  1.0 },
    { 1.0,  0.0 },
    { 0.0,  0.0 },
};

VsToPs main( uint vid : SV_VertexID )
{
    VsToPs output;
    
    output.position = cQuadVertsNDC[vid];

    output.colour = 1;
    output.colour[vid % 3] = 0;
    
    return output;
}