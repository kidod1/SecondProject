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
    private void Awake()
    {
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation == null)
            {
                Debug.LogError("SkeletonAnimation component is not assigned or found on " + gameObject.name);
                return;
            }
        }

        spineAnimationState = skeletonAnimation.AnimationState;

        if (spineAnimationState == null)
        {
            Debug.LogError("AnimationState is null in SkeletonAnimation.");
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
            Debug.LogError("StandardAnimationAsset�� �Ҵ���� �ʾҰų� Animation�� �����ϴ�.");
            return;
        }
        spineAnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true).MixDuration = 0.2f;
        Debug.Log($"Standard �ִϸ��̼� ����: {standardAnimationAsset.Animation.Name}");
    }
    private void SetEndAnimation()
    {
        if (shokingAnimation == null || shokingAnimation.Animation == null)
        {
            Debug.LogError("shoking AnmationAsset�� �Ҵ���� �ʾҰų� Animation�� �����ϴ�.");
            return;
        }
        spineAnimationState.SetAnimation(0, shokingAnimation.Animation.Name, true).MixDuration = 0.2f;
        Debug.Log($"shoking �ִϸ��̼� ����: {shokingAnimation.Animation.Name}");
    }

    public void PlayDeathAnimations()
    {
        if (skeletonAnimation == null || spineAnimationState == null)
        {
            Debug.LogError("SkeletonAnimation �Ǵ� AnimationState�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        PlayFirstDeathAnimation();
    }

    private void PlayFirstDeathAnimation()
    {
        if (firstDeathAnimationAsset == null || firstDeathAnimationAsset.Animation == null)
        {
            Debug.LogError("FirstDeathAnimationAsset�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        int deathTrack = 1;
        spineAnimationState.SetAnimation(deathTrack, firstDeathAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"ù ��° �ִϸ��̼� ����: {firstDeathAnimationAsset.Animation.Name}");
    }

    private void PlaySecondDeathAnimation()
    {
        if (secondDeathAnimationAsset == null || secondDeathAnimationAsset.Animation == null)
        {
            Debug.LogError("SecondDeathAnimationAsset�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        int deathTrack = 1;
        spineAnimationState.SetAnimation(deathTrack, secondDeathAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"�� ��° �ִϸ��̼� ����: {secondDeathAnimationAsset.Animation.Name}");
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
                Debug.Log("��� ���� �ִϸ��̼� �Ϸ�");

                // Standard �ִϸ��̼� ���� ����
                SetEndAnimation();
                OnDeathAnimationsCompleted?.Invoke();
            }
        }
    }
}
