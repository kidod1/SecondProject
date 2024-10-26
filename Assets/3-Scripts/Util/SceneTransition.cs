using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public string targetSceneName; // �̵��� ���� �̸�

    private bool isTransitioning = false; // �ߺ� ���� ������ �÷���

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton;

    private void Start()
    {

        if (sceneChangeSkeleton == null)
        {
            Debug.LogError("SceneChangeSkeleton ��ũ��Ʈ�� ���� ������Ʈ�� ���� �������� �ʽ��ϴ�.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTransitioning && other.CompareTag("Player"))
        {
            sceneChangeSkeleton.gameObject.SetActive(true);
            isTransitioning = true; // �ߺ� ���� ����

            if (sceneChangeSkeleton != null)
            {
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
