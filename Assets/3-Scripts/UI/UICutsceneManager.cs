using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UICutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneFrame
    {
        public Sprite sprite; // 표시할 스프라이트
        public Image image;   // 스프라이트가 할당될 Image 컴포넌트
    }

    [System.Serializable]
    public class CutscenePage
    {
        public CutsceneFrame[] frames; // 각 페이지에 속한 컷신 프레임들
    }

    [SerializeField]
    private CutscenePage[] cutscenePages; // 페이지별 컷신 프레임 배열
    [SerializeField]
    private float fadeDuration = 1f;     // 페이드 인/아웃 지속 시간
    [SerializeField]
    private float initialDelay = 2f;     // 씬 시작 후 첫 컷 시작 전 지연 시간
    [SerializeField]
    private string loadSceneName = "";   // 컷신 종료 후 로드할 씬 이름

    [SerializeField]
    private SceneChangeSkeleton sceneChangeSkeleton; // SceneChangeSkeleton 참조 (인스펙터에서 할당)

    private int currentPageIndex = 0;       // 현재 페이지 인덱스
    private int currentFrameIndex = 0;      // 현재 프레임 인덱스
    private bool isTransitioning = false;   // 현재 전환 중인지 여부

    private void Start()
    {
        // 초기 설정: 모든 Image를 투명하게 설정하고 비활성화
        foreach (var page in cutscenePages)
        {
            foreach (var frame in page.frames)
            {
                if (frame.image != null)
                {
                    frame.image.color = new Color(frame.image.color.r, frame.image.color.g, frame.image.color.b, 0f);
                    frame.image.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("CutsceneFrame.image가 할당되지 않았습니다.");
                }
            }
        }

        // 컷신 페이지 배열 확인
        if (cutscenePages.Length > 0)
        {
            // 초기 지연 후 첫 컷신 시작
            StartCoroutine(InitialDelayAndStart());
        }
        else
        {
            Debug.LogWarning("CutscenePages 배열이 비어있습니다.");
        }
    }

    private IEnumerator InitialDelayAndStart()
    {
        yield return new WaitForSeconds(initialDelay);
        // 첫 프레임 표시
        ShowNextFrame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTransitioning)
        {
            // 현재 프레임 또는 페이지 전환을 위한 스페이스바 입력 처리
            ShowNextFrame();
        }
    }

    private void ShowNextFrame()
    {
        if (currentPageIndex >= cutscenePages.Length)
        {
            // 모든 페이지가 끝나면 씬 전환
            if (sceneChangeSkeleton != null && !string.IsNullOrEmpty(loadSceneName))
            {
                sceneChangeSkeleton.PlayCloseAnimation(loadSceneName);
            }
            else
            {
                Debug.LogError("SceneChangeSkeleton이 할당되지 않았거나 loadSceneName이 설정되지 않았습니다.");
            }
            return;
        }

        CutscenePage currentPage = cutscenePages[currentPageIndex];

        if (currentFrameIndex < currentPage.frames.Length)
        {
            // 다음 프레임 표시
            StartCoroutine(FadeInFrame(currentPage.frames[currentFrameIndex]));
            currentFrameIndex++;
        }
        else
        {
            // 현재 페이지의 모든 프레임이 표시된 상태에서 스페이스바 입력 시 페이지 전환
            StartCoroutine(TransitionToNextPage());
        }
    }

    private IEnumerator FadeInFrame(CutsceneFrame frame)
    {
        if (frame.image == null)
        {
            yield break;
        }

        frame.image.sprite = frame.sprite;
        frame.image.gameObject.SetActive(true);

        float elapsed = 0f;
        Color color = frame.image.color;
        color.a = 0f;
        frame.image.color = color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            color.a = alpha;
            frame.image.color = color;
            yield return null;
        }

        color.a = 1f;
        frame.image.color = color;
    }

    private IEnumerator TransitionToNextPage()
    {
        isTransitioning = true;

        // 현재 페이지의 모든 프레임 페이드 아웃
        CutscenePage currentPage = cutscenePages[currentPageIndex];
        foreach (var frame in currentPage.frames)
        {
            if (frame.image != null && frame.image.gameObject.activeSelf)
            {
                StartCoroutine(FadeOutFrame(frame));
            }
        }

        // 페이드 아웃이 완료될 때까지 대기
        yield return new WaitForSeconds(fadeDuration);

        // 모든 프레임 비활성화
        foreach (var frame in currentPage.frames)
        {
            if (frame.image != null)
            {
                frame.image.gameObject.SetActive(false);
            }
        }

        // 추가 대기 시간 (2초)
        yield return new WaitForSeconds(2f);

        // 다음 페이지로 이동
        currentPageIndex++;
        currentFrameIndex = 0;

        if (currentPageIndex < cutscenePages.Length)
        {
            // 다음 페이지의 첫 프레임을 준비 (사용자가 스페이스바를 누를 때까지 대기)
            isTransitioning = false;
        }
        else
        {
            // 모든 페이지가 끝나면 씬 전환
            if (sceneChangeSkeleton != null && !string.IsNullOrEmpty(loadSceneName))
            {
                sceneChangeSkeleton.PlayCloseAnimation(loadSceneName);
            }
            else
            {
                Debug.LogError("SceneChangeSkeleton이 할당되지 않았거나 loadSceneName이 설정되지 않았습니다.");
            }
        }

        isTransitioning = false;
    }

    private IEnumerator FadeOutFrame(CutsceneFrame frame)
    {
        float elapsed = 0f;
        Color color = frame.image.color;
        color.a = 1f;
        frame.image.color = color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            color.a = alpha;
            frame.image.color = color;
            yield return null;
        }

        color.a = 0f;
        frame.image.color = color;
    }
}
