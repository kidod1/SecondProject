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

    // ���� ǥ�� ���� ���� �߰�
    [SerializeField]
    private Image highlightImage; // ���� ǥ�� �̹���
    private int currentIndex = 0; // ���� ���õ� �ɷ��� �ε���
    private bool isAbilitySelectionActive = false; // �ɷ� ���� UI Ȱ��ȭ ����

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
            Debug.Log("�� ��ȯ");
        }

        Time.timeScale = 0f;
        isAbilitySelectionActive = true; // �ɷ� ���� UI Ȱ��ȭ

        if (abilitySelectionPanel != null)
        {
            abilitySelectionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("AbilityManager: abilitySelectionPanel�� �Ҵ���� �ʾҽ��ϴ�.");
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

            // ��ư�� �������� �������� �⺻������ �ʱ�ȭ
            ResetButtonAndIconScale(abilityButtons[i], abilityIcons[i]);

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

        // ���̶���Ʈ �̹����� ��ġ�� ���� ��ǥ (0, 45, 0)���� ����
        if (highlightImage != null)
        {
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();
            highlightRect.localPosition = new Vector3(0f, 45f, 0f);
        }
        else
        {
            Debug.LogError("AbilityManager: highlightImage�� �Ҵ���� �ʾҽ��ϴ�.");
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

            // ���� �̹����� ���� ���õ� ��ư�� ��ġ�� �̵� �� ũ�� ����
            highlightRect.position = buttonRect.position;

            // ���� �̹����� ũ�⸦ 1.1��� ����
            highlightRect.localScale = Vector3.one * highlightScale;

            // ����Ʈ ������Ʈ
            UpdateHighlightEffect(buttonRect);

            // ���� ���õ� �ɷ��� �̸��� ���� Ȱ��ȭ�ϰ� �������� ��Ȱ��ȭ
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

                // ��ư�� �������� ũ�� ����
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
            Debug.LogError("AbilityManager: ���� �̹����� �ɷ� ��ư�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    private void UpdateHighlightEffect(RectTransform buttonRect)
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
            effectRect.position = buttonRect.position;
            effectRect.localScale = Vector3.one; // �ʿ信 ���� ������ ����

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
            Debug.LogError("AbilityManager: SynergyAbility�� null�Դϴ�.");
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

        // ����Ʈ ����
        if (currentHighlightEffect != null)
        {
            Destroy(currentHighlightEffect);
        }
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

            // ��ư�� �������� �������� �⺻������ �ʱ�ȭ
            ResetButtonAndIconScale(abilityButtons[i], abilityIcons[i]);

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

        // ���� ǥ�� ��ġ �ʱ�ȭ �� ������Ʈ
        currentIndex = 0;
        currentHighlightRotation = 0f;

        // ���̶���Ʈ �̹����� ��ġ�� ���� ��ǥ (0, 45, 0)���� ����
        if (highlightImage != null)
        {
            RectTransform highlightRect = highlightImage.GetComponent<RectTransform>();
            highlightRect.localPosition = new Vector3(0f, 45f, 0f);
        }
        else
        {
            Debug.LogError("AbilityManager: highlightImage�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        UpdateHighlightPosition();
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
