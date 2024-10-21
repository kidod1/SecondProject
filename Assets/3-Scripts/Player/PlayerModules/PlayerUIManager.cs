using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System.Collections;

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
    private GameObject bossHealthUIPanel; // ���� ü�� UI �г�
    [SerializeField]
    private TMP_Text bossHealthText; // ���� ü�� ��ġ �ؽ�Ʈ
    [SerializeField]
    private RectTransform bossHealthBarMaskRect;  // ���� ü�¹� ����ũ ������ �� RectTransform
    [SerializeField]
    private RectTransform bossHealthBarFullRect;  // ���� ü�¹� ��ü ũ�� RectTransform

    private DepthOfField depthOfField;

    private int maxHP;
    private Player player;
    private Color originalHealthTextColor = Color.white;

    private float fullHealthBarWidth;
    private float fullExperienceBarWidth;
    private PlayerAbilityManager abilityManager;

    private int bossMaxHealth; // ������ �ִ� ü�� ����

    // Coroutine references to prevent multiple coroutines running simultaneously
    private Coroutine playerHealthCoroutine;
    private Coroutine bossHealthCoroutine;

    private void Awake()
    {
        // ���� ü�� UI �г��� ��Ȱ��ȭ ���·� �ʱ�ȭ
        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: bossHealthUIPanel�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

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

    /// <summary>
    /// �÷��̾��� ü�� UI�� ������Ʈ�մϴ�.
    /// </summary>
    public void UpdateHealthUI()
    {
        if (player == null || healthBarMaskRect == null || healthBarFullRect == null)
        {
            Debug.LogError("PlayerUIManager: �ʿ��� ��Ұ� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        float healthPercentage = (float)player.GetCurrentHP() / maxHP;
        healthPercentage = Mathf.Clamp01(healthPercentage);

        // ü�¿� ����� ����ũ�� �ʺ� ���� (���� ����)
        float newMaskWidth = fullHealthBarWidth * healthPercentage;


        // ���� Coroutine�� ���� ���̸� ����
        if (playerHealthCoroutine != null)
        {
            StopCoroutine(playerHealthCoroutine);
        }

        // �÷��̾� ü�¹� �ִϸ��̼� ����
        playerHealthCoroutine = StartCoroutine(AnimateHealthBar(healthBarMaskRect, newMaskWidth, 0.5f)); // �ִϸ��̼� �ð� 0.5��

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

    /// <summary>
    /// ����ġ UI�� ������Ʈ�մϴ�. (�Ű����� ���� ȣ��)
    /// </summary>
    public void UpdateExperienceUIWithoutParam()
    {
        UpdateExperienceUI();
    }

    /// <summary>
    /// ����ġ UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="gainedExperience">ȹ���� ����ġ (���� ����)</param>
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

    /// <summary>
    /// ���� �ؽ�Ʈ�� ������Ʈ�մϴ�.
    /// </summary>
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

    /// <summary>
    /// ��ȭ UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="currentCurrency">���� ��ȭ �ݾ�</param>
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

    /// <summary>
    /// �÷��̾� ��� �� ȣ��Ǵ� �޼���
    /// </summary>
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
    /// ������ �ִ� ü���� �ʱ�ȭ�ϰ� ���� ü�� UI �г��� Ȱ��ȭ�մϴ�.
    /// </summary>
    /// <param name="maxHealth">������ �ִ� ü��</param>
    public void InitializeBossHealth(int maxHealth)
    {
        bossMaxHealth = maxHealth; // ������ �ִ� ü�� ����

        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(true); // ���� ü�� UI �г� Ȱ��ȭ
            Debug.Log($"Boss Health UI Panel Ȱ��ȭ��.");
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: bossHealthUIPanel�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (bossHealthBarMaskRect != null && bossHealthBarFullRect != null)
        {
            // ���� ü�¹� ����ũ�� �ʺ� �ִ� ü�¿� �°� ����
            bossHealthBarMaskRect.sizeDelta = new Vector2(bossHealthBarFullRect.rect.width, bossHealthBarMaskRect.sizeDelta.y);
            Debug.Log($"Initialized Boss Health: Max Health = {maxHealth}, Mask Width = {bossHealthBarFullRect.rect.width}");
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: bossHealthBarMaskRect �Ǵ� bossHealthBarFullRect�� �Ҵ���� �ʾҽ��ϴ�.");
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
        if (bossHealthBarMaskRect != null && bossHealthBarFullRect != null)
        {
            float healthPercentage = (float)currentHealth / bossMaxHealth;
            healthPercentage = Mathf.Clamp01(healthPercentage);
            float targetWidth = bossHealthBarFullRect.rect.width * healthPercentage;

            Debug.Log($"Updating Boss Health: Current HP = {currentHealth}, Health Percentage = {healthPercentage}, Target Width = {targetWidth}");

            // ���� Coroutine�� ���� ���̸� ����
            if (bossHealthCoroutine != null)
            {
                StopCoroutine(bossHealthCoroutine);
                Debug.Log("Stopped existing bossHealthCoroutine.");
            }

            // ���� ü�¹� �ִϸ��̼� ����
            bossHealthCoroutine = StartCoroutine(AnimateHealthBar(bossHealthBarMaskRect, targetWidth, 0.5f)); // �ִϸ��̼� �ð� 0.5��
            Debug.Log("Started new bossHealthCoroutine.");
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: bossHealthBarMaskRect �Ǵ� bossHealthBarFullRect�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (bossHealthText != null)
        {
            bossHealthText.text = $"{currentHealth}/{bossMaxHealth} HP";
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: bossHealthText�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // ���� ü���� 0�� �� UI �г��� ��Ȱ��ȭ�Ϸ��� �Ʒ� �ּ��� �����ϼ���.
        /*
        if (currentHealth <= 0 && bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(false);
            Debug.Log("Boss Health UI Panel ��Ȱ��ȭ��.");
        }
        */
    }

    /// <summary>
    /// ü�¹��� ũ�⸦ �ε巴�� �ִϸ��̼��ϴ� Coroutine
    /// </summary>
    /// <param name="rect">�ִϸ��̼��� RectTransform</param>
    /// <param name="targetWidth">��ǥ �ʺ�</param>
    /// <param name="duration">�ִϸ��̼� �ð�</param>
    /// <returns></returns>
    private IEnumerator AnimateHealthBar(RectTransform rect, float targetWidth, float duration)
    {
        float initialWidth = rect.sizeDelta.x;
        Debug.Log($"Animating Health Bar from {initialWidth} to {targetWidth} over {duration} seconds.");
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
    /// ���� ��� �� ���� ü�� UI �г��� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public void HideBossHealthUI()
    {
        if (bossHealthUIPanel != null)
        {
            bossHealthUIPanel.SetActive(false);
            Debug.Log("Boss Health UI Panel ��Ȱ��ȭ��.");
        }
        else
        {
            Debug.LogWarning("PlayerUIManager: bossHealthUIPanel�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }
}
