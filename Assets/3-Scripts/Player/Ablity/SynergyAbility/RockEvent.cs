using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "SynergyAbilities/RockEvent")]
public class RockEvent : SynergyAbility
{
    public GameObject effectPrefab;
    public float buffDuration = 10f;

    private Player playerInstance;
    private Coroutine buffCoroutine;

    public override void Apply(Player player)
    {
        playerInstance = player;
        if (buffCoroutine != null)
        {
            playerInstance.StopCoroutine(buffCoroutine);
        }
        buffCoroutine = playerInstance.StartCoroutine(BuffCycle());
    }

    private IEnumerator BuffCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(cooldownDuration);

            // 공격력 버프
            ActivateBuff(playerInstance, BuffType.Attack);
            yield return new WaitForSeconds(buffDuration);

            // 공격 속도 버프
            ActivateBuff(playerInstance, BuffType.AttackSpeed);
            yield return new WaitForSeconds(buffDuration);

            // 이동 속도 버프
            ActivateBuff(playerInstance, BuffType.MoveSpeed);
            yield return new WaitForSeconds(buffDuration);
        }
    }

    private void ActivateBuff(Player player, BuffType buffType)
    {
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, player.transform.position, Quaternion.identity);
            effect.transform.SetParent(player.transform);
            Destroy(effect, buffDuration);
        }

        switch (buffType)
        {
            case BuffType.Attack:
                player.stat.currentPlayerDamage = (int)(player.stat.currentPlayerDamage * 1.5f);
                player.StartCoroutine(RemoveBuffAfterDuration(() => player.stat.currentPlayerDamage = (int)(player.stat.currentPlayerDamage / 1.5f), buffDuration));
                break;

            case BuffType.AttackSpeed:
                player.stat.currentShootCooldown *= 0.5f;
                player.StartCoroutine(RemoveBuffAfterDuration(() => player.stat.currentShootCooldown *= 2f, buffDuration));
                break;

            case BuffType.MoveSpeed:
                player.stat.currentPlayerSpeed *= 1.5f;
                player.StartCoroutine(RemoveBuffAfterDuration(() => player.stat.currentPlayerSpeed /= 1.5f, buffDuration));
                break;
        }
    }

    private IEnumerator RemoveBuffAfterDuration(System.Action removeBuffAction, float duration)
    {
        yield return new WaitForSeconds(duration);
        removeBuffAction();
    }
}

public enum BuffType
{
    Attack,
    AttackSpeed,
    MoveSpeed
}
