using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SharkStrike")]
public class SharkStrike : Ability
{
    public GameObject sharkPrefab;  // 상어 프리팹
    public int hitThreshold = 5;  // 적중 임계값
    public float sharkSpeed = 5f;  // 상어 속도
    public float chaseDelay = 0.5f;  // 상어 추격 시작 전 대기 시간

    private Player playerInstance;
    private int hitCount = 0;

    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    // 플레이어가 적을 적중시켰을 때 호출되는 메서드
    public void OnProjectileHit(Collider2D enemy)
    {
        hitCount++;

        if (hitCount >= hitThreshold)
        {
            SpawnShark();
            hitCount = 0;
        }
    }

    // 상어를 생성하는 메서드
    private void SpawnShark()
    {
        if (sharkPrefab != null)
        {
            GameObject sharkObject = Instantiate(sharkPrefab, playerInstance.transform.position, Quaternion.identity);
            Shark sharkInstance = sharkObject.GetComponent<Shark>();

            if (sharkInstance != null)
            {
                sharkInstance.Initialize(sharkSpeed, chaseDelay);
            }
            else
            {
                Debug.LogError("Shark component is missing from the prefab.");
            }
        }
        else
        {
            Debug.LogError("Shark prefab is null. Cannot spawn shark.");
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        hitCount = 0;  // 적중 횟수 초기화
    }

    protected override int GetNextLevelIncrease()
    {
        return 1;  // 레벨 증가 시 변경 가능
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            // 레벨에 따른 상어 속도 또는 적중 임계값을 조정할 수 있음
        }
    }
}
