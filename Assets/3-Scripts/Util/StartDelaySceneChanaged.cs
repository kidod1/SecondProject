using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartDelaySceneChanged : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName; // 다음 씬의 이름을 할당
    [SerializeField]
    private Image fadeOutImage; // 페이드 아웃에 사용할 이미지
    [SerializeField]
    private float fadeDuration = 1f; // 페이드 아웃에 걸리는 시간

    private void Start()
    {
        if (fadeOutImage == null)
        {
            Debug.LogError("FadeOutImage가 할당되지 않았습니다.");
            return;
        }

        // 처음에 이미지의 알파를 0으로 설정 (투명 상태)
        fadeOutImage.gameObject.SetActive(true);
        Color initialColor = fadeOutImage.color;
        initialColor.a = 0f;
        fadeOutImage.color = initialColor;

        // 코루틴 시작
        StartCoroutine(DelayedSceneChange());
    }

    private IEnumerator DelayedSceneChange()
    {
        // 3초 대기
        yield return new WaitForSeconds(3f);

        // 페이드 아웃 시작
        yield return StartCoroutine(FadeOut());

        // 다음 씬 로드
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = fadeOutImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            color.a = alpha;
            fadeOutImage.color = color;
            yield return null;
        }

        // 최종적으로 알파를 1로 설정하여 완전히 불투명하게 만듦
        color.a = 1f;
        fadeOutImage.color = color;
    }
}
