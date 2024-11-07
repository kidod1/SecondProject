using UnityEngine;
using Spine.Unity;
using Spine;

public class SlothAnimationController : MonoBehaviour
{
    private SkeletonGraphic skeletonGraphic;

    [SpineAnimation]
    public string surprisedAnimation; // Surprised 애니메이션 이름

    [SpineAnimation]
    public string idleAnimation;      // Idle 애니메이션 이름

    [SpineAnimation]
    public string surprisedFaceAnimation; // Surprised 얼굴 애니메이션 이름

    private void Start()
    {
        // SkeletonGraphic 컴포넌트 가져오기
        skeletonGraphic = GetComponent<SkeletonGraphic>();
        if (skeletonGraphic == null)
        {
            Debug.LogError("SkeletonGraphic 컴포넌트가 없습니다.");
            return;
        }

        // 애니메이션 상태의 Complete 이벤트에 콜백 등록
        skeletonGraphic.AnimationState.Complete += OnAnimationComplete;

        // 시작할 때 Surprised 애니메이션 실행 (트랙 0, 루프하지 않음)
        skeletonGraphic.AnimationState.SetAnimation(0, surprisedAnimation, false);

        // 얼굴 애니메이션 실행 (트랙 1, 루프하지 않음)
        skeletonGraphic.AnimationState.SetAnimation(1, surprisedFaceAnimation, false);
    }

    /// <summary>
    /// 애니메이션이 완료되었을 때 호출되는 콜백 메서드
    /// </summary>
    /// <param name="trackEntry">완료된 트랙의 엔트리</param>
    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        // 트랙 0에서 완료된 애니메이션이 Surprised 애니메이션인지 확인
        if (trackEntry.TrackIndex == 0 && trackEntry.Animation.Name == surprisedAnimation)
        {
            // Idle 애니메이션을 트랙 0에서 루프로 설정
            skeletonGraphic.AnimationState.SetAnimation(0, idleAnimation, true);
        }

        // 트랙 1에서 완료된 애니메이션이 SurprisedFace 애니메이션인지 확인
        if (trackEntry.TrackIndex == 1 && trackEntry.Animation.Name == surprisedFaceAnimation)
        {
            // 트랙 1을 비웁니다.
            skeletonGraphic.AnimationState.ClearTrack(1);
        }
    }

    private void OnDestroy()
    {
        if (skeletonGraphic != null && skeletonGraphic.AnimationState != null)
        {
            skeletonGraphic.AnimationState.Complete -= OnAnimationComplete;
        }
    }
}
