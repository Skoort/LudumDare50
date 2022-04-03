using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    private Dictionary<PoolableObject, Queue<PoolableObject>> _prefabInstances;

    private void Awake()
    {
        Debug.Assert(Instance == null, "ObjectPool.Awake: Attempted to create multiple instances of ObjectPool.");

        Instance = this;
        _prefabInstances = new Dictionary<PoolableObject, Queue<PoolableObject>>();
    }

    public T RequestInstance<T>(PoolableObject prefab,
        Transform desiredParent = null, Vector3? desiredPosition = null, Vector3? desiredRight = null) where T : Component
    {
        _prefabInstances.TryGetValue(prefab, out var freeInstances);

        PoolableObject prefabInstance;
        if (freeInstances?.Count > 0)
        {
            prefabInstance = freeInstances.Dequeue();
            if (freeInstances.Count <= 0)
            {
                _prefabInstances.Remove(prefab);
            }
        }
        else
        {
            prefabInstance = Instantiate(prefab, desiredParent ?? this.transform);
            prefabInstance.Prefab = prefab;
        }

        if (desiredPosition.HasValue)
        {
            prefabInstance.transform.position = desiredPosition.Value;
        }
        if (desiredRight.HasValue)
        {
            prefabInstance.transform.right = desiredRight.Value;
        }

        prefabInstance.OnRequested();

        return prefabInstance.transform.GetComponent<T>();
    }

    public void ReleaseInstance(PoolableObject prefabInstance)
    {
        var prefab = prefabInstance.Prefab;
        if (!_prefabInstances.TryGetValue(prefab, out var freeInstances))
        {
            freeInstances = new Queue<PoolableObject>();
            _prefabInstances.Add(prefab, freeInstances);
        }
        freeInstances.Enqueue(prefabInstance);

        prefabInstance.OnReclaimed();
    }
}