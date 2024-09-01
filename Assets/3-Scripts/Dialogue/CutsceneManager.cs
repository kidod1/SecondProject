using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneDialogue
    {
        [System.Serializable]
        public struct DialogueEntry
        {
            public string characterName; // ĳ���� �̸�
            public GameObject characterImage; // ĳ���� �̹��� ������Ʈ
            public int hideAfterSentence; // �� ��� ���Ŀ� ĳ���� �̹����� ��Ȱ��ȭ�� �ε���
        }

        [TextArea(3, 10)]
        public string[] sentences;
        public DialogueEntry[] dialogueEntries; // ���� �Բ� ĳ���� ������ �����ϴ� ����ü �迭
        public int[] characterImageTogglePoints; // Ư�� ��翡�� ĳ���� �̹����� Ȱ��ȭ�� �ε���
    }

    public CutsceneDialogue cutsceneDialogue;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public int nextSceneIndex = -1;

    private Queue<string> sentences;
    private int currentSentenceIndex = 0;

    private void Start()
    {
        sentences = new Queue<string>();

        // ������ �� ��� ĳ���� �̹����� ��Ȱ��ȭ
        foreach (var entry in cutsceneDialogue.dialogueEntries)
        {
            if (entry.characterImage != null)
            {
                entry.characterImage.SetActive(false);
            }
        }

        StartCutscene();
    }

    public void StartCutscene()
    {
        sentences.Clear();

        foreach (string sentence in cutsceneDialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndCutscene();
            return;
        }

        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;
        currentSentenceIndex++;

        ToggleCharacterImagesAndNames();
    }

    private void ToggleCharacterImagesAndNames()
    {
        // Ư�� �ε������� ĳ���� �̹����� Ȱ��ȭ
        for (int i = 0; i < cutsceneDialogue.characterImageTogglePoints.Length; i++)
        {
            if (cutsceneDialogue.characterImageTogglePoints[i] == currentSentenceIndex)
            {
                if (i < cutsceneDialogue.dialogueEntries.Length)
                {
                    var entry = cutsceneDialogue.dialogueEntries[i];
                    if (entry.characterImage != null)
                    {
                        entry.characterImage.SetActive(true);
                    }
                    if (!string.IsNullOrEmpty(entry.characterName))
                    {
                        nameText.text = entry.characterName;
                    }
                }
            }
        }

        // Ư�� �ε��� ���Ŀ� ĳ���� �̹����� ��Ȱ��ȭ
        foreach (var entry in cutsceneDialogue.dialogueEntries)
        {
            if (entry.characterImage != null && currentSentenceIndex > entry.hideAfterSentence)
            {
                entry.characterImage.SetActive(false);
            }
        }
    }

    private void EndCutscene()
    {
        if (nextSceneIndex >= 0 && nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogError("���� ���� �ε����� ��ȿ���� �ʽ��ϴ�.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextSentence();
        }
    }
}
