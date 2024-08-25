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

    public void Initialize(Player player)
    {
        this.player = player;
        player.OnTakeDamage.AddListener(UpdateHealthUI);
        player.OnLevelUp.AddListener(UpdateExperienceUI);
        player.OnPlayerDeath.AddListener(OnPlayerDeath);

        maxHP = player.stat.currentMaxHP; // 최대 HP를 설정
        UpdateUI(); // UI를 초기화하여 HP가 최대치로 설정되도록 함
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

        // 경험치 최대치가 올바른지 확인
        if (player.stat.currentLevel < player.stat.experienceThresholds.Length)
        {
            levelText.text = "Lv. " + player.stat.currentLevel;

            // 경험치 비율 계산
            float expRatio = (float)player.stat.currentExperience / player.stat.experienceThresholds[player.stat.currentLevel];
            experienceScrollbar.size = Mathf.Clamp01(expRatio); // 0과 1 사이의 값으로 제한
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
<<<<<<< HEAD

        // 사망시 UI 업로드를 담당할 메서드. 미완
=======
>>>>>>> main
    }
}
