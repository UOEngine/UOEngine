namespace UOEngine.Runtime.EntityComponentSystem
{
    public readonly struct EntityRecord(Archetype archetype, int row)
    {
        readonly public Archetype   Archetype = archetype;
        readonly public int         Row = row;
    }
}
