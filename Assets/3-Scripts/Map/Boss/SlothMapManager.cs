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
    public GameObject PortalObjectNextScene; // ��Ż ������Ʈ ����

    // ���ο� �ʵ� �߰�: DeathAnimationHandler
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
                Debug.LogError("SlothMapManager: SkeletonAnimation ������Ʈ�� ã�� �� �����ϴ�.");
                return;
            }
        }

        spineAnimationState = skeletonAnimation.AnimationState;

        if (spineAnimationState == null)
        {
            Debug.LogError("SlothMapManager: AnimationState�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        SetStandardAnimation();
        spineAnimationState.Complete += OnAnimationComplete;

        // DeathAnimationHandler ����
        if (deathAnimationHandler != null)
        {
            deathAnimationHandler.OnSecondDeathAnimationCompleted += HandleSecondDeathAnimationCompleted;
            deathAnimationHandler.OnShockingAnimationCompleted += HandleShockingAnimationCompleted;
        }
        else
        {
            Debug.LogError("SlothMapManager: DeathAnimationHandler�� �Ҵ���� �ʾҽ��ϴ�.");
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
            Debug.LogError("SlothMapManager: standardAnimationAsset�� �������� �ʾҽ��ϴ�.");
            return;
        }
        spineAnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true).MixDuration = 0.2f;
    }

    public void PlayDeathAnimations()
    {
        if (skeletonAnimation == null || spineAnimationState == null || deathAnimationHandler == null)
        {
            Debug.LogError("SlothMapManager: PlayDeathAnimations�� ������ �� �����ϴ�. �ʿ��� ������Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        PlayFirstDeathAnimation();
    }

    private void PlayFirstDeathAnimation()
    {
        if (firstDeathAnimationAsset == null || firstDeathAnimationAsset.Animation == null)
        {
            Debug.LogError("SlothMapManager: firstDeathAnimationAsset�� �������� �ʾҽ��ϴ�.");
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
        // Standard �ִϸ��̼� ���� ����
        SetStandardAnimation();
        PortalObjectNextScene.SetActive(true); // ��Ż Ȱ��ȭ
        OnDeathAnimationsCompleted?.Invoke();
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (trackEntry.TrackIndex == 1)
        {
            if (trackEntry.Animation.Name == firstDeathAnimationAsset.Animation.Name)
            {
                // PlaySecondDeathAnimation ��� DeathAnimationHandler ���
                deathAnimationHandler?.PlaySecondDeathAnimation();
            }
            // else if ��� ���ŵ�, HandleSecondDeathAnimationCompleted�� ó��
        }
    }
}
