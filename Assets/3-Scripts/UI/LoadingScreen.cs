using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Spine.Unity;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Image loadingBarFill;  // �ε� ���� �̹��� (Fill Ÿ��)

    [SerializeField]
    private float loadingDuration = 5f;  // �ε� �ٰ� �� ���� �ð� (��)

    [SerializeField]
    private int nextSceneIndex;  // ��ȯ�� ���� ���� �ε��� ��ȣ

    [SerializeField]
    private GameObject pressSpaceText;  // �ε� �Ϸ� �� ��Ÿ�� �ؽ�Ʈ UI

    [SerializeField]
    private GameObject noiseImage;  // ����� ����� UI �̹��� (������ ȿ���� ������ ��Ȱ��ȭ�� �̹���)

    [SerializeField]
    private GameObject loadingImage;  // �ε� �߿� ǥ�õ� �̹��� (�߰���)

    [SerializeField]
    private SkeletonGraphic skeletonGraphic;  // Spine SkeletonGraphic ������Ʈ

    [SerializeField]
    private AnimationReferenceAsset idleAnimationAsset;  // �ε� �� ����� �ִϸ��̼� ����

    [SerializeField]
    private AnimationReferenceAsset completeAnimationAsset;  // �ε� �Ϸ� �� ����� �ִϸ��̼� ����

    [SerializeField]
    private GameObject[] resultImages;  // �ε� �Ϸ� �� ��Ÿ�� �̹��� �迭

    // ���̴��� ����� Material
    [SerializeField]
    private Material noiseMaterial;

    // ������ ������ �پ��� �ӵ�
    [SerializeField]
    private float noiseFadeSpeed = 1f;

    // ������ ������ �ʱⰪ (�ִ�)
    private const float noiseMaxStrength = 1f;

    private bool isLoadingComplete = false;
    private bool isNoiseFading = false;

    private void Start()
    {
        // �ε� �� �ʱ�ȭ �� ä��� ����
        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 0f;
            StartCoroutine(FillLoadingBar());
        }

        // �ε� �Ϸ� �ؽ�Ʈ ��Ȱ��ȭ
        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(false);
        }

        // ������ UI �̹��� ��Ȱ��ȭ
        if (noiseImage != null)
        {
            noiseImage.SetActive(false);  // �ʱ� ���¿����� ��Ȱ��ȭ
        }

        // ������ ������ �ִ�ġ�� �ʱ�ȭ
        if (noiseMaterial != null)
        {
            noiseMaterial.SetFloat("_NoiseStrength", noiseMaxStrength);  // ������ ������ �ִ�ġ�� ����
        }

        // Spine �ִϸ��̼� ���� (�ε� ���� ��)
        if (skeletonGraphic != null && idleAnimationAsset != null && idleAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(0, idleAnimationAsset.Animation.Name, true);  // �ε� ���� �ִϸ��̼� ���
        }

        // �ε� �Ϸ� �� ���� �̹����� ��Ȱ��ȭ
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

        // �ε� �Ϸ� �� �ؽ�Ʈ Ȱ��ȭ
        if (pressSpaceText != null)
        {
            pressSpaceText.SetActive(true);
        }

        // �ε� �̹��� ��Ȱ��ȭ
        if (loadingImage != null)
        {
            loadingImage.SetActive(false);  // �ε� �̹��� ��Ȱ��ȭ
        }

        // ������ �̹��� Ȱ��ȭ �� ������ ���̵� �ƿ� ����
        if (noiseImage != null)
        {
            noiseImage.SetActive(true);  // ������ �̹��� Ȱ��ȭ
            StartCoroutine(FadeOutNoise());  // ������ ���̵� �ƿ� ����
        }

        // Spine �ִϸ��̼� Ʈ����
        if (skeletonGraphic != null && completeAnimationAsset != null && completeAnimationAsset.Animation != null)
        {
            skeletonGraphic.AnimationState.SetAnimation(0, completeAnimationAsset.Animation.Name, true);  // �ε� �Ϸ� �ִϸ��̼� ���
        }

        // ��� �̹��� �迭 Ȱ��ȭ
        foreach (var resultImage in resultImages)
        {
            if (resultImage != null)
            {
                resultImage.SetActive(true);  // �� �̹��� Ȱ��ȭ
            }
        }
    }

    // ������ ȿ���� ������ ���̸鼭 UI(������ �̹���)�� ��Ȱ��ȭ
    private IEnumerator FadeOutNoise()
    {
        float noiseStrength = noiseMaterial.GetFloat("_NoiseStrength");

        while (noiseStrength > 0)
        {
            noiseStrength -= Time.deltaTime * noiseFadeSpeed;
            noiseMaterial.SetFloat("_NoiseStrength", Mathf.Clamp01(noiseStrength));
            yield return null;
        }

        // ����� 0�� �Ǹ� ������ UI �̹����� ��Ȱ��ȭ
        if (noiseStrength <= 0.5f)
        {
            noiseImage.SetActive(false);
        }

        // Space Ű �Է� ��� �� �� ��ȯ
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }
}
