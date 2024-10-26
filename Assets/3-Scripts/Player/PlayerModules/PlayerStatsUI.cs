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
        // ������ Player ��ü�� ã�� �Ҵ��մϴ�.
        player = FindObjectOfType<Player>();

        if (player == null)
        {
            Debug.LogError("PlayerStatsUI: Player ��ü�� ã�� �� �����ϴ�.");
            return;
        }

        // �ʱ� ���� UI�� ������Ʈ�մϴ�.
        UpdateStatsUI();

        // �÷��̾��� ������ ����� ������ UI�� ������Ʈ�ϱ� ���� �̺�Ʈ�� �����մϴ�.
        SubscribeToPlayerEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayerEvents();
    }

    /// <summary>
    /// �÷��̾��� ���� ���� �̺�Ʈ�� �����մϴ�.
    /// </summary>
    private void SubscribeToPlayerEvents()
    {
        if (player != null && player.stat != null)
        {
            player.stat.OnStatsChanged += UpdateStatsUI;
        }
    }

    /// <summary>
    /// �÷��̾��� ���� ���� �̺�Ʈ ������ �����մϴ�.
    /// </summary>
    private void UnsubscribeFromPlayerEvents()
    {
        if (player != null && player.stat != null)
        {
            player.stat.OnStatsChanged -= UpdateStatsUI;
        }
    }

    /// <summary>
    /// �÷��̾��� ������ UI�� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateStatsUI()
    {
        if (player == null || player.stat == null)
            return;

        if (playerDamageText != null)
            playerDamageText.text = $"���ݷ�: {player.stat.buffedPlayerDamage}";

        if (playerSpeedText != null)
            playerSpeedText.text = $"�ӵ�: {player.stat.currentPlayerSpeed:F2}";

        if (playerDefenseText != null)
            playerDefenseText.text = $"����: {player.stat.currentDefense}";

        if (playerShootCooldownText != null)
            playerShootCooldownText.text = $"���� �ӵ�: {player.stat.currentShootCooldown:F2}";
    }
}
