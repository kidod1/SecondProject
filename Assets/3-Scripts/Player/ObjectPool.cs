using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject[] objectPrefabs;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private Transform poolParent;

    private List<List<GameObject>> pools = new List<List<GameObject>>();
    private List<int> currentIndexes = new List<int>();

    private void Awake()
    {
        if (poolParent == null)
        {
            poolParent = new GameObject("ObjectPoolParent").transform;
        }

        foreach (GameObject prefab in objectPrefabs)
        {
            List<GameObject> pool = new List<GameObject>();
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab, poolParent);
                obj.SetActive(false);
                pool.Add(obj);
            }
            pools.Add(pool);
            currentIndexes.Add(0);
        }
    }

    public GameObject GetObject(int prefabIndex)
    {
        if (prefabIndex < 0 || prefabIndex >= pools.Count)
        {
            Debug.LogError("Invalid prefab index");
            return null;
        }

        List<GameObject> pool = pools[prefabIndex];
        int currentIndex = currentIndexes[prefabIndex];

        GameObject obj = pool[currentIndex];
        obj.SetActive(true);

        // Move to the next index, and wrap around if necessary
        currentIndexes[prefabIndex] = (currentIndex + 1) % poolSize;

        return obj;
    }

    public void ReturnObject(GameObject obj, int prefabIndex)
    {
        if (prefabIndex < 0 || prefabIndex >= pools.Count)
        {
            Debug.LogError("Invalid prefab index");
            return;
        }

        obj.SetActive(false);
    }
}
