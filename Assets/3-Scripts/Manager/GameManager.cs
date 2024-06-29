using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private AbilitySelection abilitySelection;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            abilitySelection.gameObject.SetActive(true);
            abilitySelection.ShuffleAndDisplayAbilities();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            OpenAbilitySelection();
        }
    }
    public void OpenAbilitySelection()
    {
        abilitySelection.gameObject.SetActive(true);
        abilitySelection.ShuffleAndDisplayAbilities();
    }
}
