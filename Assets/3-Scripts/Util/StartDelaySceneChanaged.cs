using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AK.Wwise; // WWISE 네임스페이스 추가

public class StartDelaySceneChanged : MonoBehaviour
{
    [SerializeField]
    private int nextSceneNumber; // 다음 씬의 이름을 할당

    [SerializeField]
    private Image fadeOutImage; // 페이드 아웃에 사용할 이미지

    [SerializeField]
    private float fadeDuration = 1f; // 페이드 아웃에 걸리는 시간

    [Header("WWISE Events")]
    [SerializeField]
    private AK.Wwise.Event onFadeInCompleteEvent; // 페이드 인 완료 시 실행할 WWISE 이벤트 추가

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
        // 페이드 인 완료 시 WWISE 이벤트 실행
        if (onFadeInCompleteEvent != null)
        {
            onFadeInCompleteEvent.Post(gameObject);
            Debug.Log("페이드 인 완료 시 WWISE 이벤트가 실행되었습니다.");
        }
        else
        {
            Debug.LogWarning("onFadeInCompleteEvent가 할당되지 않았습니다.");
        }

        // 페이드 아웃 시작
        yield return StartCoroutine(FadeOut());

        // 다음 씬 로드
        PlayManager.I.ChangeScene(nextSceneNumber);
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

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeOutImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            color.a = alpha;
            fadeOutImage.color = color;
            yield return null;
        }

        // 최종적으로 알파를 0으로 설정하여 완전히 투명하게 만듦
        color.a = 0f;
        fadeOutImage.color = color;
    }
}
