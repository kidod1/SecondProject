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
    private GameObject synergyAbilityPanel;  // 시너지 능력 설명 UI 패널
    [SerializeField]
    private TMP_Text synergyAbilityNameText;  // 시너지 능력 이름 텍스트
    [SerializeField]
    private TMP_Text synergyAbilityDescriptionText;  // 시너지 능력 설명 텍스트
    [SerializeField]
    private Image synergyAbilityIcon;  // 시너지 능력 아이콘 이미지
    [SerializeField]
    private Button synergyAbilityButton;  // 시너지 능력 선택 버튼
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

    private List<Ability> availableAbilities;

    private void OnEnable()
    {
        player.OnLevelUp.AddListener(ShowAbilitySelection);
    }

    private void OnDisable()
    {
        player.OnLevelUp.RemoveListener(ShowAbilitySelection);
    }

    private void Start()
    {
        player.ResetAbilities();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowAbilitySelection();
        }
    }

    public void ShowAbilitySelection()
    {
        Time.timeScale = 0f;
        abilitySelectionPanel.SetActive(true);
        availableAbilities = player.GetAvailableAbilities();
        ShuffleAbilities(); // 능력들을 무작위로 섞음

        int abilitiesToShow = Mathf.Min(abilityButtons.Length, availableAbilities.Count);

        for (int i = 0; i < abilitiesToShow; i++)
        {
            var ability = availableAbilities[i];
            abilityNameTexts[i].text = ability.abilityName;
            abilityDescriptionTexts[i].text = ability.GetDescription();
            abilityIcons[i].sprite = ability.abilityIcon;
            abilityButtons[i].onClick.RemoveAllListeners();
            abilityButtons[i].onClick.AddListener(() => SelectAbility(ability));
            abilityButtons[i].gameObject.SetActive(true);
        }

        for (int i = abilitiesToShow; i < abilityButtons.Length; i++)
        {
            abilityButtons[i].gameObject.SetActive(false);
        }

        rerollButton.gameObject.SetActive(true);
        rerollButton.onClick.RemoveAllListeners();
        rerollButton.onClick.AddListener(RerollAbilities);
    }

    public void ShowSynergyAbility(SynergyAbility synergyAbility)
    {
        // 기존 능력 선택 UI를 숨김
        abilitySelectionPanel.SetActive(false);

        // 시너지 능력 설명 UI를 활성화
        synergyAbilityPanel.SetActive(true);
        synergyAbilityNameText.text = synergyAbility.abilityName;
        synergyAbilityDescriptionText.text = synergyAbility.GetDescription();
        synergyAbilityIcon.sprite = synergyAbility.abilityIcon;
        synergyAbilityButton.onClick.RemoveAllListeners();
        synergyAbilityButton.onClick.AddListener(() => ApplySynergyAbility(synergyAbility));
    }

    public void ApplySynergyAbility(SynergyAbility synergyAbility)
    {
        player.ApplySynergyAbility(synergyAbility);
        synergyAbilityPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void SelectAbility(Ability ability)
    {
        player.SelectAbility(ability);
        abilitySelectionPanel.SetActive(false);
        rerollButton.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private void RerollAbilities()
    {
        Debug.Log("능력 리롤!");
        availableAbilities = player.GetAvailableAbilities();
        ShuffleAbilities();

        int abilitiesToShow = Mathf.Min(abilityButtons.Length, availableAbilities.Count);

        for (int i = 0; i < abilitiesToShow; i++)
        {
            var ability = availableAbilities[i];
            abilityNameTexts[i].text = ability.abilityName;
            abilityDescriptionTexts[i].text = ability.GetDescription();
            abilityIcons[i].sprite = ability.abilityIcon;
            abilityButtons[i].onClick.RemoveAllListeners();
            abilityButtons[i].onClick.AddListener(() => SelectAbility(ability));
            abilityButtons[i].gameObject.SetActive(true);
        }

        for (int i = abilitiesToShow; i < abilityButtons.Length; i++)
        {
            abilityButtons[i].gameObject.SetActive(false);
        }
    }

    private void ShuffleAbilities()
    {
        for (int i = 0; i < availableAbilities.Count; i++)
        {
            var temp = availableAbilities[i];
            int randomIndex = Random.Range(i, availableAbilities.Count);
            availableAbilities[i] = availableAbilities[randomIndex];
            availableAbilities[randomIndex] = temp;
        }
    }
}
