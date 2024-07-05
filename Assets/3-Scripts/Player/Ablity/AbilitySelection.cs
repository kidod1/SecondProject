using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AbilitySelection : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private Button[] abilityButtons;
    [SerializeField]
    private TextMeshProUGUI[] abilityNameTexts;
    [SerializeField]
    private TextMeshProUGUI[] abilityDescriptionTexts;
    [SerializeField]
    private GameObject abilitySelectionCanvas;
    [SerializeField]
    private Button rerollButton; // ���� ��ư �߰�

    private List<Ability> allAbilities;
    private HashSet<string> acquiredAbilities; // ȹ���� �ɷ��� �����ϴ� ����
    private bool canReroll = true; // ���� ���� ����

    private void Start()
    {
        InitializeAllAbilities();
        acquiredAbilities = new HashSet<string>(); // ���� �ʱ�ȭ
        LoadAcquiredAbilities(); // ȹ���� �ɷ� �ε�
        ShuffleAndDisplayAbilities();

        rerollButton.onClick.AddListener(RerollAbilities); // ���� ��ư Ŭ�� �� �̺�Ʈ �߰�
    }

    private void InitializeAllAbilities()
    {
        allAbilities = new List<Ability>
        {
            new IncreasePride(),
            new IncreaseGluttony(),
            new IncreaseGreed(),
            new IncreaseSloth(),
            new IncreaseEnvy(),
            new IncreaseLust(),
            new IncreaseAttack(),
            new IncreaseRange(),
            new IncreaseAttackSpeed(),
            new ShieldOnLowHP(),
            new ReduceMaxHPIncreaseAttack(),
            new ReduceMaxHPAndRefillShield(),
            new IncreaseAttackWithShield(),
            // �ٸ� ��ųƮ���� �ٸ� �ɷµ鵵 �߰�
        };
    }

    public void ShuffleAndDisplayAbilities()
    {
        var availableAbilities = allAbilities.Where(a => !acquiredAbilities.Contains(a.Name)).OrderBy(x => Random.value).Take(3).ToArray();

        for (int i = 0; i < abilityButtons.Length; i++)
        {
            if (i < availableAbilities.Length)
            {
                int index = i;
                var button = abilityButtons[i];

                abilityNameTexts[i].text = availableAbilities[i].Name;
                abilityDescriptionTexts[i].text = availableAbilities[i].Description;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ApplyAbility(availableAbilities[index]));
                button.onClick.AddListener(() => abilitySelectionCanvas.SetActive(false));
            }
            else
            {
                abilityButtons[i].gameObject.SetActive(false);
            }
        }

        canReroll = true;
        rerollButton.gameObject.SetActive(true);
    }

    void ApplyAbility(Ability ability)
    {
        player.AddAbility(ability);
        acquiredAbilities.Add(ability.Name);
        Debug.Log(ability.Name + " �Ϸ�.");
        canReroll = false;
        rerollButton.gameObject.SetActive(false);
        player.SavePlayerData(); // �ɷ� �߰� �� ������ ����
    }

    void RerollAbilities()
    {
        if (canReroll)
        {
            ShuffleAndDisplayAbilities();
            canReroll = false;
            rerollButton.gameObject.SetActive(false);
        }
    }

    // ȹ���� �ɷ��� �ε��ϴ� �޼���
    private void LoadAcquiredAbilities()
    {
        string saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");

        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerDataToJson data = JsonUtility.FromJson<PlayerDataToJson>(json);
            acquiredAbilities = new HashSet<string>(data.acquiredAbilities);
        }
    }
}
