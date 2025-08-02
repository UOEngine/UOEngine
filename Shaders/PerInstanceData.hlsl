#define HLSL 1

#define PER_FRAME_UPDATE    space0
#define PER_DRAW_UPDATE     space1
#define BINDLESS_UPDATE     space2

struct PerInstanceData
{
    float4x4 ModelToWorld;
    uint     mTextureIndex;
};

StructuredBuffer<PerInstanceData> sbPerInstanceData: register(t0, PER_FRAME_UPDATE);