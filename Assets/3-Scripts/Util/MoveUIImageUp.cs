using UnityEngine;
using UnityEngine.Events; // UnityEvent�� ����ϱ� ���� �߰�
using UnityEngine.SceneManagement; // �� ��ȯ�� ���� �߰�
using System.Collections;

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

    [Header("�� ��ȯ ����")]
    [Tooltip("��ǥ ������ ������ �� ��ȯ�� ���� ���� �̸�")]
    [SerializeField]
    private string nextSceneName = "NextScene"; // Inspector���� ���� ����

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
        Debug.Log($"MoveUIImageUp Awake: �ʱ� Y ��ġ ������. startY = {startY}");
    }

    private void Update()
    {
        if (hasReachedMaxY) return;

        // Y������ �̵�
        float deltaY = speed * Time.deltaTime;
        rectTransform.anchoredPosition += new Vector2(0, deltaY);
        Debug.Log($"MoveUIImageUp Update: ���� Y ��ġ = {rectTransform.anchoredPosition.y}");

        // �̵� ���� ����
        if (!moveIndefinitely && rectTransform.anchoredPosition.y >= maxY)
        {
            hasReachedMaxY = true;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, maxY);
            Debug.Log($"MoveUIImageUp Update: ��ǥ Y ��ġ ����. maxY = {maxY}");
            InvokeReachedMaxYEvent();
        }
    }

    /// <summary>
    /// ��ǥ ������ �������� �� �̺�Ʈ�� ȣ���մϴ�.
    /// </summary>
    private void InvokeReachedMaxYEvent()
    {
        if (OnReachedMaxY != null)
        {
            OnReachedMaxY.Invoke();
            Debug.Log("MoveUIImageUp: OnReachedMaxY �̺�Ʈ�� ȣ��Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("MoveUIImageUp: OnReachedMaxY �̺�Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // 10�� �Ŀ� ���� ������ ��ȯ�ϴ� �ڷ�ƾ ����
        StartCoroutine(TransitionToNextScene());
    }

    /// <summary>
    /// 10�� ��� �� ���� ������ ��ȯ�մϴ�.
    /// </summary>
    private IEnumerator TransitionToNextScene()
    {
        Debug.Log("MoveUIImageUp: 10�� �Ŀ� ���� ������ ��ȯ�˴ϴ�.");
        yield return new WaitForSeconds(10f);
        Debug.Log($"MoveUIImageUp: �� '{nextSceneName}'���� ��ȯ�մϴ�.");

        // �� ��ȯ �� �ش� ���� ���� ������ ���ԵǾ� �ִ��� Ȯ���ϼ���.
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("MoveUIImageUp: ���� ���� �̸��� �������� �ʾҽ��ϴ�.");
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// �̵� �ӵ��� �����մϴ�.
    /// </summary>
    /// <param name="newSpeed">���ο� �ӵ� ��</param>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        Debug.Log($"MoveUIImageUp: �̵� �ӵ��� {newSpeed}���� �����Ǿ����ϴ�.");
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
        Debug.Log("MoveUIImageUp: �̵��� ����۵Ǿ����ϴ�.");
    }

    private void OnDestroy()
    {
        Debug.Log("MoveUIImageUp: ������Ʈ�� �ı��Ǿ����ϴ�.");
    }
}
