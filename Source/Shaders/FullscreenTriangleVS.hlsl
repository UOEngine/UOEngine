// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
struct VsToPs
{
    float4 position : SV_Position;
    float2 uv: TEXCOORD0;
};

static const float4 cQuadVertsNDC[3] =
{
    { -1.0f, -1.0f,  0.0, 1.0f},
    { -1.0f,  3.0f,  0.0, 1.0f},
    {  3.0f, -1.0f,  0.0, 1.0},
};

static const float2 cQuadUVs[4] =
{
    { 0.0,  0.0 },
    { 0.0,  1.0 },
    { 1.0,  1.0 },
    { 0.0,  1.0 },
};

VsToPs main(uint vid : SV_VertexID, uint instance_id : SV_InstanceID)
{
    VsToPs output;

    float4 vert = cQuadVertsNDC[vid];

    output.position = vert;
    output.uv = cQuadUVs[vid];

    return output;
}
