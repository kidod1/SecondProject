using UnityEngine;

public class ExperienceMagnetItem : MonoBehaviour
{
    [SerializeField]
    private ExperienceMagnetItemData itemData;

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

            // �������� �ı��Ͽ� �� ���� �۵��ϵ��� �մϴ�.
            Destroy(gameObject);
        }
    }
}
