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
    private TMP_Text levelText;
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


    private DepthOfField depthOfField;

    private int maxHP;
    private Player player;
    private Color originalHealthTextColor = Color.white;

    private float fullHealthBarWidth;
    private float fullExperienceBarWidth;

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

        // OnGainExperience�� UnityEvent<int> Ÿ���̹Ƿ�, �Ű������� �޴� �޼���� �����ؾ� �մϴ�.
        player.OnGainExperience.AddListener(UpdateExperienceUI);
        player.OnLevelUp.AddListener(UpdateExperienceUIWithoutParam);
        player.OnPlayerDeath.AddListener(OnPlayerDeath);

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
        if (levelText != null)
        {
            levelText.text = player.stat.currentLevel.ToString();
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
        // ����� UI ���ε带 ����� �޼���. �̿�
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

}
