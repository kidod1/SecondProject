using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "SynergyAbilities/EmergencyHealing")]
public class EmergencyHealing : SynergyAbility
{
    public GameObject healingDronePrefab; // 치유 드론 프리팹
    public float cooldownDuration = 20f; // 쿨타임 20초
    private Player playerInstance;
    private Coroutine cooldownCoroutine;

    public override void Apply(Player player)
    {
        playerInstance = player;

        // 플레이어의 OnTakeDamage 이벤트에 리스너 추가
        player.OnTakeDamage.AddListener(CheckAndHealPlayer);
    }

    private void CheckAndHealPlayer()
    {
        // 체력이 10% 이하로 떨어졌는지 확인
        if (playerInstance.GetCurrentHP() <= playerInstance.stat.maxHP * 0.1f)
        {
            // 쿨타임이 아닌 상태에서만 회복
            if (cooldownCoroutine == null)
            {
                HealPlayer();
            }
        }
    }

    private void HealPlayer()
    {
        // 체력의 30%를 회복
        int healAmount = Mathf.RoundToInt(playerInstance.stat.maxHP * 0.3f);
        playerInstance.Heal(healAmount);

        // 치유 드론 비주얼 효과
        if (healingDronePrefab != null)
        {
            GameObject healingDrone = Instantiate(healingDronePrefab, playerInstance.transform.position, Quaternion.identity);
            healingDrone.transform.SetParent(playerInstance.transform);
            Destroy(healingDrone, 2f); // 치유 드론은 2초 후에 사라짐
        }

        // 쿨타임 시작
        cooldownCoroutine = playerInstance.StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldownDuration);
        cooldownCoroutine = null; // 쿨타임이 끝나면 초기화
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        // 리스너 제거
        if (playerInstance != null)
        {
            playerInstance.OnTakeDamage.RemoveListener(CheckAndHealPlayer);
        }

        // 쿨타임 중단
        if (cooldownCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
    }
}
