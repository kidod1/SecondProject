using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public string targetSceneName; // 이동할 씬의 이름

    private bool isTransitioning = false; // 중복 실행 방지용 플래그

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton;

    private void Start()
    {

        if (sceneChangeSkeleton == null)
        {
            Debug.LogError("SceneChangeSkeleton 스크립트를 가진 오브젝트가 씬에 존재하지 않습니다.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTransitioning && other.CompareTag("Player"))
        {
            sceneChangeSkeleton.gameObject.SetActive(true);
            isTransitioning = true; // 중복 실행 방지

            if (sceneChangeSkeleton != null)
            {
                // SceneChangeSkeleton을 통해 씬 전환 애니메이션 재생
                sceneChangeSkeleton.PlayCloseAnimation(targetSceneName);
            }
            else
            {
                // SceneChangeSkeleton이 없을 경우 바로 씬 로드
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}
