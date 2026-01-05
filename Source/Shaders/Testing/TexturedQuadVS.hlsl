
struct VSInput {
    float3 position : POSITION;
    float4 colour : COLOR;
};


struct VsToPs
{
    float4 position : SV_Position;
    float4 colour : COLOR;
};

VsToPs main(VSInput input)
{
    VsToPs output;

    output.position = float4(input.position, 1.0f);
    output.colour = input.colour;

    return output;
}
