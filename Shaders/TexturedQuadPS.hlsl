struct VsToPs
{
    float4 colour : COLOR0;
    float4 position : SV_Position;
};

float4 main(in VsToPs input) : SV_TARGET
{
    return input.colour;
}