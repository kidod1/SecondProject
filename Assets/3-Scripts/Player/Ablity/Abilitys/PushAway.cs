using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/PushAway")]
public class PushAway : Ability
{
    public GameObject shockwavePrefab;
    private Player playerInstance;
    private Coroutine pushCoroutine;

    private float[] cooldownTimes = { 20f, 15f, 15f, 10f, 10f }; // �� ������ ��Ÿ��
    private int[] damageValues = { 10, 10, 10, 10, 20 }; // �� ������ ���ط�
    private float[] pushRanges = { 200f, 200f, 250f, 250f, 300f }; // �� ������ ����

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (currentLevel == 0)
        {
            StartPushAway();
        }

        currentLevel++;
    }

    protected override int GetNextLevelIncrease()
    {
        return currentLevel + 1;
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
            if (currentLevel - 1 >= 0 && currentLevel - 1 < cooldownTimes.Length)
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
        // ���� ���� ���͵��� ã��
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(playerInstance.transform.position, pushRanges[currentLevel - 1]);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                Monster monster = hitCollider.GetComponent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damageValues[currentLevel - 1]);

                    Vector2 pushDirection = (monster.transform.position - playerInstance.transform.position).normalized;
                    monster.GetComponent<Rigidbody2D>().AddForce(pushDirection * 1000f);
                }
            }
        }

        if (shockwavePrefab != null)
        {
            GameObject shockwave = Instantiate(shockwavePrefab, playerInstance.transform.position, Quaternion.identity);
            shockwave.transform.localScale = Vector3.one * pushRanges[currentLevel - 1] / 100f;
            Destroy(shockwave, 1f);
        }
    }

    public override void Upgrade()
    {
        Apply(playerInstance);
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
