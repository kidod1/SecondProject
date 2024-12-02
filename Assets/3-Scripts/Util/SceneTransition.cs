using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    public int targetSceneNumber; // �̵��� ���� �̸�

    private bool isTransitioning = false; // �ߺ� ���� ������ �÷���

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton;

    [Header("Portal Events")]
    [Tooltip("�÷��̾ ��Ż�� ����� �� ȣ��Ǵ� �̺�Ʈ")]
    public UnityEvent OnPortalEntered;

    // �߰��� �κ�: �� ��ȯ ��� ������ ���� ����
    [Header("Transition Method Settings")]
    [SerializeField]
    private bool useCloseAnimation = true; // CloseAnimation�� ������� ����

    private void Start()
    {
        if (sceneChangeSkeleton == null)
        {
            Debug.LogError("SceneChangeSkeleton ��ũ��Ʈ�� ���� ������Ʈ�� ���� �������� �ʽ��ϴ�.");
        }

        if (OnPortalEntered == null)
        {
            OnPortalEntered = new UnityEvent();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTransitioning && other.CompareTag("Player"))
        {
            isTransitioning = true; // �ߺ� ���� ������ �÷��� ����

            // ��Ż ��� �� �̺�Ʈ ȣ��
            OnPortalEntered?.Invoke();

            if (useCloseAnimation)
            {
                // CloseAnimation�� ����ϴ� ���
                if (sceneChangeSkeleton != null)
                {
                    sceneChangeSkeleton.gameObject.SetActive(true);
                    // SceneChangeSkeleton�� ���� �� ��ȯ �ִϸ��̼� ���
                    sceneChangeSkeleton.PlayCloseAnimation(targetSceneNumber);
                }
                else
                {
                    Debug.LogWarning("SceneChangeSkeleton�� �Ҵ���� �ʾҽ��ϴ�. �ٷ� ���� �ε��մϴ�.");
                    // SceneChangeSkeleton�� ���� ��� �ٷ� �� �ε�
                    LoadNextScene();
                }
            }
            else
            {
                // SceneManager.LoadScene�� ����ϴ� ���
                LoadNextScene();
            }
        }
    }

    /// <summary>
    /// ���� ���� �ε��ϴ� �޼���
    /// </summary>
    private void LoadNextScene()
    {
        // �� �ε�
        PlayManager.I.ChangeScene(targetSceneNumber);
    }
}
