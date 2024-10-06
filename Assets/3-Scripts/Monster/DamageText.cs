using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float lifetime = 1f; // �ؽ�Ʈ�� ������� �ð�
    public Vector3 floatSpeed = new Vector3(0, 50f, 0); // �ؽ�Ʈ�� ���� �������� �ӵ� (�ȼ� ����)
    public TextMeshProUGUI damageText;
    public Color textColor = Color.red;

    private float timer = 0f;
    private RectTransform rectTransform;

    void Start()
    {
        // damageText�� �ν����Ϳ��� �Ҵ���� �ʾҴٸ� ã�ƺ���
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
            Debug.LogError("DamageText ��ũ��Ʈ���� TextMeshProUGUI ������Ʈ�� ã�� �� �����ϴ�.");
        }

        rectTransform = GetComponent<RectTransform>();

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // ���� �������� ȿ�� (��ũ�� ��ǥ���� Y�� �̵�)
        rectTransform.position += floatSpeed * Time.deltaTime;
        timer += Time.deltaTime;

        // �ؽ�Ʈ�� ���İ��� ���� �ٿ��� ���̵� �ƿ� ȿ��
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
            Debug.LogError("damageText�� null�Դϴ�. SetText���� �ؽ�Ʈ�� ������ �� �����ϴ�.");
        }
    }
}
