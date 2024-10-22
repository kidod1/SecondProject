using System.Collections;
using UnityEngine;
using Spine.Unity;

public class HitEffectController : MonoBehaviour
{
    [Header("Hit Animations")]
    // Spine의 SkeletonGraphic을 사용하여 히트 이펙트를 제어합니다.
    public SkeletonGraphic hitEffectSkeletonGraphic;

    // Spine 애니메이션 이름을 선택할 수 있도록 [SpineAnimation] 어트리뷰트를 사용합니다.
    [SpineAnimation] public string hitAnim1;
    [SpineAnimation] public string hitAnim2;
    [SpineAnimation] public string hitAnim3;

    [Header("Idle Animation")]
    [SpineAnimation] public string idleAnimName; // 기본 Idle 애니메이션 이름

    private bool isHitAnimating = false; // Hit 애니메이션 재생 중인지 추적

    private void Start()
    {
        if (hitEffectSkeletonGraphic == null)
        {
            Debug.LogError("hitEffectSkeletonGraphic이 할당되지 않았습니다.");
            return;
        }

        // 기본 Idle 애니메이션 설정
        SetIdleAnimation();
    }

    /// <summary>
    /// 히트 애니메이션을 랜덤하게 재생합니다.
    /// </summary>
    public void PlayRandomHitAnimation()
    {
        if (isHitAnimating)
            return; // 이미 Hit 애니메이션이 재생 중이면 무시

        string[] hitAnimations = { hitAnim1, hitAnim2, hitAnim3 };
        int randomIndex = UnityEngine.Random.Range(0, hitAnimations.Length);
        string selectedHitAnim = hitAnimations[randomIndex];

        if (string.IsNullOrEmpty(selectedHitAnim))
        {
            Debug.LogError("선택된 히트 애니메이션 이름이 유효하지 않습니다.");
            return;
        }

        // Hit 애니메이션 재생
        hitEffectSkeletonGraphic.AnimationState.SetAnimation(0, selectedHitAnim, false);

        isHitAnimating = true; // Hit 애니메이션 재생 중으로 설정

        Debug.Log("Hit 애니메이션 실행: " + selectedHitAnim);

        // 애니메이션 완료 시 콜백을 위해 코루틴 시작
        StartCoroutine(HitAnimationCoroutine());
    }

    /// <summary>
    /// Hit 애니메이션이 완료되었을 때 호출되는 코루틴
    /// </summary>
    private IEnumerator HitAnimationCoroutine()
    {
        if (hitEffectSkeletonGraphic == null)
        {
            yield break;
        }

        // 현재 재생 중인 애니메이션의 TrackEntry 가져오기
        var trackEntry = hitEffectSkeletonGraphic.AnimationState.GetCurrent(0);
        if (trackEntry == null)
        {
            yield break;
        }

        // 애니메이션 길이 가져오기
        float animationDuration = trackEntry.Animation.Duration;

        // 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(animationDuration);

        // 기본 Idle 애니메이션으로 돌아가기
        SetIdleAnimation();

        isHitAnimating = false; // Hit 애니메이션 재생 완료
    }

    /// <summary>
    /// 기본 Idle 애니메이션을 설정합니다.
    /// </summary>
    private void SetIdleAnimation()
    {
        if (string.IsNullOrEmpty(idleAnimName))
        {
            Debug.LogError("Idle 애니메이션 이름이 설정되지 않았습니다.");
            return;
        }

        hitEffectSkeletonGraphic.AnimationState.SetAnimation(0, idleAnimName, true);
    }
}
