using UnityEngine;
using UnityEngine.Events;

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    public string targetSceneName; // �̵��� ���� �̸�

    private bool isTransitioning = false; // �ߺ� ���� ������ �÷���

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton;

    [Header("Portal Events")]
    [Tooltip("�÷��̾ ��Ż�� ����� �� ȣ��Ǵ� �̺�Ʈ")]
    public UnityEvent OnPortalEntered;

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

            if (sceneChangeSkeleton != null)
            {
                sceneChangeSkeleton.gameObject.SetActive(true);
                // SceneChangeSkeleton�� ���� �� ��ȯ �ִϸ��̼� ���
                sceneChangeSkeleton.PlayCloseAnimation(targetSceneName);
            }
            else
            {
                // SceneChangeSkeleton�� ���� ��� �ٷ� �� �ε�
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}
