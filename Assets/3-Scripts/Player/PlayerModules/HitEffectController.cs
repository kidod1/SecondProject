using System.Collections;
using UnityEngine;
using Spine.Unity;

public class HitEffectController : MonoBehaviour
{
    [Header("Hit Animations")]
    // Spine�� SkeletonGraphic�� ����Ͽ� ��Ʈ ����Ʈ�� �����մϴ�.
    public SkeletonGraphic hitEffectSkeletonGraphic;

    // Hit �ִϸ��̼� �̸���
    [SpineAnimation] public string hitAnim1;
    [SpineAnimation] public string hitAnim2;
    [SpineAnimation] public string hitAnim3;

    [Header("Pain Animations")]
    // ü�¿� ���� Pain �ִϸ��̼� �̸���
    [SpineAnimation] public string painAnim1; // ü�� 50% �̻� ~ 70% �̸�
    [SpineAnimation] public string painAnim2; // ü�� 30% �̻� ~ 50% �̸�
    [SpineAnimation] public string painAnim3; // ü�� 30% �̸�

    [Header("Idle Animation")]
    [SpineAnimation] public string idleAnimName; // �⺻ Idle �ִϸ��̼� �̸� (ü�� 70% �̻�)

    private bool isHitAnimating = false; // Hit �ִϸ��̼� ��� ������ ����

    private Player player; // �÷��̾� ����

    private void Start()
    {
        if (hitEffectSkeletonGraphic == null)
        {
            Debug.LogError("hitEffectSkeletonGraphic�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // Player ������Ʈ ã��
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Player ������Ʈ�� ã�� �� �����ϴ�.");
            return;
        }

        // �ʱ� ǥ�� ������Ʈ
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
    /// �÷��̾��� ü�� ������ ���� ǥ�� �ִϸ��̼��� ������Ʈ�մϴ�.
    /// </summary>
    public void UpdateExpressionBasedOnHealth()
    {
        if (player == null || player.stat == null)
        {
            Debug.LogError("�÷��̾� �Ǵ� �÷��̾� �����Ͱ� �������� �ʾҽ��ϴ�.");
            return;
        }

        // ü�� ���� ���
        float healthPercentage = (float)player.stat.currentHP / player.stat.currentMaxHP * 100f;

        // �ִϸ��̼� �̸� ����
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
            expressionAnimName = idleAnimName; // �⺻ Idle �ִϸ��̼�
        }

        // �ִϸ��̼� ����
        if (!string.IsNullOrEmpty(expressionAnimName))
        {
            hitEffectSkeletonGraphic.AnimationState.SetAnimation(0, expressionAnimName, true);
        }
        else
        {
            Debug.LogError("ǥ�� �ִϸ��̼� �̸��� ��ȿ���� �ʽ��ϴ�.");
        }
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

        isHitAnimating = false; // Hit �ִϸ��̼� ��� �Ϸ�

        // Hit �ִϸ��̼��� ���� �� ���� ü�¿� �´� ǥ�� ������Ʈ
        UpdateExpressionBasedOnHealth();
    }
}
