using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Spine.Unity;
using Spine;
using Cinemachine;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�
using UnityEngine.Events; // UnityEvent ���ӽ����̽� �߰�

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Image loadingBarFill;

    [SerializeField]
    private float loadingDuration = 5f;

    [SerializeField]
    private int nextSceneIndex; // ���� ���� �ε���

    [SerializeField]
    private GameObject pressSpaceText;

    [SerializeField]
    private GameObject loadingImage;

    [SerializeField]
    private SkeletonGraphic skeletonGraphic;

    [SerializeField]
    private AnimationReferenceAsset completeAnimationAsset;

    [SerializeField]
    private AnimationReferenceAsset standardAnimationAsset; // ��� �ִϸ��̼� �ڻ�

    [SerializeField]
    private AnimationReferenceAsset deletePlayerAnimationAsset; // �÷��̾� ���� �ִϸ��̼� �ڻ�

    [SerializeField]
    private AnimationReferenceAsset playerAnimationAsset; // �÷��̾� �ִ� �ִϸ��̼� �ڻ�

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
    private Animator fadeInAnimator; // FadeIn �� Loop �ִϸ�����

    [Header("WWISE Events")]
    [SerializeField]
    private AK.Wwise.Event loadSceneEvent; // WWISE �̺�Ʈ ���� �߰�

    [Header("Unity Events")]
    public UnityEvent beforeSceneLoadEvent; // �� ��ȯ ���� ȣ��� �̺�Ʈ
    public UnityEvent onFadeOutComplete;    // ���̵� �ƿ� �Ϸ� �� ȣ��� �̺�Ʈ
    public UnityEvent onFadeInStart;        // ���̵� �� ���� �� ȣ��� �̺�Ʈ

    private bool isLoadingComplete = false;
    private bool hasFadeOutStarted = false;

    private void Start()
    {
        // �ʱ� ���̵� �� ����
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            StartCoroutine(FadeIn());
        }

        // �ε� �� �ʱ�ȭ �� ä��� ����
        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 0f;
            StartCoroutine(FillLoadingBar());
        }

        // "Press Space" �ؽ�Ʈ ��Ȱ��ȭ
        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(false);
        }

        // Ʈ�� 0: ��� �ִϸ��̼� ���
        if (skeletonGraphic != null && standardAnimationAsset != null && standardAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true);
        }
        // Ʈ�� 1: �ʱ⿡�� �÷��̾� ���� �ִϸ��̼� ���
        if (skeletonGraphic != null && deletePlayerAnimationAsset != null && deletePlayerAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(1, deletePlayerAnimationAsset.Animation.Name, true);
        }

        // ��� �̹��� ��Ȱ��ȭ
        foreach (var resultImage in resultImages)
        {
            if (resultImage != null)
            {
                resultImage.SetActive(false);
            }
        }

        // Spine �ִϸ��̼� �̺�Ʈ ����
        if (skeletonGraphic != null)
        {
            skeletonGraphic.AnimationState.Event += OnAnimationEvent;
        }
    }

    private void OnDestroy()
    {
        // Spine �ִϸ��̼� �̺�Ʈ ���� ����
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

        // Ʈ�� 1: �÷��̾� �ִ� �ִϸ��̼����� ��ȯ
        if (skeletonGraphic != null && playerAnimationAsset != null && playerAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.ClearTracks(); // �ִϸ��̼� ���� ���� Ʈ���� Ŭ����
            skeletonGraphic.Initialize(true);
            skeletonGraphic.AnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true);
            skeletonGraphic.AnimationState.SetAnimation(1, playerAnimationAsset.Animation.Name, true);
            Debug.Log(skeletonGraphic.AnimationState);
        }

        // ��� �̹��� Ȱ��ȭ
        foreach (var resultImage in resultImages)
        {
            if (resultImage != null)
            {
                resultImage.SetActive(true);
            }
        }

        // FadeIn �ִϸ����� ����
        if (fadeInAnimator != null)
        {
            fadeInAnimator.SetTrigger("FadeIn");
            StartCoroutine(TriggerLoopAfterDelay(0.1f));
        }
        else
        {
            Debug.LogWarning("FadeInAnimator�� �Ҵ���� �ʾҽ��ϴ�.");
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

        // **�� ��ȯ ���� UnityEvent ȣ��**
        if (beforeSceneLoadEvent != null)
        {
            beforeSceneLoadEvent.Invoke();
            Debug.Log("beforeSceneLoadEvent�� ȣ��Ǿ����ϴ�.");
        }

        // WWISE �̺�Ʈ ����
        if (loadSceneEvent != null)
        {
            loadSceneEvent.Post(gameObject);
            Debug.Log("WWISE �̺�Ʈ�� ����Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("loadSceneEvent�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (fadeImage != null)
        {
            if (fadeInAnimator != null)
            {
                fadeInAnimator.SetTrigger("FadeOut");
                Debug.Log("FadeOut ����");
            }
            yield return StartCoroutine(FadeOut());
            yield return new WaitForSeconds(1f); // ���̵� �ƿ� �Ϸ� �� 1�� ���
        }

        PlayManager.I.ChangeScene(nextSceneIndex);
        PlayManager.I.StopAllSounds();
    }

    private IEnumerator FadeIn()
    {
        // ���̵� �� ���� �� �̺�Ʈ ȣ��
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

        // ���̵� �ƿ� �Ϸ� �� �̺�Ʈ ȣ��
        onFadeOutComplete?.Invoke();
    }

    /// <summary>
    /// Spine �ִϸ��̼� �̺�Ʈ ó��
    /// </summary>
    /// <param name="trackEntry">�ִϸ��̼� Ʈ�� ��Ʈ��</param>
    /// <param name="e">Spine �̺�Ʈ</param>
    private void OnAnimationEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == "Reload")
        {
            Debug.Log("Spine Reload �̺�Ʈ �߻�!");
            ReloadFunction();
        }
    }

    /// <summary>
    /// Reload �̺�Ʈ ó�� �Լ�
    /// </summary>
    private void ReloadFunction()
    {
        // Reload ��ư�� ������ ���� ����
        // ����: Ư�� �Լ� ȣ�� �Ǵ� �� ���ε�
        Debug.Log("ReloadFunction�� ȣ��Ǿ����ϴ�.");
    }
}
