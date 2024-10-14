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
    public AnimationReferenceAsset firstDeathAnimationAsset;
    public AnimationReferenceAsset secondDeathAnimationAsset;

    [Header("Standard Animation")]
    public AnimationReferenceAsset standardAnimationAsset;

    [Header("Additional Animations After Death")]
    [Tooltip("중간 보스 사망 후 재생할 추가 애니메이션 1")]
    public AnimationReferenceAsset additionalAnimationAsset1;
    [Tooltip("중간 보스 사망 후 재생할 추가 애니메이션 2")]
    public AnimationReferenceAsset additionalAnimationAsset2;

    [Header("Key-Controlled Animations")]
    [Tooltip("키 입력으로 재생할 애니메이션")]
    public AnimationReferenceAsset keyControlledAnimationAsset;

    public event Action OnDeathAnimationsCompleted;

    private Spine.AnimationState spineAnimationState;

    private void Awake()
    {
        // SkeletonAnimation 컴포넌트 할당
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation == null)
            {
                Debug.LogError("SkeletonAnimation component is not assigned or found on " + gameObject.name);
                return;
            }
        }

        // AnimationState 초기화
        spineAnimationState = skeletonAnimation.AnimationState;

        if (spineAnimationState == null)
        {
            Debug.LogError("AnimationState is null in SkeletonAnimation.");
            return;
        }

        // 초기 Standard 애니메이션 설정 (트랙 0, 루프)
        SetStandardAnimation();

        // 애니메이션 완료 이벤트 핸들러 등록
        spineAnimationState.Complete += OnAnimationComplete;
    }

    private void OnDestroy()
    {
        // 이벤트 핸들러 해제
        if (spineAnimationState != null)
        {
            spineAnimationState.Complete -= OnAnimationComplete;
        }
    }

    /// <summary>
    /// Standard 애니메이션을 트랙 0에서 루프 재생합니다.
    /// </summary>
    private void SetStandardAnimation()
    {
        if (standardAnimationAsset == null || standardAnimationAsset.Animation == null)
        {
            Debug.LogError("StandardAnimationAsset이 할당되지 않았거나 Animation이 없습니다.");
            return;
        }
        if (spineAnimationState == null)
        {
            Debug.LogError("AnimationState is not initialized.");
            return;
        }
        spineAnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true).MixDuration = 0.2f;
        Debug.Log($"Standard 애니메이션 설정: {standardAnimationAsset.Animation.Name}");
    }

    /// <summary>
    /// Death 애니메이션을 트랙 1에서 순차적으로 재생합니다.
    /// </summary>
    public void PlayDeathAnimations()
    {
        if (skeletonAnimation == null)
        {
            Debug.LogError("SkeletonAnimation is not assigned.");
            return;
        }
        if (spineAnimationState == null)
        {
            Debug.LogError("AnimationState is not initialized.");
            return;
        }

        // Death 애니메이션 재생 시작
        PlayFirstDeathAnimation();
    }

    /// <summary>
    /// 첫 번째 Death 애니메이션 재생
    /// </summary>
    private void PlayFirstDeathAnimation()
    {
        if (firstDeathAnimationAsset == null)
        {
            Debug.LogError("FirstDeathAnimationAsset이 할당되지 않았습니다.");
            return;
        }
        if (firstDeathAnimationAsset.Animation == null)
        {
            Debug.LogError("FirstDeathAnimationAsset의 Animation이 없습니다.");
            return;
        }

        int deathTrack = 1; // Death 애니메이션을 위한 별도 트랙 사용

        spineAnimationState.SetAnimation(deathTrack, firstDeathAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"첫 번째 애니메이션 설정: {firstDeathAnimationAsset.Animation.Name}");
    }

    /// <summary>
    /// 두 번째 Death 애니메이션 재생
    /// </summary>
    private void PlaySecondDeathAnimation()
    {
        if (secondDeathAnimationAsset == null || secondDeathAnimationAsset.Animation == null)
        {
            Debug.LogError("SecondDeathAnimationAsset이 할당되지 않았거나 Animation이 없습니다.");
            return;
        }

        int deathTrack = 1; // 동일 트랙에서 재생

        spineAnimationState.SetAnimation(deathTrack, secondDeathAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"두 번째 애니메이션 설정: {secondDeathAnimationAsset.Animation.Name}");
    }

    /// <summary>
    /// 추가 애니메이션 1 재생
    /// </summary>
    public void PlayAdditionalAnimation1()
    {
        if (additionalAnimationAsset1 == null || additionalAnimationAsset1.Animation == null)
        {
            Debug.LogError("AdditionalAnimationAsset1이 할당되지 않았거나 Animation이 없습니다.");
            return;
        }

        int additionalTrack = 2; // 추가 애니메이션을 위한 별도 트랙

        spineAnimationState.SetAnimation(additionalTrack, additionalAnimationAsset1.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"추가 애니메이션 1 설정: {additionalAnimationAsset1.Animation.Name}");
    }

    /// <summary>
    /// 추가 애니메이션 2 재생
    /// </summary>
    public void PlayAdditionalAnimation2()
    {
        if (additionalAnimationAsset2 == null || additionalAnimationAsset2.Animation == null)
        {
            Debug.LogError("AdditionalAnimationAsset2이 할당되지 않았거나 Animation이 없습니다.");
            return;
        }

        int additionalTrack = 2; // 추가 애니메이션을 위한 별도 트랙

        spineAnimationState.SetAnimation(additionalTrack, additionalAnimationAsset2.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"추가 애니메이션 2 설정: {additionalAnimationAsset2.Animation.Name}");
    }

    /// <summary>
    /// 키 입력으로 제어할 애니메이션 재생
    /// </summary>
    public void PlayKeyControlledAnimation()
    {
        if (keyControlledAnimationAsset == null || keyControlledAnimationAsset.Animation == null)
        {
            Debug.LogError("KeyControlledAnimationAsset이 할당되지 않았거나 Animation이 없습니다.");
            return;
        }

        int keyControlTrack = 3; // 키 제어 애니메이션을 위한 별도 트랙

        spineAnimationState.SetAnimation(keyControlTrack, keyControlledAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"키 제어 애니메이션 설정: {keyControlledAnimationAsset.Animation.Name}");
    }

    /// <summary>
    /// 애니메이션 완료 시 호출되는 이벤트 핸들러
    /// </summary>
    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (trackEntry.TrackIndex == 1)
        {
            if (trackEntry.Animation.Name == firstDeathAnimationAsset.Animation.Name)
            {
                // 첫 번째 Death 애니메이션 완료 후 두 번째 애니메이션 재생
                PlaySecondDeathAnimation();
            }
            else if (trackEntry.Animation.Name == secondDeathAnimationAsset.Animation.Name)
            {
                // 두 번째 Death 애니메이션 완료 후 추가 애니메이션 재생 또는 이벤트 호출
                Debug.Log("모든 죽음 애니메이션 완료");

                // 추가 애니메이션을 자동으로 재생하려면 아래 주석을 해제하세요.
                // PlayAdditionalAnimation1();
                // PlayAdditionalAnimation2();

                // 이벤트 호출
                OnDeathAnimationsCompleted?.Invoke();

                // 필요한 추가 로직 (예: 보스 오브젝트 파괴) 여기서 처리 가능
            }
        }
    }

    /// <summary>
    /// 키 입력을 통해 애니메이션을 제어합니다.
    /// 예시: Update 메서드에서 키 입력을 감지하여 애니메이션을 재생할 수 있습니다.
    /// </summary>
    private void Update()
    {
        // 기본적인 키 입력 예시
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayAdditionalAnimation1();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayAdditionalAnimation2();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayKeyControlledAnimation();
        }

        // 활성화된 몬스터 리스트를 업데이트하여 죽은 몬스터를 제거
        // 필요한 경우 여기에 추가 로직을 넣을 수 있습니다.
    }
}
