#version 450

//layout(binding = 1) uniform sampler2D FontSampler;

layout (location = 0) in vec4 color;
layout (location = 1) in vec2 texCoord;

layout (location = 0) out vec4 outputColor;

void main()
{
    outputColor = color;// * texture(FontSampler, texCoord);
}