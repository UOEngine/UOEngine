using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine;

internal class EntityManager
{
    public static EntityManager Instance => _lazy.Value;

    private static readonly Lazy<EntityManager> _lazy = new(() => new EntityManager());

    private List<IEntity> _entities = [];

    private EntityManager()
    {

    }

    public T NewEntity<T>() where T : IEntity, new()
    {
        T newEntity = new T();

        _entities.Add(newEntity);

        return newEntity;
    }

    public T NewEntity<T>(Func<T> createFunc) where T : IEntity
    {
        T newEntity = createFunc();

        _entities.Add(newEntity);

        return newEntity;
    }
}
