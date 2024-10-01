using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 튜토리얼 다이얼로그의 조건 타입을 정의하는 열거형
/// </summary>
public enum ConditionType
{
    None,                // 조건 없음
    PressWASD,           // WASD 키 입력
    PressR,              // R 키 입력
    DefeatAllMonsters    // 모든 몬스터 처치
}

/// <summary>
/// 각 대화 데이터를 저장하는 클래스
/// </summary>
[System.Serializable]
public class DialogueData
{
    [TextArea(3, 10)]
    public string sentence; // 대사 내용
    public bool isConditional; // 조건부 대사인지 여부
    public ConditionType conditionType; // 조건 타입
    public GameObject associatedImage; // 대사와 함께 보여줄 이미지 (옵션)
}

/// <summary>
/// 튜토리얼 다이얼로그를 관리하는 매니저 클래스
/// </summary>
public class TutorialDialogueManager : MonoBehaviour
{
    [Header("대화 목록")]
    public List<DialogueData> dialogues; // 대사 목록

    [Header("UI 요소")]
    public TMP_Text dialogueText; // 대사 텍스트 UI
    public Image dialogueImage; // 대사 이미지 UI

    [Header("애니메이션 및 이미지 설정")]
    public float textAnimationSpeed = 0.05f; // 텍스트 애니메이션 속도
    public float conditionTimeout = 10f; // 조건 충족 대기 시간
    public float autoAdvanceDuration = 3f; // 일반 대사 자동 진행 시간

    private int currentDialogueIndex = 0; // 현재 대사 인덱스
    private Coroutine textAnimationCoroutine; // 텍스트 애니메이션 코루틴
    private Coroutine conditionCoroutine; // 조건 대기 코루틴

    private bool conditionMet = false; // 조건 충족 여부

    // 플레이어 입력 및 상태 체크를 위한 변수들
    private bool hasMoved = false; // 플레이어가 WASD를 눌렀는지
    private bool hasUsedAbility = false; // 플레이어가 R 키를 눌러 능력을 사용했는지
    private bool allMonstersDefeated = false; // 모든 몬스터를 물리쳤는지

    private void Start()
    {
        // 시작 시 첫 번째 대사를 출력
        DisplayNextDialogue();
    }

    private void Update()
    {
        // 플레이어의 움직임 체크 (WASD 입력)
        if (!hasMoved && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                          Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)))
        {
            hasMoved = true;
        }

        // 플레이어의 능력 사용 체크 (R 키 입력)
        if (!hasUsedAbility && Input.GetKeyDown(KeyCode.R))
        {
            hasUsedAbility = true;
        }

        // 몬스터 처치 여부 체크
        if (!allMonstersDefeated && AreAllMonstersDefeated())
        {
            allMonstersDefeated = true;
        }
    }

    /// <summary>
    /// 씬 내의 모든 몬스터가 처치되었는지 확인하는 메서드
    /// </summary>
    /// <returns>모든 몬스터가 처치되었으면 true, 아니면 false</returns>
    private bool AreAllMonstersDefeated()
    {
        Monster[] monsters = FindObjectsOfType<Monster>();
        return monsters.Length == 0;
    }

    /// <summary>
    /// 다음 대사를 화면에 표시하는 메서드
    /// </summary>
    public void DisplayNextDialogue()
    {
        if (currentDialogueIndex >= dialogues.Count)
        {
            EndDialogue();
            return;
        }

        DialogueData dialogue = dialogues[currentDialogueIndex];

        // 이전 텍스트 애니메이션 중지
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }

        // 대사와 이미지 설정
        if (dialogueImage != null)
        {
            dialogueImage.sprite = dialogue.associatedImage != null ? dialogue.associatedImage.GetComponent<SpriteRenderer>().sprite : null;
            dialogueImage.gameObject.SetActive(dialogue.associatedImage != null);
        }

        // 텍스트 애니메이션 시작
        textAnimationCoroutine = StartCoroutine(AnimateText(dialogue.sentence));

        if (dialogue.isConditional)
        {
            // 조건부 대사의 경우 조건 대기 코루틴 시작
            if (conditionCoroutine != null)
            {
                StopCoroutine(conditionCoroutine);
            }
            conditionCoroutine = StartCoroutine(WaitForCondition(dialogue.conditionType));
        }
        else
        {
            // 일반 대사의 경우 자동 진행 코루틴 시작
            StartCoroutine(WaitForAutoAdvance());
        }
    }

    /// <summary>
    /// 텍스트를 한 글자씩 표시하는 애니메이션 코루틴
    /// </summary>
    /// <param name="sentence">표시할 대사</param>
    /// <returns></returns>
    private IEnumerator AnimateText(string sentence)
    {
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(textAnimationSpeed);
        }
    }

    /// <summary>
    /// 일반 대사를 일정 시간 후 자동으로 다음 대사로 진행하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForAutoAdvance()
    {
        yield return new WaitForSecondsRealtime(autoAdvanceDuration);
        currentDialogueIndex++;
        DisplayNextDialogue();
    }

    /// <summary>
    /// 조건부 대사를 위한 조건 충족을 기다리는 코루틴
    /// </summary>
    /// <param name="conditionType">대사의 조건 타입</param>
    /// <returns></returns>
    private IEnumerator WaitForCondition(ConditionType conditionType)
    {
        float elapsedTime = 0f;
        conditionMet = false;

        while (elapsedTime < conditionTimeout)
        {
            if (CheckConditionMet(conditionType))
            {
                conditionMet = true;
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (conditionMet)
        {
            currentDialogueIndex++;
            DisplayNextDialogue();
        }
        else
        {
            // 조건 미충족 시 대사와 이미지 재출력
            DisplayNextDialogue();
        }
    }

    /// <summary>
    /// 주어진 조건 타입이 충족되었는지 확인하는 메서드
    /// </summary>
    /// <param name="conditionType">확인할 조건 타입</param>
    /// <returns>조건이 충족되었으면 true, 아니면 false</returns>
    private bool CheckConditionMet(ConditionType conditionType)
    {
        switch (conditionType)
        {
            case ConditionType.PressWASD:
                return hasMoved;
            case ConditionType.PressR:
                return hasUsedAbility;
            case ConditionType.DefeatAllMonsters:
                return allMonstersDefeated;
            default:
                return false;
        }
    }

    /// <summary>
    /// 대화가 모두 끝났을 때 호출되는 메서드
    /// </summary>
    private void EndDialogue()
    {
        // 튜토리얼 종료 또는 다음 씬으로 이동 등의 처리
        Debug.Log("튜토리얼 종료");
        // 예: SceneManager.LoadScene("NextSceneName");
    }
}
