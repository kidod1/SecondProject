using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    public string name;
    public Dialogue dialogue;
    public bool autoClose = false;
}

public class DialogueTrigger : MonoBehaviour
{
    public List<DialogueEntry> dialogues;

    public void TriggerDialogueByName(string name)
    {
        DialogueEntry entry = dialogues.Find(d => d.name == name);
        if (entry != null && DialogueManager.Instance != null)
        {
            Debug.Log($"Triggering dialogue by name: {name}");
            DialogueManager.Instance.StartDialogue(entry.dialogue, entry.autoClose);
        }
        else
        {
            Debug.LogError($"Dialogue with name {name} not found or DialogueManager not found in the scene.");
        }
    }
}
