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
    [Tooltip("�߰� ���� ��� �� ����� �߰� �ִϸ��̼� 1")]
    public AnimationReferenceAsset additionalAnimationAsset1;
    [Tooltip("�߰� ���� ��� �� ����� �߰� �ִϸ��̼� 2")]
    public AnimationReferenceAsset additionalAnimationAsset2;

    [Header("Key-Controlled Animations")]
    [Tooltip("Ű �Է����� ����� �ִϸ��̼�")]
    public AnimationReferenceAsset keyControlledAnimationAsset;

    public event Action OnDeathAnimationsCompleted;

    private Spine.AnimationState spineAnimationState;

    private void Awake()
    {
        // SkeletonAnimation ������Ʈ �Ҵ�
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation == null)
            {
                Debug.LogError("SkeletonAnimation component is not assigned or found on " + gameObject.name);
                return;
            }
        }

        // AnimationState �ʱ�ȭ
        spineAnimationState = skeletonAnimation.AnimationState;

        if (spineAnimationState == null)
        {
            Debug.LogError("AnimationState is null in SkeletonAnimation.");
            return;
        }

        // �ʱ� Standard �ִϸ��̼� ���� (Ʈ�� 0, ����)
        SetStandardAnimation();

        // �ִϸ��̼� �Ϸ� �̺�Ʈ �ڵ鷯 ���
        spineAnimationState.Complete += OnAnimationComplete;
    }

    private void OnDestroy()
    {
        // �̺�Ʈ �ڵ鷯 ����
        if (spineAnimationState != null)
        {
            spineAnimationState.Complete -= OnAnimationComplete;
        }
    }

    /// <summary>
    /// Standard �ִϸ��̼��� Ʈ�� 0���� ���� ����մϴ�.
    /// </summary>
    private void SetStandardAnimation()
    {
        if (standardAnimationAsset == null || standardAnimationAsset.Animation == null)
        {
            Debug.LogError("StandardAnimationAsset�� �Ҵ���� �ʾҰų� Animation�� �����ϴ�.");
            return;
        }
        if (spineAnimationState == null)
        {
            Debug.LogError("AnimationState is not initialized.");
            return;
        }
        spineAnimationState.SetAnimation(0, standardAnimationAsset.Animation.Name, true).MixDuration = 0.2f;
        Debug.Log($"Standard �ִϸ��̼� ����: {standardAnimationAsset.Animation.Name}");
    }

    /// <summary>
    /// Death �ִϸ��̼��� Ʈ�� 1���� ���������� ����մϴ�.
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

        // Death �ִϸ��̼� ��� ����
        PlayFirstDeathAnimation();
    }

    /// <summary>
    /// ù ��° Death �ִϸ��̼� ���
    /// </summary>
    private void PlayFirstDeathAnimation()
    {
        if (firstDeathAnimationAsset == null)
        {
            Debug.LogError("FirstDeathAnimationAsset�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }
        if (firstDeathAnimationAsset.Animation == null)
        {
            Debug.LogError("FirstDeathAnimationAsset�� Animation�� �����ϴ�.");
            return;
        }

        int deathTrack = 1; // Death �ִϸ��̼��� ���� ���� Ʈ�� ���

        spineAnimationState.SetAnimation(deathTrack, firstDeathAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"ù ��° �ִϸ��̼� ����: {firstDeathAnimationAsset.Animation.Name}");
    }

    /// <summary>
    /// �� ��° Death �ִϸ��̼� ���
    /// </summary>
    private void PlaySecondDeathAnimation()
    {
        if (secondDeathAnimationAsset == null || secondDeathAnimationAsset.Animation == null)
        {
            Debug.LogError("SecondDeathAnimationAsset�� �Ҵ���� �ʾҰų� Animation�� �����ϴ�.");
            return;
        }

        int deathTrack = 1; // ���� Ʈ������ ���

        spineAnimationState.SetAnimation(deathTrack, secondDeathAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"�� ��° �ִϸ��̼� ����: {secondDeathAnimationAsset.Animation.Name}");
    }

    /// <summary>
    /// �߰� �ִϸ��̼� 1 ���
    /// </summary>
    public void PlayAdditionalAnimation1()
    {
        if (additionalAnimationAsset1 == null || additionalAnimationAsset1.Animation == null)
        {
            Debug.LogError("AdditionalAnimationAsset1�� �Ҵ���� �ʾҰų� Animation�� �����ϴ�.");
            return;
        }

        int additionalTrack = 2; // �߰� �ִϸ��̼��� ���� ���� Ʈ��

        spineAnimationState.SetAnimation(additionalTrack, additionalAnimationAsset1.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"�߰� �ִϸ��̼� 1 ����: {additionalAnimationAsset1.Animation.Name}");
    }

    /// <summary>
    /// �߰� �ִϸ��̼� 2 ���
    /// </summary>
    public void PlayAdditionalAnimation2()
    {
        if (additionalAnimationAsset2 == null || additionalAnimationAsset2.Animation == null)
        {
            Debug.LogError("AdditionalAnimationAsset2�� �Ҵ���� �ʾҰų� Animation�� �����ϴ�.");
            return;
        }

        int additionalTrack = 2; // �߰� �ִϸ��̼��� ���� ���� Ʈ��

        spineAnimationState.SetAnimation(additionalTrack, additionalAnimationAsset2.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"�߰� �ִϸ��̼� 2 ����: {additionalAnimationAsset2.Animation.Name}");
    }

    /// <summary>
    /// Ű �Է����� ������ �ִϸ��̼� ���
    /// </summary>
    public void PlayKeyControlledAnimation()
    {
        if (keyControlledAnimationAsset == null || keyControlledAnimationAsset.Animation == null)
        {
            Debug.LogError("KeyControlledAnimationAsset�� �Ҵ���� �ʾҰų� Animation�� �����ϴ�.");
            return;
        }

        int keyControlTrack = 3; // Ű ���� �ִϸ��̼��� ���� ���� Ʈ��

        spineAnimationState.SetAnimation(keyControlTrack, keyControlledAnimationAsset.Animation.Name, false).MixDuration = 0.2f;
        Debug.Log($"Ű ���� �ִϸ��̼� ����: {keyControlledAnimationAsset.Animation.Name}");
    }

    /// <summary>
    /// �ִϸ��̼� �Ϸ� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯
    /// </summary>
    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (trackEntry.TrackIndex == 1)
        {
            if (trackEntry.Animation.Name == firstDeathAnimationAsset.Animation.Name)
            {
                // ù ��° Death �ִϸ��̼� �Ϸ� �� �� ��° �ִϸ��̼� ���
                PlaySecondDeathAnimation();
            }
            else if (trackEntry.Animation.Name == secondDeathAnimationAsset.Animation.Name)
            {
                // �� ��° Death �ִϸ��̼� �Ϸ� �� �߰� �ִϸ��̼� ��� �Ǵ� �̺�Ʈ ȣ��
                Debug.Log("��� ���� �ִϸ��̼� �Ϸ�");

                // �߰� �ִϸ��̼��� �ڵ����� ����Ϸ��� �Ʒ� �ּ��� �����ϼ���.
                // PlayAdditionalAnimation1();
                // PlayAdditionalAnimation2();

                // �̺�Ʈ ȣ��
                OnDeathAnimationsCompleted?.Invoke();

                // �ʿ��� �߰� ���� (��: ���� ������Ʈ �ı�) ���⼭ ó�� ����
            }
        }
    }

    /// <summary>
    /// Ű �Է��� ���� �ִϸ��̼��� �����մϴ�.
    /// ����: Update �޼��忡�� Ű �Է��� �����Ͽ� �ִϸ��̼��� ����� �� �ֽ��ϴ�.
    /// </summary>
    private void Update()
    {
        // �⺻���� Ű �Է� ����
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

        // Ȱ��ȭ�� ���� ����Ʈ�� ������Ʈ�Ͽ� ���� ���͸� ����
        // �ʿ��� ��� ���⿡ �߰� ������ ���� �� �ֽ��ϴ�.
    }
}
