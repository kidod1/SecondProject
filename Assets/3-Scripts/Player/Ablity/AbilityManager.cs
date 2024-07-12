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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowAbilitySelection();
        }
    }

    public void ShowAbilitySelection()
    {
        abilitySelectionPanel.SetActive(true);
        availableAbilities = player.GetAvailableAbilities();

        for (int i = 0; i < abilityButtons.Length; i++)
        {
            if (i < availableAbilities.Count)
            {
                var ability = availableAbilities[i];
                abilityNameTexts[i].text = ability.abilityName;
                abilityDescriptionTexts[i].text = ability.GetDescription();
                abilityButtons[i].onClick.RemoveAllListeners();
                abilityButtons[i].onClick.AddListener(() => SelectAbility(ability));
                abilityButtons[i].gameObject.SetActive(true); // 버튼 활성화
            }
            else
            {
                abilityButtons[i].gameObject.SetActive(false);
            }
        }

        rerollButton.gameObject.SetActive(true); // 리롤 버튼 활성화
        rerollButton.onClick.RemoveAllListeners();
        rerollButton.onClick.AddListener(RerollAbilities);
    }

    public void SelectAbility(Ability ability)
    {
        player.SelectAbility(ability);
        abilitySelectionPanel.SetActive(false);
        rerollButton.gameObject.SetActive(false); // 리롤 버튼 비활성화
    }

    private void RerollAbilities()
    {
        Debug.Log("Rerolling abilities...");
        availableAbilities = player.GetAvailableAbilities(); // 가능한 능력 다시 가져오기
        ShuffleAbilities(); // 리스트 섞기

        for (int i = 0; i < abilityButtons.Length; i++)
        {
            if (i < availableAbilities.Count)
            {
                var ability = availableAbilities[i];
                abilityNameTexts[i].text = ability.abilityName;
                abilityDescriptionTexts[i].text = ability.GetDescription();
                abilityButtons[i].onClick.RemoveAllListeners();
                abilityButtons[i].onClick.AddListener(() => SelectAbility(ability));
                abilityButtons[i].gameObject.SetActive(true); // 버튼 활성화
            }
            else
            {
                abilityButtons[i].gameObject.SetActive(false);
            }
        }

        rerollButton.gameObject.SetActive(false); // 리롤 버튼 비활성화
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
