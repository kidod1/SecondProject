using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker2 : MonoBehaviour
{
    public Light2D light2D;  // 2D ����Ʈ ������Ʈ
    public float flickerDuration = 2.0f; // ��Ⱑ ������ ���ϴ� �� �ɸ��� �ð�
    public float minIntensity = 0.5f;    // �ּ� ���
    public float maxIntensity = 1.5f;    // �ִ� ���

    private float timeElapsed = 0.0f;
    private bool isIncreasing = true;    // ��Ⱑ ���� ������ ����

    void Update()
    {
        // �ð��� �����鼭 ��⸦ ���� ������Ŵ
        if (isIncreasing)
        {
            timeElapsed += Time.unscaledDeltaTime;

            // ���� ��� ���
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, timeElapsed / flickerDuration);

            // Light2D ������Ʈ�� intensity �� ����
            if (light2D != null)
            {
                light2D.intensity = intensity;
            }

            // �ִ� ��⿡ �����ϸ� ���� ����
            if (intensity >= maxIntensity)
            {
                isIncreasing = false;  // �� �̻� ��⸦ ������Ű�� ����
            }
        }
    }
}
