using System.Collections;
using UnityEngine;
using Spine.Unity;

public class HitEffectController : MonoBehaviour
{
    [Header("Hit Animations")]
    // Spine의 SkeletonGraphic을 사용하여 히트 이펙트를 제어합니다.
    public SkeletonGraphic hitEffectSkeletonGraphic;

    // Hit 애니메이션 이름들
    [SpineAnimation] public string hitAnim1;
    [SpineAnimation] public string hitAnim2;
    [SpineAnimation] public string hitAnim3;

    [Header("Pain Animations")]
    // 체력에 따른 Pain 애니메이션 이름들
    [SpineAnimation] public string painAnim1; // 체력 50% 이상 ~ 70% 미만
    [SpineAnimation] public string painAnim2; // 체력 30% 이상 ~ 50% 미만
    [SpineAnimation] public string painAnim3; // 체력 30% 미만

    [Header("Idle Animation")]
    [SpineAnimation] public string idleAnimName; // 기본 Idle 애니메이션 이름 (체력 70% 이상)

    private bool isHitAnimating = false; // Hit 애니메이션 재생 중인지 추적

    private Player player; // 플레이어 참조

    private void Start()
    {
        if (hitEffectSkeletonGraphic == null)
        {
            Debug.LogError("hitEffectSkeletonGraphic이 할당되지 않았습니다.");
            return;
        }

        // Player 오브젝트 찾기
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Player 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // 초기 표정 업데이트
        UpdateExpressionBasedOnHealth();
    }

    private void OnEnable()
    {
        if (player != null && player.stat != null)
        {
            player.stat.OnStatsChanged += OnPlayerStatsChanged;
        }
    }

    private void OnDisable()
    {
        if (player != null && player.stat != null)
        {
            player.stat.OnStatsChanged -= OnPlayerStatsChanged;
        }
    }

    private void OnPlayerStatsChanged()
    {
        if (!isHitAnimating)
        {
            UpdateExpressionBasedOnHealth();
        }
    }

    /// <summary>
    /// 플레이어의 체력 비율에 따라 표정 애니메이션을 업데이트합니다.
    /// </summary>
    public void UpdateExpressionBasedOnHealth()
    {
        if (player == null || player.stat == null)
        {
            Debug.LogError("플레이어 또는 플레이어 데이터가 설정되지 않았습니다.");
            return;
        }

        // 체력 비율 계산
        float healthPercentage = (float)player.stat.currentHP / player.stat.currentMaxHP * 100f;

        // 애니메이션 이름 결정
        string expressionAnimName = "";

        if (healthPercentage < 30f)
        {
            expressionAnimName = painAnim3;
        }
        else if (healthPercentage < 50f)
        {
            expressionAnimName = painAnim2;
        }
        else if (healthPercentage < 70f)
        {
            expressionAnimName = painAnim1;
        }
        else
        {
            expressionAnimName = idleAnimName; // 기본 Idle 애니메이션
        }

        // 애니메이션 변경
        if (!string.IsNullOrEmpty(expressionAnimName))
        {
            hitEffectSkeletonGraphic.AnimationState.SetAnimation(0, expressionAnimName, true);
        }
        else
        {
            Debug.LogError("표정 애니메이션 이름이 유효하지 않습니다.");
        }
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

        isHitAnimating = false; // Hit 애니메이션 재생 완료

        // Hit 애니메이션이 끝난 후 현재 체력에 맞는 표정 업데이트
        UpdateExpressionBasedOnHealth();
    }
}
