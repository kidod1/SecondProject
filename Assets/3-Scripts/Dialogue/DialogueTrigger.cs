using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueEntry
{
    public string name;
    public Dialogue dialogue;
    public bool autoClose = false;
    public bool triggerSceneChange = false; 
    public int nextSceneIndex;
}

public class DialogueTrigger : MonoBehaviour
{
    public List<DialogueEntry> dialogues;

    public void TriggerDialogueByName(string name)
    {
        DialogueEntry entry = dialogues.Find(d => d.name == name);
        if (entry != null && DialogueManager.Instance != null)
        {
            Debug.Log($"���� ���� �̸��� ���̾�α׸� Ʈ�����߽��ϴ�.: {name}");
            DialogueManager.Instance.StartDialogue(entry.dialogue, entry.autoClose);

            if (entry.triggerSceneChange)
            {
                StartCoroutine(WaitForDialogueToEnd(entry.nextSceneIndex));
            }
        }
        else
        {
            Debug.LogError($"�̸��� {name} �� ��ȭ�� ã�� �� ���ų� ���� DialogueManager�� �����ϴ�.");
        }
    }

    private IEnumerator WaitForDialogueToEnd(int nextSceneIndex)
    {
        while (DialogueManager.Instance.IsDialogueActive)
        {
            yield return null;
        }

        if (nextSceneIndex >= 0 && nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            PlayManager.I.ChangeScene(nextSceneIndex);
        }
        else
        {
            Debug.LogError("���� ���� �ε����� ��ȿ���� �ʽ��ϴ�. SceneManager���� �̵��� �� �����ϴ�.");
        }
    }
}
