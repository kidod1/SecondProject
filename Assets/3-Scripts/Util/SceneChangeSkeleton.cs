using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChangeSkeleton : MonoBehaviour
{
    [Header("Skeleton Graphic Settings")]
    [Tooltip("Spine SkeletonGraphic 컴포넌트")]
    public SkeletonGraphic skeletonGraphic;

    [Tooltip("Close 애니메이션 이름")]
    [SpineAnimation] public string closeAnimationName;

    [Tooltip("Open 애니메이션 이름")]
    [SpineAnimation] public string openAnimationName;

    [Tooltip("씬 전환 딜레이 (초)")]
    public float sceneChangeDelay = 0f;

    [Tooltip("씬 이름")]
    public string targetSceneName;

    // 내부 상태 추적 변수
    private bool isAnimating = false;
    private bool isOpening = false;

    // Close 애니메이션 완료 후 Open 애니메이션 실행을 위한 딜레이
    private float delayAfterClose = 2f;

    void Awake()
    {
        if (skeletonGraphic == null)
        {
            skeletonGraphic = GetComponent<SkeletonGraphic>();
            if (skeletonGraphic == null)
            {
                Debug.LogError("SkeletonGraphic 컴포넌트가 할당되지 않았습니다.");
            }
        }

        // Singleton 패턴 적용 (선택 사항)
        if (FindObjectsOfType<SceneChangeSkeleton>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // 애니메이션 완료 시 이벤트 리스너 등록
        if (skeletonGraphic.AnimationState != null)
        {
            skeletonGraphic.AnimationState.Complete += OnAnimationComplete;
        }
        else
        {
            Debug.LogError("SkeletonGraphic의 AnimationState가 초기화되지 않았습니다.");
        }

        // 씬이 로드될 때 호출되는 이벤트 리스너 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (skeletonGraphic != null && skeletonGraphic.AnimationState != null)
        {
            skeletonGraphic.AnimationState.Complete -= OnAnimationComplete;
        }

        // 씬 로드 이벤트 리스너 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Close 애니메이션을 재생하고 씬을 변경합니다.
    /// </summary>
    /// <param name="sceneName">전환할 씬의 이름</param>
    public void PlayCloseAnimation(string sceneName)
    {
        if (isAnimating)
        {
            Debug.LogWarning("이미 애니메이션이 실행 중입니다.");
            return;
        }

        if (skeletonGraphic == null)
        {
            Debug.LogError("SkeletonGraphic 컴포넌트가 할당되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(closeAnimationName))
        {
            Debug.LogError("Close 애니메이션 이름이 설정되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Target Scene Name이 설정되지 않았습니다.");
            return;
        }

        targetSceneName = sceneName;
        isAnimating = true;
        isOpening = false;
        gameObject.SetActive(true); // 오브젝트 활성화
        skeletonGraphic.AnimationState.SetAnimation(0, closeAnimationName, false);
        Debug.Log($"Close 애니메이션 '{closeAnimationName}' 재생 시작.");
    }

    /// <summary>
    /// Open 애니메이션을 재생합니다.
    /// </summary>
    public void PlayOpenAnimation()
    {
        if (isAnimating)
        {
            Debug.LogWarning("이미 애니메이션이 실행 중입니다.");
            return;
        }

        if (skeletonGraphic == null)
        {
            Debug.LogError("SkeletonGraphic 컴포넌트가 할당되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(openAnimationName))
        {
            Debug.LogError("Open 애니메이션 이름이 설정되지 않았습니다.");
            return;
        }

        isAnimating = true;
        isOpening = true;
        gameObject.SetActive(true); // 오브젝트 활성화
        skeletonGraphic.AnimationState.SetAnimation(0, openAnimationName, false);
        Debug.Log($"Open 애니메이션 '{openAnimationName}' 재생 시작.");
    }

    /// <summary>
    /// 애니메이션 완료 시 호출되는 메서드
    /// </summary>
    /// <param name="trackEntry">완료된 애니메이션 트랙 엔트리</param>
    private void OnAnimationComplete(Spine.TrackEntry trackEntry)
    {
        if (isOpening)
        {
            // Open 애니메이션 완료 시
            StartCoroutine(HandleOpenAnimationComplete());
            Debug.Log("Open 애니메이션 완료. 씬 전환 준비.");
        }
        else
        {
            // Close 애니메이션 완료 시
            StartCoroutine(HandleCloseAnimationComplete());
            Debug.Log("Close 애니메이션 완료. Open 애니메이션 재생을 시작합니다.");
        }
    }

    /// <summary>
    /// Close 애니메이션 완료 후 2초 딜레이 후 Open 애니메이션을 재생합니다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleCloseAnimationComplete()
    {
        // 2초 딜레이
        yield return new WaitForSeconds(delayAfterClose);
        Debug.Log($"2초 딜레이 후 Open 애니메이션 재생 시작.");

        // isAnimating을 false로 설정하여 PlayOpenAnimation이 실행될 수 있도록 함
        isAnimating = false;

        // Open 애니메이션 재생
        PlayOpenAnimation();
    }

    /// <summary>
    /// Open 애니메이션 완료 후 씬 전환 딜레이 후 씬을 변경합니다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleOpenAnimationComplete()
    {
        // 씬 전환 딜레이
        yield return new WaitForSeconds(sceneChangeDelay);
        Debug.Log($"씬 전환 딜레이 후 씬 '{targetSceneName}' 로드 시작.");

        // 씬 전환
        SceneManager.LoadScene(targetSceneName);
    }

    /// <summary>
    /// 씬이 로드된 후 Open 애니메이션을 재생합니다.
    /// </summary>
    /// <param name="scene">로드된 씬</param>
    /// <param name="mode">씬 로드 모드</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 현재 애니메이션이 재생 중이 아니고 Open 애니메이션 이름이 설정되어 있을 때만 실행
        if (!isAnimating && !string.IsNullOrEmpty(openAnimationName))
        {
            Debug.Log($"씬 '{scene.name}' 로드 완료. Open 애니메이션 재생.");
            PlayOpenAnimation();
        }
    }
}
