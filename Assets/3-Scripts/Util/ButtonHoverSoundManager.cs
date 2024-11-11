using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using AK.Wwise; // Wwise 네임스페이스 추가

public class ButtonHoverSoundManager : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event hoverSoundEvent; // 버튼 호버링 사운드 이벤트

    private GameObject lastHoveredButton = null;

    private void Update()
    {
        // 마우스 포지션에서 UI 오브젝트를 감지하기 위한 작업
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        // 레이캐스트 결과를 저장할 리스트
        List<RaycastResult> results = new List<RaycastResult>();

        // 현재 레이캐스트 실행
        EventSystem.current.RaycastAll(pointerData, results);

        GameObject currentHoveredButton = null;

        // 결과 중에서 Button 컴포넌트를 가진 오브젝트를 찾음
        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null && button.interactable)
            {
                currentHoveredButton = result.gameObject;
                break;
            }
        }

        // 새로운 버튼에 호버링했다면 사운드 재생
        if (currentHoveredButton != null && currentHoveredButton != lastHoveredButton)
        {
            if (hoverSoundEvent != null)
            {
                hoverSoundEvent.Post(currentHoveredButton);
            }
            else
            {
                Debug.LogWarning("Hover Sound Event가 할당되지 않았습니다.");
            }
        }

        lastHoveredButton = currentHoveredButton;
    }
}
