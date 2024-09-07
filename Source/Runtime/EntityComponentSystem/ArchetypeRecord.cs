namespace UOEngine.Runtime.EntityComponentSystem
{
    public readonly struct ArchetypeRecord(int componentDataIndex)
    {
        public readonly int ComponentDataIndex = componentDataIndex;
    }
}
