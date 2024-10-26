using UnityEngine;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Player Stats Texts")]
    [SerializeField]
    private TMP_Text playerDamageText;
    [SerializeField]
    private TMP_Text playerSpeedText;
    [SerializeField]
    private TMP_Text playerDefenseText;
    [SerializeField]
    private TMP_Text playerShootCooldownText;

    private Player player;

    private void Start()
    {
        // 씬에서 Player 객체를 찾아 할당합니다.
        player = FindObjectOfType<Player>();

        if (player == null)
        {
            Debug.LogError("PlayerStatsUI: Player 객체를 찾을 수 없습니다.");
            return;
        }

        // 초기 스탯 UI를 업데이트합니다.
        UpdateStatsUI();

        // 플레이어의 스탯이 변경될 때마다 UI를 업데이트하기 위해 이벤트를 구독합니다.
        SubscribeToPlayerEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayerEvents();
    }

    /// <summary>
    /// 플레이어의 스탯 변경 이벤트에 구독합니다.
    /// </summary>
    private void SubscribeToPlayerEvents()
    {
        if (player != null && player.stat != null)
        {
            player.stat.OnStatsChanged += UpdateStatsUI;
        }
    }

    /// <summary>
    /// 플레이어의 스탯 변경 이벤트 구독을 해제합니다.
    /// </summary>
    private void UnsubscribeFromPlayerEvents()
    {
        if (player != null && player.stat != null)
        {
            player.stat.OnStatsChanged -= UpdateStatsUI;
        }
    }

    /// <summary>
    /// 플레이어의 스탯을 UI에 업데이트합니다.
    /// </summary>
    private void UpdateStatsUI()
    {
        if (player == null || player.stat == null)
            return;

        if (playerDamageText != null)
            playerDamageText.text = $"공격력: {player.stat.buffedPlayerDamage}";

        if (playerSpeedText != null)
            playerSpeedText.text = $"속도: {player.stat.currentPlayerSpeed:F2}";

        if (playerDefenseText != null)
            playerDefenseText.text = $"방어력: {player.stat.currentDefense}";

        if (playerShootCooldownText != null)
            playerShootCooldownText.text = $"공격 속도: {player.stat.currentShootCooldown:F2}";
    }
}
