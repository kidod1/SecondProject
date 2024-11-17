using UnityEngine;
using Spine.Unity;
using System.Collections;
using Spine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LustCutSceneAnimationController : MonoBehaviour
{
    [SpineAnimation] public string fadeInAnimation;    // Fade_In �ִϸ��̼�
    [SpineAnimation] public string animation1;         // Animation1
    [SpineAnimation] public string fadeOutAnimation;   // Fade_Out �ִϸ��̼�
    [SpineAnimation] public string animation2;         // Animation2

    [SerializeField]
    private SkeletonGraphic skeletonGraphic; // UI�� SkeletonGraphic ������Ʈ
    private Spine.AnimationState spineAnimationState;
    private LustCutSceneManager cutsceneManager;

    private bool hasFadedOut = false;

    // UnityEvent �߰�: �ִϸ��̼� �Ϸ� �� ȣ���
    [SerializeField]
    private UnityEvent onCutsceneEnd;

    void Start()
    {
        // SkeletonGraphic ������Ʈ�� �����ɴϴ�.
        skeletonGraphic = GetComponent<SkeletonGraphic>();
        if (skeletonGraphic == null)
        {
            Debug.LogError("SkeletonGraphic ������Ʈ�� ã�� �� �����ϴ�.");
            return;
        }

        spineAnimationState = skeletonGraphic.AnimationState;

        // CutsceneManager�� ã�Ƽ� ����
        cutsceneManager = FindObjectOfType<LustCutSceneManager>();
        if (cutsceneManager == null)
        {
            Debug.LogError("CutsceneManager�� ã�� �� �����ϴ�.");
        }

        // Animation1�� ��� ���� (�ݺ� ����)
        spineAnimationState.SetAnimation(0, animation1, false).MixDuration = 0.5f;

        // Coroutine�� �����Ͽ� 2�� �Ŀ� Fade_In�� ����
        StartCoroutine(PlayFadeInAfterDelay(2f));
    }

    // Fade_In �ִϸ��̼��� ���� �� �����ϴ� Coroutine
    IEnumerator PlayFadeInAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Fade_In �ִϸ��̼��� Ʈ�� 1�� ���� (�ݺ� ����)
        spineAnimationState.SetAnimation(1, fadeInAnimation, false).MixDuration = 0.5f;
    }

    void Update()
    {
        // �ƾ��� ����Ǿ���, ���� Fade_Out�� ������� �ʾҴٸ�
        if (cutsceneManager != null && cutsceneManager.IsCutsceneEnded() && !hasFadedOut)
        {
            // Fade_Out�� Animation2�� ���� �ٸ� Ʈ������ ���ÿ� ���� (�ݺ� ����)
            spineAnimationState.SetAnimation(1, fadeOutAnimation, false).MixDuration = 0.5f;
            spineAnimationState.SetAnimation(0, animation2, false).MixDuration = 0.5f;

            // �ִϸ��̼� �Ϸ� �� ȣ��� �̺�Ʈ �ڵ鷯 ���
            spineAnimationState.Complete += OnAnimationComplete;

            hasFadedOut = true;
        }
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        // Animation2�� ����Ǿ��� ��
        if (trackEntry.Animation.Name == animation2)
        {
            // �̺�Ʈ �ڵ鷯 ����
            spineAnimationState.Complete -= OnAnimationComplete;

            // UnityEvent ȣ��
            onCutsceneEnd?.Invoke();
        }
    }

    // ��ü�� �ı��� �� �̺�Ʈ �ڵ鷯 ����
    private void OnDestroy()
    {
        if (spineAnimationState != null)
        {
            spineAnimationState.Complete -= OnAnimationComplete;
        }
    }

    // ��ü�� ��Ȱ��ȭ�� �� �̺�Ʈ �ڵ鷯 ���� (���� ����)
    private void OnDisable()
    {
        if (spineAnimationState != null)
        {
            spineAnimationState.Complete -= OnAnimationComplete;
        }
    }
}
