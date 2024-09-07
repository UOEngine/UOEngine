namespace UOEngine.Runtime.EntityComponentSystem
{
    public readonly record struct ComponentType(ulong id): IComparer<ComponentType>
    {
        public readonly ulong Id = id;
        public Type Type => ComponentRegistry.Types[(int)Id];

        public int Compare(ComponentType x, ComponentType y)
        {
            throw new NotImplementedException();
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
