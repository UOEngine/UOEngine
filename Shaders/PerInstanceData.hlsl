#define HLSL 1

#define PER_FRAME_UPDATE    space0
#define PER_DRAW_UPDATE     space1
#define BINDLESS_UPDATE     space2

struct PerInstanceData
{
    float3  mTileCorner;
    uint    mTextureIndex;
};

StructuredBuffer<PerInstanceData> sbPerInstanceData: register(t0, PER_FRAME_UPDATE);