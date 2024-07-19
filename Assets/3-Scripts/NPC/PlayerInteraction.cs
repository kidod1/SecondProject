using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private float interactionRange = 2.0f;
    [SerializeField]
    private LayerMask npcLayer;

    private string dialogueNameToTrigger;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

        if (SomeConditionMet())
        {
            Interact();
        }
    }

    private void Interact()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, npcLayer);

        foreach (var hit in hits)
        {
            DialogueTrigger dialogueTrigger = hit.GetComponent<DialogueTrigger>();
            if (dialogueTrigger != null && !string.IsNullOrEmpty(dialogueNameToTrigger))
            {
                Debug.Log($"Attempting to trigger dialogue: {dialogueNameToTrigger}");
                dialogueTrigger.TriggerDialogueByName(dialogueNameToTrigger);
                break;
            }
        }
    }

    private bool SomeConditionMet()
    {
        // TODO 특정 조건이 충족되는지 여부를 판단하는 코드 (예: 특정 아이템을 획득한 경우)
        // TODO 이 함수는 실제 조건 체크 로직으로 대체되어야 합니다.
        // 예시: 특정 아이템을 획득했는지 확인하는 로직
        // return Inventory.HasItem("SpecialItem");
        return false;
    }

    public void SetDialogueNameToTrigger(string name)
    {
        Debug.Log($"Set dialogue name to trigger: {name}");
        dialogueNameToTrigger = name;

        // 다이얼로그를 즉시 트리거
        if (!string.IsNullOrEmpty(dialogueNameToTrigger))
        {
            DialogueManager.Instance.TriggerDialogueByName(dialogueNameToTrigger);
        }
    }
}
