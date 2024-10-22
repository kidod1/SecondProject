using System.Collections;
using UnityEngine;
using Spine.Unity;

public class HitEffectController : MonoBehaviour
{
    [Header("Hit Animations")]
    // Spine�� SkeletonGraphic�� ����Ͽ� ��Ʈ ����Ʈ�� �����մϴ�.
    public SkeletonGraphic hitEffectSkeletonGraphic;

    // Spine �ִϸ��̼� �̸��� ������ �� �ֵ��� [SpineAnimation] ��Ʈ����Ʈ�� ����մϴ�.
    [SpineAnimation] public string hitAnim1;
    [SpineAnimation] public string hitAnim2;
    [SpineAnimation] public string hitAnim3;

    [Header("Idle Animation")]
    [SpineAnimation] public string idleAnimName; // �⺻ Idle �ִϸ��̼� �̸�

    private bool isHitAnimating = false; // Hit �ִϸ��̼� ��� ������ ����

    private void Start()
    {
        if (hitEffectSkeletonGraphic == null)
        {
            Debug.LogError("hitEffectSkeletonGraphic�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // �⺻ Idle �ִϸ��̼� ����
        SetIdleAnimation();
    }

    /// <summary>
    /// ��Ʈ �ִϸ��̼��� �����ϰ� ����մϴ�.
    /// </summary>
    public void PlayRandomHitAnimation()
    {
        if (isHitAnimating)
            return; // �̹� Hit �ִϸ��̼��� ��� ���̸� ����

        string[] hitAnimations = { hitAnim1, hitAnim2, hitAnim3 };
        int randomIndex = UnityEngine.Random.Range(0, hitAnimations.Length);
        string selectedHitAnim = hitAnimations[randomIndex];

        if (string.IsNullOrEmpty(selectedHitAnim))
        {
            Debug.LogError("���õ� ��Ʈ �ִϸ��̼� �̸��� ��ȿ���� �ʽ��ϴ�.");
            return;
        }

        // Hit �ִϸ��̼� ���
        hitEffectSkeletonGraphic.AnimationState.SetAnimation(0, selectedHitAnim, false);

        isHitAnimating = true; // Hit �ִϸ��̼� ��� ������ ����

        Debug.Log("Hit �ִϸ��̼� ����: " + selectedHitAnim);

        // �ִϸ��̼� �Ϸ� �� �ݹ��� ���� �ڷ�ƾ ����
        StartCoroutine(HitAnimationCoroutine());
    }

    /// <summary>
    /// Hit �ִϸ��̼��� �Ϸ�Ǿ��� �� ȣ��Ǵ� �ڷ�ƾ
    /// </summary>
    private IEnumerator HitAnimationCoroutine()
    {
        if (hitEffectSkeletonGraphic == null)
        {
            yield break;
        }

        // ���� ��� ���� �ִϸ��̼��� TrackEntry ��������
        var trackEntry = hitEffectSkeletonGraphic.AnimationState.GetCurrent(0);
        if (trackEntry == null)
        {
            yield break;
        }

        // �ִϸ��̼� ���� ��������
        float animationDuration = trackEntry.Animation.Duration;

        // �ִϸ��̼��� ���� ������ ���
        yield return new WaitForSeconds(animationDuration);

        // �⺻ Idle �ִϸ��̼����� ���ư���
        SetIdleAnimation();

        isHitAnimating = false; // Hit �ִϸ��̼� ��� �Ϸ�
    }

    /// <summary>
    /// �⺻ Idle �ִϸ��̼��� �����մϴ�.
    /// </summary>
    private void SetIdleAnimation()
    {
        if (string.IsNullOrEmpty(idleAnimName))
        {
            Debug.LogError("Idle �ִϸ��̼� �̸��� �������� �ʾҽ��ϴ�.");
            return;
        }

        hitEffectSkeletonGraphic.AnimationState.SetAnimation(0, idleAnimName, true);
    }
}
