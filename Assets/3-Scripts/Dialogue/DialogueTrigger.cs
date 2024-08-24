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
            Debug.Log($"위와 같은 이름의 다이얼로그를 트리거했습니다.: {name}");
            DialogueManager.Instance.StartDialogue(entry.dialogue, entry.autoClose);
        }
        else
        {
            Debug.LogError($"이름이 {name} 인 대화를 찾을 수 없거나 씬에 DialogueManager이 없습니다.");
        }
    }
}
