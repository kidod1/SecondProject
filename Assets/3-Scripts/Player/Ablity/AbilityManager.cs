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

    private PlayerAbilityManager playerAbilityManager;
    private List<Ability> availableAbilities;

    private void OnEnable()
    {
        player.OnLevelUp.AddListener(ShowAbilitySelection);
    }

    private void OnDisable()
    {
        player.OnLevelUp.RemoveListener(ShowAbilitySelection);
    }

    public void Initialize(PlayerAbilityManager abilityManager)
    {
        playerAbilityManager = abilityManager;
    }

    public void ShowAbilitySelection()
    {
        Time.timeScale = 0f;
        abilitySelectionPanel.SetActive(true);

        availableAbilities = playerAbilityManager.GetAvailableAbilities();
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

        rerollButton.gameObject.SetActive(true);
        rerollButton.onClick.RemoveAllListeners();
        rerollButton.onClick.AddListener(RerollAbilities);
    }

    public void ShowSynergyAbility(SynergyAbility synergyAbility)
    {
        abilitySelectionPanel.SetActive(false);
        synergyAbilityPanel.SetActive(true);
        synergyAbilityNameText.text = synergyAbility.abilityName;
        synergyAbilityDescriptionText.text = synergyAbility.GetDescription();
        synergyAbilityIcon.sprite = synergyAbility.abilityIcon;
        synergyAbilityButton.onClick.RemoveAllListeners();
        synergyAbilityButton.onClick.AddListener(() => ApplySynergyAbility(synergyAbility));
    }

    private void ApplySynergyAbility(SynergyAbility synergyAbility)
    {
        playerAbilityManager.ApplySynergyAbility(synergyAbility);
        synergyAbilityPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void SelectAbility(Ability ability)
    {
        playerAbilityManager.SelectAbility(ability);
        abilitySelectionPanel.SetActive(false);
        rerollButton.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private void RerollAbilities()
    {
        availableAbilities = playerAbilityManager.GetAvailableAbilities();
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
