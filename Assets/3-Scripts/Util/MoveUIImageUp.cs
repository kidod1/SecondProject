using UnityEngine;
using UnityEngine.Events; // UnityEvent를 사용하기 위해 추가
using UnityEngine.SceneManagement; // 씬 전환을 위해 추가
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class MoveUIImageUp : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("이미지가 초당 이동할 Y축 거리")]
    [SerializeField]
    private float speed = 50f;

    [Tooltip("이미지가 무한히 이동할지 여부")]
    [SerializeField]
    private bool moveIndefinitely = true;

    [Tooltip("이미지가 멈출 최대 Y 위치 (무한 이동이 아닐 경우 필요)")]
    [SerializeField]
    private float maxY = 1000f;

    [Header("추가 설정")]
    [Tooltip("이동을 시작할 때의 초기 Y 위치")]
    [SerializeField]
    private float startY = 0f;

    [Header("이벤트 설정")]
    [Tooltip("이미지가 목표 지점에 도달했을 때 호출되는 이벤트")]
    [SerializeField]
    private UnityEvent OnReachedMaxY;

    [Header("씬 전환 설정")]
    [Tooltip("목표 지점에 도달한 후 전환할 다음 씬의 이름")]
    [SerializeField]
    private string nextSceneName = "NextScene"; // Inspector에서 설정 가능

    private RectTransform rectTransform;
    private bool hasReachedMaxY = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("MoveUIImageUp 스크립트는 RectTransform 컴포넌트를 필요로 합니다.");
            enabled = false;
            return;
        }

        // 초기 Y 위치 설정
        Vector2 anchoredPos = rectTransform.anchoredPosition;
        anchoredPos.y = startY;
        rectTransform.anchoredPosition = anchoredPos;
        Debug.Log($"MoveUIImageUp Awake: 초기 Y 위치 설정됨. startY = {startY}");
    }

    private void Update()
    {
        if (hasReachedMaxY) return;

        // Y축으로 이동
        float deltaY = speed * Time.deltaTime;
        rectTransform.anchoredPosition += new Vector2(0, deltaY);
        Debug.Log($"MoveUIImageUp Update: 현재 Y 위치 = {rectTransform.anchoredPosition.y}");

        // 이동 제한 설정
        if (!moveIndefinitely && rectTransform.anchoredPosition.y >= maxY)
        {
            hasReachedMaxY = true;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, maxY);
            Debug.Log($"MoveUIImageUp Update: 목표 Y 위치 도달. maxY = {maxY}");
            InvokeReachedMaxYEvent();
        }
    }

    /// <summary>
    /// 목표 지점에 도달했을 때 이벤트를 호출합니다.
    /// </summary>
    private void InvokeReachedMaxYEvent()
    {
        if (OnReachedMaxY != null)
        {
            OnReachedMaxY.Invoke();
            Debug.Log("MoveUIImageUp: OnReachedMaxY 이벤트가 호출되었습니다.");
        }
        else
        {
            Debug.LogWarning("MoveUIImageUp: OnReachedMaxY 이벤트가 할당되지 않았습니다.");
        }

        // 10초 후에 다음 씬으로 전환하는 코루틴 시작
        StartCoroutine(TransitionToNextScene());
    }

    /// <summary>
    /// 10초 대기 후 다음 씬으로 전환합니다.
    /// </summary>
    private IEnumerator TransitionToNextScene()
    {
        Debug.Log("MoveUIImageUp: 10초 후에 다음 씬으로 전환됩니다.");
        yield return new WaitForSeconds(10f);
        Debug.Log($"MoveUIImageUp: 씬 '{nextSceneName}'으로 전환합니다.");

        // 씬 전환 시 해당 씬이 빌드 설정에 포함되어 있는지 확인하세요.
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("MoveUIImageUp: 다음 씬의 이름이 설정되지 않았습니다.");
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// 이동 속도를 설정합니다.
    /// </summary>
    /// <param name="newSpeed">새로운 속도 값</param>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        Debug.Log($"MoveUIImageUp: 이동 속도가 {newSpeed}으로 설정되었습니다.");
    }

    /// <summary>
    /// 이동을 다시 시작합니다.
    /// </summary>
    public void RestartMovement()
    {
        hasReachedMaxY = false;
        Vector2 anchoredPos = rectTransform.anchoredPosition;
        anchoredPos.y = startY;
        rectTransform.anchoredPosition = anchoredPos;
        Debug.Log("MoveUIImageUp: 이동이 재시작되었습니다.");
    }

    private void OnDestroy()
    {
        Debug.Log("MoveUIImageUp: 오브젝트가 파괴되었습니다.");
    }
}
