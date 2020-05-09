using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A pool of objects of type T
/// </summary>
/// <typeparam name="T">The type of pool objects</typeparam>
public abstract class GenericObjectPool<T>
{
    readonly private List<T> activeObjects = new List<T>();
    readonly private List<T> inactiveObjects = new List<T>();
    protected abstract T CreateNewObject(params object[] args);

    public T GetObject(params object[] args)
    {
        T recycledObject;
        if (inactiveObjects.Count == 0)
        {
            recycledObject = CreateNewObject(args);
        }
        else
        {
            recycledObject = inactiveObjects[0];
            inactiveObjects.RemoveAt(0);
        }
        activeObjects.Add(recycledObject);
        return recycledObject;
    }

    public void PutObject(T recycledObject)
    {
        activeObjects.Remove(recycledObject);
        if (!inactiveObjects.Contains(recycledObject) && recycledObject != null)
        {
            inactiveObjects.Add(recycledObject);
        }
    }
}

/// <summary>
/// A general version of object pools factory
/// It creates a pool of objects of the given type on demand
/// </summary>
public class ObjectPoolFactory
{
    readonly Dictionary<System.Type, object> pools = new Dictionary<System.Type, object>();
    protected virtual object CreatePoolOfType(System.Type type)
    {
        /*
        // NOTE: this code won't work due to the abscence of CreateNewObject implementation!!!
        System.Type genericPoolType = typeof(GenericObjectPool<>).MakeGenericType(new System.Type[] { type });
        return System.Activator.CreateInstance(genericPoolType);
        */
        return null;
    }

    protected GenericObjectPool<T> GetPool<T>()
    {
        System.Type type = typeof(T);
        GenericObjectPool<T> pool;
        if (!pools.ContainsKey(type))
        {
            //Debug.Log($"[{GetType().Name}.{nameof(GetPool)}] Just-in-time creating a pool of {type.Name}");
            pool = (GenericObjectPool<T>)CreatePoolOfType(type);
            pools.Add(type, pool);
        }
        else
        {
            //Debug.Log($"[{GetType().Name}.{nameof(GetPool)}] Get an existing pool of {type.Name}");
            pool = (GenericObjectPool<T>)pools[type];
        }
        return pool;
    }
    public T GetObject<T>(params object[] args)
    {
        GenericObjectPool<T> pool = GetPool<T>();
        return pool.GetObject(args);
    }

    public void PutObject<T>(T obj)
    {
        GenericObjectPool<T> pool = GetPool<T>();
        pool.PutObject(obj);
    }
}

/// <summary>
/// An interface to object pool creators
/// </summary>
public interface IPoolCreator
{
    System.Type[] SupportedTypes { get; }
    object CreatePoolOfType(System.Type type);
    void Initialize();
}

/// <summary>
/// A universal object pool factory: it creates GenericObjectPools of given type using a list of predefined object pool creators
/// </summary>
public class UniversalObjectPool : ObjectPoolFactory
{
    protected Dictionary<System.Type, IPoolCreator> Creators { get; } = new Dictionary<System.Type, IPoolCreator>();
    protected override object CreatePoolOfType(System.Type type)
    {
        if (Creators.ContainsKey(type))
        {
            return Creators[type].CreatePoolOfType(type);
        }
        Debug.LogError($"[{GetType().Name}.{nameof(CreatePoolOfType)}] No pool of {type.Name} creators available");
        return null;
    }

    public void AddPoolCreator(IPoolCreator creator)
    {
        if (creator != null)
        {
            foreach (System.Type type in creator.SupportedTypes)
            {
                if (!Creators.ContainsKey(type))
                {
                    //Debug.Log($"[{nameof(UniversalObjectPool)}.{nameof(AddPoolCreator)}] Adding pool creator {creator.GetType().Name} for type {type.Name}");
                    Creators.Add(type, creator);
                }
            }
        }
    }

}

