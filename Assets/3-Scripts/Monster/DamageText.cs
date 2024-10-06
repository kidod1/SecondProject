using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float lifetime = 1f; // 텍스트가 사라지는 시간
    public Vector3 floatSpeed = new Vector3(0, 50f, 0); // 텍스트가 위로 떠오르는 속도 (픽셀 단위)
    public TextMeshProUGUI damageText;
    public Color textColor = Color.red;

    private float timer = 0f;
    private RectTransform rectTransform;

    void Start()
    {
        // damageText가 인스펙터에서 할당되지 않았다면 찾아보기
        if (damageText == null)
        {
            damageText = GetComponent<TextMeshProUGUI>();
            if (damageText == null)
            {
                damageText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        if (damageText == null)
        {
            Debug.LogError("DamageText 스크립트에서 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다.");
        }

        rectTransform = GetComponent<RectTransform>();

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 위로 떠오르는 효과 (스크린 좌표에서 Y축 이동)
        rectTransform.position += floatSpeed * Time.deltaTime;
        timer += Time.deltaTime;

        // 텍스트의 알파값을 점점 줄여서 페이드 아웃 효과
        if (damageText != null)
        {
            Color color = damageText.color;
            color.a = Mathf.Lerp(1f, 0f, timer / lifetime);
            damageText.color = color;
        }
    }

    public void SetText(string text, TMP_FontAsset font, int fontSize, Color color)
    {
        if (damageText != null)
        {
            damageText.text = text;
            if (font != null)
            {
                damageText.font = font;
            }
            damageText.fontSize = fontSize;
            damageText.color = color;
        }
        else
        {
            Debug.LogError("damageText가 null입니다. SetText에서 텍스트를 설정할 수 없습니다.");
        }
    }
}
