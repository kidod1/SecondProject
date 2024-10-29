using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

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

    [Header("Synergy Ability UI")]
    [SerializeField]
    private Image synergyAbilityIcon; // Synergy Ability Icon Image 추가
    [SerializeField]
    private Image synergyAbilityOverlay; // Synergy Ability Overlay Image 추가

    private DepthOfField depthOfField;

    private int maxHP;
    private Player player;
    private Color originalHealthTextColor = Color.white;

    private float fullHealthBarWidth;
    private float fullExperienceBarWidth;
    private PlayerAbilityManager playerAbilityManager;
    [SerializeField]
    private AbilityManager abilityManager;

    private int bossMaxHealth;

    private Coroutine playerHealthCoroutine;
    private Coroutine bossHealthCoroutine;

    // 오브젝트 풀링을 위한 리스트
    private List<GameObject> abilityIconPool = new List<GameObject>();

    private void Awake()
    {
        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(false);
        }

        // 초기 오버레이 상태 설정
        if (synergyAbilityOverlay != null)
        {
            synergyAbilityOverlay.gameObject.SetActive(false); // 초기에는 비활성화
        }
        else
        {
            Debug.LogError("PlayerUIManager: synergyAbilityOverlay가 할당되지 않았습니다.");
        }
    }

    private void Start()
    {
        if (player == null)
        {
            player = PlayManager.I.GetPlayer();
        }
        // Synergy Ability Icon 초기 상태 설정
        if (synergyAbilityIcon != null)
        {
            synergyAbilityIcon.gameObject.SetActive(false); // 초기에는 비활성화
        }
        else
        {
            Debug.LogError("PlayerUIManager: synergyAbilityIcon이 할당되지 않았습니다.");
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

        playerAbilityManager = player.abilityManager;
        if (playerAbilityManager != null)
        {
            playerAbilityManager.OnAbilitiesChanged += UpdateSelectedAbilitiesUI;
            abilityManager.OnSynergyAbilityChanged.AddListener(UpdateSynergyAbilityIcon);
        }
        else
        {
            Debug.LogError("PlayerUIManager: AbilityManager가 할당되지 않았습니다.");
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

    /// <summary>
    /// Synergy Ability가 변경될 때 UI를 업데이트하는 메서드
    /// </summary>
    /// <param name="synergyAbility">새로운 Synergy Ability</param>
    public void UpdateSynergyAbilityIcon(SynergyAbility synergyAbility)
    {
        Debug.Log("업데이트 시너지 어빌리티 아이콘");
        if (synergyAbilityIcon == null)
        {
            Debug.LogError("PlayerUIManager: synergyAbilityIcon이 할당되지 않았습니다.");
            return;
        }

        if (synergyAbility != null)
        {
            synergyAbilityIcon.sprite = synergyAbility.abilityIcon;
            synergyAbilityIcon.gameObject.SetActive(true);
        }
        else
        {
            synergyAbilityIcon.gameObject.SetActive(false);
        }
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
    /// 능력 선택 UI를 업데이트합니다. 오브젝트 풀링을 통해 성능 최적화
    /// </summary>
    public void UpdateSelectedAbilitiesUI()
    {
        if (abilitiesContainer == null || abilityIconPrefab == null || playerAbilityManager == null)
        {
            return;
        }

        // 현재 능력 수와 풀에 있는 아이콘 수 비교
        int requiredIcons = playerAbilityManager.abilities.Count;
        int currentPoolCount = abilityIconPool.Count;

        // 필요한 만큼 아이콘을 풀에서 가져오기 또는 추가 생성
        for (int i = currentPoolCount; i < requiredIcons; i++)
        {
            GameObject iconObj = Instantiate(abilityIconPrefab, abilitiesContainer);
            iconObj.SetActive(false); // 초기에는 비활성화
            abilityIconPool.Add(iconObj);
        }

        // 모든 기존 아이콘 비활성화
        foreach (var icon in abilityIconPool)
        {
            icon.SetActive(false);
        }

        // 필요한 만큼 아이콘을 활성화하고 설정
        for (int i = 0; i < requiredIcons; i++)
        {
            GameObject iconObj = abilityIconPool[i];
            iconObj.SetActive(true);
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null && playerAbilityManager.abilities[i] != null)
            {
                iconImage.sprite = playerAbilityManager.abilities[i].abilityIcon;
            }
            else
            {
                Debug.LogError($"PlayerUIManager: abilityManager.abilities[{i}]가 null이거나 Image 컴포넌트가 없습니다.");
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

    /// <summary>
    /// Synergy Ability의 쿨타임을 관리하고 오버레이 이미지를 업데이트하는 메서드
    /// </summary>
    /// <param name="ability">쿨타임을 관리할 SynergyAbility 인스턴스</param>
    public void StartSynergyAbilityCooldown(SynergyAbility ability)
    {
        if (synergyAbilityOverlay == null)
        {
            Debug.LogError("PlayerUIManager: synergyAbilityOverlay가 할당되지 않았습니다.");
            return;
        }

        synergyAbilityOverlay.fillAmount = 1f;
        synergyAbilityOverlay.gameObject.SetActive(true);

        UnityAction onCooldownComplete = () => OnSynergyAbilityCooldownComplete();

        ability.OnCooldownComplete.AddListener(onCooldownComplete);

        StartCoroutine(UpdateSynergyAbilityCooldown(ability, onCooldownComplete));
    }

    private IEnumerator UpdateSynergyAbilityCooldown(SynergyAbility ability, UnityAction onCooldownComplete)
    {
        float elapsed = 0f;
        while (!ability.IsReady)
        {
            elapsed = Time.time - ability.lastUsedTime;
            float fill = 1f - (elapsed / ability.cooldownDuration);
            fill = Mathf.Clamp01(fill);
            synergyAbilityOverlay.fillAmount = fill;
            yield return null;
        }

        ability.OnCooldownComplete.RemoveListener(onCooldownComplete);
        OnSynergyAbilityCooldownComplete();
    }

    private void OnSynergyAbilityCooldownComplete()
    {
        if (synergyAbilityOverlay != null)
        {
            synergyAbilityOverlay.gameObject.SetActive(false);
        }
    }
}
