using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text levelText;
    [SerializeField]
    private Scrollbar experienceScrollbar;
    [SerializeField]
    private RectTransform maskRectTransform;
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private Image healthFillImage;
    [SerializeField]
    private TMP_Text currencyText;
    [SerializeField]
    private GameObject deathPanel;

    private int maxHP;
    private Player player;
    private Color originalHealthTextColor = Color.white;

    public void Initialize(Player player)
    {
        if (player == null)
        {
            Debug.LogError("PlayerUIManager: 플레이어가 할당되지 않았습니다.");
            return;
        }

        this.player = player;

        if (player.OnTakeDamage == null || player.OnLevelUp == null || player.OnPlayerDeath == null)
        {
            Debug.LogError("PlayerUIManager: 플레이어 이벤트가 초기화되지 않았습니다.");
            return;
        }

        player.OnTakeDamage.AddListener(UpdateHealthUI);
        player.OnLevelUp.AddListener(UpdateExperienceUI);
        player.OnPlayerDeath.AddListener(OnPlayerDeath);

        maxHP = player.stat.currentMaxHP;

        if (healthText != null)
        {
            healthText.color = originalHealthTextColor;
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: 체력 텍스트가 할당되지 않았습니다.");
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (player == null)
        {
            Debug.LogError("PlayerUIManager: 플레이어가 할당되지 않았습니다.");
            return;
        }

        if (player.stat == null)
        {
            Debug.LogError("PlayerUIManager: 플레이어의 스탯이 초기화되지 않았습니다.");
            return;
        }

        maxHP = player.stat.currentMaxHP;
        UpdateHealthUI();
        UpdateExperienceUI();
        UpdateCurrencyUI(player.stat.currentCurrency);
    }

    public void UpdateHealthUI()
    {
        if (player == null)
        {
            Debug.LogError("PlayerUIManager: 플레이어가 할당되지 않았습니다.");
            return;
        }

        float healthPercentage = (float)player.GetCurrentHP() / maxHP;

        if (maskRectTransform != null)
        {
            float rightPadding = (1 - healthPercentage) * 120 * 4;
            maskRectTransform.offsetMax = new Vector2(-rightPadding, maskRectTransform.offsetMax.y);
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: Mask RectTransform이 할당되지 않았습니다.");
        }

        if (healthText != null)
        {
            healthText.text = $"{healthPercentage * 100:F0}%";
            healthText.color = Color.Lerp(Color.red, originalHealthTextColor, healthPercentage);
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: 체력 텍스트가 할당되지 않았습니다.");
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercentage;
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: 체력 이미지가 할당되지 않았습니다.");
        }
    }

    public void UpdateExperienceUI()
    {
        if (experienceScrollbar == null || levelText == null)
        {
            Debug.LogError("PlayerUIManager: 경험치 UI 요소가 할당되지 않았습니다.");
            return;
        }

        if (player == null || player.stat == null)
        {
            Debug.LogError("PlayerUIManager: 플레이어 또는 플레이어의 스탯이 할당되지 않았습니다.");
            return;
        }

        if (player.stat.currentLevel < player.stat.experienceThresholds.Length)
        {
            levelText.text = "" +player.stat.currentLevel;

            float expRatio = (float)player.stat.currentExperience / player.stat.experienceThresholds[player.stat.currentLevel];
            experienceScrollbar.size = Mathf.Clamp01(expRatio);
        }
        else
        {
            Debug.LogError("PlayerUIManager: 현재 레벨이 경험치 임계값의 범위를 벗어났습니다.");
        }
    }

    public void UpdateCurrencyUI(int currentCurrency)
    {
        if (currencyText != null)
        {
            currencyText.text = "현재 재화: " + currentCurrency;
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: currencyText가 인스펙터에 할당되지 않았습니다.");
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("플레이어 사망 UI 처리");

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
<<<<<<< HEAD
        // 사망시 UI 업로드를 담당할 메서드. 미완
=======
        else
        {
            Debug.LogError("PlayerUIManager: deathPanel이 할당되지 않았습니다.");
        }
>>>>>>> main
    }
}
