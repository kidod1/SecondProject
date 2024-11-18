using System;
using UnityEngine;
using Spine;
using Spine.Unity;

public class SlothMapManager : MonoBehaviour
{
    [Header("Spine Animation Settings")]
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;

    [Header("Death Animations")]
    [SerializeField]
    private AnimationReferenceAsset firstDeathAnimationAsset;
    [SerializeField]
    private AnimationReferenceAsset secondDeathAnimationAsset;

    [Header("Standard Animation")]
    [SerializeField]
    private AnimationReferenceAsset standardAnimationAsset;

    [Header("Shooking Animation")]
    [SerializeField]
    private AnimationReferenceAsset shokingAnimation;

    private Spine.AnimationState spineAnimationState;
    public event Action OnDeathAnimationsCompleted;

    public GameObject PortalObjectNextScene;

    private void Awake()
    {
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation == null)
            {
                return;
            }
        }

        spineAnimationState = skeletonAnimation.AnimationState;

        if (spineAnimationState == null)
        {
            return;
        }

        SetStandardAnimation();
        spineAnimationState.Complete += OnAnimationComplete;
    }

    private void OnDestroy()
    {
        if (spineAnimationState != null)
        {
            spineAnimationState.Complete -= OnAnimationComplete;
        }
    }

    private void SetStandardAnimation()
    {
        if (standardAnimationAsset == null || standardAnimationAsset.Animation == null)
        {
            return;
        }
        spineAnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true).MixDuration = 0.2f;
    }

    private void SetEndAnimation()
    {
        if (shokingAnimation == null || shokingAnimation.Animation == null)
        {
            return;
        }
        spineAnimationState.SetAnimation(0, shokingAnimation.Animation.Name, true).MixDuration = 0.2f;
    }

    public void PlayDeathAnimations()
    {
        if (skeletonAnimation == null || spineAnimationState == null)
        {
            return;
        }

        PlayFirstDeathAnimation();
    }

    private void PlayFirstDeathAnimation()
    {
        if (firstDeathAnimationAsset == null || firstDeathAnimationAsset.Animation == null)
        {
            return;
        }

        int deathTrack = 1;
        spineAnimationState.SetAnimation(deathTrack, firstDeathAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
    }

    private void PlaySecondDeathAnimation()
    {
        if (secondDeathAnimationAsset == null || secondDeathAnimationAsset.Animation == null)
        {
            return;
        }

        int deathTrack = 1;
        spineAnimationState.SetAnimation(deathTrack, secondDeathAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
        PortalObjectNextScene.SetActive(true);
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (trackEntry.TrackIndex == 1)
        {
            if (trackEntry.Animation.Name == firstDeathAnimationAsset.Animation.Name)
            {
                PlaySecondDeathAnimation();
            }
            else if (trackEntry.Animation.Name == secondDeathAnimationAsset.Animation.Name)
            {
                // Standard 애니메이션 루프 시작
                SetEndAnimation();
                OnDeathAnimationsCompleted?.Invoke();
            }
        }
    }
}
