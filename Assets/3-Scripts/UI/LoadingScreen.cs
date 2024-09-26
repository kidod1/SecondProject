using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Spine.Unity;

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

    [SerializeField]
    private GameObject noiseImage;  // 노이즈가 적용된 UI 이미지 (노이즈 효과가 끝나면 비활성화할 이미지)

    [SerializeField]
    private GameObject loadingImage;  // 로딩 중에 표시될 이미지 (추가됨)

    [SerializeField]
    private SkeletonGraphic skeletonGraphic;  // Spine SkeletonGraphic 컴포넌트

    [SerializeField]
    private AnimationReferenceAsset idleAnimationAsset;  // 로딩 중 재생할 애니메이션 참조

    [SerializeField]
    private AnimationReferenceAsset completeAnimationAsset;  // 로딩 완료 후 재생할 애니메이션 참조

    [SerializeField]
    private GameObject[] resultImages;  // 로딩 완료 후 나타날 이미지 배열

    // 쉐이더가 적용된 Material
    [SerializeField]
    private Material noiseMaterial;

    // 노이즈 강도가 줄어드는 속도
    [SerializeField]
    private float noiseFadeSpeed = 1f;

    // 노이즈 강도의 초기값 (최대)
    private const float noiseMaxStrength = 1f;

    private bool isLoadingComplete = false;
    private bool isNoiseFading = false;

    private void Start()
    {
        // 로딩 바 초기화 및 채우기 시작
        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 0f;
            StartCoroutine(FillLoadingBar());
        }

        // 로딩 완료 텍스트 비활성화
        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(false);
        }

        // 노이즈 UI 이미지 비활성화
        if (noiseImage != null)
        {
            noiseImage.SetActive(false);  // 초기 상태에서는 비활성화
        }

        // 노이즈 강도를 최대치로 초기화
        if (noiseMaterial != null)
        {
            noiseMaterial.SetFloat("_NoiseStrength", noiseMaxStrength);  // 노이즈 강도를 최대치로 설정
        }

        // Spine 애니메이션 설정 (로딩 시작 시)
        if (skeletonGraphic != null && idleAnimationAsset != null && idleAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(0, idleAnimationAsset.Animation.Name, true);  // 로딩 시작 애니메이션 재생
        }

        // 로딩 완료 후 나올 이미지들 비활성화
        foreach (var resultImage in resultImages)
        {
            if (resultImage != null)
            {
                resultImage.SetActive(false);
            }
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

        // 로딩 완료 후 텍스트 활성화
        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(true);
        }

        // 로딩 이미지 비활성화
        if (loadingImage != null)
        {
            loadingImage.SetActive(false);  // 로딩 이미지 비활성화
        }

        // 노이즈 이미지 활성화 및 노이즈 페이드 아웃 시작
        if (noiseImage != null)
        {
            noiseImage.SetActive(true);  // 노이즈 이미지 활성화
            StartCoroutine(FadeOutNoise());  // 노이즈 페이드 아웃 시작
        }

        // Spine 애니메이션 트리거
        if (skeletonGraphic != null && completeAnimationAsset != null && completeAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(0, completeAnimationAsset.Animation.Name, true);  // 로딩 완료 애니메이션 재생
        }

        // 결과 이미지 배열 활성화
        foreach (var resultImage in resultImages)
        {
            if (resultImage != null)
            {
                resultImage.SetActive(true);  // 각 이미지 활성화
            }
        }
    }

    // 노이즈 효과를 서서히 줄이면서 UI(노이즈 이미지)를 비활성화
    private IEnumerator FadeOutNoise()
    {
        float noiseStrength = noiseMaterial.GetFloat("_NoiseStrength");

        while (noiseStrength > 0)
        {
            noiseStrength -= Time.deltaTime * noiseFadeSpeed;
            noiseMaterial.SetFloat("_NoiseStrength", Mathf.Clamp01(noiseStrength));
            yield return null;
        }

        // 노이즈가 0이 되면 노이즈 UI 이미지를 비활성화
        if (noiseStrength <= 0.5f)
        {
            noiseImage.SetActive(false);
        }

        // Space 키 입력 대기 후 씬 전환
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }
}
