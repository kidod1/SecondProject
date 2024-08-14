using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/PushAway")]
public class PushAway : Ability
{
    public GameObject shockwavePrefab;
    private Player playerInstance;
    private Coroutine pushCoroutine;

    [SerializeField]
    private float[] cooldownTimes = { 20f, 15f, 15f, 10f, 10f }; // 각 레벨별 쿨타임
    [SerializeField]
    private int[] damageValues = { 10, 10, 10, 10, 20 }; // 각 레벨별 피해량
    [SerializeField]
    private float[] pushRanges = { 200f, 200f, 250f, 250f, 300f }; // 각 레벨별 범위

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (currentLevel == 0)
        {
            StartPushAway();
        }
    }

    protected override int GetNextLevelIncrease()
    {
        return currentLevel < maxLevel ? currentLevel + 1 : 0;
    }

    private void StartPushAway()
    {
        if (pushCoroutine == null && playerInstance != null)
        {
            pushCoroutine = playerInstance.StartCoroutine(PushAwayCoroutine());
        }
    }

    private IEnumerator PushAwayCoroutine()
    {
        while (true)
        {
            // currentLevel이 유효한 범위 내에 있는지 확인
            if (currentLevel > 0 && currentLevel <= cooldownTimes.Length)
            {
                yield return new WaitForSeconds(cooldownTimes[currentLevel - 1]);

                if (playerInstance != null)
                {
                    PerformPushAway();
                }
            }
            else
            {
                Debug.LogError("Current level is out of bounds for cooldownTimes array.");
                yield break;
            }
        }
    }

    private void PerformPushAway()
    {
        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot perform PushAway.");
            return;
        }

        // currentLevel이 유효한 범위 내에 있는지 확인
        if (currentLevel > 0 && currentLevel <= pushRanges.Length && currentLevel <= damageValues.Length)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(playerInstance.transform.position, pushRanges[currentLevel - 1]);

            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Monster"))
                {
                    Monster monster = hitCollider.GetComponent<Monster>();
                    if (monster != null)
                    {
                        monster.TakeDamage(damageValues[currentLevel - 1]);

                        Rigidbody2D monsterRb = monster.GetComponent<Rigidbody2D>();
                        if (monsterRb != null)
                        {
                            Vector2 pushDirection = (monster.transform.position - playerInstance.transform.position).normalized;
                            monsterRb.AddForce(pushDirection * 1000f);
                        }
                        else
                        {
                            Debug.LogWarning("Monster does not have a Rigidbody2D component. Cannot apply force.");
                        }
                    }
                }
            }

            if (shockwavePrefab != null)
            {
                GameObject shockwave = Instantiate(shockwavePrefab, playerInstance.transform.position, Quaternion.identity);
                shockwave.transform.localScale = Vector3.one * pushRanges[currentLevel - 1] / 100f;
                Destroy(shockwave, 1f);
            }
            else
            {
                Debug.LogWarning("Shockwave prefab is null. Cannot create visual effect.");
            }
        }
        else
        {
            Debug.LogError("Current level is out of bounds for pushRanges or damageValues array.");
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (pushCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(pushCoroutine);
            pushCoroutine = null;
        }
    }
}
