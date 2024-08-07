using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/RockEvent")]
public class RockEvent : SynergyAbility
{
    public GameObject effectPrefab;
    public float buffDuration = 10f;
    public float cooldownDuration = 20f;

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

            // ���ݷ� ����
            ActivateBuff(playerInstance, BuffType.Attack);
            yield return new WaitForSeconds(buffDuration);

            // ���� �ӵ� ����
            ActivateBuff(playerInstance, BuffType.AttackSpeed);
            yield return new WaitForSeconds(buffDuration);

            // �̵� �ӵ� ����
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
                player.stat.playerDamage = (int)(player.stat.playerDamage * 1.5f);
                player.StartCoroutine(RemoveBuffAfterDuration(() => player.stat.playerDamage = (int)(player.stat.playerDamage / 1.5f), buffDuration));
                break;

            case BuffType.AttackSpeed:
                player.stat.ShotCooldown *= 0.5f;
                player.StartCoroutine(RemoveBuffAfterDuration(() => player.stat.ShotCooldown *= 2f, buffDuration));
                break;

            case BuffType.MoveSpeed:
                player.stat.playerSpeed *= 1.5f;
                player.StartCoroutine(RemoveBuffAfterDuration(() => player.stat.playerSpeed /= 1.5f, buffDuration));
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
