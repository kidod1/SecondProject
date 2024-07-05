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
    private Button rerollButton; // 리롤 버튼 추가

    private List<Ability> allAbilities;
    private HashSet<string> acquiredAbilities; // 획득한 능력을 추적하는 집합
    private bool canReroll = true; // 리롤 가능 여부

    private void Start()
    {
        InitializeAllAbilities();
        acquiredAbilities = new HashSet<string>(); // 집합 초기화
        LoadAcquiredAbilities(); // 획득한 능력 로드
        ShuffleAndDisplayAbilities();

        rerollButton.onClick.AddListener(RerollAbilities); // 리롤 버튼 클릭 시 이벤트 추가
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
            // 다른 스킬트리의 다른 능력들도 추가
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
        Debug.Log(ability.Name + " 완료.");
        canReroll = false;
        rerollButton.gameObject.SetActive(false);
        player.SavePlayerData(); // 능력 추가 후 데이터 저장
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

    // 획득한 능력을 로드하는 메서드
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
