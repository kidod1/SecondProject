using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int index;
    public Ability ability;
    public AbilityManager abilityManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        abilityManager?.OnAbilityHovered?.Invoke(ability);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        abilityManager?.OnAbilityHovered?.Invoke(ability);
    }
}
