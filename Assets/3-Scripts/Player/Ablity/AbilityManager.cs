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
    // ī�װ��� ��ư �̹��� ���� ��ųʸ�
    private Dictionary<string, Sprite> categoryButtonImages = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> synergyCategoryButtonImages = new Dictionary<string, Sprite>();

    // ���� ǥ�� ���� ���� �߰�
    [SerializeField]
    private Image highlightImage; // ���� ǥ�� �̹���
    private int currentIndex = 0; // ���� ���õ� �ɷ��� �ε���
    private bool isAbilitySelectionActive = false; // �ɷ� ���� UI Ȱ��ȭ ����
    private bool isSynergyAbilityActive = false; // �ó��� �ɷ� Ȱ��ȭ ����

    private int abilitiesToShow = 0; // ǥ�õǴ� �ɷ��� �� �߰�

    // ȸ�� ���� ���� �߰�
    private float highlightRotationSpeed = 50f; // ���� �̹��� ȸ�� �ӵ� (��/��)
    private float currentHighlightRotation = 0f; // ���� ȸ�� ����

    // ũ�� ���� ���� ���� �߰�
    private float highlightScale = 1.1f; // ������ ��ҵ��� ������ ����

    // ����Ʈ ���� ���� �߰�
    [SerializeField]
    private GameObject highlightEffectPrefab; // ����Ʈ ������
    private GameObject currentHighlightEffect; // ���� ��� ���� ����Ʈ �ν��Ͻ�

    // Coroutine references to manage scaling animations
    private Coroutine highlightScaleCoroutine;
    private Coroutine buttonScaleCoroutine;
    private Coroutine iconScaleCoroutine;

    // ���� ũ�� ������ ���� �迭
    private Vector2[] originalButtonSizes;
    private Vector2[] originalIconSizes;

    // �ʱ� ũ�� ������ ���� �迭 �߰�
    private Vector2[] initialButtonSizes;
    private Vector2[] initialIconSizes;

    // ���ο� ���� ���� (���̶���Ʈ ������ ����)
    private Vector3 originalHighlightScale;

    // ���� �ó��� �ɷ��� �����ϴ� ���� �߰�
    private SynergyAbility currentSynergyAbility;
    // �ɷ� ���� �� ȣ���� �̺�Ʈ
    public UnityEvent OnAbilitiesChanged;
    // �߰�: �ڷ�ƾ ���� ����
    private Coroutine delayedUpdateHighlightCoroutine;
    private void Awake()
    {
        // �ʱ� ũ�� �迭 �ʱ�ȭ
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
                    Debug.LogError($"AbilityManager: abilityButtons[{i}]�� RectTransform�� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogError($"AbilityManager: abilityButtons[{i}]�� �Ҵ���� �ʾҽ��ϴ�.");
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
                    Debug.LogError($"AbilityManager: abilityIcons[{i}]�� RectTransform�� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogError($"AbilityManager: abilityIcons[{i}]�� �Ҵ���� �ʾҽ��ϴ�.");
            }
        }

        // �ʱ� ���̶���Ʈ �̹��� ������ ����
        if (highlightImage != null)
        {
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();
            if (highlightRect != null)
            {
                originalHighlightScale = highlightRect.localScale;
            }
            else
            {
                Debug.LogError("AbilityManager: highlightImage�� RectTransform�� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("AbilityManager: highlightImage�� �Ҵ���� �ʾҽ��ϴ�.");
        }
        LoadCategoryButtonImages();
    }

    private void LoadCategoryButtonImages()
    {
        // ī�װ� �̸� �迭 (Null ����)
        string[] categories = { "Lust", "Envy", "Sloth", "Gluttony", "Greed", "Wrath", "Pride", "Null" };

        foreach (string category in categories)
        {
            // �Ϲ� �ɷ��� ��ư �̹��� �ε�
            Sprite buttonImage = Resources.Load<Sprite>($"CategoryButtonImages/{category}");
            if (buttonImage != null)
            {
                categoryButtonImages.Add(category, buttonImage);
            }
            else
            {
                Debug.LogWarning($"Button image for category '{category}' not found.");
            }

            // �ó��� �ɷ��� ��ư �̹��� �ε�
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
            Debug.LogError("AbilityManager: Player�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnLevelUp.RemoveListener(ShowAbilitySelection);
        }

        // ����Ʈ ����
        if (currentHighlightEffect != null)
        {
            Destroy(currentHighlightEffect);
        }
        // ��� Ȱ�� �ڷ�ƾ ����
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
            Debug.LogError("AbilityManager: PlayerAbilityManager�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    public void ShowAbilitySelection()
    {
        if (playerAbilityManager == null)
        {
            Debug.LogError("AbilityManager: PlayerAbilityManager�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        if (uiManager != null)
        {
            uiManager.EnableDepthOfField();
        }

        Time.timeScale = 0f;
        isAbilitySelectionActive = true;
        isSynergyAbilityActive = false; // �Ϲ� �ɷ� ���� �� �ó��� ��Ȱ��ȭ

        if (abilitySelectionPanel != null)
        {
            abilitySelectionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("AbilityManager: abilitySelectionPanel�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (synergyAbilityPanel != null)
        {
            synergyAbilityPanel.SetActive(false);
        }

        availableAbilities = playerAbilityManager.GetAvailableAbilities();
        if (availableAbilities == null || availableAbilities.Count == 0)
        {
            Debug.LogError("AbilityManager: ��� ������ �ɷ��� �����ϴ�.");
            return;
        }

        ShuffleAbilities();

        abilitiesToShow = Mathf.Min(abilityButtons.Length, availableAbilities.Count);

        if (abilitiesToShow <= 0)
        {
            Debug.LogError("AbilityManager: abilitiesToShow�� 0 �����Դϴ�.");
            return;
        }

        // ��ư�� ������ �ʱ�ȭ
        for (int i = 0; i < abilitiesToShow; i++)
        {
            if (abilityButtons[i] == null || abilityNameTexts[i] == null ||
                abilityDescriptionTexts[i] == null || abilityIcons[i] == null)
            {
                Debug.LogError($"AbilityManager: abilityButton �Ǵ� UI ��Ұ� {i}��°���� �Ҵ���� �ʾҽ��ϴ�.");
                continue;
            }

            var ability = availableAbilities[i];

            if (categoryButtonImages.TryGetValue(ability.category, out Sprite buttonImage))
            {
                // ��ư�� �̹����� �����մϴ�.
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

            // ��� �ɷ��� �̸��� ������ ��Ȱ��ȭ
            abilityNameTexts[i].gameObject.SetActive(false);
            abilityDescriptionTexts[i].gameObject.SetActive(false);

            // ��ư�� �������� ũ�⸦ �ʱ� ũ��� ����
            RectTransform buttonRect = abilityButtons[i].GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                // �ʱ� ũ��� ����
                buttonRect.sizeDelta = originalButtonSizes[i];
            }

            RectTransform iconRect = abilityIcons[i].GetComponent<RectTransform>();
            if (iconRect != null)
            {
                // �ʱ� ũ��� ����
                iconRect.sizeDelta = originalIconSizes[i];
            }

            // �ִϸ������� ���¸� �ʱ�ȭ
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
            Debug.LogError("AbilityManager: rerollButton�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // ���� ǥ�� ��ġ �ʱ�ȭ
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
            Debug.LogError("AbilityManager: highlightImage�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // �ִϸ��̼��� �Ϸ�� �� ���̶���Ʈ ��ġ ������Ʈ �� ǥ��
        float animationDuration = 0.5f; // ��ư �ִϸ��̼��� ���� ���̷� ����
        if (delayedUpdateHighlightCoroutine != null)
        {
            StopCoroutine(delayedUpdateHighlightCoroutine);
        }
        delayedUpdateHighlightCoroutine = StartCoroutine(DelayedUpdateHighlightPosition(animationDuration));
    }

    private IEnumerator DelayedUpdateHighlightPosition(float delay)
    {
        // �ִϸ��̼��� �Ϸ�� ������ ���
        yield return new WaitForSecondsRealtime(delay);

        // ���̶���Ʈ ��ġ ������Ʈ
        UpdateHighlightPosition();
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        if (isSynergyAbilityActive)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                // �ó��� �ɷ� ���� ó��
                ApplySynergyAbility(currentSynergyAbility);
            }
            // ����/������ ȭ��ǥ�� ����
            return;
        }

        // �Ϲ� �ɷ� ���� �Է� ó��
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
                    Debug.LogError("AbilityManager: synergyAbilityIcon�� RectTransform�� �����ϴ�.");
                    return;
                }

                if (highlightRect == null)
                {
                    Debug.LogError("AbilityManager: highlightImage�� RectTransform�� �����ϴ�.");
                    return;
                }

                // ���� �̹����� �ó��� �ɷ� �������� ��ġ�� �̵�
                highlightRect.anchoredPosition = iconRect.anchoredPosition;
                highlightRect.localScale = originalHighlightScale;

                // ����Ʈ ������Ʈ
                UpdateHighlightEffect(iconRect);

                // �Ϲ� �ɷ� ��ư���� �̸��� ������ ��Ȱ��ȭ
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

                // �ó��� �ɷ��� �̸��� ������ Ȱ��ȭ
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
                Debug.LogError("AbilityManager: �ó��� �ɷ� ������ �Ǵ� ���̶���Ʈ �̹����� �Ҵ���� �ʾҽ��ϴ�.");
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

            // ���� ���õ� �ε����� ��ȿ���� Ȯ��
            if (abilitiesToShow <= 0)
            {
                Debug.LogError("AbilityManager: abilitiesToShow�� 0 �����Դϴ�.");
                return;
            }

            // abilityButtons�� highlightImage�� ����� �Ҵ�Ǿ����� Ȯ��
            if (highlightImage == null)
            {
                Debug.LogError("AbilityManager: highlightImage�� �Ҵ���� �ʾҽ��ϴ�.");
                return;
            }

            if (currentIndex >= abilityButtons.Length)
            {
                Debug.LogError($"AbilityManager: currentIndex({currentIndex})�� abilityButtons.Length({abilityButtons.Length})�� �ʰ��߽��ϴ�.");
                return;
            }

            if (abilityButtons[currentIndex] == null)
            {
                Debug.LogError($"AbilityManager: abilityButtons[{currentIndex}]�� null�Դϴ�.");
                return;
            }

            RectTransform buttonRect = abilityButtons[currentIndex].GetComponent<RectTransform>();
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();

            if (buttonRect == null)
            {
                Debug.LogError($"AbilityManager: abilityButtons[{currentIndex}]�� RectTransform�� �����ϴ�.");
                return;
            }

            if (highlightRect == null)
            {
                Debug.LogError("AbilityManager: highlightImage�� RectTransform�� �����ϴ�.");
                return;
            }

            // ���� �̹����� ���� ���õ� ��ư�� ��ġ�� �̵�
            highlightRect.anchoredPosition = buttonRect.anchoredPosition;

            // ���� �����Ϸ� �ʱ�ȭ
            highlightRect.localScale = originalHighlightScale;

            // ����Ʈ ������Ʈ
            UpdateHighlightEffect(buttonRect);

            // ��� ��ư�� �������� ũ�⸦ ���� ũ��� �ʱ�ȭ
            for (int i = 0; i < abilitiesToShow; i++)
            {
                // ��ư ũ�� �ʱ�ȭ
                RectTransform btnRect = abilityButtons[i].GetComponent<RectTransform>();
                if (btnRect != null)
                {
                    btnRect.sizeDelta = originalButtonSizes[i];
                }
                else
                {
                    Debug.LogError($"AbilityManager: abilityButtons[{i}]�� RectTransform�� �����ϴ�.");
                }

                // ������ ũ�� �ʱ�ȭ
                RectTransform iconRect = abilityIcons[i].GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    iconRect.sizeDelta = originalIconSizes[i];
                }
                else
                {
                    Debug.LogError($"AbilityManager: abilityIcons[{i}]�� RectTransform�� �����ϴ�.");
                }

                // ��� �ɷ��� �̸��� ������ ��Ȱ��ȭ
                if (abilityNameTexts[i] != null)
                {
                    abilityNameTexts[i].gameObject.SetActive(i == currentIndex);
                }
                else
                {
                    Debug.LogError($"AbilityManager: abilityNameTexts[{i}]�� null�Դϴ�.");
                }

                if (abilityDescriptionTexts[i] != null)
                {
                    abilityDescriptionTexts[i].gameObject.SetActive(i == currentIndex);
                }
                else
                {
                    Debug.LogError($"AbilityManager: abilityDescriptionTexts[{i}]�� null�Դϴ�.");
                }
            }

            // ���� ���õ� ��ư�� �������� ũ�⸦ �ִϸ��̼����� ����
            Button currentButton = abilityButtons[currentIndex];
            Image currentIcon = abilityIcons[currentIndex];

            if (currentButton != null)
            {
                RectTransform btnRect = currentButton.GetComponent<RectTransform>();
                if (btnRect != null)
                {
                    // ���� �ڷ�ƾ ����
                    if (buttonScaleCoroutine != null)
                    {
                        StopCoroutine(buttonScaleCoroutine);
                    }
                    // ���ο� ������ �ִϸ��̼� ����
                    Vector2 targetSize = originalButtonSizes[currentIndex] * 1.1f;
                    buttonScaleCoroutine = StartCoroutine(AnimateSize(btnRect, targetSize, originalButtonSizes[currentIndex], 0.2f));
                }
                else
                {
                    Debug.LogError($"AbilityManager: currentButton[{currentIndex}]�� RectTransform�� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogError($"AbilityManager: currentButton[{currentIndex}]�� null�Դϴ�.");
            }

            if (currentIcon != null)
            {
                RectTransform iconRect = currentIcon.GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    // ���� �ڷ�ƾ ����
                    if (iconScaleCoroutine != null)
                    {
                        StopCoroutine(iconScaleCoroutine);
                    }
                    // ���ο� ������ �ִϸ��̼� ����
                    Vector2 targetSize = originalIconSizes[currentIndex] * 1.1f;
                    iconScaleCoroutine = StartCoroutine(AnimateSize(iconRect, targetSize, originalIconSizes[currentIndex], 0.2f));
                }
                else
                {
                    Debug.LogError($"AbilityManager: currentIcon[{currentIndex}]�� RectTransform�� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogError($"AbilityManager: currentIcon[{currentIndex}]�� null�Դϴ�.");
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
    /// RectTransform�� sizeDelta�� �ִϸ��̼����� �����ϴ� �ڷ�ƾ
    /// </summary>
    private IEnumerator AnimateSize(RectTransform target, Vector2 targetSize, Vector2 originalSize, float duration)
    {
        float halfDuration = duration / 2f;
        float elapsed = 0f;

        // ũ�� ����
        while (elapsed < halfDuration)
        {
            target.sizeDelta = Vector2.Lerp(originalSize, targetSize, elapsed / halfDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        target.sizeDelta = targetSize;

        // ũ�� ����
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
    /// Transform�� scale�� �ִϸ��̼����� �����ϴ� �ڷ�ƾ (���̶���Ʈ �̹�����)
    /// </summary>
    private IEnumerator AnimateScale(Transform target, Vector3 originalScale, float targetScaleFactor, float duration)
    {
        float halfDuration = duration / 2f;
        float elapsed = 0f;
        Vector3 targetScale = originalScale * targetScaleFactor;

        // ������ ����
        while (elapsed < halfDuration)
        {
            target.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / halfDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        target.localScale = targetScale;

        // ������ ����
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
            Debug.LogError("AbilityManager: highlightEffectPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // ���� ����Ʈ�� ������ ����
        if (currentHighlightEffect != null)
        {
            Destroy(currentHighlightEffect);
        }

        // ���ο� ����Ʈ �ν��Ͻ� ����
        currentHighlightEffect = Instantiate(highlightEffectPrefab, highlightImage.transform.parent);

        // ����Ʈ�� RectTransform ����
        RectTransform effectRect = currentHighlightEffect.GetComponent<RectTransform>();
        if (effectRect != null)
        {
            // ����Ʈ ��ġ �� ũ�� ����
            effectRect.position = targetRect.position;
            effectRect.sizeDelta = Vector2.one; // �ʿ信 ���� ũ�� ����

            // ����Ʈ�� ���̶���Ʈ �̹��� �ڿ� �ֵ��� ����
            effectRect.SetSiblingIndex(highlightImage.transform.GetSiblingIndex());
        }

        // ����Ʈ ��� (��ƼŬ �ý����� ���)
        ParticleSystem particleSystem = currentHighlightEffect.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }

    /// <summary>
    /// �ó��� �ɷ��� ǥ���ϴ� �޼���
    /// </summary>
    public void ShowSynergyAbility(SynergyAbility synergyAbility)
    {
        if (synergyAbility == null)
        {
            Debug.LogError("AbilityManager: SynergyAbility�� null�Դϴ�.");
            return;
        }

        Time.timeScale = 0f;
        isAbilitySelectionActive = true;
        isSynergyAbilityActive = true; // �ó��� �ɷ� Ȱ��ȭ

        currentSynergyAbility = synergyAbility; // ���� �ó��� �ɷ� ����

        if (abilitySelectionPanel != null)
        {
            abilitySelectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("AbilityManager: abilitySelectionPanel�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (synergyAbilityPanel != null)
        {
            synergyAbilityPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("AbilityManager: synergyAbilityPanel�� �Ҵ���� �ʾҽ��ϴ�.");
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
            Debug.LogError("AbilityManager: �ó��� �ɷ� UI ��ҵ��� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // �ó��� �ɷ� ��ư�� �⺻ �������� ����
        EventSystem.current.SetSelectedGameObject(synergyAbilityButton.gameObject);

        // �ó��� �ɷ��� ���, ���� �ε����� 0���� ����
        currentIndex = 0;
        UpdateHighlightPosition();
    }

    private void ApplySynergyAbility(SynergyAbility synergyAbility)
    {
        if (synergyAbility == null)
        {
            Debug.LogError("AbilityManager: ������ SynergyAbility�� null�Դϴ�.");
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
            Debug.LogError("AbilityManager: PlayerAbilityManager�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
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
        isSynergyAbilityActive = false; // �ó��� �ɷ� ��Ȱ��ȭ

        // ����Ʈ ����
        if (currentHighlightEffect != null)
        {
            Destroy(currentHighlightEffect);
        }
    }

    public IEnumerator DelayedShowSynergyAbility(SynergyAbility synergyAbility)
    {
        yield return new WaitForSecondsRealtime(0.2f); // 0.2�� ����

        ShowSynergyAbility(synergyAbility);
    }
    private void SelectAbility(Ability ability)
    {
        if (ability == null)
        {
            Debug.LogError("AbilityManager: ���õ� Ability�� null�Դϴ�.");
            return;
        }

        // �ִϸ��̼��� �����ϰ� ���� �Ϸ� �Ŀ� �ɷ��� ����
        StartCoroutine(PlaySelectionAnimation(ability));
    }

    private IEnumerator PlaySelectionAnimation(Ability selectedAbility)
    {
        // �ִϸ��̼� �� �Է� ��Ȱ��ȭ
        isAbilitySelectionActive = false;

        // �ִϸ����� Ʈ���� ����
        for (int i = 0; i < abilitiesToShow; i++)
        {
            Animator animator = abilityButtons[i].GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime; // Unscaled Time���� ����
                if (availableAbilities[i] == selectedAbility)
                {
                    // ���õ� �ɷ��� "Choice" Ʈ���� �ߵ�
                    animator.SetTrigger("Choice");
                }
                else
                {
                    // ���õ��� ���� �ɷµ��� "NunChoice" Ʈ���� �ߵ�
                    animator.SetTrigger("NunChoice");
                }
            }
            else
            {
                Debug.LogError($"AbilityManager: abilityButtons[{i}]�� Animator�� �����ϴ�.");
            }
        }

        // �ִϸ��̼� ���̸�ŭ ��� (��: 0.5��)
        float animationDuration = 0.5f; // ���� �ִϸ��̼� ���̿� �°� ����
        yield return new WaitForSecondsRealtime(animationDuration);

        // �ִϸ��̼��� ���� �Ŀ� �ɷ� ����
        ApplySelectedAbility(selectedAbility);

        // �г��� �ִϸ��̼� ���Ŀ� ��Ȱ��ȭ
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

                // �ó��� �ɷ� â�� ǥ���ϱ� ���� 0.5�� ���� �ð� �߰�
                StartCoroutine(DelayedShowSynergyAbility(synergyAbility));

                // Time.timeScale�� �ٽ� 1�� �������� ����
                return;
            }
        }
        else
        {
            Debug.LogError("AbilityManager: PlayerAbilityManager�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
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
        isSynergyAbilityActive = false; // ���� �Ϸ� �� �ó��� ��Ȱ��ȭ

        // ����Ʈ ����
        if (currentHighlightEffect != null)
        {
            Destroy(currentHighlightEffect);
        }
    }

    private void RerollAbilities()
    {
        if (playerAbilityManager == null)
        {
            Debug.LogError("AbilityManager: PlayerAbilityManager�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        availableAbilities = playerAbilityManager.GetAvailableAbilities();
        if (availableAbilities == null)
        {
            Debug.LogError("AbilityManager: ��� ������ �ɷ��� �������� ���߽��ϴ�.");
            return;
        }

        ShuffleAbilities();

        abilitiesToShow = Mathf.Min(abilityButtons.Length, availableAbilities.Count);

        // ���� ũ�� �迭 �ʱ�ȭ
        originalButtonSizes = new Vector2[abilitiesToShow];
        originalIconSizes = new Vector2[abilitiesToShow];

        for (int i = 0; i < abilitiesToShow; i++)
        {
            if (abilityButtons[i] == null || abilityNameTexts[i] == null ||
                abilityDescriptionTexts[i] == null || abilityIcons[i] == null)
            {
                Debug.LogError($"AbilityManager: abilityButton �Ǵ� UI ��Ұ� {i}��°���� �Ҵ���� �ʾҽ��ϴ�.");
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

            // ��� �ɷ��� �̸��� ������ ��Ȱ��ȭ
            abilityNameTexts[i].gameObject.SetActive(false);
            abilityDescriptionTexts[i].gameObject.SetActive(false);

            // ��ư�� �������� ũ�⸦ �ʱ� ũ��� ����
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

            // �ִϸ������� ���¸� �ʱ�ȭ
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
            Debug.LogError("AbilityManager: rerollButton�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // ���� ǥ�� ��ġ �ʱ�ȭ �� ������Ʈ
        currentIndex = 0;
        currentHighlightRotation = 0f;
        isSynergyAbilityActive = false; // ���� �� �ó��� ��Ȱ��ȭ

        // ���̶���Ʈ �̹����� ��ġ�� ���� ��ǥ (-550, 50, 0)���� ����
        if (highlightImage != null)
        {
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();
            highlightRect.localPosition = new Vector3(-550f, 50f, 0f);
            highlightRect.localScale = originalHighlightScale; // ���� �����Ϸ� ����
        }
        else
        {
            Debug.LogError("AbilityManager: highlightImage�� �Ҵ���� �ʾҽ��ϴ�.");
        }
        float animationDuration = 0.5f; // ��ư �ִϸ��̼��� ���� ���̷� ����
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
            Debug.LogError("AbilityManager: ����� �� �ִ� �ɷ� ����Ʈ�� null�Դϴ�.");
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
