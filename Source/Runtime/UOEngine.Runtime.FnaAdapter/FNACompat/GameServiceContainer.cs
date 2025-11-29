namespace Microsoft.Xna.Framework;

public class GameServiceContainer
{
    private readonly Dictionary<Type, object> services = [];

    public GameServiceContainer()
    {
    }
    public void AddService(Type type, object provider)
    {
        if (type == null)
        {
            throw new ArgumentNullException("type");
        }
        if (provider == null)
        {
            throw new ArgumentNullException("provider");
        }
        if (!type.IsAssignableFrom(provider.GetType()))
        {
            throw new ArgumentException(
                "The provider does not match the specified service type!"
            );
        }

        services.Add(type, provider);
    }

    public T? GetService<T>() where T : class
    {
        if (services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }

        return null;
    }
}
