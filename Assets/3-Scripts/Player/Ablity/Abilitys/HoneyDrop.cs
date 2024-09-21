using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/HoneyDrop")]
public class HoneyDrop : Ability
{
    [Header("Ability Parameters")]
    public int healthRecoveryAmount = 20;
    [Range(0f, 1f)]
    public float honeyDropChance = 0.3f;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    public void OnMonsterDeath(Monster monster)
    {
        if (Random.value <= honeyDropChance)
        {
            SpawnHoney(monster.transform.position);
        }
    }

    private void SpawnHoney(Vector3 position)
    {
        GameObject honeyPrefab = Resources.Load<GameObject>("HoneyItemPrefab");

        if (honeyPrefab != null)
        {
            GameObject honeyItem = Instantiate(honeyPrefab, position, Quaternion.identity);
            HoneyItem honeyScript = honeyItem.GetComponent<HoneyItem>();
            if (honeyScript != null)
            {
                honeyScript.ItemData.healAmount = healthRecoveryAmount;
            }
        }
        else
        {
            Debug.LogWarning("벌꿀 아이템 프리팹을 찾을 수 없습니다: HoneyItemPrefab");
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
    }

    protected override int GetNextLevelIncrease()
    {
        return 1;
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            healthRecoveryAmount += 10;
        }
    }
}
