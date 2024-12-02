using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events; // UnityEvent�� ����ϱ� ���� �߰�
using System.Collections;

public class SceneChangeSkeleton : MonoBehaviour
{
    [Header("Skeleton Graphic Settings")]
    [Tooltip("Spine SkeletonGraphic ������Ʈ")]
    public SkeletonGraphic skeletonGraphic;

    [Tooltip("Close �ִϸ��̼� �̸�")]
    [SpineAnimation] public string closeAnimationName;

    [Tooltip("Open �ִϸ��̼� �̸�")]
    [SpineAnimation] public string openAnimationName;

    [Tooltip("�� ��ȯ ������ (��)")]
    public float sceneChangeDelay = 0f;

    [Tooltip("�� ��ȣ")]
    public int targetSceneNumber;

    [Header("Unity Events")]
    [Tooltip("Close �ִϸ��̼��� �Ϸ�� �� ȣ��˴ϴ�.")]
    public UnityEvent OnCloseAnimationComplete;

    [Tooltip("Open �ִϸ��̼��� �Ϸ�� �� ȣ��˴ϴ�.")]
    public UnityEvent OnOpenAnimationComplete;

    // ���� ���� ���� ����
    private bool isAnimating = false;
    private bool isOpening = false;

    // Close �ִϸ��̼� �Ϸ� �� Open �ִϸ��̼� ������ ���� ������
    private float delayAfterClose = 1.5f;

    void Awake()
    {
        if (skeletonGraphic == null)
        {
            skeletonGraphic = GetComponent<SkeletonGraphic>();
            if (skeletonGraphic == null)
            {
                Debug.LogError("SkeletonGraphic ������Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
            }
        }

        if (FindObjectsOfType<SceneChangeSkeleton>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // �ִϸ��̼� �Ϸ� �� �̺�Ʈ ������ ���
        if (skeletonGraphic.AnimationState != null)
        {
            skeletonGraphic.AnimationState.Complete += OnAnimationComplete;
        }
        else
        {
            Debug.LogError("SkeletonGraphic�� AnimationState�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
        }

        // ���� �ε�� �� ȣ��Ǵ� �̺�Ʈ ������ ���
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (skeletonGraphic != null && skeletonGraphic.AnimationState != null)
        {
            skeletonGraphic.AnimationState.Complete -= OnAnimationComplete;
        }

        // �� �ε� �̺�Ʈ ������ ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Close �ִϸ��̼��� ����ϰ� ���� �����մϴ�.
    /// </summary>
    /// <param name="sceneIndex">��ȯ�� ���� ��ȣ</param>
    public void PlayCloseAnimation(int sceneIndex)
    {
        if (isAnimating)
        {
            Debug.LogWarning("�̹� �ִϸ��̼��� ���� ���Դϴ�.");
            return;
        }

        if (skeletonGraphic == null)
        {
            Debug.LogError("SkeletonGraphic ������Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (string.IsNullOrEmpty(closeAnimationName))
        {
            Debug.LogError("Close �ִϸ��̼� �̸��� �������� �ʾҽ��ϴ�.");
            return;
        }

        if (sceneIndex < 0)
        {
            Debug.LogError("Target Scene Index�� �ùٸ��� �ʽ��ϴ�.");
            return;
        }
        Time.timeScale = 1;

        gameObject.SetActive(true); // ������Ʈ Ȱ��ȭ
        targetSceneNumber = sceneIndex;
        isAnimating = true;
        isOpening = false;
        skeletonGraphic.AnimationState.SetAnimation(0, closeAnimationName, false);
        // Close �ִϸ��̼� �Ϸ� �� �̺�Ʈ ȣ��
        OnCloseAnimationComplete?.Invoke();
    }

    /// <summary>
    /// Open �ִϸ��̼��� ����մϴ�.
    /// </summary>
    public void PlayOpenAnimation()
    {
        if (isAnimating)
        {
            Debug.LogWarning("�̹� �ִϸ��̼��� ���� ���Դϴ�.");
            return;
        }

        if (skeletonGraphic == null)
        {
            Debug.LogError("SkeletonGraphic ������Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (string.IsNullOrEmpty(openAnimationName))
        {
            Debug.LogError("Open �ִϸ��̼� �̸��� �������� �ʾҽ��ϴ�.");
            return;
        }
        Time.timeScale = 1;
        isAnimating = true;
        isOpening = true;
        gameObject.SetActive(true); // ������Ʈ Ȱ��ȭ
        skeletonGraphic.AnimationState.SetAnimation(0, openAnimationName, false);
        // Open �ִϸ��̼� �Ϸ� �� �̺�Ʈ ȣ��
        OnOpenAnimationComplete?.Invoke();
    }

    /// <summary>
    /// �ִϸ��̼� �Ϸ� �� ȣ��Ǵ� �޼���
    /// </summary>
    /// <param name="trackEntry">�Ϸ�� �ִϸ��̼� Ʈ�� ��Ʈ��</param>
    private void OnAnimationComplete(Spine.TrackEntry trackEntry)
    {
        if (isOpening)
        {
            // Open �ִϸ��̼� �Ϸ� ��
            StartCoroutine(HandleOpenAnimationComplete());
        }
        else
        {
            // Close �ִϸ��̼� �Ϸ� ��
            StartCoroutine(HandleCloseAnimationComplete());
        }
    }

    /// <summary>
    /// Close �ִϸ��̼� �Ϸ� �� ������ �� Open �ִϸ��̼��� ����մϴ�.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleCloseAnimationComplete()
    {
        // ������
        yield return new WaitForSeconds(delayAfterClose);

        // isAnimating�� false�� �����Ͽ� PlayOpenAnimation�� ����� �� �ֵ��� ��
        isAnimating = false;

        // Open �ִϸ��̼� ���
        PlayOpenAnimation();
    }

    /// <summary>
    /// Open �ִϸ��̼� �Ϸ� �� �� ��ȯ ������ �� ���� �����մϴ�.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleOpenAnimationComplete()
    {
        // �� ��ȯ ������
        yield return new WaitForSeconds(sceneChangeDelay);

        // �� ��ȯ
        PlayManager.I.ChangeScene(targetSceneNumber);
    }

    /// <summary>
    /// ���� �ε�� �� Open �ִϸ��̼��� ����մϴ�.
    /// </summary>
    /// <param name="scene">�ε�� ��</param>
    /// <param name="mode">�� �ε� ���</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� �ִϸ��̼��� ��� ���� �ƴϰ� Open �ִϸ��̼� �̸��� �����Ǿ� ���� ���� ����
        if (!isAnimating && !string.IsNullOrEmpty(openAnimationName))
        {
            PlayManager.I.StopAllSounds();
            PlayOpenAnimation();
        }
    }
}
