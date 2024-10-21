using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class AbilityManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private GameObject abilitySelectionPanel;
    [SerializeField]
    private GameObject synergyAbilityPanel;
    [SerializeField]
    private TMP_Text synergyAbilityNameText;
    [SerializeField]
    private TMP_Text synergyAbilityDescriptionText;
    [SerializeField]
    private Image synergyAbilityIcon;
    [SerializeField]
    private Button synergyAbilityButton;
    [SerializeField]
    private Button[] abilityButtons;
    [SerializeField]
    private TMP_Text[] abilityNameTexts;
    [SerializeField]
    private TMP_Text[] abilityDescriptionTexts;
    [SerializeField]
    private Image[] abilityIcons;
    [SerializeField]
    private Button rerollButton;
    [SerializeField]
    private PlayerAbilityManager playerAbilityManager;
    [SerializeField]
    private PlayerUIManager uiManager;
    private List<Ability> availableAbilities;
    // 카테고리별 버튼 이미지 매핑 딕셔너리
    private Dictionary<string, Sprite> categoryButtonImages = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> synergyCategoryButtonImages = new Dictionary<string, Sprite>();

    // 강조 표시 관련 변수 추가
    [SerializeField]
    private Image highlightImage; // 강조 표시 이미지
    private int currentIndex = 0; // 현재 선택된 능력의 인덱스
    private bool isAbilitySelectionActive = false; // 능력 선택 UI 활성화 여부
    private bool isSynergyAbilityActive = false; // 시너지 능력 활성화 여부

    private int abilitiesToShow = 0; // 표시되는 능력의 수 추가

    // 회전 관련 변수 추가
    private float highlightRotationSpeed = 50f; // 강조 이미지 회전 속도 (도/초)
    private float currentHighlightRotation = 0f; // 현재 회전 각도

    // 크기 조정 관련 변수 추가
    private float highlightScale = 1.1f; // 강조된 요소들의 스케일 배율

    // 이펙트 관련 변수 추가
    [SerializeField]
    private GameObject highlightEffectPrefab; // 이펙트 프리팹
    private GameObject currentHighlightEffect; // 현재 재생 중인 이펙트 인스턴스

    // Coroutine references to manage scaling animations
    private Coroutine highlightScaleCoroutine;
    private Coroutine buttonScaleCoroutine;
    private Coroutine iconScaleCoroutine;

    // 원본 크기 저장을 위한 배열
    private Vector2[] originalButtonSizes;
    private Vector2[] originalIconSizes;

    // 초기 크기 저장을 위한 배열 추가
    private Vector2[] initialButtonSizes;
    private Vector2[] initialIconSizes;

    // 새로운 변수 선언 (하이라이트 스케일 통일)
    private Vector3 originalHighlightScale;

    // 현재 시너지 능력을 저장하는 변수 추가
    private SynergyAbility currentSynergyAbility;
    // 능력 변경 시 호출할 이벤트
    public UnityEvent OnAbilitiesChanged;
    // 추가: 코루틴 참조 변수
    private Coroutine delayedUpdateHighlightCoroutine;
    private void Awake()
    {
        // 초기 크기 배열 초기화
        int numButtons = abilityButtons.Length;
        originalButtonSizes = new Vector2[numButtons];
        originalIconSizes = new Vector2[numButtons];
        initialButtonSizes = new Vector2[numButtons];
        initialIconSizes = new Vector2[numButtons];

        for (int i = 0; i < numButtons; i++)
        {
            if (abilityButtons[i] != null)
            {
                RectTransform buttonRect = abilityButtons[i].GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    originalButtonSizes[i] = buttonRect.sizeDelta;
                    initialButtonSizes[i] = buttonRect.sizeDelta;
                }
                else
                {
                    Debug.LogError($"AbilityManager: abilityButtons[{i}]에 RectTransform이 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"AbilityManager: abilityButtons[{i}]가 할당되지 않았습니다.");
            }

            if (abilityIcons[i] != null)
            {
                RectTransform iconRect = abilityIcons[i].GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    originalIconSizes[i] = iconRect.sizeDelta;
                    initialIconSizes[i] = iconRect.sizeDelta;
                }
                else
                {
                    Debug.LogError($"AbilityManager: abilityIcons[{i}]에 RectTransform이 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"AbilityManager: abilityIcons[{i}]가 할당되지 않았습니다.");
            }
        }

        // 초기 하이라이트 이미지 스케일 저장
        if (highlightImage != null)
        {
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();
            if (highlightRect != null)
            {
                originalHighlightScale = highlightRect.localScale;
            }
            else
            {
                Debug.LogError("AbilityManager: highlightImage에 RectTransform이 없습니다.");
            }
        }
        else
        {
            Debug.LogError("AbilityManager: highlightImage가 할당되지 않았습니다.");
        }
        LoadCategoryButtonImages();
    }

    private void LoadCategoryButtonImages()
    {
        // 카테고리 이름 배열 (Null 포함)
        string[] categories = { "Lust", "Envy", "Sloth", "Gluttony", "Greed", "Wrath", "Pride", "Null" };

        foreach (string category in categories)
        {
            // 일반 능력의 버튼 이미지 로드
            Sprite buttonImage = Resources.Load<Sprite>($"CategoryButtonImages/{category}");
            if (buttonImage != null)
            {
                categoryButtonImages.Add(category, buttonImage);
            }
            else
            {
                Debug.LogWarning($"Button image for category '{category}' not found.");
            }

            // 시너지 능력의 버튼 이미지 로드
            Sprite synergyButtonImage = Resources.Load<Sprite>($"SynergyCategoryButtonImages/{category}");
            if (synergyButtonImage != null)
            {
                synergyCategoryButtonImages.Add(category, synergyButtonImage);
            }
            else
            {
                Debug.LogWarning($"Synergy button image for category '{category}' not found.");
            }
        }
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.OnLevelUp.AddListener(ShowAbilitySelection);
        }
        else
        {
            Debug.LogError("AbilityManager: Player가 할당되지 않았습니다.");
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnLevelUp.RemoveListener(ShowAbilitySelection);
        }

        // 이펙트 정리
        if (currentHighlightEffect != null)
        {
            Destroy(currentHighlightEffect);
        }
        // 모든 활성 코루틴 중지
        if (delayedUpdateHighlightCoroutine != null)
        {
            StopCoroutine(delayedUpdateHighlightCoroutine);
            delayedUpdateHighlightCoroutine = null;
        }
    }

    private void Update()
    {
        if (isAbilitySelectionActive)
        {
            HandleKeyboardInput();
            RotateHighlightImage();
        }
    }

    public void Initialize(PlayerAbilityManager abilityManager)
    {
        if (abilityManager != null)
        {
            playerAbilityManager = abilityManager;
        }
        else
        {
            Debug.LogError("AbilityManager: PlayerAbilityManager가 할당되지 않았습니다.");
        }
    }

    public void ShowAbilitySelection()
    {
        if (playerAbilityManager == null)
        {
            Debug.LogError("AbilityManager: PlayerAbilityManager가 초기화되지 않았습니다.");
            return;
        }

        if (uiManager != null)
        {
            uiManager.EnableDepthOfField();
        }

        Time.timeScale = 0f;
        isAbilitySelectionActive = true;
        isSynergyAbilityActive = false; // 일반 능력 선택 시 시너지 비활성화

        if (abilitySelectionPanel != null)
        {
            abilitySelectionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("AbilityManager: abilitySelectionPanel이 할당되지 않았습니다.");
            return;
        }

        if (synergyAbilityPanel != null)
        {
            synergyAbilityPanel.SetActive(false);
        }

        availableAbilities = playerAbilityManager.GetAvailableAbilities();
        if (availableAbilities == null || availableAbilities.Count == 0)
        {
            Debug.LogError("AbilityManager: 사용 가능한 능력이 없습니다.");
            return;
        }

        ShuffleAbilities();

        abilitiesToShow = Mathf.Min(abilityButtons.Length, availableAbilities.Count);

        if (abilitiesToShow <= 0)
        {
            Debug.LogError("AbilityManager: abilitiesToShow가 0 이하입니다.");
            return;
        }

        // 버튼과 아이콘 초기화
        for (int i = 0; i < abilitiesToShow; i++)
        {
            if (abilityButtons[i] == null || abilityNameTexts[i] == null ||
                abilityDescriptionTexts[i] == null || abilityIcons[i] == null)
            {
                Debug.LogError($"AbilityManager: abilityButton 또는 UI 요소가 {i}번째에서 할당되지 않았습니다.");
                continue;
            }

            var ability = availableAbilities[i];

            if (categoryButtonImages.TryGetValue(ability.category, out Sprite buttonImage))
            {
                // 버튼의 이미지를 변경합니다.
                Image buttonImageComponent = abilityButtons[i].GetComponent<Image>();
                if (buttonImageComponent != null)
                {
                    buttonImageComponent.sprite = buttonImage;
                }
                else
                {
                    Debug.LogError($"Ability button at index {i} does not have an Image component.");
                }
            }
            else
            {
                Debug.LogWarning($"No button image found for category '{ability.category}'.");
            }

            abilityNameTexts[i].text = ability.abilityName;
            abilityDescriptionTexts[i].text = ability.GetDescription();
            abilityIcons[i].sprite = ability.abilityIcon;
            abilityButtons[i].onClick.RemoveAllListeners();
            int index = i;
            abilityButtons[i].onClick.AddListener(() => SelectAbility(availableAbilities[index]));
            abilityButtons[i].gameObject.SetActive(true);

            // 모든 능력의 이름과 설명을 비활성화
            abilityNameTexts[i].gameObject.SetActive(false);
            abilityDescriptionTexts[i].gameObject.SetActive(false);

            // 버튼과 아이콘의 크기를 초기 크기로 설정
            RectTransform buttonRect = abilityButtons[i].GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                // 초기 크기로 리셋
                buttonRect.sizeDelta = originalButtonSizes[i];
            }

            RectTransform iconRect = abilityIcons[i].GetComponent<RectTransform>();
            if (iconRect != null)
            {
                // 초기 크기로 리셋
                iconRect.sizeDelta = originalIconSizes[i];
            }

            // 애니메이터의 상태를 초기화
        }

        for (int i = abilitiesToShow; i < abilityButtons.Length; i++)
        {
            if (abilityButtons[i] != null)
            {
                abilityButtons[i].gameObject.SetActive(false);
            }
        }

        if (rerollButton != null)
        {
            rerollButton.gameObject.SetActive(true);
            rerollButton.onClick.RemoveAllListeners();
            rerollButton.onClick.AddListener(RerollAbilities);
        }
        else
        {
            Debug.LogError("AbilityManager: rerollButton이 할당되지 않았습니다.");
        }

        // 강조 표시 위치 초기화
        currentIndex = 0;
        currentHighlightRotation = 0f;

        if (highlightImage != null)
        {
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();
            highlightRect.anchoredPosition = new Vector2(-550f, 50f);
            highlightRect.localScale = originalHighlightScale;
        }
        else
        {
            Debug.LogError("AbilityManager: highlightImage가 할당되지 않았습니다.");
        }

        // 애니메이션이 완료된 후 하이라이트 위치 업데이트 및 표시
        float animationDuration = 0.5f; // 버튼 애니메이션의 실제 길이로 설정
        if (delayedUpdateHighlightCoroutine != null)
        {
            StopCoroutine(delayedUpdateHighlightCoroutine);
        }
        delayedUpdateHighlightCoroutine = StartCoroutine(DelayedUpdateHighlightPosition(animationDuration));
    }

    private IEnumerator DelayedUpdateHighlightPosition(float delay)
    {
        // 애니메이션이 완료될 때까지 대기
        yield return new WaitForSecondsRealtime(delay);

        // 하이라이트 위치 업데이트
        UpdateHighlightPosition();
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        if (isSynergyAbilityActive)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                // 시너지 능력 선택 처리
                ApplySynergyAbility(currentSynergyAbility);
            }
            // 왼쪽/오른쪽 화살표는 무시
            return;
        }

        // 일반 능력 선택 입력 처리
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex + 1) % abilitiesToShow;
            UpdateHighlightPosition();
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex - 1 + abilitiesToShow) % abilitiesToShow;
            UpdateHighlightPosition();
        }
        else if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SelectAbility(availableAbilities[currentIndex]);
        }
    }

    private void RotateHighlightImage()
    {
        if (highlightImage != null)
        {
            currentHighlightRotation += highlightRotationSpeed * Time.unscaledDeltaTime;
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();
            highlightRect.rotation = Quaternion.Euler(0, 0, currentHighlightRotation);
        }
    }

    private void UpdateHighlightPosition()
    {

        if (isSynergyAbilityActive)
        {
            if (highlightImage != null && synergyAbilityIcon != null)
            {
                RectTransform iconRect = synergyAbilityIcon.GetComponent<RectTransform>();
                RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();

                if (iconRect == null)
                {
                    Debug.LogError("AbilityManager: synergyAbilityIcon의 RectTransform이 없습니다.");
                    return;
                }

                if (highlightRect == null)
                {
                    Debug.LogError("AbilityManager: highlightImage의 RectTransform이 없습니다.");
                    return;
                }

                // 강조 이미지를 시너지 능력 아이콘의 위치로 이동
                highlightRect.anchoredPosition = iconRect.anchoredPosition;
                highlightRect.localScale = originalHighlightScale;

                // 이펙트 업데이트
                UpdateHighlightEffect(iconRect);

                // 일반 능력 버튼들의 이름과 설명을 비활성화
                for (int i = 0; i < abilitiesToShow; i++)
                {
                    if (abilityNameTexts[i] != null)
                    {
                        abilityNameTexts[i].gameObject.SetActive(false);
                    }
                    if (abilityDescriptionTexts[i] != null)
                    {
                        abilityDescriptionTexts[i].gameObject.SetActive(false);
                    }
                }

                // 시너지 능력의 이름과 설명을 활성화
                if (synergyAbilityNameText != null)
                {
                    synergyAbilityNameText.gameObject.SetActive(true);
                }
                if (synergyAbilityDescriptionText != null)
                {
                    synergyAbilityDescriptionText.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogError("AbilityManager: 시너지 능력 아이콘 또는 하이라이트 이미지가 할당되지 않았습니다.");
            }
        }
        else
        {
            if (currentIndex < 0)
            {
                currentIndex = abilitiesToShow - 1;
                Debug.Log($"currentIndex adjusted to: {currentIndex}");
            }
            else if (currentIndex >= abilitiesToShow)
            {
                currentIndex = 0;
                Debug.Log($"currentIndex adjusted to: {currentIndex}");
            }

            // 현재 선택된 인덱스가 유효한지 확인
            if (abilitiesToShow <= 0)
            {
                Debug.LogError("AbilityManager: abilitiesToShow가 0 이하입니다.");
                return;
            }

            // abilityButtons와 highlightImage가 제대로 할당되었는지 확인
            if (highlightImage == null)
            {
                Debug.LogError("AbilityManager: highlightImage가 할당되지 않았습니다.");
                return;
            }

            if (currentIndex >= abilityButtons.Length)
            {
                Debug.LogError($"AbilityManager: currentIndex({currentIndex})가 abilityButtons.Length({abilityButtons.Length})을 초과했습니다.");
                return;
            }

            if (abilityButtons[currentIndex] == null)
            {
                Debug.LogError($"AbilityManager: abilityButtons[{currentIndex}]가 null입니다.");
                return;
            }

            RectTransform buttonRect = abilityButtons[currentIndex].GetComponent<RectTransform>();
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();

            if (buttonRect == null)
            {
                Debug.LogError($"AbilityManager: abilityButtons[{currentIndex}]의 RectTransform이 없습니다.");
                return;
            }

            if (highlightRect == null)
            {
                Debug.LogError("AbilityManager: highlightImage의 RectTransform이 없습니다.");
                return;
            }

            // 강조 이미지를 현재 선택된 버튼의 위치로 이동
            highlightRect.anchoredPosition = buttonRect.anchoredPosition;

            // 원래 스케일로 초기화
            highlightRect.localScale = originalHighlightScale;

            // 이펙트 업데이트
            UpdateHighlightEffect(buttonRect);

            // 모든 버튼과 아이콘의 크기를 원본 크기로 초기화
            for (int i = 0; i < abilitiesToShow; i++)
            {
                // 버튼 크기 초기화
                RectTransform btnRect = abilityButtons[i].GetComponent<RectTransform>();
                if (btnRect != null)
                {
                    btnRect.sizeDelta = originalButtonSizes[i];
                }
                else
                {
                    Debug.LogError($"AbilityManager: abilityButtons[{i}]의 RectTransform이 없습니다.");
                }

                // 아이콘 크기 초기화
                RectTransform iconRect = abilityIcons[i].GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    iconRect.sizeDelta = originalIconSizes[i];
                }
                else
                {
                    Debug.LogError($"AbilityManager: abilityIcons[{i}]의 RectTransform이 없습니다.");
                }

                // 모든 능력의 이름과 설명을 비활성화
                if (abilityNameTexts[i] != null)
                {
                    abilityNameTexts[i].gameObject.SetActive(i == currentIndex);
                }
                else
                {
                    Debug.LogError($"AbilityManager: abilityNameTexts[{i}]가 null입니다.");
                }

                if (abilityDescriptionTexts[i] != null)
                {
                    abilityDescriptionTexts[i].gameObject.SetActive(i == currentIndex);
                }
                else
                {
                    Debug.LogError($"AbilityManager: abilityDescriptionTexts[{i}]가 null입니다.");
                }
            }

            // 현재 선택된 버튼과 아이콘의 크기를 애니메이션으로 조정
            Button currentButton = abilityButtons[currentIndex];
            Image currentIcon = abilityIcons[currentIndex];

            if (currentButton != null)
            {
                RectTransform btnRect = currentButton.GetComponent<RectTransform>();
                if (btnRect != null)
                {
                    // 기존 코루틴 중지
                    if (buttonScaleCoroutine != null)
                    {
                        StopCoroutine(buttonScaleCoroutine);
                    }
                    // 새로운 스케일 애니메이션 시작
                    Vector2 targetSize = originalButtonSizes[currentIndex] * 1.1f;
                    buttonScaleCoroutine = StartCoroutine(AnimateSize(btnRect, targetSize, originalButtonSizes[currentIndex], 0.2f));
                }
                else
                {
                    Debug.LogError($"AbilityManager: currentButton[{currentIndex}]의 RectTransform이 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"AbilityManager: currentButton[{currentIndex}]가 null입니다.");
            }

            if (currentIcon != null)
            {
                RectTransform iconRect = currentIcon.GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    // 기존 코루틴 중지
                    if (iconScaleCoroutine != null)
                    {
                        StopCoroutine(iconScaleCoroutine);
                    }
                    // 새로운 스케일 애니메이션 시작
                    Vector2 targetSize = originalIconSizes[currentIndex] * 1.1f;
                    iconScaleCoroutine = StartCoroutine(AnimateSize(iconRect, targetSize, originalIconSizes[currentIndex], 0.2f));
                }
                else
                {
                    Debug.LogError($"AbilityManager: currentIcon[{currentIndex}]의 RectTransform이 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"AbilityManager: currentIcon[{currentIndex}]가 null입니다.");
            }

            if (highlightImage != null)
            {
                if (highlightScaleCoroutine != null)
                {
                    StopCoroutine(highlightScaleCoroutine);
                }
                highlightScaleCoroutine = StartCoroutine(AnimateScale(highlightImage.transform, originalHighlightScale, highlightScale, 0.2f));
            }
        }
    }


    /// <summary>
    /// RectTransform의 sizeDelta를 애니메이션으로 조정하는 코루틴
    /// </summary>
    private IEnumerator AnimateSize(RectTransform target, Vector2 targetSize, Vector2 originalSize, float duration)
    {
        float halfDuration = duration / 2f;
        float elapsed = 0f;

        // 크기 증가
        while (elapsed < halfDuration)
        {
            target.sizeDelta = Vector2.Lerp(originalSize, targetSize, elapsed / halfDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        target.sizeDelta = targetSize;

        // 크기 감소
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            target.sizeDelta = Vector2.Lerp(targetSize, originalSize, elapsed / halfDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        target.sizeDelta = originalSize;
    }

    /// <summary>
    /// Transform의 scale을 애니메이션으로 조정하는 코루틴 (하이라이트 이미지용)
    /// </summary>
    private IEnumerator AnimateScale(Transform target, Vector3 originalScale, float targetScaleFactor, float duration)
    {
        float halfDuration = duration / 2f;
        float elapsed = 0f;
        Vector3 targetScale = originalScale * targetScaleFactor;

        // 스케일 증가
        while (elapsed < halfDuration)
        {
            target.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / halfDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        target.localScale = targetScale;

        // 스케일 감소
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            target.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / halfDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        target.localScale = originalScale;
    }

    private void UpdateHighlightEffect(RectTransform targetRect)
    {
        if (highlightEffectPrefab == null)
        {
            Debug.LogError("AbilityManager: highlightEffectPrefab이 할당되지 않았습니다.");
            return;
        }

        // 이전 이펙트가 있으면 삭제
        if (currentHighlightEffect != null)
        {
            Destroy(currentHighlightEffect);
        }

        // 새로운 이펙트 인스턴스 생성
        currentHighlightEffect = Instantiate(highlightEffectPrefab, highlightImage.transform.parent);

        // 이펙트의 RectTransform 설정
        RectTransform effectRect = currentHighlightEffect.GetComponent<RectTransform>();
        if (effectRect != null)
        {
            // 이펙트 위치 및 크기 설정
            effectRect.position = targetRect.position;
            effectRect.sizeDelta = Vector2.one; // 필요에 따라 크기 조정

            // 이펙트가 하이라이트 이미지 뒤에 있도록 설정
            effectRect.SetSiblingIndex(highlightImage.transform.GetSiblingIndex());
        }

        // 이펙트 재생 (파티클 시스템인 경우)
        ParticleSystem particleSystem = currentHighlightEffect.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }

    /// <summary>
    /// 시너지 능력을 표시하는 메서드
    /// </summary>
    public void ShowSynergyAbility(SynergyAbility synergyAbility)
    {
        if (synergyAbility == null)
        {
            Debug.LogError("AbilityManager: SynergyAbility가 null입니다.");
            return;
        }

        Time.timeScale = 0f;
        isAbilitySelectionActive = true;
        isSynergyAbilityActive = true; // 시너지 능력 활성화

        currentSynergyAbility = synergyAbility; // 현재 시너지 능력 저장

        if (abilitySelectionPanel != null)
        {
            abilitySelectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("AbilityManager: abilitySelectionPanel이 할당되지 않았습니다.");
        }

        if (synergyAbilityPanel != null)
        {
            synergyAbilityPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("AbilityManager: synergyAbilityPanel이 할당되지 않았습니다.");
            return;
        }

        if (synergyAbilityNameText != null && synergyAbilityDescriptionText != null &&
            synergyAbilityIcon != null && synergyAbilityButton != null)
        {
            synergyAbilityNameText.text = synergyAbility.abilityName;
            synergyAbilityDescriptionText.text = synergyAbility.GetDescription();
            synergyAbilityIcon.sprite = synergyAbility.abilityIcon;
            synergyAbilityButton.onClick.RemoveAllListeners();
            synergyAbilityButton.onClick.AddListener(() => ApplySynergyAbility(synergyAbility));
        }
        else
        {
            Debug.LogError("AbilityManager: 시너지 능력 UI 요소들이 할당되지 않았습니다.");
        }

        // 시너지 능력 버튼을 기본 선택으로 설정
        EventSystem.current.SetSelectedGameObject(synergyAbilityButton.gameObject);

        // 시너지 능력의 경우, 현재 인덱스를 0으로 고정
        currentIndex = 0;
        UpdateHighlightPosition();
    }

    private void ApplySynergyAbility(SynergyAbility synergyAbility)
    {
        if (synergyAbility == null)
        {
            Debug.LogError("AbilityManager: 적용할 SynergyAbility가 null입니다.");
            return;
        }

        if (playerAbilityManager != null)
        {
            playerAbilityManager.ApplySynergyAbility(synergyAbility);
            player.AcquireSynergyAbility(synergyAbility);
            Debug.Log($"Acquired synergy ability: {synergyAbility.abilityName}");
        }
        else
        {
            Debug.LogError("AbilityManager: PlayerAbilityManager가 초기화되지 않았습니다.");
            return;
        }

        if (synergyAbilityPanel != null)
        {
            synergyAbilityPanel.SetActive(false);
        }

        if (uiManager != null)
        {
            uiManager.DisableDepthOfField();
        }

        Time.timeScale = 1f;
        isAbilitySelectionActive = false;
        isSynergyAbilityActive = false; // 시너지 능력 비활성화

        // 이펙트 정리
        if (currentHighlightEffect != null)
        {
            Destroy(currentHighlightEffect);
        }
    }

    public IEnumerator DelayedShowSynergyAbility(SynergyAbility synergyAbility)
    {
        yield return new WaitForSecondsRealtime(0.2f); // 0.2초 지연

        ShowSynergyAbility(synergyAbility);
    }
    private void SelectAbility(Ability ability)
    {
        if (ability == null)
        {
            Debug.LogError("AbilityManager: 선택된 Ability가 null입니다.");
            return;
        }

        // 애니메이션을 실행하고 선택 완료 후에 능력을 적용
        StartCoroutine(PlaySelectionAnimation(ability));
    }

    private IEnumerator PlaySelectionAnimation(Ability selectedAbility)
    {
        // 애니메이션 중 입력 비활성화
        isAbilitySelectionActive = false;

        // 애니메이터 트리거 설정
        for (int i = 0; i < abilitiesToShow; i++)
        {
            Animator animator = abilityButtons[i].GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime; // Unscaled Time으로 설정
                if (availableAbilities[i] == selectedAbility)
                {
                    // 선택된 능력은 "Choice" 트리거 발동
                    animator.SetTrigger("Choice");
                }
                else
                {
                    // 선택되지 않은 능력들은 "NunChoice" 트리거 발동
                    animator.SetTrigger("NunChoice");
                }
            }
            else
            {
                Debug.LogError($"AbilityManager: abilityButtons[{i}]에 Animator가 없습니다.");
            }
        }

        // 애니메이션 길이만큼 대기 (예: 0.5초)
        float animationDuration = 0.5f; // 실제 애니메이션 길이에 맞게 조정
        yield return new WaitForSecondsRealtime(animationDuration);

        // 애니메이션이 끝난 후에 능력 적용
        ApplySelectedAbility(selectedAbility);

        // 패널을 애니메이션 이후에 비활성화
        if (abilitySelectionPanel != null)
        {
            abilitySelectionPanel.SetActive(false);
        }
    }

    private void ApplySelectedAbility(Ability ability)
    {
        if (playerAbilityManager != null)
        {
            playerAbilityManager.SelectAbility(ability);

            if (ability is SynergyAbility synergyAbility)
            {
                player.AcquireSynergyAbility(synergyAbility);
                Debug.Log($"Acquired synergy ability: {synergyAbility.abilityName}");

                // 시너지 능력 창을 표시하기 전에 0.5초 지연 시간 추가
                StartCoroutine(DelayedShowSynergyAbility(synergyAbility));

                // Time.timeScale을 다시 1로 설정하지 않음
                return;
            }
        }
        else
        {
            Debug.LogError("AbilityManager: PlayerAbilityManager가 초기화되지 않았습니다.");
            return;
        }

        if (rerollButton != null)
        {
            rerollButton.gameObject.SetActive(false);
        }

        if (uiManager != null)
        {
            uiManager.DisableDepthOfField();
        }

        Time.timeScale = 1f;
        isAbilitySelectionActive = false;
        isSynergyAbilityActive = false; // 선택 완료 후 시너지 비활성화

        // 이펙트 정리
        if (currentHighlightEffect != null)
        {
            Destroy(currentHighlightEffect);
        }
    }

    private void RerollAbilities()
    {
        if (playerAbilityManager == null)
        {
            Debug.LogError("AbilityManager: PlayerAbilityManager가 초기화되지 않았습니다.");
            return;
        }

        availableAbilities = playerAbilityManager.GetAvailableAbilities();
        if (availableAbilities == null)
        {
            Debug.LogError("AbilityManager: 사용 가능한 능력을 가져오지 못했습니다.");
            return;
        }

        ShuffleAbilities();

        abilitiesToShow = Mathf.Min(abilityButtons.Length, availableAbilities.Count);

        // 원본 크기 배열 초기화
        originalButtonSizes = new Vector2[abilitiesToShow];
        originalIconSizes = new Vector2[abilitiesToShow];

        for (int i = 0; i < abilitiesToShow; i++)
        {
            if (abilityButtons[i] == null || abilityNameTexts[i] == null ||
                abilityDescriptionTexts[i] == null || abilityIcons[i] == null)
            {
                Debug.LogError($"AbilityManager: abilityButton 또는 UI 요소가 {i}번째에서 할당되지 않았습니다.");
                continue;
            }

            var ability = availableAbilities[i];
            abilityNameTexts[i].text = ability.abilityName;
            abilityDescriptionTexts[i].text = ability.GetDescription();
            abilityIcons[i].sprite = ability.abilityIcon;
            abilityButtons[i].onClick.RemoveAllListeners();
            int index = i;
            abilityButtons[i].onClick.AddListener(() => SelectAbility(availableAbilities[index]));
            abilityButtons[i].gameObject.SetActive(true);

            // 모든 능력의 이름과 설명을 비활성화
            abilityNameTexts[i].gameObject.SetActive(false);
            abilityDescriptionTexts[i].gameObject.SetActive(false);

            // 버튼과 아이콘의 크기를 초기 크기로 설정
            RectTransform buttonRect = abilityButtons[i].GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.sizeDelta = initialButtonSizes[i];
                originalButtonSizes[i] = initialButtonSizes[i];
            }

            RectTransform iconRect = abilityIcons[i].GetComponent<RectTransform>();
            if (iconRect != null)
            {
                iconRect.sizeDelta = initialIconSizes[i];
                originalIconSizes[i] = initialIconSizes[i];
            }

            // 애니메이터의 상태를 초기화
            Animator animator = abilityButtons[i].GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Idle", 0, 0f);
            }
        }

        for (int i = abilitiesToShow; i < abilityButtons.Length; i++)
        {
            if (abilityButtons[i] != null)
            {
                abilityButtons[i].gameObject.SetActive(false);
            }
        }

        if (rerollButton != null)
        {
            rerollButton.gameObject.SetActive(true);
            rerollButton.onClick.RemoveAllListeners();
            rerollButton.onClick.AddListener(RerollAbilities);
        }
        else
        {
            Debug.LogError("AbilityManager: rerollButton이 할당되지 않았습니다.");
        }

        // 강조 표시 위치 초기화 및 업데이트
        currentIndex = 0;
        currentHighlightRotation = 0f;
        isSynergyAbilityActive = false; // 리롤 시 시너지 비활성화

        // 하이라이트 이미지의 위치를 로컬 좌표 (-550, 50, 0)으로 설정
        if (highlightImage != null)
        {
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();
            highlightRect.localPosition = new Vector3(-550f, 50f, 0f);
            highlightRect.localScale = originalHighlightScale; // 원래 스케일로 리셋
        }
        else
        {
            Debug.LogError("AbilityManager: highlightImage가 할당되지 않았습니다.");
        }
        float animationDuration = 0.5f; // 버튼 애니메이션의 실제 길이로 설정
        if (delayedUpdateHighlightCoroutine != null)
        {
            StopCoroutine(delayedUpdateHighlightCoroutine);
        }
        delayedUpdateHighlightCoroutine = StartCoroutine(DelayedUpdateHighlightPosition(animationDuration));
    }
    public void TriggerShowSynergyAbility(SynergyAbility synergyAbility)
    {
        StartCoroutine(DelayedShowSynergyAbility(synergyAbility));
    }
    private void ShuffleAbilities()
    {
        if (availableAbilities == null)
        {
            Debug.LogError("AbilityManager: 사용할 수 있는 능력 리스트가 null입니다.");
            return;
        }

        for (int i = 0; i < availableAbilities.Count; i++)
        {
            var temp = availableAbilities[i];
            int randomIndex = Random.Range(i, availableAbilities.Count);
            availableAbilities[i] = availableAbilities[randomIndex];
            availableAbilities[randomIndex] = temp;
        }
    }
}
