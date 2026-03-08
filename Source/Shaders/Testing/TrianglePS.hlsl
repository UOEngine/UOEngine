// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.

struct VsToPs
{
    float4 position : SV_Position;
    float4 colour : COLOR;
};

float4 main(in VsToPs input) : SV_TARGET
{
    return input.colour;
}    