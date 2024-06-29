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
    private Button[] abilityButtons; // 3개의 버튼
    [SerializeField]
    private TextMeshProUGUI[] abilityNameTexts; // 3개의 능력 이름 텍스트
    [SerializeField]
    private TextMeshProUGUI[] abilityDescriptionTexts; // 3개의 능력 설명 텍스트
    [SerializeField]
    private GameObject abilitySelectionCanvas;

    private List<Ability> allAbilities;
    private HashSet<string> acquiredAbilities; // 획득한 능력을 추적하는 집합

    private void Start()
    {
        InitializeAllAbilities();
        acquiredAbilities = new HashSet<string>(); // 집합 초기화
        ShuffleAndDisplayAbilities();
    }

    // 모든 능력 초기화
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
            // 다른 스킬트리의 다른 능력들도 추가
        };
    }

    // 능력을 랜덤으로 셔플하고 UI에 표시
    public void ShuffleAndDisplayAbilities()
    {
        // 이미 획득한 능력을 제외한 능력을 셔플
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
        acquiredAbilities.Add(ability.Name); // 획득한 능력 이름을 추가
        Debug.Log(ability.Name + " 완료.");
    }
}
