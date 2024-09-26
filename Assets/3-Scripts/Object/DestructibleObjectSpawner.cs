using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestructibleObjectSpawner : MonoBehaviour
{
    // 소환할 DestructibleObject 프리팹
    [SerializeField]
    private GameObject[] destructiblePrefabs;

    // 소환 위치 배열
    [SerializeField]
    private Transform[] spawnLocations;

    // 소환 시간 간격 범위 (최소값, 최대값)
    [SerializeField]
    private float minSpawnInterval = 1f;
    [SerializeField]
    private float maxSpawnInterval = 5f;

    // 게임 내 최대 소환 가능한 오브젝트 수
    [SerializeField]
    private int maxSpawnedObjects = 10;

    // 총 소환할 오브젝트 수
    [SerializeField]
    private int totalSpawnCount = 20;

    private int currentSpawnedCount = 0;
    private int totalSpawnedCount = 0;

    private bool isSpawning = true;

    // 현재 소환되어 있는 오브젝트들을 추적하기 위한 리스트
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Start()
    {
        if (destructiblePrefabs.Length == 0)
        {
            Debug.LogError("DestructibleObjectSpawner: 소환할 프리팹이 지정되어 있지 않습니다.");
            isSpawning = false;
            return;
        }

        if (spawnLocations.Length == 0)
        {
            Debug.LogError("DestructibleObjectSpawner: 소환 위치가 지정되어 있지 않습니다.");
            isSpawning = false;
            return;
        }

        // 소환 코루틴 시작
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (isSpawning)
        {
            // 현재 소환된 오브젝트 수와 총 소환된 오브젝트 수를 확인하여 소환 여부 결정
            if (currentSpawnedCount < maxSpawnedObjects && totalSpawnedCount < totalSpawnCount)
            {
                // 랜덤한 시간 대기
                float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(waitTime);

                // 랜덤한 프리팹 선택
                int randomPrefabIndex = Random.Range(0, destructiblePrefabs.Length);
                GameObject prefabToSpawn = destructiblePrefabs[randomPrefabIndex];

                // 랜덤한 소환 위치 선택
                int randomLocationIndex = Random.Range(0, spawnLocations.Length);
                Transform selectedSpawnLocation = spawnLocations[randomLocationIndex];

                // 오브젝트 소환
                GameObject spawnedObject = Instantiate(prefabToSpawn, selectedSpawnLocation.position, Quaternion.identity);

                // 소환된 오브젝트 관리 리스트에 추가
                spawnedObjects.Add(spawnedObject);

                // 소환된 오브젝트 수 증가
                currentSpawnedCount++;
                totalSpawnedCount++;

                // 소환된 오브젝트의 파괴 시 이벤트 등록
                DestructibleObject destructible = spawnedObject.GetComponent<DestructibleObject>();
                if (destructible != null)
                {
                    destructible.OnDestroyed += OnObjectDestroyed;
                }
            }
            else
            {
                // 더 이상 소환할 수 없으면 대기
                yield return null;

                // 총 소환해야 할 오브젝트를 모두 소환했다면 소환 중지
                if (totalSpawnedCount >= totalSpawnCount)
                {
                    isSpawning = false;
                }
            }
        }
    }

    // 오브젝트가 파괴되었을 때 호출될 메서드
    private void OnObjectDestroyed(GameObject destroyedObject)
    {
        // 리스트에서 제거
        spawnedObjects.Remove(destroyedObject);

        // 현재 소환된 오브젝트 수 감소
        currentSpawnedCount--;
    }

    // 소환 중지 메서드 (필요 시)
    public void StopSpawning()
    {
        isSpawning = false;
    }
}
