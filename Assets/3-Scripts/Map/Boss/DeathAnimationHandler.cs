using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class DeathAnimationHandler : MonoBehaviour
{
    [Header("Second Death Animation")]
    [SerializeField]
    [Tooltip("�� ��° ��� �ִϸ��̼��� �̸�"), SpineAnimation]
    private string secondDeathAnimationName;

    [Header("Shocking Animation")]
    [SerializeField]
    [Tooltip("��� �ִϸ��̼��� �̸�"), SpineAnimation]
    private string shockingAnimationName;

    private Spine.AnimationState spineAnimationState;
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;

    public event Action OnSecondDeathAnimationCompleted;
    public event Action OnShockingAnimationCompleted;

    private void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("DeathAnimationHandler: SkeletonAnimation ������Ʈ�� ã�� �� �����ϴ�.");
            return;
        }

        spineAnimationState = skeletonAnimation.AnimationState;
        if (spineAnimationState == null)
        {
            Debug.LogError("DeathAnimationHandler: AnimationState�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        spineAnimationState.Complete += OnAnimationComplete;
    }

    private void OnDestroy()
    {
        if (spineAnimationState != null)
        {
            spineAnimationState.Complete -= OnAnimationComplete;
        }
    }

    /// <summary>
    /// �� ��° ��� �ִϸ��̼��� ����մϴ�.
    /// </summary>
    public void PlaySecondDeathAnimation()
    {
        if (string.IsNullOrEmpty(secondDeathAnimationName))
        {
            Debug.LogError("DeathAnimationHandler: secondDeathAnimationName�� �������� �ʾҽ��ϴ�.");
            return;
        }

        spineAnimationState.SetAnimation(1, secondDeathAnimationName, false).MixDuration = 0.2f;
    }

    /// <summary>
    /// ��� �ִϸ��̼��� ����մϴ�.
    /// </summary>
    public void PlayShockingAnimation()
    {
        if (string.IsNullOrEmpty(shockingAnimationName))
        {
            Debug.LogError("DeathAnimationHandler: shockingAnimationName�� �������� �ʾҽ��ϴ�.");
            return;
        }

        spineAnimationState.SetAnimation(2, shockingAnimationName, false).MixDuration = 0.2f;
    }

    /// <summary>
    /// �ִϸ��̼��� �Ϸ�Ǿ��� �� ȣ��Ǵ� �ݹ� �޼����Դϴ�.
    /// </summary>
    /// <param name="trackEntry">�Ϸ�� Ʈ�� ��Ʈ��</param>
    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (trackEntry.TrackIndex == 1 && trackEntry.Animation.Name == secondDeathAnimationName)
        {
            OnSecondDeathAnimationCompleted?.Invoke();
        }
        else if (trackEntry.TrackIndex == 2 && trackEntry.Animation.Name == shockingAnimationName)
        {
            OnShockingAnimationCompleted?.Invoke();
        }
    }
}
