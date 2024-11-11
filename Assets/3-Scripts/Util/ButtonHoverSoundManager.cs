using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using AK.Wwise; // Wwise ���ӽ����̽� �߰�

public class ButtonHoverSoundManager : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event hoverSoundEvent; // ��ư ȣ���� ���� �̺�Ʈ

    private GameObject lastHoveredButton = null;

    private void Update()
    {
        // ���콺 �����ǿ��� UI ������Ʈ�� �����ϱ� ���� �۾�
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        // ����ĳ��Ʈ ����� ������ ����Ʈ
        List<RaycastResult> results = new List<RaycastResult>();

        // ���� ����ĳ��Ʈ ����
        EventSystem.current.RaycastAll(pointerData, results);

        GameObject currentHoveredButton = null;

        // ��� �߿��� Button ������Ʈ�� ���� ������Ʈ�� ã��
        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null && button.interactable)
            {
                currentHoveredButton = result.gameObject;
                break;
            }
        }

        // ���ο� ��ư�� ȣ�����ߴٸ� ���� ���
        if (currentHoveredButton != null && currentHoveredButton != lastHoveredButton)
        {
            if (hoverSoundEvent != null)
            {
                hoverSoundEvent.Post(currentHoveredButton);
            }
            else
            {
                Debug.LogWarning("Hover Sound Event�� �Ҵ���� �ʾҽ��ϴ�.");
            }
        }

        lastHoveredButton = currentHoveredButton;
    }
}
