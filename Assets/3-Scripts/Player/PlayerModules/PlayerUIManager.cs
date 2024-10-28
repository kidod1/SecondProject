using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Level and Experience UI")]
    [SerializeField]
    private TMP_Text levelTextPrimary;
    [SerializeField]
    private TMP_Text levelTextSecondary;
    [SerializeField]
    private Scrollbar experienceScrollbar;
    [SerializeField]
    private TMP_Text experienceText;

    [Header("Health UI")]
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private Image healthFillImage;
    [SerializeField]
    private RectTransform healthBarMaskRect;
    [SerializeField]
    private RectTransform healthBarFullRect;

    [Header("Currency UI")]
    [SerializeField]
    private TMP_Text currencyText;
    [SerializeField]
    private GameObject deathPanel;

    [Header("Experience UI")]
    [SerializeField]
    private RectTransform experienceBarMaskRect;
    [SerializeField]
    private RectTransform experienceBarFullRect;

    [Header("Post-Processing")]
    private Volume globalVolume;

    [Header("Selected Abilities UI")]
    [SerializeField]
    private Transform abilitiesContainer;
    [SerializeField]
    private GameObject abilityIconPrefab;

    [Header("Boss Health UI")]
    [SerializeField]
    private GameObject bossHealthUIPanel;
    [SerializeField]
    private TMP_Text bossHealthText;
    [SerializeField]
    private RectTransform bossHealthBarMaskRect;
    [SerializeField]
    private RectTransform bossHealthBarFullRect;

    private DepthOfField depthOfField;

    private int maxHP;
    private Player player;
    private Color originalHealthTextColor = Color.white;

    private float fullHealthBarWidth;
    private float fullExperienceBarWidth;
    private PlayerAbilityManager abilityManager;

    private int bossMaxHealth;

    private Coroutine playerHealthCoroutine;
    private Coroutine bossHealthCoroutine;

    // ������Ʈ Ǯ���� ���� ����Ʈ
    private List<GameObject> abilityIconPool = new List<GameObject>();

    private void Awake()
    {
        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(false);
        }
    }

    private void Start()
    {
        if (player == null)
        {
            player = PlayManager.I.GetPlayer();
        }
    }

    public void Initialize(Player player)
    {
        if (player == null)
        {
            return;
        }

        this.player = player;

        player.OnTakeDamage.AddListener(UpdateHealthUI);
        player.OnHeal.AddListener(UpdateHealthUI);
        player.OnGainExperience.AddListener(UpdateExperienceUI);
        player.OnLevelUp.AddListener(UpdateExperienceUIWithoutParam);
        player.OnPlayerDeath.AddListener(OnPlayerDeath);

        abilityManager = player.abilityManager;
        if (abilityManager != null)
        {
            abilityManager.OnAbilitiesChanged += UpdateSelectedAbilitiesUI;
        }

        maxHP = player.stat.currentMaxHP;

        fullHealthBarWidth = healthBarFullRect.rect.width;
        fullExperienceBarWidth = experienceBarFullRect.rect.width;

        globalVolume = FindObjectsOfType<Volume>().FirstOrDefault(v => v.isGlobal);

        if (globalVolume != null)
        {
            if (globalVolume.profile.TryGet(out depthOfField))
            {
                depthOfField.active = false;
            }
        }

        UpdateUI();
        UpdateSelectedAbilitiesUI();
    }


    private void UpdateUI()
    {
        if (player == null || player.stat == null)
        {
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
            return;
        }

        float healthPercentage = (float)player.GetCurrentHP() / maxHP;
        healthPercentage = Mathf.Clamp01(healthPercentage);

        float newMaskWidth = fullHealthBarWidth * healthPercentage;

        if (playerHealthCoroutine != null)
        {
            StopCoroutine(playerHealthCoroutine);
        }

        playerHealthCoroutine = StartCoroutine(AnimateHealthBar(healthBarMaskRect, newMaskWidth, 0.5f));

        if (healthText != null)
        {
            healthText.text = $"{healthPercentage * 100:F0}%";
            healthText.color = Color.Lerp(Color.red, originalHealthTextColor, healthPercentage);
        }

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
            return;
        }

        float experienceRatio = 0f;

        if (player.stat.currentLevel < player.stat.experienceThresholds.Length)
        {
            experienceRatio = (float)player.stat.currentExperience / player.stat.experienceThresholds[player.stat.currentLevel];
        }
        else
        {
            experienceRatio = 1f;
        }
        experienceRatio = Mathf.Clamp01(experienceRatio);
        experienceRatio *= 2;
        float newMaskWidth = fullExperienceBarWidth * experienceRatio;
        experienceBarMaskRect.sizeDelta = new Vector2(newMaskWidth, experienceBarMaskRect.sizeDelta.y);

        if (experienceText != null)
        {
            experienceText.text = $"{player.stat.currentExperience}/{(player.stat.currentLevel < player.stat.experienceThresholds.Length ? player.stat.experienceThresholds[player.stat.currentLevel].ToString() : "Max")} XP";
        }

        if (experienceScrollbar != null)
        {
            experienceScrollbar.size = experienceRatio;
        }

        UpdateLevelTexts();
    }

    private void UpdateLevelTexts()
    {
        if (levelTextPrimary != null)
        {
            levelTextPrimary.text = player.stat.currentLevel.ToString();
        }

        if (levelTextSecondary != null)
        {
            levelTextSecondary.text = player.stat.currentLevel.ToString();
        }
    }

    public void UpdateCurrencyUI(int currentCurrency)
    {
        if (currencyText != null)
        {
            currencyText.text = "" + currentCurrency;
        }
    }

    private void OnPlayerDeath()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
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
    /// �ɷ� ���� UI�� ������Ʈ�մϴ�. ������Ʈ Ǯ���� ���� ���� ����ȭ
    /// </summary>
    public void UpdateSelectedAbilitiesUI()
    {
        if (abilitiesContainer == null || abilityIconPrefab == null || abilityManager == null)
        {
            return;
        }

        // ���� �ɷ� ���� Ǯ�� �ִ� ������ �� ��
        int requiredIcons = abilityManager.abilities.Count;
        int currentPoolCount = abilityIconPool.Count;

        // �ʿ��� ��ŭ �������� Ǯ���� �������� �Ǵ� �߰� ����
        for (int i = currentPoolCount; i < requiredIcons; i++)
        {
            GameObject iconObj = Instantiate(abilityIconPrefab, abilitiesContainer);
            iconObj.SetActive(false); // �ʱ⿡�� ��Ȱ��ȭ
            abilityIconPool.Add(iconObj);
        }

        // ��� ���� ������ ��Ȱ��ȭ
        foreach (var icon in abilityIconPool)
        {
            icon.SetActive(false);
        }

        // �ʿ��� ��ŭ �������� Ȱ��ȭ�ϰ� ����
        for (int i = 0; i < requiredIcons; i++)
        {
            GameObject iconObj = abilityIconPool[i];
            iconObj.SetActive(true);
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null && abilityManager.abilities[i] != null)
            {
                iconImage.sprite = abilityManager.abilities[i].abilityIcon;
            }
            else
            {
                Debug.LogError($"PlayerUIManager: abilityManager.abilities[{i}]�� null�̰ų� Image ������Ʈ�� �����ϴ�.");
            }
        }
    }

    public void InitializeBossHealth(int maxHealth)
    {
        bossMaxHealth = maxHealth;

        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(true);
        }

        if (bossHealthBarMaskRect != null && bossHealthBarFullRect != null)
        {
            bossHealthBarMaskRect.sizeDelta = new Vector2(bossHealthBarFullRect.rect.width, bossHealthBarMaskRect.sizeDelta.y);
        }

        if (bossHealthText != null)
        {
            bossHealthText.text = $"{maxHealth}/{maxHealth} HP";
        }
    }

    public void UpdateBossHealth(int currentHealth)
    {
        if (bossHealthBarMaskRect != null && bossHealthBarFullRect != null)
        {
            float healthPercentage = (float)currentHealth / bossMaxHealth;
            healthPercentage = Mathf.Clamp01(healthPercentage);
            float targetWidth = bossHealthBarFullRect.rect.width * healthPercentage;

            if (bossHealthCoroutine != null)
            {
                StopCoroutine(bossHealthCoroutine);
            }

            bossHealthCoroutine = StartCoroutine(AnimateHealthBar(bossHealthBarMaskRect, targetWidth, 0.5f));
        }

        if (bossHealthText != null)
        {
            bossHealthText.text = $"{currentHealth}/{bossMaxHealth} HP";
        }
    }

    private IEnumerator AnimateHealthBar(RectTransform rect, float targetWidth, float duration)
    {
        float initialWidth = rect.sizeDelta.x;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float newWidth = Mathf.Lerp(initialWidth, targetWidth, elapsed / duration);
            rect.sizeDelta = new Vector2(newWidth, rect.sizeDelta.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.sizeDelta = new Vector2(targetWidth, rect.sizeDelta.y);
    }

    public void HideBossHealthUI()
    {
        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(false);
        }
    }
}
