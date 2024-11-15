using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DangerWarningController : MonoBehaviour
{
    [Header("��� UI ����")]
    [Tooltip("��� �ؽ�Ʈ �Ǵ� �̹���")]
    [SerializeField] private GameObject warningUI;

    [Header("�÷��� ȿ�� ����")]
    [Tooltip("�÷��ø� ����ϴ� ��ü ȭ�� �̹���")]
    [SerializeField] private Image flashOverlay;

    [Tooltip("�÷��� ȿ���� �ӵ� (�� ����)")]
    [SerializeField] private float flashSpeed = 0.5f;

    [Tooltip("�÷��� ȿ���� ���� (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float flashIntensity = 0.5f;

    [Header("�̺�Ʈ ����")]
    [Tooltip("�÷��� ȿ���� �Ϸ�Ǿ��� �� ȣ��Ǵ� �̺�Ʈ")]
    [SerializeField] private UnityEvent OnFlashComplete;

    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (warningUI != null)
            warningUI.SetActive(false); // �ʱ⿡�� ��� UI�� ����ϴ�.

        if (flashOverlay != null)
            flashOverlay.color = new Color(flashOverlay.color.r, flashOverlay.color.g, flashOverlay.color.b, 0f); // �ʱ� �÷��� ���� �����ϰ� ����
    }

    /// <summary>
    /// ��� UI�� ǥ���մϴ�.
    /// </summary>
    public void ShowWarning()
    {
        if (warningUI != null)
            warningUI.SetActive(true);
    }

    /// <summary>
    /// ��� UI�� ����ϴ�.
    /// </summary>
    public void HideWarning()
    {
        if (warningUI != null)
            warningUI.SetActive(false);
    }

    /// <summary>
    /// �÷��� ȿ���� �����մϴ�.
    /// </summary>
    public void StartFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    /// <summary>
    /// �÷��� ȿ���� ����ϴ�.
    /// </summary>
    public void StopFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        if (flashOverlay != null)
            flashOverlay.color = new Color(flashOverlay.color.r, flashOverlay.color.g, flashOverlay.color.b, 0f);
    }

    /// <summary>
    /// �÷��� ȿ���� �ݺ������� �����ϴ� �ڷ�ƾ
    /// </summary>
    private IEnumerator FlashRoutine()
    {
        while (true)
        {
            // �÷��� ���� (������ ���� ������)
            yield return StartCoroutine(FadeOverlay(0f, flashIntensity, flashSpeed));
            // �÷��� �� (���� ������ ��������)
            yield return StartCoroutine(FadeOverlay(flashIntensity, 0f, flashSpeed));
        }
    }

    /// <summary>
    /// ���������� ���� ���� ������ �����ϴ� �ڷ�ƾ
    /// </summary>
    /// <param name="startAlpha">���� ���� ��</param>
    /// <param name="endAlpha">���� ���� ��</param>
    /// <param name="duration">���� �ð�</param>
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

        // ���� ���� �� ����
        if (flashOverlay != null)
            flashOverlay.color = new Color(flashOverlay.color.r, flashOverlay.color.g, flashOverlay.color.b, endAlpha);
    }

    /// <summary>
    /// �÷��� ȿ���� �� �� �����ϰ� �Ϸ� �� �̺�Ʈ�� ȣ���մϴ�.
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
