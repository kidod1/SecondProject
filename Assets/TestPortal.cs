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
    private UnityEvent onPortalEnter; // ����Ƽ �̺�Ʈ �߰�

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // ����Ƽ �̺�Ʈ ����
            onPortalEnter?.Invoke();

            // �ִϸ��̼� ����
            skeleton.PlayCloseAnimation(nextMap);

            // �÷��̾� ������ ����
            PlayerDataManager.Instance.SavePlayerData();
        }
    }
}
