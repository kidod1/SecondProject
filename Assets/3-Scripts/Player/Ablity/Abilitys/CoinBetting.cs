using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/CoinBetting")]
public class CoinBetting : Ability
{
    [Tooltip("폭탄의 피해량")]
    public int damage = 50;

    [Tooltip("폭탄을 생성하는 쿨다운 시간 (초)")]
    public float cooldown = 5f;

    [Tooltip("폭탄의 지속 시간 (초)")]
    public float bombDuration = 10f;

    [Tooltip("생성할 코인 폭탄 프리팹")]
    public GameObject coinBombPrefab;

    private Player playerInstance;
    private Coroutine bombCoroutine;

    /// <summary>
    /// 능력을 플레이어에게 적용합니다.
    /// </summary>
    /// <param name="player">능력을 적용할 플레이어</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("CoinBetting Apply: player 인스턴스가 null입니다.");
            return;
        }

        playerInstance = player;

        // 코인 폭탄 생성 코루틴 시작
        if (bombCoroutine == null)
        {
            bombCoroutine = player.StartCoroutine(DropCoinBombs());
        }
    }

    /// <summary>
    /// 능력을 업그레이드합니다. 레벨이 증가할 때마다 피해량이 증가합니다.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            damage += 10; // 예시로 피해량을 10씩 증가시킴

            // 레벨 업 후 피해량 업데이트
            if (playerInstance != null)
            {
                // 현재 활성화된 코인 베팅 능력이 있다면 피해량을 업데이트
                // 필요 시 추가 로직을 구현할 수 있습니다.
            }
        }
        else
        {
            Debug.LogWarning("CoinBetting: 이미 최대 레벨에 도달했습니다.");
        }
    }

    /// <summary>
    /// 능력을 초기화합니다.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        // 코루틴 정지
        if (bombCoroutine != null)
        {
            playerInstance.StopCoroutine(bombCoroutine);
            bombCoroutine = null;
        }

        // 현재 적용된 버프 제거
        RemoveCurrentBuff();
    }

    /// <summary>
    /// 다음 레벨의 데미지 증가값을 반환합니다.
    /// </summary>
    /// <returns>다음 레벨에서의 피해량 증가량</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            return 10; // 다음 레벨에서 피해량이 10 증가하도록 설정
        }
        return 0;
    }

    /// <summary>
    /// 일정 시간마다 코인 폭탄을 떨어뜨리는 코루틴
    /// </summary>
    private IEnumerator DropCoinBombs()
    {
        while (true)
        {
            DropCoinBomb();
            yield return new WaitForSeconds(cooldown);
        }
    }

    /// <summary>
    /// 현재 플레이어 위치에 코인 폭탄을 생성합니다.
    /// </summary>
    private void DropCoinBomb()
    {
        if (coinBombPrefab == null)
        {
            Debug.LogError("CoinBetting: coinBombPrefab이 설정되지 않았습니다.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;
        GameObject coinBombInstance = Instantiate(coinBombPrefab, spawnPosition, Quaternion.identity);

        CoinBomb coinBombScript = coinBombInstance.GetComponent<CoinBomb>();
        if (coinBombScript != null)
        {
            coinBombScript.Initialize(damage, bombDuration);
        }
        else
        {
            Debug.LogError("CoinBetting: CoinBomb 스크립트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 현재 버프를 제거합니다.
    /// </summary>
    private void RemoveCurrentBuff()
    {
        // 현재 구현된 버프가 없으므로 필요 시 추가 구현
    }

    /// <summary>
    /// 능력의 설명을 반환합니다.
    /// </summary>
    /// <returns>능력 설명 문자열</returns>
    public override string GetDescription()
    {
        return $"{baseDescription}\n" +
               $"현재 레벨: {currentLevel + 1}\n" +
               $"폭탄 피해량: {damage}\n" +
               $"쿨다운: {cooldown}초\n" +
               $"폭탄 지속 시간: {bombDuration}초";
    }

}
