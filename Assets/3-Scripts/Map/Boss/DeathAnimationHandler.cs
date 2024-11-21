using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class DeathAnimationHandler : MonoBehaviour
{
    [Header("Second Death Animation")]
    [SerializeField]
    private AnimationReferenceAsset secondDeathAnimationAsset;

    [Header("Shocking Animation")]
    [SerializeField]
    private AnimationReferenceAsset shockingAnimation;

    private Spine.AnimationState spineAnimationState;
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

    public void PlaySecondDeathAnimation()
    {
        if (secondDeathAnimationAsset == null || secondDeathAnimationAsset.Animation == null)
        {
            Debug.LogError("DeathAnimationHandler: secondDeathAnimationAsset이 설정되지 않았습니다.");
            return;
        }

        spineAnimationState.SetAnimation(1, secondDeathAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
    }

    public void PlayShockingAnimation()
    {
        if (shockingAnimation == null || shockingAnimation.Animation == null)
        {
            Debug.LogError("DeathAnimationHandler: shockingAnimation이 설정되지 않았습니다.");
            return;
        }

        spineAnimationState.SetAnimation(2, shockingAnimation.Animation.Name, false).MixDuration = 0.2f;
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (trackEntry.TrackIndex == 1 && trackEntry.Animation.Name == secondDeathAnimationAsset.Animation.Name)
        {
            OnSecondDeathAnimationCompleted?.Invoke();
        }
        else if (trackEntry.TrackIndex == 2 && trackEntry.Animation.Name == shockingAnimation.Animation.Name)
        {
            OnShockingAnimationCompleted?.Invoke();
        }
    }
}
