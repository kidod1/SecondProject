using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Tooltip("�� �̸�")]
    public string targetSceneName;

    // ���� ���� ���� ����
    private bool isAnimating = false;
    private bool isOpening = false;

    // Close �ִϸ��̼� �Ϸ� �� Open �ִϸ��̼� ������ ���� ������
    private float delayAfterClose = 2f;

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

        // Singleton ���� ���� (���� ����)
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
    /// <param name="sceneName">��ȯ�� ���� �̸�</param>
    public void PlayCloseAnimation(string sceneName)
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

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Target Scene Name�� �������� �ʾҽ��ϴ�.");
            return;
        }

        targetSceneName = sceneName;
        isAnimating = true;
        isOpening = false;
        gameObject.SetActive(true); // ������Ʈ Ȱ��ȭ
        skeletonGraphic.AnimationState.SetAnimation(0, closeAnimationName, false);
        Debug.Log($"Close �ִϸ��̼� '{closeAnimationName}' ��� ����.");
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

        isAnimating = true;
        isOpening = true;
        gameObject.SetActive(true); // ������Ʈ Ȱ��ȭ
        skeletonGraphic.AnimationState.SetAnimation(0, openAnimationName, false);
        Debug.Log($"Open �ִϸ��̼� '{openAnimationName}' ��� ����.");
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
            Debug.Log("Open �ִϸ��̼� �Ϸ�. �� ��ȯ �غ�.");
        }
        else
        {
            // Close �ִϸ��̼� �Ϸ� ��
            StartCoroutine(HandleCloseAnimationComplete());
            Debug.Log("Close �ִϸ��̼� �Ϸ�. Open �ִϸ��̼� ����� �����մϴ�.");
        }
    }

    /// <summary>
    /// Close �ִϸ��̼� �Ϸ� �� 2�� ������ �� Open �ִϸ��̼��� ����մϴ�.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleCloseAnimationComplete()
    {
        // 2�� ������
        yield return new WaitForSeconds(delayAfterClose);
        Debug.Log($"2�� ������ �� Open �ִϸ��̼� ��� ����.");

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
        Debug.Log($"�� ��ȯ ������ �� �� '{targetSceneName}' �ε� ����.");

        // �� ��ȯ
        SceneManager.LoadScene(targetSceneName);
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
            Debug.Log($"�� '{scene.name}' �ε� �Ϸ�. Open �ִϸ��̼� ���.");
            PlayOpenAnimation();
        }
    }
}
