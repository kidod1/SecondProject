using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    // 강조 표시 관련 변수 추가
    [SerializeField]
    private Image highlightImage; // 강조 표시 이미지
    private int currentIndex = 0; // 현재 선택된 능력의 인덱스
    private bool isAbilitySelectionActive = false; // 능력 선택 UI 활성화 여부

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
            Debug.Log("블러 전환");
        }

        Time.timeScale = 0f;
        isAbilitySelectionActive = true; // 능력 선택 UI 활성화

        if (abilitySelectionPanel != null)
        {
            abilitySelectionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("AbilityManager: abilitySelectionPanel이 할당되지 않았습니다.");
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

            // 버튼과 아이콘의 스케일을 기본값으로 초기화
            ResetButtonAndIconScale(abilityButtons[i], abilityIcons[i]);

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

        // 하이라이트 이미지의 위치를 로컬 좌표 (0, 45, 0)으로 설정
        if (highlightImage != null)
        {
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();
            highlightRect.localPosition = new Vector3(0f, 45f, 0f);
        }
        else
        {
            Debug.LogError("AbilityManager: highlightImage가 할당되지 않았습니다.");
        }

        UpdateHighlightPosition();
    }

    private void Update()
    {
        if (isAbilitySelectionActive)
        {
            HandleKeyboardInput();
            RotateHighlightImage();
        }
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex = (currentIndex + 1) % abilitiesToShow;
            UpdateHighlightPosition();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex = (currentIndex - 1 + abilitiesToShow) % abilitiesToShow;
            UpdateHighlightPosition();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
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
        if (currentIndex < 0)
        {
            currentIndex = abilitiesToShow - 1;
        }
        else if (currentIndex >= abilitiesToShow)
        {
            currentIndex = 0;
        }

        if (highlightImage != null && abilityButtons[currentIndex] != null)
        {
            RectTransform buttonRect = abilityButtons[currentIndex].GetComponent<RectTransform>();
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();

            // 강조 이미지를 현재 선택된 버튼의 위치로 이동 및 크기 조정
            highlightRect.position = buttonRect.position;

            // 강조 이미지의 크기를 1.1배로 조정
            highlightRect.localScale = Vector3.one * highlightScale;

            // 이펙트 업데이트
            UpdateHighlightEffect(buttonRect);

            // 현재 선택된 능력의 이름과 설명만 활성화하고 나머지는 비활성화
            for (int i = 0; i < abilityNameTexts.Length; i++)
            {
                bool isCurrent = (i == currentIndex);
                if (abilityNameTexts[i] != null)
                {
                    abilityNameTexts[i].gameObject.SetActive(isCurrent);
                }
                if (abilityDescriptionTexts[i] != null)
                {
                    abilityDescriptionTexts[i].gameObject.SetActive(isCurrent);
                }

                // 버튼과 아이콘의 크기 조정
                if (abilityButtons[i] != null)
                {
                    abilityButtons[i].transform.localScale = isCurrent ? Vector3.one * highlightScale : Vector3.one;
                }
                if (abilityIcons[i] != null)
                {
                    abilityIcons[i].transform.localScale = isCurrent ? Vector3.one * highlightScale : Vector3.one;
                }
            }
        }
        else
        {
            Debug.LogError("AbilityManager: 강조 이미지나 능력 버튼이 할당되지 않았습니다.");
        }
    }

    private void UpdateHighlightEffect(RectTransform buttonRect)
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
            effectRect.position = buttonRect.position;
            effectRect.localScale = Vector3.one; // 필요에 따라 스케일 조정

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

    private void ResetButtonAndIconScale(Button button, Image icon)
    {
        if (button != null)
        {
            button.transform.localScale = Vector3.one;
        }
        if (icon != null)
        {
            icon.transform.localScale = Vector3.one;
        }
    }

    public void ShowSynergyAbility(SynergyAbility synergyAbility)
    {
        if (synergyAbility == null)
        {
            Debug.LogError("AbilityManager: SynergyAbility가 null입니다.");
            return;
        }

        Time.timeScale = 0f;
        isAbilitySelectionActive = true;

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

        // 이펙트 정리
        if (currentHighlightEffect != null)
        {
            Destroy(currentHighlightEffect);
        }
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

            // 버튼과 아이콘의 스케일을 기본값으로 초기화
            ResetButtonAndIconScale(abilityButtons[i], abilityIcons[i]);

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

        // 강조 표시 위치 초기화 및 업데이트
        currentIndex = 0;
        currentHighlightRotation = 0f;

        // 하이라이트 이미지의 위치를 로컬 좌표 (0, 45, 0)으로 설정
        if (highlightImage != null)
        {
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();
            highlightRect.localPosition = new Vector3(0f, 45f, 0f);
        }
        else
        {
            Debug.LogError("AbilityManager: highlightImage가 할당되지 않았습니다.");
        }

        UpdateHighlightPosition();
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
