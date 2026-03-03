using System;
using UnityEngine;


[CreateAssetMenu(fileName = "NewObjectPoolList", menuName = "Object Pools/NewObjectPoolList")]
public class ObjectPool : ScriptableObject
{
    public int poolSize;
    public GameObject prefabe;

    private GameObject Holder;
    private GameObject[] pool;
    private int poolCount;

    private bool is_init;

    public int PoolCount
    {
        get { return poolCount; }
        private set { poolCount = value; }
    }

    private void ExtandPool(int by)
    {
        int listSize = poolSize + by;
        GameObject[] newList = new GameObject[listSize];
        try
        {
            pool.CopyTo(newList, 0);
        }
        catch (ArgumentOutOfRangeException e)
        {
            Debug.LogError(this.ToString() + " ERROR: " + e.Message + "\n" + "poolSize: " + poolSize + " listSize: " + listSize);
        }
        for (int i = 0; i < by; i++)
        {
            newList[poolSize + i] = Instantiate(prefabe, Holder.transform);
            newList[poolSize + i].SetActive(false);
        }
        poolSize = listSize;
        pool = newList;

    }

    public void Init()
    {
        pool = new GameObject[poolSize];
        Holder = new GameObject($"{prefabe.name} poolHolder");
        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = Instantiate(prefabe, Holder.transform);
            pool[i].SetActive(false);
        }
    }

    public GameObject GetInstance(Transform spawnPoint)
    {
        return GetInstance(spawnPoint.position, spawnPoint.rotation);
    }

    public GameObject GetInstance(Vector3 spawnPoint)
    {
        return GetInstance(spawnPoint, Quaternion.identity);
    }
    public GameObject GetInstance(Vector3 spawnPoint, Quaternion rotation)
    {
        if (!Holder)
        {
            Init();
        }
        foreach (GameObject obj in pool)
        {
            if (obj != null && !obj.activeInHierarchy)
            {
                obj.transform.position = spawnPoint;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                return obj;
            }
        }
        ExtandPool(10);
        return pool[poolSize - 9];
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void ResetPool()
    {
        foreach (GameObject obj in pool)
        {
            obj.SetActive(false);
        }
    }
}

