using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TestPortal : MonoBehaviour
{
    [SerializeField]
    private string nextMap;
    [SerializeField]
    private SceneChangeSkeleton skeleton;
    [SerializeField]
    private UnityEvent onPortalEnter; // 유니티 이벤트 추가

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 유니티 이벤트 실행
            onPortalEnter?.Invoke();

            // 애니메이션 실행
            skeleton.PlayCloseAnimation(nextMap);

            // 플레이어 데이터 저장
            PlayerDataManager.Instance.SavePlayerData();
        }
    }
}
