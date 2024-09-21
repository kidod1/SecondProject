using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    public Light2D light2D;  // 2D 라이트 컴포넌트
    public float flickerDuration = 2.0f; // 밝기가 완전히 변하는 데 걸리는 시간
    public float minIntensity = 0.5f;    // 최소 밝기
    public float maxIntensity = 1.5f;    // 최대 밝기

    private float timeElapsed = 0.0f;

    void Update()
    {
        // 시간에 따라 밝기를 변경
        timeElapsed += Time.deltaTime;

        // Mathf.PingPong을 사용해 시간이 지나면서 밝기가 오르내리도록 함
        float intensity = Mathf.PingPong(timeElapsed / flickerDuration * (maxIntensity - minIntensity), maxIntensity - minIntensity) + minIntensity;

        // Light2D 컴포넌트의 intensity 값 변경
        if (light2D != null)
        {
            light2D.intensity = intensity;
        }
    }
}
