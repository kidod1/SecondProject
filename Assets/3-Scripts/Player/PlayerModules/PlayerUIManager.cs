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
    private TMP_Text experienceText; // 경험치 텍스트 추가 (선택 사항)

    [Header("Health UI")]
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private Image healthFillImage;  // 체력바 이미지
    [SerializeField]
    private RectTransform healthBarMaskRect;  // 체력바 마스크 역할을 할 RectTransform
    [SerializeField]
    private RectTransform healthBarFullRect;  // 체력바 전체 크기 RectTransform

    [Header("Currency UI")]
    [SerializeField]
    private TMP_Text currencyText;
    [SerializeField]
    private GameObject deathPanel;

    [Header("Experience UI")]
    [SerializeField]
    private RectTransform experienceBarMaskRect;  // 경험치 바 마스크 역할을 할 RectTransform
    [SerializeField]
    private RectTransform experienceBarFullRect;  // 경험치 바 전체 크기 RectTransform

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
            Debug.LogError("PlayerUIManager: 플레이어가 할당되지 않았습니다.");
            return;
        }

        this.player = player;

        // 이벤트 리스너 등록
        player.OnTakeDamage.AddListener(UpdateHealthUI);
        player.OnHeal.AddListener(UpdateHealthUI); // 회복 시에도 업데이트

        // OnGainExperience는 UnityEvent<int> 타입이므로, 매개변수를 받는 메서드와 연결해야 합니다.
        player.OnGainExperience.AddListener(UpdateExperienceUI);
        player.OnLevelUp.AddListener(UpdateExperienceUIWithoutParam);
        player.OnPlayerDeath.AddListener(OnPlayerDeath);

        maxHP = player.stat.currentMaxHP;

        // 체력바와 경험치 바의 전체 너비 저장
        fullHealthBarWidth = healthBarFullRect.rect.width;
        fullExperienceBarWidth = experienceBarFullRect.rect.width;

        // 글로벌 볼륨을 찾아 할당
        globalVolume = FindObjectsOfType<Volume>().FirstOrDefault(v => v.isGlobal);

        if (globalVolume != null)
        {
            if (globalVolume.profile.TryGet(out depthOfField))
            {
                depthOfField.active = false;
            }
            else
            {
                Debug.LogError("PlayerUIManager: Depth of Field가 글로벌 볼륨 프로파일에 존재하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("PlayerUIManager: 글로벌 볼륨을 찾을 수 없습니다.");
        }

        // 초기 UI 업데이트
        UpdateUI();
    }


    private void UpdateUI()
    {
        if (player == null || player.stat == null)
        {
            Debug.LogError("PlayerUIManager: 플레이어가 할당되지 않았습니다.");
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
            Debug.LogError("PlayerUIManager: 필요한 요소가 할당되지 않았습니다.");
            return;
        }

        float healthPercentage = (float)player.GetCurrentHP() / maxHP;

        // 체력에 비례해 마스크의 너비 조정 (왼쪽 고정)
        float newMaskWidth = fullHealthBarWidth * healthPercentage;
        healthBarMaskRect.sizeDelta = new Vector2(newMaskWidth, healthBarMaskRect.sizeDelta.y);

        // 체력 텍스트 업데이트
        if (healthText != null)
        {
            healthText.text = $"{healthPercentage * 100:F0}%";
            healthText.color = Color.Lerp(Color.red, originalHealthTextColor, healthPercentage);
        }

        // 체력 이미지 업데이트 (필요 시)
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
            Debug.LogError("PlayerUIManager: 경험치 바에 필요한 요소가 할당되지 않았습니다.");
            return;
        }

        float experienceRatio = 0f;

        if (player.stat.currentLevel < player.stat.experienceThresholds.Length)
        {
            experienceRatio = (float)player.stat.currentExperience / player.stat.experienceThresholds[player.stat.currentLevel];
        }
        else
        {
            experienceRatio = 1f; // 최대 레벨 도달 시 경험치 바를 가득 채움
        }

        experienceRatio = Mathf.Clamp01(experienceRatio);

        // 경험치에 비례해 마스크의 너비 조정 (왼쪽 고정)
        float newMaskWidth = fullExperienceBarWidth * experienceRatio;
        experienceBarMaskRect.sizeDelta = new Vector2(newMaskWidth, experienceBarMaskRect.sizeDelta.y);

        // 경험치 텍스트 업데이트 (선택 사항)
        if (experienceText != null)
        {
            experienceText.text = $"{player.stat.currentExperience}/{(player.stat.currentLevel < player.stat.experienceThresholds.Length ? player.stat.experienceThresholds[player.stat.currentLevel].ToString() : "Max")} XP";
        }

        // 경험치 Scrollbar 업데이트 (선택 사항)
        if (experienceScrollbar != null)
        {
            experienceScrollbar.size = experienceRatio;
        }

        // 레벨 텍스트 업데이트
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
            Debug.LogWarning("PlayerUIManager: currencyText가 할당되지 않았습니다.");
        }
    }

    private void OnPlayerDeath()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
        // 사망시 UI 업로드를 담당할 메서드. 미완
        else
        {
            Debug.LogError("PlayerUIManager: deathPanel이 할당되지 않았습니다.");
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
