using System.Collections;
using UnityEngine;

public class UIShaker : MonoBehaviour
{
    [SerializeField]
    private float shakeDuration = 0.5f;
    [SerializeField]
    private float baseShakeMagnitude = 5f; // �⺻ ��鸲 ����
    private float currentShakeMagnitude;

    private RectTransform rectTransform;
    private Vector2 initialPosition;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        currentShakeMagnitude = baseShakeMagnitude; // �ʱ� ���� ����
    }

    public void StartShake(float healthPercentage)
    {
        currentShakeMagnitude = baseShakeMagnitude + (1 - healthPercentage) * 8;
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * currentShakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * currentShakeMagnitude;

            rectTransform.anchoredPosition = initialPosition + new Vector2(offsetX, offsetY);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = initialPosition;
    }
}
