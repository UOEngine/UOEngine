
struct VsToPs
{
    float4 position : SV_Position;
    float4 colour : COLOR;
};

float4 main(in VsToPs input) : SV_TARGET
{
    return input.colour;
}    