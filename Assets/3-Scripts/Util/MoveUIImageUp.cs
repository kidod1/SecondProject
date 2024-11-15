using UnityEngine;
using UnityEngine.Events; // UnityEvent�� ����ϱ� ���� �߰�

[RequireComponent(typeof(RectTransform))]
public class MoveUIImageUp : MonoBehaviour
{
    [Header("�̵� ����")]
    [Tooltip("�̹����� �ʴ� �̵��� Y�� �Ÿ�")]
    [SerializeField]
    private float speed = 50f;

    [Tooltip("�̹����� ������ �̵����� ����")]
    [SerializeField]
    private bool moveIndefinitely = true;

    [Tooltip("�̹����� ���� �ִ� Y ��ġ (���� �̵��� �ƴ� ��� �ʿ�)")]
    [SerializeField]
    private float maxY = 1000f;

    [Header("�߰� ����")]
    [Tooltip("�̵��� ������ ���� �ʱ� Y ��ġ")]
    [SerializeField]
    private float startY = 0f;

    [Header("�̺�Ʈ ����")]
    [Tooltip("�̹����� ��ǥ ������ �������� �� ȣ��Ǵ� �̺�Ʈ")]
    [SerializeField]
    private UnityEvent OnReachedMaxY;

    private RectTransform rectTransform;
    private bool hasReachedMaxY = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("MoveUIImageUp ��ũ��Ʈ�� RectTransform ������Ʈ�� �ʿ�� �մϴ�.");
            enabled = false;
            return;
        }

        // �ʱ� Y ��ġ ����
        Vector2 anchoredPos = rectTransform.anchoredPosition;
        anchoredPos.y = startY;
        rectTransform.anchoredPosition = anchoredPos;
    }

    private void Update()
    {
        if (hasReachedMaxY) return;

        // Y������ �̵�
        float deltaY = speed * Time.deltaTime;
        rectTransform.anchoredPosition += new Vector2(0, deltaY);

        // �̵� ���� ����
        if (!moveIndefinitely && rectTransform.anchoredPosition.y >= maxY)
        {
            hasReachedMaxY = true;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, maxY);
            InvokeReachedMaxYEvent();
        }
    }

    /// <summary>
    /// �̵� �ӵ��� �����մϴ�.
    /// </summary>
    /// <param name="newSpeed">���ο� �ӵ� ��</param>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    /// <summary>
    /// �̵��� �ٽ� �����մϴ�.
    /// </summary>
    public void RestartMovement()
    {
        hasReachedMaxY = false;
        Vector2 anchoredPos = rectTransform.anchoredPosition;
        anchoredPos.y = startY;
        rectTransform.anchoredPosition = anchoredPos;
    }

    /// <summary>
    /// ��ǥ ������ �������� �� �̺�Ʈ�� ȣ���մϴ�.
    /// </summary>
    private void InvokeReachedMaxYEvent()
    {
        if (OnReachedMaxY != null)
        {
            OnReachedMaxY.Invoke();
            Debug.Log("�̹����� ��ǥ ������ �����߽��ϴ�. OnReachedMaxY �̺�Ʈ�� ȣ��Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("OnReachedMaxY �̺�Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }
}
