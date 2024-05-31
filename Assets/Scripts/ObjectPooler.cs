using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    public Dictionary<string, Transform> poolParents;

    #region Singleton
    public static ObjectPooler Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolParents = new Dictionary<string, Transform>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Parent objeyi oluþtur ve hiyerarþide düzenli tut
            GameObject poolParent = new GameObject(pool.tag + " Pool");
            poolParent.transform.SetParent(transform);
            poolParents[pool.tag] = poolParent.transform;

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(poolParent.transform);  // Parent olarak belirli bir Transform ayarlýyoruz
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        if (poolDictionary[tag].Count == 0)
        {
            // Havuz boþsa, yeni bir obje oluþtur
            ExtendPool(tag);
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    //public void RemoveFromPool(string tag, GameObject objectToRemove)
    //{
    //    if (poolDictionary.ContainsKey(tag) && poolDictionary[tag].Contains(objectToRemove))
    //    {
    //        poolDictionary[tag].Dequeue();
    //        Destroy(objectToRemove);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Object to remove not found in pool or wrong tag.");
    //    }
    //}

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return;
        }

        objectToReturn.SetActive(false);
        objectToReturn.transform.SetParent(poolParents[tag]);  // Obje tekrar parent altýna yerleþtir
        poolDictionary[tag].Enqueue(objectToReturn);
    }

    private void ExtendPool(string tag)
    {
        Pool pool = pools.Find(p => p.tag == tag);
        if (pool != null)
        {
            GameObject obj = Instantiate(pool.prefab);
            obj.SetActive(false);
            obj.transform.SetParent(poolParents[tag]);  // Yeni objeyi de doðru parent altýna yerleþtir
            poolDictionary[tag].Enqueue(obj);
        }
        else
        {
            Debug.LogError("No pool exists with tag: " + tag);
        }
    }
}
