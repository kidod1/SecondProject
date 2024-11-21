using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class DeathAnimationHandler : MonoBehaviour
{
    [Header("Second Death Animation")]
    [SerializeField]
    [Tooltip("두 번째 사망 애니메이션의 이름"), SpineAnimation]
    private string secondDeathAnimationName;

    [Header("Shocking Animation")]
    [SerializeField]
    [Tooltip("충격 애니메이션의 이름"), SpineAnimation]
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
            Debug.LogError("DeathAnimationHandler: SkeletonAnimation 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        spineAnimationState = skeletonAnimation.AnimationState;
        if (spineAnimationState == null)
        {
            Debug.LogError("DeathAnimationHandler: AnimationState가 초기화되지 않았습니다.");
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
    /// 두 번째 사망 애니메이션을 재생합니다.
    /// </summary>
    public void PlaySecondDeathAnimation()
    {
        if (string.IsNullOrEmpty(secondDeathAnimationName))
        {
            Debug.LogError("DeathAnimationHandler: secondDeathAnimationName이 설정되지 않았습니다.");
            return;
        }

        spineAnimationState.SetAnimation(1, secondDeathAnimationName, false).MixDuration = 0.2f;
    }

    /// <summary>
    /// 충격 애니메이션을 재생합니다.
    /// </summary>
    public void PlayShockingAnimation()
    {
        if (string.IsNullOrEmpty(shockingAnimationName))
        {
            Debug.LogError("DeathAnimationHandler: shockingAnimationName이 설정되지 않았습니다.");
            return;
        }

        spineAnimationState.SetAnimation(2, shockingAnimationName, false).MixDuration = 0.2f;
    }

    /// <summary>
    /// 애니메이션이 완료되었을 때 호출되는 콜백 메서드입니다.
    /// </summary>
    /// <param name="trackEntry">완료된 트랙 엔트리</param>
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
