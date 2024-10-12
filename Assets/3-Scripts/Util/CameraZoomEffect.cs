using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraZoomEffect : MonoBehaviour
{
    public CinemachineVirtualCamera cinemachineCamera;
    public float targetOrthoSize = 10f;
    public float zoomDuration = 2f;
    public float slowZoomDuration = 2f;
    public float fastZoomDuration = 1f;

    private float originalOrthoSize;
    private float elapsedTime = 0f;
    private bool isZooming = false;

    private void Start()
    {
        originalOrthoSize = cinemachineCamera.m_Lens.OrthographicSize;
        StartCoroutine(ZoomEffect());
    }

    private IEnumerator ZoomEffect()
    {
        Time.timeScale = 0f;

        float startOrthoSize = cinemachineCamera.m_Lens.OrthographicSize;
        cinemachineCamera.m_Lens.OrthographicSize = targetOrthoSize;

        isZooming = true;
        while (elapsedTime < slowZoomDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / slowZoomDuration;
            cinemachineCamera.m_Lens.OrthographicSize = Mathf.Lerp(targetOrthoSize, (targetOrthoSize + originalOrthoSize) / 2, t);
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < fastZoomDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / fastZoomDuration;
            cinemachineCamera.m_Lens.OrthographicSize = Mathf.Lerp((targetOrthoSize + originalOrthoSize) / 2, originalOrthoSize, t);
            yield return null;
        }

        cinemachineCamera.m_Lens.OrthographicSize = originalOrthoSize;
        elapsedTime = 0f;
        isZooming = false;

        Time.timeScale = 1f;
    }
}