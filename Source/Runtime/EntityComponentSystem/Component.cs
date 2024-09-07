namespace UOEngine.Runtime.EntityComponentSystem
{
    public readonly record struct ComponentType(ulong id): IComparable
    {
        public readonly ulong Id = id;
        public Type Type => ComponentRegistry.Types[(int)Id];

        public int Compare(ComponentType x, ComponentType y)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object? other)
        {
            if(other == null)
            {
                return 1;
            }

            ComponentType otherComponentType = (ComponentType)other;

            return Id.CompareTo(otherComponentType.Id);
        }
    }



    public static class Component<T> where T: struct
    {
        public static readonly ComponentType ComponentType;

        static Component()
        {
            ComponentType = ComponentRegistry.Add<T>();
        }
    }
}
