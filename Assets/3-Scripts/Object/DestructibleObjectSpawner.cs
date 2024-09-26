using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestructibleObjectSpawner : MonoBehaviour
{
    // ��ȯ�� DestructibleObject ������
    [SerializeField]
    private GameObject[] destructiblePrefabs;

    // ��ȯ ��ġ �迭
    [SerializeField]
    private Transform[] spawnLocations;

    // ��ȯ �ð� ���� ���� (�ּҰ�, �ִ밪)
    [SerializeField]
    private float minSpawnInterval = 1f;
    [SerializeField]
    private float maxSpawnInterval = 5f;

    // ���� �� �ִ� ��ȯ ������ ������Ʈ ��
    [SerializeField]
    private int maxSpawnedObjects = 10;

    // �� ��ȯ�� ������Ʈ ��
    [SerializeField]
    private int totalSpawnCount = 20;

    private int currentSpawnedCount = 0;
    private int totalSpawnedCount = 0;

    private bool isSpawning = true;

    // ���� ��ȯ�Ǿ� �ִ� ������Ʈ���� �����ϱ� ���� ����Ʈ
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Start()
    {
        if (destructiblePrefabs.Length == 0)
        {
            Debug.LogError("DestructibleObjectSpawner: ��ȯ�� �������� �����Ǿ� ���� �ʽ��ϴ�.");
            isSpawning = false;
            return;
        }

        if (spawnLocations.Length == 0)
        {
            Debug.LogError("DestructibleObjectSpawner: ��ȯ ��ġ�� �����Ǿ� ���� �ʽ��ϴ�.");
            isSpawning = false;
            return;
        }

        // ��ȯ �ڷ�ƾ ����
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (isSpawning)
        {
            // ���� ��ȯ�� ������Ʈ ���� �� ��ȯ�� ������Ʈ ���� Ȯ���Ͽ� ��ȯ ���� ����
            if (currentSpawnedCount < maxSpawnedObjects && totalSpawnedCount < totalSpawnCount)
            {
                // ������ �ð� ���
                float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(waitTime);

                // ������ ������ ����
                int randomPrefabIndex = Random.Range(0, destructiblePrefabs.Length);
                GameObject prefabToSpawn = destructiblePrefabs[randomPrefabIndex];

                // ������ ��ȯ ��ġ ����
                int randomLocationIndex = Random.Range(0, spawnLocations.Length);
                Transform selectedSpawnLocation = spawnLocations[randomLocationIndex];

                // ������Ʈ ��ȯ
                GameObject spawnedObject = Instantiate(prefabToSpawn, selectedSpawnLocation.position, Quaternion.identity);

                // ��ȯ�� ������Ʈ ���� ����Ʈ�� �߰�
                spawnedObjects.Add(spawnedObject);

                // ��ȯ�� ������Ʈ �� ����
                currentSpawnedCount++;
                totalSpawnedCount++;

                // ��ȯ�� ������Ʈ�� �ı� �� �̺�Ʈ ���
                DestructibleObject destructible = spawnedObject.GetComponent<DestructibleObject>();
                if (destructible != null)
                {
                    destructible.OnDestroyed += OnObjectDestroyed;
                }
            }
            else
            {
                // �� �̻� ��ȯ�� �� ������ ���
                yield return null;

                // �� ��ȯ�ؾ� �� ������Ʈ�� ��� ��ȯ�ߴٸ� ��ȯ ����
                if (totalSpawnedCount >= totalSpawnCount)
                {
                    isSpawning = false;
                }
            }
        }
    }

    // ������Ʈ�� �ı��Ǿ��� �� ȣ��� �޼���
    private void OnObjectDestroyed(GameObject destroyedObject)
    {
        // ����Ʈ���� ����
        spawnedObjects.Remove(destroyedObject);

        // ���� ��ȯ�� ������Ʈ �� ����
        currentSpawnedCount--;
    }

    // ��ȯ ���� �޼��� (�ʿ� ��)
    public void StopSpawning()
    {
        isSpawning = false;
    }
}
