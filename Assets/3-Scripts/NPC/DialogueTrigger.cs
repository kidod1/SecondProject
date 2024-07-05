using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public bool autoClose = false; // �ڵ� ���� ����

    public void TriggerDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue, autoClose);
        }
        else
        {
            Debug.LogError("DialogueManager not found in the scene.");
        }
    }
}
