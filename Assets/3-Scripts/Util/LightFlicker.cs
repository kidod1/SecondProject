using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    public Light2D light2D;  // 2D ����Ʈ ������Ʈ
    public float flickerDuration = 2.0f; // ��Ⱑ ������ ���ϴ� �� �ɸ��� �ð�
    public float minIntensity = 0.5f;    // �ּ� ���
    public float maxIntensity = 1.5f;    // �ִ� ���

    private float timeElapsed = 0.0f;

    void Update()
    {
        // �ð��� ���� ��⸦ ����
        timeElapsed += Time.deltaTime;

        // Mathf.PingPong�� ����� �ð��� �����鼭 ��Ⱑ ������������ ��
        float intensity = Mathf.PingPong(timeElapsed / flickerDuration * (maxIntensity - minIntensity), maxIntensity - minIntensity) + minIntensity;

        // Light2D ������Ʈ�� intensity �� ����
        if (light2D != null)
        {
            light2D.intensity = intensity;
        }
    }
}
