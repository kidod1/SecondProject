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
    [Header("레벨 및 경험치 UI")]
    [Tooltip("주 레벨 텍스트")]
    [SerializeField]
    private TMP_Text levelTextPrimary;

    [Tooltip("보조 레벨 텍스트")]
    [SerializeField]
    private TMP_Text levelTextSecondary;

    [Tooltip("경험치 스크롤바")]
    [SerializeField]
    private Scrollbar experienceScrollbar;

    [Tooltip("경험치 텍스트")]
    [SerializeField]
    private TMP_Text experienceText;

    [Header("체력 UI")]
    [Tooltip("체력 텍스트")]
    [SerializeField]
    private TMP_Text healthText;

    [Tooltip("체력 채우기 이미지")]
    [SerializeField]
    private Image healthFillImage;

    [Tooltip("체력 바 마스크 RectTransform")]
    [SerializeField]
    private RectTransform healthBarMaskRect;

    [Tooltip("체력 바 전체 RectTransform")]
    [SerializeField]
    private RectTransform healthBarFullRect;

    [Header("화폐 UI")]
    [Tooltip("화폐 텍스트")]
    [SerializeField]
    private TMP_Text currencyText;

    [Tooltip("사망 패널")]
    [SerializeField]
    private GameObject deathPanel;

    [Header("경험치 UI")]
    [Tooltip("경험치 바 마스크 RectTransform")]
    [SerializeField]
    private RectTransform experienceBarMaskRect;

    [Tooltip("경험치 바 전체 RectTransform")]
    [SerializeField]
    private RectTransform experienceBarFullRect;

    [Header("포스트 프로세싱")]
    private Volume globalVolume;

    [Header("선택된 능력 UI")]
    [Tooltip("능력 컨테이너 Transform")]
    [SerializeField]
    private Transform abilitiesContainer;

    [Tooltip("능력 아이콘 프리팹")]
    [SerializeField]
    private GameObject abilityIconPrefab;

    [Header("보스 체력 UI")]
    [Tooltip("보스 체력 UI 패널")]
    [SerializeField]
    private GameObject bossHealthUIPanel;

    [Tooltip("보스 체력 텍스트")]
    [SerializeField]
    private TMP_Text bossHealthText;

    [Tooltip("보스 체력 바 마스크 RectTransform")]
    [SerializeField]
    private RectTransform bossHealthBarMaskRect;

    [Tooltip("보스 체력 바 전체 RectTransform")]
    [SerializeField]
    private RectTransform bossHealthBarFullRect;

    [Header("시너지 능력 UI")]
    [Tooltip("시너지 능력 아이콘 이미지")]
    [SerializeField]
    private Image synergyAbilityIcon;

    private DepthOfField depthOfField;

    private int maxHP;
    private Player player;
    private Color originalHealthTextColor = Color.white;

    private float fullHealthBarWidth;
    private float fullExperienceBarWidth;
    private PlayerAbilityManager playerAbilityManager;

    [Tooltip("능력 매니저")]
    [SerializeField]
    private AbilityManager abilityManager;

    private int bossMaxHealth;

    private Coroutine playerHealthCoroutine;
    private Coroutine bossHealthCoroutine;

    private List<GameObject> abilityIconPool = new List<GameObject>();

    /// <summary>
    /// 초기 설정을 수행합니다.
    /// </summary>
    private void Awake()
    {
        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(false);
        }

        else
        {
            Debug.LogError("PlayerUIManager: synergyAbilityOverlay가 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 플레이어를 초기화하고 UI를 업데이트합니다.
    /// </summary>
    private void Start()
    {
        if (player == null)
        {
            player = PlayManager.I.GetPlayer();
        }

        if (synergyAbilityIcon != null)
        {
            synergyAbilityIcon.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("PlayerUIManager: synergyAbilityIcon이 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 플레이어를 설정하고 이벤트를 등록합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
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
    /// 시너지 능력이 변경될 때 UI 아이콘을 업데이트합니다.
    /// </summary>
    /// <param name="synergyAbility">새로운 시너지 능력</param>
    public void UpdateSynergyAbilityIcon(SynergyAbility synergyAbility)
    {
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

    /// <summary>
    /// UI 요소들을 업데이트합니다.
    /// </summary>
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

    /// <summary>
    /// 체력 UI를 업데이트합니다.
    /// </summary>
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

    /// <summary>
    /// 경험치 UI를 업데이트합니다.
    /// </summary>
    public void UpdateExperienceUIWithoutParam()
    {
        UpdateExperienceUI();
    }

    /// <summary>
    /// 경험치 UI를 업데이트합니다.
    /// </summary>
    /// <param name="gainedExperience">획득한 경험치 양</param>
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

    /// <summary>
    /// 레벨 텍스트를 업데이트합니다.
    /// </summary>
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

    /// <summary>
    /// 화폐 UI를 업데이트합니다.
    /// </summary>
    /// <param name="currentCurrency">현재 화폐량</param>
    public void UpdateCurrencyUI(int currentCurrency)
    {
        if (currencyText != null)
        {
            currencyText.text = "" + currentCurrency;
        }
    }

    /// <summary>
    /// 플레이어 사망 시 호출됩니다.
    /// </summary>
    private void OnPlayerDeath()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Depth of Field 효과를 활성화합니다.
    /// </summary>
    public void EnableDepthOfField()
    {
        if (depthOfField != null)
        {
            depthOfField.active = true;
        }
    }

    /// <summary>
    /// Depth of Field 효과를 비활성화합니다.
    /// </summary>
    public void DisableDepthOfField()
    {
        if (depthOfField != null)
        {
            depthOfField.active = false;
        }
    }

    /// <summary>
    /// 선택된 능력 UI를 업데이트합니다. 오브젝트 풀링을 통해 성능을 최적화합니다.
    /// </summary>
    public void UpdateSelectedAbilitiesUI()
    {
        if (abilitiesContainer == null || abilityIconPrefab == null || playerAbilityManager == null)
        {
            return;
        }

        int requiredIcons = playerAbilityManager.abilities.Count;
        int currentPoolCount = abilityIconPool.Count;

        for (int i = currentPoolCount; i < requiredIcons; i++)
        {
            GameObject iconObj = Instantiate(abilityIconPrefab, abilitiesContainer);
            iconObj.SetActive(false);
            abilityIconPool.Add(iconObj);
        }

        foreach (var icon in abilityIconPool)
        {
            icon.SetActive(false);
        }

        for (int i = 0; i < requiredIcons; i++)
        {
            GameObject iconObj = abilityIconPool[i];
            iconObj.SetActive(true);
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null && playerAbilityManager.abilities[i] != null)
            {
                iconImage.sprite = playerAbilityManager.abilities[i].ability.abilityIcon;
            }
            else
            {
                Debug.LogError($"PlayerUIManager: abilityManager.abilities[{i}]가 null이거나 Image 컴포넌트가 없습니다.");
            }
        }
    }

    /// <summary>
    /// 보스의 체력을 초기화하고 UI를 활성화합니다.
    /// </summary>
    /// <param name="maxHealth">보스의 최대 체력</param>
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

    /// <summary>
    /// 보스 체력 UI를 업데이트합니다.
    /// </summary>
    /// <param name="currentHealth">보스의 현재 체력</param>
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

    /// <summary>
    /// 체력 바의 애니메이션을 처리합니다.
    /// </summary>
    /// <param name="rect">RectTransform 객체</param>
    /// <param name="targetWidth">목표 너비</param>
    /// <param name="duration">애니메이션 지속 시간</param>
    /// <returns>코루틴용 IEnumerator</returns>
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

    /// <summary>
    /// 보스 체력 UI를 숨깁니다.
    /// </summary>
    public void HideBossHealthUI()
    {
        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 시너지 능력의 쿨타임을 관리하고 오버레이 이미지를 업데이트합니다.
    /// </summary>
    /// <param name="ability">시너지 능력 인스턴스</param>
    public void StartSynergyAbilityCooldown(SynergyAbility ability)
    {
        UnityAction onCooldownComplete = () => OnSynergyAbilityCooldownComplete();

        ability.OnCooldownComplete.AddListener(onCooldownComplete);

        StartCoroutine(UpdateSynergyAbilityCooldown(ability, onCooldownComplete));
    }

    /// <summary>
    /// 시너지 능력의 쿨타임을 업데이트하는 코루틴입니다.
    /// </summary>
    /// <param name="ability">시너지 능력 인스턴스</param>
    /// <param name="onCooldownComplete">쿨타임 완료 시 호출될 액션</param>
    /// <returns>코루틴용 IEnumerator</returns>
    private IEnumerator UpdateSynergyAbilityCooldown(SynergyAbility ability, UnityAction onCooldownComplete)
    {
        float elapsed = 0f;
        while (!ability.IsReady)
        {
            elapsed = Time.time - ability.lastUsedTime;
            float fill = 1f - (elapsed / ability.cooldownDuration);
            fill = Mathf.Clamp01(fill);
            yield return null;
        }

        ability.OnCooldownComplete.RemoveListener(onCooldownComplete);
        OnSynergyAbilityCooldownComplete();
    }

    /// <summary>
    /// 시너지 능력 쿨타임이 완료되었을 때 호출됩니다.
    /// </summary>
    private void OnSynergyAbilityCooldownComplete()
    {
    }
}
