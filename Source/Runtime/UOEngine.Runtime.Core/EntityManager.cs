using Microsoft.Extensions.DependencyInjection;

namespace UOEngine.Runtime.Core;

public class EntityManager
{
    private List<IEntity> _entities = [];

    private readonly IServiceProvider _serviceProvider;

    public EntityManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        serviceProvider.GetRequiredService<ApplicationLoop>().OnUpdate += Update;
    }

    public T NewEntity<T>(params object[] extraArgs) where T : IEntity
    {
        // ActivatorUtilities will:
        // 1. Look at T's constructor
        // 2. Inject services from _serviceProvider
        // 3. Use extraArgs for the rest of the constructor parameters
        T newEntity = ActivatorUtilities.CreateInstance<T>( _serviceProvider, extraArgs);

        _entities.Add(newEntity);

        return newEntity;
    }

    internal void Update(float time)
    {
        foreach (var entity in _entities)
        {
            entity.Update(time);
        }
    }
}
