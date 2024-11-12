using UnityEngine;
using UnityEngine.Events;

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    public string targetSceneName; // 이동할 씬의 이름

    private bool isTransitioning = false; // 중복 실행 방지용 플래그

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton;

    [Header("Portal Events")]
    [Tooltip("플레이어가 포탈을 통과할 때 호출되는 이벤트")]
    public UnityEvent OnPortalEntered;

    private void Start()
    {
        if (sceneChangeSkeleton == null)
        {
            Debug.LogError("SceneChangeSkeleton 스크립트를 가진 오브젝트가 씬에 존재하지 않습니다.");
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
            isTransitioning = true; // 중복 실행 방지용 플래그 설정

            // 포탈 통과 시 이벤트 호출
            OnPortalEntered?.Invoke();

            if (sceneChangeSkeleton != null)
            {
                sceneChangeSkeleton.gameObject.SetActive(true);
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
