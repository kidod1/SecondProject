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
            Debug.LogError("PlayerUIManager: �÷��̾ �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        this.player = player;

        if (player.OnTakeDamage == null || player.OnLevelUp == null || player.OnPlayerDeath == null)
        {
            Debug.LogError("PlayerUIManager: �÷��̾� �̺�Ʈ�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
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
            Debug.LogWarning("PlayerUIManager: ü�� �ؽ�Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (player == null)
        {
            Debug.LogError("PlayerUIManager: �÷��̾ �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (player.stat == null)
        {
            Debug.LogError("PlayerUIManager: �÷��̾��� ������ �ʱ�ȭ���� �ʾҽ��ϴ�.");
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
            Debug.LogError("PlayerUIManager: �÷��̾ �Ҵ���� �ʾҽ��ϴ�.");
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
            Debug.LogWarning("PlayerUIManager: Mask RectTransform�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (healthText != null)
        {
            healthText.text = $"{healthPercentage * 100:F0}%";
            healthText.color = Color.Lerp(Color.red, originalHealthTextColor, healthPercentage);
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: ü�� �ؽ�Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercentage;
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: ü�� �̹����� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    public void UpdateExperienceUI()
    {
        if (experienceScrollbar == null || levelText == null)
        {
            Debug.LogError("PlayerUIManager: ����ġ UI ��Ұ� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (player == null || player.stat == null)
        {
            Debug.LogError("PlayerUIManager: �÷��̾� �Ǵ� �÷��̾��� ������ �Ҵ���� �ʾҽ��ϴ�.");
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
            Debug.LogError("PlayerUIManager: ���� ������ ����ġ �Ӱ谪�� ������ ������ϴ�.");
        }
    }

    public void UpdateCurrencyUI(int currentCurrency)
    {
        if (currencyText != null)
        {
            currencyText.text = "���� ��ȭ: " + currentCurrency;
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: currencyText�� �ν����Ϳ� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("�÷��̾� ��� UI ó��");

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
<<<<<<< HEAD
        // ����� UI ���ε带 ����� �޼���. �̿�
=======
        else
        {
            Debug.LogError("PlayerUIManager: deathPanel�� �Ҵ���� �ʾҽ��ϴ�.");
        }
>>>>>>> main
    }
}
