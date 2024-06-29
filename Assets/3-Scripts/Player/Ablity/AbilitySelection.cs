using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class AbilitySelection : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private Button[] abilityButtons; // 3���� ��ư
    [SerializeField]
    private TextMeshProUGUI[] abilityNameTexts; // 3���� �ɷ� �̸� �ؽ�Ʈ
    [SerializeField]
    private TextMeshProUGUI[] abilityDescriptionTexts; // 3���� �ɷ� ���� �ؽ�Ʈ
    [SerializeField]
    private GameObject abilitySelectionCanvas;

    private List<Ability> allAbilities;
    private HashSet<string> acquiredAbilities; // ȹ���� �ɷ��� �����ϴ� ����

    private void Start()
    {
        InitializeAllAbilities();
        acquiredAbilities = new HashSet<string>(); // ���� �ʱ�ȭ
        ShuffleAndDisplayAbilities();
    }

    // ��� �ɷ� �ʱ�ȭ
    private void InitializeAllAbilities()
    {
        allAbilities = new List<Ability>
        {
            new IncreasePride(),
            new IncreaseWrath(),
            new IncreaseGluttony(),
            new IncreaseGreed(),
            new IncreaseSloth(),
            new IncreaseEnvy(),
            new IncreaseLust(),
            new IncreaseSuperWrath(),
            new IncreaseUltraWrath(),
            new IncreaseAttack(),
            new IncreaseRange(),
            new IncreaseAttackSpeed()
            // �ٸ� ��ųƮ���� �ٸ� �ɷµ鵵 �߰�
        };
    }

    // �ɷ��� �������� �����ϰ� UI�� ǥ��
    public void ShuffleAndDisplayAbilities()
    {
        // �̹� ȹ���� �ɷ��� ������ �ɷ��� ����
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
    }

    void ApplyAbility(Ability ability)
    {
        player.AddAbility(ability);
        acquiredAbilities.Add(ability.Name); // ȹ���� �ɷ� �̸��� �߰�
        Debug.Log(ability.Name + " �Ϸ�.");
    }
}
