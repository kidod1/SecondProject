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
    [Header("���� �� ����ġ UI")]
    [Tooltip("�� ���� �ؽ�Ʈ")]
    [SerializeField]
    private TMP_Text levelTextPrimary;

    [Tooltip("���� ���� �ؽ�Ʈ")]
    [SerializeField]
    private TMP_Text levelTextSecondary;

    [Tooltip("����ġ ��ũ�ѹ�")]
    [SerializeField]
    private Scrollbar experienceScrollbar;

    [Tooltip("����ġ �ؽ�Ʈ")]
    [SerializeField]
    private TMP_Text experienceText;

    [Header("ü�� UI")]
    [Tooltip("ü�� �ؽ�Ʈ")]
    [SerializeField]
    private TMP_Text healthText;

    [Tooltip("ü�� ä��� �̹���")]
    [SerializeField]
    private Image healthFillImage;

    [Tooltip("ü�� �� ����ũ RectTransform")]
    [SerializeField]
    private RectTransform healthBarMaskRect;

    [Tooltip("ü�� �� ��ü RectTransform")]
    [SerializeField]
    private RectTransform healthBarFullRect;

    [Header("ȭ�� UI")]
    [Tooltip("ȭ�� �ؽ�Ʈ")]
    [SerializeField]
    private TMP_Text currencyText;

    [Tooltip("��� �г�")]
    [SerializeField]
    private GameObject deathPanel;

    [Header("����ġ UI")]
    [Tooltip("����ġ �� ����ũ RectTransform")]
    [SerializeField]
    private RectTransform experienceBarMaskRect;

    [Tooltip("����ġ �� ��ü RectTransform")]
    [SerializeField]
    private RectTransform experienceBarFullRect;

    [Header("����Ʈ ���μ���")]
    private Volume globalVolume;

    [Header("���õ� �ɷ� UI")]
    [Tooltip("�ɷ� �����̳� Transform")]
    [SerializeField]
    private Transform abilitiesContainer;

    [Tooltip("�ɷ� ������ ������")]
    [SerializeField]
    private GameObject abilityIconPrefab;

    [Header("���� ü�� UI")]
    [Tooltip("���� ü�� UI �г�")]
    [SerializeField]
    private GameObject bossHealthUIPanel;

    [Tooltip("���� ü�� �ؽ�Ʈ")]
    [SerializeField]
    private TMP_Text bossHealthText;

    [Tooltip("���� ü�� �� ����ũ RectTransform")]
    [SerializeField]
    private RectTransform bossHealthBarMaskRect;

    [Tooltip("���� ü�� �� ��ü RectTransform")]
    [SerializeField]
    private RectTransform bossHealthBarFullRect;

    [Header("�ó��� �ɷ� UI")]
    [Tooltip("�ó��� �ɷ� ������ �̹���")]
    [SerializeField]
    private Image synergyAbilityIcon;

    private DepthOfField depthOfField;

    private int maxHP;
    private Player player;
    private Color originalHealthTextColor = Color.white;

    private float fullHealthBarWidth;
    private float fullExperienceBarWidth;
    private PlayerAbilityManager playerAbilityManager;

    [Tooltip("�ɷ� �Ŵ���")]
    [SerializeField]
    private AbilityManager abilityManager;

    private int bossMaxHealth;

    private Coroutine playerHealthCoroutine;
    private Coroutine bossHealthCoroutine;

    private List<GameObject> abilityIconPool = new List<GameObject>();

    /// <summary>
    /// �ʱ� ������ �����մϴ�.
    /// </summary>
    private void Awake()
    {
        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(false);
        }

        else
        {
            Debug.LogError("PlayerUIManager: synergyAbilityOverlay�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �÷��̾ �ʱ�ȭ�ϰ� UI�� ������Ʈ�մϴ�.
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
            Debug.LogError("PlayerUIManager: synergyAbilityIcon�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �÷��̾ �����ϰ� �̺�Ʈ�� ����մϴ�.
    /// </summary>
    /// <param name="player">�÷��̾� ��ü</param>
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
            Debug.LogError("PlayerUIManager: AbilityManager�� �Ҵ���� �ʾҽ��ϴ�.");
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
    /// �ó��� �ɷ��� ����� �� UI �������� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="synergyAbility">���ο� �ó��� �ɷ�</param>
    public void UpdateSynergyAbilityIcon(SynergyAbility synergyAbility)
    {
        if (synergyAbilityIcon == null)
        {
            Debug.LogError("PlayerUIManager: synergyAbilityIcon�� �Ҵ���� �ʾҽ��ϴ�.");
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
    /// UI ��ҵ��� ������Ʈ�մϴ�.
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
    /// ü�� UI�� ������Ʈ�մϴ�.
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
    /// ����ġ UI�� ������Ʈ�մϴ�.
    /// </summary>
    public void UpdateExperienceUIWithoutParam()
    {
        UpdateExperienceUI();
    }

    /// <summary>
    /// ����ġ UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="gainedExperience">ȹ���� ����ġ ��</param>
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
    /// ���� �ؽ�Ʈ�� ������Ʈ�մϴ�.
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
    /// ȭ�� UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="currentCurrency">���� ȭ��</param>
    public void UpdateCurrencyUI(int currentCurrency)
    {
        if (currencyText != null)
        {
            currencyText.text = "" + currentCurrency;
        }
    }

    /// <summary>
    /// �÷��̾� ��� �� ȣ��˴ϴ�.
    /// </summary>
    private void OnPlayerDeath()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Depth of Field ȿ���� Ȱ��ȭ�մϴ�.
    /// </summary>
    public void EnableDepthOfField()
    {
        if (depthOfField != null)
        {
            depthOfField.active = true;
        }
    }

    /// <summary>
    /// Depth of Field ȿ���� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public void DisableDepthOfField()
    {
        if (depthOfField != null)
        {
            depthOfField.active = false;
        }
    }

    /// <summary>
    /// ���õ� �ɷ� UI�� ������Ʈ�մϴ�. ������Ʈ Ǯ���� ���� ������ ����ȭ�մϴ�.
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
                Debug.LogError($"PlayerUIManager: abilityManager.abilities[{i}]�� null�̰ų� Image ������Ʈ�� �����ϴ�.");
            }
        }
    }

    /// <summary>
    /// ������ ü���� �ʱ�ȭ�ϰ� UI�� Ȱ��ȭ�մϴ�.
    /// </summary>
    /// <param name="maxHealth">������ �ִ� ü��</param>
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
    /// ���� ü�� UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="currentHealth">������ ���� ü��</param>
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
    /// ü�� ���� �ִϸ��̼��� ó���մϴ�.
    /// </summary>
    /// <param name="rect">RectTransform ��ü</param>
    /// <param name="targetWidth">��ǥ �ʺ�</param>
    /// <param name="duration">�ִϸ��̼� ���� �ð�</param>
    /// <returns>�ڷ�ƾ�� IEnumerator</returns>
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
    /// ���� ü�� UI�� ����ϴ�.
    /// </summary>
    public void HideBossHealthUI()
    {
        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(false);
        }
    }

    /// <summary>
    /// �ó��� �ɷ��� ��Ÿ���� �����ϰ� �������� �̹����� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="ability">�ó��� �ɷ� �ν��Ͻ�</param>
    public void StartSynergyAbilityCooldown(SynergyAbility ability)
    {
        UnityAction onCooldownComplete = () => OnSynergyAbilityCooldownComplete();

        ability.OnCooldownComplete.AddListener(onCooldownComplete);

        StartCoroutine(UpdateSynergyAbilityCooldown(ability, onCooldownComplete));
    }

    /// <summary>
    /// �ó��� �ɷ��� ��Ÿ���� ������Ʈ�ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <param name="ability">�ó��� �ɷ� �ν��Ͻ�</param>
    /// <param name="onCooldownComplete">��Ÿ�� �Ϸ� �� ȣ��� �׼�</param>
    /// <returns>�ڷ�ƾ�� IEnumerator</returns>
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
    /// �ó��� �ɷ� ��Ÿ���� �Ϸ�Ǿ��� �� ȣ��˴ϴ�.
    /// </summary>
    private void OnSynergyAbilityCooldownComplete()
    {
    }
}
