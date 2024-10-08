using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Spine.Unity;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Image loadingBarFill;

    [SerializeField]
    private float loadingDuration = 5f;

    [SerializeField]
    private int nextSceneIndex;

    [SerializeField]
    private GameObject pressSpaceText;

    [SerializeField]
    private GameObject loadingImage;

    [SerializeField]
    private SkeletonGraphic skeletonGraphic;

    [SerializeField]
    private AnimationReferenceAsset idleAnimationAsset;

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

    [Header("Animator Controllers")]
    [SerializeField]
    private Animator fadeInAnimator; // FadeIn 및 Loop 애니메이터

    private bool isLoadingComplete = false;
    private bool hasFadeOutStarted = false;

    private void Start()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            StartCoroutine(FadeIn());
        }

        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 0f;
            StartCoroutine(FillLoadingBar());
        }

        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(false);
        }

        // 트랙 0에서 배경 애니메이션 재생
        if (skeletonGraphic != null && standardAnimationAsset != null && standardAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true);
        }

        // 초기에는 플레이어가 없는 애니메이션을 트랙 1에서 재생
        if (skeletonGraphic != null && deletePlayerAnimationAsset != null && deletePlayerAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(1, deletePlayerAnimationAsset.Animation.Name, true);
        }

        foreach (var resultImage in resultImages)
        {
            if (resultImage != null)
            {
                resultImage.SetActive(false);
            }
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

        // 트랙 1에서 플레이어 있는 애니메이션으로 전환
        if (skeletonGraphic != null && playerAnimationAsset != null && playerAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(1, playerAnimationAsset.Animation.Name, true);
        }

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

        StartCoroutine(WaitForInputAndFadeOut());
    }

    private IEnumerator TriggerLoopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (fadeInAnimator != null)
        {
            fadeInAnimator.SetTrigger("Loop");
        }
    }

    private IEnumerator WaitForInputAndFadeOut()
    {
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        if (fadeImage != null)
        {
            if (fadeInAnimator != null)
            {
                fadeInAnimator.SetTrigger("FadeOut");
                Debug.Log("FadeOut실행");
            }
            yield return StartCoroutine(FadeOut());
            yield return new WaitForSeconds(1f); // 페이드 아웃 완료 후 1초 대기
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    private IEnumerator FadeIn()
    {
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
    }
}
