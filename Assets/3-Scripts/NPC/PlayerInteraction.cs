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
        // TODO Ư�� ������ �����Ǵ��� ���θ� �Ǵ��ϴ� �ڵ� (��: Ư�� �������� ȹ���� ���)
        // TODO �� �Լ��� ���� ���� üũ �������� ��ü�Ǿ�� �մϴ�.
        // ����: Ư�� �������� ȹ���ߴ��� Ȯ���ϴ� ����
        // return Inventory.HasItem("SpecialItem");
        return false;
    }

    public void SetDialogueNameToTrigger(string name)
    {
        Debug.Log($"Set dialogue name to trigger: {name}");
        dialogueNameToTrigger = name;

        // ���̾�α׸� ��� Ʈ����
        if (!string.IsNullOrEmpty(dialogueNameToTrigger))
        {
            DialogueManager.Instance.TriggerDialogueByName(dialogueNameToTrigger);
        }
    }
}
