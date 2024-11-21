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

    [Header("Standard Animation")]
    [SerializeField]
    private AnimationReferenceAsset standardAnimationAsset;

    private Spine.AnimationState spineAnimationState;
    public event Action OnDeathAnimationsCompleted;

    [Header("Portal Settings")]
    [SerializeField]
    public GameObject PortalObjectNextScene; // 포탈 오브젝트 참조

    // 새로운 필드 추가: DeathAnimationHandler
    [Header("Death Animation Handler")]
    [SerializeField]
    private DeathAnimationHandler deathAnimationHandler;

    private void Awake()
    {
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation == null)
            {
                Debug.LogError("SlothMapManager: SkeletonAnimation 컴포넌트를 찾을 수 없습니다.");
                return;
            }
        }

        spineAnimationState = skeletonAnimation.AnimationState;

        if (spineAnimationState == null)
        {
            Debug.LogError("SlothMapManager: AnimationState가 초기화되지 않았습니다.");
            return;
        }

        SetStandardAnimation();
        spineAnimationState.Complete += OnAnimationComplete;

        // DeathAnimationHandler 설정
        if (deathAnimationHandler != null)
        {
            deathAnimationHandler.OnSecondDeathAnimationCompleted += HandleSecondDeathAnimationCompleted;
            deathAnimationHandler.OnShockingAnimationCompleted += HandleShockingAnimationCompleted;
        }
        else
        {
            Debug.LogError("SlothMapManager: DeathAnimationHandler가 할당되지 않았습니다.");
        }
    }

    private void OnDestroy()
    {
        if (spineAnimationState != null)
        {
            spineAnimationState.Complete -= OnAnimationComplete;
        }

        if (deathAnimationHandler != null)
        {
            deathAnimationHandler.OnSecondDeathAnimationCompleted -= HandleSecondDeathAnimationCompleted;
            deathAnimationHandler.OnShockingAnimationCompleted -= HandleShockingAnimationCompleted;
        }
    }

    private void SetStandardAnimation()
    {
        if (standardAnimationAsset == null || standardAnimationAsset.Animation == null)
        {
            Debug.LogError("SlothMapManager: standardAnimationAsset이 설정되지 않았습니다.");
            return;
        }
        spineAnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true).MixDuration = 0.2f;
    }

    public void PlayDeathAnimations()
    {
        if (skeletonAnimation == null || spineAnimationState == null || deathAnimationHandler == null)
        {
            Debug.LogError("SlothMapManager: PlayDeathAnimations을 실행할 수 없습니다. 필요한 컴포넌트가 할당되지 않았습니다.");
            return;
        }

        PlayFirstDeathAnimation();
    }

    private void PlayFirstDeathAnimation()
    {
        if (firstDeathAnimationAsset == null || firstDeathAnimationAsset.Animation == null)
        {
            Debug.LogError("SlothMapManager: firstDeathAnimationAsset이 설정되지 않았습니다.");
            return;
        }

        int deathTrack = 1;
        spineAnimationState.SetAnimation(deathTrack, firstDeathAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
    }

    private void HandleSecondDeathAnimationCompleted()
    {
        if (deathAnimationHandler != null)
        {
            deathAnimationHandler.PlayShockingAnimation();
        }
    }

    private void HandleShockingAnimationCompleted()
    {
        // Standard 애니메이션 루프 시작
        SetStandardAnimation();
        PortalObjectNextScene.SetActive(true); // 포탈 활성화
        OnDeathAnimationsCompleted?.Invoke();
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (trackEntry.TrackIndex == 1)
        {
            if (trackEntry.Animation.Name == firstDeathAnimationAsset.Animation.Name)
            {
                // PlaySecondDeathAnimation 대신 DeathAnimationHandler 사용
                deathAnimationHandler?.PlaySecondDeathAnimation();
            }
            // else if 블록 제거됨, HandleSecondDeathAnimationCompleted가 처리
        }
    }
}
