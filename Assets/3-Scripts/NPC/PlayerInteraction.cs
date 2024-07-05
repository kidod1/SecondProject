using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private float interactionRange = 2.0f;
    [SerializeField]
    private LayerMask npcLayer;

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
            if (dialogueTrigger != null)
            {
                dialogueTrigger.TriggerDialogue();
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
}
