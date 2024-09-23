using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Image loadingBarFill;  // 로딩 바의 이미지 (Fill 타입)
    [SerializeField]
    private float loadingDuration = 5f;  // 로딩 바가 다 차는 시간 (초)
    [SerializeField]
    private int nextSceneIndex;  // 전환할 다음 씬의 인덱스 번호
    [SerializeField]
    private GameObject pressSpaceText;  // 로딩 완료 후 나타날 텍스트 UI

    private bool isLoadingComplete = false;

    private void Start()
    {
        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 0f;  // 로딩 바 초기화
            StartCoroutine(FillLoadingBar());  // 로딩 바 채우기 시작
        }
        else
        {
            Debug.LogError("LoadingScreen: 로딩 바 이미지가 할당되지 않았습니다.");
        }

        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(false);  // 텍스트 UI 비활성화
        }
        else
        {
            Debug.LogWarning("LoadingScreen: pressSpaceText가 할당되지 않았습니다.");
        }
    }

    private void Update()
    {
        if (isLoadingComplete && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(nextSceneIndex);  // 다음 씬으로 전환
        }
    }

    private IEnumerator FillLoadingBar()
    {
        float elapsedTime = 0f;

        while (elapsedTime < loadingDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float fillAmount = Mathf.Clamp01(elapsedTime / loadingDuration);
            loadingBarFill.fillAmount = fillAmount;

            yield return null;
        }

        isLoadingComplete = true;

        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(true);  // 로딩 완료 후 텍스트 UI 활성화
        }
        else
        {
            Debug.LogWarning("LoadingScreen: pressSpaceText가 할당되지 않았습니다.");
        }
    }
}
