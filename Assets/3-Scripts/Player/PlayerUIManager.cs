using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text experienceText;
    [SerializeField]
    private TMP_Text levelText;
    [SerializeField]
    private Scrollbar experienceScrollbar;
    [SerializeField]
    private RectTransform maskRectTransform;
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private Image healthFillImage;  // HP �̹��� �ʵ� �߰�
    [SerializeField]
    private TMP_Text currencyText;
    [SerializeField]
    private GameObject deathPanel;  // ��� �� ǥ���� �г�

    private int maxHP;
    private Player player;

    public void Initialize(Player player)
    {
        this.player = player;
        player.OnTakeDamage.AddListener(UpdateHealthUI);
        player.OnLevelUp.AddListener(UpdateExperienceUI);
        player.OnPlayerDeath.AddListener(OnPlayerDeath);  // ��� �̺�Ʈ ������ �߰�
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

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercentage;
        }
    }

    public void UpdateExperienceUI()
    {
        levelText.text = "Lv. " + player.stat.currentLevel;
        experienceText.text = "EXP: " + player.stat.currentExperience + " / " + player.stat.experienceThresholds[player.stat.currentLevel];

        float expRatio = (float)player.stat.currentExperience / player.stat.experienceThresholds[player.stat.currentLevel];
        experienceScrollbar.size = expRatio;
    }

    public void UpdateCurrencyUI(int currentCurrency)
    {
        if (currencyText != null)
        {
            currencyText.text = "���� ��ȭ: " + currentCurrency;
        }
        else
        {
            Debug.LogWarning("currencyText is not assigned in the inspector.");
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("�÷��̾� ��� UI ó��");

        /* if (deathPanel != null)
         {
             deathPanel.SetActive(true); // ��� �г� Ȱ��ȭ
         }

         // ��� �� �߰� UI ������Ʈ ���� ���⿡ �߰��� �� �ֽ��ϴ�.\
        */
    }
}
