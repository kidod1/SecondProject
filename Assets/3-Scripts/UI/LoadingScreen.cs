using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Spine.Unity;
using Spine;
using Cinemachine;
using AK.Wwise; // WWISE 네임스페이스 추가
using UnityEngine.Events; // UnityEvent 네임스페이스 추가

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Image loadingBarFill;

    [SerializeField]
    private float loadingDuration = 5f;

    [SerializeField]
    private int nextSceneIndex; // 다음 씬의 인덱스

    [SerializeField]
    private GameObject pressSpaceText;

    [SerializeField]
    private GameObject loadingImage;

    [SerializeField]
    private SkeletonGraphic skeletonGraphic;

    [SerializeField]
    private AnimationReferenceAsset completeAnimationAsset;

    [SerializeField]
    private AnimationReferenceAsset standardAnimationAsset; // 배경 애니메이션 자산

    [SerializeField]
    private AnimationReferenceAsset deletePlayerAnimationAsset; // 플레이어 없는 애니메이션 자산

    [SerializeField]
    private AnimationReferenceAsset playerAnimationAsset; // 플레이어 있는 애니메이션 자산

    [SerializeField]
    private GameObject[] resultImages;

    [SerializeField]
    private Image fadeImage;

    [SerializeField]
    private float fadeDuration = 1f;

    [SerializeField]
    private SceneChangeSkeleton sceneChanageSkeleton;

    [Header("Animator Controllers")]
    [SerializeField]
    private Animator fadeInAnimator; // FadeIn 및 Loop 애니메이터

    [Header("WWISE Events")]
    [SerializeField]
    private AK.Wwise.Event loadSceneEvent; // WWISE 이벤트 변수 추가

    [Header("Unity Events")]
    public UnityEvent beforeSceneLoadEvent; // 씬 전환 전에 호출될 이벤트
    public UnityEvent onFadeOutComplete;    // 페이드 아웃 완료 시 호출될 이벤트
    public UnityEvent onFadeInStart;        // 페이드 인 시작 시 호출될 이벤트

    private bool isLoadingComplete = false;
    private bool hasFadeOutStarted = false;

    private void Start()
    {
        // 초기 페이드 인 설정
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            StartCoroutine(FadeIn());
        }

        // 로딩 바 초기화 및 채우기 시작
        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 0f;
            StartCoroutine(FillLoadingBar());
        }

        // "Press Space" 텍스트 비활성화
        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(false);
        }

        // 트랙 0: 배경 애니메이션 재생
        if (skeletonGraphic != null && standardAnimationAsset != null && standardAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true);
        }
        // 트랙 1: 초기에는 플레이어 없는 애니메이션 재생
        if (skeletonGraphic != null && deletePlayerAnimationAsset != null && deletePlayerAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(1, deletePlayerAnimationAsset.Animation.Name, true);
        }

        // 결과 이미지 비활성화
        foreach (var resultImage in resultImages)
        {
            if (resultImage != null)
            {
                resultImage.SetActive(false);
            }
        }

        // Spine 애니메이션 이벤트 구독
        if (skeletonGraphic != null)
        {
            skeletonGraphic.AnimationState.Event += OnAnimationEvent;
        }
    }

    private void OnDestroy()
    {
        // Spine 애니메이션 이벤트 구독 해제
        if (skeletonGraphic != null)
        {
            skeletonGraphic.AnimationState.Event -= OnAnimationEvent;
        }
    }

    private IEnumerator FillLoadingBar()
    {
        float elapsedTime = 0f;

        while (elapsedTime < loadingDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float fillAmount = Mathf.Clamp01(elapsedTime / loadingDuration);
            loadingBarFill.fillAmount = fillAmount;

            if (!hasFadeOutStarted && elapsedTime >= (loadingDuration - 0.5f))
            {
                hasFadeOutStarted = true;
                StartCoroutine(FadeOut());
            }

            yield return null;
        }

        isLoadingComplete = true;

        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(true);
        }

        if (loadingImage != null)
        {
            loadingImage.SetActive(false);
        }

        // 트랙 1: 플레이어 있는 애니메이션으로 전환
        if (skeletonGraphic != null && playerAnimationAsset != null && playerAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.ClearTracks(); // 애니메이션 설정 전에 트랙을 클리어
            skeletonGraphic.Initialize(true);
            skeletonGraphic.AnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true);
            skeletonGraphic.AnimationState.SetAnimation(1, playerAnimationAsset.Animation.Name, true);
            Debug.Log(skeletonGraphic.AnimationState);
        }

        // 결과 이미지 활성화
        foreach (var resultImage in resultImages)
        {
            if (resultImage != null)
            {
                resultImage.SetActive(true);
            }
        }

        // FadeIn 애니메이터 실행
        if (fadeInAnimator != null)
        {
            fadeInAnimator.SetTrigger("FadeIn");
            StartCoroutine(TriggerLoopAfterDelay(0.1f));
        }
        else
        {
            Debug.LogWarning("FadeInAnimator가 할당되지 않았습니다.");
        }

        StartCoroutine(FadeIn());

        StartCoroutine(WaitForInputAndLoadNextScene());
    }

    private IEnumerator TriggerLoopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (fadeInAnimator != null)
        {
            fadeInAnimator.SetTrigger("Loop");
        }
    }

    private IEnumerator WaitForInputAndLoadNextScene()
    {
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        // **씬 전환 전에 UnityEvent 호출**
        if (beforeSceneLoadEvent != null)
        {
            beforeSceneLoadEvent.Invoke();
            Debug.Log("beforeSceneLoadEvent가 호출되었습니다.");
        }

        // WWISE 이벤트 실행
        if (loadSceneEvent != null)
        {
            loadSceneEvent.Post(gameObject);
            Debug.Log("WWISE 이벤트가 실행되었습니다.");
        }
        else
        {
            Debug.LogWarning("loadSceneEvent가 할당되지 않았습니다.");
        }

        if (fadeImage != null)
        {
            if (fadeInAnimator != null)
            {
                fadeInAnimator.SetTrigger("FadeOut");
                Debug.Log("FadeOut 실행");
            }
            yield return StartCoroutine(FadeOut());
            yield return new WaitForSeconds(1f); // 페이드 아웃 완료 후 1초 대기
        }

        PlayManager.I.ChangeScene(nextSceneIndex);
        PlayManager.I.StopAllSounds();
    }

    private IEnumerator FadeIn()
    {
        // 페이드 인 시작 시 이벤트 호출
        onFadeInStart?.Invoke();

        float elapsedTime = 0f;
        Color c = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color c = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;

        // 페이드 아웃 완료 후 이벤트 호출
        onFadeOutComplete?.Invoke();
    }

    /// <summary>
    /// Spine 애니메이션 이벤트 처리
    /// </summary>
    /// <param name="trackEntry">애니메이션 트랙 엔트리</param>
    /// <param name="e">Spine 이벤트</param>
    private void OnAnimationEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == "Reload")
        {
            Debug.Log("Spine Reload 이벤트 발생!");
            ReloadFunction();
        }
    }

    /// <summary>
    /// Reload 이벤트 처리 함수
    /// </summary>
    private void ReloadFunction()
    {
        // Reload 버튼을 누르는 동작 구현
        // 예시: 특정 함수 호출 또는 씬 리로드
        Debug.Log("ReloadFunction이 호출되었습니다.");
    }
}
