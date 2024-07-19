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
    private Button[] abilityButtons;
    [SerializeField]
    private TMP_Text[] abilityNameTexts;
    [SerializeField]
    private TMP_Text[] abilityDescriptionTexts;
    [SerializeField]
    private Image[] abilityIcons;
    [SerializeField]
    private Button rerollButton; // 리롤 버튼 추가

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
        // 게임 시작 시 능력 초기화
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
        Time.timeScale = 0f; // 게임 일시정지
        abilitySelectionPanel.SetActive(true);
        availableAbilities = player.GetAvailableAbilities();

        int abilitiesToShow = Mathf.Min(abilityButtons.Length, availableAbilities.Count);

        for (int i = 0; i < abilitiesToShow; i++)
        {
            var ability = availableAbilities[i];
            abilityNameTexts[i].text = ability.abilityName;
            abilityDescriptionTexts[i].text = ability.GetDescription();
            abilityIcons[i].sprite = ability.abilityIcon;
            abilityButtons[i].onClick.RemoveAllListeners();
            abilityButtons[i].onClick.AddListener(() => SelectAbility(ability));
            abilityButtons[i].gameObject.SetActive(true); // 버튼 활성화
        }

        // 나머지 버튼 비활성화
        for (int i = abilitiesToShow; i < abilityButtons.Length; i++)
        {
            abilityButtons[i].gameObject.SetActive(false);
        }

        rerollButton.gameObject.SetActive(true); // 리롤 버튼 활성화
        rerollButton.onClick.RemoveAllListeners();
        rerollButton.onClick.AddListener(RerollAbilities);
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
        Debug.Log("Rerolling abilities...");
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

        // 나머지 버튼 비활성화
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
