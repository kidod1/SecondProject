using UnityEngine;
using Spine.Unity;
using Spine;

public class SlothAnimationController : MonoBehaviour
{
    private SkeletonGraphic skeletonGraphic;

    [SpineAnimation]
    public string surprisedAnimation; // Surprised �ִϸ��̼� �̸�

    [SpineAnimation]
    public string idleAnimation;      // Idle �ִϸ��̼� �̸�

    [SpineAnimation]
    public string surprisedFaceAnimation; // Surprised �� �ִϸ��̼� �̸�

    private void Start()
    {
        // SkeletonGraphic ������Ʈ ��������
        skeletonGraphic = GetComponent<SkeletonGraphic>();
        if (skeletonGraphic == null)
        {
            Debug.LogError("SkeletonGraphic ������Ʈ�� �����ϴ�.");
            return;
        }

        // �ִϸ��̼� ������ Complete �̺�Ʈ�� �ݹ� ���
        skeletonGraphic.AnimationState.Complete += OnAnimationComplete;

        // ������ �� Surprised �ִϸ��̼� ���� (Ʈ�� 0, �������� ����)
        skeletonGraphic.AnimationState.SetAnimation(0, surprisedAnimation, false);

        // �� �ִϸ��̼� ���� (Ʈ�� 1, �������� ����)
        skeletonGraphic.AnimationState.SetAnimation(1, surprisedFaceAnimation, false);
    }

    /// <summary>
    /// �ִϸ��̼��� �Ϸ�Ǿ��� �� ȣ��Ǵ� �ݹ� �޼���
    /// </summary>
    /// <param name="trackEntry">�Ϸ�� Ʈ���� ��Ʈ��</param>
    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        // Ʈ�� 0���� �Ϸ�� �ִϸ��̼��� Surprised �ִϸ��̼����� Ȯ��
        if (trackEntry.TrackIndex == 0 && trackEntry.Animation.Name == surprisedAnimation)
        {
            // Idle �ִϸ��̼��� Ʈ�� 0���� ������ ����
            skeletonGraphic.AnimationState.SetAnimation(0, idleAnimation, true);
        }

        // Ʈ�� 1���� �Ϸ�� �ִϸ��̼��� SurprisedFace �ִϸ��̼����� Ȯ��
        if (trackEntry.TrackIndex == 1 && trackEntry.Animation.Name == surprisedFaceAnimation)
        {
            // Ʈ�� 1�� ���ϴ�.
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
