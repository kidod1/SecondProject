using UnityEngine;
using Spine.Unity;
using System.Collections;
using Spine;

public class SlothCutSceneAnimationController : MonoBehaviour
{
    [SpineAnimation] public string fadeInAnimation;    // Fade_In �ִϸ��̼�
    [SpineAnimation] public string animation1;         // Animation1
    [SpineAnimation] public string fadeOutAnimation;   // Fade_Out �ִϸ��̼�
    [SpineAnimation] public string animation2;         // Animation2

    [SerializeField]
    private SkeletonGraphic skeletonGraphic; // UI�� SkeletonGraphic ������Ʈ
    private Spine.AnimationState spineAnimationState;
    private CutsceneManager cutsceneManager;

    private bool hasFadedOut = false;

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
        cutsceneManager = FindObjectOfType<CutsceneManager>();
        if (cutsceneManager == null)
        {
            Debug.LogError("CutsceneManager�� ã�� �� �����ϴ�.");
        }

        // Animation1�� ��� ���� (�ݺ� ����)
        spineAnimationState.SetAnimation(0, animation1, false).MixDuration = 0.5f;

        // Coroutine�� �����Ͽ� 2�� �Ŀ� Fade_In�� ����
        StartCoroutine(PlayFadeInAfterDelay(2.3f));
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
        // Animation2�� ����Ǿ��� �� �� ��ȯ
        if (trackEntry.Animation.Name == animation2)
        {
            // �̺�Ʈ �ڵ鷯 ����
            spineAnimationState.Complete -= OnAnimationComplete;

            // �� ��ȯ
            if (cutsceneManager != null)
            {
                cutsceneManager.LoadNextScene();
            }
        }
    }
}
