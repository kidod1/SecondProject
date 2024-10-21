using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Level and Experience UI")]
    [SerializeField]
    private TMP_Text levelTextPrimary; // ���� ���� �ؽ�Ʈ (�� �ؽ�Ʈ)
    [SerializeField]
    private TMP_Text levelTextSecondary; // ���� �߰��� ���� �ؽ�Ʈ (���� �ؽ�Ʈ)
    [SerializeField]
    private Scrollbar experienceScrollbar;
    [SerializeField]
    private TMP_Text experienceText; // ����ġ �ؽ�Ʈ �߰� (���� ����)

    [Header("Health UI")]
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private Image healthFillImage;  // ü�¹� �̹���
    [SerializeField]
    private RectTransform healthBarMaskRect;  // ü�¹� ����ũ ������ �� RectTransform
    [SerializeField]
    private RectTransform healthBarFullRect;  // ü�¹� ��ü ũ�� RectTransform

    [Header("Currency UI")]
    [SerializeField]
    private TMP_Text currencyText;
    [SerializeField]
    private GameObject deathPanel;

    [Header("Experience UI")]
    [SerializeField]
    private RectTransform experienceBarMaskRect;  // ����ġ �� ����ũ ������ �� RectTransform
    [SerializeField]
    private RectTransform experienceBarFullRect;  // ����ġ �� ��ü ũ�� RectTransform

    [Header("Post-Processing")]
    private Volume globalVolume;

    [Header("Selected Abilities UI")]
    [SerializeField]
    private Transform abilitiesContainer; // �ɷ� �������� ���� �θ� Transform
    [SerializeField]
    private GameObject abilityIconPrefab; // �ɷ� ������ ������

    [Header("Boss Health UI")]
    [SerializeField]
    private Image bossHealthFillImage; // ���� ü�� ä��� �̹���
    //[SerializeField]
    //private RectTransform bossHealthBarMaskRect; // ���� ü�� �� ����ũ ������ �� RectTransform (������� ����)
    [SerializeField]
    private TMP_Text bossHealthText; // ���� ü�� ��ġ �ؽ�Ʈ

    private DepthOfField depthOfField;

    private int maxHP;
    private Player player;
    private Color originalHealthTextColor = Color.white;

    private float fullHealthBarWidth;
    private float fullExperienceBarWidth;
    private PlayerAbilityManager abilityManager;

    private int bossMaxHealth; // ������ �ִ� ü�� ����

    public void Initialize(Player player)
    {
        if (player == null)
        {
            Debug.LogError("PlayerUIManager: �÷��̾ �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        this.player = player;

        // �̺�Ʈ ������ ���
        player.OnTakeDamage.AddListener(UpdateHealthUI);
        player.OnHeal.AddListener(UpdateHealthUI); // ȸ�� �ÿ��� ������Ʈ
        player.OnGainExperience.AddListener(UpdateExperienceUI);
        player.OnLevelUp.AddListener(UpdateExperienceUIWithoutParam);
        player.OnPlayerDeath.AddListener(OnPlayerDeath);

        abilityManager = player.abilityManager;
        if (abilityManager != null)
        {
            abilityManager.OnAbilitiesChanged.AddListener(UpdateSelectedAbilitiesUI);
        }
        else
        {
            Debug.LogError("PlayerUIManager: PlayerAbilityManager�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        maxHP = player.stat.currentMaxHP;

        // ü�¹ٿ� ����ġ ���� ��ü �ʺ� ����
        fullHealthBarWidth = healthBarFullRect.rect.width;
        fullExperienceBarWidth = experienceBarFullRect.rect.width;

        // �۷ι� ������ ã�� �Ҵ�
        globalVolume = FindObjectsOfType<Volume>().FirstOrDefault(v => v.isGlobal);

        if (globalVolume != null)
        {
            if (globalVolume.profile.TryGet(out depthOfField))
            {
                depthOfField.active = false;
            }
            else
            {
                Debug.LogError("PlayerUIManager: Depth of Field�� �۷ι� ���� �������Ͽ� �������� �ʽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogError("PlayerUIManager: �۷ι� ������ ã�� �� �����ϴ�.");
        }

        // �ʱ� UI ������Ʈ
        UpdateUI();
        UpdateSelectedAbilitiesUI(); // ���õ� �ɷ� UI �ʱ�ȭ
    }

    private void UpdateUI()
    {
        if (player == null || player.stat == null)
        {
            Debug.LogError("PlayerUIManager: �÷��̾ �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        maxHP = player.stat.currentMaxHP;
        UpdateHealthUI();
        UpdateExperienceUI();
        UpdateCurrencyUI(player.stat.currentCurrency);
    }

    public void UpdateHealthUI()
    {
        if (player == null || healthBarMaskRect == null || healthBarFullRect == null)
        {
            Debug.LogError("PlayerUIManager: �ʿ��� ��Ұ� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        float healthPercentage = (float)player.GetCurrentHP() / maxHP;

        // ü�¿� ����� ����ũ�� �ʺ� ���� (���� ����)
        float newMaskWidth = fullHealthBarWidth * healthPercentage;
        healthBarMaskRect.sizeDelta = new Vector2(newMaskWidth, healthBarMaskRect.sizeDelta.y);

        // ü�� �ؽ�Ʈ ������Ʈ
        if (healthText != null)
        {
            healthText.text = $"{healthPercentage * 100:F0}%";
            healthText.color = Color.Lerp(Color.red, originalHealthTextColor, healthPercentage);
        }

        // ü�� �̹��� ������Ʈ (�ʿ� ��)
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercentage;
        }
    }

    public void UpdateExperienceUIWithoutParam()
    {
        UpdateExperienceUI();
    }

    public void UpdateExperienceUI(int gainedExperience = 0)
    {
        if (player == null || experienceBarMaskRect == null || experienceBarFullRect == null)
        {
            Debug.LogError("PlayerUIManager: ����ġ �ٿ� �ʿ��� ��Ұ� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        float experienceRatio = 0f;

        if (player.stat.currentLevel < player.stat.experienceThresholds.Length)
        {
            experienceRatio = (float)player.stat.currentExperience / player.stat.experienceThresholds[player.stat.currentLevel];
        }
        else
        {
            experienceRatio = 1f; // �ִ� ���� ���� �� ����ġ �ٸ� ���� ä��
        }

        experienceRatio = Mathf.Clamp01(experienceRatio);

        // ����ġ�� ����� ����ũ�� �ʺ� ���� (���� ����)
        float newMaskWidth = fullExperienceBarWidth * experienceRatio;
        experienceBarMaskRect.sizeDelta = new Vector2(newMaskWidth, experienceBarMaskRect.sizeDelta.y);

        // ����ġ �ؽ�Ʈ ������Ʈ (���� ����)
        if (experienceText != null)
        {
            experienceText.text = $"{player.stat.currentExperience}/{(player.stat.currentLevel < player.stat.experienceThresholds.Length ? player.stat.experienceThresholds[player.stat.currentLevel].ToString() : "Max")} XP";
        }

        // ����ġ Scrollbar ������Ʈ (���� ����)
        if (experienceScrollbar != null)
        {
            experienceScrollbar.size = experienceRatio;
        }

        // ���� �ؽ�Ʈ ������Ʈ
        UpdateLevelTexts();
    }

    private void UpdateLevelTexts()
    {
        if (levelTextPrimary != null)
        {
            levelTextPrimary.text = player.stat.currentLevel.ToString();
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: levelTextPrimary�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (levelTextSecondary != null)
        {
            levelTextSecondary.text = player.stat.currentLevel.ToString();
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: levelTextSecondary�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    public void UpdateCurrencyUI(int currentCurrency)
    {
        if (currencyText != null)
        {
            currencyText.text = "" + currentCurrency;
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: currencyText�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    private void OnPlayerDeath()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("PlayerUIManager: deathPanel�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    public void EnableDepthOfField()
    {
        if (depthOfField != null)
        {
            depthOfField.active = true;
        }
    }

    public void DisableDepthOfField()
    {
        if (depthOfField != null)
        {
            depthOfField.active = false;
        }
    }

    /// <summary>
    /// ���õ� ��� �ɷ��� �������� UI�� ǥ���ϴ� �޼���
    /// </summary>
    public void UpdateSelectedAbilitiesUI()
    {
        if (abilitiesContainer == null || abilityIconPrefab == null || abilityManager == null)
        {
            Debug.LogError("PlayerUIManager: Abilities UI ��Ұ� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // ���� �����ܵ� ����
        foreach (Transform child in abilitiesContainer)
        {
            Destroy(child.gameObject);
        }

        // ���õ� ��� �ɷ��� ������ ����
        foreach (var ability in abilityManager.abilities)
        {
            GameObject iconObj = Instantiate(abilityIconPrefab, abilitiesContainer);
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = ability.abilityIcon;
                // �߰������� �����̳� �ٸ� ����� ���� �� �ֽ��ϴ�.
            }
            else
            {
                Debug.LogError("Ability Icon Prefab�� Image ������Ʈ�� �����ϴ�.");
            }
        }
    }

    /// <summary>
    /// ������ �ִ� ü���� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="maxHealth">������ �ִ� ü��</param>
    public void InitializeBossHealth(int maxHealth)
    {
        bossMaxHealth = maxHealth; // ������ �ִ� ü�� ����

        if (bossHealthFillImage != null)
        {
            bossHealthFillImage.fillAmount = 1f; // ü�� ���� ä���
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: bossHealthFillImage�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (bossHealthText != null)
        {
            bossHealthText.text = $"{maxHealth}/{maxHealth} HP";
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: bossHealthText�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ������ ���� ü���� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="currentHealth">������ ���� ü��</param>
    public void UpdateBossHealth(int currentHealth)
    {
        if (bossHealthFillImage != null)
        {
            float healthPercentage = (float)currentHealth / bossMaxHealth;
            bossHealthFillImage.fillAmount = Mathf.Clamp01(healthPercentage); // ü�� ������ ���� fillAmount ����

        }
        else
        {
            Debug.LogWarning("PlayerUIManager: bossHealthFillImage�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (bossHealthText != null)
        {
            bossHealthText.text = $"{currentHealth}/{bossMaxHealth} HP";
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: bossHealthText�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }
}
