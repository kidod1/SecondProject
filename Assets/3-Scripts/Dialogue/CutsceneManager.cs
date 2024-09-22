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
            public string characterName; // 캐릭터 이름
            public GameObject characterImage; // 캐릭터 이미지 오브젝트
            public int hideAfterSentence; // 이 대사 이후에 캐릭터 이미지를 비활성화할 인덱스
        }

        [TextArea(3, 10)]
        public string[] sentences;
        public DialogueEntry[] dialogueEntries; // 대사와 함께 캐릭터 정보를 저장하는 구조체 배열
        public int[] characterImageTogglePoints; // 특정 대사에서 캐릭터 이미지를 활성화할 인덱스
    }

    public CutsceneDialogue cutsceneDialogue;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image animationImage; // 애니메이션이 시작될 때 true로 설정될 이미지
    public int nextSceneIndex = -1;
    public float textAnimationSpeed = 0.05f; // 텍스트 애니메이션 속도

    private Queue<string> sentences;
    private int currentSentenceIndex = 0;
    private Coroutine textAnimationCoroutine;

    private void Start()
    {
        sentences = new Queue<string>();

        // 시작할 때 모든 캐릭터 이미지를 비활성화
        foreach (var entry in cutsceneDialogue.dialogueEntries)
        {
            if (entry.characterImage != null)
            {
                entry.characterImage.SetActive(false);
            }
        }

        animationImage.gameObject.SetActive(false); // 초기화할 때 애니메이션 이미지를 비활성화
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

        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine); // 이전 애니메이션 중지
        }

        animationImage.gameObject.SetActive(false); // 다음 애니메이션이 실행될 때 이전 이미지를 비활성화

        string sentence = sentences.Dequeue();
        textAnimationCoroutine = StartCoroutine(AnimateText(sentence));
        currentSentenceIndex++;

        ToggleCharacterImagesAndNames();
    }

    private IEnumerator AnimateText(string sentence)
    {
        dialogueText.text = "";
        yield return new WaitForSecondsRealtime(0.1f); // 텍스트 애니메이션이 시작하기 전에 잠깐의 딜레이를 추가

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(textAnimationSpeed);
        }

        animationImage.gameObject.SetActive(true); // 애니메이션이 끝날 때 이미지 활성화
    }

    private void ToggleCharacterImagesAndNames()
    {
        // 특정 인덱스에서 캐릭터 이미지를 활성화
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

        // 특정 인덱스 이후에 캐릭터 이미지를 비활성화
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
            Debug.LogError("다음 씬의 인덱스가 유효하지 않습니다.");
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
