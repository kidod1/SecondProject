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
            Debug.Log($"위와 같은 이름의 다이얼로그를 트리거했습니다.: {name}");
            DialogueManager.Instance.StartDialogue(entry.dialogue, entry.autoClose);

            if (entry.triggerSceneChange)
            {
                StartCoroutine(WaitForDialogueToEnd(entry.nextSceneIndex));
            }
        }
        else
        {
            Debug.LogError($"이름이 {name} 인 대화를 찾을 수 없거나 씬에 DialogueManager이 없습니다.");
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
            Debug.LogError("다음 씬의 인덱스가 유효하지 않습니다. SceneManager에서 이동할 수 없습니다.");
        }
    }
}
