using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraZoomEffect : MonoBehaviour
{
    public CinemachineVirtualCamera cinemachineCamera;
    public float targetOrthoSize = 10f;
    public float slowZoomDuration = 2f;
    public float fastZoomDuration = 1f;

    private float originalOrthoSize;
    private bool isZooming = false;

    private void Start()
    {
        originalOrthoSize = cinemachineCamera.m_Lens.OrthographicSize;
        StartCoroutine(ZoomEffect());
    }

    private IEnumerator ZoomEffect()
    {
        float startOrthoSize = cinemachineCamera.m_Lens.OrthographicSize;

        isZooming = true;

        // ¥¿∏∞ ¡‹ ¿Œ
        float elapsedTime = 0f;
        while (elapsedTime < slowZoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slowZoomDuration;
            cinemachineCamera.m_Lens.OrthographicSize = Mathf.Lerp(startOrthoSize, (targetOrthoSize + originalOrthoSize) / 2, t);
            yield return null;
        }

        // ∫¸∏• ¡‹ æ∆øÙ
        elapsedTime = 0f;
        while (elapsedTime < fastZoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fastZoomDuration;
            cinemachineCamera.m_Lens.OrthographicSize = Mathf.Lerp((targetOrthoSize + originalOrthoSize) / 2, originalOrthoSize, t);
            yield return null;
        }

        cinemachineCamera.m_Lens.OrthographicSize = originalOrthoSize;
        isZooming = false;
    }
}
