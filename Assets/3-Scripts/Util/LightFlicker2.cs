using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker2 : MonoBehaviour
{
    public Light2D light2D;  // 2D 라이트 컴포넌트
    public float flickerDuration = 2.0f; // 밝기가 완전히 변하는 데 걸리는 시간
    public float minIntensity = 0.5f;    // 최소 밝기
    public float maxIntensity = 1.5f;    // 최대 밝기

    private float timeElapsed = 0.0f;
    private bool isIncreasing = true;    // 밝기가 증가 중인지 여부

    void Update()
    {
        // 시간이 지나면서 밝기를 점차 증가시킴
        if (isIncreasing)
        {
            timeElapsed += Time.unscaledDeltaTime;

            // 현재 밝기 계산
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, timeElapsed / flickerDuration);

            // Light2D 컴포넌트의 intensity 값 변경
            if (light2D != null)
            {
                light2D.intensity = intensity;
            }

            // 최대 밝기에 도달하면 증가 멈춤
            if (intensity >= maxIntensity)
            {
                isIncreasing = false;  // 더 이상 밝기를 증가시키지 않음
            }
        }
    }
}
