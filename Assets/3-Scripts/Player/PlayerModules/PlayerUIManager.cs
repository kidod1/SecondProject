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
    private Color originalHealthTextColor; // 원래 체력 텍스트 색상 저장

    public void Initialize(Player player)
    {
        this.player = player;
        player.OnTakeDamage.AddListener(UpdateHealthUI);
        player.OnLevelUp.AddListener(UpdateExperienceUI);
        player.OnPlayerDeath.AddListener(OnPlayerDeath);

        maxHP = player.stat.currentMaxHP;
        originalHealthTextColor = healthText.color;
        UpdateUI();
    }

    private void UpdateUI()
    {
        maxHP = player.stat.currentMaxHP;
        UpdateHealthUI();
        UpdateExperienceUI();
        UpdateCurrencyUI(player.stat.currentCurrency);
    }

    public void UpdateHealthUI()
    {
        float healthPercentage = (float)player.GetCurrentHP() / maxHP;
        float rightPadding = (1 - healthPercentage) * 240 * 4;

        maskRectTransform.offsetMax = new Vector2(-rightPadding, maskRectTransform.offsetMax.y);
        healthText.text = $"{healthPercentage * 100:F0}%";

        healthText.color = Color.Lerp(Color.red, originalHealthTextColor, healthPercentage);

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercentage;
        }
    }

    public void UpdateExperienceUI()
    {
        if (experienceScrollbar == null || levelText == null)
        {
            Debug.LogError("Experience UI elements are not assigned.");
            return;
        }

        if (player.stat.currentLevel < player.stat.experienceThresholds.Length)
        {
            levelText.text = "Lv. " + player.stat.currentLevel;

            float expRatio = (float)player.stat.currentExperience / player.stat.experienceThresholds[player.stat.currentLevel];
            experienceScrollbar.size = Mathf.Clamp01(expRatio);
        }
        else
        {
            Debug.LogError("Current level is out of bounds of experience thresholds.");
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
            Debug.LogWarning("currencyText is not assigned in the inspector.");
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("플레이어 사망 UI 처리");

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
    }
}
