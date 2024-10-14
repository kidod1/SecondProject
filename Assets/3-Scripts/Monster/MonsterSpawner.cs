using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonsterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        [Tooltip("미리 배치된 몬스터 오브젝트들")]
        public List<GameObject> monstersToSpawn; // 프리팹 대신 미리 배치된 몬스터 리스트
        public float spawnInterval = 1.0f; // 몬스터 간의 스폰 간격
    }

    [System.Serializable]
    public class Wave
    {
        public List<SpawnInfo> spawnInfos;
        public Transform[] customSpawnPoints; // 웨이브별 커스텀 스폰 지점
    }

    [Header("Wave Settings")]
    public List<Wave> waves;
    public Transform[] spawnPoints; // 기본 스폰 지점
    public bool isSlothArea = false; // 슬로스 지역 여부

    [Header("UI Elements")]
    public TextMeshProUGUI waveNumberText;

    [Header("Mid-Boss Settings")]
    [Tooltip("씬에 미리 배치된 중간 보스 오브젝트")]
    public MidBoss midBossInstance; // 씬에 미리 배치된 MidBoss 오브젝트

    private int currentWaveIndex = 0;

    private void Start()
    {
        if (waves.Count > 0)
        {
            StartCoroutine(SpawnWaves());
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: 웨이브 설정이 없습니다.");
        }

        if (midBossInstance != null)
        {
            midBossInstance.gameObject.SetActive(false); // 초기에는 비활성화
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: midBossInstance가 할당되지 않았습니다.");
        }
    }

    private IEnumerator SpawnWaves()
    {
        while (currentWaveIndex < waves.Count)
        {
            Wave currentWave = waves[currentWaveIndex];
            Transform[] spawnPointsToUse = currentWave.customSpawnPoints.Length > 0 ? currentWave.customSpawnPoints : spawnPoints;

            foreach (var spawnInfo in currentWave.spawnInfos)
            {
                foreach (var monster in spawnInfo.monstersToSpawn)
                {
                    SpawnMonster(monster, spawnPointsToUse);
                    yield return new WaitForSeconds(spawnInfo.spawnInterval);
                }
            }

            yield return new WaitUntil(() => AreAllMonstersDead());

            ShowWaveNumberUI();

            currentWaveIndex++;
        }

        SpawnMidBoss();

        if (isSlothArea && midBossInstance != null)
        {
            midBossInstance.SetAttackable(true);
        }
    }

    /// <summary>
    /// 미리 배치된 몬스터를 활성화하고 스폰 지점에 배치합니다.
    /// </summary>
    /// <param name="monster">활성화할 몬스터 오브젝트</param>
    /// <param name="spawnPointsToUse">사용할 스폰 지점 배열</param>
    private void SpawnMonster(GameObject monster, Transform[] spawnPointsToUse)
    {
        if (spawnPointsToUse.Length == 0)
        {
            Debug.LogWarning("MonsterSpawner: 스폰 지점이 설정되지 않았습니다.");
            return;
        }

        // 비활성화된 몬스터를 찾아 활성화
        if (monster.activeInHierarchy)
        {
            Debug.LogWarning($"MonsterSpawner: {monster.name}은(는) 이미 활성화되어 있습니다.");
            return;
        }

        // 랜덤 스폰 지점 선택
        int spawnPointIndex = Random.Range(0, spawnPointsToUse.Length);
        Transform spawnPoint = spawnPointsToUse[spawnPointIndex];

        // 몬스터 위치 및 회전 설정
        monster.transform.position = spawnPoint.position;
        monster.transform.rotation = spawnPoint.rotation;

        // 몬스터 활성화
        monster.SetActive(true);

        Debug.Log($"MonsterSpawner: {monster.name}을(를) 스폰 지점 {spawnPoint.name}에 스폰했습니다.");
    }

    private void SpawnMidBoss()
    {
        if (midBossInstance != null)
        {
            // 미리 배치된 MidBoss의 위치 및 회전 설정 (필요 시 조정)
            // 예를 들어, 특정 위치로 이동하거나 회전을 설정할 수 있습니다.
            // midBossInstance.transform.position = midBossSpawnPoint.position;
            // midBossInstance.transform.rotation = midBossSpawnPoint.rotation;

            midBossInstance.gameObject.SetActive(true);
            Debug.Log("MonsterSpawner: 중간 보스가 활성화되었습니다.");
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: midBossInstance가 설정되지 않았습니다.");
        }
    }

    private bool AreAllMonstersDead()
    {
        // 씬에 있는 모든 몬스터를 찾습니다. (태그를 "Monster"로 가정)
        GameObject[] activeMonsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (var monster in activeMonsters)
        {
            if (monster.activeInHierarchy)
            {
                return false;
            }
        }
        return true;
    }

    private void ShowWaveNumberUI()
    {
        if (waveNumberText != null)
        {
            waveNumberText.text = $"웨이브 {currentWaveIndex} 클리어!";
            waveNumberText.alpha = 1f; // 텍스트를 완전히 불투명하게 설정
            StartCoroutine(FadeOutWaveNumber(3f)); // 3초 후에 페이드 아웃 시작
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: waveNumberText가 할당되지 않았습니다.");
        }
    }

    private IEnumerator FadeOutWaveNumber(float duration)
    {
        float elapsedTime = 0f;
        Color originalColor = waveNumberText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // 점점 투명해지게 설정
            waveNumberText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        waveNumberText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // 페이드 아웃이 끝나면 완전히 투명하게 설정
    }
}
