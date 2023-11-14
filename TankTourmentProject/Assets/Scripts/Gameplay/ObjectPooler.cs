using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [Serializable]
    private struct PoolData
    {
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField] public int MaxAmount { get; private set; }
    }
    
    [Header("Settings")]
    [SerializeField] private int defaultMaxPool = 10;
    [SerializeField] private List<PoolData> poolData = new List<PoolData>();
    
    private Dictionary<GameObject,Queue<GameObject>> poolDictionary = new ();

    private static ObjectPooler instance;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    
    private void Start()
    {
        foreach (var data in poolData)
        {
            GetPool(data.Prefab,data.MaxAmount);
        }
    }

    private Queue<GameObject> GetPool(GameObject prefab,int amount = 0)
    {
        if(poolDictionary.TryGetValue(prefab, out var pool)) return pool;

        var queue = new Queue<GameObject>();
        
        poolDictionary.Add(prefab, queue);
        
        if(amount == 0) amount = defaultMaxPool;
        for (int i = 0; i < amount; i++)
        {
            var obj = Instantiate(prefab,transform);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }
        
        return queue;
    }
    
    private T GetObjectFromPool<T>(T prefab) where T : Component
    {
        var pool = GetPool(prefab.gameObject);

        var obj = pool.Dequeue();
        pool.Enqueue(obj);
        
        return obj.GetComponent<T>();
    }
    
    public static T Pool<T>(T prefab,Vector3 position,Quaternion rotation,Transform parent = null) where T : Component
    {
        if (instance == null)
        {
            Debug.LogWarning("No Pool Instance Found!");
            return Instantiate(prefab, position, rotation, parent);
        }
        
        var obj = instance.GetObjectFromPool(prefab);
        
        var tr = obj.transform;
        
        tr.SetPositionAndRotation(position,rotation);
        tr.SetParent(parent);
        
        obj.gameObject.SetActive(true);
        
        return obj;
    }
}
    