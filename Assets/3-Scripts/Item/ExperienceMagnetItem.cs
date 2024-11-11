using UnityEngine;
using AK.Wwise; // Wwise ���ӽ����̽� �߰�

public class ExperienceMagnetItem : MonoBehaviour
{
    [SerializeField]
    private ExperienceMagnetItemData itemData;

    // �߰��� �κ�: Wwise �̺�Ʈ ����
    [SerializeField]
    private AK.Wwise.Event pickupSoundEvent; // Wwise �̺�Ʈ �Ҵ��� ���� �߰�

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // �÷��̾��� Transform�� ����
            Transform playerTransform = other.transform;

            // �� ���� ��� ����ġ �������� ã���ϴ�.
            ExperienceItem[] experienceItems = FindObjectsOfType<ExperienceItem>();

            foreach (ExperienceItem expItem in experienceItems)
            {
                // �� ����ġ �����ۿ� �÷��̾�� ���������� ���
                expItem.StartAttractingToPlayer(playerTransform, itemData.speed, itemData.duration);
            }

            // Wwise �̺�Ʈ ȣ��: ������ �Ⱦ� �� ���� ���
            if (pickupSoundEvent != null)
            {
                // ���� ������Ʈ�� ��ġ���� ���带 ���
                pickupSoundEvent.Post(gameObject);
            }
            else
            {
                Debug.LogWarning("Pickup Sound Event�� �Ҵ���� �ʾҽ��ϴ�.");
            }

            // �������� �ı��Ͽ� �� ���� �۵��ϵ��� �մϴ�.
            Destroy(gameObject);
        }
    }
}
