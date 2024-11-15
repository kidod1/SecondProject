using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DangerWarningController : MonoBehaviour
{
    [Header("경고 UI 설정")]
    [Tooltip("경고 텍스트 또는 이미지")]
    [SerializeField] private GameObject warningUI;

    [Header("플래시 효과 설정")]
    [Tooltip("플래시를 담당하는 전체 화면 이미지")]
    [SerializeField] private Image flashOverlay;

    [Tooltip("플래시 효과의 속도 (초 단위)")]
    [SerializeField] private float flashSpeed = 0.5f;

    [Tooltip("플래시 효과의 강도 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float flashIntensity = 0.5f;

    [Header("이벤트 설정")]
    [Tooltip("플래시 효과가 완료되었을 때 호출되는 이벤트")]
    [SerializeField] private UnityEvent OnFlashComplete;

    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (warningUI != null)
            warningUI.SetActive(false); // 초기에는 경고 UI를 숨깁니다.

        if (flashOverlay != null)
            flashOverlay.color = new Color(flashOverlay.color.r, flashOverlay.color.g, flashOverlay.color.b, 0f); // 초기 플래시 색상 투명하게 설정
    }

    /// <summary>
    /// 경고 UI를 표시합니다.
    /// </summary>
    public void ShowWarning()
    {
        if (warningUI != null)
            warningUI.SetActive(true);
    }

    /// <summary>
    /// 경고 UI를 숨깁니다.
    /// </summary>
    public void HideWarning()
    {
        if (warningUI != null)
            warningUI.SetActive(false);
    }

    /// <summary>
    /// 플래시 효과를 시작합니다.
    /// </summary>
    public void StartFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    /// <summary>
    /// 플래시 효과를 멈춥니다.
    /// </summary>
    public void StopFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        if (flashOverlay != null)
            flashOverlay.color = new Color(flashOverlay.color.r, flashOverlay.color.g, flashOverlay.color.b, 0f);
    }

    /// <summary>
    /// 플래시 효과를 반복적으로 수행하는 코루틴
    /// </summary>
    private IEnumerator FlashRoutine()
    {
        while (true)
        {
            // 플래시 시작 (투명에서 강한 색으로)
            yield return StartCoroutine(FadeOverlay(0f, flashIntensity, flashSpeed));
            // 플래시 끝 (강한 색에서 투명으로)
            yield return StartCoroutine(FadeOverlay(flashIntensity, 0f, flashSpeed));
        }
    }

    /// <summary>
    /// 오버레이의 알파 값을 서서히 변경하는 코루틴
    /// </summary>
    /// <param name="startAlpha">시작 알파 값</param>
    /// <param name="endAlpha">종료 알파 값</param>
    /// <param name="duration">변경 시간</param>
    private IEnumerator FadeOverlay(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            if (flashOverlay != null)
                flashOverlay.color = new Color(flashOverlay.color.r, flashOverlay.color.g, flashOverlay.color.b, newAlpha);
            yield return null;
        }

        // 최종 알파 값 설정
        if (flashOverlay != null)
            flashOverlay.color = new Color(flashOverlay.color.r, flashOverlay.color.g, flashOverlay.color.b, endAlpha);
    }

    /// <summary>
    /// 플래시 효과를 한 번 수행하고 완료 시 이벤트를 호출합니다.
    /// </summary>
    public void FlashOnce()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        StartCoroutine(FlashOnceRoutine());
    }

    private IEnumerator FlashOnceRoutine()
    {
        yield return StartCoroutine(FadeOverlay(0f, flashIntensity, flashSpeed));
        yield return StartCoroutine(FadeOverlay(flashIntensity, 0f, flashSpeed));

        OnFlashComplete?.Invoke();
    }
}
