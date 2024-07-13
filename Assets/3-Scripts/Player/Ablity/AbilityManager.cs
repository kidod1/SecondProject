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
    private Button rerollButton; // ���� ��ư �߰�

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
        // ���� ���� �� �ɷ� �ʱ�ȭ
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
        Time.timeScale = 0f; // ���� �Ͻ�����
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
                abilityButtons[i].gameObject.SetActive(true); // ��ư Ȱ��ȭ
            }
            else
            {
                abilityButtons[i].gameObject.SetActive(false);
            }
        }

        rerollButton.gameObject.SetActive(true); // ���� ��ư Ȱ��ȭ
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

        for (int i = 0; i < abilityButtons.Length; i++)
        {
            if (i < availableAbilities.Count)
            {
                var ability = availableAbilities[i];
                abilityNameTexts[i].text = ability.abilityName;
                abilityDescriptionTexts[i].text = ability.GetDescription();
                abilityButtons[i].onClick.RemoveAllListeners();
                abilityButtons[i].onClick.AddListener(() => SelectAbility(ability));
                abilityButtons[i].gameObject.SetActive(true);
            }
            else
            {
                abilityButtons[i].gameObject.SetActive(false);
            }
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
