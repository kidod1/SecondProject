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

        // 코루틴 시작 전에 플레이어 데이터 로드
        LoadPlayerData();

        StartCoroutine(ZoomEffect());
    }

    private IEnumerator ZoomEffect()
    {
        float startOrthoSize = cinemachineCamera.m_Lens.OrthographicSize;

        isZooming = true;

        // 느린 줌 인
        float elapsedTime = 0f;
        while (elapsedTime < slowZoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slowZoomDuration;
            cinemachineCamera.m_Lens.OrthographicSize = Mathf.Lerp(startOrthoSize, (targetOrthoSize + originalOrthoSize) / 2, t);
            yield return null;
        }

        // 빠른 줌 아웃
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

    private void LoadPlayerData()
    {
        PlayerDataManager dataManager = PlayerDataManager.Instance;
        if (dataManager != null)
        {
            dataManager.LoadPlayerData();
            Debug.Log("플레이어 데이터가 성공적으로 로드되었습니다.");
        }
        else
        {
            Debug.LogError("PlayerDataManager 인스턴스가 존재하지 않습니다. 플레이어 데이터를 로드할 수 없습니다.");
        }
    }
}
